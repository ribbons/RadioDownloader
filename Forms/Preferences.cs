/*
 * Copyright © 2007-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Drawing;
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

            if (browse.ShowDialog() == DialogResult.OK)
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
                            () =>
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
            Settings.RssServer = this.CheckRssServer.Checked;

            if (this.CheckRssServer.Checked)
            {
                Settings.RssServerPort = (int)this.NumberServerPort.Value;
                Settings.RssServerNumRecentEps = (int)this.NumberEpisodes.Value;
            }

            if (new TaskbarNotify().Supported)
            {
                Settings.CloseToSystray = this.CheckCloseToSystray.Checked;
            }

            OsUtils.ApplyRunOnStartup();
        }

        private void Preferences_FormClosing(object sender, FormClosingEventArgs e)
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

            if (new TaskbarNotify().Supported)
            {
                this.CheckCloseToSystray.Checked = Settings.CloseToSystray;
            }
            else
            {
                this.CheckCloseToSystray.Checked = true;
                this.CheckCloseToSystray.Enabled = false;
            }

            this.CheckRssServer.Checked = Settings.RssServer;
            this.NumberServerPort.Enabled = this.CheckRssServer.Checked;
            this.NumberServerPort.Value = Settings.RssServerPort;
            this.NumberEpisodes.Enabled = this.CheckRssServer.Checked;
            this.NumberEpisodes.Value = Settings.RssServerNumRecentEps;
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
            OsUtils.LaunchUrl(new Uri("https://nerdoftheherd.com/tools/radiodld/help/dialogs.options/"), "Context Help");
        }

        private void TextFileNameFormat_TextChanged(object sender, EventArgs e)
        {
            Model.Programme dummyProg = new Model.Programme();
            dummyProg.Name = "Programme Name";

            Model.Episode dummyEp = new Model.Episode();
            dummyEp.Name = "Episode Name";
            dummyEp.Date = DateTime.Now;

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

        private void CheckRssServer_CheckedChanged(object sender, EventArgs e)
        {
            this.NumberServerPort.Enabled = this.CheckRssServer.Checked;
            this.NumberEpisodes.Enabled = this.CheckRssServer.Checked;
        }
    }
}
