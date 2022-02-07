
' strona nie do końca zmieniona pod nowy układ - bo jednak nie będzie jako osobna strona, tylko w ramach Setup raczej


Public Class OneFeedName
    Public Property sUrl As String
    Public Property sName As String
    Public Property sOldName As String
End Class


Public NotInheritable Class RenameFeed
    Inherits Page

    'Dim mlFeedNames As Collection(Of OneFeedName)

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)

        feeds.FeedsLoad()

        For Each oFeed As VBlib.JedenFeed In VBlib.Feeds.glFeeds
            oFeed.sName2 = oFeed.sName
        Next

        uiListItems.ItemsSource = VBlib.Feeds.glFeeds
    End Sub

    Private Sub uiOk_Click(sender As Object, e As RoutedEventArgs)

        Dim bZmiany As Boolean = False
        For Each oItem As VBlib.JedenFeed In VBlib.Feeds.glFeeds
            If oItem.sName <> oItem.sName2 Then
                ' tylko wtedy cos zapisujemy, gdy nowa nazwa inna niz stara
                oItem.sName = oItem.sName2
                oItem.iNameType = VBlib.FeedNameType.UserDefined
                bZmiany = True
            End If
        Next

        If bZmiany Then feeds.FeedsSave(False)

        Me.Frame.GoBack()
    End Sub
End Class
