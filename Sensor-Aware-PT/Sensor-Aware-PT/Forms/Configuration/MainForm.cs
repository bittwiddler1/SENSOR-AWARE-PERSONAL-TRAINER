using System;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using OpenTK;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.ComponentModel;
using System.Diagnostics;
using Sensor_Aware_PT.Forms;

namespace Sensor_Aware_PT
{
    public partial class MainForm : Form
    {

        private Nexus mSensorManager;
        private MappingDialog MD;
        private ConfigurationDialog CD;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();

            foreach( string s in ports )
            {
                Logger.Info( "{0}", s );
            }
            
            mSensorManager = Nexus.Instance;
            mSensorManager.InitializationComplete += new Nexus.InitializationCompleteHandler( mSensorManager_NexusInitializedEvent );
            mSensorManager.SensorStatusChanged += new Nexus.SensorStatusChangedHandler( mSensorManager_SensorStatusChanged );
            mSensorManager.initialize();
        }

        void mSensorManager_SensorStatusChanged( object sender, EventArgs e )
        {
            refreshSensorStatusList();
        }

        /// <summary>
        /// Handles the nexus ready event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mSensorManager_NexusInitializedEvent( object sender, EventArgs e )
        {
            refreshSensorStatusList();
        }

        private void refreshSensorStatusList()
        {
            List<Sensor> all = mSensorManager.getAllSensors();

            this.Invoke( ( MethodInvoker ) delegate
            {
                sensorListView.Items.Clear();
            } );

            foreach( Sensor s in all )
            {
                this.Invoke( ( MethodInvoker ) delegate
                {
                    
                    ListViewItem lvi = sensorListView.Items.Add( String.Format( "Sensor {0}", s.Id ) );

                    lvi.BackColor = s.IsActivated ? Color.Green : Color.Red;
                } );
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //bRunning = false;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
           // Thread.CurrentThread.Join();
           // port.Close();
        }


        private void btnLiveView_Click( object sender, EventArgs e )
        {

            LiveDataDisplayForm EF = new LiveDataDisplayForm();
            EF.subscribeToSource( Nexus.Instance );

            EF.Show();
        }

        private void btnRecorder_Click( object sender, EventArgs e )
        {
            DataRecorderForm SDV = new DataRecorderForm();
            SDV.Show();
        }

        private void btnStability_Click( object sender, EventArgs e )
        {
            LiveAccelerometerGraphForm liveGrah = new LiveAccelerometerGraphForm();

            liveGrah.Show();
            liveGrah.subscribeToSource( Nexus.Instance );
        }
   
      
        private void sensorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Nexus.Instance.mSensorDict.Clear();
            Nexus.Instance.mSensorIDDict.Clear();

            ConfigurationDialog CD = ConfigurationDialog.GetInstance();
            CD.ShowDialog();
        }

        private void mappingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MappingDialog MD = MappingDialog.GetInstance(); 
            MD.ShowDialog();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
