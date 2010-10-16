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

namespace RadioDld
{


	internal partial class ReportError
	{

		private ErrorReporting report;
		public void AssignReport(ErrorReporting report)
		{
			this.report = report;
		}

		private void cmdSend_Click(System.Object sender, System.EventArgs e)
		{
			try {
				this.Visible = false;
				report.SendReport(Properties.Settings.Default.ErrorReportURL);
			} catch {
				// No way of reporting errors that have happened here, so just give up
			}

			this.Close();
		}

		private void lnkWhatData_LinkClicked(System.Object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Interaction.MsgBox(report.ToString());
		}

		private void cmdDontSend_Click(System.Object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void Error_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
		{
			System.Environment.Exit(0);
			// As there has been an error, call 'end' to blow away the rest of the app reasonably tidily
		}
		public ReportError()
		{
			FormClosing += Error_FormClosing;
			InitializeComponent();
		}
	}
}
