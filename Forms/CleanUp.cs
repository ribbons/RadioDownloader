/*
 * Copyright © 2008-2015 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    internal partial class CleanUp : Form
    {
        public CleanUp()
        {
            this.InitializeComponent();
        }

        private void CleanUp_Load(object sender, EventArgs e)
        {
            this.Font = SystemFonts.MessageBoxFont;
            this.LabelRemove.Font = new Font(this.Font.FontFamily, (int)(this.Font.SizeInPoints * 1.34), this.Font.Style, GraphicsUnit.Point);
            this.LabelAdditional.Font = this.LabelRemove.Font;

            this.DateOlderThan.Value = DateTime.Now.AddMonths(-6);
            this.DateOlderThan.MaxDate = DateTime.Now;

            List<Model.Programme> downloadProgrammes = Model.Programme.FetchAllWithDownloads();

            if (downloadProgrammes.Count > 0)
            {
                foreach (Model.Programme prog in downloadProgrammes)
                {
                    this.ListProgrammes.Items.Add(new ComboNameValue<int>(prog.Name, prog.Progid));
                }

                this.ListProgrammes.SelectedIndex = 0;
            }
        }

        private void CleanUp_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.ShowHelp();
            e.Cancel = true;
        }

        private void CleanUp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                e.Handled = true;
                this.ShowHelp();
            }
        }

        private void ShowHelp()
        {
            OsUtils.LaunchUrl(new Uri("https://nerdoftheherd.com/tools/radiodld/help/dialogs.clean-up-downloads/"), "Context Help");
        }

        private void SetOkEnabled()
        {
            this.ButtonOk.Enabled = this.CheckByDate.Checked || this.CheckByProgramme.Checked ||
                                    this.CheckOrphan.Checked || this.CheckPlayed.Checked;
        }

        private void CheckOrphan_CheckedChanged(object sender, EventArgs e)
        {
            this.CheckKeepFiles.Enabled = !this.CheckOrphan.Checked;
            this.SetOkEnabled();
        }

        private void CheckKeepFiles_CheckedChanged(object sender, EventArgs e)
        {
            this.CheckOrphan.Enabled = !this.CheckKeepFiles.Checked;
            this.SetOkEnabled();
        }

        private void CheckByProgramme_CheckedChanged(object sender, EventArgs e)
        {
            this.ListProgrammes.Enabled = this.CheckByProgramme.Checked;
            this.SetOkEnabled();
        }

        private void CheckByDate_CheckedChanged(object sender, EventArgs e)
        {
            this.DateOlderThan.Enabled = this.CheckByDate.Checked;
            this.SetOkEnabled();
        }

        private void CheckPlayed_CheckedChanged(object sender, EventArgs e)
        {
            this.SetOkEnabled();
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            DateTime? olderThan = null;
            int? progid = null;

            if (this.CheckByDate.Checked)
            {
                // Just pass the date part and discard the time
                olderThan = new DateTime(this.DateOlderThan.Value.Year, this.DateOlderThan.Value.Month, this.DateOlderThan.Value.Day);
            }

            if (this.CheckByProgramme.Checked)
            {
                progid = ((ComboNameValue<int>)this.ListProgrammes.SelectedItem).Value;
            }

            using (Status status = new Status())
            {
                status.ShowDialog(
                    this,
                    () =>
                    {
                        Model.Download.Cleanup(status, olderThan, progid, this.CheckOrphan.Checked, this.CheckPlayed.Checked, this.CheckKeepFiles.Checked);
                    });
            }
        }
    }
}
