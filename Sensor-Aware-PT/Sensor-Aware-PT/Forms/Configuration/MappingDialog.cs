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
using Sensor_Aware_PT.Forms;

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
        private BackgroundWorker mConfigReader = null;
        private Nexus mNexus = Nexus.Instance;
        private Dictionary<String, BoneType> mSensorMappings;
        private String[] mValidSensors = null;
        private int mPageCount = 0;

        private bool bSaved = false;
        #endregion

        static private MappingDialog mInstance = null;
        #region ConstructorDeconstructor
        private MappingDialog()
        {
            InitializeComponent();
            mSensorMappings = mNexus.BoneMappings;
 
            this.LaunchWorkerThread();
            //this.Focus();
        }


        internal static MappingDialog Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new MappingDialog();

                return mInstance;
            }
        }

        private void LaunchWorkerThread()
        {
            /* Threading stuff */
            this.mConfigReader = new BackgroundWorker();
            this.mConfigReader.DoWork += new System.ComponentModel.DoWorkEventHandler(this.Configure);

            this.mConfigReader.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.GenerateForm);
            this.mConfigReader.RunWorkerAsync();
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
                this.mValidSensors = mNexus.mSensorDict.Select(sensor => sensor.Value.Id).ToArray<String>();
                this.ClearAllMappings();
            }
            catch (ArgumentException)
            {
                return;
            }

            try
            {
                if (this.ReadConfigFile() == false) 
                    GenerateDefaultConfig();
            }
            catch (FileNotFoundException)
            {
                GenerateDefaultConfig();
            }
            catch (DirectoryNotFoundException)
            {
                GenerateDefaultConfig();
            }
        }

        private void ClearAllMappings()
        {
            mSensorMappings.Clear();

            foreach( String t in this.mValidSensors )
            {
                mSensorMappings.Add( t, BoneType.None );
            }
        }

        void GenerateDefaultConfig()
        {
            Logger.Warning("Could not read mapping configuration file!");
            Logger.Warning("A new one will have to be generated!");

            BoneType current  = BoneType.ArmUpperL;

            foreach( Sensor sensy in mNexus.getAllSensors() )
            {
                mSensorMappings[ sensy.Id ] = current;
                ++current;
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
            this.mSplitter.SplitterDistance = 200;
            this.mSplitter.IsSplitterFixed = true;
            this.mSplitter.Show();
            
            //this.mSplitter.Panel1.Controls.Add(this.mTabControl);


            /* Add things to the splitter */
            this.mSplitter.Panel1.Controls.Add(this.mTabControl);
            this.mSplitter.Panel2.Padding = new Padding(2, 2, 2, 2);
            this.mTabControl.Dock = DockStyle.Fill;

            this.mSplitter.Panel2.Controls.Add(this.mQuitButton);
            this.mQuitButton.Dock = DockStyle.Left; 

            this.mSplitter.Panel2.Controls.Add(this.mQuitButton);
            this.mQuitButton.Dock = DockStyle.Right;


            /* Add the splitter */
            this.mPanel.Controls.Add(this.mSplitter);

            this.mQuitButton.Height = this.mSplitter.Panel2.Height / 2 - this.mSplitter.Panel2.Padding.Vertical;
            this.mQuitButton.Width = this.mSplitter.Panel2.Width / 2 - this.mSplitter.Panel2.Padding.Horizontal;

            this.mQuitButton.Height = this.mSplitter.Panel2.Height / 2 - this.mSplitter.Panel2.Padding.Vertical;
            this.mQuitButton.Width = this.mSplitter.Panel2.Width / 2 - this.mSplitter.Panel2.Padding.Horizontal;


            this.mPanel.Show();
            foreach (Control child in this.mPanel.Controls)
            {
                child.Show();
            }

            foreach (String sensorlabel in this.mValidSensors)
            {
                GenerateControls(sensorlabel);
            }
            this.mTabControl.Show();
 
           // this.mTabControl.Show();
        }

        /// <summary>
        /// Calls the other overload of GenerateControls using the hidden id parameter.
        /// </summary>
        /// <param name="id">A SensorIdentification containing an already-detected sensor's info</param>
        /// <param name="mPossibleIds">A list of possible letters we can assign to that sensor.</param>
        private void GenerateControls(String sensorLabel)
        {
            if (mTabControl.TabPages.ContainsKey(sensorLabel) == false)
            {
                TabPage newpage = new TabPage(sensorLabel);
                ComboBox newcombo = new ComboBox();

                newcombo.Name = "newComboBox";
                newcombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

                foreach (BoneType t in Enum.GetValues(typeof(BoneType)))
                    newcombo.Items.Add(t.ToString());

                newpage.Controls.Add(newcombo);
                newcombo.Dock = DockStyle.None;
                newcombo.Left = (mTabControl.Width / 2) - newcombo.Width;
                newcombo.Top = (mTabControl.Height / 2) - newcombo.Height;


                if (mNexus.GetSensor(sensorLabel) != null)
                {
                    newcombo.SelectedIndex = (Int32)this.mSensorMappings[sensorLabel];
                }

                this.mTabControl.TabPages.Add(newpage);
                newpage.Show();
            }
        }

        /// <summary>
        /// Save Button event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mSaveButton_Click(object sender, EventArgs e)
        {
            if (false == this.VerifyInput())
            {
                MessageBox.Show( "Configuration not valid. Stop confusing me", "You're a jerk", MessageBoxButtons.OK );
                return;
            }
            this.ClearAllMappings();
            this.PopulateDictionaries();

            this.SaveConfigFile();

            this.Close();
            
        }

        

        /// <summary>
        /// Rescan Button event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mQuitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region ConfigFileFunctions
        /// <summary>
        /// Reads the config file at %APPDATA%/Sensor-Aware-PT/config.xml
        /// </summary>
        public bool ReadConfigFile()
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
                    
                    //Sensor currentSensor = null;
                    //if (pair.SensorLabel != null)
                    //{
                    //    currentSensor = mNexus.GetSensor(pair.SensorLabel);

                    //    if (currentSensor != null && mSensorMappings.Keys.Contains(currentSensor.Id))
                    //    {
                    //        throw new ArgumentOutOfRangeException(String.Format("There is already a sensor with ID \"{0}\"", pair.SensorLabel));
                    //    }
                    //}
                    mSensorMappings[pair.SensorLabel] = pair.Bone;
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

            BoneLabelPair[] tmpArray = new BoneLabelPair[mSensorMappings.Count];
            int count = 0;

            foreach (String t in this.mValidSensors) //(BoneType t in mSensorMappings.Keys)
            {
                tmpArray[count] = new BoneLabelPair(t, BoneType.None);
                tmpArray[count].Bone = mSensorMappings[t];
                ++count;
            }
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
                String label = page.Text;
                BoneType bone = BoneType.None;
                

                foreach (Control c in page.Controls)
                {
                    if (c.Name == "newComboBox")
                    {
                        try
                        {
                            bone = (BoneType)Enum.Parse(typeof(BoneType),
                                                       ((c as ComboBox).SelectedItem as String));
                        }
                        catch (ArgumentException)
                        {
                            bone = (BoneType)Enum.Parse(typeof(BoneType),
                                                   ((c as ComboBox).SelectedValue as String));
                        }
                    }
                }

                if( mSensorMappings.ContainsKey( label ) == false )
                {
                    mSensorMappings.Add( label , bone );
                }
                else
                {
                    mSensorMappings[ label ] = bone;
                }
                
            }
        }

        private bool VerifyInput()
        {

            Dictionary<String, Object> CCComboBoxResults = new Dictionary<String, Object>();
            foreach( TabPage tabby in mTabControl.TabPages )
            {
                foreach( Control cont in tabby.Controls )
                {
                    if( "newComboBox" == cont.Name )
                    {
                        String name = ( cont as ComboBox ).SelectedItem.ToString();

                        try
                        {
                            CCComboBoxResults.Add( name, null );
                        }
                        catch( Exception )
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        #endregion

        private void MappingDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

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
