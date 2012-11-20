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
        private BackgroundWorker mRescanWorker = null;
        private Nexus mNexus = Nexus.Instance;

        private Boolean mbScanForPort = false;

        private Boolean bSaved = false;

        private List<Char> mPossibleIds;
        private ManagementObject[] mWmiCOMData = null;
        
        #region GenerateWinformShit
        private SplitContainer mSplitter = null;
        private TabControl mTabControl = null;
        private Button mSaveButton = null;
        private Button mRescanButton = null;


        #endregion
        #endregion

        static private ConfigurationDialog mInstance = null;

        #region ConstructorDeconstructor
        private ConfigurationDialog()
        {
            InitializeComponent();

            /* Setup WinForms */
            this.mFakeProgressBar.MarqueeAnimationSpeed = 100;

            this.LaunchWorkerThread();


            this.Focus();
        }

        internal static ConfigurationDialog GetInstance()
        {
            if (mInstance == null)
                mInstance = new ConfigurationDialog();

            return mInstance;
        }

        private void LaunchWorkerThread(Boolean bReadFile = true)
        {
            /* Threading stuff */
            this.mRescanWorker = new BackgroundWorker();
            this.mRescanWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.Configure);

            this.mRescanWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.GenerateForm);
            this.mRescanWorker.RunWorkerAsync(bReadFile);
        }
        #endregion

        #region EventHandlers
        /// <summary>
        /// Worker thread that attempts to read the Config and optionally scans for new 
        /// BT Serial Ports.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Configure(object sender, DoWorkEventArgs e)
        {

            Boolean readfile = (Boolean)e.Argument;

            /* Check for config file */
            if (readfile == false || File.Exists(this.ConfigFilePath) == false)
            {
                if (readfile) 
                    MessageBox.Show("Could not find the configuration file.\nRescan will be necessary");
                this.mbScanForPort = true;
            }
            else
            {
                /* If we sucessfully read the file, no need to rescan */
                if (this.ReadConfigFile() == true)
                {
                    this.mbScanForPort = false;
                }
            }

            /* if any of the above  checks failed, we gotta scan for ports */
            if (this.mbScanForPort)
            {
                this.ScanForPorts();
            }
        }

        /// <summary>
        /// Called as soon as Configure(...) ends.  Gets rid of the "Scanning" page on
        /// the form and replaces it with tabs and buttons and context menus, etc.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GenerateForm(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            /* Get rid of the "Scanning" page */
            this.mPanel.Controls.Remove(this.mScanLabel);
            this.mPanel.Controls.Remove(this.mFakeProgressBar);

            this.mScanLabel.Hide();
            this.mFakeProgressBar.Hide();

            /* Set up the Splitter */
            this.mSplitter = new SplitContainer();
            this.mSplitter.Name = "mSplitter";
            this.mSplitter.Orientation = Orientation.Horizontal;
            this.mSplitter.Dock = DockStyle.Fill;
            this.mSplitter.SplitterDistance = 200;
            this.mSplitter.IsSplitterFixed = true;
            this.mSplitter.Show();

            /* Setup the TabControl */
            this.mTabControl = new TabControl();
            this.mTabControl.Name = "mTabControl";

            /* Setup the Button */
            this.mSaveButton = new Button();
            this.mSaveButton.Text = "Save && Quit";
            this.mSaveButton.Name = "mSaveButton";
            this.mSaveButton.Click += new EventHandler(mSaveButton_Click);

            /* Setup the rescan button */
            this.mRescanButton = new Button();
            this.mRescanButton.Text = "Rescan";
            this.mRescanButton.Name = "mRescanButton";
            this.mRescanButton.Click += new EventHandler(mRescanButton_Click);

            /* Add things to the splitter */
            this.mSplitter.Panel1.Controls.Add(this.mTabControl);
            this.mSplitter.Panel2.Padding = new Padding(2,2,2,2);
            this.mTabControl.Dock = DockStyle.Fill;

            this.mSplitter.Panel2.Controls.Add(this.mSaveButton);
            this.mSaveButton.Dock = DockStyle.Left; // None; //DockStyle.Top | DockStyle.Left;
  
            this.mSplitter.Panel2.Controls.Add(this.mRescanButton);
            this.mRescanButton.Dock = DockStyle.Right; // None; //DockStyle.Top | DockStyle.Left;


            /* Add the splitter */
            this.mPanel.Controls.Add(this.mSplitter);

            this.mSaveButton.Height = this.mSplitter.Panel2.Height / 2 - this.mSplitter.Panel2.Padding.Vertical;
            this.mSaveButton.Width = this.mSplitter.Panel2.Width / 2   - this.mSplitter.Panel2.Padding.Horizontal;

            this.mRescanButton.Height = this.mSplitter.Panel2.Height / 2 - this.mSplitter.Panel2.Padding.Vertical;
            this.mRescanButton.Width = this.mSplitter.Panel2.Width / 2   - this.mSplitter.Panel2.Padding.Horizontal;

            this.mPanel.Show();
            foreach (Control child in this.mPanel.Controls)
            {
                child.Show();
            }


            /* Enumerate a letter for each sensor */
            mPossibleIds = new List<Char>();
            int limit = this.mbScanForPort ? mWmiCOMData.Length : mNexus.mSensorIDDict.Count;
            if (limit < 4) limit = 4;
            for (int i = 0; i < limit; ++i)
            {
                mPossibleIds.Add(Convert.ToChar(65 + i));
            }

            if (this.mbScanForPort)
            {                
                foreach (ManagementObject obj in mWmiCOMData)
                {
                    String pnpDeviceID = obj["PNPDeviceID"] as String;
                    String deviceID    = obj["DeviceID"]    as String; 

                    /** Parse the pnpDeviceID to get the MAC address */
                    int index = pnpDeviceID.LastIndexOf('_');
                    String MacAddress = pnpDeviceID.Substring(index - MAC_ADDRESS_LENGTH, MAC_ADDRESS_LENGTH);

                    GenerateControls(MacAddress, deviceID, this.mPossibleIds); 
                }
            }
            else
            {
                Logger.Info("Reading from Config file");
                
                foreach (SensorIdentification id in mNexus.mSensorIDDict.Values)
                {
                    GenerateControls(id, this.mPossibleIds);
                }
            }
            this.mTabControl.Show();
        }

        /// <summary>
        /// Calls the other overload of GenerateControls using the hidden id parameter.
        /// </summary>
        /// <param name="id">A SensorIdentification containing an already-detected sensor's info</param>
        /// <param name="mPossibleIds">A list of possible letters we can assign to that sensor.</param>
        private void GenerateControls(SensorIdentification id, List<Char> mPossibleIds)
        {
            GenerateControls(id.Mac, id.PortName, mPossibleIds, id);
        }

        /// <summary>
        /// Generates a tab for each sensor on the TabControl and every control in each tab
        /// </summary>
        /// <param name="MacAddress">Mac Address of the sensor</param>
        /// <param name="portName">The COM Port of th sensor</param>
        /// <param name="mPossibleIds">Possible Letters of the alphabet we can assign to this sensor</param>
        /// <param name="id">An optional SensorIdentification parameter that should not be passed in by the user. Use the overload instead.</param>
        private void GenerateControls(string MacAddress, string portName, List<char> mPossibleIds, SensorIdentification id = null)
        {
            TabPage newpage = new TabPage(portName);
            ComboBox newcombo = new ComboBox();
            Label maclabel = new Label();
            Label addrlabel = new Label();
            maclabel.Name = "maclabel";
            maclabel.Text = "MAC";
            addrlabel.Name = "addrlabel";
            addrlabel.Text = MacAddress;
            newcombo.Name = "newComboBox";

            foreach (Char c in this.mPossibleIds) { newcombo.Items.Add(c.ToString()); };

            newpage.Controls.Add(newcombo);
            newcombo.Dock = DockStyle.None;
            newcombo.Left = (mTabControl.Width / 2) - newcombo.Width;
            newcombo.Top = (mTabControl.Height / 2) - newcombo.Height;

            if (id == null) newcombo.SelectedIndex = 0;
            else newcombo.SelectedItem = id.Id;

            newpage.Controls.Add(maclabel);
            newpage.Dock = DockStyle.None;
            maclabel.Top = newcombo.Top - maclabel.Height;
            maclabel.Left = newcombo.Left;
            maclabel.AutoSize = true;

            newpage.Controls.Add(addrlabel);
            newpage.Dock = DockStyle.None;
            addrlabel.Top = maclabel.Top;
            addrlabel.Left = maclabel.Left + maclabel.Width;

            this.mTabControl.TabPages.Add(newpage);
        }

        /// <summary>
        /// Save Button event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mSaveButton_Click(object sender, EventArgs e)
        {
            this.PopulateDictionaries();

            this.SaveConfigFile();

            this.Close();
            
        }

        /// <summary>
        /// Rescan Button event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mRescanButton_Click(object sender, EventArgs e)
        {
            this.mPanel.Controls.Clear();
            this.mTabControl.TabPages.Clear();
            this.mNexus.mSensorDict.Clear();
            this.mNexus.mSensorIDDict.Clear();

            this.mSplitter.Hide();

            this.mPanel.Controls.Add(this.mFakeProgressBar);
            this.mPanel.Controls.Add(this.mScanLabel);

            this.mFakeProgressBar.Show();
            this.mScanLabel.Show();

            this.LaunchWorkerThread(false);
            
        }

        private void ConfigurationDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (false == bSaved)
            {
                DialogResult res = MessageBox.Show("Would you like to save the configuration?", "Save Dialog", MessageBoxButtons.YesNoCancel);

                if (System.Windows.Forms.DialogResult.Cancel == res)
                {
                    e.Cancel = true;
                }
                else if (System.Windows.Forms.DialogResult.Yes == res)
                {
                    this.mSaveButton_Click(sender, e);
                }
                else
                {
                    this.PopulateDictionaries();
                }
            }
            e.Cancel = true;
            this.Hide();
        }
        #endregion

        #region ConfigFunctions
        /// <summary>
        /// Reads the config file at %APPDATA%/Sensor-Aware-PT/config.xml
        /// </summary>
        private bool ReadConfigFile()
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
                    if (mNexus.mSensorIDDict.Keys.Contains(id.Id))
                    {
                        throw new ArgumentOutOfRangeException(String.Format("There is already a sensor with ID \"{0}\"", id.Id));
                    }
                    mNexus.mSensorIDDict[id.Id] = id;
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
            catch (FileNotFoundException exc)
            {
                Logger.Warning("FileNotFoundException- {0}", exc.FileName);
                if (fileStream.BaseStream.CanRead)
                {
                    fileStream.Close();
                }
                return false;
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
        /// Dumps the contents of the mNexus.mSensorIDDict into a .xml configuration file
        /// </summary>
        private void SaveConfigFile()
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

            SensorIdentification[] tmpArray = mNexus.mSensorIDDict.Values.ToArray();
            XmlSerializer serializer = new XmlSerializer(tmpArray.GetType());
            serializer.Serialize(fileStream, tmpArray);

            fileStream.Flush();
            fileStream.Close();

            this.bSaved = true;
        }


        private void PopulateDictionaries()
        {
            foreach (TabPage page in this.mTabControl.TabPages)
            {
                String COMPort = page.Text;
                String MACAddr = null;
                String Label = null;
                foreach (Control c in page.Controls)
                {
                    if (c.Name == "addrlabel")
                    {
                        MACAddr = (c as Label).Text;
                    }
                    if (c.Name == "newComboBox")
                    {
                        Label = ((c as ComboBox).SelectedItem) as String;
                        if (Label == null)
                        {
                            Label = ((c as ComboBox).SelectedValue as String);
                        }
                    }
                }
                SensorIdentification tmpSensorID = new SensorIdentification(Label, MACAddr, COMPort);

                if (mNexus.mSensorIDDict.ContainsKey(Label) == false)
                {
                    mNexus.mSensorIDDict.Add(Label, tmpSensorID);
                }
                if (mNexus.mSensorDict.ContainsKey(Label) == false)
                {
                    mNexus.mSensorDict.Add(Label, new Sensor(tmpSensorID));
                }
            }
        }

        #endregion
        #region Scans

        /// <summary>
        /// Scans WMI for new Bluetooth COM ports and then delays 60 seconds
        /// to give the sensors time to initialize. Overrides ScanForPorts(object, DoWorkEventArgs)
        /// </summary>
        private void ScanForPorts()
        {
            this.ScanForPorts(this, new DoWorkEventArgs(null));
        }

        /// <summary>
        /// The true ScanForPorts function/event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScanForPorts(object sender, DoWorkEventArgs e)
        {
            System.DateTime mStartOfInit = System.DateTime.Now;

            Logger.Info("Wmi scan started");
            mWmiCOMData = getWmiCOMData();

            System.TimeSpan timeSpent = System.DateTime.Now - mStartOfInit;
            double timeLeft = 60.0 - (int)timeSpent.TotalSeconds;
            if (timeLeft > 0)
            {
                Logger.Info("Delaying for {0} seconds while sensors initialize...", (int)timeLeft);
                //Thread.Sleep((int)(timeLeft * 1000));
            }
            Logger.Info("Wmi scan ended");
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

            /* Extremeley self-referencing code to get rid of WMI-detected COM ports that
             * have a MAC address of 000000000000 */
            wmiObjects = wmiObjects.Where(obj =>
                (obj["PNPDeviceID"] as String).Substring(
                (obj["PNPDeviceID"] as String).LastIndexOf('_') - MAC_ADDRESS_LENGTH, MAC_ADDRESS_LENGTH)
                != "000000000000").ToArray();

            Logger.Info("Verified {0} Bluetooth COM Device(s)", wmiObjects.Length);
            return wmiObjects;
        }
        #endregion


        #region oldcode


        /// <summary>
        /// Queries the system for bluetooth COM ports and enumerates the sensors using the provided data.
        /// </summary>
        //internal void probeWmiComPorts()
        //{


        //    foreach (ManagementObject obj in wmiCOMData)
        //    {
        //        String pnpDeviceID = obj["PNPDeviceID"] as String;
        //        String portName = obj["DeviceID"] as String;
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
        //            //mConfigFileDataDict[portName] = sensorIDstring;

        //            SensorIdentification sensorID = new SensorIdentification(sensorIDstring, MacAddress, portName);

        //            /** Create the Sensor with its SensorID **/
        //            mSensorDict[sensorIDstring] = new Sensor(sensorID);
        //        mNexus.mSensorIDDict[sensorIDstring] = sensorID;
        //        }
        //    }
        //}
        #endregion


    }
}
