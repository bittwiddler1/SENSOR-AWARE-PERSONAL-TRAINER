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

namespace Sensor_Aware_PT
{
    public partial class MainForm : Form
    {

        private Nexus mSensorManager;
        
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
            
            mSensorManager.initialize();
        }

        /// <summary>
        /// Handles the nexus ready event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mSensorManager_NexusInitializedEvent( object sender, EventArgs e )
        {
            List<Sensor> all = mSensorManager.getAllSensors();
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
            /*
            BackgroundWorker bg = new BackgroundWorker();

            bg.DoWork += new DoWorkEventHandler( delegate
            {
                SDV.ShowDialog();
                
            } );

            bg.RunWorkerAsync();
             * */
            DataRecorderForm SDV = new DataRecorderForm();
            SDV.Show();
        }

        private void btnStability_Click( object sender, EventArgs e )
        {
   
        }

        private void listView1_SelectedIndexChanged( object sender, EventArgs e )
        {
            
        }

        private void sensorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigurationDialog CD = new ConfigurationDialog();
            CD.ShowDialog();
        }

        private void mappingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MappingDialog MD = new MappingDialog();
            MD.ShowDialog();
        }
    }
}
