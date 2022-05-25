
' STORE 2202.11

' 2022.02.19
' * chyba już mam co trzeba wpisać do Default.rd.xml:
'   <Type Name="System.ServiceModel.Syndication.SyndicationFeedFormatter">
'	<Method Name = "DefaultUriParser" Dynamic="Required" />
'	</Type>
'	<Namespace Name = "System.ServiceModel.Syndication" Dynamic="Required All" />
' * poprawka - skracanie GUID dla LoveKraków (a przez to skracanie Tag)
' * poprawka: używa skróconego sGuid do sTag zamiast sGuid :)

' 2022.02.18
' * przełącznik #define dla używania RSS w VBlib (.Net) lub w App.vb (UWP)

' 2022.02.11
' * z powrotem na UWP.RSS, bo .Net.RSS nie działa w trybie RELEASE:
'System.Reflection.MissingRuntimeArtifactException
'HResult = 0x8013151A
'  Message = Cannot retrieve a MethodInfo For this Delegate because the method it targeted (System.ServiceModel.Syndication.SyndicationFeedFormatter.DefaultUriParser(XmlUriData, Uri&)) was Not enabled For metadata Using the Dynamic attribute. For more information, please visit http://go.microsoft.com/fwlink/?LinkID=616868
'  Source = <Cannot evaluate the exception source>
'  StackTrace:
'<Cannot evaluate the exception stack trace>
' (na linii oRssFeed = ServiceModel.Syndication.SyndicationFeed.Load(oReader))
' czyli musiałem czytanie Rss przywrócić z VBlib.App do App.


' STORE 2202.09
' 2022.02.09
' * ERROR od Aśki: poprzednia wersja zrobiła XML len=0, i na tym wylatywało przy wczytywaniu (LoadIndex XML catch, no root element)
' * nie robi czytania XML gdy jest len<10, a MainPage:ShowPostsList nie robi błędu przy null


' STORE 2202

' 2022.02.04-06
' * VBlib
' * zmiana Nuget .NetCore.UniwersalWindows z 5.4.0 na 6.2.13 (security vulnerabilities)
' * z Windows.Web.Syndication.SyndicationClient na ServiceModel.Syndication.SyndicationFeed
' * przeniesienie skracania linku z wczytywania RSS do pokazywania go w MainPage
' * MainPage:Link do pełnej treści, dodałem tooltip z pełnym linkiem
' * dodawanie Feedów, próbuje ściągnąć title dla Facebook oraz Twitter (setup.xaml.lib.vb)
' * w Setup, na liście Feeds
'       * pojawia się czerwony wykrzyknik gdy był błąd ostatnio w danym feed
'       * w ContextMenu jest 'openweb' (w przeglądarce)
'       * w ContextMenu, Reset Seen list
' Settings->oFeed: TIMExxx -> .sLastOKdate, NotifyWhite -> iNotifyWhite, xxx -> oItem.sLastGuids, MaxDays -> iMaxDays
' NIE! W ogóle bez NotifyWhite, przecież sterowanie Toastami jest indywidualne
' Settings per feed: iMaxDays (ale było ładne Combo, 7 dni, 30 dni, 365 dni, teraz jest NumberBox - czyli zmiana stylu ikonek
' Settings: button Add ze zwykłego "+" jest SymbolIcon.Add
' Settings:prawy guzik na MaxDays, ustawianie: week/month/year (odpowiednik combo, tyle ze z right mouse)
' *TODO* sprawdzic Twittera czy sie da
' *TODO* sprawdzic facebook czy sie da - chyba TAK! niestety nie.

' STORE 2112

' 2021.12.30
' * zablokowanie SmallSetup (było dla width<700) - nie chce mi się tego zmieniać
'       ostatnie 30 dni, to 147 moich sesji (z Polski w każdym razie) i 5 z Wietnamu

' 2021.12.21
' * obsługa black/white list per feed (aktualna staje się global)
' * Toast przy pomocy Builder zamiast własnego

' 2021.12.15
' * lista Feeds w JSON a nie zmiennej; przerobienie Setup, Rename, etc.
' * każdy feed ma własny typ toastu dla niego
' * oraz własne black/white list
' * przy wczytywaniu listy feedów robi konwersję z dotychczasowej wersji (zmiennej) wraz z rename'ami
' *TODO* zmiana w SmallSetup (dla małych ekranów)
' * inicjalizacja PKAR_init (lista feedów oraz blacklist) jest teraz w pliku pswd.vb
' * gdy pokazuje listę itemów, kasuje znacznik że lista zmieniona (dla GotFocus)
' * KillFile Save - reset długości pliku

' 2021.12.05
' KillFileLoadMain, brakowało warunku przed DlgBox - że nie z timera

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
        VBlib.DumpCurrMethod()
        ' wczytuje z sAllFeeds, przerabia to na krotka liste do lewego WebView

        If VBlib.App.glItems Is Nothing Then Return

        uiCount.Text = VBlib.App.glItems.Count & " items"
        SetBadgeNo(VBlib.App.glItems.Count)

        App.bChangedXML = False
        uiListItems.ItemsSource = From c In VBlib.App.glItems
    End Sub

    Private Sub uiLista_Click(sender As Object, e As TappedRoutedEventArgs)
        Dim oItem As VBlib.JedenItem = TryCast(sender, Grid).DataContext
        ShowTorrentData(oItem)
    End Sub


    Public Sub ShowTorrentData(oItem As VBlib.JedenItem)
        Dim sTmp As String
        sTmp = "<html><body><h1>" & oItem.sTitle & "</h1>"
        If oItem.sDate <> "" Then sTmp = sTmp & "<p><small>Posted: " & oItem.sDate & "</small></p>"
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

        SetSettingsString("sLastId", oItem.sGuid)

    End Sub

    Public Sub ShowTorrentData(sGuid As String)
        'Dim sTmp As String

        If VBlib.App.glItems Is Nothing Then
            DialogBox("IMPOSSIBLE, ShowTorrentData and oAllItems null")
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

        DialogBox("IMPOSSIBLE? Cannot find such item")
        Exit Sub

    End Sub


    ' obsluga zdarzen formatki
    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)

        VBlib.pkarlibmodule.InitSettings(AddressOf pkar.SetSettingsString, AddressOf pkar.SetSettingsInt, AddressOf pkar.SetSettingsBool, AddressOf pkar.GetSettingsString, AddressOf pkar.GetSettingsInt, AddressOf pkar.GetSettingsBool)
        VBlib.pkarlibmodule.InitDialogBox(AddressOf pkar.DialogBoxAsync, AddressOf pkar.DialogBoxYNAsync, AddressOf pkar.DialogBoxInputAllDirectAsync)
        VBlib.pkarlibmodule.InitDump(GetSettingsInt("debugLogLevel", 0), Windows.Storage.ApplicationData.Current.TemporaryFolder.Path)

        CrashMessageInit()
        Await CrashMessageShowAsync()   ' jesli cos bylo, to pokaze tresc

        AppResuming()   ' wczytaj listę RSSów
        Await App.RegisterTriggers()

        ' poniewaz nie dziala językowanie w tle, to trzeba teraz przepisac
        SetSettingsString("resDelete", GetLangString("resDelete"))
        SetSettingsString("resOpen", GetLangString("resOpen"))
        SetSettingsString("resBrowser", GetLangString("resBrowser"))
        SetSettingsString("resNewItemsInFeed", GetLangString("resNewItemsInFeed"))
        SetSettingsString("resNewItemsList", GetLangString("resNewItemsList"))

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
        VBlib.DumpCurrMethod()
        uiClockRead.IsChecked = GetSettingsBool("autoRead")

        ' 20171101: jeśli w środku jest moje, to nie rób reload
        'Dim sHtml As String = ""

        'Try
        '    sHtml = Await uiLista.InvokeScriptAsync("eval", New String() {"document.documentElement.outerHTML;"})
        'Catch ex As Exception
        '    ' jesli strona jest pusta, jest Exception
        'End Try

        'If sHtml.IndexOf("<!-- FilteredRSS -->") < 1 Or GetSettingsBool("ChangedXML") Then ShowPostsList()
        If App.bChangedXML Then ShowPostsList()

        tbLastRead.Text = GetSettingsString("lastRead")

    End Sub

    ' obsluga guzikow
    Public Async Sub bReadFeed_Click(sender As Object, e As RoutedEventArgs)
        VBlib.DumpCurrMethod()

        Dim sTmp As String = Await App.ReadFeed(tbLastRead)

        Try
            tbLastRead.Text = sTmp
            ShowPostsList()
        Catch ex As Exception
        End Try

    End Sub
    Private Sub uiClockRead_Click(sender As Object, e As RoutedEventArgs) Handles uiClockRead.Click
        SetSettingsBool("autoRead", uiClockRead.IsChecked)
        If uiClockRead.IsChecked AndAlso Not IsTriggersRegistered("FilteredRSStimer") Then App.RegisterTriggers()
    End Sub

    Private Sub bSetup_Click(sender As Object, e As RoutedEventArgs)
        'If uiNaViews.ActualWidth < 700 Then
        '    Me.Frame.Navigate(GetType(SmallSetup))
        'Else
        Me.Frame.Navigate(GetType(Setup))
        'End If
    End Sub

    Private Sub bDelOnePost_Click(sender As Object, e As RoutedEventArgs)
        VBlib.DumpCurrMethod()

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
        VBlib.DumpCurrMethod()

        SetSettingsString("sLastId", "")

        Windows.UI.Notifications.ToastNotificationManager.History.Clear()

        VBlib.App.glItems.Clear()
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

    Public Sub AppSuspending()
        VBlib.DumpCurrMethod()
        App.SaveIndex(True)
    End Sub

    Public Sub AppResuming()
        App.LoadIndex()
        App.KillFileLoad(False)
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

    Private Async Sub DeleteFromContextMenu(oMFI As MenuFlyoutItem, iMode As Integer)
        VBlib.DumpCurrMethod()
        Dim oItem As VBlib.JedenItem = TryCast(oMFI?.DataContext, VBlib.JedenItem)
        If oItem Is Nothing Then Return

        Await VBlib.MainPage.DeleteFromContextMenu(oItem, iMode, AddressOf UsunToast)

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
