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
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal class TabBarRenderer : ToolStripSystemRenderer, IDisposable
    {
        private const int TabSeparation = 3;
        private const int CurveSize = 10;

        private ToolStrip rendererFor;

        private bool isDisposed;
        private bool isActive = true;

        private Brush inactiveTabBkg;
        private Brush hoverTabBkg;
        private Brush pressedTabBkg;

        private Pen tabBorder = new Pen(SystemColors.ControlDark);
        private Pen nonAeroBorder = new Pen(Color.FromArgb(255, 182, 193, 204));

        ~TabBarRenderer()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void Initialize(System.Windows.Forms.ToolStrip toolStrip)
        {
            base.Initialize(toolStrip);

            this.rendererFor = toolStrip;

            toolStrip.FindForm().Activated += this.Form_Activated;
            toolStrip.FindForm().Deactivate += this.Form_Deactivated;
        }

        protected override void OnRenderItemImage(System.Windows.Forms.ToolStripItemImageRenderEventArgs e)
        {
            if (e.Item.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText)
            {
                e.Graphics.DrawImage(e.Item.Image, e.ImageRectangle);
                return;
            }
            else if (e.Item.DisplayStyle != ToolStripItemDisplayStyle.Image)
            {
                base.OnRenderItemImage(e);
                return;
            }

            if (VisualStyleRenderer.IsSupported)
            {
                VisualStyleRenderer navigation = null;
                int stylePart = e.ToolStrip.Items.IndexOf(e.Item) + 1;

                try
                {
                    if (!e.Item.Enabled)
                    {
                        navigation = new VisualStyleRenderer("Navigation", stylePart, NativeMethods.NAV_BF_DISABLED);
                    }
                    else if (e.Item.Pressed)
                    {
                        navigation = new VisualStyleRenderer("Navigation", stylePart, NativeMethods.NAV_BF_PRESSED);
                    }
                    else if (e.Item.Selected)
                    {
                        navigation = new VisualStyleRenderer("Navigation", stylePart, NativeMethods.NAV_BF_HOT);
                    }
                    else
                    {
                        navigation = new VisualStyleRenderer("Navigation", stylePart, NativeMethods.NAV_BF_NORMAL);
                    }
                }
                catch (ArgumentException)
                {
                    // The element is not defined in the current theme
                }

                if (navigation != null)
                {
                    navigation.DrawBackground(e.Graphics, new Rectangle(0, -2, e.Item.Width, e.Item.Width));
                    return;
                }
            }

            base.OnRenderItemImage(e);
        }

        protected override void OnRenderToolStripBackground(System.Windows.Forms.ToolStripRenderEventArgs e)
        {
            if (OsUtils.CompositionEnabled())
            {
                // Set the background colour to transparent to make it glass
                e.Graphics.Clear(Color.Transparent);
            }
            else
            {
                if (VisualStyleRenderer.IsSupported && OsUtils.WinVistaOrLater())
                {
                    // Set the background the same as the title bar to give the illusion of an extended frame
                    if (this.isActive)
                    {
                        // It would be simpler to use e.Graphics.Clear(SystemColors.GradientActiveCaption), but
                        // that crashes sometimes when running under terminal services due to a bug in GDI+
                        using (Brush backgroundBrush = new SolidBrush(SystemColors.GradientActiveCaption))
                        {
                            e.Graphics.FillRectangle(backgroundBrush, e.AffectedBounds);
                        }
                    }
                    else
                    {
                        // It would be simpler to use e.Graphics.Clear(SystemColors.GradientInactiveCaption), but
                        // that crashes sometimes when running under terminal services due to a bug in GDI+
                        using (Brush backgroundBrush = new SolidBrush(SystemColors.GradientInactiveCaption))
                        {
                            e.Graphics.FillRectangle(backgroundBrush, e.AffectedBounds);
                        }
                    }
                }
                else
                {
                    base.OnRenderToolStripBackground(e);
                }
            }
        }

        protected override void OnRenderButtonBackground(System.Windows.Forms.ToolStripItemRenderEventArgs e)
        {
            if (e.Item.DisplayStyle == ToolStripItemDisplayStyle.Image)
            {
                // Do not paint a background for icon only buttons
                return;
            }

            if (this.inactiveTabBkg == null)
            {
                this.inactiveTabBkg = new LinearGradientBrush(new Point(0, 0), new Point(0, e.Item.Height), SystemColors.Control, SystemColors.ControlDark);
                this.hoverTabBkg = new LinearGradientBrush(new Point(0, 0), new Point(0, e.Item.Height), SystemColors.ControlLight, SystemColors.Control);
                this.pressedTabBkg = new SolidBrush(SystemColors.ControlLight);
            }

            using (LinearGradientBrush activeTabBkg = new LinearGradientBrush(new Point(0, 0), new Point(0, e.Item.Height), SystemColors.ControlLight, this.GetActiveTabBtmCol(e.ToolStrip, e.Item)))
            {
                ToolStripButton button = (ToolStripButton)e.Item;
                Brush colour = this.inactiveTabBkg;

                if (button.Checked)
                {
                    colour = activeTabBkg;

                    // Invalidate between the buttons and the bottom of the toolstrip so that it gets repainted
                    e.ToolStrip.Invalidate(new Rectangle(0, e.Item.Bounds.Bottom, e.ToolStrip.Bounds.Width, e.ToolStrip.Bounds.Height - e.Item.Bounds.Bottom));
                }
                else if (e.Item.Selected)
                {
                    if (e.Item.Pressed)
                    {
                        colour = this.pressedTabBkg;
                    }
                    else
                    {
                        colour = this.hoverTabBkg;
                    }
                }

                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                int width = e.Item.Width - TabSeparation;
                int height = e.Item.Height;

                using (GraphicsPath tab = new GraphicsPath())
                {
                    tab.AddLine(0, height, 0, CurveSize);
                    tab.AddArc(0, 0, CurveSize, CurveSize, 180, 90);
                    tab.AddLine(CurveSize, 0, width - CurveSize, 0);
                    tab.AddArc(width - CurveSize, 0, CurveSize, CurveSize, 270, 90);
                    tab.AddLine(width, CurveSize, width, height);

                    e.Graphics.FillPath(colour, tab);
                    e.Graphics.DrawPath(this.tabBorder, tab);
                }
            }
        }

        protected override void OnRenderItemText(System.Windows.Forms.ToolStripItemTextRenderEventArgs e)
        {
            if (!OsUtils.CompositionEnabled())
            {
                // The OS doesn't support desktop composition, or it isn't enabled
                base.OnRenderItemText(e);
                return;
            }

            // Drawing text on glass is a bit of a pain - text generated with GDI (e.g. standard
            // controls) ends up being transparent as GDI doesn't understand alpha transparency.
            // GDI+ is fine drawing text on glass but it doesn't use ClearType, so the text ends
            // up looking out of place, ugly or both.  The proper way is using DrawThemeTextEx,
            // which works fine, but requires a top-down DIB to draw to, rather than the bottom
            // up ones that GDI normally uses.  Hence; create top-down DIB, draw text to it and
            // then AlphaBlend it in to the graphics object that we are rendering to.

            // Get the rendering HDC, and create a compatible one for drawing the text to
            IntPtr renderHdc = e.Graphics.GetHdc();
            IntPtr memoryHdc = NativeMethods.CreateCompatibleDC(renderHdc);

            // NULL Pointer
            if (memoryHdc == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            NativeMethods.BITMAPINFO info = default(NativeMethods.BITMAPINFO);
            info.biSize = Convert.ToUInt32(Marshal.SizeOf(typeof(NativeMethods.BITMAPINFO)));
            info.biWidth = e.TextRectangle.Width;
            info.biHeight = -e.TextRectangle.Height; // Negative = top-down
            info.biPlanes = 1;
            info.biBitCount = 32;
            info.biCompression = NativeMethods.BI_RGB;

            IntPtr bits = IntPtr.Zero;

            // Create the top-down DIB
            IntPtr dib = NativeMethods.CreateDIBSection(renderHdc, ref info, 0, ref bits, IntPtr.Zero, 0);

            // NULL Pointer
            if (dib == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            // Select the created DIB into our memory DC for use
            // NULL Pointer
            if (NativeMethods.SelectObject(memoryHdc, dib) == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            // Create a font we can use with GetThemeTextEx
            IntPtr hFont = e.TextFont.ToHfont();

            // And select it into the DC as well
            // NULL Pointer
            if (NativeMethods.SelectObject(memoryHdc, hFont) == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            // Fetch a VisualStyleRenderer suitable for toolbar text
            VisualStyleRenderer renderer = new VisualStyleRenderer(VisualStyleElement.ToolBar.Button.Normal);

            // Set up a RECT for the area to draw the text in
            NativeMethods.RECT textRect = default(NativeMethods.RECT);
            textRect.left = 0;
            textRect.top = 0;
            textRect.right = e.TextRectangle.Width;
            textRect.bottom = e.TextRectangle.Height;

            // Options for GetThemeTextEx
            NativeMethods.DTTOPTS opts = default(NativeMethods.DTTOPTS);
            opts.dwSize = Convert.ToUInt32(Marshal.SizeOf(opts));
            opts.dwFlags = NativeMethods.DTT_COMPOSITED | NativeMethods.DTT_TEXTCOLOR;
            opts.crText = Convert.ToUInt32(ColorTranslator.ToWin32(e.TextColor)); // Alpha blended text of the colour specified

            // Paint the text
            if (NativeMethods.DrawThemeTextEx(renderer.Handle, memoryHdc, 0, 0, e.Text, -1, (uint)e.TextFormat, ref textRect, ref opts) != 0)
            {
                throw new Win32Exception();
            }

            // Set up the AlphaBlend copy
            NativeMethods.BLENDFUNCTION blendFunc = default(NativeMethods.BLENDFUNCTION);
            blendFunc.BlendOp = NativeMethods.AC_SRC_OVER;
            blendFunc.SourceConstantAlpha = 255;
            blendFunc.AlphaFormat = NativeMethods.AC_SRC_ALPHA; // Per-pixel alpha only

            // Blend the painted text into the render DC
            if (!NativeMethods.AlphaBlend(renderHdc, e.TextRectangle.Left, e.TextRectangle.Top, e.TextRectangle.Width, e.TextRectangle.Height, memoryHdc, 0, 0, e.TextRectangle.Width, e.TextRectangle.Height, blendFunc))
            {
                throw new Win32Exception();
            }

            // Clean up the GDI objects
            if (!NativeMethods.DeleteObject(hFont))
            {
                throw new Win32Exception();
            }

            if (!NativeMethods.DeleteObject(dib))
            {
                throw new Win32Exception();
            }

            if (!NativeMethods.DeleteDC(memoryHdc))
            {
                throw new Win32Exception();
            }

            e.Graphics.ReleaseHdc();
        }

        protected override void OnRenderToolStripBorder(System.Windows.Forms.ToolStripRenderEventArgs e)
        {
            if (OsUtils.WinVistaOrLater() && VisualStyleRenderer.IsSupported)
            {
                if (!OsUtils.CompositionEnabled())
                {
                    e.Graphics.DrawLine(this.nonAeroBorder, 0, e.AffectedBounds.Bottom - 1, e.ToolStrip.Width, e.AffectedBounds.Bottom - 1);
                }
            }
            else
            {
                e.Graphics.DrawLine(this.tabBorder, 0, e.AffectedBounds.Bottom - 1, e.ToolStrip.Width, e.AffectedBounds.Bottom - 1);
            }

            ToolStripButton @checked = null;

            // Find the currently checked ToolStripButton
            foreach (ToolStripItem item in e.ToolStrip.Items)
            {
                ToolStripButton buttonItem = item as ToolStripButton;

                if (buttonItem != null && buttonItem.Checked)
                {
                    @checked = buttonItem;
                    break;
                }
            }

            if (@checked != null)
            {
                // Extend the bottom of the tab over the client area border, joining the tab onto the main client area
                using (SolidBrush toolbarBkg = new SolidBrush(this.GetActiveTabBtmCol(e.ToolStrip, @checked)))
                {
                    e.Graphics.FillRectangle(toolbarBkg, new Rectangle(@checked.Bounds.Left, @checked.Bounds.Bottom, @checked.Bounds.Width - TabSeparation, e.ToolStrip.Bounds.Bottom - @checked.Bounds.Bottom));
                }

                e.Graphics.DrawLine(this.tabBorder, @checked.Bounds.Left, @checked.Bounds.Bottom, @checked.Bounds.Left, e.AffectedBounds.Bottom);
                e.Graphics.DrawLine(this.tabBorder, @checked.Bounds.Right - TabSeparation, @checked.Bounds.Bottom, @checked.Bounds.Right - TabSeparation, e.AffectedBounds.Bottom);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    if (this.inactiveTabBkg != null)
                    {
                        this.inactiveTabBkg.Dispose();
                        this.hoverTabBkg.Dispose();
                        this.pressedTabBkg.Dispose();
                    }

                    this.rendererFor.FindForm().Activated -= this.Form_Activated;
                    this.rendererFor.FindForm().Deactivate -= this.Form_Deactivated;

                    this.tabBorder.Dispose();
                    this.nonAeroBorder.Dispose();
                }
            }

            this.isDisposed = true;
        }

        private Color GetActiveTabBtmCol(ToolStrip strip, ToolStripItem active)
        {
            Color toolbarColour = SystemColors.Control;

            if (VisualStyleRenderer.IsSupported)
            {
                // Visual styles are enabled, so draw the correct background behind the toolbars
                Bitmap background = new Bitmap(strip.Width, strip.Height);
                Graphics graphics = Graphics.FromImage(background);

                try
                {
                    VisualStyleRenderer rebar = new VisualStyleRenderer("Rebar", 0, 0);
                    rebar.DrawBackground(graphics, new Rectangle(0, 0, strip.Width, strip.Height));
                    toolbarColour = background.GetPixel(active.Bounds.Left + Convert.ToInt32(active.Width / 2), 0);
                }
                catch (ArgumentException)
                {
                    // The 'Rebar' background image style did not exist
                }
            }

            return toolbarColour;
        }

        private void Form_Activated(object sender, System.EventArgs e)
        {
            this.isActive = true;
            this.rendererFor.Invalidate();
        }

        private void Form_Deactivated(object sender, System.EventArgs e)
        {
            this.isActive = false;
            this.rendererFor.Invalidate();
        }
    }
}
