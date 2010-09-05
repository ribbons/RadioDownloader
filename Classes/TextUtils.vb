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

Imports System.Globalization
Imports System.Text.RegularExpressions

Friend Class TextUtils
    Public Shared Function StripDateFromName(ByVal name As String, ByVal stripDate As Date) As String
        ' Use regex to remove a number of different date formats from episode titles.
        ' Will only remove dates with the same month & year as the programme itself, but any day of the month
        ' as there is sometimes a mismatch of a day or two between the date in a title and the publish date.
        Dim matchStripDate As New Regex("\A(" + stripDate.ToString("yyyy", CultureInfo.InvariantCulture) + "/" + stripDate.ToString("MM", CultureInfo.InvariantCulture) + "/\d{2} ?-? )?(?<name>.*?)( ?:? (\d{2}/" + stripDate.ToString("MM", CultureInfo.InvariantCulture) + "/" + stripDate.ToString("yyyy", CultureInfo.InvariantCulture) + "|((Mon|Tue|Wed|Thu|Fri) )?(\d{1,2}(st|nd|rd|th)? )?(" + stripDate.ToString("MMMM", CultureInfo.InvariantCulture) + "|" + stripDate.ToString("MMM", CultureInfo.InvariantCulture) + ")( \d{1,2}(st|nd|rd|th)?| (" + stripDate.ToString("yy", CultureInfo.InvariantCulture) + "|" + stripDate.ToString("yyyy", CultureInfo.InvariantCulture) + "))?))?\Z")

        If matchStripDate.IsMatch(name) Then
            name = matchStripDate.Match(name).Groups("name").ToString
        End If

        Return name
    End Function

    Public Shared Function DescDuration(ByVal duration As Integer) As String
        Dim readable As String = String.Empty

        If duration <> 0 Then
            readable += Environment.NewLine + "Duration: "

            Dim mins As Integer = CInt(Math.Round(duration / 60, 0))
            Dim hours As Integer = mins \ 60
            mins = mins Mod 60

            If hours > 0 Then
                readable += CStr(hours) + "hr" + If(hours = 1, "", "s")
            End If

            If hours > 0 And mins > 0 Then
                readable += " "
            End If

            If mins > 0 Then
                readable += CStr(mins) + "min"
            End If
        End If

        Return readable
    End Function
End Class
