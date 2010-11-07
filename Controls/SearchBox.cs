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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace RadioDld
{
    public class SearchBox : Control
    {
        [DllImport("uxtheme.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

        // Window Messages
        private const int EM_SETCUEBANNER = 0x1501;

        // Search box theme class, part and states
        private const string SEARCHBOX = "SearchBox";

        private const int SBBACKGROUND = 0x1;
        private const int SBB_NORMAL = 0x1;
        private const int SBB_HOT = 0x2;
        private const int SBB_DISABLED = 0x3;
        private const int SBB_FOCUSED = 0x4;

        private int boxState = SBB_NORMAL;
        private int themeHeight;

        private bool buttonHover;

        private string _cueBanner;

        private TextBox textBox;
        private PictureBox button;

        public SearchBox()
            : base()
        {
            TextChanged += this.SearchBox_TextChanged;
            Resize += this.SearchBox_Resize;
            Paint += this.SearchBox_Paint;
            HandleCreated += this.SearchBox_HandleCreated;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            // Create the child textbox control for the user to type in, without a border
            this.textBox = new TextBox();
            this.themeHeight = this.textBox.Height + 2;
            this.textBox.BorderStyle = BorderStyle.None;
            this.Controls.Add(this.textBox);

            this.textBox.MouseEnter += this.textBox_MouseEnter;
            this.textBox.MouseLeave += this.textBox_MouseLeave;
            this.textBox.GotFocus += this.textBox_GotFocus;
            this.textBox.LostFocus += this.textBox_LostFocus;
            this.textBox.TextChanged += this.textBox_TextChanged;
            this.textBox.KeyDown += this.textBox_KeyDown;

            // Create a picturebox to display the search icon and cancel 'button'
            this.button = new PictureBox();
            this.button.BackColor = Color.Transparent;
            this.button.Image = Properties.Resources.search_icon;
            this.button.SizeMode = PictureBoxSizeMode.AutoSize;
            this.Controls.Add(this.button);

            this.button.MouseEnter += this.button_MouseEnter;
            this.button.MouseLeave += this.button_MouseLeave;
            this.button.MouseDown += this.button_MouseDown;
            this.button.MouseUp += this.button_MouseUp;
            this.button.MouseClick += this.button_MouseClick;

            // Work out the height that the search box should be displayed
            if (OsUtils.WinVistaOrLater() && VisualStyleRenderer.IsSupported)
            {
                VisualStyleRenderer sizeStyle = new VisualStyleRenderer(SEARCHBOX, SBBACKGROUND, SBB_NORMAL);

                using (Graphics sizeGraphics = this.CreateGraphics())
                {
                    this.themeHeight = sizeStyle.GetPartSize(sizeGraphics, ThemeSizeType.True).Height;
                }
            }
        }

        public string CueBanner
        {
            get
            {
                return this._cueBanner;
            }

            set
            {
                this._cueBanner = value;
                SendMessage(this.textBox.Handle, EM_SETCUEBANNER, IntPtr.Zero, this._cueBanner);
            }
        }

        private void SearchBox_HandleCreated(object sender, System.EventArgs e)
        {
            if (OsUtils.WinXpOrLater())
            {
                // Set the theme of this parent control and the edit control, so they are rendered correctly
                if (SetWindowTheme(this.Handle, "SearchBoxComposited", null) != 0)
                {
                    throw new Win32Exception();
                }

                if (SetWindowTheme(this.textBox.Handle, "SearchBoxEditComposited", null) != 0)
                {
                    throw new Win32Exception();
                }
            }
        }

        private void SearchBox_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (VisualStyleRenderer.IsSupported)
            {
                VisualStyleRenderer searchBoxStyle = null;

                if (OsUtils.WinVistaOrLater())
                {
                    // Fetch the correct style based on the current state
                    searchBoxStyle = new VisualStyleRenderer(SEARCHBOX, SBBACKGROUND, this.boxState);
                }
                else
                {
                    searchBoxStyle = new VisualStyleRenderer(VisualStyleElement.TextBox.TextEdit.Normal);
                }

                // Paint the visual style background for the control
                searchBoxStyle.DrawBackground(e.Graphics, new Rectangle(0, 0, this.Width, this.Height));
            }
            else
            {
                e.Graphics.Clear(SystemColors.Window);

                // Paint a 'classic textbox' border for the control
                using (Pen controlDark = new Pen(SystemColors.ControlDark))
                {
                    using (Pen controlDarkDark = new Pen(SystemColors.ControlDarkDark))
                    {
                        using (Pen controlLightLight = new Pen(SystemColors.ControlLightLight))
                        {
                            using (Pen controlLight = new Pen(SystemColors.ControlLight))
                            {
                                e.Graphics.DrawLine(controlDark, 0, this.Height, 0, 0);
                                e.Graphics.DrawLine(controlDark, 0, 0, this.Width, 0);
                                e.Graphics.DrawLine(controlDarkDark, 1, this.Height - 1, 1, 1);
                                e.Graphics.DrawLine(controlDarkDark, 1, 1, this.Width - 1, 1);

                                e.Graphics.DrawLine(controlLight, this.Width - 2, 1, this.Width - 2, this.Height - 2);
                                e.Graphics.DrawLine(controlLight, this.Width - 2, this.Height - 2, 1, this.Height - 2);
                                e.Graphics.DrawLine(controlLightLight, this.Width - 1, 0, this.Width - 1, this.Height - 1);
                                e.Graphics.DrawLine(controlLightLight, this.Width - 1, this.Height - 1, 0, this.Height - 1);
                            }
                        }
                    }
                }
            }
        }

        private void textBox_MouseEnter(object sender, System.EventArgs e)
        {
            if (this.boxState == SBB_NORMAL)
            {
                this.boxState = SBB_HOT;
            }

            if (VisualStyleRenderer.IsSupported)
            {
                // Repaint the control and child textbox
                this.Invalidate();
                this.textBox.Invalidate();
            }
        }

        private void textBox_MouseLeave(object sender, System.EventArgs e)
        {
            if (this.boxState == SBB_HOT)
            {
                this.boxState = SBB_NORMAL;
            }

            if (VisualStyleRenderer.IsSupported)
            {
                // Repaint the control and child textbox
                this.Invalidate();
                this.textBox.Invalidate();
            }
        }

        private void textBox_GotFocus(object sender, System.EventArgs e)
        {
            this.boxState = SBB_FOCUSED;
            this.Invalidate(); // Repaint the control
        }

        private void textBox_LostFocus(object sender, System.EventArgs e)
        {
            this.boxState = SBB_NORMAL;
            this.Invalidate(); // Repaint the control
        }

        private void button_MouseEnter(object sender, System.EventArgs e)
        {
            this.buttonHover = true;

            if (!string.IsNullOrEmpty(this.Text))
            {
                this.button.Image = Properties.Resources.search_close_hover;
            }

            this.textBox_MouseEnter(sender, e);
        }

        private void button_MouseLeave(object sender, System.EventArgs e)
        {
            this.buttonHover = false;

            if (!string.IsNullOrEmpty(this.Text))
            {
                this.button.Image = Properties.Resources.search_close;
            }

            this.textBox_MouseLeave(sender, e);
        }

        private void button_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Text))
            {
                this.button.Image = Properties.Resources.search_close_pressed;
            }
        }

        private void button_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Text))
            {
                this.button.Image = Properties.Resources.search_close_hover;
            }
        }

        private void button_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.textBox.Text))
            {
                this.textBox.Text = string.Empty;
            }
        }

        private void SearchBox_Resize(object sender, System.EventArgs e)
        {
            if (this.Height != this.themeHeight)
            {
                // Force the height to always be set to that specified from the theme
                this.Height = this.themeHeight;
            }

            // Vertically center the search / cancel button on the right hand side
            this.button.Left = this.Width - (this.button.Width + 6);
            this.button.Top = Convert.ToInt32((this.Height - this.button.Height) / 2) + 1;

            // Use the rest of the space for the textbox
            this.textBox.Top = 4;
            this.textBox.Width = this.button.Left - (this.textBox.Left + 4);

            if (OsUtils.WinVistaOrLater() & VisualStyleRenderer.IsSupported)
            {
                // The textbox is given extra padding as part of the visual style
                this.textBox.Left = 2;
            }
            else
            {
                this.textBox.Left = 6;
            }
        }

        private void SearchBox_TextChanged(object sender, System.EventArgs e)
        {
            if (this.textBox.Text != this.Text)
            {
                this.textBox.Text = this.Text;
            }
        }

        private void textBox_TextChanged(object sender, System.EventArgs e)
        {
            // Hook up changes to the child textbox through this control
            this.Text = this.textBox.Text;

            // Update the displayed icon
            if (string.IsNullOrEmpty(this.Text))
            {
                this.button.Image = Properties.Resources.search_icon;
            }
            else
            {
                if (this.buttonHover)
                {
                    this.button.Image = Properties.Resources.search_close_hover;
                }
                else
                {
                    this.button.Image = Properties.Resources.search_close;
                }
            }
        }

        private void textBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.textBox.Text = string.Empty;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    this.textBox.Dispose();
                    this.button.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
