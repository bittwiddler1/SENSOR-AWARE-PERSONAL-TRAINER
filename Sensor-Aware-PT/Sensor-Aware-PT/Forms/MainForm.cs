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


        private void button1_Click( object sender, EventArgs e )
        {
            /*

            BackgroundWorker bg = new BackgroundWorker();

            bg.DoWork += new DoWorkEventHandler( delegate
            {
                EF.ShowDialog();
            } );

            bg.RunWorkerAsync();
             * */
            ExperimentalForm EF = new ExperimentalForm();
            EF.subscribeToSource( Nexus.Instance );
            EF.Show();
        }

        private void button2_Click( object sender, EventArgs e )
        {
            /*
            BackgroundWorker bg = new BackgroundWorker();

            bg.DoWork += new DoWorkEventHandler( delegate
            {
                SDV.ShowDialog();
                
            } );

            bg.RunWorkerAsync();
             * */
            SensorDataView SDV = new SensorDataView();
            SDV.Show();
        }

        private void button3_Click( object sender, EventArgs e )
        {
            MappingDialog MD = new MappingDialog();
            MD.ShowDialog();
        }

        private void listView1_SelectedIndexChanged( object sender, EventArgs e )
        {

        }
    }
}
