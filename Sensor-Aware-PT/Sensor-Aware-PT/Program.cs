using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Threading;


namespace Sensor_Aware_PT
{

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        ///     

        
        [DllImport( "kernel32.dll" )]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        [STAThread]
        static void Main()
        {
                        // redirect console output to parent process;
            // must be before any calls to Console.WriteLine()
            AttachConsole( ATTACH_PARENT_PROCESS );
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.SetUnhandledExceptionMode( UnhandledExceptionMode.CatchException );
            System.Windows.Forms.Application.ThreadException += new System.Threading.ThreadExceptionEventHandler( onGuiUnhandledException );
            AppDomain.CurrentDomain.UnhandledException += onUnhandledException;

            Application.Run(new MainForm());
        }

        private static void onGuiUnhandledException(object sender, ThreadExceptionEventArgs e)
        {
            handleUnhandledException(e.Exception);
        }

        private static void onUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            handleUnhandledException(e.ExceptionObject);
        }

        private static void handleUnhandledException(Object o)
        {
            Exception e = o as Exception;

            if (e != null)
            {
                StringBuilder tmp = new StringBuilder(e.GetType().ToString() + ": ");
                tmp.Append(e.Message);
                tmp.AppendLine();
                tmp.AppendLine(e.StackTrace);
                Logger.Error(tmp.ToString());

                //MessageBox.Show(MainForm, e.GetType().ToString(), e.Message+"\nThe program will now exit\n\nStackTrace:\n"+e.StackTrace, MessageBoxButtons.OK);
            }
        }
    }
    
}


