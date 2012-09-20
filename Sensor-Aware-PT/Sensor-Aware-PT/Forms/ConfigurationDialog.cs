using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Management;
using System.Threading;

namespace Sensor_Aware_PT
{


    public partial class ConfigurationDialog : Form
    {
        #region Constants
        private const String CFG_DIR = "Sensor-Aware-PT";
        private static String AppDataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private String ConfigFilePath = Path.Combine(new String[] { AppDataDirPath, CFG_DIR, "config.xml" });
        private const int MAC_ADDRESS_LENGTH = 12;
        #endregion

        #region InstanceVariables
        BackgroundWorker mRescanWorker = null;
        Nexus mNexus = Nexus.Instance;
        Boolean mbScanForPort = false;

        private List<Char> Labels;

        private Dictionary<String, String> mPortToMac;
        private Dictionary<String, ComboBox> mPortToComboBox;
        private List<SplitContainer> mContainers;
        private ManagementObject[] mWmiCOMData = null;

        private Dictionary<String, Sensor> mSensorDict;
        private Dictionary<String, SensorIdentification> mSensorIDDict;
        #endregion

   
        public ConfigurationDialog()
        {
            InitializeComponent();

            this.mSensorDict = mNexus.mSensorDict;
            this.mSensorIDDict = mNexus.mSensorIDDict;
            
            this.mFakeProgressBar.MarqueeAnimationSpeed = 500;
            this.mFakeProgressBar.Show();
            this.mScanLabel.Show();

            
            this.mRescanWorker = new BackgroundWorker();
            this.mRescanWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.Configure);
    
            this.mRescanWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.GenerateForm);
            this.mRescanWorker.RunWorkerAsync();

        }

        private void Configure(object sender, DoWorkEventArgs e)
        {
            if (File.Exists(this.ConfigFilePath) == false)
            {
                MessageBox.Show("Could not find the configuration file.\nRescan will be necessary");
                this.mbScanForPort = true;
            }
            else
            {
                if (this.readConfigFile() == true)
                {
                    this.mbScanForPort = false;
                }
            }

            if (this.mbScanForPort)
            {
                this.ScanForPorts();
            }
        }

        void GenerateForm(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this.mScanLabel.Hide();
            this.mFakeProgressBar.Hide();

            this.mPortToMac = new Dictionary<String,String>();

            if (this.mbScanForPort)
            {
                Labels = new List<Char>();
                for (int i = 0; i < mWmiCOMData.Length; ++i)
                {
                    Labels.Add(Convert.ToChar(65 + i));
                }

                foreach (ManagementObject obj in mWmiCOMData)
                {
                    String pnpDeviceID = obj["PNPDeviceID"] as String;
                    String deviceID = obj["DeviceID"] as String; ;

                    /** Parse the pnpDeviceID to get the MAC address */
                    int index = pnpDeviceID.LastIndexOf('_');
                    String MacAddress = pnpDeviceID.Substring(index - MAC_ADDRESS_LENGTH, MAC_ADDRESS_LENGTH);

                    /** There seems to be a second port generated along with each Bluetooth serial port. 
                        * we ignore this one **/
                    if ("000000000000" != MacAddress)
                    {
                        TabPage newpage = new TabPage(deviceID);

                        SplitContainer horizontal = new SplitContainer();
                        
                        ComboBox newcombo = new ComboBox();
                        
                        horizontal.Orientation = Orientation.Horizontal;

                        newcombo.Items.Add(Labels); 
                        horizontal.Panel1.Controls.Add(newcombo);
                        newpage.Controls.Add(horizontal);
                        mTabControl.TabPages.Add(newpage);
         
                    }
                }
            }
            else
            {
                //foreach (SensorIdentification id in mSensorIDDict.Values)
                //{
                //    mListBox.Items.Add(id.PortName);
                //}
            }
          


           
        }

        /// <summary>
        /// Reads the config file at %APPDATA%/Sensor-Aware-PT/config.xml
        /// </summary>
        private bool readConfigFile()
        {
            bool retval = true;
            StreamReader fileStream = null;
            try
            {
                // Get an array of sensor IDs from the xml file
                fileStream = new StreamReader(ConfigFilePath);

                SensorIdentification[] tmpArray = new SensorIdentification[1] { null };
                XmlSerializer serializer = new XmlSerializer(tmpArray.GetType());

                // Take each array object and insert it into the id dictionary
                tmpArray = serializer.Deserialize(fileStream) as SensorIdentification[];
                foreach (SensorIdentification id in tmpArray)
                {
                    mSensorIDDict[id.Id] = id;
                }
            }
            catch (System.InvalidOperationException e)
            {
                if (e.Message.StartsWith("There is an error in XML document"))
                {
                    Logger.Warning(String.Format("{0}- {1}", e.GetType().ToString(), e.Message));
                    retval = false;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                fileStream.Close();
            }
            return retval;
        }

        /// <summary>
        /// Dumps the contents of the mSensorIDDict into a .xml configuration file
        /// </summary>
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

            fileStream.Flush();
            fileStream.Close();
        }


        /// <summary>
        /// Scans WMI for new Bluetooth COM ports and then delays 60 seconds
        /// to give the sensors time to initialize.
        /// </summary>
        private void ScanForPorts()
        {

            System.DateTime mStartOfInit = System.DateTime.Now;

            Logger.Info("Wmi scan started");

            mWmiCOMData = getWmiCOMData();

            System.TimeSpan timeSpent = System.DateTime.Now - mStartOfInit;
            double timeLeft = 60.0 - (int)timeSpent.TotalSeconds;
            if (timeLeft > 0)
            {
                Logger.Info("Delaying for {0} seconds while sensors initialize...", (int)timeLeft);
                Thread.Sleep((int)(timeLeft * 1000));
            }
            Logger.Info("Wmi scan ended");
        }

        /// <summary>
        /// Queries the system for bluetooth COM ports and enumerates the sensors using the provided data.
        /// </summary>
        //internal void probeWmiComPorts()
        //{


        //    foreach (ManagementObject obj in wmiCOMData)
        //    {
        //        String pnpDeviceID = obj["PNPDeviceID"] as String;
        //        String deviceID = obj["DeviceID"] as String;
        //        String sensorIDstring;

        //        /** Parse the pnpDeviceID to get the MAC address */
        //        int index = pnpDeviceID.LastIndexOf('_');
        //        String MacAddress = pnpDeviceID.Substring(index - MAC_ADDRESS_LENGTH, MAC_ADDRESS_LENGTH);

        //        /** There seems to be a second port generated along with each Bluetooth serial port. 
        //         * we ignore this one **/
        //        if ("000000000000" != MacAddress)
        //        {
        //            //if (mConfigFileDataDict == null)
        //            //{
        //            //    mConfigFileDataDict = new SerializableDictionary<string, string>();
        //            //}

        //            Console.WriteLine();
        //            Console.WriteLine("Please enter the sensor id for MAC address {0} [ex: A,B,C,ARM,...etc]", MacAddress);
        //            Console.Write(">");
        //            sensorIDstring = Console.ReadLine();
        //            //mConfigFileDataDict[deviceID] = sensorIDstring;

        //            SensorIdentification sensorID = new SensorIdentification(sensorIDstring, MacAddress, deviceID);

        //            /** Create the Sensor with its SensorID **/
        //            mSensorDict[sensorIDstring] = new Sensor(sensorID);
        //            mSensorIDDict[sensorIDstring] = sensorID;
        //        }
        //    }
        //}

        
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
