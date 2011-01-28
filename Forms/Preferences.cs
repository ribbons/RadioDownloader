/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2011 Matt Robinson
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
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;
    using Microsoft.VisualBasic;

    internal partial class Preferences : Form
    {
        private bool cancelClose;

        public Preferences()
        {
            this.InitializeComponent();
        }

        private void ButtonChangeFolder_Click(object eventSender, System.EventArgs eventArgs)
        {
            FolderBrowserDialog browse = new FolderBrowserDialog();
            browse.SelectedPath = this.TextSaveIn.Text;
            browse.Description = "Choose the folder to save downloaded programmes in:";

            if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.TextSaveIn.Text = browse.SelectedPath;
            }
        }

        private void ButtonOk_Click(object eventSender, System.EventArgs eventArgs)
        {
            if (string.IsNullOrEmpty(this.TextFileNameFormat.Text))
            {
                Interaction.MsgBox("Please enter a value for the downloaded programme file name format.", MsgBoxStyle.Exclamation);
                this.TextFileNameFormat.Focus();
                this.cancelClose = true;
                return;
            }

            Properties.Settings.Default.RunOnStartup = this.CheckRunOnStartup.Checked;
            Properties.Settings.Default.SaveFolder = this.TextSaveIn.Text;
            Properties.Settings.Default.FileNameFormat = this.TextFileNameFormat.Text;
            Properties.Settings.Default.RunAfterCommand = this.TextRunAfter.Text;

            if (OsUtils.WinSevenOrLater())
            {
                Properties.Settings.Default.CloseToSystray = this.CheckCloseToSystray.Checked;
            }

            OsUtils.ApplyRunOnStartup();
            Properties.Settings.Default.Save();
        }

        private void Preferences_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (this.cancelClose)
            {
                // Prevent the form from being automatically closed if it failed validation
                e.Cancel = true;
                this.cancelClose = false;
            }
        }

        private void Preferences_Load(object eventSender, System.EventArgs eventArgs)
        {
            this.Font = SystemFonts.MessageBoxFont;

            this.CheckRunOnStartup.Checked = Properties.Settings.Default.RunOnStartup;

            if (OsUtils.WinSevenOrLater())
            {
                this.CheckCloseToSystray.Checked = Properties.Settings.Default.CloseToSystray;
            }
            else
            {
                this.CheckCloseToSystray.Checked = true;
                this.CheckCloseToSystray.Enabled = false;
            }

            try
            {
                this.TextSaveIn.Text = FileUtils.GetSaveFolder();
            }
            catch (DirectoryNotFoundException)
            {
                this.TextSaveIn.Text = Properties.Settings.Default.SaveFolder;
            }

            this.TextFileNameFormat.Text = Properties.Settings.Default.FileNameFormat;
            this.TextRunAfter.Text = Properties.Settings.Default.RunAfterCommand;
        }

        private void TextFileNameFormat_TextChanged(object sender, System.EventArgs e)
        {
            this.LabelFilenameFormatResult.Text = "Result: " + FileUtils.CreateSaveFileName(this.TextFileNameFormat.Text, "Programme Name", "Episode Name", DateAndTime.Now) + ".mp3";
        }

        private void ButtonReset_Click(object sender, System.EventArgs e)
        {
            if (Interaction.MsgBox("Are you sure that you would like to reset all of your settings?", MsgBoxStyle.YesNo | MsgBoxStyle.Question) == MsgBoxResult.Yes)
            {
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
