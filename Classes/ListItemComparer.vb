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

Friend Class ListItemComparer
    Implements IComparer

    Public Enum ListType
        Favourite
        Subscription
        Download
    End Enum

    Private dataInstance As Data
    Private compareType As ListType

    Public Sub New(ByVal compareType As ListType)
        dataInstance = Data.GetInstance
        Me.compareType = compareType
    End Sub

    Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare
        Dim itemXId As Integer = CInt(CType(x, ListViewItem).Name)
        Dim itemYId As Integer = CInt(CType(y, ListViewItem).Name)

        Select Case compareType
            Case ListType.Favourite
                Return dataInstance.CompareFavourites(itemXId, itemYId)
            Case ListType.Subscription
                Return dataInstance.CompareSubscriptions(itemXId, itemYId)
            Case ListType.Download
                Return dataInstance.CompareDownloads(itemXId, itemYId)
            Case Else
                Throw New InvalidOperationException("Unknown compare type of " + compareType.ToString)
        End Select
    End Function
End Class
