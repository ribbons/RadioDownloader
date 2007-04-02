Option Explicit On
Option Strict On

Friend Class clsInterfaceThread
    Private strType As String
    Private strStationID As String
    Private plgAvailablePlugins() As AvailablePlugin
    Private clsProgData As clsData

    Public Property Type() As String
        Get
            Return strType
        End Get
        Set(ByVal strNewType As String)
            strType = strNewType
        End Set
    End Property

    Public Property StationID() As String
        Get
            Return strStationID
        End Get
        Set(ByVal strStationID As String)
            Me.strStationID = strStationID
        End Set
    End Property

    Public Property AvailablePlugins() As AvailablePlugin()
        Get
            Return plgAvailablePlugins
        End Get
        Set(ByVal plgPlugins As AvailablePlugin())
            plgAvailablePlugins = plgPlugins
        End Set
    End Property

    Public Property DataInstance() As clsData
        Get
            Return clsProgData
        End Get
        Set(ByVal clsDataInstance As clsData)
            clsProgData = clsDataInstance
        End Set
    End Property

    Public Sub lstNew_ListPrograms()
        Call clsProgData.StartListingStation(strType, strStationID)
    End Sub
End Class
