/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2018 by the authors - see the AUTHORS file for details.
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

    internal class TaskbarNotify
    {
        private NativeMethods.ITaskbarList3 taskBarListInst;

        public TaskbarNotify()
        {
            if (!OsUtils.Windows())
            {
                // OS doesn't support ITaskbarList3 interface
                return;
            }

            this.taskBarListInst = (NativeMethods.ITaskbarList3)new TaskbarList();

            try
            {
                this.taskBarListInst.HrInit();
            }
            catch (NotImplementedException)
            {
                // Some third party Windows shells don't support ITaskbarList3
                return;
            }

            this.Supported = true;
        }

        public bool Supported { get; private set; }

        public void SetOverlayIcon(Form parentWin, Icon icon, string description)
        {
            if (!this.Supported)
            {
                return;
            }

            try
            {
                this.taskBarListInst.SetOverlayIcon(parentWin.Handle, icon == null ? IntPtr.Zero : icon.Handle, description);
            }
            catch
            {
                // Ignore exceptions as SetOverlayIcon seems to throw various sorts for no apparent reason
            }
        }

        public void SetThumbnailTooltip(Form parentWin, string tooltip)
        {
            if (!this.Supported)
            {
                return;
            }

            try
            {
                this.taskBarListInst.SetThumbnailTooltip(parentWin.Handle, tooltip);
            }
            catch (COMException)
            {
                // Ignore COMExceptions, as they seem to be erroneously thrown sometimes when calling SetThumbnailTooltip
            }
        }

        public void SetProgressValue(Form parentWin, long value, long total)
        {
            if (!this.Supported)
            {
                return;
            }

            if (value < 0)
            {
                throw new ArgumentException("value must not be negative", "value");
            }

            if (total < 0)
            {
                throw new ArgumentException("total must not be negative", "total");
            }

            this.taskBarListInst.SetProgressValue(parentWin.Handle, Convert.ToUInt64(value), Convert.ToUInt64(total));
            this.taskBarListInst.SetProgressState(parentWin.Handle, NativeMethods.TBPFLAG.TBPF_NORMAL);
        }

        public void SetProgressMarquee(Form parentWin)
        {
            if (!this.Supported)
            {
                return;
            }

            this.taskBarListInst.SetProgressState(parentWin.Handle, NativeMethods.TBPFLAG.TBPF_INDETERMINATE);
        }

        public void SetProgressNone(Form parentWin)
        {
            if (!this.Supported)
            {
                return;
            }

            this.taskBarListInst.SetProgressState(parentWin.Handle, NativeMethods.TBPFLAG.TBPF_NOPROGRESS);
        }

        [ComImport]
        [Guid("56FDF344-FD6D-11D0-958A-006097C9A090")]
        [ClassInterface(ClassInterfaceType.None)]
        private class TaskbarList
        {
        }
    }
}
