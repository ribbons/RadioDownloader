Option Strict On
Option Explicit On

Imports System.Diagnostics.Process

Public NotInheritable Class frmAbout
    Private Sub frmAbout_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ' Set the title of the form.
        Dim ApplicationTitle As String
        If My.Application.Info.Title <> "" Then
            ApplicationTitle = My.Application.Info.Title
        Else
            ApplicationTitle = System.IO.Path.GetFileNameWithoutExtension(My.Application.Info.AssemblyName)
        End If
        Me.Text = String.Format("About {0}", ApplicationTitle)
        Me.LabelNameAndVer.Text = String.Format("{0} {1}", My.Application.Info.ProductName, My.Application.Info.Version.ToString)
        Me.LabelCopyright.Text = My.Application.Info.Copyright
    End Sub

    Private Sub OKButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonOK.Click
        Me.Close()
    End Sub

    Private Sub HomepageLink_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles HomepageLink.Click
        Start("www.nerdoftheherd.com/utils/radiodld")
    End Sub
End Class
