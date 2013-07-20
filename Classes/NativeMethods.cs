/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2012 Matt Robinson
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
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class NativeMethods
    {
        // Window Messages
        public const int WM_CREATE = 0x1;
        public const int WM_SETFOCUS = 0x7;
        public const int WM_PAINT = 0xf;
        public const int WM_SETCURSOR = 0x20;
        public const int WM_NOTIFY = 0x4e;
        public const int WM_NCHITTEST = 0x84;
        public const int WM_CHANGEUISTATE = 0x127;
        public const int WM_DWMCOMPOSITIONCHANGED = 0x31e;
        public const int WM_USER = 0x400;
        public const int TB_SETBUTTONINFO = WM_USER + 64;
        public const int LVM_FIRST = 0x1000;
        public const int LVM_GETHEADER = LVM_FIRST + 31;
        public const int LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54;
        public const int HDM_FIRST = 0x1200;
        public const int HDM_GETITEM = HDM_FIRST + 11;
        public const int HDM_SETITEM = HDM_FIRST + 12;
        public const int EM_SETCUEBANNER = 0x1501;

        // TBBUTTONINFO Mask Flags
        public const uint TBIF_STYLE = 0x8;
        public const int TBIF_SIZE = 0x40;
        public const uint TBIF_BYINDEX = 0x80000000;

        // TBBUTTONINFO Style Flags
        public const int BTNS_AUTOSIZE = 0x10;
        public const int BTNS_WHOLEDROPDOWN = 0x80;

        // WM_NCHITTEST return values
        public const int HTTRANSPARENT = -0x1;
        public const int HTCLIENT = 0x1;
        public const int HTCAPTION = 0x2;

        // GetWindow flags
        public const short GW_CHILD = 5;
        public const short GW_HWNDNEXT = 2;

        // DrawAnimatedRects 'type of animation' flag
        public const short IDANI_CAPTION = 0x3;

        // Theme classes & parts
        public const string SEARCHBOX = "SearchBox";
        public const int SBBACKGROUND = 0x1;

        // Theme part states
        public const int SBB_NORMAL = 0x1;
        public const int SBB_HOT = 0x2;
        public const int SBB_DISABLED = 0x3;
        public const int SBB_FOCUSED = 0x4;
        public const int NAV_BF_NORMAL = 1;
        public const int NAV_BF_HOT = 2;
        public const int NAV_BF_PRESSED = 3;
        public const int NAV_BF_DISABLED = 4;

        // BITMAPINFO uncompressed type
        public const int BI_RGB = 0;

        // DrawThemeText flags
        public const int DTT_COMPOSITED = 8192;
        public const int DTT_GLOWSIZE = 2048;
        public const int DTT_TEXTCOLOR = 1;

        // AlphaBlend flags
        public const int AC_SRC_OVER = 0;
        public const int AC_SRC_ALPHA = 1;

        // WM_CHANGEUISTATE Parameters
        public const int UIS_INITIALIZE = 0x3;
        public const int UISF_HIDEFOCUS = 0x1;

        // ListView header info flags
        public const int HDI_FORMAT = 0x4;
        public const int HDF_SORTUP = 0x400;
        public const int HDF_SORTDOWN = 0x200;

        // Extended ListView Styles
        public const int LVS_EX_DOUBLEBUFFER = 0x10000;

        // Notify messages
        public const int NM_FIRST = 0;
        public const int NM_RCLICK = NM_FIRST - 5;

        // System cursors
        public const int IDC_HAND = 32649;

        // ITaskbarList3 Flags
        [Flags]
        public enum TBATFLAG
        {
            TBATF_USEMDITHUMBNAIL = 0x1,
            TBATF_USEMDILIVEPREVIEW = 0x2
        }

        [Flags]
        public enum TBPFLAG
        {
            TBPF_NOPROGRESS = 0,
            TBPF_INDETERMINATE = 0x1,
            TBPF_NORMAL = 0x2,
            TBPF_ERROR = 0x4,
            TBPF_PAUSED = 0x8
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEFAF")]
        public interface ITaskbarList3
        {
            void HrInit();

            void AddTab(IntPtr hwnd);

            void DeleteTab(IntPtr hwnd);

            void ActivateTab(IntPtr hwnd);

            void SetActivateAlt(IntPtr hwnd);

            void MarkFullscreenWindow(IntPtr hwnd, bool fFullscreen);

            void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);

            void SetProgressState(IntPtr hwnd, TBPFLAG tbpFlags);

            void RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);

            void UnregisterTab(IntPtr hwndTab);

            void SetTabOrder(IntPtr hwndTab, int hwndInsertBefore);

            void SetTabActive(IntPtr hwndTab, int hwndMDI, TBATFLAG tbatFlags);

            void ThumbBarAddButtons(IntPtr hwnd, uint cButtons, THUMBBUTTON[] pButton);

            void ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, THUMBBUTTON[] pButton);

            void ThumbBarSetImageList(IntPtr hwnd, IntPtr himl);

            void SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);

            void SetThumbnailTooltip(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);

            void SetThumbnailClip(IntPtr hwnd, NativeMethods.RECT prcClip);
        }

        // API Declarations
        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled([MarshalAs(UnmanagedType.Bool)] ref bool pfEnabled);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, [In()] ref BITMAPINFO lpbmi, uint usage, ref IntPtr ppvBits, IntPtr hSection, uint offset);

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr ho);

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

        [DllImport("msimg32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AlphaBlend(IntPtr hdcDest, int xoriginDest, int yoriginDest, int wDest, int hDest, IntPtr hdcSrc, int xoriginSrc, int yoriginSrc, int wSrc, int hSrc, BLENDFUNCTION ftn);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DrawAnimatedRects(IntPtr hWnd, int idAni, ref RECT lprcFrom, ref RECT lprcTo);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, int wCmd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetClassName(System.IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref HDITEM lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref TBBUTTONINFO lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public static extern int DrawThemeTextEx(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, string pszText, int iCharCount, uint dwFlags, ref RECT pRect, [In()] ref DTTOPTS pOptions);
        
        // API Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyButtomheight;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
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
        public struct DTTOPTS
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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TBBUTTONINFO
        {
            public int cbSize;
            public uint dwMask;
            public int idCommand;
            public int iImage;
            public byte fsState;
            public byte fsStyle;
            public short cx;
            public IntPtr lParam;
            public IntPtr pszText;
            public int cchText;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMHDR
        {
            public IntPtr hwndFrom;
            public UIntPtr idFrom;
            public int code;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HDITEM
        {
            public int mask;
            public int cxy;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public int fmt;
            public int lParam;
            public int iImage;
            public int iOrder;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct THUMBBUTTON
        {
            public uint dwMask;
            public uint iId;
            public uint iBitmap;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
            public ushort[] szTip;
            public uint dwFlags;
        }
    }
}
