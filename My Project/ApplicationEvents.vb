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

Namespace My
    Partial Friend Class MyApplication
        Private Sub MyApplication_Startup(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup
            ' Add an extra handler to catch unhandled exceptions in other threads
            If Debugger.IsAttached = False Then
                AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf AppDomainExceptionHandler
            End If

            ' If /exit was passed on the command line, then just exit immediately
            For Each commandLineArg As String In Environment.GetCommandLineArgs
                If commandLineArg.ToUpperInvariant = "/EXIT" Then
                    e.Cancel = True
                    Exit Sub
                End If
            Next
        End Sub

        Private Sub MyApplication_StartupNextInstance(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs) Handles Me.StartupNextInstance
            For Each commandLineArg As String In e.CommandLine
                If commandLineArg.ToUpperInvariant = "/EXIT" Then
                    ' Close the application
                    RadioDld.Main.mnuTrayExit_Click(sender, e)
                    Exit Sub
                End If
            Next

            ' Do the same as a double click on the tray icon
            Call RadioDld.Main.mnuTrayShow_Click(sender, e)
        End Sub

        Private Sub MyApplication_UnhandledException(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            If ReportError.Visible = False Then
                Dim report As New ErrorReporting(e.Exception)
                ReportError.AssignReport(report)
                ReportError.ShowDialog()
            End If
        End Sub

        Private Sub AppDomainExceptionHandler(ByVal sender As Object, ByVal e As System.UnhandledExceptionEventArgs)
            Dim unhandledExp As Exception

            Try
                unhandledExp = DirectCast(e.ExceptionObject, Exception)
            Catch classCastExp As InvalidCastException
                ' The ExceptionObject isn't a child of System.Exception, so we don't know
                ' how to report it.  Instead, let the standard .net dialog appear.
                Exit Sub
            End Try

            If ReportError.Visible = False Then
                Dim report As New ErrorReporting(unhandledExp)
                ReportError.AssignReport(report)
                ReportError.ShowDialog()
            End If
        End Sub
    End Class
End Namespace