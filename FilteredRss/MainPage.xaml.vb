' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

Imports System.Net.Http
'Imports Windows.ApplicationModel.Background
Imports Windows.Data.Xml.Dom
Imports Windows.Storage
Imports Windows.UI.Notifications
Imports Windows.UI.Popups

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page

    Shared oAllItems As New XmlDocument
    Dim miLastRssGuid As Integer = 0
    Dim sLastId As String = ""
    Dim mReadErrors As String ' informacja o bledach wczytywania ("wiecej nowosci, sprawdz sam")


    Private Sub bGoToFeed(sender As Object, e As RoutedEventArgs)
        uiPost.NavigateToString(oAllItems.DocumentElement.InnerText)
    End Sub
    Private Shared Function ItemHdrTable(oNode As XmlElement, iWidth As Integer, sHLevel As String) As String
        Dim sResult As String = "<table><tr>"
        Dim sTmp As String
        Dim iInd As Integer

        sResult = sResult & "<td width=" & iWidth \ 4 & ">"

        ' to daje Error: oNode.SelectSingleNode("//media:thumbnail/@url")
        ' (bez @url takze, i bez slashy takze)
        sTmp = oNode.GetXml
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
        End If
        sTmp = oNode.SelectSingleNode("title").InnerText
        sTmp = sTmp.Replace("( Seedów: ", "(S:")
        sTmp = sTmp.Replace(" Peerów: ", "+P:")
        sTmp = sTmp.Replace(" )", ")")
        sResult = sResult & "<td><" & sHLevel & ">" & sTmp & "</" & sHLevel & "></td>"

        sResult = sResult & "</tr></table>"

        Return sResult

    End Function

    Private Sub ShowPostsList(sender As Object)
        ' wczytuje z sAllFeeds, przerabia to na krotka liste do lewego WebView

        Dim sResult As String = ""
        'Dim oRoot1 As XmlElement = oAllItems.DocumentElement
        Dim oNodes1 As XmlNodeList = oAllItems.SelectNodes("//item")


        If Not sender Is Nothing Then
            'Dim iInd As Integer

            For Each oNode In oNodes1
                sResult = sResult & vbCrLf & "<hr>"

                ' klikniecie bedzie przejmowane
                sResult = sResult & "<a href=" & oNode.SelectSingleNode("guid").InnerText & ">"

                sResult = sResult & ItemHdrTable(oNode, uiLista.ActualWidth, "h5")
                sResult = sResult & "</a>"
            Next

            Try
                uiLista.NavigateToString("<html><body>" & sResult & "</body></html>")
            Catch ex As Exception
                ' iInd = 11
            End Try

            uiCount.Text = oNodes1.Count & " items"
        End If

        App.SetBadgeNo(oNodes1.Count)

        'If oNodes1.Count = 0 Then
        '    uiPost.NavigateToString("<html></html>")
        'End If
    End Sub

    Private Shared Async Function MsgBox(sMsg As String) As Task(Of String)
        Dim oMD = New MessageDialog(sMsg)
        Await oMD.ShowAsync()
        Return ""

    End Function

    Private Shared Function NodeToIgnorePkar(oNode As IXmlNode, sFeedUrl As String) As Boolean
        Dim sTmp As String
        'Dim sTitle = oNode.SelectSingleNode("title").InnerText

        'If sTitle.IndexOf("audiobook") > 0 Then Return True

        Dim iInd As Integer

        sTmp = oNode.GetXml.ToLower
        iInd = sTmp.IndexOf("gatunek: ", StringComparison.OrdinalIgnoreCase)

        If iInd < 1 Then Return False

        sTmp = sTmp.Substring(iInd)
        iInd = sTmp.IndexOf("<", StringComparison.Ordinal)
        If iInd > 0 Then sTmp = sTmp.Substring(0, iInd)
        sTmp = sTmp.Trim

        If sTmp.IndexOf("thriller") > 0 Then Return True
        If sTmp.IndexOf("karate") > 0 Then Return True
        If sTmp.IndexOf("sportowy") > 0 Then Return True
        If sTmp.IndexOf("sztuki walki") > 0 Then Return True
        If sTmp.IndexOf("prawniczy") > 0 Then Return True
        If sTmp.IndexOf("reality show") > 0 Then Return True
        If sTmp.IndexOf("teledyski") > 0 Then Return True
        If sTmp.IndexOf("thriller") > 0 Then Return True
        If sTmp.IndexOf("horror") > 0 Then Return True

        ' devil muzyka
        If sFeedUrl = "http://devil-torrents.pl/rss.php?cat=10" Then
            If sTmp.IndexOf("thriller") > 0 Then Return True
            If sTmp.IndexOf("drum n Bass") > 0 Then Return True
            If sTmp.IndexOf("electro house") > 0 Then Return True
            If sTmp.IndexOf("game music") > 0 Then Return True
            If sTmp.IndexOf("gothic") > 0 Then Return True
            If sTmp.IndexOf("hard") > 0 Then Return True
            If sTmp.IndexOf("heavy metal") > 0 Then Return True
            If sTmp.IndexOf("hip-hop") > 0 Then Return True
            If sTmp.IndexOf("indie") > 0 Then Return True
            If sTmp.IndexOf("industrial") > 0 Then Return True
            If sTmp.IndexOf("metal") > 0 Then Return True
            If sTmp.IndexOf("progressive") > 0 Then Return True
            If sTmp.IndexOf("punk") > 0 Then Return True
            If sTmp.IndexOf("rap") > 0 Then Return True
            If sTmp.IndexOf("składanki") > 0 Then Return True
            If sTmp.IndexOf("techno") > 0 Then Return True
            If sTmp.IndexOf("trap") > 0 Then Return True
            If sTmp.IndexOf("uplifting trance") > 0 Then Return True
            If sTmp.IndexOf("vocal house") > 0 Then Return True
            If sTmp.IndexOf("vocal trance") > 0 Then Return True

        End If

        Return False
    End Function

    Private Shared Function NodeToIgnore(oNode As IXmlNode, sFeedUrl As String) As Boolean
        Dim sTmp As String
        Dim sTitle = oNode.SelectSingleNode("title").InnerText
        sTmp = oNode.GetXml.ToLower

        Dim bIgnore = False
        Dim bWhite = False

        Dim sPhrases As String()
        sPhrases = App.GetSettingsString("BlackList").ToLower.Split(vbCrLf)
        For Each sWord In sPhrases
            If sWord = "pkar_rules" Then
                bIgnore = NodeToIgnorePkar(oNode, sFeedUrl)
            Else
                If sWord.Length > 2 Then
                    If sWord.Substring(0, 2) = "t:" Then
                        If sTitle.IndexOf(sWord.Substring(2)) > 0 Then
                            bIgnore = True
                            Exit For
                        End If
                    Else
                        If sTmp.IndexOf(sWord) > 0 Then
                            bIgnore = True
                            Exit For
                        End If
                    End If
                End If
            End If
        Next

        sPhrases = App.GetSettingsString("WhiteList").ToLower.Split(vbCrLf)
        For Each sWord In sPhrases
            If sWord = "*" Then
                bWhite = True
                'bIgnore = False
                'Exit For ' gdy sWord="*" wtedy nie szukaj jej w tresci
            End If
            If sWord.Length > 2 Then
                If sWord.Substring(0, 2) = "t:" Then
                    If sTitle.IndexOf(sWord.Substring(2)) > 0 Then
                        bIgnore = False
                        bWhite = True
                        Exit For
                    End If
                Else
                    If sTmp.IndexOf(sWord) > 0 Then
                        bIgnore = False
                        bWhite = True
                        Exit For
                    End If
                End If
            End If
        Next

        NodeToIgnore = bIgnore
        If bIgnore Then Exit Function

        If bWhite And App.GetSettingsBool("NotifyWhite") Then
            App.MakeToast(oNode.SelectSingleNode("title").InnerText)
        End If

    End Function

    Private Async Function AddFeedItems(sUrl As String, sender As Object) As Task(Of String)
        Dim sGuidsValueName, sGuids As String
        Dim sTmp As String

        If Not sender Is Nothing Then tbLastRead.Text = sUrl

        ' pobierz aktualna liste ostatnio widzianych w feed GUIDów
        sGuidsValueName = sUrl.Replace("/", "")
        sGuidsValueName = sGuidsValueName.Replace("?", "")
        sGuidsValueName = sGuidsValueName.Replace(":", "")
        sGuids = App.GetSettingsString(sGuidsValueName)

        Dim oHttp As New HttpClient()
        sTmp = Await oHttp.GetStringAsync(sUrl)
        Dim iInd As Integer
        iInd = sTmp.IndexOf("<rss")
        sTmp = sTmp.Substring(iInd)

        Dim oFeedItems As New XmlDocument
        Try
            oFeedItems.LoadXml(sTmp)
        Catch ex As Exception
            iInd = 11
        End Try

        ' *TODO* konwersja - dodanie do kazdego Item wlasnych atrybutow
        ' feedURL, unread ?

        Dim sResult As String = ""

        Dim oNodes1 As XmlNodeList = oFeedItems.SelectNodes("//item")
        Dim iCurrId As Integer
        'Dim bSkip As Boolean
        Dim bSeen As Boolean

        bSeen = False

        For Each oNode In oNodes1
            'bSkip = False
            If sUrl.IndexOf("devil-torrents") > 0 Then
                sTmp = oNode.SelectSingleNode("guid").InnerText
                iInd = sTmp.LastIndexOf("/")
                iCurrId = CInt(sTmp.Substring(iInd + 1))
                miLastRssGuid = Math.Max(miLastRssGuid, iCurrId)
                'If iCurrId < iLastRssGuid - 200 Then bSkip = True
                If iCurrId < miLastRssGuid - 200 Then
                    bSeen = True
                    Exit For
                End If
            End If

            'If Not bSkip And Not NodeToIgnore(oNode, sUrl) Then
            If Not NodeToIgnore(oNode, sUrl) Then
                If sGuids.IndexOf(oNode.SelectSingleNode("guid").InnerText & "|") < 0 Then
                    sResult = sResult & vbCrLf & oNode.GetXml
                    sGuids = sGuids & oNode.SelectSingleNode("guid").InnerText & "|"
                End If
            End If
        Next

        'mReadErrors: jesli nie wszystko moze pokazac
        ' ale to teraz chyba nie da sie zrobic, bo bSeen reaguje przy duzym odstepie GUID

        sTmp = oAllItems.GetXml
        sTmp = sTmp.Replace("<root>", "")
        sTmp = sTmp.Replace("</root>", "")
        sResult = "<root>" & sResult & sTmp & "</root>"
        oAllItems.LoadXml(sResult)

        ' zapisz do nastepnego uruchomienia (okolo 100 torrentow)
        If sGuids.Length > 3900 Then
            ' limit 8K - ale bajtow
            sGuids = sGuids.Substring(sGuids.Length - 3900)
            iInd = sGuids.IndexOf("|")
            sGuids = sGuids.Substring(iInd)
        End If
        App.SetSettingsString(sGuidsValueName, sGuids)


        Return ""
    End Function


    Private Sub uiListaNavStart(sender As WebView, args As WebViewNavigationStartingEventArgs) Handles uiLista.NavigationStarting

        If Not (args.Uri Is Nothing) Then
            ShowTorrentData(args.Uri.ToString)
            args.Cancel = True
        End If

    End Sub

    Private Sub ShowTorrentData(sGuid As String)
        Dim sTmp As String

        Dim oRoot1 As XmlElement = oAllItems.DocumentElement
        Dim oNode As XmlElement = oRoot1.SelectSingleNode("//item[guid='" & sGuid & "']")
        Dim sResult As String

        sResult = "<html><body><h1>" & oNode.SelectSingleNode("title").InnerText & "</h1>" ' ItemHdrTable(oNode, uiPost.ActualWidth, "h1")
        sResult = sResult & "<p><small>Posted: " & oNode.SelectSingleNode("pubDate").InnerText & "</small></p>"

        sTmp = oNode.SelectSingleNode("description").InnerText
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
        uiBLink.Content = oNode.SelectSingleNode("link").InnerText
        uiBLink.NavigateUri = New Uri(oNode.SelectSingleNode("link").InnerText)
        sLastId = oNode.SelectSingleNode("guid").InnerText

    End Sub


    Public Async Sub CalledFromBackground(sTaskname As String)
        Select Case sTaskname
            Case "FilteredRSStimer"
                If App.GetSettingsBool("autoRead") Then bReadFeed(Nothing, Nothing)
            Case "FilteredRSSservCompl"
                App.UnregisterTriggers(True)
                Await App.RegisterTriggers()
        End Select

    End Sub

    ' obsluga zdarzen formatki
    Private Async Sub MainPage_Loaded(sender As Object, e As RoutedEventArgs)
        AppResuming()   ' wczytaj listę RSSów
        Await App.RegisterTriggers()
    End Sub
    Private Sub Form_Resized(sender As Object, e As SizeChangedEventArgs)
        uiLista.Width = uiNaList.ActualWidth - 20
        uiPost.Width = uiNaPost.ActualWidth - 20
        uiLista.Height = uiNaViews.ActualHeight - 20
        uiPost.Height = uiNaViews.ActualHeight - 20
    End Sub


    Private Sub Page_GotFocus(sender As Object, e As RoutedEventArgs)

        uiClockRead.IsChecked = App.GetSettingsBool("autoRead")
        ShowPostsList(sender)

        tbLastRead.Text = App.GetSettingsString("lastRead")

    End Sub

    ' obsluga guzikow
    Public Async Sub bReadFeed(sender As Object, e As RoutedEventArgs)
        Dim sTmp As String

        miLastRssGuid = App.GetSettingsInt("iLastRssGuid")

        mReadErrors = "" ' tu bedzie suma bledow wczytywania

        If oAllItems.GetXml = "" Then oAllItems.LoadXml("<root></root>")

        Dim aFeeds() As String
        aFeeds = App.GetSettingsString("KnownFeeds").Split(vbCrLf)

        For Each sFeed In aFeeds
            ' bez pustych linii
            If sFeed.Length > 10 Then sTmp = Await AddFeedItems(sFeed, sender)
        Next

        sTmp = "Last read: " & Date.Now().ToString
        App.SetSettingsString("lastRead", sTmp)
        If Not sender Is Nothing Then tbLastRead.Text = sTmp

        If mReadErrors <> "" Then
            ' ddoaj Entry do oAllItems - read report
        End If
        ShowPostsList(sender)

        ' specjane dla DevilTorrents; musi byc po wczytaniu wszystkich
        App.SetSettingsInt("iLastRssGuid", miLastRssGuid)
        ' AppSuspending() ' ewentualnie zapisz dane także tu (na wypadek crash programu)
    End Sub
    Private Sub uiClockRead_Click(sender As Object, e As RoutedEventArgs) Handles uiClockRead.Click
        App.SetSettingsBool("autoRead", uiClockRead.IsChecked)
    End Sub

    Private Sub bSetup_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(Setup))
    End Sub

    Private Sub bDelOnePost_Click(sender As Object, e As RoutedEventArgs)
        If sLastId <> "" Then
            Dim oNode As XmlElement = oAllItems.DocumentElement.SelectSingleNode("//item[guid='" & sLastId & "']")
            If Not (oNode Is Nothing) Then
                Dim oNext = oNode.NextSibling
                Try
                    oAllItems.DocumentElement.RemoveChild(oNode)
                Catch ex As Exception
                    'sLastId = sLastId + 0
                End Try
                If Not oNext Is Nothing Then ShowTorrentData(oNext.SelectSingleNode("guid").InnerText)
            End If
            ShowPostsList(sender)
        End If
    End Sub
    Private Sub bDelAllPosts_Click(sender As Object, e As RoutedEventArgs)
        sLastId = ""
        oAllItems.LoadXml("<root></root>")
        ShowPostsList(sender)
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
        Dim sampleFile As StorageFile = Await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(
            "oAllItems.xml", CreationCollisionOption.ReplaceExisting)
        Await FileIO.WriteTextAsync(sampleFile, oAllItems.GetXml)

    End Sub

    Public Async Sub AppResuming()

        Try
            Dim oFile As StorageFile
            oFile = ApplicationData.Current.LocalCacheFolder.GetFileAsync("oAllItems.xml")
            Dim sTxt As String = Await FileIO.ReadTextAsync(oFile)
            oAllItems.LoadXml(sTxt)
        Catch ex As Exception
            ' zapewne ze filesa nie ma - ignorujemy
        End Try
    End Sub

    Private Sub bInfo_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(Info))
    End Sub

    Private Sub uiPost_NavigationStarted(sender As WebView, args As WebViewNavigationStartingEventArgs) Handles uiPost.NavigationStarting
        If args.Uri Is Nothing Then Exit Sub

        args.Cancel = True
        Windows.System.Launcher.LaunchUriAsync(args.Uri)

    End Sub
End Class
