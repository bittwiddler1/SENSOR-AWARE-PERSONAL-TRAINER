using System;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
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

        public MainForm()
        {
            InitializeComponent();
            /*
            ReadThread = new Thread(ReadThread_main);
            ReadThread.Name = "Read Thread";
            ReadThread.IsBackground = true;
             * */
        }

        private void ReadThread_main()
        {
            while (bRunning)
            {
                try
                {
                    String message = port.ReadLine();
                    Console.WriteLine(message);
                }
                catch (TimeoutException)
                {

                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            string[] ports = SerialPort.GetPortNames();

            foreach( string s in ports )
            {
                Logger.Info( "{0}", s );
            }
            mSensorManager = new Nexus();
            
            /*
            // Allow the user to set the appropriate properties
            port = new SerialPort();
            port.PortName = "COM3";
            port.BaudRate = 9600;
            port.Parity   = Parity.Even;
            port.DataBits = 7;
            port.StopBits = StopBits.One;
            port.Handshake = Handshake.None;

            // Read/Write timeout
            port.ReadTimeout  = 500;
            port.WriteTimeout = 500;

            port.Open();
            ReadThread.Start();
             * */
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bRunning = false;
            //ReadThread.Abort();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
           // Thread.CurrentThread.Join();
           // port.Close();
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            loaded = true;
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            if (!loaded)
                return;
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded)
                return;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            glControl1.SwapBuffers();
        }
    }
}
