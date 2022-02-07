
Public Class Setup

    Public Shared Async Sub GetNewFeed(sUri As String)

        If Feeds.TryInitMyDefaultFeeds(sUri) Then Return

        Dim oNew As VBlib.JedenFeed = Feeds.FeedsCreateNew(sUri)
        HttpPageSetAgent("FilteredRSS")
        ' spróbuj nazwać Feed, niekoniecznie z URLu

        If sUri.ToLower.Contains("twitter.com") Then
            Dim sPage As String = Await HttpPageAsync(sUri)
            Dim oDoc As New HtmlAgilityPack.HtmlDocument()
            oDoc.LoadHtml(sPage)
            Dim oTitleList As HtmlAgilityPack.HtmlNodeCollection = oDoc.DocumentNode.SelectNodes("//head/title")

            For Each oTitle As HtmlAgilityPack.HtmlNode In oTitleList
                Dim sTitle As String = oTitle.InnerText
                Dim iInd As Integer = sTitle.IndexOf("(@")
                If iInd > 0 Then sTitle = sTitle.Substring(0, iInd)
                oNew.sName = sTitle
                oNew.iNameType = FeedNameType.FromFeed
            Next
        End If

        If sUri.ToLower.Contains("facebook.com") Then
            Dim sPage As String = Await HttpPageAsync(sUri)
            Dim oDoc As New HtmlAgilityPack.HtmlDocument()
            oDoc.LoadHtml(sPage)
            Dim oTitleList As HtmlAgilityPack.HtmlNodeCollection = oDoc.DocumentNode.SelectNodes("//head/title")

            For Each oTitle As HtmlAgilityPack.HtmlNode In oTitleList
                Dim sTitle As String = oTitle.InnerText
                Dim iInd As Integer = sTitle.IndexOf(" - ")
                If iInd > 0 Then sTitle = sTitle.Substring(0, iInd)
                oNew.sName = sTitle ' 'prawdziwa'
                oNew.sName2 = sTitle ' ta, która jest widoczna i edytowalna
                oNew.iNameType = FeedNameType.FromFeed
            Next
        End If

        If oNew.iNameType <> FeedNameType.FromFeed Then

            Try

                Dim oRssFeed As ServiceModel.Syndication.SyndicationFeed = Nothing

                Dim oReq As Net.HttpWebRequest = Net.HttpWebRequest.Create(sUri)
                oReq.UserAgent = "FilteredRSS" '  konieczne dla LoveKrakow było w Windows.Web.Syndication.SyndicationClient, więc niech będzie

                Using oResp = oReq.GetResponse
                    Using oStream = oResp.GetResponseStream
                        Using oReader = Xml.XmlReader.Create(oStream)
                            oRssFeed = ServiceModel.Syndication.SyndicationFeed.Load(oReader)
                        End Using
                    End Using
                End Using

                oNew.sName = oRssFeed.Title.Text.Trim
                oNew.iNameType = FeedNameType.FromFeed
            Catch ex As Exception
            End Try
        End If

        Feeds.glFeeds.Add(oNew)

    End Sub

End Class


