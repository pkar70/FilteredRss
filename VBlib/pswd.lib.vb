Partial Public Class feeds

    '  dane initial z dnia 2021.12.15
    Public Shared Function TryInitMyDefaultFeeds(sFeedName As String) As Boolean
        If sFeedName <> "PKAR_init" Then Return False

        FeedsTryAddNew("https://devil-torrents.pl/rss/kategoria/Filmy%20XviD/DivX", "Devil Xvid")
        FeedsTryAddNew("https://devil-torrents.pl/rss/kategoria/Filmy%204K", "Devil 4K")
        FeedsTryAddNew("https://devil-torrents.pl/rss/kategoria/Filmy%20Blu-Ray/HD", "Devil HD")
        FeedsTryAddNew("https://devil-torrents.pl/rss/kategoria/Filmy%20x264/h264", "Devil h264")
        FeedsTryAddNew("https://devil-torrents.pl/rss/kategoria/Filmy%20x265/h265", "Devil h265")
        FeedsTryAddNew("https://devil-torrents.pl/rss/kategoria/Filmy%20DVD", "Devil DVD")
        FeedsTryAddNew("https://devil-torrents.pl/rss/kategoria/TV/Seriale", "Devil seriale")
        FeedsTryAddNew("https://devil-torrents.pl/rss/kategoria/Erotyka", "Devil ero")
        FeedsTryAddNew("https://devil-torrents.pl/rss/kategoria/Muzyka", "Devil music")
        FeedsTryAddNew("https://devil-torrents.pl/rss/kategoria/Programy", "Devil prgs")
        FeedsTryAddNew("https://devil-torrents.pl/rss/kategoria/Ksi%C4%85%C5%BCki", "Devil books")
        FeedsTryAddNew("https://zdmk.krakow.pl/nasze-dzialania/feed/", "ZDMK")
        FeedsTryAddNew("http://onlyoldmovies.blogspot.com/feeds/posts/default", "onlyoldmovies")
        FeedsTryAddNew("https://blogs.technet.microsoft.com/sysinternals/feed/", "sysinternals")
        FeedsTryAddNew("https://exiftool.org/rss.xml", "Exiftool")
        FeedsTryAddNew("https://www.tomshardware.com/feeds/all", "TomsHardware")
        FeedsTryAddNew("http://feeds.feedburner.com/WinaeroBlog/", "WinAero")
        FeedsTryAddNew("https://lovekrakow.pl/rss/aktualnosci.html", "LoveKrakow")
        FeedsTryAddNew("https://twitter.com/JkmMikke", "JK Mikke")
        'FeedsTryAddNew("http://www.comece.eu/backend/rss/atom?channel=comece_news_in_english", "COMECE")
        FeedsTryAddNew("http://www.comece.eu/feed/", "COMECE")
        FeedsTryAddNew("https://www.facebook.com/Roman-Giertych-strona-oficjalna-215392231834473", "Giertych")
        FeedsTryAddNew("https://twitter.com/donaldtusk", "Tusk")

        'FeedsTryAddNew("http://devil-torrents.pl/rss.php?cat=1" & vbCrLf &
        'FeedsTryAddNew("http://devil-torrents.pl/rss.php?cat=4" & vbCrLf &
        'FeedsTryAddNew("http://devil-torrents.pl/rss.php?cat=642" & vbCrLf &
        'FeedsTryAddNew("http://devil-torrents.pl/rss.php?cat=3" & vbCrLf &
        'FeedsTryAddNew("http://devil-torrents.pl/rss.php?cat=362" & vbCrLf &
        'FeedsTryAddNew("http://devil-torrents.pl/rss.php?cat=7" & vbCrLf &
        'FeedsTryAddNew("http://devil-torrents.pl/rss.php?cat=702" & vbCrLf &
        'FeedsTryAddNew("http://devil-torrents.pl/rss.php?cat=8" & vbCrLf &
        'FeedsTryAddNew("http://devil-torrents.pl/rss.php?cat=10" & vbCrLf &
        'FeedsTryAddNew("http://devil-torrents.pl/rss.php?cat=11" & vbCrLf &
        'FeedsTryAddNew("http://devil-torrents.pl/rss.php?cat=16" & vbCrLf &
        'FeedsTryAddNew("http://devil-torrents.pl/rss.php?cat=699" & vbCrLf &
        'FeedsTryAddNew("http://devil-torrents.pl/rss.php?cat=21" & vbCrLf &    ' "http://korwin-mikke.pl/blog/rss" & vbCrLf &
        'FeedsTryAddNew("http://zikit.krakow.pl/feeds/rss/komunikaty/1794" & vbCrLf &
        'FeedsTryAddNew("http://zikit.krakow.pl/feeds/rss/komunikaty/1829" & vbCrLf &
        'FeedsTryAddNew("http://zikit.krakow.pl/feeds/rss/komunikaty/1787" & vbCrLf &
        'FeedsTryAddNew("http://onlyoldmovies.blogspot.com/feeds/posts/default" & vbCrLf &
        'FeedsTryAddNew("https://blogs.technet.microsoft.com/sysinternals/feed/" & vbCrLf &
        'FeedsTryAddNew("http://owl.phy.queensu.ca/~phil/exiftool/rss.xml" & vbCrLf &
        'FeedsTryAddNew("https://support.microsoft.com/app/content/api/content/feeds/sap/en-sg/6ae59d69-36fc-8e4d-23dd-631d98bf74a9/rss" & vbCrLf &
        'FeedsTryAddNew("https://support.microsoft.com/app/content/api/content/feeds/sap/en-sg/2f11206b-c490-cd6f-e033-661968ad085c/rss" & vbCrLf &
        'FeedsTryAddNew("https://support.microsoft.com/app/content/api/content/feeds/sap/en-sg/e9323f40-7ca8-4ecd-621d-fcf6c96a2eb0/rss" & vbCrLf &
        'FeedsTryAddNew("https://support.microsoft.com/app/content/api/content/feeds/sap/en-sg/f3753f34-a410-4d2a-bbda-72c4abfe87d7/rss" & vbCrLf &
        'FeedsTryAddNew("https://support.microsoft.com/app/content/api/content/feeds/sap/en-sg/73b77d93-c44a-7bf3-295c-b729cf00eb82/rss" & vbCrLf &
        'FeedsTryAddNew("https://support.microsoft.com/app/content/api/content/feeds/sap/en-sg/7ebec7c8-1b8a-c547-bea4-cd285c103fd3/rss" & vbCrLf &
        'FeedsTryAddNew("https://www.tomshardware.com/feeds/all" & vbCrLf &
        'FeedsTryAddNew("http://feeds.feedburner.com/WinaeroBlog/" & vbCrLf &
        'FeedsTryAddNew("https://lovekrakow.pl/rss/aktualnosci.html" & vbCrLf &
        'FeedsTryAddNew("https://www.instagram.com/linkaa_m/" & vbCrLf &
        'FeedsTryAddNew("https://www.instagram.com/juliawieniawa/"

        Return True
    End Function

    Public Shared Function TryInitMyDefaultsBlackList(sFeedName As String) As String
        If sFeedName <> "PKAR_init" Then Return ""
        Return "t:(?i)audiobook
t:^VA -
d:(?i)gatunek:.*(thriller|karate|sportowy|prawniczy|horror)
d:(?i)gatunek:.*(sztuki walki|reality show|teledyski)
d:(?i)gatunek:.*(gothic|techno|punk|progressive|metal|industrial|indie)
d:(?i)gatunek:.*(hard|składanki|trap|hip-hop)
d:(?i)gatunek:.*(drum n Bass|electro house|uplifting trance|vocal trance)
d:(?i)genre:.*(gothic|techno|punk|progressive|metal|industrial|indie)
t:OS Build 17763
t:OS Build 16299
t:mp3@96
t:mp3@64
t:mp3@56
t:mp3@48
t:mp3@80
t:Echo serca \(2019\)
t:Elif \(2014\)
t:\*Prodo\*
t:Rodzice - Breeders \[S0
t:M jak Miłość \(20
t:składanka fredzi i mireczka
t:Ślub od pierwszego wejrzenia
t:Parszywa zgraja - Star Wars
t:Barwy szczęścia 2020
t:Ekipa z Warszawy - Warsaw Shore
t:Pierwsza Miłość \(201
t:Złomowisko PL \(20
t:Ojciec Mateusz \(20
t:Na Dobre i Na Złe \(20
t:a Wspólnej \(202"
    End Function

End Class
