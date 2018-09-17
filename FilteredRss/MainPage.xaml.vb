' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

Imports System.Net.Http
Imports System.Text.RegularExpressions
Imports Windows.ApplicationModel.Background
'Imports Windows.ApplicationModel.Background
Imports Windows.Data.Xml.Dom
Imports Windows.Storage
Imports Windows.UI.Notifications
Imports Windows.UI.Popups
Imports Windows.Web.Syndication



''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page

    ' Dim miLastRssGuid As Integer = 0
    ' Dim sLastId As String = ""
    ' Dim mReadErrors As String ' informacja o bledach wczytywania ("wiecej nowosci, sprawdz sam")


    ' podczas przerabiania na Syndication - że nieużywane?
    'Private Sub bGoToFeed(sender As Object, e As RoutedEventArgs)
    '    uiPost.NavigateToString(oAllItems.DocumentElement.InnerText)
    'End Sub

    'Private Shared Function ItemHdrTable(oNode As XmlElement, iWidth As Integer, sHLevel As String) As String
    '    Dim sResult As String = "<table><tr>"
    '    Dim sTmp As String
    '    Dim iInd As Integer

    '    sResult = sResult & "<td width=" & iWidth \ 4 & ">"

    '    ' to daje Error: oNode.SelectSingleNode("//media:thumbnail/@url")
    '    ' (bez @url takze, i bez slashy takze)
    '    sTmp = oNode.GetXml
    '    iInd = sTmp.IndexOf("<media:thumbnail", StringComparison.Ordinal)
    '    If iInd > 0 Then
    '        sTmp = sTmp.Substring(iInd)
    '        iInd = sTmp.IndexOf("url=""http:", StringComparison.Ordinal)
    '        If iInd > 0 Then sTmp = sTmp.Substring(iInd + 5)
    '        iInd = sTmp.IndexOf("""", StringComparison.Ordinal)
    '        If iInd > 0 Then sTmp = sTmp.Substring(0, iInd)
    '        sResult = sResult & "<img src='" & sTmp & "' width='" & (iWidth \ 4) - 4 & "'></td>"
    '    Else
    '        iInd = sTmp.IndexOf("<img src", StringComparison.Ordinal)
    '        If iInd > 0 Then
    '            sTmp = sTmp.Substring(iInd)
    '            iInd = sTmp.IndexOf("""http:", StringComparison.Ordinal)
    '            If iInd > 0 Then sTmp = sTmp.Substring(iInd + 1)
    '            iInd = sTmp.IndexOf("""", StringComparison.Ordinal)
    '            If iInd > 0 Then sTmp = sTmp.Substring(0, iInd)
    '            sResult = sResult & "<img src='" & sTmp & "' width='" & (iWidth \ 4) - 4 & "'></td>"
    '        End If
    '    End If
    '    sTmp = oNode.SelectSingleNode("title").InnerText
    '    sTmp = sTmp.Replace("( Seedów: ", "(S:")
    '    sTmp = sTmp.Replace(" Peerów: ", "+P:")
    '    sTmp = sTmp.Replace(" )", ")")
    '    sResult = sResult & "<td><" & sHLevel & ">" & sTmp & "</" & sHLevel & "></td>"

    '    sResult = sResult & "</tr></table>"

    '    Return sResult

    'End Function

    'Private Sub ShowPostsListOwn(sender As Object)
    '    ' wczytuje z sAllFeeds, przerabia to na krotka liste do lewego WebView

    '    Dim sResult As String = ""
    '    'Dim oRoot1 As XmlElement = oAllItems.DocumentElement
    '    Dim oNodes1 As XmlNodeList = oAllItems.SelectNodes("//item")


    '    'If Not sender Is Nothing Then
    '    'Dim iInd As Integer

    '    For Each oNode As IXmlNode In oNodes1
    '        sResult = sResult & vbCrLf & "<hr>"

    '        ' klikniecie bedzie przejmowane
    '        If oNode.SelectSingleNode("guid") Is Nothing Then
    '            sResult = sResult & "<a href=" & oNode.SelectSingleNode("link").InnerText & ">"
    '        Else
    '            sResult = sResult & "<a href=" & oNode.SelectSingleNode("guid").InnerText & ">"
    '        End If

    '        sResult = sResult & ItemHdrTable(oNode, uiLista.ActualWidth, "h5")
    '        sResult = sResult & "</a>"
    '    Next

    '    Try
    '        ' 20171101: Dodanie <!-- FilteredRSS --> jako sygnalizacji że zawartość jest moja
    '        ' bo czasem nie wiadomo czemu pokazuje stronę z devil-torrents
    '        uiLista.NavigateToString("<html><body><!-- FilteredRSS -->" & sResult & "</body></html>")
    '    Catch ex As Exception
    '        ' iInd = 11
    '    End Try

    '    uiCount.Text = oNodes1.Count & " items"
    '    'End If

    '    App.SetBadgeNo(oNodes1.Count)

    '    'If oNodes1.Count = 0 Then
    '    '    uiPost.NavigateToString("<html></html>")
    '    'End If
    '    App.SetSettingsBool("ChangedXML", False)
    'End Sub

    Private Shared Function ItemHdrTable(oNode As SyndicationItem, iWidth As Integer, sHLevel As String) As String
        Dim sResult As String = "<table><tr>"
        Dim sTmp As String
        Dim iInd As Integer

        sResult = sResult & "<td width=" & iWidth \ 4 & ">"

        ' to daje Error: oNode.SelectSingleNode("//media:thumbnail/@url")
        ' (bez @url takze, i bez slashy takze)
        sTmp = Net.WebUtility.HtmlDecode(oNode.GetXmlDocument(SyndicationFormat.Rss20).GetXml)
        iInd = sTmp.IndexOf("<media:thumbnail", StringComparison.Ordinal)
        If iInd > 0 Then
            sTmp = sTmp.Substring(iInd)
            iInd = sTmp.IndexOf("url=""http:", StringComparison.Ordinal)
            If iInd > 0 Then sTmp = sTmp.Substring(iInd + 5)
            iInd = sTmp.IndexOf("""", StringComparison.Ordinal)
            If iInd > 0 Then sTmp = sTmp.Substring(0, iInd)
            sResult = sResult & "<img src='" & sTmp & "' width='" & (iWidth \ 4) - 4 & "'></td>"
        Else
            iInd = sTmp.IndexOf("<img src", StringComparison.Ordinal)
            If iInd > 0 Then
                sTmp = sTmp.Substring(iInd)
                iInd = sTmp.IndexOf("""http:", StringComparison.Ordinal)
                If iInd > 0 Then sTmp = sTmp.Substring(iInd + 1)
                iInd = sTmp.IndexOf("""", StringComparison.Ordinal)
                If iInd > 0 Then sTmp = sTmp.Substring(0, iInd)
                sResult = sResult & "<img src='" & sTmp & "' width='" & (iWidth \ 4) - 4 & "'></td>"
            End If
            ' dla atom oldmovies: w <content> img alt=&quot;&quot; border=&quot;0&quot; src=&quot;https:/
        End If
        sTmp = oNode.Title.Text
        sTmp = sTmp.Replace("( Seedów: ", "(S:")
        sTmp = sTmp.Replace(" Peerów: ", "+P:")
        sTmp = sTmp.Replace(" )", ")")
        sResult = sResult & "<td><" & sHLevel & ">" & sTmp & "</" & sHLevel & "></td>"

        sResult = sResult & "</tr></table>"

        Return sResult

    End Function

    Public Sub ShowPostsList()
        ' wczytuje z sAllFeeds, przerabia to na krotka liste do lewego WebView

        Dim sResult As String = ""

        For Each oNode As SyndicationItem In App.oAllItems.Items
            sResult = sResult & vbCrLf & "<hr>"

            ' klikniecie bedzie przejmowane
            ' te warunki powinny byc niepotrzebne, bo podczas wczytywania uzupelniam na link
            'If oNode.Id Is Nothing OrElse oNode.Id = "" Then
            '    sResult = sResult & "<a href=" & oNode.Links.Item(0).ToString & ">"
            'Else
            sResult = sResult & "<a href=" & oNode.Id & ">"
            'End If

            sResult = sResult & ItemHdrTable(oNode, uiLista.ActualWidth, "h5")
            sResult = sResult & "</a>"
        Next

        Try
            ' 20171101: Dodanie <!-- FilteredRSS --> jako sygnalizacji że zawartość jest moja
            ' bo czasem nie wiadomo czemu pokazuje stronę z devil-torrents
            uiLista.NavigateToString("<html><body><!-- FilteredRSS -->" & sResult & "</body></html>")
        Catch ex As Exception
            ' iInd = 11
        End Try

        uiCount.Text = App.oAllItems.Items.Count & " items"
        'End If

        App.SetBadgeNo(App.oAllItems.Items.Count)

        'If oNodes1.Count = 0 Then
        '    uiPost.NavigateToString("<html></html>")
        'End If
        App.SetSettingsBool("ChangedXML", False)
    End Sub

    'Private Shared Function NodeToIgnoreSyndic(oNode As SyndicationItem, sRssFeed As String) As Boolean
    '    Dim sTitle As String = oNode.Title.Text
    '    Dim sDesc As String = Net.WebUtility.HtmlDecode(oNode.GetXmlDocument(SyndicationFormat.Rss20).GetXml)

    '    If oNode.PublishedDate.AddDays(App.GetSettingsInt("MaxDays", 30)) < Date.Now Then Return True

    '    Dim bIgnore As Boolean = False
    '    Dim bWhite As Boolean = False

    '    Dim sPhrases As String()
    '    sPhrases = App.GetSettingsString("BlackList").Split(vbCrLf)
    '    For Each sWord As String In sPhrases
    '        If App.TestNodeMatch(sWord, sTitle, sDesc, sRssFeed) Then
    '            bIgnore = True
    '            Exit For
    '        End If
    '    Next

    '    sPhrases = App.GetSettingsString("WhiteList").Split(vbCrLf)
    '    For Each sWord As String In sPhrases
    '        If sWord = "*" Then
    '            bWhite = True
    '            'bIgnore = False
    '        ElseIf sWord.Substring(1, 2).ToLower = "f:" Then

    '        Else
    '            If App.TestNodeMatch(sWord, sTitle, sDesc, sRssFeed) Then
    '                bIgnore = False
    '                bWhite = True
    '                Exit For
    '            End If
    '        End If
    '    Next


    '    NodeToIgnoreSyndic = bIgnore
    '    If bIgnore Then Exit Function

    '    If bWhite And App.GetSettingsBool("NotifyWhite") Then
    '        App.MakeToast(oNode.Id, oNode.Title.Text, sRssFeed)
    '    End If

    'End Function

    'Private Shared Function NodeToIgnore(oNode As IXmlNode, sFeedUrl As String) As Boolean
    '    Dim sTitle As String = oNode.SelectSingleNode("title").InnerText
    '    Dim sTmp As String = oNode.GetXml

    '    Dim bIgnore As Boolean = False
    '    Dim bWhite As Boolean = False

    '    Dim sPhrases As String()
    '    sPhrases = App.GetSettingsString("BlackList").Split(vbCrLf)
    '    For Each sWord As String In sPhrases
    '        If App.TestNodeMatch(sWord, sTitle, sTmp) Then
    '            bIgnore = True
    '            Exit For
    '        End If
    '    Next

    '    sPhrases = App.GetSettingsString("WhiteList").Split(vbCrLf)
    '    For Each sWord As String In sPhrases
    '        If sWord = "*" Then
    '            bWhite = True
    '            'bIgnore = False
    '        Else
    '            If App.TestNodeMatch(sWord, sTitle, sTmp) Then
    '                bIgnore = False
    '                bWhite = True
    '                Exit For
    '            End If
    '        End If
    '    Next

    '    NodeToIgnore = bIgnore
    '    If bIgnore Then Exit Function

    '    If bWhite And App.GetSettingsBool("NotifyWhite") Then
    '        App.MakeToast(oNode.SelectSingleNode("title").InnerText)
    '    End If

    'End Function

    'Private Async Function AddFeedItemsSyndic(sUrl As String, sender As Object) As Task(Of String)
    '    ' przerobiona funkcja AddFeedItemsOwn, na wykorzystującą klienta Rss

    '    Dim sGuidsValueName, sGuids As String
    '    Dim sTmp As String = ""

    '    If sender IsNot Nothing Then
    '        sTmp = sUrl
    '        ' zabezpieczenie numer 2 (poza ustalaniem szerokości w Form_Resize)
    '        If sTmp.Length > 64 Then sTmp = sTmp.Substring(0, 64)
    '        tbLastRead.Text = sTmp
    '    End If

    '    ' pobierz aktualna liste ostatnio widzianych w feed GUIDów
    '    'sGuidsValueName = sUrl.Replace("/", "")
    '    'sGuidsValueName = sGuidsValueName.Replace("?", "")
    '    'sGuidsValueName = sGuidsValueName.Replace(":", "")
    '    sGuidsValueName = App.Url2VarName(sUrl)
    '    sGuids = App.GetSettingsString(sGuidsValueName)


    '    Dim oRssClnt As SyndicationClient = New SyndicationClient
    '    Dim oRssFeed As SyndicationFeed = Nothing
    '    Dim bErr As Boolean = False
    '    Try
    '        oRssFeed = Await oRssClnt.RetrieveFeedAsync(New Uri(sUrl))
    '    Catch ex As Exception
    '        bErr = True
    '    End Try
    '    If bErr Then
    '        If Not App.GetSettingsBool("autoRead") Then App.DialogBox("Unrecognized or no response from" & vbCrLf & sUrl)
    '        Return ""
    '    End If

    '    ' *TODO* konwersja - dodanie do kazdego Item wlasnych atrybutow
    '    ' feedURL, unread ?

    '    ' Dim sResult As String = ""

    '    If oRssFeed.Items.Count < 1 Then Return ""

    '    Dim iInd As Integer
    '    Dim iCurrId As Integer
    '    Dim bChanged As Boolean = False
    '    Dim bSeen As Boolean
    '    bSeen = False

    '    For Each oNode As SyndicationItem In oRssFeed.Items

    '        ' wiekszosc to Rss2.0
    '        ' oldmovies to Atom1 - i dlatego przerabiam na wersje z Syndication

    '        ' sGuid: <guid>http://devil-torrents.pl/torrent/145394</guid>
    '        '   lub <link>
    '        ' 

    '        'bSkip = False
    '        If sUrl.IndexOf("devil-torrents") > 0 Then
    '            sTmp = oNode.Id
    '            iInd = sTmp.LastIndexOf("/")
    '            iCurrId = CInt(sTmp.Substring(iInd + 1))
    '            miLastRssGuid = Math.Max(miLastRssGuid, iCurrId)
    '            'If iCurrId < iLastRssGuid - 200 Then bSkip = True
    '            If iCurrId < miLastRssGuid - 200 Then
    '                bSeen = True
    '                Exit For
    '            End If
    '        End If

    '        Dim sGuid As String
    '        sGuid = oNode.Id
    '        If sGuid = "" Then
    '            ' blogi Microsoft są RSS, nie mają GUID - użyj link do tego
    '            sGuid = oNode.Links.Item(0).NodeValue
    '            oNode.Id = sGuid
    '        End If

    '        ' dla takich co nie mają guid jako http, np. OnlyOldMovies (atom)
    '        ' efekt: kliknięcie na itemie pokazuje "select app to open link", bo link jest robiony z .guid
    '        If sGuid.Substring(0, 4) <> "http" Then
    '            Dim bMam As Boolean = False
    '            For Each oLink As SyndicationLink In oNode.Links
    '                If oLink.Relationship = "alternate" Then
    '                    sGuid = oLink.Uri.ToString
    '                    bMam = True
    '                    Exit For
    '                End If

    '                ' ostateczny fallback - tak, by webviewer uruchomiło OnNavStart
    '                If Not bMam Then sGuid = "http://host.com/app?" & Net.WebUtility.HtmlEncode(sGuid)
    '            Next
    '        End If

    '        If sGuids.IndexOf(sGuid & "|") < 0 Then
    '            'If Not bSkip And Not NodeToIgnore(oNode, sUrl) Then
    '            Dim sFeedTitle As String
    '            sFeedTitle = App.GetSettingsString("FeedName_" & App.Url2VarName(sUrl))
    '            If sFeedTitle = "" Then sFeedTitle = oRssFeed.Title.Text

    '            If Not NodeToIgnoreSyndic(oNode, sFeedTitle) Then
    '                ' sResult = sResult & vbCrLf & Net.WebUtility.HtmlDecode(oNode.GetXmlDocument(SyndicationFormat.Rss20).GetXml)
    '                App.oAllItems.Items.Insert(0, oNode)
    '                sGuids = sGuids & sGuid & "|"
    '                bChanged = True
    '            End If
    '        End If

    '    Next

    '    ' jesli nic nie ma dodania (wszystko jest ignore), to wracaj bez zmian
    '    If Not bChanged Then Return ""


    '    App.SetSettingsBool("ChangedXML", True)     ' zaznacz ze jest zmiana

    '    ' zapisz do nastepnego uruchomienia (okolo 100 torrentow)
    '    If sGuids.Length > 3900 Then
    '        ' limit 8K - ale bajtow
    '        sGuids = sGuids.Substring(sGuids.Length - 3900)
    '        iInd = sGuids.IndexOf("|")
    '        sGuids = sGuids.Substring(iInd)
    '    End If
    '    App.SetSettingsString(sGuidsValueName, sGuids)


    '    Return ""
    'End Function

    Private Sub uiListaNavStart(sender As WebView, args As WebViewNavigationStartingEventArgs) Handles uiLista.NavigationStarting

        If Not (args.Uri Is Nothing) Then
            ShowTorrentData(args.Uri.ToString)
            args.Cancel = True
        End If

    End Sub

    Public Sub ShowTorrentData(sGuid As String)
        Dim sTmp As String

        For Each oNode As SyndicationItem In App.oAllItems.Items
            If oNode.Id = sGuid Then

                Dim sResult As String

                sResult = "<html><body><h1>" & oNode.Title.Text & "</h1>" ' ItemHdrTable(oNode, uiPost.ActualWidth, "h1")
                sResult = sResult & "<p><small>Posted: " & oNode.PublishedDate.ToString & "</small></p>"

                sTmp = oNode.Summary.Text
                If sTmp = "" Then sTmp = oNode.Content.Text     ' dla onlyoldmovies

                If App.GetSettingsBool("LinksActive") Then
                    Dim iInd, iPrev, iIndStart, iIndEnd As Integer
                    Dim sLink As String
                    iPrev = 15

                    Dim sEndChars As String = " " & vbTab & vbCr & vbLf
                    iInd = sTmp.IndexOf("://", iPrev) ' moze byc http i https!
                    While iInd > 1
                        iPrev = iInd + 2
                        sLink = sTmp.Substring(iInd - 12, 10)
                        If sLink.IndexOf("href=") < 1 And sLink.IndexOf("src=") < 1 Then
                            iIndStart = sTmp.LastIndexOf("http", iInd)  ' moze byc http i https!
                            iIndEnd = sTmp.IndexOfAny(sEndChars.ToCharArray, iInd)
                            sLink = sTmp.Substring(iIndStart, iIndEnd - iIndStart)
                            sTmp = sTmp.Substring(0, iIndStart) & "<a href='" & sLink & "'>" & sLink & "</a>" & sTmp.Substring(iIndEnd)
                            iPrev = iInd + 2 * sLink.Length
                        End If
                        iInd = sTmp.IndexOf("://", iPrev)
                    End While
                    ' *TODO* konwersja linkow
                End If
                sResult = sResult & sTmp
                ' sResult = sResult & "<p>More: <a href='" & oNode.SelectSingleNode("link").InnerText & "'> link</a></p>"
                sResult = sResult & "</body></html>"
                uiPost.NavigateToString(sResult)
                sResult = oNode.Links.Item(0).NodeValue
                uiBLink.NavigateUri = New Uri(sResult)
                If sResult.Length > 30 Then
                    Dim iInd, iInd1 As Integer
                    iInd = sResult.IndexOf("/", 10)
                    iInd1 = sResult.LastIndexOf("/")
                    sResult = sResult.Substring(0, iInd + 1) & "..." & sResult.Substring(iInd1)
                End If
                uiBLink.Content = sResult

                App.SetSettingsString("sLastId", oNode.Id)


            End If
        Next


    End Sub

    ' obsluga zdarzen formatki
    Private Async Sub MainPage_Loaded(sender As Object, e As RoutedEventArgs)
        AppResuming()   ' wczytaj listę RSSów
        Await App.RegisterTriggers()

        ' poniewaz nie dziala językowanie w tle, to trzeba teraz przepisac
        App.SetSettingsString("resDelete", App.GetLangString("resDelete"))
        App.SetSettingsString("resOpen", App.GetLangString("resOpen"))
        App.SetSettingsString("resBrowser", App.GetLangString("resBrowser"))

    End Sub
    Private Sub Form_Resized(sender As Object, e As SizeChangedEventArgs)
        uiLista.Width = uiNaList.ActualWidth - 20
        uiPost.Width = uiNaPost.ActualWidth - 20
        uiLista.Height = uiNaViews.ActualHeight - 20
        uiPost.Height = uiNaViews.ActualHeight - 20
        tbLastRead.Width = uiNaViews.ActualWidth / 2    ' nie wiecej niż pół szerokości
    End Sub


    Private Async Sub Page_GotFocus(sender As Object, e As RoutedEventArgs)

        uiClockRead.IsChecked = App.GetSettingsBool("autoRead")

        ' 20171101: jeśli w środku jest moje, to nie rób reload
        Dim sHtml As String = ""

        Try
            sHtml = Await uiLista.InvokeScriptAsync("eval", New String() {"document.documentElement.outerHTML;"})
        Catch ex As Exception
            ' jesli strona jest pusta, jest Exception
        End Try

        If sHtml.IndexOf("<!-- FilteredRSS -->") < 1 Or App.GetSettingsBool("ChangedXML") Then ShowPostsList()

        tbLastRead.Text = App.GetSettingsString("lastRead")

    End Sub

    ' obsluga guzikow
    Public Async Sub bReadFeed(sender As Object, e As RoutedEventArgs)
        Dim sTmp As String = Await App.ReadFeed(tbLastRead)

        Try
            tbLastRead.Text = sTmp
            ShowPostsList()
        Catch ex As Exception
        End Try

    End Sub
    Private Sub uiClockRead_Click(sender As Object, e As RoutedEventArgs) Handles uiClockRead.Click
        App.SetSettingsBool("autoRead", uiClockRead.IsChecked)
    End Sub

    Private Sub bSetup_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(Setup))
    End Sub

    Private Sub bDelOnePost_Click(sender As Object, e As RoutedEventArgs)

        Dim sGuid As String = App.GetSettingsString("sLastId")
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
        App.SetSettingsString("sLastId", "")

        ToastNotificationManager.History.Clear()

        App.oAllItems.Items.Clear()
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
End Class
