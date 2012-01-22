/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2012 Matt Robinson
 * 
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
 * License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with this program.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

namespace RadioDld
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;
    using Microsoft.VisualBasic.ApplicationServices;

    internal class AppInstance : WindowsFormsApplicationBase
    {
        private AppInstance()
        {
            this.IsSingleInstance = true;
            this.EnableVisualStyles = true;
            this.ShutdownStyle = ShutdownMode.AfterMainFormCloses;

            Startup += this.App_Startup;
            UnhandledException += this.App_UnhandledException;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            if (!Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += AppDomainExceptionHandler;
            }

            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                new AppInstance().Run(args);
            }
            catch (CantStartSingleInstanceException)
            {
                MessageBox.Show("Radio Downloader is already running, but is not responding." + Environment.NewLine + Environment.NewLine + "Please wait a few minutes and try again, or if the problem persists you can try restarting your system.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        protected override void OnCreateMainForm()
        {
            this.MainForm = new Main();
            StartupNextInstance += ((Main)this.MainForm).App_StartupNextInstance;
        }

        private static void ReportException(Exception exp)
        {
            ErrorReporting report = new ErrorReporting(exp);

            using (ReportError showError = new ReportError())
            {
                showError.ShowReport(report);
            }
        }

        private static void AppDomainExceptionHandler(object sender, System.UnhandledExceptionEventArgs e)
        {
            Exception unhandledExp = null;

            try
            {
                unhandledExp = (Exception)e.ExceptionObject;
            }
            catch (InvalidCastException)
            {
                // The ExceptionObject isn't a child of System.Exception, so we don't know
                // how to report it.  Instead, let the standard .net dialog appear.
                return;
            }

            ReportException(unhandledExp);
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            // If /exit was passed on the command line, then just exit immediately
            foreach (string commandLineArg in Environment.GetCommandLineArgs())
            {
                if (commandLineArg.ToUpperInvariant() == "/EXIT")
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Make sure that the temp folder exists
            Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "RadioDownloader"));

            const string DbFileName = "store.db";
            string tmplDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DbFileName);
            string appDbPath = Path.Combine(FileUtils.GetAppDataFolder(), DbFileName);

            // Ensure that the template database exists
            if (!File.Exists(tmplDbPath))
            {
                MessageBox.Show("The Radio Downloader template database was not found at '" + tmplDbPath + "'." + Environment.NewLine + Environment.NewLine + "Try repairing the Radio Downloader installation or installing the latest version from nerdoftheherd.com", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                e.Cancel = true;
                return;
            }

            // Test if there is an existing application database
            if (!File.Exists(appDbPath))
            {
                // Start with a copy of the template database
                File.Copy(tmplDbPath, appDbPath);
            }
            else
            {
                // Update the current database to match the template database structure
                using (UpdateDB doDbUpdate = new UpdateDB(tmplDbPath, appDbPath))
                {
                    doDbUpdate.UpdateStructure();
                }
            }
        }

        private void App_UnhandledException(object sender, Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs e)
        {
            ReportException(e.Exception);
        }
    }
}
