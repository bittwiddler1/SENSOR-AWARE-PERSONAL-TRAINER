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


    public partial class MappingDialog : Form
    {
        #region Constants
        private const String CFG_DIR = "Sensor-Aware-PT";
        private static String AppDataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private String ConfigFilePath = Path.Combine(new String[] { AppDataDirPath, CFG_DIR, "mappings.xml" });
        private const int MAC_ADDRESS_LENGTH = 12;
        #endregion

        #region InstanceVariables
        private BackgroundWorker mRescanWorker = null;
        private Nexus mNexus = Nexus.Instance;
        private Dictionary<BoneType, Sensor> mSensorMappings;
        private List<BoneType> mPossibleBones;

        private int mPageCount = 0;

        #region GenerateWinformShit
        private Button mSaveButton = null;
        private Button mQuitButton = null;
        #endregion

        #endregion

        #region ConstructorDeconstructor
        public MappingDialog()
        {
            InitializeComponent();
            mSensorMappings = mNexus.BoneMapping;

            this.LaunchWorkerThread();
            this.Focus();
        }

        private void LaunchWorkerThread()
        {
            /* Threading stuff */
            this.mRescanWorker = new BackgroundWorker();
            this.mRescanWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.Configure);

            this.mRescanWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.GenerateForm);
            this.mRescanWorker.RunWorkerAsync();
        }
        #endregion

        #region EventHandlers
        /// <summary>
        /// Worker thread that attempts to read the Config
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Configure(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (this.readConfigFile() == false) 
                    HandleConfigNotFound();
            }
            catch (FileNotFoundException)
            {
                HandleConfigNotFound();
            }
            catch (DirectoryNotFoundException)
            {
                HandleConfigNotFound();
            }
        }

        void HandleConfigNotFound()
        {
            Logger.Warning("Could not read mapping configuration file!");
            Logger.Warning("A new one will have to be generated!");

            try
            {
                mSensorMappings[BoneType.ArmUpperL] = mNexus.GetSensor("A");
                mSensorMappings[BoneType.ArmLowerL] = mNexus.GetSensor("C");
                mSensorMappings[BoneType.ArmUpperR] = mNexus.GetSensor("B");
                mSensorMappings[BoneType.ArmLowerR] = mNexus.GetSensor("D");
            }
            catch (KeyNotFoundException exc)
            {
                Logger.Warning("One or more sensors is missing from the configuration Dictionary!");
                Logger.Warning("Exception- {0}", exc);
                
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

            /* Set up the Splitter */
            this.mSplitter.Name = "mSplitter";
            this.mSplitter.Orientation = Orientation.Horizontal;
            this.mSplitter.Dock = DockStyle.Fill;
            this.mSplitter.SplitterDistance = 300;
            this.mSplitter.IsSplitterFixed = true;
            this.mSplitter.Show();
            
            this.mSplitter.Panel1.Controls.Add(this.mTabControl);
            //this.mTabControl
            /* Setup the Button */
            this.mSaveButton = new Button();
            this.mSaveButton.Text = "Save";
            this.mSaveButton.Name = "mSaveButton";
            this.mSaveButton.Click += new EventHandler(mSaveButton_Click);

            /* Setup the rescan button */
            this.mQuitButton = new Button();
            this.mQuitButton.Text = "Quit";
            this.mQuitButton.Name = "mQuitButton";
            this.mQuitButton.Click += new EventHandler(mQuitButton_Click);

            /* Add things to the splitter */
            this.mSplitter.Panel1.Controls.Add(this.mTabControl);
            this.mSplitter.Panel2.Padding = new Padding(2, 2, 2, 2);
            this.mTabControl.Dock = DockStyle.Fill;

            this.mSplitter.Panel2.Controls.Add(this.mSaveButton);
            this.mSaveButton.Dock = DockStyle.Left; 

            this.mSplitter.Panel2.Controls.Add(this.mQuitButton);
            this.mQuitButton.Dock = DockStyle.Right;


            /* Add the splitter */
            this.mPanel.Controls.Add(this.mSplitter);

            this.mSaveButton.Height = this.mSplitter.Panel2.Height / 2 - this.mSplitter.Panel2.Padding.Vertical;
            this.mSaveButton.Width = this.mSplitter.Panel2.Width / 2 - this.mSplitter.Panel2.Padding.Horizontal;

            this.mQuitButton.Height = this.mSplitter.Panel2.Height / 2 - this.mSplitter.Panel2.Padding.Vertical;
            this.mQuitButton.Width = this.mSplitter.Panel2.Width / 2 - this.mSplitter.Panel2.Padding.Horizontal;


            this.mPanel.Show();
            foreach (Control child in this.mPanel.Controls)
            {
                child.Show();
            }

            foreach (Sensor sensor in mSensorMappings.Values)
            {
                GenerateControls(sensor.Id);
            }
            this.mTabControl.Show();
 
            this.mTabControl.Show();
        }

        /// <summary>
        /// Calls the other overload of GenerateControls using the hidden id parameter.
        /// </summary>
        /// <param name="id">A SensorIdentification containing an already-detected sensor's info</param>
        /// <param name="mPossibleIds">A list of possible letters we can assign to that sensor.</param>
        private void GenerateControls(String sensorLabel)
        {
            
            TabPage newpage = new TabPage(sensorLabel);
            ComboBox newcombo = new ComboBox();

            newcombo.Name = "newComboBox";

            foreach (BoneType t in Enum.GetValues(typeof(BoneType)))
                newcombo.Items.Add(t.ToString());
            
            
            newpage.Controls.Add(newcombo);
            newcombo.Dock = DockStyle.None;
            newcombo.Left = (mTabControl.Width / 2) - newcombo.Width;
            newcombo.Top = (mTabControl.Height / 2) - newcombo.Height;

            if (sensorLabel == null) newcombo.SelectedIndex = mPageCount;
           

            this.mTabControl.TabPages.Add(newpage);
            newpage.Show();
            ++mPageCount;
        }

        /// <summary>
        /// Save Button event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mSaveButton_Click(object sender, EventArgs e)
        {
            foreach (TabPage page in this.mTabControl.TabPages)
            {
                String COMPort = page.Text;
                String MACAddr = null;
                String Label   = null;
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
            this.saveConfigFile();
            this.Close();
            
        }

        /// <summary>
        /// Rescan Button event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mQuitButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
            
        }
        #endregion

        #region ConfigFileFunctions
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

                BoneLabelPair[] tmpArray = new BoneLabelPair[1] { null };
                XmlSerializer serializer = new XmlSerializer(tmpArray.GetType());

                // Take each array object and insert it into the id dictionary
                tmpArray = serializer.Deserialize(fileStream) as BoneLabelPair[];

                foreach (BoneLabelPair pair in tmpArray)
                {
                    Sensor currentSensor = mNexus.GetSensor(pair.SensorLabel);
                    if (mSensorMappings.Values.Contains(currentSensor))
                    {
                        throw new ArgumentOutOfRangeException(String.Format("There is already a sensor with ID \"{0}\"", pair.SensorLabel));
                    }
                    mSensorMappings[pair.Bone] = currentSensor;
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
                retval = false;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
            return retval;
        }

        /// <summary>
        /// Dumps the contents of the mNexus.mSensorIDDict into a .xml configuration file
        /// </summary>
        private void saveConfigFile()
        {
            throw new NotImplementedException();   
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
