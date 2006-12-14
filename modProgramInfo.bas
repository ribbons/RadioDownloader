Attribute VB_Name = "modProgramInfo"
Option Explicit

Public Sub ListStation(strStationId As String, lstAddTo As ListView)
    lstAddTo.ListItems.Clear
    
    Dim strStationPage As String
    Dim lngTrim As Long
    
    strStationPage = GetUrlAsString("http://www.bbc.co.uk/radio/aod/networks/" + strStationId + "/audiolist.shtml")
    ' Cut off the 'highlights' from the top of the page to stop repitition
    lngTrim = InStr(1, strStationPage, "<div id=""az"">")
    strStationPage = Mid$(strStationPage, lngTrim)
    
    Dim objRegExp As RegExp
    Dim objMatch As Match
    Dim colMatches As MatchCollection

    Set objRegExp = New RegExp
    objRegExp.Pattern = "<a href=""aod\.shtml\?(.*?)"" target=""bbcplayer""( class=""txday""|)>(.*?)</a>"
    objRegExp.IgnoreCase = True
    objRegExp.Global = True

    If objRegExp.Test(strStationPage) = False Then
        Exit Sub
    End If

    Set colMatches = objRegExp.Execute(strStationPage)
    
    Dim strThisName As String
    Dim strLastName As String
    Dim booLastWasNamed As Boolean
    Dim strDay As String
    
    For Each objMatch In colMatches
        Select Case objMatch.SubMatches(2)
            Case "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN"
                strThisName = strLastName
                strDay = objMatch.SubMatches(2)
                
                If booLastWasNamed Then
                    booLastWasNamed = False
                    Call lstAddTo.ListItems.Remove(lstAddTo.ListItems.Count)
                End If
            Case Else
                strThisName = objMatch.SubMatches(2)
                strDay = ""
                strLastName = strThisName
                booLastWasNamed = True
        End Select
        
        Dim lstAddItem As ListItem
        Set lstAddItem = lstAddTo.ListItems.Add
        
        If strDay <> "" Then
            strDay = " (" + StrConv(strDay, vbProperCase) + ")"
        End If
        
        lstAddItem.Text = Replace$(strThisName, "&amp;", "&") + strDay
        lstAddItem.SmallIcon = 5
        lstAddItem.Tag = "BBCLA" + "||" + objMatch.SubMatches(0)
    Next
End Sub
