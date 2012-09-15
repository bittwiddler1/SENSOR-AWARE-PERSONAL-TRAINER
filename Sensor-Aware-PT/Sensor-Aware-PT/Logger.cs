using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sensor_Aware_PT
{
    // TODO rework to use numbers for log level so we can selectively filter shit out
    public static class Logger
    {
        public static int mLogLevel = 3;
        /// <summary>
        /// Log level 0
        /// </summary>
        /// <param name="text"></param>
        /// <param name="args"></param>
        public static void Print(string text, params object[] args)
        {
            DoLog("", text, args);
        }

        /// <summary>
        /// Log level 1
        /// </summary>
        /// <param name="text"></param>
        /// <param name="args"></param>
        public static void Info(string text, params object[] args)
        {
            if( mLogLevel >= 1 )
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                DoLog( "INFO ", text, args );
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Log level 2
        /// </summary>
        /// <param name="text"></param>
        /// <param name="args"></param>
        public static void Warning(string text, params object[] args)
        {
            if( mLogLevel >= 2 )
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                DoLog( "WARNING ", text, args );
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Log level 3
        /// </summary>
        /// <param name="text"></param>
        /// <param name="args"></param>
        public static void Error(string text, params object[] args)
        {
            if( mLogLevel >= 3 )
            {
                Console.ForegroundColor = ConsoleColor.Red;
                DoLog( "ERROR ", text, args );
                Console.ResetColor();
            }
        }

        public static void WaitForEnter()
        {
            WaitForEnter("Press ENTER to continue...");
        }

        public static void WaitForEnter(string prompt)
        {
            DoLog("", prompt);
            Console.ReadLine();
        }

        private static void DoLog(string prefix, string text, params object[] args)
        {

            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            
            Console.Write("[{0:D4}] [{1}] ",
                threadId,
                DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            Console.Write(prefix);
            Console.WriteLine(text, args);
            /*

            DateTime datet = DateTime.Now;
            String filePath = "Log" + datet.ToString( "MM_dd" ) + ".log";
            if( !File.Exists( filePath ) )
            {
                FileStream files = File.Create( filePath );
                files.Close();
            }
            try
            {
                StreamWriter sw = File.AppendText( filePath );
                sw.Write( prefix );
                sw.WriteLine( text, args );
                sw.Flush();
                sw.Close();
            }
            catch( Exception e )
            {
                Console.WriteLine( e.Message.ToString() );
            }
             */
        }
    }
}
