' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class SmallSetup
    Inherits Page

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        uiVers.Text = GetAppVers()

        uiNotifyWhite.IsOn = GetSettingsBool("NotifyWhite")
        uiRenameFeed.IsEnabled = GetSettingsBool("NotifyWhite")

        uiLinksActive.IsOn = GetSettingsBool("LinksActive")
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

    Private Sub uiFeeds_Click(sender As Object, e As RoutedEventArgs) Handles uiFeeds.Click
        Me.Frame.Navigate(GetType(SmallSeupFeeds))
    End Sub

    Private Sub uiBlack_Click(sender As Object, e As RoutedEventArgs) Handles uiBlack.Click
        Me.Frame.Navigate(GetType(SmallSetupBlack))
    End Sub

    Private Sub uiWhite_Click(sender As Object, e As RoutedEventArgs) Handles uiWhite.Click
        Me.Frame.Navigate(GetType(SmallSetupWhite))
    End Sub

    Private Sub bSetupOk_Click(sender As Object, e As RoutedEventArgs)
        Dim iPkarInit As Boolean = False

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

    Private Async Sub uiClearCache_Click(sender As Object, e As RoutedEventArgs)
        uiClearCache.IsEnabled = False
        Await WebView.ClearTemporaryWebDataAsync()
        DialogBoxRes("msgCacheCleared")
        uiClearCache.IsEnabled = True
    End Sub

    Private Sub bRenameFeed_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(RenameFeed))
    End Sub
End Class
