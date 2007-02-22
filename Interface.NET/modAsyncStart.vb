Option Strict Off
Option Explicit On
Module modAsyncStart
	
	' ==============================================================
	' FileName:    mStart.bas
	' Author:      SP McMahon
	' Date:        2 February 2000
	'
	' This BAs module gives you the code you need to implement the
	' Java out-of-process style threading model in VB.  Just
	' add it to an ActiveX EXE, reference Runnable.TLB and then
	' do this in your ActiveX EXE:
	'
	' Implements Runnable
	'
	' Public Sub Start()
	'     mStart.Start Me
	' End Sub
	'
	' Private Sub IRunnable_Start()
	'     ' Do your code here.  It will run async against
	'     ' the calling app.
	' End Sub
	'
	'
	'              based on the Codeflow sample © 1997 Microsoft
	'              from Microsoft.  Microsoft has no warranty,
	'              obligations or liability for any code used here.
	'
	' ==============================================================
	
	
	' To prevent object going out of scope whilst the timer fires:
	'UPGRADE_WARNING: Structure IUnknown may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"'
	Private Declare Function CoLockObjectExternal Lib "ole32" (ByVal pUnk As stdole.IUnknown, ByVal fLock As Integer, ByVal fLastUnlockReleases As Integer) As Integer
	
	' Timer API:
	Private Declare Function SetTimer Lib "user32" (ByVal hWnd As Integer, ByVal nIDEvent As Integer, ByVal uElapse As Integer, ByVal lpTimerFunc As Integer) As Integer
	Private Declare Function KillTimer Lib "user32" (ByVal hWnd As Integer, ByVal nIDEvent As Integer) As Integer
	
	' Collection of Runnable items to start:
	Private m_colRunnables As Collection
	' The ID of our API Timer:
	Private m_lTimerID As Integer
	
	Private Sub TimerProc(ByVal lHwnd As Integer, ByVal lMsg As Integer, ByVal lTimerID As Integer, ByVal lTime As Integer)
		Dim this As RunnableLib.Runnable
		' Enumerate through the collection, firing the
		' Runnable_Start method for each item in it and
		' releasing our extra lock on the object:
		With m_colRunnables
			Do While .Count() > 0
				this = .Item(1)
				.Remove(1)
				this.Start()
				'Ask the system to release its lock on the object
				'UPGRADE_WARNING: Couldn't resolve default property of object this. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				CoLockObjectExternal(this, 0, 1)
			Loop 
		End With
		' Remove the timer:
		KillTimer(0, lTimerID)
		m_lTimerID = 0
	End Sub
	
	Public Sub Start(ByRef this As RunnableLib.Runnable)
		' Ask the system to lock the object so that
		' it will still perform its work even if it
		' is released
		'UPGRADE_WARNING: Couldn't resolve default property of object this. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		CoLockObjectExternal(this, 1, 1)
		' Add this to runnables:
		If m_colRunnables Is Nothing Then
			m_colRunnables = New Collection
		End If
		m_colRunnables.Add(this)
		' Create a timer to start running the object:
		If Not m_lTimerID Then
			'UPGRADE_WARNING: Add a delegate for AddressOf TimerProc Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="E9E157F7-EF0C-4016-87B7-7D7FBBC6EE08"'
			m_lTimerID = SetTimer(0, 0, 1, AddressOf TimerProc)
		End If
	End Sub
End Module