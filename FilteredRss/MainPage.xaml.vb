
' 2021.10.18
' * podczas KillFileLoad, zabezpieczenie przed throw (typu brak przekodowania z Unicode do CodePage)
' * i ta jedna funkcja z DebugOut na Dump

' 2021.06.15
' * KILLFILE: 30 dni pamiętane entries

' 2021.06.08
' * aktualizacja pkarModule, a więc RemSys i CmdLine

' 2020.11.08
' * gdy InTimer, to DelPost nie robi zapisu indeksu (zeby nie narazac sie na rownolegly FileWrite)

' 2020.11.07
' * poprawka - usuwanie ostatniego Toastu nie wylatuje z błędem (w jego efekcie: toast znikał, ale element zostawał w liście)

' 2020.11.06
' * przejscie na "globalny" pkarmodule (w tym także Triggers z biblioteki)
' * Twitter, FaceBook - try/catch w nich, z MakeToast jak będzie Exception - bo coś wylatuje z błędem nie wiem czemu

' 2020.10.16
' * poprawka do FaceBook - wylatywało z błędem (Giertych), teraz błąd obsługuje. Bez aktualizji STORE - bo obsługa FaceBook nie jest oficjalna.

' 2020.04.22
' * facebook: poprawka linku 'czytaj dalej' (gdy jest do oddzielnej strony, a nie tylko rozwinięcie?)
' * setup: odświeżenie listy feed w PKAR_init 

' WERSJA STORE 2004

' 2020.04.14 
' * obsługa Facebook
' * dla małych rozdzielczości (width <500 px) inny setup (rozbity na podstrony), "mama.jaga.init" wstawia Tuska i Giertycha
' * na stronie Setup (pełnej, oraz głównej małej) podawany jest numer wersji
' * BackButton (brakowało AddHandler, funkcje były!)
' 2020.04.15
' * obsługa Instagram
' * kolejna możliwość skracania linku pokazywanego na ekranie
' 2020.04.14 
' * obsługa Twittera
' * wydzielone dwie funkcje (ShouldShowEmptyFeedWarning, ExtractPicLink) z AddFeedItemsSyndic (uczytelnienie kodu)
' * dodaję UserAgent = FilteredRSS, z UserAgent=NULL był błąd w LoveKraków
' 2020.01.24 TomsHardware - uwzględnienie (przy skracaniu linków) że to nie musi być /news/, może być np. /reviews/

' wersja 1912
' 2019.08.07 gdy oNode.Title jest Nothing, nie wylatuje z błędem tylko "(no title)" - przypadek Korwina

' wersja 1908
' 2019.07.01 dodaje pkarmodule, więc i BackButton (niepelna likwidacja z App powtorek)
' 2019.07.01 MainPage:lista postów:contextMenu: usuwanie postu, feed, oraz postów wedle tytułu

' TODO: czasem znika, z utratą zawartosci cache!

' ZMIANY 2019.03.20
'   - przerobienie listy postów z WebView do ListView
'   - pokazuje drobnym druczkiem z jakiego feed pochodzi post

' ZMIANY 2019.03.13
' support.microsoft
'   - krótsze ID robi, więc powinno więcej pamiętać
'   - podobnie jak do Devil-Torrents, id nie może być 200 do tyłu
' onlyoldmovies - zmiana node.id, teraz powinno byc klikalne

' v4.1904 (zmiana stylu numerowania):
' Inny mechanizm prezentowania listy wpisów (ListView, zamiast WebView); teraz każdy wpis ma także etykietkę nazwy feed.

' Wersja 3.0.0:
' 1. Umie już obsługiwać Atom (poprzednio umiała jedynie RSS)
'2. Całkowicie przebudowana obsługa powiadomień (toast), teraz są one pogrupowane wedle feed, oraz można z nich wykonać podstawowe akcje (np. skasować pozycję z listy, otworzyć w przeglądarce)
'3. Podczas definiowania whitelist, można użyć prefiksu f: z nazwą feed
'4. Można zmienić nazwę feed na coś bardziej znaczącego

'Wersja 2.1.0:
'1. Obsługa wyrażeń regularnych (regexp)
'2. Mniejsze "migotanie" przy przełączaniu aktywnych okien
'2. Lepsza obsługa pracy w tle

' Wersja 1.1.3:
' 1. poprawione kasowanie pojedynczego postu;
' 2. link z GUID jest skracany, tak by się zmieścił na ekranie (np. blog Korwina-Mikke czy krakowski ZIKiT mają długie linki)



Public NotInheritable Class MainPage
    Inherits Page

    Public Sub ShowPostsList()
        ' wczytuje z sAllFeeds, przerabia to na krotka liste do lewego WebView

        uiCount.Text = App.glItems.Count & " items"
        SetBadgeNo(App.glItems.Count)

        '        SetSettingsBool("ChangedXML", False)
        uiListItems.ItemsSource = From c In App.glItems
    End Sub

    Private Sub uiLista_Click(sender As Object, e As TappedRoutedEventArgs)
        Dim oItem As JedenItem = TryCast(sender, Grid).DataContext
        ShowTorrentData(oItem)
    End Sub


    Public Sub ShowTorrentData(oItem As JedenItem)
        Dim sResult As String
        sResult = "<html><body><h1>" & oItem.sTitle & "</h1>"
        If oItem.sDate <> "" Then sResult = sResult & "<p><small>Posted: " & oItem.sDate & "</small></p>"
        sResult = sResult & oItem.sItemHtmlData
        sResult = sResult & "</body></html>"
        uiPost.NavigateToString(sResult)

        If oItem.sLinkToDescr IsNot Nothing AndAlso oItem.sLinkToDescr.Contains("http") Then
            uiBLink.NavigateUri = New Uri(oItem.sLinkToDescr)
            sResult = oItem.sLinkToDescr
            If sResult.Length > 30 Then
                Dim iInd, iInd1 As Integer
                iInd = sResult.IndexOf("/", 10)
                iInd1 = sResult.LastIndexOf("/")
                sResult = sResult.Substring(0, iInd + 1) & "..." & sResult.Substring(iInd1)
                If sResult.Length > 30 Then
                    ' jeszcze jedna próba (pod kątem obrazków Instagram
                    iInd = sResult.IndexOf("?")
                    If iInd > 0 Then sResult = sResult.Substring(0, iInd)
                End If
            End If
            uiBLink.Content = sResult
        Else
            uiBLink.NavigateUri = New Uri("http://127.0.0.1/")
            uiBLink.Content = "<error>"
        End If

        SetSettingsString("sLastId", oItem.sGuid)

    End Sub

    Public Sub ShowTorrentData(sGuid As String)
        'Dim sTmp As String

        If App.glItems Is Nothing Then
            DialogBox("IMPOSSIBLE, ShowTorrentData and oAllItems null")
            Exit Sub
        End If

        'DialogBox("ShowTorrentData(" & sGuid)

        For Each oItem In App.glItems
            If oItem.sGuid = sGuid Then
                ShowTorrentData(oItem)
                Exit Sub
            End If
        Next

        DialogBox("IMPOSSIBLE? Cannot find such item")
        Exit Sub

    End Sub


    ' obsluga zdarzen formatki
    Private Async Sub MainPage_Loaded(sender As Object, e As RoutedEventArgs)
        CrashMessageInit()
        Await CrashMessageShowAsync()   ' jesli cos bylo, to pokaze tresc

        AppResuming()   ' wczytaj listę RSSów
        Await App.RegisterTriggers()

        ' poniewaz nie dziala językowanie w tle, to trzeba teraz przepisac
        SetSettingsString("resDelete", GetLangString("resDelete"))
        SetSettingsString("resOpen", GetLangString("resOpen"))
        SetSettingsString("resBrowser", GetLangString("resBrowser"))

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

        uiClockRead.IsChecked = GetSettingsBool("autoRead")

        ' 20171101: jeśli w środku jest moje, to nie rób reload
        'Dim sHtml As String = ""

        'Try
        '    sHtml = Await uiLista.InvokeScriptAsync("eval", New String() {"document.documentElement.outerHTML;"})
        'Catch ex As Exception
        '    ' jesli strona jest pusta, jest Exception
        'End Try

        'If sHtml.IndexOf("<!-- FilteredRSS -->") < 1 Or GetSettingsBool("ChangedXML") Then ShowPostsList()
        If GetSettingsBool("ChangedXML") Then ShowPostsList()

        tbLastRead.Text = GetSettingsString("lastRead")

    End Sub

    ' obsluga guzikow
    Public Async Sub bReadFeed_Click(sender As Object, e As RoutedEventArgs)
        Dim sTmp As String = Await App.ReadFeed(tbLastRead)

        Try
            tbLastRead.Text = sTmp
            ShowPostsList()
        Catch ex As Exception
        End Try

    End Sub
    Private Sub uiClockRead_Click(sender As Object, e As RoutedEventArgs) Handles uiClockRead.Click
        SetSettingsBool("autoRead", uiClockRead.IsChecked)
    End Sub

    Private Sub bSetup_Click(sender As Object, e As RoutedEventArgs)
        If uiNaViews.ActualWidth < 700 Then
            Me.Frame.Navigate(GetType(SmallSetup))
        Else
            Me.Frame.Navigate(GetType(Setup))
        End If
    End Sub

    Private Sub bDelOnePost_Click(sender As Object, e As RoutedEventArgs)

        Dim sGuid As String = GetSettingsString("sLastId")
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
        SetSettingsString("sLastId", "")

        Windows.UI.Notifications.ToastNotificationManager.History.Clear()

        App.glItems.Clear()
        ShowPostsList()
    End Sub


    ' obsluga Suspend/Resume (save data)
    ' https://docs.microsoft.com/en-us/windows/uwp/launch-resume/suspend-an-app
    ' oraz pliki
    ' https://docs.microsoft.com/en-us/windows/uwp/app-settings/store-and-retrieve-app-data
    Public Sub New()
        InitializeComponent()
        AddHandler Application.Current.Suspending, AddressOf AppSuspending
        AddHandler Application.Current.Resuming, AddressOf AppResuming
    End Sub

    Public Async Sub AppSuspending()
        ' tu kiedys było, wedle DeveloperDashboard, fail with FileNotFound (= zła nazwa) ???
        'Dim sampleFile As StorageFile = Await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(
        '    "oAllItems.xml", CreationCollisionOption.ReplaceExisting)
        'Await App.oAllItems.GetXmlDocument(SyndicationFormat.Rss20).SaveToFileAsync(sampleFile)
        Await App.SaveIndex(True)
    End Sub

    Public Async Sub AppResuming()
        Await App.LoadIndex()
        Await App.KillFileLoad(False)
        ShowPostsList()
    End Sub

    Private Sub bInfo_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(Info))
    End Sub

    Private Sub uiPost_NavigationStarted(sender As WebView, args As WebViewNavigationStartingEventArgs) Handles uiPost.NavigationStarting
        If args.Uri Is Nothing Then Exit Sub

        args.Cancel = True
#Disable Warning BC42358 ' Because this call is not awaited, execution of the current method continues before the call is completed
        Windows.System.Launcher.LaunchUriAsync(args.Uri)
#Enable Warning BC42358

    End Sub

    Private Sub uiBLink_RightTapped(sender As Object, e As RightTappedRoutedEventArgs) Handles uiBLink.RightTapped
        OpenBrowser(uiBLink.NavigateUri, True)
    End Sub

    Private Async Sub DeleteFromContextMenu(oMFI As MenuFlyoutItem, iMode As Integer)
        If oMFI Is Nothing Then Return
        Dim oItem As JedenItem = TryCast(oMFI.DataContext, JedenItem)
        If oItem Is Nothing Then Return


        Select Case iMode
            Case 1  ' jeden post
                App.glItems.Remove(oItem)
            Case 2  ' wedle tematu - ask!
                Dim sSubj As String = Await DialogBoxInputResAsync("msgSubjectToRemove", oItem.sTitle)
                If sSubj = "" Then Return

                For iLoop As Integer = App.glItems.Count - 1 To 0 Step -1
                    If App.glItems.Item(iLoop).sTitle.Contains(sSubj) Then App.glItems.RemoveAt(iLoop)
                Next

            Case 3  ' all from feed - askYN
                If Not Await DialogBoxResYNAsync("msgDelAllFromThisFeed") Then Return
                For iLoop As Integer = App.glItems.Count - 1 To 0 Step -1
                    If oItem.sFeedName = App.glItems.Item(iLoop).sFeedName Then App.glItems.RemoveAt(iLoop)
                Next
            Case 4  ' kill file
                Dim sKill As String = Await DialogBoxInputDirectAsync(GetLangString("msgKillFile14days"), oItem.sTitle)
                If String.IsNullOrEmpty(sKill) Then Return
                For iLoop As Integer = App.glItems.Count - 1 To 0 Step -1
                    If App.RegExpOrInstr(App.glItems.Item(iLoop).sTitle, sKill) Then App.glItems.RemoveAt(iLoop)
                Next
                Await App.KillFileAdd(sKill)

            Case Else
                Return
        End Select

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
End Class
