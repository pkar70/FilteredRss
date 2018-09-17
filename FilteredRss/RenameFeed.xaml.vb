' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238


Imports System.Xml.Serialization

Public Class OneFeedName
    Public Property sUrl As String
    Public Property sName As String
    Public Property sOldName As String
End Class

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class RenameFeed
    Inherits Page

    Dim mlFeedNames As Collection(Of OneFeedName)

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        mlFeedNames = New Collection(Of OneFeedName)
        'Dim sXml As String = App.GetSettingsString("RenameFeeds", "")
        'Dim oSer As XmlSerializer = New XmlSerializer(GetType(ObservableCollection(Of OneFeedName)))
        'Dim oStream As Stream = New MemoryStream
        'Dim oWrt As StreamWriter = New StreamWriter(oStream)
        'oWrt.Write(sXml)
        'oWrt.Flush()

        'Dim bError As Boolean = False
        'Try
        '    mlFeedNames = TryCast(oSer.Deserialize(oStream), ObservableCollection(Of OneFeedName))
        'Catch ex As Exception
        '    bError = True
        'End Try
        'If bError Then
        '    App.DialogBoxRes("resErrorReadingXML")
        'End If

        Dim sTmp As String = App.GetSettingsString("KnownFeeds", "")
        Dim aFeeds As String() = sTmp.Split(vbCrLf)
        For Each sFeed As String In aFeeds
            ' bez pustych linii
            If sFeed.Length > 10 Then
                Dim oNew As OneFeedName = New OneFeedName
                oNew.sUrl = sFeed
                oNew.sName = App.GetSettingsString("FeedName_" & App.Url2VarName(sFeed))
                oNew.sOldName = oNew.sName
                mlFeedNames.Add(oNew)
            End If
        Next


        uiListItems.ItemsSource = mlFeedNames
    End Sub

    Private Sub uiOk_Click(sender As Object, e As RoutedEventArgs)

        'Dim sXml As String = App.GetSettingsString("RenameFeeds", "")
        'Dim oSer As XmlSerializer = New XmlSerializer(GetType(ObservableCollection(Of OneFeedName)))
        'Dim oStream As Stream = New MemoryStream
        'Dim oWrt As StreamWriter = New StreamWriter(oStream)
        'oWrt.Write(sXml)
        'oWrt.Flush()

        For Each oItem As OneFeedName In mlFeedNames
            If oItem.sName <> oItem.sOldName Then
                ' tylko wtedy cos zapisujemy, gdy nowa nazwa inna niz stara
                App.SetSettingsString("FeedName_" & App.Url2VarName(oItem.sUrl), oItem.sName, uiRoam.IsOn)
            End If
        Next

        Me.Frame.GoBack()
    End Sub
End Class
