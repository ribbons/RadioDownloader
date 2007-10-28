' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2007  www.nerdoftheherd.com
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

Friend Class clsDldProgData
    Private strProgType As String
    Private strProgID As String
    Private intDuration As Integer
    Private dteProgDate As Date
    Private strProgTitle As String
    Private strStationID As String
    Private strFinalName As String
    Private strProgDldUrl As String
    Private intBandwidthLimit As Integer

    Public Property ProgramType() As String
        Get
            ProgramType = strProgType
        End Get
        Set(ByVal strProgramType As String)
            strProgType = strProgramType
        End Set
    End Property

    Public Property StationID() As String
        Get
            Return strStationID
        End Get
        Set(ByVal strInStationID As String)
            strStationID = strInStationID
        End Set
    End Property

    Public Property ProgramID() As String
        Get
            ProgramID = strProgID
        End Get
        Set(ByVal strProgramID As String)
            strProgID = strProgramID
        End Set
    End Property

    Public Property ProgramDuration() As Integer
        Get
            Return intDuration
        End Get
        Set(ByVal intInDuration As Integer)
            intDuration = intInDuration
        End Set
    End Property

    Public Property ProgramDate() As Date
        Get
            ProgramDate = dteProgDate
        End Get
        Set(ByVal dteProgramDate As Date)
            dteProgDate = dteProgramDate
        End Set
    End Property

    Public Property ProgramTitle() As String
        Get
            Return strProgTitle
        End Get
        Set(ByVal strProgramTitle As String)
            strProgTitle = strProgramTitle
        End Set
    End Property

    Public Property FinalName() As String
        Get
            FinalName = strFinalName
        End Get
        Set(ByVal strFinalName As String)
            Me.strFinalName = strFinalName
        End Set
    End Property

    Public Property DownloadUrl() As String
        Get
            Return strProgDldUrl
        End Get
        Set(ByVal strProgDldUrl As String)
            Me.strProgDldUrl = strProgDldUrl
        End Set
    End Property

    Public Property BandwidthLimit() As Integer
        Get
            Return intbandwidthlimit
        End Get
        Set(ByVal intBandwidthLimit As Integer)
            Me.intbandwidthlimit = intBandwidthLimit
        End Set
    End Property
End Class