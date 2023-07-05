
Imports vb14 = VBlib.pkarlibmodule14
Imports pkar.Uwp.Configs

Public NotInheritable Class MainPage
    Inherits Page

    Private inVb As New VBlib.MainPage(AddressOf UsunToast)

    Public Sub ShowPostsList()
        vb14.DumpCurrMethod()
        ' wczytuje z sAllFeeds, przerabia to na krotka liste do lewego WebView

        If VBlib.App.glItems Is Nothing Then Return

        uiCount.Text = VBlib.App.glItems.Count & " items"
        SetBadgeNo(VBlib.App.glItems.Count)

        VBlib.App.bChangedXML = False

        Select Case vb14.GetSettingsInt("uiSortOrder")
            Case 1  ' by name
                uiListItems.ItemsSource = From c In VBlib.App.glItems Order By c.sTitle
            Case 2  ' by thread
                uiListItems.ItemsSource = From c In VBlib.App.glItems Order By c.sFeedName, c.sTitle
            Case Else ' 0, i nieznane: jak dotychczas
                uiListItems.ItemsSource = From c In VBlib.App.glItems

        End Select

    End Sub

    Private Sub uiLista_Click(sender As Object, e As TappedRoutedEventArgs)
        Dim oItem As VBlib.JedenItem = TryCast(sender, Grid).DataContext
        ShowTorrentData(oItem)
    End Sub


    Public Sub ShowTorrentData(oItem As VBlib.JedenItem)
        Dim sTmp As String
        sTmp = "<html><body><h1>" & oItem.sTitle & "</h1>"
        If oItem.sDate <> "" Then sTmp = sTmp & $"<p><small>Posted: {oItem.sDate}</small></p>"
        sTmp = sTmp & oItem.sItemHtmlData
        sTmp = sTmp & "</body></html>"
        uiPost.NavigateToString(sTmp)

        If oItem.sLinkToDescr IsNot Nothing AndAlso oItem.sLinkToDescr.Contains("http") Then
            uiBLink.NavigateUri = New Uri(oItem.sLinkToDescr)
            sTmp = oItem.sLinkToDescr
            ToolTipService.SetToolTip(uiBLink, sTmp)

            'teraz różne wersje skracania pokazywanego linku
            ' (2022.02.04: tutaj dopiero zmiana, w gItems jest pełnej długości, nie ma wcześniej skracania)

            Dim iInd As Integer

            ' próba 1: ucinamy po ?xxxx (m.in. obrazki instagram w ten sposób są traktowane)
            If sTmp.Length > 30 Then
                iInd = sTmp.IndexOf("?")
                If iInd > 1 Then sTmp = sTmp.Substring(0, iInd)
            End If

            ' próba 2: ucięcie ścieżki w ramach serwera
            If sTmp.Length > 30 Then
                ' gdy link jest za długi, kasujemy ścieżkę w ramach serwera
                Dim iInd1 As Integer
                iInd = sTmp.IndexOf("/", 10)
                iInd1 = sTmp.LastIndexOf("/")
                sTmp = sTmp.Substring(0, iInd + 1) & "..." & sTmp.Substring(iInd1)
            End If

            ' próba 3: gdy sama filename jest długa
            If sTmp.Length > 30 Then
                iInd = sTmp.LastIndexOf("/")
                If sTmp.Length - iInd > 20 Then sTmp = sTmp.Substring(0, iInd + 15) & "..."
            End If
            uiBLink.Content = sTmp
        Else
            uiBLink.NavigateUri = New Uri("http://127.0.0.1/")
            uiBLink.Content = "<error>"
            ToolTipService.SetToolTip(uiBLink, "")
        End If

        vb14.SetSettingsString("sLastId", oItem.sGuid)

    End Sub

    Public Sub ShowTorrentData(sGuid As String)
        'Dim sTmp As String

        If VBlib.App.glItems Is Nothing Then
            vb14.DialogBox("IMPOSSIBLE, ShowTorrentData and oAllItems null")
            Exit Sub
        End If

        If sGuid = "" Then Return   ' gdy byl pusty GUID (zbiorcze Toasty)

        'DialogBox("ShowTorrentData(" & sGuid)

        For Each oItem In VBlib.App.glItems
            If oItem.sGuid = sGuid Then
                ShowTorrentData(oItem)
                Exit Sub
            End If
        Next

        vb14.DialogBox("IMPOSSIBLE? Cannot find such item")
        Exit Sub

    End Sub


    ' obsluga zdarzen formatki
    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)

        'CrashMessageInit()
        Await CrashMessageShowAsync()   ' jesli cos bylo, to pokaze tresc

        AppResuming()   ' wczytaj listę RSSów
        Await App.RegisterTriggers()

        ' poniewaz nie dziala językowanie w tle, to trzeba teraz przepisac
        'SetSettingsString("resDelete", GetLangString("resDelete"))
        'SetSettingsString("resOpen", GetLangString("resOpen"))
        'SetSettingsString("resBrowser", GetLangString("resBrowser"))
        'SetSettingsString("resNewItemsInFeed", GetLangString("resNewItemsInFeed"))
        'SetSettingsString("resNewItemsList", GetLangString("resNewItemsList"))

    End Sub
    Private Sub Form_Resized(sender As Object, e As SizeChangedEventArgs)
        'uiLista.Width = uiNaList.ActualWidth - 20
        'uiPost.Width = uiNaPost.ActualWidth - 20
        'uiLista.Height = uiNaViews.ActualHeight - 20
        'uiPost.Height = uiNaViews.ActualHeight - 20

        If uiNaViews.ActualWidth < 200 Then
            tbLastRead.Width = 0
        ElseIf uiNaViews.ActualWidth < 600 Then
            tbLastRead.Width = 100
        Else
            tbLastRead.Width = uiNaViews.ActualWidth / 2    ' nie wiecej niż pół szerokości
        End If
        'uiImage.Source = New ImageSource()
        '    Image img = sender as Image; 
        'BitmapImage BitmapImage = New BitmapImage();
        'img.Width = BitmapImage.DecodePixelWidth = 80; //natural px width Of image source
        '// don't need to set Height, system maintains aspect ratio, and calculates the other
        '// dimension, so long as one dimension measurement Is provided
        'BitmapImage.UriSource = New Uri(img.BaseUri, "Images/myimage.png");
    End Sub


    Private Sub Page_GotFocus(sender As Object, e As RoutedEventArgs)
        vb14.DumpCurrMethod()
        uiClockRead.IsChecked = vb14.GetSettingsBool("autoRead")

        ' 20171101: jeśli w środku jest moje, to nie rób reload
        'Dim sHtml As String = ""

        'Try
        '    sHtml = Await uiLista.InvokeScriptAsync("eval", New String() {"document.documentElement.outerHTML;"})
        'Catch ex As Exception
        '    ' jesli strona jest pusta, jest Exception
        'End Try

        'If sHtml.IndexOf("<!-- FilteredRSS -->") < 1 Or GetSettingsBool("ChangedXML") Then ShowPostsList()
        If VBlib.App.bChangedXML Then ShowPostsList()

        tbLastRead.Text = vb14.GetSettingsString("lastRead")

    End Sub

#Region "commandbar"

    Public Async Sub bReadFeed_Click(sender As Object, e As RoutedEventArgs)
        vb14.DumpCurrMethod()

        Dim sTmp As String = Await App.ReadFeeds(tbLastRead)

        Try
            tbLastRead.Text = sTmp
            ShowPostsList()
        Catch ex As Exception
        End Try

    End Sub
    Private Sub uiClockRead_Click(sender As Object, e As RoutedEventArgs) Handles uiClockRead.Click
        uiClockRead.SetSettingsBool()
        If uiClockRead.IsChecked AndAlso Not IsTriggersRegistered("FilteredRSStimer") Then App.RegisterTriggers()
    End Sub

    Private Sub bSetup_Click(sender As Object, e As RoutedEventArgs)
        'If uiNaViews.ActualWidth < 700 Then
        '    Me.Frame.Navigate(GetType(SmallSetup))
        'Else
        Me.Navigate(GetType(Setup))
        'End If
    End Sub

    Private Sub bDelOnePost_Click(sender As Object, e As RoutedEventArgs)
        vb14.DumpCurrMethod()

        Dim sGuid As String = vb14.GetSettingsString("sLastId")
        If sGuid = "" Then Exit Sub

        ' 20171101: ponieważ czasem seria kasowania powoduje crash - może dlatego że nie skończy poprzedniego kasowania jak zaczyna następne?
        uiDelPost.IsEnabled = False

        Dim sNextGuid As String = App.DelOnePost(sGuid)

        Try
            ShowPostsList()
        Catch ex As Exception
        End Try

        If sNextGuid <> "" Then ShowTorrentData(sNextGuid)

        uiDelPost.IsEnabled = True

    End Sub

    Private Sub bDelAllPosts_Click(sender As Object, e As RoutedEventArgs)
        vb14.DumpCurrMethod()

        vb14.SetSettingsString("sLastId", "")

        Windows.UI.Notifications.ToastNotificationManager.History.Clear()

        VBlib.App.glItems.Clear()
        ShowPostsList()
    End Sub

    Private Sub bInfo_Click(sender As Object, e As RoutedEventArgs)
        Me.Navigate(GetType(Info))
    End Sub

#End Region

    ' obsluga Suspend/Resume (save data)
    ' https://docs.microsoft.com/en-us/windows/uwp/launch-resume/suspend-an-app
    ' oraz pliki
    ' https://docs.microsoft.com/en-us/windows/uwp/app-settings/store-and-retrieve-app-data
    Public Sub New()
        InitializeComponent()
        AddHandler Application.Current.Suspending, AddressOf AppSuspending
        AddHandler Application.Current.Resuming, AddressOf AppResuming
    End Sub

    Public Sub AppSuspending()
        vb14.DumpCurrMethod()
        App.SaveIndex(True)
    End Sub

    Public Sub AppResuming()
        App.LoadIndex()
        App.KillFileLoad(False)
        ShowPostsList()
    End Sub


    Private Sub uiPost_NavigationStarted(sender As WebView, args As WebViewNavigationStartingEventArgs) Handles uiPost.NavigationStarting
        If args.Uri Is Nothing Then Exit Sub

        args.Cancel = True
#Disable Warning BC42358 ' Because this call is not awaited, execution of the current method continues before the call is completed
        Windows.System.Launcher.LaunchUriAsync(args.Uri)
#Enable Warning BC42358

    End Sub

    Private Sub uiBLink_RightTapped(sender As Object, e As RightTappedRoutedEventArgs) Handles uiBLink.RightTapped
        uiBLink.NavigateUri.OpenBrowser(True)
    End Sub

    Public Function UsunToast(sGuid As String) As Boolean

        Dim sTag As String = VBlib.App.ConvertGuidToTag(sGuid)
        If sTag = "" Then Return False
        Try
            Windows.UI.Notifications.ToastNotificationManager.History.Remove(sTag)
        Catch ex As Exception
            ' chyba jeśli nie ma takowego - więc już usunięty jest
        End Try
        Return True
    End Function

#Region "ItemOnList-contextMenu"
    Private Async Sub DeleteFromContextMenu(oMFI As MenuFlyoutItem, iMode As Integer)
        vb14.DumpCurrMethod()
        Dim oItem As VBlib.JedenItem = TryCast(oMFI?.DataContext, VBlib.JedenItem)
        If oItem Is Nothing Then Return

        Await inVb.DeleteFromContextMenu(oItem, iMode)

        Try
            ShowPostsList()
        Catch ex As Exception
        End Try

    End Sub

    Private Sub uiDelThis_Click(sender As Object, e As RoutedEventArgs)
        DeleteFromContextMenu(sender, 1)
    End Sub

    Private Sub uiDelSubject_Click(sender As Object, e As RoutedEventArgs)
        DeleteFromContextMenu(sender, 2)
    End Sub

    Private Sub uiDelFeed_Click(sender As Object, e As RoutedEventArgs)
        DeleteFromContextMenu(sender, 3)
    End Sub

    Private Sub uiKillFile_Click(sender As Object, e As RoutedEventArgs)
        DeleteFromContextMenu(sender, 4)
    End Sub

    Private Sub uiDelDoTego_Click(sender As Object, e As RoutedEventArgs)
        DeleteFromContextMenu(sender, 5)
    End Sub

#End Region

End Class
