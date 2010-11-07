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
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace RadioDld
{
    internal class TabBarRenderer : ToolStripSystemRenderer, IDisposable
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateDIBSection(IntPtr hdc, [In()] ref BITMAPINFO lpbmi, uint usage, ref IntPtr ppvBits, IntPtr hSection, uint offset);

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr ho);

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("UxTheme.dll", SetLastError = true)]
        private static extern int DrawThemeTextEx(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, [MarshalAs(UnmanagedType.LPWStr)] string pszText, int iCharCount, uint dwFlags, ref RECT pRect, [In()] ref DTTOPTS pOptions);

        [DllImport("msimg32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AlphaBlend(IntPtr hdcDest, int xoriginDest, int yoriginDest, int wDest, int hDest, IntPtr hdcSrc, int xoriginSrc, int yoriginSrc, int wSrc, int hSrc, BLENDFUNCTION ftn);

        [StructLayout(LayoutKind.Sequential)]
        private struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFO
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DTTOPTS
        {
            public uint dwSize;
            public uint dwFlags;
            public uint crText;
            public uint crBorder;
            public uint crShadow;
            public int iTextShadowType;
            public Point ptShadowOffset;
            public int iBorderSize;
            public int iFontPropId;
            public int iColorPropId;
            public int iStateId;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fApplyOverlay;
            public int iGlowSize;
            public int pfnDrawTextCallback;
            public int lParam;
        }

        private const int BI_RGB = 0;

        private const int DTT_COMPOSITED = 8192;
        private const int DTT_GLOWSIZE = 2048;
        private const int DTT_TEXTCOLOR = 1;

        private const int AC_SRC_OVER = 0;
        private const int AC_SRC_ALPHA = 1;

        // NAV_BACKBUTTONSTATES / NAV_FORWARDBUTTONSTATES
        private const int NAV_BF_NORMAL = 1;
        private const int NAV_BF_HOT = 2;
        private const int NAV_BF_PRESSED = 3;
        private const int NAV_BF_DISABLED = 4;

        private const int tabSeparation = 3;

        private ToolStrip rendererFor;

        private bool isDisposed;
        private bool isActive = true;

        private Brush inactiveTabBkg;
        private Brush hoverTabBkg;
        private Brush pressedTabBkg;

        private Pen tabBorder = new Pen(SystemColors.ControlDark);
        private Pen nonAeroBorder = new Pen(Color.FromArgb(255, 182, 193, 204));

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
                    if (e.Item.Enabled == false)
                    {
                        navigation = new VisualStyleRenderer("Navigation", stylePart, NAV_BF_DISABLED);
                    }
                    else if (e.Item.Pressed)
                    {
                        navigation = new VisualStyleRenderer("Navigation", stylePart, NAV_BF_PRESSED);
                    }
                    else if (e.Item.Selected)
                    {
                        navigation = new VisualStyleRenderer("Navigation", stylePart, NAV_BF_HOT);
                    }
                    else
                    {
                        navigation = new VisualStyleRenderer("Navigation", stylePart, NAV_BF_NORMAL);
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
                        e.Graphics.Clear(SystemColors.GradientActiveCaption);
                    }
                    else
                    {
                        e.Graphics.Clear(SystemColors.GradientInactiveCaption);
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

                int width = e.Item.Width - tabSeparation;
                int height = e.Item.Height;

                const int curveSize = 10;

                using (GraphicsPath tab = new GraphicsPath())
                {
                    tab.AddLine(0, height, 0, curveSize);
                    tab.AddArc(0, 0, curveSize, curveSize, 180, 90);
                    tab.AddLine(curveSize, 0, width - curveSize, 0);
                    tab.AddArc(width - curveSize, 0, curveSize, curveSize, 270, 90);
                    tab.AddLine(width, curveSize, width, height);

                    e.Graphics.FillPath(colour, tab);
                    e.Graphics.DrawPath(this.tabBorder, tab);
                }
            }
        }

        protected override void OnRenderItemText(System.Windows.Forms.ToolStripItemTextRenderEventArgs e)
        {
            if (OsUtils.CompositionEnabled() == false)
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
            IntPtr memoryHdc = CreateCompatibleDC(renderHdc);

            // NULL Pointer
            if (memoryHdc == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            BITMAPINFO info = default(BITMAPINFO);
            info.biSize = Convert.ToUInt32(Marshal.SizeOf(typeof(BITMAPINFO)));
            info.biWidth = e.TextRectangle.Width;
            info.biHeight = -e.TextRectangle.Height; // Negative = top-down
            info.biPlanes = 1;
            info.biBitCount = 32;
            info.biCompression = BI_RGB;

            IntPtr bits = IntPtr.Zero;

            // Create the top-down DIB
            IntPtr dib = CreateDIBSection(renderHdc, ref info, 0, ref bits, IntPtr.Zero, 0);

            // NULL Pointer
            if (dib == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            // Select the created DIB into our memory DC for use
            // NULL Pointer
            if (SelectObject(memoryHdc, dib) == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            // Create a font we can use with GetThemeTextEx
            IntPtr hFont = e.TextFont.ToHfont();

            // And select it into the DC as well
            // NULL Pointer
            if (SelectObject(memoryHdc, hFont) == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            // Fetch a VisualStyleRenderer suitable for toolbar text
            VisualStyleRenderer renderer = new VisualStyleRenderer(VisualStyleElement.ToolBar.Button.Normal);

            // Set up a RECT for the area to draw the text in
            RECT textRect = default(RECT);
            textRect.left = 0;
            textRect.top = 0;
            textRect.right = e.TextRectangle.Width;
            textRect.bottom = e.TextRectangle.Height;

            // Options for GetThemeTextEx
            DTTOPTS opts = default(DTTOPTS);
            opts.dwSize = Convert.ToUInt32(Marshal.SizeOf(opts));
            opts.dwFlags = DTT_COMPOSITED | DTT_TEXTCOLOR;
            opts.crText = Convert.ToUInt32(ColorTranslator.ToWin32(e.TextColor)); // Alpha blended text of the colour specified

            // Paint the text
            if (DrawThemeTextEx(renderer.Handle, memoryHdc, 0, 0, e.Text, -1, (uint)e.TextFormat, ref textRect, ref opts) != 0)
            {
                throw new Win32Exception();
            }

            // Set up the AlphaBlend copy
            BLENDFUNCTION blendFunc = default(BLENDFUNCTION);
            blendFunc.BlendOp = AC_SRC_OVER;
            blendFunc.SourceConstantAlpha = 255;
            blendFunc.AlphaFormat = AC_SRC_ALPHA; // Per-pixel alpha only

            // Blend the painted text into the render DC
            if (!AlphaBlend(renderHdc, e.TextRectangle.Left, e.TextRectangle.Top, e.TextRectangle.Width, e.TextRectangle.Height, memoryHdc, 0, 0, e.TextRectangle.Width, e.TextRectangle.Height, blendFunc))
            {
                throw new Win32Exception();
            }

            // Clean up the GDI objects
            if (DeleteObject(hFont) == false)
            {
                throw new Win32Exception();
            }

            if (DeleteObject(dib) == false)
            {
                throw new Win32Exception();
            }

            if (DeleteDC(memoryHdc) == false)
            {
                throw new Win32Exception();
            }

            e.Graphics.ReleaseHdc();
        }

        protected override void OnRenderToolStripBorder(System.Windows.Forms.ToolStripRenderEventArgs e)
        {
            if (OsUtils.WinVistaOrLater() && VisualStyleRenderer.IsSupported)
            {
                if (OsUtils.CompositionEnabled() == false)
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
                    e.Graphics.FillRectangle(toolbarBkg, new Rectangle(@checked.Bounds.Left, @checked.Bounds.Bottom, @checked.Bounds.Width - tabSeparation, e.ToolStrip.Bounds.Bottom - @checked.Bounds.Bottom));
                }

                e.Graphics.DrawLine(this.tabBorder, @checked.Bounds.Left, @checked.Bounds.Bottom, @checked.Bounds.Left, e.AffectedBounds.Bottom);
                e.Graphics.DrawLine(this.tabBorder, @checked.Bounds.Right - tabSeparation, @checked.Bounds.Bottom, @checked.Bounds.Right - tabSeparation, e.AffectedBounds.Bottom);
            }
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

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TabBarRenderer()
        {
            this.Dispose(false);
        }
    }
}
