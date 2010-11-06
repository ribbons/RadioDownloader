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

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace RadioDld
{
    internal partial class Status : Form
    {
        private Thread showThread;
        private TaskbarNotify tbarNotif;

        public Status()
        {
            InitializeComponent();
        }

        public new void Show()
        {
            showThread = new Thread(ShowFormThread);
            showThread.Start();
        }

        private void ShowFormThread()
        {
            if (OsUtils.WinSevenOrLater())
            {
                tbarNotif = new TaskbarNotify();
            }

            base.ShowDialog();
        }

        public string StatusText
        {
            get
            {
                return lblStatus.Text;
            }

            set
            {
                if (this.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate { SetStatusText_FormThread(value); });
                }
                else
                {
                    SetStatusText_FormThread(value);
                }
            }
        }

        private void SetStatusText_FormThread(string text)
        {
            lblStatus.Text = text;
        }

        public bool ProgressBarMarquee
        {
            get
            {
                return prgProgress.Style == ProgressBarStyle.Marquee;
            }

            set
            {
                if (this.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate { SetProgressBarMarquee_FormThread(value); });
                }
                else
                {
                    SetProgressBarMarquee_FormThread(value);
                }
            }
        }

        private void SetProgressBarMarquee_FormThread(bool marquee)
        {
            prgProgress.Style = marquee ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;

            if (OsUtils.WinSevenOrLater() & this.IsHandleCreated)
            {
                if (marquee)
                {
                    tbarNotif.SetProgressMarquee(this);
                }
                else
                {
                    tbarNotif.SetProgressNone(this);
                }
            }
        }

        public int ProgressBarMax
        {
            get
            {
                return prgProgress.Maximum;
            }

            set
            {
                if (this.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate { SetProgressBarMax_FormThread(value); });
                }
                else
                {
                    SetProgressBarMax_FormThread(value);
                }
            }
        }

        private void SetProgressBarMax_FormThread(int value)
        {
            prgProgress.Maximum = value;
        }

        public int ProgressBarValue
        {
            get
            {
                return prgProgress.Value;
            }

            set
            {
                if (this.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate { SetProgressBarValue_FormThread(value); });
                }
                else
                {
                    SetProgressBarValue_FormThread(value);
                }
            }
        }

        private void SetProgressBarValue_FormThread(int value)
        {
            prgProgress.Value = value;

            if (OsUtils.WinSevenOrLater() & this.IsHandleCreated)
            {
                tbarNotif.SetProgressValue(this, value, prgProgress.Maximum);
            }
        }

        public new void Hide()
        {
            if (this.IsHandleCreated)
            {
                this.Invoke((MethodInvoker)delegate { HideForm_FormThread(); });
            }
            else
            {
                HideForm_FormThread();
            }
        }

        private void HideForm_FormThread()
        {
            base.Hide();
        }

        private void Status_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void Status_Load(object sender, System.EventArgs e)
        {
            this.Font = SystemFonts.MessageBoxFont;
        }

        private void Status_Shown(object sender, System.EventArgs e)
        {
            if (OsUtils.WinSevenOrLater())
            {
                if (prgProgress.Style == ProgressBarStyle.Marquee)
                {
                    tbarNotif.SetProgressMarquee(this);
                }
                else
                {
                    if (prgProgress.Value != 0)
                    {
                        tbarNotif.SetProgressValue(this, prgProgress.Value, prgProgress.Maximum);
                    }
                }
            }
        }
    }
}
