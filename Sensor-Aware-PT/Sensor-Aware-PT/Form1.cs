using System;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Sensor_Aware_PT
{
    public partial class MainForm : Form
    {
        private bool loaded = false;
        private bool bRunning = true;

        private SerialPort port;
        private Thread ReadThread;
        private Nexus mSensorManager;
        private ExperimentalDisplay ed = new ExperimentalDisplay();

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
            
            ReadThread = new Thread( () =>
            {
                ed.Run( 60, 60 );
            } );
            ReadThread.IsBackground = true;
            ReadThread.Start();
            
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


    }
}
