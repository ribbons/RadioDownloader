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
using System.Globalization;
namespace RadioDld
{

    internal partial class Preferences : Form
	{

		private bool cancelClose;

        public Preferences()
        {
            InitializeComponent();
        }

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

			Properties.Settings.Default.RunOnStartup = uxRunOnStartup.Checked;
			Properties.Settings.Default.SaveFolder = txtSaveIn.Text;
			Properties.Settings.Default.FileNameFormat = txtFileNameFormat.Text;
			Properties.Settings.Default.RunAfterCommand = txtRunAfter.Text;

			if (OsUtils.WinSevenOrLater()) {
				Properties.Settings.Default.CloseToSystray = uxCloseToSystray.Checked;
			}

			OsUtils.ApplyRunOnStartup();
			Properties.Settings.Default.Save();
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

			uxRunOnStartup.Checked = Properties.Settings.Default.RunOnStartup;

			if (OsUtils.WinSevenOrLater()) {
				uxCloseToSystray.Checked = Properties.Settings.Default.CloseToSystray;
			} else {
				uxCloseToSystray.Checked = true;
				uxCloseToSystray.Enabled = false;
			}

			try {
				txtSaveIn.Text = FileUtils.GetSaveFolder();
			} catch (DirectoryNotFoundException) {
				txtSaveIn.Text = Properties.Settings.Default.SaveFolder;
			}

			txtFileNameFormat.Text = Properties.Settings.Default.FileNameFormat;
			txtRunAfter.Text = Properties.Settings.Default.RunAfterCommand;
		}

		private void txtFileNameFormat_TextChanged(System.Object sender, System.EventArgs e)
		{
			lblFilenameFormatResult.Text = "Result: " + FileUtils.CreateSaveFileName(txtFileNameFormat.Text, "Programme Name", "Episode Name", DateAndTime.Now) + ".mp3";
		}

		private void cmdReset_Click(System.Object sender, System.EventArgs e)
		{
			if (Interaction.MsgBox("Are you sure that you would like to reset all of your settings?", MsgBoxStyle.YesNo | MsgBoxStyle.Question) == MsgBoxResult.Yes) {
                Properties.Settings.Default.RunOnStartup = Convert.ToBoolean(Properties.Settings.Default.Properties["RunOnStartup"].DefaultValue, CultureInfo.InvariantCulture);
                Properties.Settings.Default.CloseToSystray = Convert.ToBoolean(Properties.Settings.Default.Properties["CloseToSystray"].DefaultValue, CultureInfo.InvariantCulture);
				Properties.Settings.Default.SaveFolder = (string)Properties.Settings.Default.Properties["SaveFolder"].DefaultValue;
                Properties.Settings.Default.FileNameFormat = (string)Properties.Settings.Default.Properties["FileNameFormat"].DefaultValue;
                Properties.Settings.Default.RunAfterCommand = (string)Properties.Settings.Default.Properties["RunAfterCommand"].DefaultValue;

				OsUtils.ApplyRunOnStartup();
				Properties.Settings.Default.Save();

				this.Close();
			}
		}
	}
}
