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

Friend Class DldProgData
    Private _pluginId As Guid
    Private _progExtId As String
    Private _epId As Integer
    Private _episodeExtId As String
    Private _progInfo As ProgrammeInfo
    Private _episodeInfo As EpisodeInfo
    Private _finalName As String

    Public Property PluginId() As Guid
        Get
            Return _pluginId
        End Get
        Set(ByVal pluginId As Guid)
            Me._pluginId = pluginId
        End Set
    End Property

    Public Property ProgExtId() As String
        Get
            Return _progExtId
        End Get
        Set(ByVal progExtId As String)
            Me._progExtId = progExtId
        End Set
    End Property

    Public Property EpId() As Integer
        Get
            Return _epId
        End Get
        Set(ByVal epId As Integer)
            Me._epId = epId
        End Set
    End Property

    Public Property EpisodeExtId() As String
        Get
            Return _episodeExtId
        End Get
        Set(ByVal episodeExtId As String)
            Me._episodeExtId = episodeExtId
        End Set
    End Property

    Public Property ProgInfo() As ProgrammeInfo
        Get
            Return _progInfo
        End Get
        Set(ByVal progInfo As ProgrammeInfo)
            Me._progInfo = progInfo
        End Set
    End Property

    Public Property EpisodeInfo() As EpisodeInfo
        Get
            Return _episodeInfo
        End Get
        Set(ByVal episodeInfo As EpisodeInfo)
            Me._episodeInfo = episodeInfo
        End Set
    End Property

    Public Property FinalName() As String
        Get
            Return _finalName
        End Get
        Set(ByVal finalName As String)
            Me._finalName = finalName
        End Set
    End Property
End Class