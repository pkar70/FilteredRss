' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

Imports Windows.Storage
''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class Setup
    Inherits Page

    Private Sub bSetupOk_Click(sender As Object, e As RoutedEventArgs)
        Dim iPkarInit As Boolean = False

        If uiFeeds.Text = "PKAR_init" Then
            uiFeeds.Text = "http://devil-torrents.pl/rss.php?cat=1" & vbCrLf &
            "http://devil-torrents.pl/rss.php?cat=4" & vbCrLf &
            "http://devil-torrents.pl/rss.php?cat=642" & vbCrLf &
            "http://devil-torrents.pl/rss.php?cat=3" & vbCrLf &
            "http://devil-torrents.pl/rss.php?cat=362" & vbCrLf &
            "http://devil-torrents.pl/rss.php?cat=7" & vbCrLf &
            "http://devil-torrents.pl/rss.php?cat=702" & vbCrLf &
            "http://devil-torrents.pl/rss.php?cat=8" & vbCrLf &
            "http://devil-torrents.pl/rss.php?cat=10" & vbCrLf &
            "http://devil-torrents.pl/rss.php?cat=11" & vbCrLf &
            "http://devil-torrents.pl/rss.php?cat=16" & vbCrLf &
            "http://devil-torrents.pl/rss.php?cat=699" & vbCrLf &
            "http://devil-torrents.pl/rss.php?cat=21" & vbCrLf &    ' "http://korwin-mikke.pl/blog/rss" & vbCrLf &
            "http://zikit.krakow.pl/feeds/rss/komunikaty/1794" & vbCrLf &
            "http://zikit.krakow.pl/feeds/rss/komunikaty/1829" & vbCrLf &
            "http://zikit.krakow.pl/feeds/rss/komunikaty/1787" & vbCrLf &
            "http://onlyoldmovies.blogspot.com/feeds/posts/default" & vbCrLf &
            "https://blogs.technet.microsoft.com/sysinternals/feed/" & vbCrLf &
            "http://owl.phy.queensu.ca/~phil/exiftool/rss.xml" & vbCrLf &
            "https://support.microsoft.com/app/content/api/content/feeds/sap/en-sg/6ae59d69-36fc-8e4d-23dd-631d98bf74a9/rss" & vbCrLf &
            "https://support.microsoft.com/app/content/api/content/feeds/sap/en-sg/2f11206b-c490-cd6f-e033-661968ad085c/rss" & vbCrLf &
            "https://support.microsoft.com/app/content/api/content/feeds/sap/en-sg/e9323f40-7ca8-4ecd-621d-fcf6c96a2eb0/rss" & vbCrLf &
            "https://support.microsoft.com/app/content/api/content/feeds/sap/en-sg/f3753f34-a410-4d2a-bbda-72c4abfe87d7/rss" & vbCrLf &
            "https://support.microsoft.com/app/content/api/content/feeds/sap/en-sg/73b77d93-c44a-7bf3-295c-b729cf00eb82/rss" & vbCrLf &
            "https://support.microsoft.com/app/content/api/content/feeds/sap/en-sg/7ebec7c8-1b8a-c547-bea4-cd285c103fd3/rss" & vbCrLf &
            "https://www.tomshardware.com/feeds/all" & vbCrLf &
            "http://feeds.feedburner.com/WinaeroBlog/" & vbCrLf &
            "https://lovekrakow.pl/rss/aktualnosci.html" & vbCrLf &
            "https://mobile.twitter.com/JkmMikke" & vbCrLf &
            "https://www.facebook.com/Roman-Giertych-strona-oficjalna-215392231834473" & vbCrLf &
            "https://www.instagram.com/linkaa_m/" & vbCrLf &
            "https://twitter.com/donaldtusk" & vbCrLf &
            "https://www.instagram.com/juliawieniawa/"
            iPkarInit = True
        End If

        If uiBlacklist.Text = "PKAR_init" Then
            uiBlacklist.Text = "t:(?i)audiobook" & vbCrLf &
            "t:^VA -" & vbCrLf &
            "d:(?i)gatunek:.*(thriller|karate|sportowy|prawniczy|horror)" & vbCrLf &
            "d:(?i)gatunek:.*(sztuki walki|reality show|teledyski)" & vbCrLf &
            "d:(?i)gatunek:.*(gothic|techno|punk|progressive|metal|industrial|indie)" & vbCrLf &
            "d:(?i)gatunek:.*(hard|składanki|trap|hip-hop)" & vbCrLf &
            "d:(?i)gatunek:.*(drum n Bass|electro house|uplifting trance|vocal trance)"
            iPkarInit = True
        End If

        If iPkarInit Then Exit Sub

        SetSettingsString("KnownFeeds", uiFeeds.Text, uiFeedsRoam.IsOn)
        SetSettingsString("WhiteList", uiWhitelist.Text, uiWhiteRoam.IsOn)
        SetSettingsString("BlackList", uiBlacklist.Text, uiBlackRoam.IsOn)
        SetSettingsBool("NotifyWhite", uiNotifyWhite.IsOn)
        SetSettingsBool("LinksActive", uiLinksActive.IsOn)

        Dim sTxt As String
        sTxt = uiInterval.SelectionBoxItem.ToString
        sTxt = sTxt.Replace(" min", "")
        SetSettingsInt("TimerInterval", CInt(sTxt))

        Dim iInd As Integer = uiMaxDays.SelectedIndex
        Select Case iInd
            Case 0
                iInd = 7
            Case 1
                iInd = 30
            Case 2
                iInd = 365
            Case Else   ' nie powinno sie zdarzyc...
                iInd = 30
        End Select
        SetSettingsInt("MaxDays", iInd)

        ' Await App.RegisterTriggers() ' i zrob triggery
        ' bedzie, bo w MainPage_Loaded jest register
        Me.Frame.GoBack()
    End Sub

    Private Sub Setup_Loaded(sender As Object, e As RoutedEventArgs)
        GetAppVers(uiVers)
        GetSettingsString(uiFeeds, "KnownFeeds", "")
        GetSettingsString(uiWhitelist, "WhiteList")
        GetSettingsString(uiBlacklist, "BlackList")

        GetSettingsBool(uiNotifyWhite, "NotifyWhite")
        uiRenameFeed.IsEnabled = GetSettingsBool("NotifyWhite")

        GetSettingsBool(uiLinksActive, "LinksActive")
        Select Case GetSettingsInt("TimerInterval", 30)
            Case 15
                uiInterval.SelectedIndex = 0
            Case 30
                uiInterval.SelectedIndex = 1
            Case 60
                uiInterval.SelectedIndex = 2
            Case 90
                uiInterval.SelectedIndex = 3
            Case Else
                uiInterval.SelectedIndex = 1
        End Select

        Select Case GetSettingsInt("MaxDays", 30)
            Case 7
                uiMaxDays.SelectedIndex = 0
            Case 30
                uiMaxDays.SelectedIndex = 1
            Case 365
                uiMaxDays.SelectedIndex = 2
            Case Else
                uiMaxDays.SelectedIndex = 1
        End Select

    End Sub

    Private Sub bRegExpTest_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(TestRegExp))
    End Sub

    Private Sub bRenameFeed_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(RenameFeed))
    End Sub

    Private Sub uiNotifyWhite_Toggled(sender As Object, e As RoutedEventArgs) Handles uiNotifyWhite.Toggled
        uiRenameFeed.IsEnabled = uiNotifyWhite.IsOn
    End Sub

    Private Async Sub uiClearCache_Click(sender As Object, e As RoutedEventArgs)
        uiClearCache.IsEnabled = False
        Await WebView.ClearTemporaryWebDataAsync()
        DialogBoxRes("msgCacheCleared")
        uiClearCache.IsEnabled = True
    End Sub
End Class
