
Public Class Setup

    Public Sub GetNewFeed(sUri As String)

        If Feeds.TryInitMyDefaultFeeds(sUri) Then Return

        Dim oNew As VBlib.JedenFeed = Feeds.FeedsCreateNew(sUri)
        HttpPageSetAgent("FilteredRSS")
        ' spróbuj nazwać Feed, niekoniecznie z URLu

        VBlib.DumpMessage("nametype: " & oNew.iNameType)

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
                oNew.sName = oRssFeed.Title?.Text?.Trim
                If oNew.sName Is Nothing Then oNew.sName = oNew.sName2
            Catch ex As Exception
                VBlib.CrashMessageAdd("Nieudane dodanie Feedu", ex)
            End Try

            oNew.iNameType = FeedNameType.FromFeed


        End If

        Feeds.glFeeds.Add(oNew)
    End Sub

End Class


