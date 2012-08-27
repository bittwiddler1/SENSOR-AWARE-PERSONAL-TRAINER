using System;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using OpenTK;
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
        public static Form_3Dcuboid formF;


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

            formF = new Form_3Dcuboid();

            BackgroundWorker bg = new BackgroundWorker();
            bg.DoWork += new DoWorkEventHandler( delegate
            {
                formF.ShowDialog();
            } );

            bg.RunWorkerAsync();
            mSensorManager = new Nexus();
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
