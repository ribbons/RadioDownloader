Attribute VB_Name = "modAsyncStart"
Option Explicit

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
Private Declare Function CoLockObjectExternal Lib "ole32" ( _
    ByVal pUnk As IUnknown, ByVal fLock As Long, _
    ByVal fLastUnlockReleases As Long) As Long

' Timer API:
Private Declare Function SetTimer Lib "user32" (ByVal hWnd As Long, _
    ByVal nIDEvent As Long, ByVal uElapse As Long, ByVal lpTimerFunc As Long) _
    As Long
Private Declare Function KillTimer Lib "user32" (ByVal hWnd As Long, _
    ByVal nIDEvent As Long) As Long

' Collection of Runnable items to start:
Private m_colRunnables As Collection
' The ID of our API Timer:
Private m_lTimerID As Long

Private Sub TimerProc(ByVal lHwnd As Long, ByVal lMsg As Long, _
    ByVal lTimerID As Long, ByVal lTime As Long)
Dim this As Runnable
   ' Enumerate through the collection, firing the
   ' Runnable_Start method for each item in it and
   ' releasing our extra lock on the object:
   With m_colRunnables
      Do While .Count > 0
         Set this = .Item(1)
         .Remove 1
         this.Start
         'Ask the system to release its lock on the object
         CoLockObjectExternal this, 0, 1
      Loop
   End With
   ' Remove the timer:
   KillTimer 0, lTimerID
   m_lTimerID = 0
End Sub

Public Sub Start(this As Runnable)
   ' Ask the system to lock the object so that
   ' it will still perform its work even if it
   ' is released
   CoLockObjectExternal this, 1, 1
   ' Add this to runnables:
   If m_colRunnables Is Nothing Then
      Set m_colRunnables = New Collection
   End If
   m_colRunnables.Add this
   ' Create a timer to start running the object:
   If Not m_lTimerID Then
      m_lTimerID = SetTimer(0, 0, 1, AddressOf TimerProc)
   End If
End Sub

