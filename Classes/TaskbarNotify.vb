' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2007-2010 Matt Robinson
'
' This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
' Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
' option) any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
' implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
' License for more details.
'
' You should have received a copy of the GNU General Public License along with this program; if not, write
' to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

Option Strict On
Option Explicit On

Imports System.Runtime.InteropServices

Friend Class TaskbarNotify
    <Flags()> _
    Private Enum TBATFLAG
        TBATF_USEMDITHUMBNAIL = &H1
        TBATF_USEMDILIVEPREVIEW = &H2
    End Enum

    <Flags()> _
    Private Enum TBPFLAG
        TBPF_NOPROGRESS = 0
        TBPF_INDETERMINATE = &H1
        TBPF_NORMAL = &H2
        TBPF_ERROR = &H4
        TBPF_PAUSED = &H8
    End Enum

    <StructLayout(LayoutKind.Sequential, Pack:=4)> _
    Private Structure THUMBBUTTON
        Public dwMask As UInt32
        Public iId As UInt32
        Public iBitmap As UInt32
        Public hIcon As IntPtr
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)> Public szTip As UInt16()
        Public dwFlags As UInt32
    End Structure

    <ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEFAF")> _
    Private Interface ITaskbarList3
        Sub HrInit()
        Sub AddTab(ByVal hwnd As IntPtr)
        Sub DeleteTab(ByVal hwnd As IntPtr)
        Sub ActivateTab(ByVal hwnd As IntPtr)
        Sub SetActivateAlt(ByVal hwnd As IntPtr)
        Sub MarkFullscreenWindow(ByVal hwnd As IntPtr, ByVal fFullscreen As Boolean)
        Sub SetProgressValue(ByVal hwnd As IntPtr, ByVal ullCompleted As UInt64, ByVal ullTotal As UInt64)
        Sub SetProgressState(ByVal hwnd As IntPtr, ByVal tbpFlags As TBPFLAG)
        Sub RegisterTab(ByVal hwndTab As IntPtr, ByVal hwndMDI As IntPtr)
        Sub UnregisterTab(ByVal hwndTab As IntPtr)
        Sub SetTabOrder(ByVal hwndTab As IntPtr, ByVal hwndInsertBefore As Integer)
        Sub SetTabActive(ByVal hwndTab As IntPtr, ByVal hwndMDI As Integer, ByVal tbatFlags As TBATFLAG)
        Sub ThumbBarAddButtons(ByVal hwnd As IntPtr, ByVal cButtons As UInt32, ByVal pButton() As THUMBBUTTON)
        Sub ThumbBarUpdateButtons(ByVal hwnd As IntPtr, ByVal cButtons As UInt32, ByVal pButton() As THUMBBUTTON)
        Sub ThumbBarSetImageList(ByVal hwnd As IntPtr, ByVal himl As IntPtr)
        Sub SetOverlayIcon(ByVal hwnd As IntPtr, ByVal hIcon As IntPtr, <MarshalAs(UnmanagedType.LPWStr)> ByVal pszDescription As String)
        Sub SetThumbnailTooltip(ByVal hwnd As IntPtr, <MarshalAs(UnmanagedType.LPWStr)> ByVal pszTip As String)
        Sub SetThumbnailClip(ByVal hwnd As IntPtr, ByVal prcClip As RECT)
    End Interface

    <ComImport(), Guid("56FDF344-FD6D-11D0-958A-006097C9A090"), ClassInterface(ClassInterfaceType.None)> _
    Private Class TaskbarList
    End Class

    Private taskBarListInst As ITaskbarList3

    Public Sub New()
        taskBarListInst = CType(New TaskbarList, ITaskbarList3)
        taskBarListInst.HrInit()
    End Sub

    Public Sub SetOverlayIcon(ByVal parentWin As Form, ByVal icon As Icon, ByVal description As String)
        Try
            taskBarListInst.SetOverlayIcon(parentWin.Handle, If(icon Is Nothing, IntPtr.Zero, icon.Handle), description)
        Catch comExp As COMException
            ' Ignore COMExceptions, as they seem to be erroneously thrown sometimes when calling SetOverlayIcon
        End Try
    End Sub

    Public Sub SetThumbnailTooltip(ByVal parentWin As Form, ByVal tooltip As String)
        taskBarListInst.SetThumbnailTooltip(parentWin.Handle, tooltip)
    End Sub

    Public Sub SetProgressValue(ByVal parentWin As Form, ByVal value As Long, ByVal total As Long)
        If value < 0 Then
            Throw New ArgumentException("value must not be negative", "value")
        End If

        If total < 0 Then
            Throw New ArgumentException("total must not be negative", "total")
        End If

        taskBarListInst.SetProgressValue(parentWin.Handle, CULng(value), CULng(total))
        taskBarListInst.SetProgressState(parentWin.Handle, TBPFLAG.TBPF_NORMAL)
    End Sub

    Public Sub SetProgressMarquee(ByVal parentWin As Form)
        taskBarListInst.SetProgressState(parentWin.Handle, TBPFLAG.TBPF_INDETERMINATE)
    End Sub

    Public Sub SetProgressNone(ByVal parentWin As Form)
        taskBarListInst.SetProgressState(parentWin.Handle, TBPFLAG.TBPF_NOPROGRESS)
    End Sub
End Class
