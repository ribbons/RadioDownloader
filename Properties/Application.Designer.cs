using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------



namespace RadioDld.My
{

	//NOTE: This file is auto-generated; do not modify it directly.  To make changes,
	// or if you encounter build errors in this file, go to the Project Designer
	// (go to Project Properties or double-click the My Project node in
	// Solution Explorer), and make changes on the Application tab.
	//
	internal partial class MyApplication
	{

		[System.Diagnostics.DebuggerStepThroughAttribute()]
		public MyApplication() : base(global::Microsoft.VisualBasic.ApplicationServices.AuthenticationMode.Windows)
		{
			UnhandledException += MyApplication_UnhandledException;
			StartupNextInstance += MyApplication_StartupNextInstance;
			Startup += MyApplication_Startup;
			this.IsSingleInstance = true;
			this.EnableVisualStyles = true;
			this.SaveMySettingsOnExit = true;
			this.ShutdownStyle = global::Microsoft.VisualBasic.ApplicationServices.ShutdownMode.AfterMainFormCloses;
		}

		[System.Diagnostics.DebuggerStepThroughAttribute()]
		protected override void OnCreateMainForm()
		{
			this.MainForm = My.MyProject.Forms.Main;
		}
	}
}