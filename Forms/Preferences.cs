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


using System.IO;
namespace RadioDld
{

	internal partial class Preferences : System.Windows.Forms.Form
	{

		private bool cancelClose;

		private void cmdChangeFolder_Click(System.Object eventSender, System.EventArgs eventArgs)
		{
			FolderBrowserDialog BrowseDialog = new FolderBrowserDialog();
			BrowseDialog.SelectedPath = txtSaveIn.Text;
			BrowseDialog.Description = "Choose the folder to save downloaded programmes in:";

			if (BrowseDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				txtSaveIn.Text = BrowseDialog.SelectedPath;
			}
		}

		private void cmdOK_Click(System.Object eventSender, System.EventArgs eventArgs)
		{
			if (txtFileNameFormat.Text == string.Empty) {
				Interaction.MsgBox("Please enter a value for the downloaded programme file name format.", MsgBoxStyle.Exclamation);
				txtFileNameFormat.Focus();
				cancelClose = true;
				return;
			}

			RadioDld.My.Settings.RunOnStartup = uxRunOnStartup.Checked;
			RadioDld.My.Settings.SaveFolder = txtSaveIn.Text;
			RadioDld.My.Settings.FileNameFormat = txtFileNameFormat.Text;
			RadioDld.My.Settings.RunAfterCommand = txtRunAfter.Text;

			if (OsUtils.WinSevenOrLater()) {
				RadioDld.My.Settings.CloseToSystray = uxCloseToSystray.Checked;
			}

			OsUtils.ApplyRunOnStartup();
			RadioDld.My.Settings.Save();
		}

		private void Preferences_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
		{
			if (cancelClose) {
				// Prevent the form from being automatically closed if it failed validation
				e.Cancel = true;
				cancelClose = false;
			}
		}

		private void Preferences_Load(System.Object eventSender, System.EventArgs eventArgs)
		{
			this.Font = SystemFonts.MessageBoxFont;

			uxRunOnStartup.Checked = RadioDld.My.Settings.RunOnStartup;

			if (OsUtils.WinSevenOrLater()) {
				uxCloseToSystray.Checked = RadioDld.My.Settings.CloseToSystray;
			} else {
				uxCloseToSystray.Checked = true;
				uxCloseToSystray.Enabled = false;
			}

			try {
				txtSaveIn.Text = FileUtils.GetSaveFolder();
			} catch (DirectoryNotFoundException dirNotFoundExp) {
				txtSaveIn.Text = RadioDld.My.Settings.SaveFolder;
			}

			txtFileNameFormat.Text = RadioDld.My.Settings.FileNameFormat;
			txtRunAfter.Text = RadioDld.My.Settings.RunAfterCommand;
		}

		private void txtFileNameFormat_TextChanged(System.Object sender, System.EventArgs e)
		{
			lblFilenameFormatResult.Text = "Result: " + FileUtils.CreateSaveFileName(txtFileNameFormat.Text, "Programme Name", "Episode Name", DateAndTime.Now) + ".mp3";
		}

		private void cmdReset_Click(System.Object sender, System.EventArgs e)
		{
			if (Interaction.MsgBox("Are you sure that you would like to reset all of your settings?", MsgBoxStyle.YesNo | MsgBoxStyle.Question) == MsgBoxResult.Yes) {
				RadioDld.My.Settings.RunOnStartup = Convert.ToBoolean(RadioDld.My.Settings.Properties.Item("RunOnStartup").DefaultValue);
				RadioDld.My.Settings.CloseToSystray = Convert.ToBoolean(RadioDld.My.Settings.Properties.Item("CloseToSystray").DefaultValue);
				RadioDld.My.Settings.SaveFolder = RadioDld.My.Settings.Properties.Item("SaveFolder").DefaultValue.ToString;
				RadioDld.My.Settings.FileNameFormat = RadioDld.My.Settings.Properties.Item("FileNameFormat").DefaultValue.ToString;
				RadioDld.My.Settings.RunAfterCommand = RadioDld.My.Settings.Properties.Item("RunAfterCommand").DefaultValue.ToString;

				OsUtils.ApplyRunOnStartup();
				RadioDld.My.Settings.Save();

				this.Close();
			}
		}
	}
}
