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
    public partial class MainForm : Form, IObserver<DataFrame>
    {
        
        
        ExperimentalForm EF = new ExperimentalForm();
        SensorDataView SDV = new SensorDataView();

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

            long a, b, c;
            c = Stopwatch.Frequency;
            a = Stopwatch.GetTimestamp();
            double d = ( double ) ( a / c );
            Logger.Info( a.ToString() );
            Logger.Info( "Stop watch is high res? {0}", Stopwatch.IsHighResolution );
            b = Stopwatch.GetTimestamp();
            Stopwatch jg;
            
            double ef = ( double ) ( b / c );
            Logger.Info( b.ToString() );

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
            ///** If the nexus is now ready, query for the active sensors and attach a cuboid window to them */
            //List<Sensor> activeSensors = mSensorManager.getActivatedSensors();
            ////foreach( Sensor s in activeSensors )
            ////attachCuboidWindow( s );
            //{
            //    Sensor s = activeSensors[ 0 ];
            //    Sensor s2 = activeSensors[ 1 ];
            //    Sensor s3 = activeSensors[ 2 ];
            //    Sensor s4 = activeSensors[ 3 ];
            //    ExperimentalForm cuboid = new ExperimentalForm( s, s2,s3, s4,mSensorManager );
            //    /** Create background worker to show the form and run it asynchronously */
            //    BackgroundWorker cuboidWorker = new BackgroundWorker();
            //    cuboidWorker.DoWork += new DoWorkEventHandler( delegate
            //    {
            //        cuboid.ShowDialog();

            //    } );
            //    cuboidWorker.RunWorkerAsync();
            //}
            
            //mSensorManager.Subscribe( this );

            

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
            //bRunning = false;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
           // Thread.CurrentThread.Join();
           // port.Close();
        }


        #region IObserver<NexusDataFrame> Members

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError( Exception error )
        {
            //throw new NotImplementedException();
        }

        public void OnNext( DataFrame value )
        {
            //throw new NotImplementedException();
            Logger.Info( "Nexus data frame {0}", value.sequenceNumber );
            foreach( KeyValuePair<String, SensorDataEntry> kv in value.concurrentData )
            {
                Logger.Info( "Sensor {0} data frame: {1},{2},{3}",
                    kv.Key, 
                    kv.Value.orientation.X,
                    kv.Value.orientation.Y,
                    kv.Value.orientation.Z );
            }
        }

        #endregion

        private void button1_Click( object sender, EventArgs e )
        {

            BackgroundWorker bg = new BackgroundWorker();

            bg.DoWork += new DoWorkEventHandler( delegate
            {
                EF.ShowDialog();
            } );

            bg.RunWorkerAsync();
        }

        private void button2_Click( object sender, EventArgs e )
        {
            BackgroundWorker bg = new BackgroundWorker();

            bg.DoWork += new DoWorkEventHandler( delegate
            {
                SDV.ShowDialog();
            } );

            bg.RunWorkerAsync();
        }
    }
}
