/*
 * Copyright © 2013-2019 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
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
                    this.Invoke((MethodInvoker)(() => { this.SetStatusText_FormThread(value); }));
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
                    this.Invoke((MethodInvoker)(() => { this.SetProgressBarMarquee_FormThread(value); }));
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
                    this.Invoke((MethodInvoker)(() => { this.SetProgressBarMax_FormThread(value); }));
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
                    this.Invoke((MethodInvoker)(() => { this.SetProgressBarValue_FormThread(value); }));
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
            if (!this.IsDisposed)
            {
                this.Invoke((MethodInvoker)(() => { base.Hide(); }));
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

            if (this.IsHandleCreated)
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

            if (this.IsHandleCreated)
            {
                this.tbarNotif.SetProgressValue(this, value, this.Progress.Maximum);
            }
        }

        private void Status_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Prevent closing under Windows via right-click taskbar menu
            // Mono sets e.CloseReason to UserClosing from a call to Hide()
            if (e.CloseReason == CloseReason.UserClosing && OsUtils.Windows())
            {
                e.Cancel = true;
            }
        }

        private void Status_Load(object sender, EventArgs e)
        {
            this.Font = SystemFonts.MessageBoxFont;
            this.tbarNotif = new TaskbarNotify();
        }

        private void Status_Shown(object sender, EventArgs e)
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
