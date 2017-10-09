' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

Imports Windows.Storage
''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class Setup
    Inherits Page

    Private Async Sub bSetupOk_Click(sender As Object, e As RoutedEventArgs)
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
            "http://devil-torrents.pl/rss.php?cat=21" & vbCrLf &
            "http://korwin-mikke.pl/blog/rss" & vbCrLf &
            "http://zikit.krakow.pl/feeds/rss/komunikaty/1794" & vbCrLf &
            "http://zikit.krakow.pl/feeds/rss/komunikaty/1829" & vbCrLf &
            "http://zikit.krakow.pl/feeds/rss/komunikaty/1787"
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

        App.SetSettingsString("KnownFeeds", uiFeeds.Text, uiFeedsRoam.IsOn)
        App.SetSettingsString("WhiteList", uiWhitelist.Text, uiWhiteRoam.IsOn)
        App.SetSettingsString("BlackList", uiBlacklist.Text, uiBlackRoam.IsOn)
        App.SetSettingsBool("NotifyWhite", uiNotifyWhite.IsOn)
        App.SetSettingsBool("LinksActive", uiLinksActive.IsOn)

        Dim sTxt As String
        sTxt = uiInterval.SelectionBoxItem.ToString
        sTxt = sTxt.Replace(" min", "")
        App.SetSettingsInt("TimerInterval", CInt(sTxt))
        Await App.RegisterTriggers() ' i zrob triggery
        Me.Frame.GoBack()
    End Sub

    Private Sub Setup_Loaded(sender As Object, e As RoutedEventArgs)
        uiFeeds.Text = App.GetSettingsString("KnownFeeds", "")
        uiWhitelist.Text = App.GetSettingsString("WhiteList")
        uiBlacklist.Text = App.GetSettingsString("BlackList")
        uiNotifyWhite.IsOn = App.GetSettingsBool("NotifyWhite")
        uiLinksActive.IsOn = App.GetSettingsBool("LinksActive")
        Select Case App.GetSettingsInt("TimerInterval", 30)
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

    End Sub

    Private Sub bRegExpTest_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(TestRegExp))
    End Sub
End Class
