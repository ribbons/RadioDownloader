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

using System.Drawing;
using System.Windows.Forms;

namespace RadioDld
{
    internal partial class CleanUp : Form
    {
        private Data progData;

        public CleanUp()
        {
            InitializeComponent();
        }

        private void CleanUp_Load(System.Object sender, System.EventArgs e)
        {
            progData = Data.GetInstance();
            this.Font = SystemFonts.MessageBoxFont;
        }

        private void cmdCancel_Click(System.Object eventSender, System.EventArgs eventArgs)
        {
            this.Close();
            this.Dispose();
        }

        private void cmdOK_Click(System.Object sender, System.EventArgs e)
        {
            cmdOK.Enabled = false;
            cmdCancel.Enabled = false;
            radType.Enabled = false;
            lblExplainOrphan.Enabled = false;

            progData.PerformCleanup();

            this.Close();
            this.Dispose();
        }
    }
}
