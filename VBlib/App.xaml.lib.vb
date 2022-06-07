
'Imports Windows.Data.Xml.Dom
'Imports Windows.ApplicationModel.Background
'Imports Windows.Storage
'Imports Windows.UI.Notifications
'Imports Windows.UI.Popups
'Imports Windows.Web.Syndication
'Imports System.Linq.Expressions
'Imports Newtonsoft.Json.Serialization

' gdy True, to Release będzie miał błąd 

'''' <summary>
'''' Provides application-specific behavior to supplement the default Application class.
'''' </summary>

Public Class JedenItem
    Public Property sTitle As String
    Public Property sPicLink As String
    Public Property sFeedName As String
    Public Property sLinkToDescr As String
    Public Property sItemHtmlData As String
    Public Property sGuid As String     ' do kasowania etc.
    Public Property sDate As String
End Class


Public Class App

    '    ' Public Shared oAllItems As SyndicationFeed
    Public Shared glItems As List(Of JedenItem) = Nothing

    Public Shared Function DelOnePost(sGuid As String) As String
        ' zwraca NextGuid (czyli to, co ma zostac pokazane)

        If sGuid = "" Then Return ""

        Dim iMode As Integer = 0
        Dim oDelNode As ServiceModel.Syndication.SyndicationItem = Nothing
        Dim sNextGuid As String = ""

        Dim iInd As Integer = 0
        For iInd = 0 To App.glItems.Count - 1
            If glItems.Item(iInd).sGuid = sGuid Then Exit For
        Next
        If iInd > glItems.Count - 1 Then Return ""  ' nie znalazl

        glItems.RemoveAt(iInd)

        If iInd > glItems.Count - 1 Then iInd = glItems.Count - 1
        If iInd < 0 Then Return ""

        Return App.glItems.Item(iInd).sGuid
    End Function

    'Private Shared miLastRssGuid As Integer = 0
    'Private Shared miLastRssGuidMs As Integer = 0

    'Public Shared Async Function ReadFeed(oTB As TextBlock) As Task(Of String)
    '    Dim sTmp As String

    '    Try

    '        miLastRssGuid = GetSettingsInt("iLastRssGuid")
    '        miLastRssGuidMs = GetSettingsInt("iLastRssGuidMS")

    '        ' poprzednia wersja FEEDS (zmienna)
    '        'Dim aFeeds() As String
    '        'aFeeds = GetSettingsString("KnownFeeds").Split(vbCrLf)

    '        'For Each sFeed As String In aFeeds
    '        '    ' bez pustych linii
    '        '    If sFeed.Length > 10 AndAlso sFeed.Substring(0, 1) <> ";" Then Await AddFeedItemsSyndic(sFeed, oTB)
    '        'Next

    '        ' nowa wersja FEEDS (plik)
    '        If Not Await FeedsLoad() Then Return "NO FEEDS" ' nie ma nic

    '        For Each oFeed As VBlib.JedenFeed In glFeeds
    '            Await AddFeedItemsSyndic(oFeed, oTB)

    '            Select Case oFeed.iToastType
    '                Case FeedToastType.ListItems
    '                    If oFeed.sToastString <> "" Then
    '                        MakeToast("", GetSettingsString("resNewItemsList") & " (" & oFeed.iToastCnt & ")" & vbCrLf & oFeed.sToastString, oFeed.sName)
    '                    End If
    '                Case FeedToastType.NewExist
    '                    If oFeed.iToastCnt > 0 Then
    '                        MakeToast("", GetSettingsString("resNewItemsInFeed") & " (" & oFeed.iToastCnt & ")", oFeed.sName)
    '                    End If
    '            End Select
    '        Next

    '        sTmp = "Last read: " & Date.Now().ToString
    '        SetSettingsString("lastRead", sTmp)

    '        ' specjane dla DevilTorrents; musi byc po wczytaniu wszystkich
    '        SetSettingsInt("iLastRssGuid", miLastRssGuid)
    '        SetSettingsInt("iLastRssGuidMS", miLastRssGuidMs)
    '        ' AppSuspending() ' ewentualnie zapisz dane także tu (na wypadek crash programu)

    '        Return sTmp
    '    Catch ex As Exception
    '        CrashMessageAdd("ReadFeed catch", ex)
    '    End Try
    '    Return "ERROR"
    'End Function


    ' public, bo z App.Xaml.vb, przy przejściu z powrotem na Windows.Rss
    Public Shared Function ActivateLinks(sInput As String) As String
        Dim sTmp As String

        sTmp = sInput
        If sTmp.Length < 20 Then Return sInput

        Dim iInd, iPrev, iIndStart, iIndEnd As Integer
        Dim sLink As String
        iPrev = 15      ' 15 znaków na poczatek znacznika

        Try
            Dim sEndChars As String = " " & vbTab & vbCr & vbLf
            iInd = sTmp.IndexOf("://", iPrev) ' moze byc http i https!
            While iInd > 12     ' było: >1, ale potem jest iInd-12
                iPrev = iInd + 2
                sLink = sTmp.Substring(iInd - 12, 12)
                If sLink.IndexOf("href=") < 0 And sLink.IndexOf("src=") < 0 Then
                    iIndStart = sTmp.LastIndexOf("http", iInd)  ' moze byc http i https!
                    If iIndStart < 0 Then Exit While
                    iIndEnd = sTmp.IndexOfAny(sEndChars.ToCharArray, iInd)
                    If iIndEnd < 0 Then Exit While
                    sLink = sTmp.Substring(iIndStart, iIndEnd - iIndStart)
                    sTmp = sTmp.Substring(0, iIndStart) & "<a href='" & sLink & "'>" & sLink & "</a>" & sTmp.Substring(iIndEnd)
                    iPrev = iInd + 2 * sLink.Length
                End If
                iInd = sTmp.IndexOf("://", iPrev)
            End While

            Return sTmp

        Catch ex As Exception
            Return sInput
        End Try

    End Function




#Region "Rss feed"
    Private Shared Function ExtractPicLink(sContent As String) As String
        DumpCurrMethod()

        Dim sTmp As String = sContent
        Dim iInd As Integer

        iInd = sTmp.IndexOf("<media:thumbnail", StringComparison.Ordinal)
        If iInd > -1 Then
            sTmp = sTmp.Substring(iInd)
            iInd = sTmp.IndexOf("url=""http:", StringComparison.Ordinal)
            If iInd > 0 Then sTmp = sTmp.Substring(iInd + 5)
            iInd = sTmp.IndexOf("""", StringComparison.Ordinal)
            If iInd > 0 Then sTmp = sTmp.Substring(0, iInd)
            Return sTmp
        Else
            iInd = sTmp.IndexOf("<img src", StringComparison.Ordinal)
            If iInd > -1 Then
                sTmp = sTmp.Substring(iInd)
                iInd = sTmp.IndexOf("""http", StringComparison.Ordinal)     '2018.11.08, http: -> http (bo RSS TomsHardware ma tu https:)
                If iInd > 0 Then sTmp = sTmp.Substring(iInd + 1)
                iInd = sTmp.IndexOf("""", StringComparison.Ordinal)
                If iInd > 0 Then sTmp = sTmp.Substring(0, iInd)
                Return sTmp
            End If
            ' dla atom oldmovies: w <content> img alt=&quot;&quot; border=&quot;0&quot; src=&quot;https:/
        End If

        Return ""
    End Function

    Private Shared Function RssGetFeedName(oFeed As JedenFeed, oNode As ServiceModel.Syndication.SyndicationItem) As String
        DumpCurrMethod()
        If oFeed.sName <> "" Then Return oFeed.sName
        Return oNode.SourceFeed.Title.Text.Trim
    End Function

    Private Shared Function RssGetDate(oNode As ServiceModel.Syndication.SyndicationItem) As String
        DumpCurrMethod()
        ' 2018.11.10, TomsHardware Atom ma Update nie ma Published

        Try
            If oNode.PublishDate < New DateTime(1900, 1, 1) Then
                If oNode.LastUpdatedTime > New DateTime(1900, 1, 1) Then Return oNode.LastUpdatedTime.ToString
            End If
            Return oNode.PublishDate.ToString.ToString
        Catch ex As Exception
            ' Devil daje tu błąd? 
            Return ""
        End Try

    End Function

    Private Shared Function RssGetTitle(oNode As ServiceModel.Syndication.SyndicationItem) As String
        DumpCurrMethod()

        Dim sTmp As String

        If oNode.Title Is Nothing Then
            sTmp = "(no title)"
        Else
            sTmp = oNode.Title.Text
        End If
        sTmp = sTmp.Replace("( Seedów: ", "(S:")
        sTmp = sTmp.Replace(" Peerów: ", "+P:")
        sTmp = sTmp.Replace(" )", ")")

        Return sTmp.Trim
    End Function

    Private Shared Function RssGetPicLink(oNode As ServiceModel.Syndication.SyndicationItem) As String
        DumpCurrMethod()
        ' https://social.msdn.microsoft.com/Forums/en-US/8b6a4d60-b286-41fc-997b-9fde81d0f14a/how-to-get-content-from-syndicationitem

        Dim sTmp As String = ""
        If oNode.Summary IsNot Nothing Then sTmp = oNode.Summary.Text
        If sTmp = "" AndAlso oNode.Content IsNot Nothing Then
            Dim oTmp As ServiceModel.Syndication.TextSyndicationContent = TryCast(oNode.Content, ServiceModel.Syndication.TextSyndicationContent)
            If oTmp Is Nothing Then Return ""
            sTmp = oTmp.Text
        End If
        sTmp = Net.WebUtility.HtmlDecode(sTmp)
        Return ExtractPicLink(sTmp)
    End Function

    Private Shared Function RssGetHtmlData(oNode As ServiceModel.Syndication.SyndicationItem, iConvertLinks As Integer) As String
        DumpCurrMethod()

        Dim sTmp As String = ""
        If oNode.Summary IsNot Nothing Then sTmp = oNode.Summary.Text
        If sTmp = "" AndAlso oNode.Content IsNot Nothing Then
            Dim oTmp As ServiceModel.Syndication.TextSyndicationContent = TryCast(oNode.Content, ServiceModel.Syndication.TextSyndicationContent)
            If oTmp Is Nothing Then Return ""

            sTmp = Net.WebUtility.HtmlDecode(oTmp.Text)
        End If

        If iConvertLinks > 0 Then sTmp = ActivateLinks(sTmp)
        Return sTmp
    End Function
    Private Shared Function RssGetDescLink(oNode As ServiceModel.Syndication.SyndicationItem, oFeed As JedenFeed) As String
        DumpCurrMethod()

        Dim sLinkToDescr As String

        If oNode.Links IsNot Nothing AndAlso oNode.Links.Count > 0 Then
            sLinkToDescr = oNode.Links.Item(0).Uri.AbsoluteUri
        Else
            ' 2019.03.13: moze to doda linki dla tomshardware, ktory czasem ich nie ma?
            sLinkToDescr = oNode.Id
        End If
        ' było przed VBLib: uwaga: dla TomsHardware jest później skrócenie linku

        ' 2019.03.13, onlyoldmovies - nie mają pustego, ale idiotyczny :)
        If oFeed.sUri.ToLower.Contains("onlyoldmovies.blogspot.com") Then
            For Each oLink As ServiceModel.Syndication.SyndicationLink In oNode.Links
                If oLink.RelationshipType = "alternate" Then
                    ' http://onlyoldmovies.blogspot.com/2019/03/born-yesterday-1950-classic-comedydrama.html
                    sLinkToDescr = oLink.Uri.AbsoluteUri
                    Exit For
                End If
            Next
        End If

        Return sLinkToDescr
    End Function

    Private Shared Function RssGetGuid(oNode As ServiceModel.Syndication.SyndicationItem, oFeed As JedenFeed) As String
        DumpCurrMethod()

        ' sGuid: <guid>http://devil-torrents.pl/torrent/145394</guid>
        '   lub <link>
        ' skraca GUIDy, by więcej się zmieściło w zmiennej "seen"

        Dim sTmp As String
        Dim iInd As Integer

        Dim sGuid As String
        sGuid = oNode.Id

        If oFeed.sUri.Contains("devil-torrents") Then
            ' https://devil-torrents.pl/torrent/330489
            Dim iCurrId As Integer
            sTmp = oNode.Id
            iInd = sTmp.LastIndexOf("/")
            iCurrId = CInt(sTmp.Substring(iInd + 1))
            Dim iLastRssGuidDevil As Integer = CInt(oFeed.sLastGuid.Replace("DEVIL-", ""))
            iLastRssGuidDevil = Math.Max(iLastRssGuidDevil, iCurrId)
            oFeed.sLastGuid = iLastRssGuidDevil
            'If iCurrId < iLastRssGuid - 200 Then bSkip = True
            If iCurrId < iLastRssGuidDevil - 200 Then Return ""
            sGuid = "DEVIL-" & iCurrId
        End If

        ' 2022.06.07, GUID jest teraz krótki <guid isPermaLink="false">H8j8FJesm47HqhyoP9jdR9</guid>
        '' 2018.11.10, dla TomsHardware ktory ma długie ID
        '' https://www.tomshardware.com/news/sapphire-radeon-rx-590-nitro-special-edition-specs,38051.html#xtor=RSS-5
        '' dziala też https://www.tomshardware.com/news/moje,38051.html
        '' skrócenie powoduje oszczędność SettingString, i mieści się więcej
        'If oFeed.sUri.ToLower.Contains("tomshardware") Then
        '    iInd = sGuid.IndexOf("#")
        '    If iInd > 0 Then sGuid = sGuid.Substring(0, iInd)
        '    sGuid = sGuid.Replace("#xtor=RSS-5", "")    ' skracamy końcówkę
        '    iInd = sGuid.IndexOf("/", "https://www.tomshardware.com/n".Length) ' to nie musi być news!
        '    sTmp = sGuid.Substring(0, iInd + 1)   ' https://www.tomshardware.com/(news|reviews|...)
        '    iInd = sGuid.LastIndexOf(",")
        '    If iInd > 0 Then
        '        sGuid = sGuid.Substring(iInd)
        '    End If
        'End If

        ' 2019.03.13, z Microsoftu malutki GUID robimy
        ' formalnie: https://support.microsoft.com/help/4343889
        ' teraz:     http://MS/4343889
        ' teraz:     4343889 [7]
        ' zysk:                       1234567890123456789012345 [25]
        ' jest:      12345678901234567 [17]
        ' zmiana: 7 zamiast 42, czyli mieszcze znacznie wiecej
        If oFeed.sUri.ToLower.Contains("support.microsoft.com") Then
            Dim iCurrId As Integer

            Try
                If oNode.Links.Count > 0 Then
                    sGuid = oNode.Links.Item(0).Uri.ToString
                    'oNew.sLinkToDescr = sGuid
                    Dim iTmp As Integer = sGuid.LastIndexOf("/")
                    sGuid = sGuid.Substring(iTmp + 1)
                    iCurrId = CInt(sGuid)
                    sGuid = "MS-" & sGuid
                    ' iLastRssGuidMs = Math.Max(iLastRssGuidMs, iCurrId) ' ale tego nie używam nigdzie, więc po co?
                End If
            Catch ex As Exception
            End Try
        End If

        If oFeed.sUri.ToLower.Contains("zdmk.krakow.pl") Then
            ' https://zdmk.krakow.pl/?post_type=nasze-dzialania&p=33481
            sGuid = sGuid.Replace("https://zdmk.krakow.pl/?", "")
        End If

        If oFeed.sUri.ToLower.Contains("onlyoldmovies.blogspot.com") Then
            ' tag:blogger.com,1999:blog-5189854510646440465.post-8417547528023070273
            sGuid = sGuid.Replace("tag:blogger.com,1999:blog-5189854510646440465.", "")
        End If

        If sGuid = "" Then
            ' blogi Microsoft są RSS, nie mają GUID - użyj link do tego
            ' 20181104: ale to jest złe dla onlyOld, powinien byc inny link...
            ' (<link rel='alternate' type='text/html') - natomiast w Microsoft to jest jedyny link

            ' dla onlyOld, szukamy wsrod linkow konkretnego
            sGuid = ""
            For Each oLink As ServiceModel.Syndication.SyndicationLink In oNode.Links
                If oLink.RelationshipType = "alternate" Then
                    sGuid = oLink.Uri.ToString
                    Exit For
                End If
            Next

            ' jesli nie ma, to tak jak poprzednio - po prostu pierwszy
            If sGuid = "" Then sGuid = oNode.Links.Item(0).Uri.ToString
        End If

        If sGuid.Contains("lovekrakow") Then
            ' LoveKraków nie ma GUID, więc mamy link tylko
            ' http://lovekrakow.pl/aktualnosci/rzecznik-jednego-z-instytutow-uj-napisal-o-robieniu-farszu-z-plodow_44887.html
            iInd = sGuid.LastIndexOf("/")
            Dim iInd1 As Integer = sGuid.LastIndexOf("_")
            ' pierwsza litera, bo nie mozna zacząć linku od underscore... choć i tak tu nie kwestia linku
            sGuid = sGuid.Substring(0, iInd + 2) + sGuid.Substring(iInd1)
        End If

        ' 2018.11.10, dla TomsHardware ktory ma długie ID
        ' https://www.tomshardware.com/news/sapphire-radeon-rx-590-nitro-special-edition-specs,38051.html#xtor=RSS-5
        ' dziala też https://www.tomshardware.com/news/moje,38051.html
        ' skrócenie powoduje oszczędność SettingString, i mieści się więcej
        If sGuid.ToLower.Contains("tomshardware") Then
            iInd = sGuid.IndexOf("#")
            If iInd > 0 Then sGuid = sGuid.Substring(0, iInd)
            iInd = sGuid.IndexOf("/", "https://www.tomshardware.com/n".Length) ' to nie musi być news!
            sTmp = sGuid.Substring(0, iInd + 1)   ' https://www.tomshardware.com/(news|reviews|...)
            Dim iInd2 = sGuid.LastIndexOf(",")
            If iInd2 > 0 Then
                sGuid = sGuid.Substring(0, iInd) & sGuid.Substring(iInd)
            End If
        End If


        Return sGuid

    End Function

    Private Shared Function ConvertRssItemToJedenItem(oFeed As JedenFeed, oNode As ServiceModel.Syndication.SyndicationItem) As JedenItem
        DumpCurrMethod()

        Dim oNew As JedenItem = New JedenItem

        ' silne rozbicie, bo funkcja ma > 300 linii
        oNew.sFeedName = RssGetFeedName(oFeed, oNode)
        oNew.sDate = RssGetDate(oNode)
        oNew.sTitle = RssGetTitle(oNode)    ' właściwie tylko podmiany dla Devil
        oNew.sPicLink = RssGetPicLink(oNode)
        oNew.sItemHtmlData = RssGetHtmlData(oNode, oFeed.iLinksActive)
        oNew.sLinkToDescr = RssGetDescLink(oNode, oFeed)
        oNew.sGuid = RssGetGuid(oNode, oFeed)
        If oNew.sGuid = "" Then
            ' znaczy do pominięcia, zbyt daleko w historii Devila
            DumpMessage("sGuid empty")
            Return Nothing
        End If

        Return oNew
    End Function



    ''' <summary>
    ''' ret NULL gdy empty HttpPage, lub lista nowych
    ''' </summary>
    Private Shared Async Function GetRssFeed(oFeed As JedenFeed) As Task(Of List(Of JedenItem))
        DumpCurrMethod("feed=" & oFeed.sName)

        Dim oRssFeed As ServiceModel.Syndication.SyndicationFeed = Nothing

        Try

            HttpPageSetAgent("FilteredRSS") '  konieczne dla LoveKrakow było w Windows.Web.Syndication.SyndicationClient, więc niech będzie
            Dim sPage As String = Await VBlib.HttpPageAsync(New Uri(oFeed.sUri))
            If sPage = "" Then
                DumpMessage("empty sPage")
                Return Nothing
            End If
            ' wycięcie z DevilTorrents, do znaku "<"
            sPage = sPage.TrimBefore("<")
            sPage = sPage.Replace("CET</pubDate>", "</pubDate>") ' przykład nie ma strefy czasowej, więc wedle https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.syndication.syndicationitem.publishdate?f1url=%3FappId%3DDev16IDEF1%26l%3DEN-US%26k%3Dk(System.ServiceModel.Syndication.SyndicationItem.PublishDate);k(DevLang-VB)%26rd%3Dtrue&view=dotnet-plat-ext-6.0
            sPage = sPage.Replace("CEST</pubDate>", "</pubDate>") ' przykład nie ma strefy czasowej, więc wedle https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.syndication.syndicationitem.publishdate?f1url=%3FappId%3DDev16IDEF1%26l%3DEN-US%26k%3Dk(System.ServiceModel.Syndication.SyndicationItem.PublishDate);k(DevLang-VB)%26rd%3Dtrue&view=dotnet-plat-ext-6.0
            Using oReader As Xml.XmlReader = Xml.XmlReader.Create(New IO.StringReader(sPage))
                oRssFeed = ServiceModel.Syndication.SyndicationFeed.Load(oReader)
            End Using
        Catch ex As Exception
            DumpMessage("FAIL: " & ex.Message)
            Return Nothing
        End Try

        Return GetRssFeed(oFeed, oRssFeed)

    End Function

    Public Shared Function GetRssFeed(oFeed As JedenFeed, oRssFeed As ServiceModel.Syndication.SyndicationFeed) As List(Of JedenItem)
        DumpCurrMethod("oFeed=" & oFeed.sName & ", oRssFeed.Count=" & oRssFeed.Items.Count)

        Dim oRetList As New List(Of JedenItem)

        For Each oNode As ServiceModel.Syndication.SyndicationItem In oRssFeed.Items
            Dim oNew As VBlib.JedenItem = App.ConvertRssItemToJedenItem(oFeed, oNode)

            Try
                oNode.SourceFeed = oRssFeed ' bo niestety sam tego nie robi
                oNew = App.ConvertRssItemToJedenItem(oFeed, oNode)
                If oNew Is Nothing Then
                    ' z Devila, za daleko w historii - koniec pętli
                    DumpMessage("oNew is NULL")
                    Exit For
                End If
            Catch
                DumpMessage("CATCH przy konwersji itemu")
            End Try

            Try
                ' warunek dla normalnego - wcześniej go nie było widać; dla twittera zawsze spełniony (tam nie ma "|")
                If oFeed.sLastGuids.Contains(oNew.sGuid & "|") Then
                    DumpMessage("node in sLastGuids")
                Else
                    If NodeToIgnore(oFeed, oNode.PublishDate, oNew) Then
                        DumpMessage("node to be ignored")
                    Else
                        oFeed.sLastGuids = oFeed.sLastGuids & oNew.sGuid & "|"
                        If oNew.sGuid > oFeed.sLastGuid Then oFeed.sLastGuid = oNew.sGuid
                        oRetList.Add(oNew)
                    End If
                End If
            Catch
                DumpMessage("CATCH przy tescie dla ignore")
            End Try
        Next

        oFeed.sLastOkDate = Date.Now.ToString("yyMMdd")

        ' dla normalnego RSS, dla twittera tego w ogole nie robi
        If Not oFeed.sUri.ToLower.Contains("twitter.com") Then
            ' zapisz do nastepnego uruchomienia (okolo 100 torrentow)
            If oFeed.sLastGuids.Length > 3900 Then
                Dim iInd As Integer
                ' limit 8K - ale bajtow
                oFeed.sLastGuids = oFeed.sLastGuids.Substring(oFeed.sLastGuids.Length - 3900)
                iInd = oFeed.sLastGuids.IndexOf("|")
                oFeed.sLastGuids = oFeed.sLastGuids.Substring(iInd)
            End If
            oFeed.sLastGuids = oFeed.sLastGuids
        End If

        Return oRetList
    End Function

#End Region

#Region "Twitter feed"

    ' wszystkie funkcje mają bAsFiltered:
    ' TRUE: dla odwołania jako HttpAgent=FilteredRSS
    ' FALSE: dla odwołania jako Edge (ale wtedy mamy skomplikowane skrypty, które dopiero ściągają dane

    Private Shared Function TwittGetPicture(sPage As String, bAsFiltered As Boolean) As String
        ' obrazek
        Dim sProfilePhoto As String = ""

        Dim iInd As Integer

        If bAsFiltered Then
            iInd = sPage.IndexOf("https://pbs.twimg.com/profile_images")
        Else
            iInd = sPage.IndexOf("<img alt=""Opens profile photo")
        End If

        If iInd > 0 Then
            sProfilePhoto = sPage.Substring(iInd, 400)
            iInd = sProfilePhoto.IndexOf("""")
            sProfilePhoto = sProfilePhoto.Substring(0, iInd)
        End If

        Return sProfilePhoto

    End Function

    Private Shared Function TwittGetFeedName(oFeed As JedenFeed) As String
        If oFeed.sName <> "" Then Return oFeed.sName
        Dim iInd As Integer = oFeed.sUri.LastIndexOf("/")
        Return oFeed.sUri.Substring(iInd + 1)
    End Function

    Private Shared Function TwittGetDate(oNode As HtmlAgilityPack.HtmlNode, bAsFiltered As Boolean) As String
        Dim iInd As Integer
        Dim sRet As String = ""

        Try
            If bAsFiltered Then
                iInd = oNode.InnerHtml.IndexOf("data-time=")
                If iInd > 0 Then
                    iInd = oNode.InnerHtml.IndexOf("""", iInd)
                    sRet = oNode.InnerHtml.Substring(iInd, 20)
                    iInd = sRet.IndexOf("""")
                    sRet = sRet.Substring(0, iInd)
                    Dim lDate As Long = 0
                    If Long.TryParse(sRet, lDate) Then
                        sRet = DateTimeOffset.FromUnixTimeSeconds(lDate).ToString("yyyy.MM.dd HH:mm:ss")
                    End If
                End If
            Else
                ' <time datetime="2022-02-04T14:04:10.000Z">4h</time>
                iInd = oNode.InnerHtml.IndexOf("<time datetime")
                If iInd > 0 Then
                    sRet = oNode.InnerHtml.Substring(iInd + "<time datetime".Length + 1, 50)
                    iInd = sRet.IndexOf("""")
                    sRet = sRet.Substring(0, iInd)
                End If
            End If

            Return sRet
        Catch ex As Exception
            Return ""
        End Try

    End Function

    Private Shared Function TwittGetTitle(oNode As HtmlAgilityPack.HtmlNode, bAsFiltered As Boolean) As String

        Dim sTitle As String = ""

        If bAsFiltered Then
            Dim iInd As Integer
            iInd = oNode.InnerHtml.IndexOf("js-tweet-text ")
            If iInd > 0 Then
                iInd = oNode.InnerHtml.IndexOf(">", iInd)
                sTitle = oNode.InnerHtml.Substring(iInd + 1) ' .TrimAfter("<")
                iInd = sTitle.IndexOf("<", iInd)
                sTitle = sTitle.Substring(0, iInd)
            End If
        Else
            Dim oNode1 = oNode.SelectSingleNode("p")
            If oNode1 IsNot Nothing Then
                sTitle = oNode1.InnerText.Trim
            End If
        End If

        If sTitle = "" Then sTitle = RemoveHtmlTags(TwittGetHtmlData(oNode, bAsFiltered, False))

        If sTitle.Length > 30 Then sTitle = sTitle.Substring(0, 30)

        Return sTitle

    End Function

    Private Shared Function TwittGetHtmlData(oNode As HtmlAgilityPack.HtmlNode, bAsFiltered As Boolean, iConvertLinks As Integer) As String

        Dim sTmp As String = oNode.InnerHtml
        If iConvertLinks > 0 Then sTmp = ActivateLinks(sTmp)

        Return sTmp
    End Function
    Private Shared Function TwittGetDescLink(oNode As HtmlAgilityPack.HtmlNode, bAsFiltered As Boolean) As String
        Dim sLinkToDescr As String = ""
        Dim iInd As Integer

        ' link bezposrednio do postu?
        iInd = oNode.InnerHtml.IndexOf("data-permalink")
        If iInd > 0 Then
            iInd = oNode.InnerHtml.IndexOf("""", iInd)
            sLinkToDescr = oNode.InnerHtml.Substring(0, 250)
            iInd = sLinkToDescr.IndexOf("""")
            sLinkToDescr = sLinkToDescr.Substring(0, iInd)
        End If

        Return sLinkToDescr
    End Function

    Private Shared Function ConvertTwittItemToJedenItem(oFeed As JedenFeed, oNode As HtmlAgilityPack.HtmlNode, bAsFiltered As Boolean) As JedenItem

        Dim oNew As JedenItem = New JedenItem

        ' silne rozbicie, bo żeby było jak w Rss
        oNew.sFeedName = TwittGetFeedName(oFeed)
        oNew.sDate = TwittGetDate(oNode, bAsFiltered)
        oNew.sItemHtmlData = TwittGetHtmlData(oNode, bAsFiltered, oFeed.iLinksActive)
        oNew.sTitle = TwittGetTitle(oNode, bAsFiltered)  ' dla Twitter to musi byc ponizej sItemHtmlData
        oNew.sLinkToDescr = TwittGetDescLink(oNode, bAsFiltered)
        oNew.sGuid = oNew.sDate.Replace(" ", "").Replace(":", "").Replace(".", "")

        Return oNew
    End Function


    Private Shared Async Function GetTwitterFeed(oFeed As JedenFeed) As Task(Of List(Of JedenItem))
        ' If oTB IsNot Nothing Then Await DialogBoxAsync("Sorry, Twitter zmienil access")

        ' https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/get-users-show
        ' wymagana autoryzacja

        ' smieszne, bo to działało przez trochę! i znowu przestało, nie ma w sPage nic...

        Dim oRetList As New List(Of JedenItem)

        Dim bAsFiltered As Boolean = True   ' HttpAgent "Filtered"

        Try
            If bAsFiltered Then
                HttpPageSetAgent("FilteredRSS")
            Else
                HttpPageSetAgent()  ' jako Edge
            End If

            Dim sUrl As String = oFeed.sUri
            ' sUrl = sUrl.Replace("//twitter.com/", "//mobile.twitter.com/")  ' przejscie na wersje normalniejszą
            sUrl = sUrl.Replace("/mobile.", "/")
            Dim sPage As String = Await VBlib.HttpPageAsync(New Uri(sUrl), "", False)
            If sPage = "" Then Return Nothing

            Dim sProfilePhoto As String = TwittGetPicture(sPage, True)

            ' przykrojenie - tak aby były tylko posty
            If bAsFiltered Then
                Dim iInd As Integer
                iInd = sPage.IndexOf("<ol class=""stream-items")
                If iInd < 10 Then Return Nothing
                sPage = sPage.Substring(iInd)
                iInd = sPage.IndexOf("</ol>")
                sPage = sPage.Substring(0, iInd + 5)
            End If


            Dim oDoc As New HtmlAgilityPack.HtmlDocument()
            oDoc.LoadHtml(sPage)

            Dim oPostsList As HtmlAgilityPack.HtmlNodeCollection
            If bAsFiltered Then
                oPostsList = oDoc.DocumentNode.SelectNodes("/ol/li")
            Else
                oPostsList = oDoc.DocumentNode.SelectNodes("//article")
            End If

            Dim sLastGuid As String = oFeed.sLastGuid
            Dim bFirst As Boolean = True

            For Each oPost As HtmlAgilityPack.HtmlNode In oPostsList
                Dim oNew As VBlib.JedenItem = ConvertTwittItemToJedenItem(oFeed, oPost, bAsFiltered)
                oNew.sPicLink = sProfilePhoto

                If Not oPost.InnerHtml.Contains("user-pinned") Then
                    ' sprawdzenie dat
                    If oNew.sGuid = sLastGuid Then Exit For
                    If bFirst Then
                        oFeed.sLastGuid = oNew.sGuid ' SetSettingsString(sLastGuidSettings, oNew.sGuid)
                        bFirst = False
                    End If
                Else
                    ' a gdy pinned, to jeśli starszy niż ostatni widziany, to go pomijamy
                    If oNew.sGuid < sLastGuid Then Continue For
                End If

                '  i odatkowo sprawdzanie ignore, oraz "already seen"
                If Not NodeToIgnore(oFeed, Nothing, oNew) Then    ' oNode: dla daty w wersji date
                    oRetList.Add(oNew)
                End If

            Next

        Catch ex As Exception
            oFeed.bLastError = True
            CrashMessageAdd("GetTwitterFeed catch: " & oFeed.sName, ex)
        End Try

        Return oRetList
    End Function
#End Region

#If False Then

    Private Shared Async Function GetTwarzetnikFeed(oFeed As JedenFeed) As Task(Of List(Of JedenItem))

        If oFeed.sUri.ToLower.Contains("/groups/") Then
            DebugOut(0, "Facebook groups are not implemented yet")
            Return Nothing
        End If

        Return Nothing ' niestety, nie działa ani jako FilteredRSS ani jako Edge

        'HttpPageSetAgent("FilteredRSS")
        ''HttpPageSetAgent()
        'Dim sPage As String = Await HttpPageAsync(oFeed.sUri)
        'If sPage = "" Then Return Nothing

        'Dim oRetList As New List(Of JedenItem)

        'Dim oResp As Windows.Web.Http.HttpResponseMessage = Nothing
        'Dim sError = ""
        'Dim iInd As Integer
        'Dim sUrl As String = oFeed.sUri


        'Try

        '    ' testowanie: see TestLoveKrakow:MainPage


        '    Dim sFBpageId As String = sUrl.Substring(iInd + 1)

        '    Dim oHttp As Windows.Web.Http.HttpClient = New Windows.Web.Http.HttpClient

        '    oHttp.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.3 Safari/537.36 Edg/83.0.478.5")
        '    ' accept-Encoding: gzip, deflate, br
        '    oHttp.DefaultRequestHeaders.AcceptEncoding.Add(New Windows.Web.Http.Headers.HttpContentCodingWithQualityHeaderValue("gzip"))
        '    oHttp.DefaultRequestHeaders.AcceptEncoding.Add(New Windows.Web.Http.Headers.HttpContentCodingWithQualityHeaderValue("deflate"))
        '    oHttp.DefaultRequestHeaders.AcceptEncoding.Add(New Windows.Web.Http.Headers.HttpContentCodingWithQualityHeaderValue("br"))
        '    ' accept-Language: en-US,en;q=0.9,pl;q=0.8
        '    oHttp.DefaultRequestHeaders.AcceptLanguage.Add(New Windows.Web.Http.Headers.HttpLanguageRangeWithQualityHeaderValue("en-US"))
        '    oHttp.DefaultRequestHeaders.AcceptLanguage.Add(New Windows.Web.Http.Headers.HttpLanguageRangeWithQualityHeaderValue("en", 0.9))
        '    oHttp.DefaultRequestHeaders.AcceptLanguage.Add(New Windows.Web.Http.Headers.HttpLanguageRangeWithQualityHeaderValue("pl", 0.8))
        '    ' accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9
        '    ' ale takie tez dziala:
        '    ' "accept"="text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
        '    oHttp.DefaultRequestHeaders.Accept.Add(New Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("text/html"))
        '    oHttp.DefaultRequestHeaders.Accept.Add(New Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("application/xhtml+xml"))
        '    oHttp.DefaultRequestHeaders.Accept.Add(New Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("application/xml", 0.9))
        '    oHttp.DefaultRequestHeaders.Accept.Add(New Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("*/*", 0.8))

        '    Try
        '        oResp = Await oHttp.GetAsync(New Uri(sUrl)) '  & "?_fb_noscript=1" , "/posts"
        '    Catch ex As Exception
        '        sError = ex.Message
        '    End Try
        '    If oResp.StatusCode = 303 Or oResp.StatusCode = 302 Or oResp.StatusCode = 301 Then
        '        sUrl = oResp.Headers.Location.ToString
        '        oResp = Await oHttp.GetAsync(New Uri(sUrl))
        '    End If

        '    If oResp.StatusCode > 290 Then
        '        If oTB IsNot Nothing Then Await DialogBoxAsync("ERROR " & oResp.StatusCode)
        '        Return
        '    End If

        'Catch ex As Exception
        '    CrashMessageAdd("GetTwarzetnikFeed catch 1:" & sUrl, ex)
        '    Return
        'End Try

        'If oResp Is Nothing Then
        '    CrashMessageAdd("GetTwarzetnikFeed catch 1a:" & sUrl, "oResp is null")
        '    Return
        'End If

        'Dim sResp As String = ""

        'Try

        '    Try
        '        sResp = Await oResp.Content.ReadAsStringAsync
        '    Catch ex As Exception
        '        sError = ex.Message
        '    End Try

        '    If sError <> "" Then
        '        If oTB IsNot Nothing Then Await DialogBoxAsync("error " & sError & " at ReadAsStringAsync " & sUrl & " page")
        '        Return
        '    End If

        '    Dim sPage As String
        '    iInd = sResp.IndexOf("PagesProfileHomePrimaryColumnPagelet")
        '    If iInd < 10 Then
        '        If oTB IsNot Nothing Then Await DialogBoxAsync("error interpreting page for " & sUrl)
        '        Return
        '    End If

        '    sPage = sResp.Substring(iInd)
        '    Dim iInd1 As Integer


        '    ' Dim sLastGuidSettings As String = VBlib.App.Url2VarName(sUrl)
        '    Dim sLastGuid As String = oFeed.sLastGuid ' GetSettingsString(sLastGuidSettings)
        '    Dim bFirst As Boolean = True


        '    iInd = sPage.IndexOf("story-subtitle")
        '    While iInd > 0
        '        'Debug.WriteLine(vbCrLf & vbCrLf & "nowy post: " & vbCrLf)
        '        Dim oNew As New VBlib.JedenItem

        '        iInd = sPage.LastIndexOf("<img", iInd)
        '        iInd = sPage.IndexOf("src=", iInd) + 5  ' src="
        '        iInd1 = sPage.IndexOf("""", iInd)
        '        oNew.sPicLink = sPage.Substring(iInd, iInd1 - iInd).Replace("&amp;", "&")
        '        'Debug.WriteLine(" piclink = " & sPage.Substring(iInd, iInd1 - iInd + 2))    ' PLUS DWA kontroli konca

        '        iInd = sPage.IndexOf("a href", iInd)
        '        iInd = sPage.IndexOf(">", iInd) + 1
        '        iInd1 = sPage.IndexOf("<", iInd)
        '        'Debug.WriteLine(" feed = " & sPage.Substring(iInd, iInd1 - iInd + 2))    ' PLUS DWA kontroli konca
        '        oNew.sFeedName = sPage.Substring(iInd, iInd1 - iInd)

        '        iInd = sPage.IndexOf("/permalink", iInd)
        '        iInd1 = sPage.IndexOf("""", iInd + 1)
        '        'Debug.WriteLine(" linktodescr = " & sPage.Substring(iInd, iInd1 - iInd + 2))    ' PLUS DWA kontroli konca
        '        ' ewentualnie, przy dluzszych historiach...
        '        oNew.sLinkToDescr = sPage.Substring(iInd, iInd1 - iInd)

        '        iInd = sPage.IndexOf("data-utime", iInd) + "data-utime=""".Length
        '        iInd1 = sPage.IndexOf("""", iInd)
        '        Dim sTmp As String = sPage.Substring(iInd, iInd1 - iInd)
        '        'Debug.WriteLine(" guid = " & sTmp)
        '        oNew.sGuid = sTmp
        '        If sTmp = sLastGuid Then Exit While
        '        If bFirst Then
        '            oFeed.sLastGuid = sTmp ' SetSettingsString(sLastGuidSettings, sTmp)
        '            bFirst = False
        '        End If

        '        ' bedzie potrzebne później
        '        Dim oPostDate As DateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(sTmp)

        '        oNew.sDate = oPostDate.ToString("f")
        '        'Debug.WriteLine(" data = " & DateTimeOffset.FromUnixTimeSeconds(sTmp).ToString("f"))

        '        iInd = sPage.IndexOf("post_message", iInd)
        '        iInd = sPage.IndexOf("<p>", iInd)
        '        iInd1 = sPage.IndexOf("<", iInd + 3)
        '        sTmp = sPage.Substring(iInd + 3, iInd1 - iInd - 3)
        '        oNew.sTitle = sTmp
        '        'Debug.WriteLine(" title = " & sTmp)

        '        iInd1 = sPage.IndexOf("</div", iInd)
        '        sTmp = sPage.Substring(iInd, iInd1 - iInd)
        '        sTmp = sTmp.Replace("<span class=""text_exposed_hide"">...</span>", "") ' bo calosc pokazujemy

        '        ' 2020.04.22
        '        sTmp = sTmp.Replace("href=""/", "href=""https://www.facebook.com/") ' link dla duzo dluzszych (otwieranych w oddzielnej stronie)
        '        ' ewentualnie ściągać takie coś, i podmieniać sTmp na nowe?



        '        'Debug.WriteLine(" tekst = " & sTmp)
        '        oNew.sItemHtmlData = sTmp

        '        If Not NodeToIgnore(oFeed, oNew.sFeedName, oNew.sGuid, oNew.sTitle, oPostDate, oNew.sItemHtmlData) Then
        '            VBlib.App.glItems.Add(oNew)
        '        End If

        '        sPage = sPage.Substring(iInd1)

        '        iInd = sPage.IndexOf("story-subtitle")
        '    End While

        'Catch ex As Exception
        '    CrashMessageAdd("GetTwarzetnikFeed catch:" & sUrl, ex)
        'End Try

        Return Nothing
    End Function
    Private Shared Async Function GetInstagramFeed(oFeed As JedenFeed) As Task(Of List(Of JedenItem))
        Dim oRetList As New List(Of JedenItem)
        'Try

        '    Dim sInstaName As String = oFeed.sUri.Replace("//instagram.com/", "")
        '    sInstaName = sInstaName.Replace("https:", "")
        '    sInstaName = sInstaName.Replace("http:", "")
        '    sInstaName = "Insta: " & sInstaName


        '    'Dim bMsg As Boolean = If(oTB Is Nothing, False, True)
        '    Dim sPage As String = Await HttpPageAsync(oFeed.sUri, sInstaName, False)
        '    If sPage = "" Then
        '        If oTB IsNot Nothing Then Await DialogBoxAsync("Bad response from" & vbCrLf & oFeed.sUri)
        '        Return
        '    End If

        '    Dim iInd As Integer = sPage.IndexOf(">window._sharedData")
        '    iInd = sPage.IndexOf("{", iInd)
        '    sPage = sPage.Substring(iInd)
        '    iInd = sPage.IndexOf(";</script>")
        '    sPage = sPage.Substring(0, iInd)

        '    Dim oJSON As JSONinstagram = Newtonsoft.Json.JsonConvert.DeserializeObject(sPage, GetType(JSONinstagram))

        '    Dim oJUser As JSONinstaUser = oJSON.entry_data.ProfilePage.ElementAt(0).graphql.user

        '    Dim sLastGuidSettings As String = App.Url2VarName(oFeed.sUri)
        '    Dim sLastGuid As String = ofeed.slastguid GetSettingsString(sLastGuidSettings)
        '    Dim bFirst As Boolean = True

        '    Dim sFeedTitle As String
        '    sFeedTitle = oFeed.sName  GetSettingsString("FeedName_" & App.Url2VarName(oFeed.sUri)).Trim

        '    For Each oEdge As JSONinstaPicEdge In oJUser.edge_owner_to_timeline_media.edges
        '        Dim oItem As JSONinstaPicNode = oEdge.node
        '        If oItem.id = sLastGuid Then Exit For
        '        If bFirst Then
        '            oFeed.sLastGuid = oitem.id SetSettingsString(sLastGuidSettings, oItem.id)
        '            bFirst = False
        '            sFeedTitle = "insta: " & oJUser.full_name.Trim
        '        End If

        '        Dim oNew As JedenItem = New JedenItem
        '        oNew.sFeedName = sFeedTitle
        '        oNew.sGuid = "insta:" & oItem.id
        '        oNew.sTitle = oItem.owner.username
        '        oNew.sPicLink = oItem.display_url
        '        oNew.sItemHtmlData = "<img src='" & oItem.display_url & "'>"
        '        If oItem.location IsNot Nothing Then
        '            oNew.sItemHtmlData = oNew.sItemHtmlData & "<p>" & oItem.location.name
        '        End If
        '        oNew.sItemHtmlData = oNew.sItemHtmlData & "<p>" & oItem.accessibility_caption

        '        oNew.sDate = DateTimeOffset.FromUnixTimeSeconds(oItem.taken_at_timestamp).ToString("f")
        '        oNew.sLinkToDescr = oNew.sPicLink   ' na razie to samo... można będzie zapisać obrazek :)

        '        If Not NodeToIgnoreMain(oFeed, oNew.sFeedName, oNew.sGuid, oNew.sTitle,
        '                            DateTimeOffset.FromUnixTimeSeconds(oItem.taken_at_timestamp),
        '                            oNew.sItemHtmlData) Then
        '            App.glItems.Add(oNew)
        '        Else
        '            ' App.glItems.Add(oNew)
        '        End If
        '    Next

        '    ' teoretycznie mamy tu feed obrazkow
        '    ' ale obrazki mają problem - jakby timestamp był w środku, albo inszy 
        '    ' https://scontent-waw1-1.cdninstagram.com/v/t51.2885-15/sh0.08/e35/c0.180.1440.1440a/s640x640/73251710_350808852403727_6326428339021572324_n.jpg?_nc_ht=scontent-waw1-1.cdninstagram.com\u0026_nc_cat=111\u0026_nc_ohc=GUEQvgt-bOUAX9Nij2A\u0026oh=0cbf86dd7c1a8a2c05de385d66c21c81\u0026oe=5EC1BA1D
        '    ' Bad URL timestamp

        '    ' https://scontent-waw1-1.cdninstagram.com/v/t51.2885-15/sh0.08/e35/c0.180.1440.1440a/s640x640/73251710_350808852403727_6326428339021572324_n.jpg?_nc_ht=scontent-waw1-1.cdninstagram.com\u0026_nc_cat=111&_nc_ohc=GUEQvgt-bOUAX9Nij2A&oh=0cbf86dd7c1a8a2c05de385d66c21c81&oe=5EC1BA1D
        '    ' URL signature mismatch
        'Catch ex As Exception
        '    CrashMessageAdd("GetInstagramFeed catch: " & oFeed.sUri, ex)
        'End Try

        Return Nothing
    End Function
#End If
    ''' <summary>
    ''' ret NULL gdy empty HttpPage, lub lista nowych
    ''' </summary>
    Private Shared Async Function GetAnyFeed(oFeed As VBlib.JedenFeed) As Task(Of List(Of JedenItem))
        DumpCurrMethod("feed = " & oFeed.sName)
        Try
            If oFeed.sUri.ToLower.Contains("twitter.com") Then Return Await GetTwitterFeed(oFeed)
            'If oFeed.sUri.ToLower.Contains("facebook.com") Then Return Await GetTwarzetnikFeed(oFeed)
            'If oFeed.sUri.ToLower.Contains("instagram.com") Then Return Await GetInstagramFeed(oFeed)
            If oFeed.sUri.ToLower.Contains("facebook.com") Then Return Nothing
            If oFeed.sUri.ToLower.Contains("instagram.com") Then Return Nothing
            Return Await GetRssFeed(oFeed)

        Catch ex As Exception
            CrashMessageAdd("GetAnyFeed switch catch: " & oFeed.sUri, ex)
        End Try

        Return Nothing
    End Function

    Public Delegate Sub UISetTextBox(sTxt As String)
    Public Delegate Sub UISaveIndex()
    Public Delegate Sub UIMakeToast(sGuid As String, sMsg As String, sFeed As String)
    Public Shared bChangedXML As Boolean = False

    Private Shared Sub ZrobToasty(oMakeToast As UIMakeToast, oFeed As VBlib.JedenFeed, oNewList As List(Of VBlib.JedenItem))
        DumpCurrMethod()

        ' zrobienie Toastów
        ' przeniesione z GetRssFeed:NodeToIgnore

        If oFeed.iToastType = FeedToastType.NoToast Then Return

        Dim iToastCnt As Integer = 0
        Dim sToastString As String = ""

        For Each oItem As JedenItem In oNewList

            ' 2021.12.15
            ' Toast w kilku sytuacjach, wedle oFeed.iToastType , istnienia na WhiteList, oraz GetSettingsBool("NotifyWhite")

            iToastCnt += 1

            Select Case oFeed.iToastType
                Case FeedToastType.NoToast
                    ' wtedy nic nie pokazujemy

                Case FeedToastType.NewExist
                    ' pokaże dopiero później

                Case FeedToastType.ListItems
                    sToastString = sToastString & vbCrLf & oItem.sTitle

                Case FeedToastType.Separate
                    oMakeToast(oItem.sGuid, oItem.sTitle, oFeed.sName)

            End Select

        Next

        ' i teraz podsumowanie toastów

        Select Case oFeed.iToastType
            Case VBlib.FeedToastType.ListItems
                If sToastString <> "" Then
                    Try
                        oMakeToast("", GetLangString("resNewItemsList") & " (" & iToastCnt & ")" & vbCrLf & sToastString, oFeed.sName)
                    Catch ex As Exception
                        ' too big (np. lista 88 sztuk)
                        oMakeToast("", GetLangString("resNewItemsList") & " (" & iToastCnt & ")", oFeed.sName)
                    End Try
                End If
            Case VBlib.FeedToastType.NewExist
                If iToastCnt > 0 Then
                    oMakeToast("", GetLangString("resNewItemsInFeed") & " (" & iToastCnt & ")", oFeed.sName)
                End If
        End Select

    End Sub

    Public Shared Async Function ReadFeeds(oSetTBox As UISetTextBox, oSaveIndex As UISaveIndex, oMakeToast As UIMakeToast) As Task(Of String)
        DumpCurrMethod()

        ' Poprzednia sekwencja: ReadFeed -> AddFeedItemsSyndic -> GetRssFeed

        Try
            Dim sTmp As String

            For Each oFeed As JedenFeed In Feeds.glFeeds
                oFeed.bLastError = False

                ' przeniesione z AddFeedItemsSyndic
                If oSetTBox IsNot Nothing Then
                    sTmp = oFeed.sUri
                    ' zabezpieczenie numer 2 (poza ustalaniem szerokości w Form_Resize)
                    If sTmp.Length > 64 Then sTmp = sTmp.Substring(0, 64)
                    oSetTBox(sTmp)
                End If

                Dim oNewList As List(Of JedenItem) = Await GetAnyFeed(oFeed)

                ' przeniesione z GetRssFeed
                If oNewList Is Nothing Then
                    ' był błąd
                    If oSetTBox IsNot Nothing Then Await DialogBoxAsync("Bad response from " & oFeed.sName)
                    Continue For
                End If

                If oNewList IsNot Nothing Then oSaveIndex() ' local, nie Roaming

                ' przeniesione z GetRssFeed
                If oNewList.Count < 1 Then
                    ' nic nie ma... sprawdz czy to nie error! (czy nie za dlugo nie ma)
                    If VBlib.App.ShouldShowEmptyFeedWarning(oFeed.sLastOkDate) Then
                        If oSetTBox IsNot Nothing Then Await DialogBoxAsync("Seems like feed " & oFeed.sName & " is dead?")
                    End If
                    Continue For
                End If

                bChangedXML = True

                glItems = glItems.Concat(oNewList).ToList

                ' zrobienie Toastów
                ' przeniesione z GetRssFeed:NodeToIgnore
                If oFeed.iToastType <> VBlib.FeedToastType.NoToast Then ZrobToasty(oMakeToast, oFeed, oNewList)
            Next

            sTmp = "Last read: " & Date.Now().ToString
            SetSettingsString("lastRead", sTmp)

            ' specjane dla DevilTorrents; musi byc po wczytaniu wszystkich
            ' AppSuspending() ' ewentualnie zapisz dane także tu (na wypadek crash programu)

            Return sTmp
        Catch ex As Exception
            CrashMessageAdd("ReadFeed catch", ex)
        End Try

        Return "ERROR"
    End Function

    ' public, bo z app.xaml.vb, przy powrocie na Windows.RSS
    Public Shared Function NodeToIgnore(oFeed As JedenFeed, dPublDate As DateTimeOffset, oNew As JedenItem) As Boolean ' sFeedName As String

        DumpCurrMethod("(" & oFeed.sName & ", title=" & oNew.sTitle)

        ' 2018.11.09 RSS z TomsHardware nie ma daty (wszystkie to 1/1/1601)
        ' Nothing: gdy na razie nie mamy jak porównywać
        If dPublDate <> Nothing AndAlso dPublDate > New DateTime(1900, 1, 1) Then
            If dPublDate.AddDays(oFeed.iMaxDays) < Date.Now Then
                DebugOut(2, "ignoring because of being too old")
                Return True
            End If
        End If

        Dim bIgnore As Boolean = False
        Dim bWhite As Boolean = False

        Dim sPhrases As String()
        sPhrases = oFeed.sGlobalBlack.Split(vbCrLf)
        For Each sWord As String In sPhrases
            If TestNodeMatch(sWord, oNew.sTitle, oNew.sItemHtmlData, oFeed.sName) Then
                bIgnore = True
                DebugOut(2, "ignoring because of global BlackList entry: " & sWord)
                Exit For
            End If
        Next

        sPhrases = oFeed.sBlacklist.Split(vbCrLf)
        For Each sWord As String In sPhrases
            If TestNodeMatch(sWord, oNew.sTitle, oNew.sItemHtmlData, oFeed.sName) Then
                bIgnore = True
                DebugOut(2, "ignoring because of feed BlackList entry: " & sWord)
                Exit For
            End If
        Next

        ' killfile
        Dim aKills As String() = GetKillFileContent.Split(vbCrLf)
        DebugOut(3, "starting KillFile, entries " & aKills.Length)
        For Each sKillLine As String In GetKillFileContent.Split(vbCrLf)
            DebugOut(4, "testing entry " & sKillLine)
            Dim iInd As Integer = sKillLine.IndexOf(vbTab)
            If iInd > 2 Then
                Dim sFiltr As String = sKillLine.Substring(iInd + 1).Replace(vbCr, "").Replace(vbLf, "").Trim
                If RegExpOrInstr(oNew.sTitle, sFiltr) Then
                    DebugOut(2, "ignoring because of KillFile entry: " & sFiltr)
                    bIgnore = True
                    Exit For
                Else
                    DebugOut(4, "kill entry no match: " & sFiltr)
                End If
            Else
                DebugOut(3, "entry ignored (vbTab too soon): " & sKillLine)
            End If
        Next

        sPhrases = oFeed.sGlobalWhite.Split(vbCrLf)
        For Each sWord As String In sPhrases
            If sWord = "" Then Continue For
            If sWord = "*" Then
                bWhite = True
                'bIgnore = False
            ElseIf sWord.Substring(1, 2).ToLower = "f:" Then

            Else
                If TestNodeMatch(sWord, oNew.sTitle, oNew.sItemHtmlData, oFeed.sName) Then
                    bIgnore = False
                    bWhite = True
                    Exit For
                End If
            End If
        Next

        sPhrases = oFeed.sWhitelist.Split(vbCrLf)
        For Each sWord As String In sPhrases
            If sWord = "" Then Continue For
            If sWord = "*" Then
                bWhite = True
                'bIgnore = False
            ElseIf sWord.Substring(1, 2).ToLower = "f:" Then

            Else
                If TestNodeMatch(sWord, oNew.sTitle, oNew.sItemHtmlData, oFeed.sName) Then
                    bIgnore = False
                    bWhite = True
                    Exit For
                End If
            End If
        Next


        Return bIgnore

    End Function

    Public Shared Function ConvertGuidToTag(sGuid As String) As String
        ' The size of the notification tag is too large.
        ' tag: 16 chrs, od Creators (14971 ??) jest 64 chrs
        ' https://docs.microsoft.com/en-us/uwp/api/windows.ui.notifications.toastnotification.tag#Windows_UI_Notifications_ToastNotification_Tag
        If sGuid.Length > 60 Then sGuid = sGuid.Substring(0, 60)
        Return sGuid
    End Function

    Public Shared Function ShouldShowEmptyFeedWarning(sLastOkTimeStamp As String) As Boolean
        DumpCurrMethod("sLastOkTimeStamp=" & sLastOkTimeStamp)

        ' nic nie ma... sprawdz czy to nie error! (czy nie za dlugo nie ma)
        Dim sCurrDate As String = Date.Now.ToString("yyMMdd")
        If sLastOkTimeStamp = "" Then Return False

        Dim iCurrDate As Integer = Integer.Parse(sCurrDate)
        Dim iLastItem As Integer = Integer.Parse(sLastOkTimeStamp)
        If iLastItem + GetSettingsInt("MaxDays") < iCurrDate Then Return True

        Return False
    End Function


#Region "RegExpy"

    ''' <summary>
    ''' Zwraca 0: nie, 1: tak, 10: nie regexp, 11: timeout
    ''' </summary>
    Public Shared Function RegExp(sTxt As String, sPattern As String) As Integer

        Dim iRet As Integer = 99
        Try
            If Text.RegularExpressions.Regex.Match(sTxt, sPattern).Success Then
                iRet = 1
            Else
                iRet = 0
            End If
        Catch ex As ArgumentNullException   ' input or pattern is Nothing.
            iRet = 0
        Catch ex As ArgumentException   ' to nie regex
            iRet = 10
        Catch ex As Text.RegularExpressions.RegexMatchTimeoutException ' A time-out occurred. For more 
            iRet = 11
        End Try

        Return iRet

    End Function

    ''' <summary>
    ''' Zwraca True, gdy RegExpMatch, lub gdy to InStr
    ''' </summary>
    Public Shared Function RegExpOrInstr(sTxt As String, sPattern As String) As Boolean
        Dim iRet As Integer
        iRet = RegExp(sTxt, sPattern)
        If iRet = 0 Then Return False
        If iRet = 1 Then Return True
        If iRet <> 10 Then Return False ' inne bledy niz "bad regexp"
        Return sTxt.IndexOf(sPattern) > -1
    End Function

    Public Shared Function RegExpPerLine(sTxt As String, sPattern As String) As Boolean
        Dim aLines As String() = sTxt.Split("<br>")    ' zapewne beda "text|<|br>text|"
        If aLines.Count < 2 Then aLines = sTxt.Split("<br/>")
        For Each sLine As String In aLines
            If App.RegExpOrInstr(sLine, sPattern) Then Return True
        Next
        Return False
    End Function

    Public Shared Function TestNodeMatch(sPattern As String, sT As String, sD As String, sF As String) As Boolean
        If sPattern.Length < 3 Then Return False

        If sPattern.Substring(0, 2).ToLower = "t:" Then Return RegExpOrInstr(sT, sPattern.Substring(2))
        If sPattern.Substring(0, 2).ToLower = "d:" Then Return RegExpPerLine(sD, sPattern.Substring(2))
        If sPattern.Substring(0, 2).ToLower = "f:" Then Return RegExpPerLine(sF, sPattern.Substring(2))

        If RegExpOrInstr(sT, sPattern) Then Return True
        Return RegExpPerLine(sD, sPattern)

    End Function
#End Region

    Public Shared Function Url2VarName(sUrl As String) As String
        Dim sGuidsValueName As String = sUrl.Replace("/", "")
        sGuidsValueName = sGuidsValueName.Replace("?", "")
        sGuidsValueName = sGuidsValueName.Replace(":", "")
        Return sGuidsValueName
    End Function

#Region "Index file"

    Private Const INDEX_FILENAME_XML As String = "glItems.xml"
    Private Const INDEX_FILENAME_JSON As String = "glItems.json"

    Public Shared Sub SaveIndex(sCacheFolder As String)
        Dim sFilename As String = IO.Path.Combine(sCacheFolder, INDEX_FILENAME_JSON)
        Dim sTxt As String = Newtonsoft.Json.JsonConvert.SerializeObject(glItems, Newtonsoft.Json.Formatting.Indented)
        IO.File.WriteAllText(sFilename, sTxt)
    End Sub

    Public Shared Sub LoadIndex(sCacheFolder As String)

        ' 20171101: omijamy wczytywanie gdy zmienna nie jest wyczyszczona
        If glItems IsNot Nothing Then
            If glItems.Count > 0 Then Return
        End If

        Dim sFilename As String = IO.Path.Combine(sCacheFolder, INDEX_FILENAME_JSON)
        If IO.File.Exists(sFilename) Then
            Try
                Dim sTxt As String = IO.File.ReadAllText(sFilename)
                If sTxt.Length > 10 Then
                    glItems = Newtonsoft.Json.JsonConvert.DeserializeObject(sTxt, GetType(List(Of JedenItem)))
                Else
                    glItems = New List(Of JedenItem)
                End If
            Catch ex As Exception
                CrashMessageAdd("LoadIndex JSON catch", ex)
            End Try
        Else
            sFilename = IO.Path.Combine(sCacheFolder, INDEX_FILENAME_XML)
            If Not IO.File.Exists(sFilename) Then Return
            Try
                Dim sTxt As String = IO.File.ReadAllText(sFilename)
                If sTxt.Length > 10 Then
                    ' bez tego był catch że file nie ma root, gdy była len=0
                    Dim oSer As Xml.Serialization.XmlSerializer
                    oSer = New Xml.Serialization.XmlSerializer(GetType(List(Of JedenItem)))
                    glItems = TryCast(oSer.Deserialize(New IO.StringReader(sTxt)), List(Of JedenItem))
                Else
                    glItems = New List(Of JedenItem)
                End If

            Catch ex As Exception
                CrashMessageAdd("LoadIndex XML catch", ex)
                End Try

            End If

    End Sub
#End Region

#Region "Killfile"

    Private Shared mKillFileContent As String = ""
    Private Shared mKillFilePathname As String = ""

    Public Shared Function GetKillFileContent() As String
        Return mKillFileContent
    End Function

    ''' <summary>
    ''' Dodaj wpis do pliku
    ''' </summary>
    Public Shared Sub KillFileAddEntry(sMask As String)
        ' zakładam że było wcześniej wczytanie (ustawiona zmienna ścieżki)
        Dim sLine As String = DateTime.Now.ToString("yyyyMMdd") & vbTab & sMask & vbCrLf
        mKillFileContent = mKillFileContent & sLine
        IO.File.AppendAllText(mKillFilePathname, sLine)
    End Sub

    Private Shared Function KillFileLoadMain(bMsg As Boolean) As String
        DumpCurrMethod()

        If Not IO.File.Exists(mKillFilePathname) Then
            DumpMessage("KillFileLoadMain - file doesnt exist")
            Return ""
        End If

        Dim sRet As String = IO.File.ReadAllText(mKillFilePathname)
        Return sRet

    End Function

    Public Shared Sub KillFileLoad(bMsg As Boolean, sKillFileDirPathname As String)
        DumpCurrMethod("sKillFileDirPathname=" & sKillFileDirPathname)
        mKillFilePathname = IO.Path.Combine(sKillFileDirPathname, "killfile.txt")

        Dim sWpisy As String = KillFileLoadMain(bMsg)
        DumpMessage("read len: " & sWpisy.Length)
        If sWpisy = "" Then Return
        Dim sDataLimit As String = DateTime.Now.AddDays(-30).ToString("yyyyMMdd")
        DumpMessage("sDataLimit: " & sDataLimit)
        Dim bUsunieto As Boolean = False

        mKillFileContent = ""
        For Each sLine In sWpisy.Split(vbCrLf)
            sLine = sLine.Replace(vbCr, "").Replace(vbLf, "").Trim
            DumpMessage("kill line: " & sLine, 2)
            If sLine.Length < 10 Then
                DumpMessage("kill line too short, ignoring", 2)
                Continue For
            End If
            If sLine > sDataLimit Then
                DumpMessage("added to kill memory kill file", 2)
                mKillFileContent = mKillFileContent & sLine & vbCrLf
            Else
                DumpMessage("too old - ignoring", 2)
                bUsunieto = True
            End If
        Next

        If bUsunieto Then
            'DebugOut(2, "some kill entries timeouted - saving new version of kill file")
            IO.File.WriteAllText(mKillFilePathname, mKillFileContent)
        End If
    End Sub
#End Region




End Class

