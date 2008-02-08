' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2008  www.nerdoftheherd.com
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

Public Class frmChooseStations
    Private WithEvents clsProgData As clsData

    Private Sub frmChooseStations_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        lstStations.Columns.Add("Station Name", 300)
        lstStations.Columns.Add("Provider", 150)

        clsProgData = New clsData()
        clsProgData.StartListingStations(True)
    End Sub

    Private Sub clsProgData_AddStationToList(ByRef strStationName As String, ByRef strStationId As String, ByRef strStationType As String, ByVal booVisible As Boolean) Handles clsProgData.AddStationToList
        If lstStations.Groups(strStationType) Is Nothing Then
            lstStations.Groups.Add(strStationType, clsProgData.ProviderName(strStationType))
        End If

        Dim lstAddItem As ListViewItem
        lstAddItem = lstStations.Items.Add(strStationName)
        lstAddItem.Tag = strStationType + "||" + strStationId
        lstAddItem.Group = lstStations.Groups(strStationType)
        lstAddItem.SubItems.Add(clsProgData.ProviderName(strStationType))
        lstAddItem.Checked = booVisible
    End Sub

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
        Me.Close()
        Me.Dispose()
    End Sub

    Private Sub cmdOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOK.Click
        cmdOK.Enabled = False
        cmdCancel.Enabled = False

        Dim strSplit() As String

        For Each lstItem As ListViewItem In lstStations.Items
            strSplit = Split(lstItem.Tag.ToString, "||")
            clsProgData.SetStationVisibility(strSplit(0), strSplit(1), lstItem.Checked)
        Next

        Me.Close()
        Me.Dispose()
    End Sub
End Class