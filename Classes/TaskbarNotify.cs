/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2010 Matt Robinson
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
    using System.Windows.Forms;

    internal class TaskbarNotify
    {
        private ITaskbarList3 taskBarListInst;

        public TaskbarNotify()
        {
            this.taskBarListInst = (ITaskbarList3)new TaskbarList();
            this.taskBarListInst.HrInit();
        }

        [Flags()]
        private enum TBATFLAG
        {
            TBATF_USEMDITHUMBNAIL = 0x1,
            TBATF_USEMDILIVEPREVIEW = 0x2
        }

        [Flags()]
        private enum TBPFLAG
        {
            TBPF_NOPROGRESS = 0,
            TBPF_INDETERMINATE = 0x1,
            TBPF_NORMAL = 0x2,
            TBPF_ERROR = 0x4,
            TBPF_PAUSED = 0x8
        }

        [ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEFAF")]
        private interface ITaskbarList3
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

        public void SetOverlayIcon(Form parentWin, Icon icon, string description)
        {
            try
            {
                this.taskBarListInst.SetOverlayIcon(parentWin.Handle, icon == null ? IntPtr.Zero : icon.Handle, description);
            }
            catch (COMException)
            {
                // Ignore COMExceptions, as they seem to be erroneously thrown sometimes when calling SetOverlayIcon
            }
        }

        public void SetThumbnailTooltip(Form parentWin, string tooltip)
        {
            this.taskBarListInst.SetThumbnailTooltip(parentWin.Handle, tooltip);
        }

        public void SetProgressValue(Form parentWin, long value, long total)
        {
            if (value < 0)
            {
                throw new ArgumentException("value must not be negative", "value");
            }

            if (total < 0)
            {
                throw new ArgumentException("total must not be negative", "total");
            }

            this.taskBarListInst.SetProgressValue(parentWin.Handle, Convert.ToUInt64(value), Convert.ToUInt64(total));
            this.taskBarListInst.SetProgressState(parentWin.Handle, TBPFLAG.TBPF_NORMAL);
        }

        public void SetProgressMarquee(Form parentWin)
        {
            this.taskBarListInst.SetProgressState(parentWin.Handle, TBPFLAG.TBPF_INDETERMINATE);
        }

        public void SetProgressNone(Form parentWin)
        {
            this.taskBarListInst.SetProgressState(parentWin.Handle, TBPFLAG.TBPF_NOPROGRESS);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct THUMBBUTTON
        {
            public uint dwMask;
            public uint iId;
            public uint iBitmap;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
            public ushort[] szTip;
            public uint dwFlags;
        }

        [ComImport(), Guid("56FDF344-FD6D-11D0-958A-006097C9A090"), ClassInterface(ClassInterfaceType.None)]
        private class TaskbarList
        {
        }
    }
}
