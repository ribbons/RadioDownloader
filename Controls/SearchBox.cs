/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2012 by the authors - see the AUTHORS file for details.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace RadioDld
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal class SearchBox : Control
    {
        private int boxState = NativeMethods.SBB_NORMAL;
        private int themeHeight;

        private bool buttonHover;

        private string cueBanner;

        private TextBox textBox;
        private PictureBox button;

        public SearchBox()
            : base()
        {
            this.TextChanged += this.SearchBox_TextChanged;
            this.Resize += this.SearchBox_Resize;
            this.Paint += this.SearchBox_Paint;
            this.HandleCreated += this.SearchBox_HandleCreated;

            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            // Create the child textbox control for the user to type in, without a border
            this.textBox = new TextBox();
            this.themeHeight = this.textBox.Height + 2;
            this.textBox.BorderStyle = BorderStyle.None;
            this.Controls.Add(this.textBox);

            this.textBox.MouseEnter += this.TextBox_MouseEnter;
            this.textBox.MouseLeave += this.TextBox_MouseLeave;
            this.textBox.GotFocus += this.TextBox_GotFocus;
            this.textBox.LostFocus += this.TextBox_LostFocus;
            this.textBox.TextChanged += this.TextBox_TextChanged;
            this.textBox.KeyDown += this.TextBox_KeyDown;

            // Create a picturebox to display the search icon and cancel 'button'
            this.button = new PictureBox();
            this.button.BackColor = Color.Transparent;
            this.button.Image = Properties.Resources.search_icon;
            this.button.SizeMode = PictureBoxSizeMode.AutoSize;
            this.Controls.Add(this.button);

            this.button.MouseEnter += this.Button_MouseEnter;
            this.button.MouseLeave += this.Button_MouseLeave;
            this.button.MouseDown += this.Button_MouseDown;
            this.button.MouseUp += this.Button_MouseUp;
            this.button.MouseClick += this.Button_MouseClick;

            // Work out the height that the search box should be displayed
            if (OsUtils.WinVistaOrLater() && VisualStyleRenderer.IsSupported)
            {
                VisualStyleRenderer sizeStyle = new VisualStyleRenderer(NativeMethods.SEARCHBOX, NativeMethods.SBBACKGROUND, NativeMethods.SBB_NORMAL);

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
                return this.cueBanner;
            }

            set
            {
                this.cueBanner = value;
                NativeMethods.SendMessage(this.textBox.Handle, NativeMethods.EM_SETCUEBANNER, IntPtr.Zero, this.cueBanner);
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

        private void SearchBox_HandleCreated(object sender, EventArgs e)
        {
            if (OsUtils.WinVistaOrLater())
            {
                // Set the theme of this parent control and the edit control, so they are rendered correctly
                Marshal.ThrowExceptionForHR(NativeMethods.SetWindowTheme(this.Handle, "SearchBoxComposited", null));
                Marshal.ThrowExceptionForHR(NativeMethods.SetWindowTheme(this.textBox.Handle, "SearchBoxEditComposited", null));
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
                    searchBoxStyle = new VisualStyleRenderer(NativeMethods.SEARCHBOX, NativeMethods.SBBACKGROUND, this.boxState);
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
                // It would be simpler to use e.Graphics.Clear(SystemColors.Window), but that crashes
                // sometimes when running under terminal services due to a bug in GDI+
                using (Brush windowBrush = new SolidBrush(SystemColors.Window))
                {
                    e.Graphics.FillRectangle(windowBrush, 0, 0, this.Width - 1, this.Height - 1);
                }

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

        private void TextBox_MouseEnter(object sender, EventArgs e)
        {
            if (this.boxState == NativeMethods.SBB_NORMAL)
            {
                this.boxState = NativeMethods.SBB_HOT;
            }

            if (VisualStyleRenderer.IsSupported)
            {
                // Repaint the control and child textbox
                this.Invalidate();
                this.textBox.Invalidate();
            }
        }

        private void TextBox_MouseLeave(object sender, EventArgs e)
        {
            if (this.boxState == NativeMethods.SBB_HOT)
            {
                this.boxState = NativeMethods.SBB_NORMAL;
            }

            if (VisualStyleRenderer.IsSupported)
            {
                // Repaint the control and child textbox
                this.Invalidate();
                this.textBox.Invalidate();
            }
        }

        private void TextBox_GotFocus(object sender, EventArgs e)
        {
            this.boxState = NativeMethods.SBB_FOCUSED;
            this.Invalidate(); // Repaint the control
        }

        private void TextBox_LostFocus(object sender, EventArgs e)
        {
            this.boxState = NativeMethods.SBB_NORMAL;
            this.Invalidate(); // Repaint the control
        }

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            this.buttonHover = true;

            if (!string.IsNullOrEmpty(this.Text))
            {
                this.button.Image = Properties.Resources.search_close_hover;
            }

            this.TextBox_MouseEnter(sender, e);
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            this.buttonHover = false;

            if (!string.IsNullOrEmpty(this.Text))
            {
                this.button.Image = Properties.Resources.search_close;
            }

            this.TextBox_MouseLeave(sender, e);
        }

        private void Button_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Text))
            {
                this.button.Image = Properties.Resources.search_close_pressed;
            }
        }

        private void Button_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Text))
            {
                this.button.Image = Properties.Resources.search_close_hover;
            }
        }

        private void Button_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.textBox.Text))
            {
                this.textBox.Text = string.Empty;
            }
        }

        private void SearchBox_Resize(object sender, EventArgs e)
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

            if (OsUtils.WinVistaOrLater() && VisualStyleRenderer.IsSupported)
            {
                // The textbox is given extra padding as part of the visual style
                this.textBox.Left = 2;
            }
            else
            {
                this.textBox.Left = 6;
            }
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            if (this.textBox.Text != this.Text)
            {
                this.textBox.Text = this.Text;
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
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

        private void TextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.textBox.Text = string.Empty;
            }
        }
    }
}
