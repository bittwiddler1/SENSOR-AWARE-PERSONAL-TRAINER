using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Management;
using System.Xml.Serialization;

using OpenTK;

namespace Sensor_Aware_PT
{
    /** Holds ID and MAC of each sensor */
    public class SensorIdentification
    {
        public string Id;
        public string Mac;
        public string PortName;

        public SensorIdentification()
        {
            Id = Mac = PortName = String.Empty;
        }

        public SensorIdentification(string _id, string _mac, string port = "")
        {
            Id = _id;
            Mac = _mac;
            PortName = port;
        }
    }

    class Nexus
    {
        #region Constants
        /** baud rate for the com ports */
        public const int SENSOR_BAUD_RATE = 57600;
        public const double SERIAL_ENUMERATION_TIMEOUT_SECONDS = 5;
        private const int MAC_ADDRESS_LENGTH = 12;
        private const String CFG_DIR = "Sensor-Aware-PT";

        /** Path to config file **/
        static String AppDataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        String ConfigFilePath = Path.Combine(new String[] { AppDataDirPath, CFG_DIR, "config.xml" });
        #endregion

        #region instance variables
        /** List to hold the Sensor objects */
        Dictionary<String, Sensor> mSensorDict;
        Dictionary<String, SensorIdentification> mSensorIDDict;
        SerializableDictionary<String, String> mConfigFileDataDict;
        Sensor[] mAvailableSensors;

        private Boolean bGenerateConfig = true;
        /** This keeps track of the # of ready events we hve received */
        private static int mReadySensorCount = 0;
        System.DateTime mStartOfInit;

        #endregion
        #region Event handling stuff

        /** Delegate & event for when all sensors are initialized and reading */
        public delegate void InitializationCompleteHandler( object sender, EventArgs e );
        public event InitializationCompleteHandler InitializationComplete;
 
        #endregion

        /// <summary>
        /// The default constructor. Duh. :P
        /// </summary>
        public Nexus()
        {
            initializeVariables();
        }

        public void initialize()
        {
            this.Configure();
            
        }

        private void Configure()
        {
            try
            {
                if (File.Exists(this.ConfigFilePath))
                {
                    Logger.Info("Config file detected");
                    Console.WriteLine("Use the detected config file? If not, you will be prompted for sensor data and a config will be created for you [Y/N]");
                    String response = Console.ReadLine()[0].ToString();

                    if (response.ToLower() != "y")
                    {
                        this.readConfigFile();
                        this.bGenerateConfig = false;
                    }
                }
                if (this.bGenerateConfig)
                {

                    this.probeWmiComPorts();
                    this.saveConfig();

                    System.TimeSpan timeleft = System.DateTime.Now - mStartOfInit;
                    Logger.Info("Delaying for {0} seconds while sensors initialize...", timeleft.TotalSeconds);
                    Thread.Sleep((int)timeleft.TotalMilliseconds);
                }

                
                mAvailableSensors = mSensorDict.Values.ToArray();
                foreach (Sensor s in mAvailableSensors)
                {
                    /** Register the ready event */
                    s.InitializationComplete += new Sensor.InitializationCompleteHandler(Sensor_InitializationCompleteEvent);
                }

                /** Initialize the first member */
                mAvailableSensors[0].initialize();
            }
            catch( Exception e )
            {
                Logger.Warning( "{0}", e.Message );
                bGenerateConfig = true;
                Logger.Info( "Manual input will be required to configure the sensor array" );
            }
        }

        private void saveConfig()
        {
            StreamWriter fileStream;

            /** Generate the config file **/
            try
            {
                fileStream = new StreamWriter(ConfigFilePath, false, Encoding.ASCII);
            }
            catch (DirectoryNotFoundException e)
            {
                Directory.CreateDirectory(Path.Combine(AppDataDirPath, CFG_DIR));
                fileStream = new StreamWriter(ConfigFilePath, false, Encoding.ASCII);

            }
            XmlSerializer serializer = new XmlSerializer(mConfigFileDataDict.GetType());
            serializer.Serialize(fileStream, mConfigFileDataDict);
            fileStream.Flush();
            fileStream.Close();
        }

        #region Initialization and enumeration
        /// <summary>
        ///  Initialize all the member variables
        /// </summary>
        private void initializeVariables()
        {
    
            mSensorDict   = new Dictionary<String,Sensor>();                   // The actual sensor objects
            mSensorIDDict = new Dictionary<String, SensorIdentification>();
            mConfigFileDataDict = new SerializableDictionary<String, String>();
        }
        
        /// <summary>
        /// Queries the system for bluetooth COM ports and enumerates the sensors using the provided data.
        /// </summary>
        private void probeWmiComPorts()
        {
            Logger.Info( "Serial port enumeration started" );
   
            ManagementObject[] wmiCOMData = getWmiCOMData();

            foreach (ManagementObject obj in wmiCOMData)
            {
                String pnpDeviceID = obj["PNPDeviceID"] as String;
                String deviceID    = obj["DeviceID"]    as String;
                String sensorIDstring;

                /** Parse the pnpDeviceID to get the MAC address */
                int index = pnpDeviceID.LastIndexOf('_');
                String MacAddress = pnpDeviceID.Substring(index - MAC_ADDRESS_LENGTH, MAC_ADDRESS_LENGTH);

                /** There seems to be a second port generated along with each Bluetooth serial port. 
                 * we ignore this one **/
                if ("000000000000" != MacAddress)
                {
                    if (mConfigFileDataDict == null)
                    {
                        mConfigFileDataDict = new SerializableDictionary<string, string>();
                    }

                    Console.WriteLine();
                    Console.WriteLine("Please enter the sensor id for MAC address {0} [ex: A,B,C,ARM,...etc]", MacAddress);
                    Console.Write(">");
                    sensorIDstring = Console.ReadLine();
                    mConfigFileDataDict[deviceID] = sensorIDstring;

                    SensorIdentification sensorID = new SensorIdentification(sensorIDstring, MacAddress, deviceID);

                    /** Create the Sensor with its SensorID **/
                    mSensorDict[sensorIDstring] = new Sensor(sensorID);
                    mSensorIDDict[sensorIDstring] = sensorID;
                }
            }
        }

        /// <summary>
        /// This event gets raised after each sensor completes its initialize routine. Even if a sensor fails to initialize,
        /// it will fire this event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Sensor_InitializationCompleteEvent( object sender, EventArgs e )
        {
            Logger.Info( "Nexus has recieved ready notification from sensor {0}", ( ( Sensor ) sender ).Id );
            mReadySensorCount++;
            /** Check to see if all sensors have been initialized, if so then reset the count */
            if( mReadySensorCount == mSensorDict.Count )
            {
                Logger.Info( "Nexus has intialized all sensors and is preparing to begin reading" );
                mReadySensorCount = 0;
                /** Now take our initialized sensors, and start their respective read threads */
                foreach( Sensor s in mAvailableSensors )
                {
                    if( s.IsInitialized )
                    {
                        mReadySensorCount++;
                        s.ActivationComplete += new Sensor.ActivationCompleteHandler( Sensor_ActivationCompleteEvent );
                        s.activate();
                    }
                }
                
            }
            else
            {
                {
                    Logger.Info( "Initializing sensor list index {0}", mReadySensorCount );
                    mAvailableSensors[ mReadySensorCount ].initialize();
                }
            }


        }

        /// <summary>
        /// This event is fired after each sensor begins its read thread and synchronizes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Sensor_ActivationCompleteEvent( object sender, EventArgs e )
        {
            mReadySensorCount--;
            if( mReadySensorCount == 0 )
            {
                Logger.Info( "Nexus has synchronized and started reading for all initialized sensors and is activated complete" );
                OnNexusInitializationComplete();
            }
        }


        private void OnNexusInitializationComplete()
        {
            InitializationCompleteHandler handler = InitializationComplete;
            try
            {
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Reads the config file at %APPDATA%/Sensor-Aware-PT/config.xml
        /// </summary>
        private void readConfigFile()
        {
            StreamReader fileStream = null;
            try
            {
                fileStream = new StreamReader(ConfigFilePath);
                XmlSerializer serializer = new XmlSerializer(mConfigFileDataDict.GetType());

                mConfigFileDataDict = serializer.Deserialize(fileStream) as SerializableDictionary<String, String>;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                fileStream.Close();
            }
        }

        /// <summary>
        /// Queries WMI for Bluetooth COM Ports. Provides useful information on the Console as it does.
        /// </summary>
        /// <returns></returns>
        private ManagementObject[] getWmiCOMData()
        {
            ManagementObject[] wmiObjects = null;
            ManagementObjectSearcher wmiSearcher = null;
            try
            {
                wmiSearcher = new ManagementObjectSearcher(@"select DeviceID,PNPDeviceID from Win32_SerialPort");
              
                ManagementObjectCollection tmpSearchResults = null;

                Logger.Info("Searching for Bluetooth COM devices");
                wmiSearcher = new ManagementObjectSearcher(@"root\cimv2", "SELECT Name,DeviceID,PNPDeviceID FROM Win32_SerialPort WHERE Name LIKE \"%Bluetooth%\"");
                tmpSearchResults = wmiSearcher.Get();

                Logger.Info("Found {0} devices, verifying...", tmpSearchResults.Count);
                wmiObjects = new ManagementObject[tmpSearchResults.Count];
                tmpSearchResults.CopyTo(wmiObjects, 0);

            }
            catch (System.Management.ManagementException e)
            {
                Console.WriteLine("Query " + wmiSearcher.Query.ToString() + " threw an exception!\n");
                throw e;
            }
            try
            {
                int i = 0;
                foreach (ManagementObject obj in wmiObjects)
                {
                    if ((obj["Name"] as String).Contains("Bluetooth") == false)
                    {
                        Logger.Warning("{0} is not a Bluetooth device! Removing from search results", obj["DeviceID"] as String);
                    }
                    else
                    {
                        wmiObjects[i] = obj;
                        ++i;
                    }
                }
            }
            catch (System.Management.ManagementException e)
            {
                Console.WriteLine("Exception: Unable to read a property from wmi object!\n");
                throw e;
            }

            Logger.Info("Verified {0} Bluetooth COM Device(s)", wmiObjects.Length);
            return wmiObjects;
        }
        #endregion

        /// <summary>
        /// Gets the sensors which are available/reading already
        /// </summary>
        /// <returns>List of active sensors</returns>
        public List<Sensor> getActivatedSensors()
        {
            List<Sensor> activeSensors = new List<Sensor>();
            foreach(Sensor s in mSensorDict.Values)
            {
                if( s.IsActivated )
                    activeSensors.Add( s );
            }
            return activeSensors;
        }
    }
}
