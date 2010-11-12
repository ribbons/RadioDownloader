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
    using System.Drawing;
    using System.Windows.Forms;

    internal partial class CleanUp : Form
    {
        private Data progData;

        public CleanUp()
        {
            this.InitializeComponent();
        }

        private void CleanUp_Load(object sender, System.EventArgs e)
        {
            this.progData = Data.GetInstance();
            this.Font = SystemFonts.MessageBoxFont;
        }

        private void cmdCancel_Click(object eventSender, System.EventArgs eventArgs)
        {
            this.Close();
            this.Dispose();
        }

        private void cmdOK_Click(object sender, System.EventArgs e)
        {
            this.cmdOK.Enabled = false;
            this.cmdCancel.Enabled = false;
            this.radType.Enabled = false;
            this.lblExplainOrphan.Enabled = false;

            this.progData.PerformCleanup();

            this.Close();
            this.Dispose();
        }
    }
}
