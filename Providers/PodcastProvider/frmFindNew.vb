' Plugin for Radio Downloader to download general podcasts.
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

Imports System.Xml
Imports System.Net
Imports System.Windows.Forms

Public Class frmFindNew
    Friend clsPluginInst As clsPodcastProvider

    Private Sub cmdViewEps_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdViewEps.Click
        Try
            cmdViewEps.Enabled = False
            lblResult.ForeColor = Drawing.Color.Black
            lblResult.Text = "Checking feed..."

            Application.DoEvents()

            Dim strURL As String = txtFeedURL.Text
            Dim strRSS As String
            Dim xmlRSS As New XmlDocument

            ' First test that we can load something from the specified URL
            Try
                strRSS = clsPluginInst.clsCachedHTTP.DownloadString(strURL, clsPodcastProvider.intCacheHTTPHours)
            Catch expArgument As ArgumentException
                lblResult.Text = "The specified URL was not valid."
                lblResult.ForeColor = Drawing.Color.Red
                cmdViewEps.Enabled = True
                Exit Sub
            Catch expWeb As WebException
                lblResult.Text = "There was a problem requesting the feed from the specified URL."
                lblResult.ForeColor = Drawing.Color.Red
                cmdViewEps.Enabled = True
                Exit Sub
            End Try

            ' Now check that it is a valid XML document
            Try
                xmlRSS.LoadXml(strRSS)
            Catch expXML As XmlException
                lblResult.Text = "The data returned from the specified URL was not a valid RSS feed."
                lblResult.ForeColor = Drawing.Color.Red
                cmdViewEps.Enabled = True
                Exit Sub
            End Try

            ' Finally, make sure that the required elements that we need (title and description) exist
            Dim xmlCheckTitle As XmlNode = xmlRSS.SelectSingleNode("./rss/channel/title")
            Dim xmlCheckDescription As XmlNode = xmlRSS.SelectSingleNode("./rss/channel/description")

            If xmlCheckTitle Is Nothing Or xmlCheckDescription Is Nothing Then
                lblResult.Text = "The RSS feed returned from the specified URL was not valid."
                lblResult.ForeColor = Drawing.Color.Red
                cmdViewEps.Enabled = True
                Exit Sub
            End If

            lblResult.Text = "Loading information..."
            Application.DoEvents()

            clsPluginInst.RaiseFoundNew(strURL)

            lblResult.Text = ""
            cmdViewEps.Enabled = True
        Catch expException As Exception
            clsPluginInst.RaiseFindNewException(expException)
        End Try
    End Sub

    Private Sub txtFeedURL_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtFeedURL.KeyPress
        Try
            If Convert.ToInt32(e.KeyChar) = Keys.Enter Then
                If cmdViewEps.Enabled Then
                    Call cmdViewEps_Click(sender, e)
                End If
            End If
        Catch expException As Exception
            clsPluginInst.RaiseFindNewException(expException)
        End Try
    End Sub
End Class