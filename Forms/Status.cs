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
    using System.Threading;
    using System.Windows.Forms;

    internal partial class Status : Form
    {
        private MethodInvoker work;
        private Thread workThread;
        private TaskbarNotify tbarNotif;

        public Status()
        {
            this.InitializeComponent();
        }

        public string StatusText
        {
            get
            {
                return this.LabelStatus.Text;
            }

            set
            {
                if (this.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate { this.SetStatusText_FormThread(value); });
                }
                else
                {
                    this.SetStatusText_FormThread(value);
                }
            }
        }

        public bool ProgressBarMarquee
        {
            get
            {
                return this.Progress.Style == ProgressBarStyle.Marquee;
            }

            set
            {
                if (this.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate { this.SetProgressBarMarquee_FormThread(value); });
                }
                else
                {
                    this.SetProgressBarMarquee_FormThread(value);
                }
            }
        }

        public int ProgressBarMax
        {
            get
            {
                return this.Progress.Maximum;
            }

            set
            {
                if (this.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate { this.SetProgressBarMax_FormThread(value); });
                }
                else
                {
                    this.SetProgressBarMax_FormThread(value);
                }
            }
        }

        public int ProgressBarValue
        {
            get
            {
                return this.Progress.Value;
            }

            set
            {
                if (this.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate { this.SetProgressBarValue_FormThread(value); });
                }
                else
                {
                    this.SetProgressBarValue_FormThread(value);
                }
            }
        }

        public DialogResult ShowDialog(MethodInvoker work)
        {
            this.work = work;
            return this.ShowDialog();
        }

        public DialogResult ShowDialog(Form parent, MethodInvoker work)
        {
            this.work = work;
            return this.ShowDialog(parent);
        }

        private new void Hide()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { this.Hide(); });
                return;
            }

            base.Hide();
        }

        private void SetStatusText_FormThread(string text)
        {
            this.LabelStatus.Text = text;
        }

        private void SetProgressBarMarquee_FormThread(bool marquee)
        {
            this.Progress.Style = marquee ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;

            if (OsUtils.WinSevenOrLater() && this.IsHandleCreated)
            {
                if (marquee)
                {
                    this.tbarNotif.SetProgressMarquee(this);
                }
                else
                {
                    this.tbarNotif.SetProgressNone(this);
                }
            }
        }

        private void SetProgressBarMax_FormThread(int value)
        {
            this.Progress.Maximum = value;
        }

        private void SetProgressBarValue_FormThread(int value)
        {
            this.Progress.Value = value;

            if (OsUtils.WinSevenOrLater() && this.IsHandleCreated)
            {
                this.tbarNotif.SetProgressValue(this, value, this.Progress.Maximum);
            }
        }

        private void Status_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }

        private void Status_Load(object sender, System.EventArgs e)
        {
            this.Font = SystemFonts.MessageBoxFont;

            if (OsUtils.WinSevenOrLater())
            {
                this.tbarNotif = new TaskbarNotify();
            }
        }

        private void Status_Shown(object sender, System.EventArgs e)
        {
            if (OsUtils.WinSevenOrLater())
            {
                if (this.Progress.Style == ProgressBarStyle.Marquee)
                {
                    this.tbarNotif.SetProgressMarquee(this);
                }
                else
                {
                    if (this.Progress.Value != 0)
                    {
                        this.tbarNotif.SetProgressValue(this, this.Progress.Value, this.Progress.Maximum);
                    }
                }
            }

            this.workThread = new Thread(this.WorkThread);
            this.workThread.Start();
        }

        private void WorkThread()
        {
            this.work();
            this.Hide();
        }
    }
}
