using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LDBot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new ThreadExceptionEventHandler(ThreadExceptionHandler);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);
            try
            {
                Application.Run(new FormMain());
            }
            catch(Exception e)
            {
                if (!File.Exists("error_log.txt"))
                {
                    using (StreamWriter sw = File.CreateText("error_log.txt"))
                    {
                        sw.WriteLine("ERROR " + DateTime.Now.ToString());
                        sw.WriteLine("Target Site: " + e.TargetSite.Name);
                        sw.WriteLine(e.StackTrace);
                        sw.WriteLine("=======================================================================");
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText("error_log.txt"))
                    {
                        sw.WriteLine("ERROR " + DateTime.Now.ToString());
                        sw.WriteLine("Target Site: " + e.TargetSite.Name);
                        sw.WriteLine(e.StackTrace);
                        sw.WriteLine("=======================================================================");
                    }
                }
            }
        }

        private static void ThreadExceptionHandler(object obj, ThreadExceptionEventArgs e)
        { }

        private static void UnhandledExceptionHandler(object obj, UnhandledExceptionEventArgs e)
        { }
    }
}
