// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.VisualBasic.Devices;

/* This file was created by the VB to C# converter (SharpDevelop 4.0.0.6721).
   It contains classes for supporting the VB "My" namespace in C#.
   If the VB application does not use the "My" namespace, or if you removed the usage
   after the conversion to C#, you can delete this file. */

namespace RadioDld.My
{
    sealed partial class MyProject
    {
        [ThreadStatic]
        static MyApplication application;

        public static MyApplication Application
        {
            [DebuggerStepThrough]
            get
            {
                if (application == null)
                {
                    application = new MyApplication();
                }

                return application;
            }
        }
    }

    partial class MyApplication : WindowsFormsApplicationBase
    {
    }
}
