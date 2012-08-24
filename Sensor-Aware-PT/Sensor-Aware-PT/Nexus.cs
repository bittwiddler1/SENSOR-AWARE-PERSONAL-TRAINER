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
        /** baud rate for the com ports */
        public const int SENSOR_BAUD_RATE = 9600; /* derp 57600; */
        public const double SERIAL_ENUMERATION_TIMEOUT_SECONDS = 5;
        private const int MAC_ADDRESS_LENGTH = 12;
        private const String CFG_DIR = "Sensor-Aware-PT";

        /** Path to config file **/
        static String AppDataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static String ConfigFilePath = Path.Combine(new String[] { AppDataDirPath, CFG_DIR, "config.xml" });

        #region instance variables
        /** Holds the id data for each sensor. Eventually user can use a configurator to set this up
         * though for now it's hardcoded to our sensors */
        //List<SensorIdentification> mSensorIdList;

        /** List to hold the Sensor objects */
        Dictionary<String, Sensor> mSensorDict;
        Dictionary<String, SensorIdentification> mSensorIDDict;
        SerializableDictionary<String, String> mConfigFileDataDict;


        private Boolean bGenerateConfig = false;
        #endregion

        /// <summary>
        /// The default constructor. Duh. :P
        /// </summary>
        public Nexus()
        {
            initializeVariables();

            try
            {
                initializeConfigFileDataDict();
            }
            catch (Exception e)
            {
                Logger.Warning("{0}", e.Message);
                bGenerateConfig = true;
                Logger.Info("Manual input will be required to configure the sensor array");
            }

            enumerateSensors();
        }

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
        private void enumerateSensors()
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
                    if (false == bGenerateConfig)
                    {
                        sensorIDstring = mConfigFileDataDict[MacAddress] as String;
                        Logger.Info("Matching MAC {0} to sensor ID \"{1}\"", MacAddress, sensorIDstring);
                    }
                    // Temporarily: prompt for the sensor ID
                    else 
                    {
                        if (mConfigFileDataDict == null)
                        {
                            mConfigFileDataDict = new SerializableDictionary<string, string>();
                        }

                        Console.WriteLine();
                        Console.WriteLine("Please enter the sensor id for MAC address {0} [ex: A,B,C,ARM,...etc]", MacAddress);
                        Console.Write(">");
                        sensorIDstring = Console.ReadLine();
                        mConfigFileDataDict[MacAddress] = sensorIDstring;
                    }

                    SensorIdentification sensorID = new SensorIdentification(sensorIDstring, MacAddress, deviceID);

                    /** Create the Sensor with its SensorID **/
                    mSensorDict[sensorIDstring] = new Sensor(sensorID);
                    mSensorIDDict[sensorIDstring] = sensorID;
                }
            }
            if (true == bGenerateConfig)
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

            foreach( Sensor s in mSensorDict.Values )
            {
                s.initialize();
            }
        }

        /// <summary>
        /// Reads the config file at %APPDATA%/Sensor-Aware-PT/config.xml
        /// </summary>
        private void initializeConfigFileDataDict()
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



    }
}
