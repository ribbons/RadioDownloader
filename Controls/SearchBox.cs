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

        private TextBox withEventsField_textBox;

        private TextBox textBox
        {
            get
            {
                return withEventsField_textBox;
            }

            set
            {
                if (withEventsField_textBox != null)
                {
                    withEventsField_textBox.MouseEnter -= textBox_MouseEnter;
                    withEventsField_textBox.MouseLeave -= textBox_MouseLeave;
                    withEventsField_textBox.GotFocus -= textBox_GotFocus;
                    withEventsField_textBox.LostFocus -= textBox_LostFocus;
                    withEventsField_textBox.TextChanged -= textBox_TextChanged;
                    withEventsField_textBox.KeyDown -= textBox_KeyDown;
                }

                withEventsField_textBox = value;
                if (withEventsField_textBox != null)
                {
                    withEventsField_textBox.MouseEnter += textBox_MouseEnter;
                    withEventsField_textBox.MouseLeave += textBox_MouseLeave;
                    withEventsField_textBox.GotFocus += textBox_GotFocus;
                    withEventsField_textBox.LostFocus += textBox_LostFocus;
                    withEventsField_textBox.TextChanged += textBox_TextChanged;
                    withEventsField_textBox.KeyDown += textBox_KeyDown;
                }
            }
        }

        private PictureBox withEventsField_button;

        private PictureBox button
        {
            get
            {
                return withEventsField_button;
            }

            set
            {
                if (withEventsField_button != null)
                {
                    withEventsField_button.MouseEnter -= button_MouseEnter;
                    withEventsField_button.MouseLeave -= button_MouseLeave;
                    withEventsField_button.MouseDown -= button_MouseDown;
                    withEventsField_button.MouseUp -= button_MouseUp;
                    withEventsField_button.MouseClick -= button_MouseClick;
                }

                withEventsField_button = value;
                if (withEventsField_button != null)
                {
                    withEventsField_button.MouseEnter += button_MouseEnter;
                    withEventsField_button.MouseLeave += button_MouseLeave;
                    withEventsField_button.MouseDown += button_MouseDown;
                    withEventsField_button.MouseUp += button_MouseUp;
                    withEventsField_button.MouseClick += button_MouseClick;
                }
            }
        }

        public SearchBox()
            : base()
        {
            TextChanged += SearchBox_TextChanged;
            Resize += SearchBox_Resize;
            Paint += SearchBox_Paint;
            HandleCreated += SearchBox_HandleCreated;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            // Create the child textbox control for the user to type in, without a border
            textBox = new TextBox();
            this.themeHeight = textBox.Height + 2;
            textBox.BorderStyle = BorderStyle.None;
            this.Controls.Add(textBox);

            // Create a picturebox to display the search icon and cancel 'button'
            button = new PictureBox();
            button.BackColor = Color.Transparent;
            button.Image = Properties.Resources.search_icon;
            button.SizeMode = PictureBoxSizeMode.AutoSize;
            this.Controls.Add(button);

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
                return _cueBanner;
            }

            set
            {
                _cueBanner = value;
                SendMessage(textBox.Handle, EM_SETCUEBANNER, IntPtr.Zero, _cueBanner);
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

                if (SetWindowTheme(textBox.Handle, "SearchBoxEditComposited", null) != 0)
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
                    searchBoxStyle = new VisualStyleRenderer(SEARCHBOX, SBBACKGROUND, boxState);
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
            if (boxState == SBB_NORMAL)
            {
                boxState = SBB_HOT;
            }

            if (VisualStyleRenderer.IsSupported)
            {
                // Repaint the control and child textbox
                this.Invalidate();
                textBox.Invalidate();
            }
        }

        private void textBox_MouseLeave(object sender, System.EventArgs e)
        {
            if (boxState == SBB_HOT)
            {
                boxState = SBB_NORMAL;
            }

            if (VisualStyleRenderer.IsSupported)
            {
                // Repaint the control and child textbox
                this.Invalidate();
                textBox.Invalidate();
            }
        }

        private void textBox_GotFocus(object sender, System.EventArgs e)
        {
            boxState = SBB_FOCUSED;
            this.Invalidate(); // Repaint the control
        }

        private void textBox_LostFocus(object sender, System.EventArgs e)
        {
            boxState = SBB_NORMAL;
            this.Invalidate(); // Repaint the control
        }

        private void button_MouseEnter(object sender, System.EventArgs e)
        {
            buttonHover = true;

            if (!string.IsNullOrEmpty(this.Text))
            {
                button.Image = Properties.Resources.search_close_hover;
            }

            textBox_MouseEnter(sender, e);
        }

        private void button_MouseLeave(object sender, System.EventArgs e)
        {
            buttonHover = false;

            if (!string.IsNullOrEmpty(this.Text))
            {
                button.Image = Properties.Resources.search_close;
            }

            textBox_MouseLeave(sender, e);
        }

        private void button_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Text))
            {
                button.Image = Properties.Resources.search_close_pressed;
            }
        }

        private void button_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Text))
            {
                button.Image = Properties.Resources.search_close_hover;
            }
        }

        private void button_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = string.Empty;
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
            button.Left = this.Width - (button.Width + 6);
            button.Top = Convert.ToInt32((this.Height - button.Height) / 2) + 1;

            // Use the rest of the space for the textbox
            textBox.Top = 4;
            textBox.Width = button.Left - (textBox.Left + 4);

            if (OsUtils.WinVistaOrLater() & VisualStyleRenderer.IsSupported)
            {
                // The textbox is given extra padding as part of the visual style
                textBox.Left = 2;
            }
            else
            {
                textBox.Left = 6;
            }
        }

        private void SearchBox_TextChanged(object sender, System.EventArgs e)
        {
            if (textBox.Text != this.Text)
            {
                textBox.Text = this.Text;
            }
        }

        private void textBox_TextChanged(object sender, System.EventArgs e)
        {
            // Hook up changes to the child textbox through this control
            this.Text = textBox.Text;

            // Update the displayed icon
            if (string.IsNullOrEmpty(this.Text))
            {
                button.Image = Properties.Resources.search_icon;
            }
            else
            {
                if (buttonHover)
                {
                    button.Image = Properties.Resources.search_close_hover;
                }
                else
                {
                    button.Image = Properties.Resources.search_close;
                }
            }
        }

        private void textBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                textBox.Text = string.Empty;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    textBox.Dispose();
                    button.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
