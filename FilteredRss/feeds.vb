
Public Module feeds

    Public Function FeedsLoad(Optional bForce As Boolean = False) As Boolean
        Dim bRet As Boolean = VBlib.Feeds.FeedsLoad(Windows.Storage.ApplicationData.Current.LocalFolder.Path,
                Windows.Storage.ApplicationData.Current.RoamingFolder.Path, bForce)
        Return bRet
        'If Not bRet Then Return False

        'Dim iActiveLinks As Integer = 0
        'If vb14.GetSettingsBool("LinksActive") Then iActiveLinks = 1
        'Dim iNotifyWhite As Integer = 0
        'If vb14.GetSettingsBool("NotifyWhite") Then iNotifyWhite = 1

        '' przepisanie globalnej zmiennej tam, gdzie nie było ustawienia lokalnego
        '' oraz zmiennych które były w Settings
        'For Each oItem As VBlib.JedenFeed In VBlib.Feeds.glFeeds
        '    Dim sGuidsValueName As String = VBlib.App.Url2VarName(oItem.sUri)

        '    If oItem.iLinksActive = -1 Then oItem.iLinksActive = iActiveLinks
        '    If oItem.iNotifyWhite = -1 Then oItem.iNotifyWhite = iNotifyWhite
        '    If oItem.iMaxDays = -1 Then oItem.iMaxDays = vb14.GetSettingsInt("MaxDays")
        '    If oItem.sLastOkDate Is Nothing Then    ' dla kontroli czy pokazywać "zdechnięcie" feed
        '        oItem.sLastOkDate = vb14.GetSettingsString("TIME" & sGuidsValueName)
        '    End If
        '    If oItem.sUri.Contains("devil-torrent") Then
        '        If oItem.sLastGuid = "" Then oItem.sLastGuid = vb14.GetSettingsInt("iLastRssGuid")
        '    End If
        '    If oItem.sLastGuids = "" Then oItem.sLastGuids = vb14.GetSettingsString(sGuidsValueName)
        '    oItem.sGlobalBlack = vb14.GetSettingsString("BlackList")
        '    oItem.sGlobalWhite = vb14.GetSettingsString("WhiteList")
        'Next

        'Return True
    End Function

    Public Sub FeedsSave(bRoam As Boolean)

        If bRoam Then
            VBlib.Feeds.FeedsSave(Windows.Storage.ApplicationData.Current.RoamingFolder.Path)
        Else
            VBlib.Feeds.FeedsSave(Windows.Storage.ApplicationData.Current.LocalFolder.Path)
        End If

    End Sub

End Module


