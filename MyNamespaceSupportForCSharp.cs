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

        [ThreadStatic]
        static User user;

        public static User User
        {
            [DebuggerStepThrough]
            get
            {
                if (user == null)
                {
                    user = new User();
                }

                return user;
            }
        }

        [ThreadStatic]
        static MyForms forms;

        public static MyForms Forms
        {
            [DebuggerStepThrough]
            get
            {
                if (forms == null)
                {
                    forms = new MyForms();
                }

                return forms;
            }
        }

        internal sealed class MyForms
        {
            global::RadioDld.CleanUp CleanUp_instance;
            bool CleanUp_isCreating;

            public global::RadioDld.CleanUp CleanUp
            {
                [DebuggerStepThrough]
                get { return GetForm(ref CleanUp_instance, ref CleanUp_isCreating); }
                [DebuggerStepThrough]
                set { SetForm(ref CleanUp_instance, value); }
            }

            global::RadioDld.About About_instance;
            bool About_isCreating;

            public global::RadioDld.About About
            {
                [DebuggerStepThrough]
                get { return GetForm(ref About_instance, ref About_isCreating); }
                [DebuggerStepThrough]
                set { SetForm(ref About_instance, value); }
            }

            global::RadioDld.Status Status_instance;
            bool Status_isCreating;

            public global::RadioDld.Status Status
            {
                [DebuggerStepThrough]
                get { return GetForm(ref Status_instance, ref Status_isCreating); }
                [DebuggerStepThrough]
                set { SetForm(ref Status_instance, value); }
            }

            global::RadioDld.ReportError ReportError_instance;
            bool ReportError_isCreating;

            public global::RadioDld.ReportError ReportError
            {
                [DebuggerStepThrough]
                get { return GetForm(ref ReportError_instance, ref ReportError_isCreating); }
                [DebuggerStepThrough]
                set { SetForm(ref ReportError_instance, value); }
            }

            global::RadioDld.UpdateNotify UpdateNotify_instance;
            bool UpdateNotify_isCreating;

            public global::RadioDld.UpdateNotify UpdateNotify
            {
                [DebuggerStepThrough]
                get { return GetForm(ref UpdateNotify_instance, ref UpdateNotify_isCreating); }
                [DebuggerStepThrough]
                set { SetForm(ref UpdateNotify_instance, value); }
            }

            global::RadioDld.Main Main_instance;
            bool Main_isCreating;

            public global::RadioDld.Main Main
            {
                [DebuggerStepThrough]
                get { return GetForm(ref Main_instance, ref Main_isCreating); }
                [DebuggerStepThrough]
                set { SetForm(ref Main_instance, value); }
            }

            global::RadioDld.Preferences Preferences_instance;
            bool Preferences_isCreating;

            public global::RadioDld.Preferences Preferences
            {
                [DebuggerStepThrough]
                get { return GetForm(ref Preferences_instance, ref Preferences_isCreating); }
                [DebuggerStepThrough]
                set { SetForm(ref Preferences_instance, value); }
            }

            global::RadioDld.GlassForm GlassForm_instance;
            bool GlassForm_isCreating;

            public global::RadioDld.GlassForm GlassForm
            {
                [DebuggerStepThrough]
                get { return GetForm(ref GlassForm_instance, ref GlassForm_isCreating); }
                [DebuggerStepThrough]
                set { SetForm(ref GlassForm_instance, value); }
            }

            global::RadioDld.ChooseCols ChooseCols_instance;
            bool ChooseCols_isCreating;

            public global::RadioDld.ChooseCols ChooseCols
            {
                [DebuggerStepThrough]
                get { return GetForm(ref ChooseCols_instance, ref ChooseCols_isCreating); }
                [DebuggerStepThrough]
                set { SetForm(ref ChooseCols_instance, value); }
            }

            [DebuggerStepThrough]
            static T GetForm<T>(ref T instance, ref bool isCreating) where T : Form, new()
            {
                if (instance == null || instance.IsDisposed)
                {
                    if (isCreating)
                    {
                        throw new InvalidOperationException(Utils.GetResourceString("WinForms_RecursiveFormCreate", new string[0]));
                    }

                    isCreating = true;

                    try
                    {
                        instance = new T();
                    }
                    catch (System.Reflection.TargetInvocationException ex)
                    {
                        throw new InvalidOperationException(Utils.GetResourceString("WinForms_SeeInnerException", new string[] { ex.InnerException.Message }), ex.InnerException);
                    }
                    finally
                    {
                        isCreating = false;
                    }
                }

                return instance;
            }

            [DebuggerStepThrough]
            static void SetForm<T>(ref T instance, T value) where T : Form
            {
                if (instance != value)
                {
                    if (value == null)
                    {
                        instance.Dispose();
                        instance = null;
                    }
                    else
                    {
                        throw new ArgumentException("Property can only be set to null");
                    }
                }
            }
        }
    }

    partial class MyApplication : WindowsFormsApplicationBase
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.SetCompatibleTextRenderingDefault(UseCompatibleTextRendering);
            MyProject.Application.Run(args);
        }
    }

    partial class MyComputer : Computer
    {
    }
}
