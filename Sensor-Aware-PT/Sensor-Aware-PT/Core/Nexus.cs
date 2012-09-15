using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Management;
using System.Xml.Serialization;


namespace Sensor_Aware_PT
{
    public class Nexus : IObservable<SensorDataEntry>
    {
        #region singleton implementation variables
        private static volatile Nexus mInstance;
        private static object mInstanceLock = new object();
        #endregion
        #region Constants
        /** baud rate for the com ports */
        public const int SENSOR_BAUD_RATE = 57600;
        public const double SERIAL_ENUMERATION_TIMEOUT_SECONDS = 5;
        private const int MAC_ADDRESS_LENGTH = 12;
        private const String CFG_DIR = "Sensor-Aware-PT";

        /** Path to config file **/
        private static String AppDataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private String ConfigFilePath = Path.Combine(new String[] { AppDataDirPath, CFG_DIR, "config.xml" });
        #endregion

        #region instance variables
        /** List to hold the Sensor objects */
        private Dictionary<String, Sensor> mSensorDict;
        private Dictionary<String, SensorIdentification> mSensorIDDict;
        
        private Sensor[] mAvailableSensors;
        private Boolean bGenerateConfig = true;

        /** This keeps track of the # of ready events we hve received */
        private static int mReadySensorCount = 0;
        private System.DateTime mStartOfInit;

        /** Observable pattern & lock */
        private List<IObserver<SensorDataEntry>> mObservers = new List<IObserver<SensorDataEntry>>();
        private readonly object mObserverLock = new object();

        /** Counter for data frames */
        private int mCurrentFrameNumber = 0;

        /** Keeps the # of active sensors */

        private int mActiveSensorCount = 0;
        private static object mFrameLock = new object();

        #endregion

        #region Event handling stuff

        /** Delegate & event for when all sensors are initialized and reading */
        public delegate void InitializationCompleteHandler( object sender, EventArgs e );
        public event InitializationCompleteHandler InitializationComplete;
 
        #endregion

        /// <summary>
        /// Default constructor is private since this is a singleton. Use Nexus.Instance instead
        /// </summary>
        private Nexus()
        {
            initializeVariables();
        }

        public static Nexus Instance
        {
            get
            {
                /** Double check locking because the .net guide said to use it */
                if( mInstance == null )
                {
                    lock( mInstanceLock )
                    {
                        if( mInstance == null )
                            mInstance = new Nexus();
                    }
                }
                return mInstance;
            }
        }

        public void initialize()
        {
            this.Configure();
            
        }

        #region ObserverPattern
        public IDisposable Subscribe( IObserver<SensorDataEntry> observer )
        {
            lock (mObserverLock)
            {
                if (mObservers.Contains(observer) == false)
                {
                    mObservers.Add(observer);
                }

                return new Unsubscriber(mObservers, observer);
            }
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<SensorDataEntry>> mList;
            private IObserver<SensorDataEntry> mObserver;

            public Unsubscriber( List<IObserver<SensorDataEntry>> list, IObserver<SensorDataEntry> observer )
            {
                this.mList     = list;
                this.mObserver = observer;
            }

            public void Dispose()
            {
                if (this.mObserver != null && this.mList.Contains(mObserver))
                {
                    this.mList.Remove(mObserver);
                }
            }
        }

        public void NotifyObservers( SensorDataEntry dataFrame )
        {
            foreach( IObserver<SensorDataEntry> observer in mObservers )
            {
                observer.OnNext(dataFrame);

            }
        }

        /// <summary>
        /// When a sensor has new data this event gets called.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Sensor_DataReceived( object sender, Sensor.DataReceivedEventArgs e )
        {
            NotifyObservers( e.Data );
        }
        #endregion

        #region Configuration
        private void Configure()
        {
            try
            {
                if (File.Exists(this.ConfigFilePath))
                {
                    String response;
                    Logger.Info("Config file detected at {0}", this.ConfigFilePath);
                    Logger.Info("Prompting user to determine whether to use it...");
                    do
                    {
                        Console.WriteLine("Use the detected config file? [Y/N]");
                        Console.WriteLine("If \"No\", you will be prompted for sensor identification data and a config will be saved for future use.");
                        response = Console.ReadLine()[0].ToString();

                        if (response.ToLower() == "y")
                        {
                            Logger.Info("User chose to use existing config file. (User Input = {0})", response.ToLower()[0]);
                            this.readConfigFile();
                            this.bGenerateConfig = false;
                        }
                        else if (response.ToLower() == "n")
                        {
                            Logger.Info("User chose not to use existing config file. (User Input = {0})", response.ToLower()[0]);
                            this.bGenerateConfig = true;
                        }
                        else
                        {
                            Console.WriteLine("Input not understood. Please try again. Enter something starting with y or n");
                        }
                    } while (response.ToLower() != "y" && response.ToLower() != "n");
                    
                } 
                else
                {
                    this.bGenerateConfig = true;
                }

                if (this.bGenerateConfig)
                {
                    mStartOfInit = System.DateTime.Now;
                    this.probeWmiComPorts();
                    this.saveConfigFile();

                    System.TimeSpan timeSpent = System.DateTime.Now - mStartOfInit;
                    double timeLeft = 60.0 - (int)timeSpent.TotalSeconds;
                    if (timeLeft > 0)
                    {
                        Logger.Info("Delaying for {0} seconds while sensors initialize...", (int)timeLeft);
                        Thread.Sleep((int)(timeLeft * 1000));   
                    }
                }
                else
                {
                    foreach (SensorIdentification idStruct in mSensorIDDict.Values)
                    {
                        /** Create the Sensor with its SensorID **/
                        mSensorDict[idStruct.Id] = new Sensor(idStruct);
                    }
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

        private void saveConfigFile()
        {
            StreamWriter fileStream;

            /** Generate the config file **/
            try
            {
                fileStream = new StreamWriter(ConfigFilePath, false, Encoding.ASCII);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(Path.Combine(AppDataDirPath, CFG_DIR));
                fileStream = new StreamWriter(ConfigFilePath, false, Encoding.ASCII);
            }

            SensorIdentification[] tmpArray = mSensorIDDict.Values.ToArray();
            XmlSerializer serializer = new XmlSerializer(tmpArray.GetType());
            serializer.Serialize(fileStream, tmpArray);

            //XmlSerializer serializer = new XmlSerializer(mConfigFileDataDict.GetType());
            //serializer.Serialize(fileStream, mConfigFileDataDict);
            fileStream.Flush();
            fileStream.Close();
        }

        /// <summary>
        /// Reads the config file at %APPDATA%/Sensor-Aware-PT/config.xml
        /// </summary>
        private void readConfigFile()
        {
            StreamReader fileStream = null;
            try
            {
                // Get an array of sensor IDs from the xml file
                fileStream = new StreamReader( ConfigFilePath );

                SensorIdentification[] tmpArray = new SensorIdentification[ 1 ] { null };
                XmlSerializer serializer = new XmlSerializer( tmpArray.GetType() );

                // Take each array object and insert it into the id dictionary
                tmpArray = serializer.Deserialize( fileStream ) as SensorIdentification[];
                foreach( SensorIdentification id in tmpArray )
                {
                    mSensorIDDict[ id.Id ] = id;
                }
            }
            catch( Exception )
            {
                throw;
            }
            finally
            {
                fileStream.Close();
            }
        }

        #endregion
        
        #region Initialization and enumeration
        /// <summary>
        ///  Initialize all the member variables
        /// </summary>
        private void initializeVariables()
        {
    
            mSensorDict   = new Dictionary<String,Sensor>();                   // The actual sensor objects
            mSensorIDDict = new Dictionary<String, SensorIdentification>();
            //mConfigFileDataDict = new SerializableDictionary<String, String>();
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
                    //if (mConfigFileDataDict == null)
                    //{
                    //    mConfigFileDataDict = new SerializableDictionary<string, string>();
                    //}

                    Console.WriteLine();
                    Console.WriteLine("Please enter the sensor id for MAC address {0} [ex: A,B,C,ARM,...etc]", MacAddress);
                    Console.Write(">");
                    sensorIDstring = Console.ReadLine();
                    //mConfigFileDataDict[deviceID] = sensorIDstring;

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
            if( mReadySensorCount == mAvailableSensors.Count() )
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
                        Thread.Sleep( 2000 );
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
                foreach( Sensor s in getActivatedSensors() )
                {
                    s.DataReceived += new Sensor.DataReceivedHandler( Sensor_DataReceived );
                }
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

        #region Nexus commands
        /// <summary>
        /// Gets the sensors which are available/reading already
        /// </summary>
        /// <returns>List of active sensors</returns>
        public List<Sensor> getActivatedSensors()
        {
            List<Sensor> activeSensors = new List<Sensor>();
            
            foreach(Sensor s in  mAvailableSensors)
            {
                if( s.IsActivated )
                    activeSensors.Add( s );
            }

            /** Update the active sensor count */
            if( mActiveSensorCount != activeSensors.Count )
                mActiveSensorCount = activeSensors.Count;

            return activeSensors;
        }

        /// <summary>
        /// Resynchronizes all activated sensors.
        /// </summary>
        public void resynchronize()
        {
            List<Sensor> activeSensors = getActivatedSensors();

            foreach( Sensor s in activeSensors )
            {
                s.resynchronize();
            }

            mCurrentFrameNumber = 0;
        }

        /// <summary>
        /// Resets the sequence counters of all activated sensors
        /// </summary>
        private void resetSensorSequence()
        {
            foreach( Sensor s in getActivatedSensors() )
            {
                s.resetSequence();
            }

            mCurrentFrameNumber = 0;
        }

        #endregion
    }

}
