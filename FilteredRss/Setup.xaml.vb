
Imports vb14 = VBlib.pkarlibmodule14
Imports pkar.Uwp.Configs

Public NotInheritable Class Setup
    Inherits Page

    Private inVb As New VBlib.Setup

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Me.ShowAppVers()

        'WyliczWidthDlaCombo()

        ' GetSettingsString(uiFeeds, "KnownFeeds", "")
        feeds.FeedsLoad()
        For Each oFeed As VBlib.JedenFeed In VBlib.Feeds.glFeeds
            oFeed.sName2 = oFeed.sName
        Next
        uiListItems.ItemsSource = VBlib.Feeds.glFeeds

        uiWhitelist.GetSettingsString("WhiteList")
        uiBlacklist.GetSettingsString("BlackList")
        uiEditGlobalWhiteBlack.IsEnabled = False

        ' GetSettingsBool(uiNotifyWhite, "NotifyWhite")
        'uiRenameFeed.IsEnabled = GetSettingsBool("NotifyWhite")    ' czemu takie uzależnienie było?

        uiLinksActive.GetSettingsBool("LinksActive")
        Select Case vb14.GetSettingsInt("TimerInterval")
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

        uiSortOrder.GetSettingsInt()

        ' teraz jest per Feed
        'Select Case GetSettingsInt("MaxDays", 30)
        '    Case 7
        '        uiMaxDays.SelectedIndex = 0
        '    Case 30
        '        uiMaxDays.SelectedIndex = 1
        '    Case 365
        '        uiMaxDays.SelectedIndex = 2
        '    Case Else
        '        uiMaxDays.SelectedIndex = 1
        'End Select

    End Sub

    Private Sub RefreshnijListe()
        uiListItems.ItemsSource = Nothing
        uiListItems.ItemsSource = VBlib.Feeds.glFeeds
    End Sub

#Region "command bar"

    Private Sub bSetupOk_Click(sender As Object, e As RoutedEventArgs)


        Dim sTmp As String = VBlib.Feeds.TryInitMyDefaultsBlackList(uiBlacklist.Text)
        If sTmp <> "" Then
            uiBlacklist.Text = sTmp
            Exit Sub
        End If

        For Each oFeed As VBlib.JedenFeed In VBlib.Feeds.glFeeds
            If oFeed.sName2 <> oFeed.sName Then
                oFeed.sName = oFeed.sName2
                oFeed.iNameType = VBlib.FeedNameType.UserDefined
            End If
        Next
        feeds.FeedsSave(uiFeedsRoam.IsOn)

        If uiWhitelist.DataContext Is Nothing Then
            ' tylko gdy to globalne
            uiWhitelist.SetSettingsString("WhiteList", uiWhiteRoam.IsOn)
            uiWhitelist.SetSettingsString("BlackList", uiBlackRoam.IsOn)
        Else
            Dim oItem As VBlib.JedenFeed = TryCast(uiWhitelist.DataContext, VBlib.JedenFeed)
            oItem.sWhitelist = uiWhitelist.Text
            oItem.sBlacklist = uiBlacklist.Text

        End If

        ' SetSettingsBool("NotifyWhite", uiNotifyWhite.IsOn)
        uiLinksActive.SetSettingsBool("LinksActive")

        Dim sTxt As String
        sTxt = uiInterval.SelectionBoxItem.ToString
        sTxt = sTxt.Replace(" min", "")
        vb14.SetSettingsInt("TimerInterval", CInt(sTxt))

        uiSortOrder.SetSettingsInt()

        ' teraz jest per Feed
        'Dim iInd As Integer = uiMaxDays.SelectedIndex
        'Select Case iInd
        '    Case 0
        '        iInd = 7
        '    Case 1
        '        iInd = 30
        '    Case 2
        '        iInd = 365
        '    Case Else   ' nie powinno sie zdarzyc...
        '        iInd = 30
        'End Select
        'SetSettingsInt("MaxDays", iInd)

        ' Await App.RegisterTriggers() ' i zrob triggery
        ' bedzie, bo w MainPage_Loaded jest register
        Me.GoBack()
    End Sub

    Private Sub bRegExpTest_Click(sender As Object, e As RoutedEventArgs)
        Me.Navigate(GetType(TestRegExp))
    End Sub

    Private Async Sub uiClearCache_Click(sender As Object, e As RoutedEventArgs)
        uiClearCache.IsEnabled = False
        Await WebView.ClearTemporaryWebDataAsync()
        vb14.DialogBoxRes("msgCacheCleared")
        uiClearCache.IsEnabled = True
    End Sub

#End Region

    Private Sub uiDelFeed_Click(sender As Object, e As RoutedEventArgs)
        Dim oItem As VBlib.JedenFeed = Sender2Feed(sender)

        VBlib.Feeds.glFeeds.Remove(oItem)
        RefreshnijListe()
    End Sub

    Private Async Sub uiAddFeed_Click(sender As Object, e As RoutedEventArgs)
        Dim sNewFeed As String = Await vb14.DialogBoxInputResAsync("resNewFeed")
        If sNewFeed = "" Then Return

        inVb.GetNewFeed(sNewFeed)

        RefreshnijListe()
    End Sub

#Region "feedContextMenu"


    Private Sub uiSetToastType_Click(sender As Object, e As RoutedEventArgs)
        ' zmiana typu toastu
        Dim oDFE As FrameworkElement = sender
        Dim oItem As VBlib.JedenFeed = TryCast(oDFE.DataContext, VBlib.JedenFeed)

        Dim iNum As Integer = oDFE.Name.Substring(oDFE.Name.Length - 1, 1)
        oItem.iToastType = iNum
        ' oItem.sToastType = vb14.GetLangString("resToastType" & oItem.iToastType) <- jest juz konwerter
        RefreshnijListe()
    End Sub

    Private Async Sub uiEditWhiteBlack(sender As Object, e As RoutedEventArgs)
        Await PokazWhiteBlackList(Sender2Feed(sender))
    End Sub

    Private Sub uiGoWeb(sender As Object, e As RoutedEventArgs)
        Dim oUri = New Uri(Sender2Feed(sender).sUri)
        oUri.OpenBrowser
    End Sub

    Private Sub uiResetSeen(sender As Object, e As RoutedEventArgs)
        Dim oItem As VBlib.JedenFeed = Sender2Feed(sender)
        If oItem Is Nothing Then Return
        oItem.sLastGuid = ""
        oItem.sLastGuids = ""
    End Sub


#End Region

    Private Sub uiBlacklist_LostFocus(sender As Object, e As RoutedEventArgs) Handles uiBlacklist.LostFocus

    End Sub

    Private Sub uiWhitelist_LostFocus(sender As Object, e As RoutedEventArgs) Handles uiWhitelist.LostFocus
        If uiWhitelist.DataContext Is Nothing Then
            ' tylko gdy to globalne

        End If

    End Sub


    Private Async Function PokazWhiteBlackList(oFeed As VBlib.JedenFeed) As Task

        If uiWhitelist.DataContext Is Nothing Then
            Dim bChanged As Boolean = False
            If vb14.GetSettingsString("WhiteList") <> uiWhitelist.Text Then bChanged = True
            If vb14.GetSettingsString("BlackList") <> uiBlacklist.Text Then bChanged = True
            If bChanged Then
                If Await vb14.DialogBoxResYNAsync("msgWhiteBlackChanged") Then
                    uiWhitelist.SetSettingsString("WhiteList", uiWhiteRoam.IsOn)
                    uiBlacklist.SetSettingsString("BlackList", uiBlackRoam.IsOn)
                End If
            End If
        Else
            Dim oItem As VBlib.JedenFeed = TryCast(uiWhitelist.DataContext, VBlib.JedenFeed)

            Dim bChanged As Boolean = False
            If oItem.sWhitelist <> uiWhitelist.Text Then bChanged = True
            If oItem.sBlacklist <> uiBlacklist.Text Then bChanged = True
            If bChanged Then
                If Await vb14.DialogBoxResYNAsync("msgWhiteBlackChanged") Then
                    oItem.sWhitelist = uiWhitelist.Text
                    oItem.sBlacklist = uiBlacklist.Text
                End If
            End If
        End If

        If oFeed Is Nothing Then
            uiWhitelist.GetSettingsString("WhiteList")
            uiBlacklist.GetSettingsString("BlackList")
            uiEditGlobalWhiteBlack.IsEnabled = False
            uiEditGlobalWhiteBlack.IsChecked = True
            uiWhitelist.DataContext = Nothing
            uiWhitelist.Header = "Whitelist:"
            uiBlacklist.Header = "Blacklist:"
        Else
            uiWhitelist.Text = oFeed.sWhitelist
            uiBlacklist.Text = oFeed.sBlacklist
            uiEditGlobalWhiteBlack.IsEnabled = True
            uiEditGlobalWhiteBlack.IsChecked = False
            uiWhitelist.DataContext = oFeed
            uiWhitelist.Header = $"Whitelist for {oFeed.sName}:"
            uiBlacklist.Header = $"Blacklist for {oFeed.sName}:"
        End If


    End Function

    Private Function Sender2Feed(sender As Object) As VBlib.JedenFeed
        Dim oDFE As FrameworkElement = sender
        Dim oItem As VBlib.JedenFeed = TryCast(oDFE.DataContext, VBlib.JedenFeed)
        Return oItem
    End Function


    Private Async Sub uiEditGlobalWhiteBlack_Click(sender As Object, e As RoutedEventArgs) Handles uiEditGlobalWhiteBlack.Click
        Await PokazWhiteBlackList(Nothing)
    End Sub


    Private Sub uiSetMaxDays_Click(sender As Object, e As RoutedEventArgs)
        Dim oDFE As FrameworkElement = sender
        Dim oItem As VBlib.JedenFeed = TryCast(oDFE.DataContext, VBlib.JedenFeed)
        If oItem Is Nothing Then Return

        Dim sIleDni As String = oDFE.Name.Replace("uiSetMaxDays", "")   ' 7 / 30 / 365
        oItem.iMaxDays = sIleDni
        RefreshnijListe()
    End Sub
End Class


Public Class KonwerterWasError
    Implements IValueConverter

    ' Define the Convert method to change a DateTime object to
    ' a month string.
    Public Function Convert(ByVal value As Object,
        ByVal targetType As Type, ByVal parameter As Object,
        ByVal language As System.String) As Object _
        Implements IValueConverter.Convert

        Dim bWasError As Boolean = CType(value, Boolean)

        If Not bWasError Then Return ""

        Return "!"

    End Function

    ' ConvertBack is not implemented for a OneWay binding.
    Public Function ConvertBack(ByVal value As Object,
        ByVal targetType As Type, ByVal parameter As Object,
        ByVal language As System.String) As Object _
        Implements IValueConverter.ConvertBack

        Throw New NotImplementedException

    End Function
End Class

Public Class KonwerterToastType
    Implements IValueConverter

    ' Define the Convert method to change a DateTime object to
    ' a month string.
    Public Function Convert(ByVal value As Object,
        ByVal targetType As Type, ByVal parameter As Object,
        ByVal language As System.String) As Object _
        Implements IValueConverter.Convert

        Dim iTmp As Integer = CType(value, Integer)

        Return vb14.GetLangString("resToastType" & iTmp)

    End Function

    ' ConvertBack is not implemented for a OneWay binding.
    Public Function ConvertBack(ByVal value As Object,
        ByVal targetType As Type, ByVal parameter As Object,
        ByVal language As System.String) As Object _
        Implements IValueConverter.ConvertBack

        Throw New NotImplementedException

    End Function
End Class

