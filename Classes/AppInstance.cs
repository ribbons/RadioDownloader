/*
 * Copyright © 2007-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Windows.Forms;

    using Microsoft.VisualBasic.ApplicationServices;

    internal class AppInstance : WindowsFormsApplicationBase
    {
        private AppInstance()
        {
            this.IsSingleInstance = true;
            this.EnableVisualStyles = true;
            this.ShutdownStyle = ShutdownMode.AfterMainFormCloses;
            this.UnhandledException += this.App_UnhandledException;
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
            // If /exit was passed on the command line, then just exit immediately
            foreach (string commandLineArg in Environment.GetCommandLineArgs())
            {
                if (commandLineArg.ToUpperInvariant() == "/EXIT")
                {
                    Environment.Exit(1);
                }
            }

            if (!OsUtils.Windows())
            {
                // Mono has a bug which causes the useDefaultCredentials attribute to be
                // treated as invalid, so clear the default proxy to prevent an exception
                WebRequest.DefaultWebProxy = null;
            }

            try
            {
                // Add TLS 1.1 and 1.2 to allowed protocols for HTTPS requests
                // Constants are not defined until .NET 4.5, so use the values
                ServicePointManager.SecurityProtocol |=
                    (SecurityProtocolType)0x00000300 | // SecurityProtocolType.Tls11
                    (SecurityProtocolType)0x00000C00;  // SecurityProtocolType.Tls12
            }
            catch (NotSupportedException)
            {
                if (OsUtils.Windows())
                {
                    MessageBox.Show(
                        "The .NET framework needs an update (to enable TLS 1.1 and 1.2) before Radio Downloader can run." + Environment.NewLine + Environment.NewLine +
                        "Please check for available updates and install all of those which relate to the .NET framework.",
                        Application.ProductName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Stop);

                    Environment.Exit(1);
                }

                throw;
            }

            // Set up the application database and perform any required updates or cleanup
            if (!DatabaseInit.Startup())
            {
                Environment.Exit(1);
            }

            this.MainForm = new Main();
            this.StartupNextInstance += ((Main)this.MainForm).App_StartupNextInstance;
        }

        private static void ReportException(Exception exp)
        {
            ErrorReporting report = new ErrorReporting(exp);

            using (ReportError showError = new ReportError())
            {
                showError.ShowReport(report);
            }

            Environment.Exit(1);
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

        private void App_UnhandledException(object sender, Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs e)
        {
            ReportException(e.Exception);
        }
    }
}
