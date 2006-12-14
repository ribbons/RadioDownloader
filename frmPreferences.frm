VERSION 5.00
Begin VB.Form frmPreferences 
   BorderStyle     =   1  'Fixed Single
   Caption         =   "Options"
   ClientHeight    =   3630
   ClientLeft      =   45
   ClientTop       =   435
   ClientWidth     =   6195
   BeginProperty Font 
      Name            =   "Tahoma"
      Size            =   8.25
      Charset         =   0
      Weight          =   400
      Underline       =   0   'False
      Italic          =   0   'False
      Strikethrough   =   0   'False
   EndProperty
   Icon            =   "frmPreferences.frx":0000
   LinkTopic       =   "Form1"
   LockControls    =   -1  'True
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   3630
   ScaleWidth      =   6195
   StartUpPosition =   1  'CenterOwner
   Begin VB.CommandButton cmdCancel 
      Caption         =   "Cancel"
      Height          =   375
      Left            =   4800
      TabIndex        =   4
      Top             =   3060
      Width           =   1155
   End
   Begin VB.CommandButton cmdOK 
      Caption         =   "OK"
      Height          =   375
      Left            =   3540
      TabIndex        =   3
      Top             =   3060
      Width           =   1155
   End
   Begin VB.CommandButton cmdChangeFolder 
      Caption         =   "Change"
      Height          =   375
      Left            =   4860
      TabIndex        =   2
      Top             =   840
      Width           =   1095
   End
   Begin VB.TextBox txtSaveIn 
      Height          =   375
      Left            =   180
      Locked          =   -1  'True
      TabIndex        =   0
      Top             =   840
      Width           =   4575
   End
   Begin VB.Label lblSaveIn 
      Caption         =   "Save downloaded programs in:"
      Height          =   315
      Left            =   210
      TabIndex        =   1
      Top             =   600
      Width           =   5775
   End
End
Attribute VB_Name = "frmPreferences"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

Private Sub cmdCancel_Click()
    Unload Me
End Sub

Private Sub cmdChangeFolder_Click()
    Dim strReturned As String
    strReturned = BrowseForFolder(Me.hWnd, "Choose the folder to save downloaded programs in:", txtSaveIn.Text)
    
    If strReturned <> "" Then
        txtSaveIn.Text = strReturned
    End If
End Sub

Private Sub cmdOK_Click()
    Call SaveSetting("Radio Downloader", "Interface", "SaveFolder", txtSaveIn.Text)
    Unload Me
End Sub

Private Sub Form_Load()
    txtSaveIn.Text = GetSetting("Radio Downloader", "Interface", "SaveFolder", AddSlash(App.Path) + "Downloads")
    
    If PathIsDirectory(GetSetting("Radio Downloader", "Interface", "SaveFolder", AddSlash(App.Path) + "Downloads")) = False Then
        lblSaveIn.ForeColor = vbRed
        lblSaveIn.Caption = "Path not found - choose a location to store your downloaded programs:"
    End If
End Sub
