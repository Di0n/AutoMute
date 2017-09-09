using Microsoft.Win32;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AutoMute
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool newInstance;
            string guid = ((GuidAttribute)Assembly.GetExecutingAssembly().
            GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();

            string mutexID = String.Format("Global\\{{{0}}}", guid);
            using (Mutex mutex = new Mutex(true, mutexID, out newInstance))
            {
                if (newInstance)
                {
                    Properties.Settings.Default.IsWindows10 = IsWin10();
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainApplicationContext());
                }
                else
                    MessageBox.Show("AutoMute is already running.", "AutoMute", MessageBoxButtons.OK, MessageBoxIcon.Information);

                try
                {
                    mutex.ReleaseMutex();
                }
                catch (ApplicationException) { }
                catch (ObjectDisposedException) { }
            }
        }

        static bool IsWin10()
        {
            var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            string productName = (string)reg.GetValue("ProductName");

            return productName.StartsWith("Windows 10");
        }
    }
}
