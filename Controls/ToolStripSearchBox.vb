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

Public Class ToolStripSearchBox
    Inherits ToolStripControlHost

    Public Sub New()
        MyBase.New(New SearchBox)
    End Sub

    Public Property CueBanner As String
        Get
            Return DirectCast(Me.Control, SearchBox).CueBanner
        End Get
        Set(ByVal value As String)
            DirectCast(Me.Control, SearchBox).CueBanner = value
        End Set
    End Property
End Class

