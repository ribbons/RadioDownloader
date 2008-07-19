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

Public Class clsDldProgData
    Private gidPluginID As Guid
    Private strProgExtID As String
    Private strEpisodeExtID As String
    Private priProgInfo As IRadioProvider.ProgrammeInfo
    Private epiEpisodeInfo As IRadioProvider.EpisodeInfo
    Private strFinalName As String
    Private intBandwidthLimit As Integer
    Private intAttemptNumber As Integer

    Public Property PluginID() As Guid
        Get
            Return gidPluginID
        End Get
        Set(ByVal gidPluginID As Guid)
            Me.gidPluginID = gidPluginID
        End Set
    End Property

    Public Property ProgExtID() As String
        Get
            Return strProgExtID
        End Get
        Set(ByVal strProgExtID As String)
            Me.strProgExtID = strProgExtID
        End Set
    End Property

    Public Property EpisodeExtID() As String
        Get
            Return strEpisodeExtID
        End Get
        Set(ByVal strEpisodeExtID As String)
            Me.strEpisodeExtID = strEpisodeExtID
        End Set
    End Property

    Public Property ProgInfo() As IRadioProvider.ProgrammeInfo
        Get
            Return priProgInfo
        End Get
        Set(ByVal priProgInfo As IRadioProvider.ProgrammeInfo)
            Me.priProgInfo = priProgInfo
        End Set
    End Property

    Public Property EpisodeInfo() As IRadioProvider.EpisodeInfo
        Get
            Return epiEpisodeInfo
        End Get
        Set(ByVal priProgInfo As IRadioProvider.EpisodeInfo)
            Me.epiEpisodeInfo = epiEpisodeInfo
        End Set
    End Property

    Public Property FinalName() As String
        Get
            Return strFinalName
        End Get
        Set(ByVal strFinalName As String)
            Me.strFinalName = strFinalName
        End Set
    End Property

    Public Property BandwidthLimit() As Integer
        Get
            Return intBandwidthLimit
        End Get
        Set(ByVal intBandwidthLimit As Integer)
            Me.intBandwidthLimit = intBandwidthLimit
        End Set
    End Property

    Public Property AttemptNumber() As Integer
        Get
            Return intAttemptNumber
        End Get
        Set(ByVal intAttemptNumber As Integer)
            Me.intAttemptNumber = intAttemptNumber
        End Set
    End Property
End Class