using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;


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

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i );
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}


