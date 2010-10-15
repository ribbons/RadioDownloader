using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
// Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
// Copyright © 2007-2010 Matt Robinson
//
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
// Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
// option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
// License for more details.
//
// You should have received a copy of the GNU General Public License along with this program; if not, write
// to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

namespace RadioDld.My
{
	internal partial class MyApplication
	{
		private void MyApplication_Startup(object sender, Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
		{
			// Add an extra handler to catch unhandled exceptions in other threads
			if (Debugger.IsAttached == false) {
				AppDomain.CurrentDomain.UnhandledException += AppDomainExceptionHandler;
			}

			// If /exit was passed on the command line, then just exit immediately
			foreach (string commandLineArg in Environment.GetCommandLineArgs()) {
				if (commandLineArg.ToUpperInvariant() == "/EXIT") {
					e.Cancel = true;
					return;
				}
			}
		}

		private void MyApplication_StartupNextInstance(object sender, Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs e)
		{
			foreach (string commandLineArg in e.CommandLine) {
				if (commandLineArg.ToUpperInvariant() == "/EXIT") {
					// Close the application
					My.MyProject.Forms.Main.mnuTrayExit_Click(sender, e);
					return;
				}
			}

			// Do the same as a double click on the tray icon
			My.MyProject.Forms.Main.mnuTrayShow_Click(sender, e);
		}

		private void MyApplication_UnhandledException(object sender, Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs e)
		{
			if (My.MyProject.Forms.ReportError.Visible == false) {
				ErrorReporting report = new ErrorReporting(e.Exception);
				My.MyProject.Forms.ReportError.AssignReport(report);
				My.MyProject.Forms.ReportError.ShowDialog();
			}
		}

		private void AppDomainExceptionHandler(object sender, System.UnhandledExceptionEventArgs e)
		{
			Exception unhandledExp = null;

			try {
				unhandledExp = (Exception)e.ExceptionObject;
			} catch (InvalidCastException classCastExp) {
				// The ExceptionObject isn't a child of System.Exception, so we don't know
				// how to report it.  Instead, let the standard .net dialog appear.
				return;
			}

			if (My.MyProject.Forms.ReportError.Visible == false) {
				ErrorReporting report = new ErrorReporting(unhandledExp);
				My.MyProject.Forms.ReportError.AssignReport(report);
				My.MyProject.Forms.ReportError.ShowDialog();
			}
		}
	}
}
