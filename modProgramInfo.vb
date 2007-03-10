Option Strict Off
Option Explicit On

Imports System.Text.RegularExpressions

Module modProgramInfo
	
    Public Sub ListStation(ByVal strStationId As String, ByRef lstAddTo As System.Windows.Forms.ListView)
        lstAddTo.Items.Clear()

        Dim strStationPage As String
        Dim lngTrim As Integer

        strStationPage = GetUrlAsString("http://www.bbc.co.uk/radio/aod/networks/" & strStationId & "/audiolist.shtml")
        ' Cut off the 'highlights' from the top of the page to stop repitition
        lngTrim = InStr(1, strStationPage, "<div id=""az"">")
        strStationPage = Mid(strStationPage, lngTrim)

        Dim RegExpression As New Regex("<a href=""aod\.shtml\?(.*?)"" target=""bbcplayer""( class=""txday""|)>(.*?)</a>")

        If RegExpression.IsMatch(strStationPage) = False Then
            Exit Sub
        End If

        Dim matMatches As Match = RegExpression.Match(strStationPage)

        Dim strThisName As String
        Dim strLastName As String = ""
        Dim booLastWasNamed As Boolean
        Dim strDay As String
        Dim lstAddItem As ListViewItem

        While matMatches.Success
            Select Case matMatches.Groups(3).ToString()
                Case "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN"
                    strThisName = strLastName
                    strDay = matMatches.Groups(3).ToString

                    If booLastWasNamed Then
                        booLastWasNamed = False
                        Call lstAddTo.Items.Remove(lstAddTo.Items(lstAddTo.Items.Count - 1))
                    End If
                Case Else
                    strThisName = matMatches.Groups(3).ToString
                    strDay = ""
                    strLastName = strThisName
                    booLastWasNamed = True
            End Select

            If strDay <> "" Then
                strDay = " (" & StrConv(strDay, VbStrConv.ProperCase) & ")"
            End If

            lstAddItem = New ListViewItem
            lstAddItem.Text = Replace(strThisName, "&amp;", "&") & strDay
            lstAddItem.ImageKey = "new"
            lstAddItem.Tag = "BBCLA" & "||" + matMatches.Groups(1).ToString
            Call lstAddTo.Items.Add(lstAddItem)

            matMatches = matMatches.NextMatch
        End While
    End Sub
End Module