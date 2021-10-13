/*
 * Copyright © 2010-2012 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal partial class UpdateNotify : Form
    {
        public UpdateNotify()
        {
            this.InitializeComponent();
        }

        private void Update_Load(object sender, EventArgs e)
        {
            this.Font = SystemFonts.MessageBoxFont;
        }
    }
}
