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

namespace Sensor_Aware_PT
{
    public partial class MainForm : Form
    {
        private bool loaded = false;
        private bool bRunning = true;
        private Nexus mSensorManager;
        
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Add(5f);
            chart1.Series[0].Points.Add(7f);
            chart1.Series[0].Points.Add(5f);
            string[] ports = SerialPort.GetPortNames();

            foreach( string s in ports )
            {
                Logger.Info( "{0}", s );
            }

            mSensorManager = new Nexus();
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
            /** If the nexus is now ready, query for the active sensors and attach a cuboid window to them */
            List<Sensor> activeSensors = mSensorManager.getActivatedSensors();
            //foreach( Sensor s in activeSensors )
            //attachCuboidWindow( s );
            {
                Sensor s = activeSensors[ 0 ];
                Sensor s2 = activeSensors[ 1 ];
                ExperimentalForm cuboid = new ExperimentalForm( s, s2, mSensorManager );
                /** Create background worker to show the form and run it asynchronously */
                BackgroundWorker cuboidWorker = new BackgroundWorker();
                cuboidWorker.DoWork += new DoWorkEventHandler( delegate
                {
                    cuboid.ShowDialog();

                } );
                cuboidWorker.RunWorkerAsync();
            }
        }


        /// <summary>
        /// Takes a sensor and attaches a cuboid window to it
        /// </summary>
        /// <param name="sensor"></param>
        private void attachCuboidWindow(Sensor sensor)
        {
            /** Create new cuboid form attached to this sensor*/
            Form_3Dcuboid cuboid = new Form_3Dcuboid();
            /** Create background worker to show the form and run it asynchronously */
            BackgroundWorker cuboidWorker = new BackgroundWorker();
            cuboidWorker.DoWork += new DoWorkEventHandler( delegate
            {
                cuboid.setSensor( sensor );
                cuboid.ShowDialog();
            } );
            cuboidWorker.RunWorkerAsync();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bRunning = false;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
           // Thread.CurrentThread.Join();
           // port.Close();
        }

        private void chart1_Click( object sender, EventArgs e )
        {

        }

    }
}
