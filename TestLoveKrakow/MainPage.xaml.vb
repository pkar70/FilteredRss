
Imports Windows.Web.Syndication
Public NotInheritable Class MainPage
    Inherits Page

    Private sUrl As String = "https://lovekrakow.pl/rss/aktualnosci.html"


    Private Async Sub uiClick_Click(sender As Object, e As RoutedEventArgs)
        ' LoveKrakow
        GetTwarzetnikFeed("https://www.facebook.com/Roman-Giertych-strona-oficjalna-215392231834473", Nothing)
    End Sub

    Private Async Function DialogBox(sMsg As String) As Task
        Debug.WriteLine(sMsg)
    End Function

    Private Async Function GetTwarzetnikFeed(sUrl As String, oTB As TextBlock) As Task
        ' https://www.facebook.com/Roman-Giertych-strona-oficjalna-215392231834473
        If sUrl.ToLower.Contains("/groups/") Then
            If oTB IsNot Nothing Then Await DialogBox("Facebook groups are not implemented yet")
            Return
        End If

        Dim iInd As Integer
        iInd = sUrl.LastIndexOf("-")
        If iInd < 5 Then
            If oTB IsNot Nothing Then Await DialogBox("There should be '-' in Facebook url")
            Return
        End If
        Dim sFBpageId As String = sUrl.Substring(iInd + 1)

        ' https://www.facebook.com/pages_reaction_units/more/?page_id=215392231834473&cursor=%7B%22card_id%22%3A%22page_composer_card%22%2C%22has_next_page%22%3Atrue%7D&surface=www_pages_home&unit_count=8&referrer&fb_dtsg_ag=AQywDlwLfJw3cadTO7Np6sm5urbHJftL9vNT_Tvpeyj9Ig%3AAQzOPH9RbR9ln8nQZaclHIzDVHInxcopJCDzfFJBJ_osfA&__user=100000695378362&__a=1&__dyn=7AgNe-4amaxx2oqGSEWC5EW3m8GEW8xd4Wo8oeES2N6xucxu13wmEW4UJoK6UnGi7UK7HzEeUG3yczobohx3wCxC78O5U6y58iwBx61cxq2e1tG3i1VDCwDwLwxw-KEtxy5U4m12wgo-fw8C48szU2axC4EhwIXxK8CgjU8UlzUOmUpwAwlod86Ch4Bx3zHAy8aEaoGqfwhUO68pwAwjEW5AbxS227Ua8y4EgwtouG2O2WE9EjwtUW2mfxW686-1dwoUe888co5G&__csr=&__req=8&__beoa=0&__pc=PHASED%3ADEFAULT&dpr=1&__rev=1001987030&__s=uzpc7j%3Aul2ki1%3Ajuny6j&__hsi=6815864224167561001-0&__comet_req=0&jazoest=28329&__spin_r=1001987030&__spin_b=trunk&__spin_t=1586865921
        ' https://www.facebook.com/pages_reaction_units/more/?page_id=215392231834473&cursor=%7B%22card_id%22%3A%22page_composer_card%22%2C%22has_next_page%22%3Atrue%7D&surface=www_pages_home&unit_count=8&referrer&fb_dtsg_ag=AQywDlwLfJw3cadTO7Np6sm5urbHJftL9vNT_Tvpeyj9Ig%3AAQzOPH9RbR9ln8nQZaclHIzDVHInxcopJCDzfFJBJ_osfA&__user=100000695378362&__a=1&__dyn=7AgNe-4amaxx2oqGSEWC5EW3m8GEW8xd4Wo8oeES2N6xucxu13wmEW4UJoK6UnGi7UK7HzEeUG3yczobohx3wCxC78O5U6y58iwBx61cxq2e1tG3i1VDCwDwLwxw-KEtxy5U4m12wgo-fw8C48szU2axC4EhwIXxK8CgjU8UlzUOmUpwAwlod86Ch4Bx3zHAy8aEaoGqfwhUO68pwAwjEW5AbxS227Ua8y4EgwtouG2O2WE9EjwtUW2mfxW686-1dwoUe888co5G&__csr=&__req=8&__beoa=0&__pc=PHASED%3ADEFAULT&dpr=1&__rev=1001987030&__s=uzpc7j%3Aul2ki1%3Ajuny6j&__hsi=6815864224167561001-0&__comet_req=0
        ' https://www.facebook.com/pages_reaction_units/more/?page_id=215392231834473&cursor=%7B%22card_id%22%3A%22page_composer_card%22%2C%22has_next_page%22%3Atrue%7D&surface=www_pages_home&unit_count=8&referrer&fb_dtsg_ag=AQywDlwLfJw3cadTO7Np6sm5urbHJftL9vNT_Tvpeyj9Ig%3AAQzOPH9RbR9ln8nQZaclHIzDVHInxcopJCDzfFJBJ_osfA&__user=100000695378362&__a=1&__dyn=7AgNe-4amaxx2oqGSEWC5EW3m8GEW8xd4Wo8oeES2N6xucxu13wmEW4UJoK6UnGi7UK7HzEeUG3yczobohx3wCxC78O5U6y58iwBx61cxq2e1tG3i1VDCwDwLwxw-KEtxy5U4m12wgo-fw8C48szU2axC4EhwIXxK8CgjU8UlzUOmUpwAwlod86Ch4Bx3zHAy8aEaoGqfwhUO68pwAwjEW5AbxS227Ua8y4EgwtouG2O2WE9EjwtUW2mfxW686-1dwoUe888co5G
        ' https://www.facebook.com/pages_reaction_units/more/?page_id=215392231834473&cursor=%7B%22card_id%22%3A%22page_composer_card%22%2C%22has_next_page%22%3Atrue%7D&surface=www_pages_home&unit_count=8&referrer&fb_dtsg_ag=AQywDlwLfJw3cadTO7Np6sm5urbHJftL9vNT_Tvpeyj9Ig%3AAQzOPH9RbR9ln8nQZaclHIzDVHInxcopJCDzfFJBJ_osfA&__user=100000695378362&__a=1
        ' &__a=1    - konieczne
        ' https://www.facebook.com/pages_reaction_units/more/?page_id=215392231834473&cursor=%7B%22card_id%22%3A%22page_composer_card%22%2C%22has_next_page%22%3Atrue%7D&surface=www_pages_home&unit_count=8&referrer&fb_dtsg_ag=AQywDlwLfJw3cadTO7Np6sm5urbHJftL9vNT_Tvpeyj9Ig%3AAQzOPH9RbR9ln8nQZaclHIzDVHInxcopJCDzfFJBJ_osfA&__a=1
        ' &fb_dtsg_ag=AQywDlwLfJw3cadTO7Np6sm5urbHJftL9vNT_Tvpeyj9Ig%3AAQzOPH9RbR9ln8nQZaclHIzDVHInxcopJCDzfFJBJ_osfA
        ' https://www.facebook.com/pages_reaction_units/more/?page_id=215392231834473&cursor=%7B%22card_id%22%3A%22page_composer_card%22%2C%22has_next_page%22%3Atrue%7D&surface=www_pages_home&unit_count=8&fb_dtsg_ag=AQywDlwLfJw3cadTO7Np6sm5urbHJftL9vNT_Tvpeyj9Ig%3AAQzOPH9RbR9ln8nQZaclHIzDVHInxcopJCDzfFJBJ_osfA&__a=1
        ' &surface=www_pages_home&unit_count=8  - konieczne
        ' &cursor=%7B%22card_id%22%3A%22page_composer_card%22%2C%22has_next_page%22%3Atrue%7D   - konieczne

        ' czyli konieczne:
        ' page_id   : do wzięcia z sUrl
        ' cursor    : standardowy tekst, {"card_id":"page_composer_card","has_next_page":true}
        ' surface   : standardowy tekst, www_pages_home
        ' unit_count: pewnie liczba wpisów?
        ' fb_dtsg_ag: długie, nie wiem co to, ale można zmieniać? działa także AQywDlwLfJw3cadTO7Np6sm5urbHJftL9vNT_Tvpeyj9Ig%3A [reszta ucięta, to %3A jest ważne, nie może się zmienić przed %3A nic]
        ' __a       : standardowe == 1

        ':authority : www.facebook.com
        ':method :  GET
        ':path :  /Roman-Giertych-strona-oficjalna-215392231834473
        ':scheme : https

        'user-agent: Mozilla/ 5.0(Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, Like Gecko) Chrome/83.0.4103.3 Safari/537.36 Edg/83.0.478.5
        'accept-encoding: gzip, deflate, br
        'accept-language: en-US, en;q=0.9, pl;q=0.8
        'accept: Text/ html, Application / xhtml + Xml, Application / Xml;q=0.9, Image / webp, Image / apng,*/*;q=0.8, Application / signed - exchange;v=b3;q=0.9


        ' nowe
        'cookie: sb=Q4YMXXzIskIXbUfg42gC8npd; datr=UoYMXZJhmpcG8_1PmfhHJ_Tp; c_user=100000695378362; xs=44%3AEPh8LW-86hn2pw%3A2%3A1562702724%3A6009%3A13319; wd=1280x923; spin=r.1001993288_b.trunk_t.1586959838_s.1_v.2_; fr=3pK1l7UUV6pmiMo2k.AWW-jbGOH-pglIJe1_8H8BNpi6A.BdDIZD.mK.F6W.0.0.BelxXm.AWVt8W1i
        'dnt: 1
        'sec-fetch-dest: document
        'sec-fetch-mode: navigate
        'sec-fetch-site: cross-site
        'sec-fetch-user: ?1
        'upgrade-insecure-requests: 1
        'user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.3 Safari/537.36 Edg/83.0.478.5

        'dnt: 1
        'sec-fetch - dest: document
        'sec-fetch - mode: navigate
        'sec-fetch - site: cross-site
        'sec-fetch - user: ? 1
        'upgrade-insecure - requests:  1

        Dim oHttp As Windows.Web.Http.HttpClient = New Windows.Web.Http.HttpClient
        Dim sError = ""
        Dim oResp As Windows.Web.Http.HttpResponseMessage = Nothing
        oHttp.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.3 Safari/537.36 Edg/83.0.478.5")
        ' accept-Encoding: gzip, deflate, br
        oHttp.DefaultRequestHeaders.AcceptEncoding.Add(New Windows.Web.Http.Headers.HttpContentCodingWithQualityHeaderValue("gzip"))
        oHttp.DefaultRequestHeaders.AcceptEncoding.Add(New Windows.Web.Http.Headers.HttpContentCodingWithQualityHeaderValue("deflate"))
        oHttp.DefaultRequestHeaders.AcceptEncoding.Add(New Windows.Web.Http.Headers.HttpContentCodingWithQualityHeaderValue("br"))
        ' accept-Language: en-US,en;q=0.9,pl;q=0.8
        oHttp.DefaultRequestHeaders.AcceptLanguage.Add(New Windows.Web.Http.Headers.HttpLanguageRangeWithQualityHeaderValue("en-US"))
        oHttp.DefaultRequestHeaders.AcceptLanguage.Add(New Windows.Web.Http.Headers.HttpLanguageRangeWithQualityHeaderValue("en", 0.9))
        oHttp.DefaultRequestHeaders.AcceptLanguage.Add(New Windows.Web.Http.Headers.HttpLanguageRangeWithQualityHeaderValue("pl", 0.8))
        ' accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9
        ' ale takie tez dziala:
        ' "accept"="text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
        oHttp.DefaultRequestHeaders.Accept.Add(New Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("text/html"))
        oHttp.DefaultRequestHeaders.Accept.Add(New Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("application/xhtml+xml"))
        oHttp.DefaultRequestHeaders.Accept.Add(New Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("application/xml", 0.9))
        'oHttp.DefaultRequestHeaders.Accept.Add(New Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("image/webp"))
        'oHttp.DefaultRequestHeaders.Accept.Add(New Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("image/apng"))
        oHttp.DefaultRequestHeaders.Accept.Add(New Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("*/*", 0.8))
        'oHttp.DefaultRequestHeaders.Accept.Add(New Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("application/signed-exchange;v=b3", 0.9))
        ' Connection: keep-alive
        ' oHttp.DefaultRequestHeaders.Connection.Add(New Windows.Web.Http.Headers.HttpConnectionOptionHeaderValue("keep-alive"))
        ' Host:   pzn.pkn.pl
        ' oHttp.DefaultRequestHeaders.Host = New Windows.Networking.HostName("www.facebook.com")

        'oHttp.DefaultRequestHeaders.Add("dnt", "1")
        'oHttp.DefaultRequestHeaders.Add("sec-fetch-dest", "document")
        'oHttp.DefaultRequestHeaders.Add("'sec-fetch-mode", "navigate")
        'oHttp.DefaultRequestHeaders.Add("'sec-fetch-site", "cross-site")
        'oHttp.DefaultRequestHeaders.Add("'sec-fetch-user", "?1")
        'oHttp.DefaultRequestHeaders.Add("'upgrade-insecure-requests", "1")

        Try
            oResp = Await oHttp.GetAsync(New Uri(sUrl)) '  & "?_fb_noscript=1" , "/posts"
        Catch ex As Exception
            sError = ex.Message
        End Try
        If oResp.StatusCode = 303 Or oResp.StatusCode = 302 Or oResp.StatusCode = 301 Then
            sUrl = oResp.Headers.Location.ToString
            oResp = Await oHttp.GetAsync(New Uri(sUrl))
        End If

        If oResp.StatusCode > 290 Then
            Await DialogBox("ERROR " & oResp.StatusCode)
            Return
        End If

        Dim sResp As String = ""
        Try
            sResp = Await oResp.Content.ReadAsStringAsync
        Catch ex As Exception
            sError = ex.Message
        End Try

        If sError <> "" Then
            Await DialogBox("error " & sError & " at ReadAsStringAsync " & sUrl & " page")
            Return
        End If



        Dim sPage As String
        iInd = sResp.IndexOf("PagesProfileHomePrimaryColumnPagelet")
        sPage = sResp.Substring(iInd)
        Dim iInd1 As Integer

        iInd = sPage.IndexOf("story-subtitle")
        While iInd > 0
            Debug.WriteLine(vbCrLf & vbCrLf & "nowy post: " & vbCrLf)

            iInd = sPage.LastIndexOf("<img", iInd)
            iInd = sPage.IndexOf("src=", iInd) + 5  ' src="
            iInd1 = sPage.IndexOf("""", iInd)
            Debug.WriteLine(" piclink = " & sPage.Substring(iInd, iInd1 - iInd + 2))    ' PLUS DWA kontroli konca

            iInd = sPage.IndexOf("a href", iInd)
            iInd = sPage.IndexOf(">", iInd) + 1
            iInd1 = sPage.IndexOf("<", iInd)
            Debug.WriteLine(" feed = " & sPage.Substring(iInd, iInd1 - iInd + 2))    ' PLUS DWA kontroli konca

            iInd = sPage.IndexOf("/permalink", iInd)
            iInd1 = sPage.IndexOf("""", iInd + 1)
            Debug.WriteLine(" linktodescr = " & sPage.Substring(iInd, iInd1 - iInd + 2))    ' PLUS DWA kontroli konca
            ' ewentualnie, przy dluzszych historiach...

            iInd = sPage.IndexOf("data-utime", iInd) + "data-utime=""".Length
            iInd1 = sPage.IndexOf("""", iInd)
            Dim sTmp As String = sPage.Substring(iInd, iInd1 - iInd)
            Debug.WriteLine(" guid = " & sTmp)
            Debug.WriteLine(" data = " & DateTimeOffset.FromUnixTimeSeconds(sTmp).ToString("f"))

            iInd = sPage.IndexOf("post_message", iInd)
            iInd = sPage.IndexOf("<p>", iInd)
            iInd1 = sPage.IndexOf("<", iInd + 3)
            sTmp = sPage.Substring(iInd + 3, iInd1 - iInd - 3)
            Debug.WriteLine(" title = " & sTmp)

            iInd1 = sPage.IndexOf("</div", iInd)
            sTmp = sPage.Substring(iInd, iInd1 - iInd)
            sTmp = sTmp.Replace("<span class=""text_exposed_hide"">...</span>", "") ' bo calosc pokazujemy
            Debug.WriteLine(" tekst = " & sTmp)

            sPage = sPage.Substring(iInd1)

            iInd = sPage.IndexOf("story-subtitle")
        End While




        '' ściągamy stronę główną danego ludzia (dość długa)
        '' potrzebujemy znać fb_dtsg_ag
        ''Dim sPage As String = Await HttpPageAsync(sUrl, "")
        ''If sPage = "" Then
        ''    If oTB IsNot Nothing Then Await DialogBox("Bad response from" & vbCrLf & sUrl)
        ''    Return
        ''End If

        'iInd = sPage.IndexOf("""async_get_token"": """)
        ''                                     012345678901234566788
        'If iInd < 10 Then
        '    If oTB IsNot Nothing Then Await DialogBox("Bad response from" & vbCrLf & sUrl)
        '    Return
        'End If
        'iInd += 19  ' przesuwam na początek
        'Dim iInd1 As Integer = sPage.IndexOf("}", iInd)
        'Dim sToken As String = sPage.Substring(iInd, iInd1 - iInd - 1)  ' bez «"}»

        'Dim sNextUri As String = "https://www.facebook.com/pages_reaction_units/more/?page_id=" & sFBpageId &
        '    "&cursor=%7B%22card_id%22%3A%22page_composer_card%22%2C%22has_next_page%22%3Atrue%7D" &
        '    "&surface=www_pages_home&unit_count=8&fb_dtsg_ag=" &
        '    Net.WebUtility.UrlEncode(sToken) & "__a=1"
        'sPage = Await HttpPageAsync(sUrl, "")
        'If sPage = "" Then
        '    If oTB IsNot Nothing Then Await DialogBox("No second response from" & vbCrLf & sUrl)
        '    Return
        'End If

        'iInd = sPage.IndexOf("{""__html"":""")
        'If iInd < 5 Then
        '    If oTB IsNot Nothing Then Await DialogBox("Bad second response from" & vbCrLf & sUrl)
        '    Return
        'End If
        'sPage = sPage.Substring(iInd + 11)
        'iInd = sPage.IndexOf("}")
        'sPage = sPage.Substring(0, iInd - 2)
        'sPage = Net.WebUtility.HtmlDecode(sPage)

    End Function
    Private Async Sub LoveKrakow()

        Dim oRssClnt As SyndicationClient = New SyndicationClient
        Dim oRssFeed As SyndicationFeed = Nothing
        'Dim bErr As Boolean = False
        Dim sErr As String = ""

        ' Internal server error (500). (Exception from HRESULT: 0x801901F4)'
        oRssClnt.SetRequestHeader("User-Agent", "FilteredRSS")
        oRssFeed = Await oRssClnt.RetrieveFeedAsync(New Uri(sUrl))


        Dim moHttp = New Windows.Web.Http.HttpClient
        ' text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9
        'moHttp.DefaultRequestHeaders.Accept.TryParseAdd("text/html")
        'moHttp.DefaultRequestHeaders.Accept.TryParseAdd("application/xhtml+xml")
        'moHttp.DefaultRequestHeaders.Accept.TryParseAdd("application/xml;q=0.9")
        'moHttp.DefaultRequestHeaders.Accept.TryParseAdd("image/webp")
        'moHttp.DefaultRequestHeaders.Accept.TryParseAdd("image/apng")
        'moHttp.DefaultRequestHeaders.Accept.TryParseAdd("*/*;q=0.8")
        'moHttp.DefaultRequestHeaders.Accept.TryParseAdd("application/signed-exchange;v=b3;q=0.9")

        'moHttp.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("en-US,en;q=0.9")
        'moHttp.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("pl;q=0.8")
        ' en-US,en;q=0.9,pl;q=0.8

        'moHttp.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4101.0 Safari/537.36 Edg/83.0.474.0")
        moHttp.DefaultRequestHeaders.UserAgent.TryParseAdd("FilteredRSS")


        Dim oResp = Await moHttp.GetAsync(New Uri(sUrl))


    End Sub
End Class
