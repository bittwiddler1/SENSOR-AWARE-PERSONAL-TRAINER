using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sensor_Aware_PT
{
    // TODO rework to use numbers for log level so we can selectively filter shit out
    public class Logger
    {
        private Logger()
        {
        }

        public static void Print(string text, params object[] args)
        {
            DoLog("", text, args);
        }

        public static void Info(string text, params object[] args)
        {
            DoLog("INFO ", text, args);
        }

        public static void Warning(string text, params object[] args)
        {
            DoLog("WARNING ", text, args);
        }

        public static void Error(string text, params object[] args)
        {
            DoLog("!!!ERROR!!! ", text, args);
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
        }
    }
}
