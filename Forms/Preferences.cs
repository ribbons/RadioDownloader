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
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;
    using Microsoft.VisualBasic;

    internal partial class Preferences : Form
    {
        private bool cancelClose;
        private bool folderChanged;

        public Preferences()
        {
            this.InitializeComponent();
        }

        private void ButtonChangeFolder_Click(object eventSender, EventArgs eventArgs)
        {
            FolderBrowserDialog browse = new FolderBrowserDialog();
            browse.SelectedPath = this.TextSaveIn.Text;
            browse.Description = "Choose the folder to save downloaded programmes in:";

            if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.TextSaveIn.Text = browse.SelectedPath;
                this.folderChanged = true;
            }
        }

        private void ButtonOk_Click(object eventSender, EventArgs eventArgs)
        {
            if (string.IsNullOrEmpty(this.TextFileNameFormat.Text))
            {
                Interaction.MsgBox("Please enter a value for the downloaded programme file name format.", MsgBoxStyle.Exclamation);
                this.TextFileNameFormat.Focus();
                this.cancelClose = true;
                return;
            }

            bool formatChanged = Settings.FileNameFormat != this.TextFileNameFormat.Text;

            if (this.folderChanged || formatChanged)
            {
                string message = "Move existing downloads to \"" + this.TextSaveIn.Text + "\" and rename to new naming format?";

                if (!formatChanged)
                {
                    message = "Move existing downloads to \"" + this.TextSaveIn.Text + "\"?";
                }
                else if (!this.folderChanged)
                {
                    message = "Rename existing downloads to new naming format?";
                }

                if (MessageBox.Show(message, Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (Status status = new Status())
                    {
                        status.ShowDialog(
                            this,
                            delegate
                            {
                                Model.Download.UpdatePaths(status, this.TextSaveIn.Text, this.TextFileNameFormat.Text);
                            });
                    }
                }

                Settings.SaveFolder = this.TextSaveIn.Text;
                Settings.FileNameFormat = this.TextFileNameFormat.Text;
            }

            Settings.RunOnStartup = this.CheckRunOnStartup.Checked;
            Settings.RunAfterCommand = this.TextRunAfter.Text;
            Settings.ParallelDownloads = (int)this.NumberParallel.Value;

            if (OsUtils.WinSevenOrLater())
            {
                Settings.CloseToSystray = this.CheckCloseToSystray.Checked;
            }

            OsUtils.ApplyRunOnStartup();
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

        private void Preferences_Load(object eventSender, EventArgs eventArgs)
        {
            this.Font = SystemFonts.MessageBoxFont;

            this.CheckRunOnStartup.Checked = Settings.RunOnStartup;

            if (OsUtils.WinSevenOrLater())
            {
                this.CheckCloseToSystray.Checked = Settings.CloseToSystray;
            }
            else
            {
                this.CheckCloseToSystray.Checked = true;
                this.CheckCloseToSystray.Enabled = false;
            }

            this.NumberParallel.Value = Settings.ParallelDownloads;
            this.NumberParallel.Maximum = Math.Max(this.NumberParallel.Value, Environment.ProcessorCount * 2);

            try
            {
                this.TextSaveIn.Text = FileUtils.GetSaveFolder();
            }
            catch (DirectoryNotFoundException)
            {
                this.TextSaveIn.Text = Settings.SaveFolder;
            }

            this.TextFileNameFormat.Text = Settings.FileNameFormat;
            this.TextRunAfter.Text = Settings.RunAfterCommand;
        }

        private void Preferences_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.ShowHelp();
            e.Cancel = true;
        }

        private void Preferences_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                e.Handled = true;
                this.ShowHelp();
            }
        }

        private void ShowHelp()
        {
            OsUtils.LaunchUrl(new Uri("http://www.nerdoftheherd.com/tools/radiodld/help/dialogs.options/"), "Context Help");
        }

        private void TextFileNameFormat_TextChanged(object sender, EventArgs e)
        {
            Model.Programme dummyProg = new Model.Programme();
            dummyProg.Name = "Programme Name";

            Model.Episode dummyEp = new Model.Episode();
            dummyEp.Name = "Episode Name";
            dummyEp.EpisodeDate = DateTime.Now;

            this.LabelFilenameFormatResult.Text = "Result: " + Model.Download.CreateSaveFileName(this.TextFileNameFormat.Text, dummyProg, dummyEp) + ".mp3";
        }

        private void ButtonReset_Click(object sender, EventArgs e)
        {
            if (Interaction.MsgBox("Are you sure that you would like to reset all of your settings?", MsgBoxStyle.YesNo | MsgBoxStyle.Question) == MsgBoxResult.Yes)
            {
                Settings.ResetUserSettings();
                OsUtils.ApplyRunOnStartup();

                this.Close();
            }
        }
    }
}
