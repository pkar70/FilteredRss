
Imports Windows.Data.Xml.Dom
Imports Windows.ApplicationModel.Background
Imports Windows.Storage
Imports Windows.UI.Notifications
Imports Windows.UI.Popups
Imports Windows.Web.Syndication
Imports System.Linq.Expressions
Imports Newtonsoft.Json.Serialization
''' <summary>
''' Provides application-specific behavior to supplement the default Application class.
''' </summary>

Public Class JedenItem
    Public Property sTitle As String
    Public Property sPicLink As String
    Public Property sFeedName As String
    Public Property sLinkToDescr As String
    Public Property sItemHtmlData As String
    Public Property sGuid As String     ' do kasowania etc.
    Public Property sDate As String
End Class


NotInheritable Class App
    Inherits Application

    ' #Const FILENAME = "oAllItems.xml"
    Const FILENAME As String = "glItems.xml"

    'Public sFeed As String
#Region "Wizardowe Frame"

    Shared gRootFrame As Frame

    ' https://stefanwick.com/2017/06/24/uwp-app-with-systray-extension/
    ' opis jak zrobic minimalizacje do SysTray
    ' wymaga: WinForms

    Protected Async Function OnLaunchFragment(aes As ApplicationExecutionState) As Task(Of Frame)
        Dim mRootFrame = TryCast(Window.Current.Content, Frame)

        ' Do not repeat app initialization when the Window already has content,
        ' just ensure that the window is active

        If mRootFrame Is Nothing Then
            ' Create a Frame to act as the navigation context and navigate to the first page
            mRootFrame = New Frame()

            AddHandler mRootFrame.NavigationFailed, AddressOf OnNavigationFailed

            ' PKAR added wedle https://stackoverflow.com/questions/39262926/uwp-hardware-back-press-work-correctly-in-mobile-but-error-with-pc
            AddHandler mRootFrame.Navigated, AddressOf OnNavigatedAddBackButton
            AddHandler Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested, AddressOf OnBackButtonPressed

            'If aes = ApplicationExecutionState.Terminated Then
            Await LoadIndex()
            Await KillFileLoad()
            ' TODO: Load state from previously suspended application
            'End If
            ' Place the frame in the current Window
            Window.Current.Content = mRootFrame
        End If
        gRootFrame = mRootFrame
        Return mRootFrame
    End Function

    ''' <summary>
    ''' Invoked when the application is launched normally by the end user.  Other entry points
    ''' will be used when the application is launched to open a specific file, to display
    ''' search results, and so forth.
    ''' </summary>
    ''' <param name="e">Details about the launch request and process.</param>
    Protected Overrides Async Sub OnLaunched(e As LaunchActivatedEventArgs)
        ' Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        Dim mRootFrame As Frame = Await OnLaunchFragment(e.PreviousExecutionState)

        If e.PrelaunchActivated = False Then
            If mRootFrame.Content Is Nothing Then
                ' When the navigation stack isn't restored navigate to the first page,
                ' configuring the new page by passing required information as a navigation
                ' parameter
                mRootFrame.Navigate(GetType(MainPage), e.Arguments)
            End If

            ' Ensure the current window is active
            Window.Current.Activate()
        End If
    End Sub

    ''' <summary>
    ''' Invoked when Navigation to a certain page fails
    ''' </summary>
    ''' <param name="sender">The Frame which failed navigation</param>
    ''' <param name="e">Details about the navigation failure</param>
    Private Sub OnNavigationFailed(sender As Object, e As NavigationFailedEventArgs)
        Throw New Exception("Failed to load Page " + e.SourcePageType.FullName)
    End Sub

    ''' <summary>
    ''' Invoked when application execution is being suspended.  Application state is saved
    ''' without knowing whether the application will be terminated or resumed with the contents
    ''' of memory still intact.
    ''' </summary>
    ''' <param name="sender">The source of the suspend request.</param>
    ''' <param name="e">Details about the suspend request.</param>
    Private Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()
        ' TODO: Save application state and stop any background activity
        deferral.Complete()
    End Sub


#End Region


    Private Async Function AppServiceLocalCommand(sCommand As String) As Task(Of String)
        Return ""
    End Function


    ' wedle https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/send-local-toast
    ' foreground activation
    Protected Overrides Async Sub OnActivated(e As IActivatedEventArgs)

        ' próba czy to commandline
        If e.Kind = ActivationKind.CommandLineLaunch Then

            Dim commandLine As CommandLineActivatedEventArgs = TryCast(e, CommandLineActivatedEventArgs)
            Dim operation As CommandLineActivationOperation = commandLine?.Operation
            Dim strArgs As String = operation?.Arguments

            If Not String.IsNullOrEmpty(strArgs) Then
                Await ObsluzCommandLine(strArgs)
                Window.Current.Close()
                Return
            End If
        End If

        Dim rootFrame As Frame = Await OnLaunchFragment(e.PreviousExecutionState)

        If e.Kind = ActivationKind.ToastNotification Then
            Dim oToastAct As ToastNotificationActivatedEventArgs
            oToastAct = TryCast(e, ToastNotificationActivatedEventArgs)
            If oToastAct IsNot Nothing Then
                Dim sArgs As String = oToastAct.Argument
                Select Case sArgs.Substring(0, 4)
                    Case "OPEN"
                        ' pokaz glowną stronę, oraz wybranie sGuid
                        'MakeDebugToast("OnActivated - OPEN")
                        If rootFrame.Content Is Nothing Then
                            'MakeDebugToast("OnActivated - OPEN NULL")
                            rootFrame.Navigate(GetType(MainPage))
                        Else
                            CrashMessageAdd("OnActivated - OPEN not null", "")
                        End If
                        'MakeDebugToast("OnActivated - OPEN po Navigate")
                        Dim oMPage As MainPage = TryCast(rootFrame.Content, MainPage)
                        Dim sGuid As String = sArgs.Substring(4)
                        'MakeDebugToast("OnActivated - sGuid=" & sGuid)
                        If oMPage Is Nothing Then
                            CrashMessageAdd("OnActivated - oMPage NULL", "")
                        Else
                            'MakeDebugToast("OnActivated - oMPage OK")
                            oMPage.ShowTorrentData(sGuid)
                        End If
                End Select
            End If
        End If

        Window.Current.Activate()
    End Sub



#Region "Triggers set/reset"

    'Public Shared Sub UnregisterTriggers()

    '    For Each oTask As KeyValuePair(Of Guid, IBackgroundTaskRegistration) In BackgroundTaskRegistration.AllTasks
    '        If oTask.Value.Name = "FilteredRSStimer" Then oTask.Value.Unregister(True)
    '        If oTask.Value.Name = "FilteredRSSservCompl" Then oTask.Value.Unregister(True)
    '        If oTask.Value.Name = "FilteredRSSToast" Then oTask.Value.Unregister(True)
    '    Next

    '    ' z innego wyszlo, ze RemoveAccess z wnetrza daje Exception
    '    ' If bAll Then BackgroundExecutionManager.RemoveAccess()

    'End Sub

    Public Shared Async Function RegisterTriggers() As Task

        ' na pewno musza byc usuniete
        UnregisterTriggers("")

        If Not Await CanRegisterTriggersAsync() Then Return

        RegisterTimerTrigger("FilteredRSStimer", GetSettingsInt("TimerInterval", 15))
        RegisterServicingCompletedTrigger("FilteredRSSservCompl")
        RegisterToastTrigger("FilteredRSSToast")

#If False Then
        Dim oBAS As BackgroundAccessStatus
        oBAS = Await BackgroundExecutionManager.RequestAccessAsync()


        ' https://docs.microsoft.com/en-us/windows/uwp/launch-resume/create-And-register-an-inproc-background-task
        Dim builder As BackgroundTaskBuilder = New BackgroundTaskBuilder
        Dim oRet As BackgroundTaskRegistration

        If oBAS = BackgroundAccessStatus.AlwaysAllowed Or oBAS = BackgroundAccessStatus.AllowedSubjectToSystemPolicy Then
            builder.SetTrigger(New TimeTrigger(GetSettingsInt("TimerInterval", 15), False))  ' nie moze byc mniej niz 15 minut!
            builder.Name = "FilteredRSStimer"
            oRet = builder.Register()

            Try
                builder.SetTrigger(New SystemTrigger(SystemTriggerType.ServicingComplete, True))
                builder.Name = "FilteredRSSservCompl"
                oRet = builder.Register()
            Catch ex As Exception
                ' to nie jest taki wazny trigger, wiec moze go nie byc
            End Try

            builder.SetTrigger(New ToastNotificationActionTrigger)
            builder.Name = "FilteredRSSToast"
            oRet = builder.Register()

            ' system trigger: ServicingComplete , po aktualizacji - deregister and reregister background
        End If
#End If
    End Function
#End Region

    ' RemoteSystems, Timer
    Protected Overrides Async Sub OnBackgroundActivated(args As BackgroundActivatedEventArgs)
        Dim oTimerDeferal As BackgroundTaskDeferral
        oTimerDeferal = args.TaskInstance.GetDeferral()

        Dim bNoComplete As Boolean = False

        ' lista komend danej aplikacji
        Dim sLocalCmds As String = ""
        ' zwroci false gdy to nie jest RemoteSystem; gdy true, to zainicjalizowało odbieranie
        bNoComplete = RemSysInit(args, sLocalCmds)

        If Not bNoComplete Then
            Await CalledFromBackground(args.TaskInstance)
        End If

        If Not bNoComplete Then moTaskDeferal.Complete()

    End Sub

    Private Sub Main_ShowTorrentData(sGuid As String)
        If gRootFrame Is Nothing Then Exit Sub
        Dim oMain As MainPage = TryCast(gRootFrame.Content, MainPage)
        If oMain IsNot Nothing Then
            Try
                oMain.ShowTorrentData(sGuid)  ' moze czasem sie nie uda?
            Catch ex As Exception
            End Try
        End If
    End Sub

    Private Shared Sub Main_ShowPostsList()
        If gRootFrame Is Nothing Then Exit Sub
        Dim oMain As MainPage = TryCast(gRootFrame.Content, MainPage)
        If oMain IsNot Nothing Then
            Try
                oMain.ShowPostsList()  ' moze czasem sie nie uda?
            Catch ex As Exception
            End Try
        End If
    End Sub

    Public Async Function CalledFromBackground(oTask As Windows.ApplicationModel.Background.IBackgroundTaskInstance) As Task


        Try
            Await LoadIndex()
            Await KillFileLoad()

            Select Case oTask.Task.Name     '  sTaskname
                Case "FilteredRSStimer"
                    gbInTimer = True
                    If GetSettingsBool("autoRead") Then Await ReadFeed(Nothing)
                    gbInTimer = False
                Case "FilteredRSSservCompl"
                    Await App.RegisterTriggers()
                Case "FilteredRSSToast"
                    Dim oDetails As ToastNotificationActionTriggerDetail = oTask.TriggerDetails
                    If oDetails IsNot Nothing Then
                        Dim sGuid As String = oDetails.Argument
                        Select Case sGuid.Substring(0, 4)
                            Case "DELE"
                                ' usunąć z listy
                                sGuid = DelOnePost(sGuid.Substring(4))

                                ' odwrócenie XmlSafe
                                'sGuid = sGuid.Replace("&amp;", "&")
                                'sGuid = sGuid.Replace("&lt;", "<")
                                'sGuid = sGuid.Replace("&gt;", ">")
                                If sGuid <> "" Then Main_ShowTorrentData(sGuid)

                            Case "BROW"
                                sGuid = sGuid.Substring(4)

                                For iInd As Integer = 0 To App.glItems.Count - 1
                                    If App.glItems.Item(iInd).sGuid = sGuid Then
                                        App.OpenHttpBrowser(App.glItems.Item(iInd).sLinkToDescr)
                                        Exit For
                                    End If
                                Next
                        End Select

                    End If
            End Select

            If Not gbInTimer Then Await SaveIndex(False)    ' jak jednoczesnie robi InTimer, to DelOnePost nie zapisuje (bo kolizja FileWrite moze sie pojawic - zmiany (usuniecie) zostaną zapisane przy zapisywaniu po  Timer)

        Catch ex As Exception
            CrashMessageAdd("CalledFromBackground", ex)
        End Try

    End Function

    ' Public Shared oAllItems As SyndicationFeed
    Public Shared glItems As List(Of JedenItem) = Nothing

    Public Shared Function DelOnePost(sGuid As String) As String
        ' zwraca NextGuid (czyli to, co ma zostac pokazane)

        If sGuid = "" Then Return ""

        Dim iMode As Integer = 0
        Dim oDelNode As SyndicationItem = Nothing
        Dim sNextGuid As String = ""

        Dim iInd As Integer = 0
        For iInd = 0 To App.glItems.Count - 1
            If App.glItems.Item(iInd).sGuid = sGuid Then Exit For
        Next
        If iInd > App.glItems.Count - 1 Then Return ""  ' nie znalazl

        App.glItems.RemoveAt(iInd)
        SetSettingsBool("ChangedXML", True)

        If iInd > App.glItems.Count - 1 Then iInd = App.glItems.Count - 1

        If WinVer() > 14970 Then
            ' wczesniej Len(tag) = 16, to za mało, nie umie skasowac - ale i minvers app > 15063
            Dim sTmp As String = sGuid
            If sTmp.Length > 64 Then sTmp = sTmp.Substring(0, 64)
            ToastNotificationManager.History.Remove(sTmp)
        End If

        Main_ShowPostsList()

        If iInd < 0 Then Return ""
        Return App.glItems.Item(iInd).sGuid
    End Function

    Private Shared miLastRssGuid As Integer = 0
    Private Shared miLastRssGuidMs As Integer = 0

    Public Shared Async Function ReadFeed(oTB As TextBlock) As Task(Of String)
        Dim sTmp As String

        Try

            miLastRssGuid = GetSettingsInt("iLastRssGuid")
            miLastRssGuidMs = GetSettingsInt("iLastRssGuidMS")

            Dim aFeeds() As String
            aFeeds = GetSettingsString("KnownFeeds").Split(vbCrLf)

            For Each sFeed As String In aFeeds
                ' bez pustych linii
                If sFeed.Length > 10 AndAlso sFeed.Substring(0, 1) <> ";" Then Await AddFeedItemsSyndic(sFeed, oTB)
            Next

            sTmp = "Last read: " & Date.Now().ToString
            SetSettingsString("lastRead", sTmp)

            ' specjane dla DevilTorrents; musi byc po wczytaniu wszystkich
            SetSettingsInt("iLastRssGuid", miLastRssGuid)
            SetSettingsInt("iLastRssGuidMS", miLastRssGuidMs)
            ' AppSuspending() ' ewentualnie zapisz dane także tu (na wypadek crash programu)

            Return sTmp
        Catch ex As Exception
            CrashMessageAdd("ReadFeed catch", ex)
        End Try
        Return "ERROR"
    End Function


    Private Shared Function ActivateLinks(sInput As String) As String
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
#If False Then
    Private Shared Async Function GetTwitterAsFeedOld(sUrl As String, oTB As TextBlock) As Task(Of SyndicationFeed)
        ' a gdy to jest Twitter, to zrób symulację feed'a
        sUrl = sUrl.Replace("//twitter.com/", "//mobile.twitter.com/")  ' przejscie na wersje normalniejszą

        Dim sPage As String = Await HttpPageAsync(sUrl, "")
        If sPage = "" Then
            If oTB IsNot Nothing Then Await DialogBox("Bad response from" & vbCrLf & sUrl)
            Return Nothing ' nie ma danych, ale i jakis komunikat jest pokazany
        End If

        Dim iInd As Integer
        Dim oFeed As SyndicationFeed = New SyndicationFeed

        ' odczytanie nazwy feed - z <head><title> 
        iInd = sPage.IndexOf("<title>")
        sPage = sPage.Substring(iInd + 7)
        iInd = sPage.IndexOf("</title>")
        oFeed.Title = New SyndicationText(sPage.Substring(0, iInd))
        iInd = oFeed.Title.Text.IndexOf("(@")
        If iInd > 0 Then
            oFeed.Title.Text = oFeed.Title.Text.Substring(0, iInd - 1).Trim
        End If

        ' wyciecie samych wpisów
        iInd = sPage.IndexOf("class=""titlebar")
        If iInd < 10 Then
            If oTB IsNot Nothing Then Await DialogBox("Twitter changed page style?" & vbCrLf & sUrl)
            Return Nothing ' nie ma danych, ale i jakis komunikat jest pokazany
        End If
        iInd = sPage.IndexOf("<table", iInd)
        sPage = sPage.Substring(iInd)

        iInd = sPage.IndexOf("class=""w-button-more")
        If iInd < 10 Then
            If oTB IsNot Nothing Then Await DialogBox("Twitter changed page style?" & vbCrLf & sUrl)
            Return Nothing ' nie ma danych, ale i jakis komunikat jest pokazany
        End If
        iInd = sPage.LastIndexOf(">", iInd)
        sPage = sPage.Substring(0, iInd + 1)


        ' sPage to teraz seria entries <table> (po jednym na każdy wpis)
        Dim sLastGuidSettings As String = App.Url2VarName(sUrl)
        Dim sLastGuid As String = GetSettingsString(sLastGuidSettings)
        Dim bFirst As Boolean = True

        Dim sPicLink As String
        iInd = sPage.IndexOf("<table")
        While iInd > -1
            Dim oNew As SyndicationItem = New SyndicationItem


            '            oNode.PublishedDate
            ' teoretycznie z:
            ' <table ..><tbody><tr class="tweet-header ">..<td class="timestamp">
            '   <a name = "tweet_1249346190296186880" href="/JkmMikke/status/1249346190296186880?p=p">18h</a>
            'albo np.
            '   <a name="tweet_1248219606566789121" href="/JkmMikke/status/1248219606566789121?p=p">Apr 9</a>
            ' ale nie wiem jak odkodować, więc na razie...
            oNew.PublishedDate = DateTime.Now

            ' potrzebujemy obrazek :)
            ' wpisu? nie każdy ma, może avatar?
            ' <table ..><tbody><tr class="tweet-header "><td class="avatar"><a href="/JkmMikke?p=i"><img alt="Janusz.Korwin.Mikke" src="https://pbs.twimg.com/profile_images/593430376443445248/m99ZwQgD_normal.jpg">
            iInd = sPage.IndexOf("<img ")
            sPicLink = sPage.Substring(iInd)
            iInd = sPicLink.IndexOf(">")
            sPicLink = sPicLink.Substring(0, iInd + 1)

            iInd = sPage.IndexOf("tweet-text")

            'oNode.Id (powinien być link Do pelnych danych, ale niech bedzie tylko ID
            iInd = sPage.IndexOf("data-id", iInd)
            sPage = sPage.Substring(iInd + 9)
            iInd = sPage.IndexOf("""")
            oNew.Id = sPage.Substring(0, iInd)
            If oNew.Id = sLastGuid Then Exit While
            If bFirst Then
                SetSettingsString(sLastGuidSettings, oNew.Id)
                bFirst = False
            End If

            ' oNode.Summary.Text - właściwy tweet
            iInd = sPage.IndexOf("<div", iInd)
            iInd = sPage.IndexOf(">", iInd)
            sPage = sPage.Substring(iInd + 1)
            iInd = sPage.IndexOf("</div")
            Dim sTmp As String = sPage.Substring(0, iInd - 1)
            oNew.Summary = New SyndicationText(sPicLink & vbCrLf & sTmp, SyndicationTextType.Html)

            ' oNode.Title.Text
            sTmp = RemoveHtmlTags(sTmp).Replace(vbCr, "").Replace(vbLf, "").Trim
            If sTmp.Length > 30 Then sTmp = sTmp.Substring(0, 30)
            oNew.Title = New SyndicationText(sTmp)

            '  i odatkowo sprawdzanie ignore, oraz "already seen"

            oFeed.Items.Add(oNew)

            iInd = sPage.IndexOf("<table")
        End While

        Return oFeed

        ' ewentualnie wprost do App.glItems

    End Function
#End If
    Private Shared Async Function GetInstagramFeed(sUrl As String, oTB As TextBlock) As Task

        Try

            Dim sInstaName As String = sUrl.Replace("//instagram.com/", "")
            sInstaName = sInstaName.Replace("https:", "")
            sInstaName = sInstaName.Replace("http:", "")
            sInstaName = "Insta: " & sInstaName


            'Dim bMsg As Boolean = If(oTB Is Nothing, False, True)
            Dim sPage As String = Await HttpPageAsync(sUrl, sInstaName, False)
            If sPage = "" Then
                If oTB IsNot Nothing Then Await DialogBoxAsync("Bad response from" & vbCrLf & sUrl)
                Return
            End If

            Dim iInd As Integer = sPage.IndexOf(">window._sharedData")
            iInd = sPage.IndexOf("{", iInd)
            sPage = sPage.Substring(iInd)
            iInd = sPage.IndexOf(";</script>")
            sPage = sPage.Substring(0, iInd)

            Dim oJSON As JSONinstagram = Newtonsoft.Json.JsonConvert.DeserializeObject(sPage, GetType(JSONinstagram))

            Dim oJUser As JSONinstaUser = oJSON.entry_data.ProfilePage.ElementAt(0).graphql.user

            Dim sLastGuidSettings As String = App.Url2VarName(sUrl)
            Dim sLastGuid As String = GetSettingsString(sLastGuidSettings)
            Dim bFirst As Boolean = True

            Dim sFeedTitle As String
            sFeedTitle = GetSettingsString("FeedName_" & App.Url2VarName(sUrl)).Trim

            For Each oEdge As JSONinstaPicEdge In oJUser.edge_owner_to_timeline_media.edges
                Dim oItem As JSONinstaPicNode = oEdge.node
                If oItem.id = sLastGuid Then Exit For
                If bFirst Then
                    SetSettingsString(sLastGuidSettings, oItem.id)
                    bFirst = False
                    sFeedTitle = "insta: " & oJUser.full_name.Trim
                End If

                Dim oNew As JedenItem = New JedenItem
                oNew.sFeedName = sFeedTitle
                oNew.sGuid = "insta:" & oItem.id
                oNew.sTitle = oItem.owner.username
                oNew.sPicLink = oItem.display_url
                oNew.sItemHtmlData = "<img src='" & oItem.display_url & "'>"
                If oItem.location IsNot Nothing Then
                    oNew.sItemHtmlData = oNew.sItemHtmlData & "<p>" & oItem.location.name
                End If
                oNew.sItemHtmlData = oNew.sItemHtmlData & "<p>" & oItem.accessibility_caption

                oNew.sDate = DateTimeOffset.FromUnixTimeSeconds(oItem.taken_at_timestamp).ToString("f")
                oNew.sLinkToDescr = oNew.sPicLink   ' na razie to samo... można będzie zapisać obrazek :)

                If Not NodeToIgnoreMain(sFeedTitle, oNew.sGuid, oNew.sTitle,
                                    DateTimeOffset.FromUnixTimeSeconds(oItem.taken_at_timestamp),
                                    oNew.sItemHtmlData) Then
                    App.glItems.Add(oNew)
                Else
                    ' App.glItems.Add(oNew)
                End If
            Next

            ' teoretycznie mamy tu feed obrazkow
            ' ale obrazki mają problem - jakby timestamp był w środku, albo inszy 
            ' https://scontent-waw1-1.cdninstagram.com/v/t51.2885-15/sh0.08/e35/c0.180.1440.1440a/s640x640/73251710_350808852403727_6326428339021572324_n.jpg?_nc_ht=scontent-waw1-1.cdninstagram.com\u0026_nc_cat=111\u0026_nc_ohc=GUEQvgt-bOUAX9Nij2A\u0026oh=0cbf86dd7c1a8a2c05de385d66c21c81\u0026oe=5EC1BA1D
            ' Bad URL timestamp

            ' https://scontent-waw1-1.cdninstagram.com/v/t51.2885-15/sh0.08/e35/c0.180.1440.1440a/s640x640/73251710_350808852403727_6326428339021572324_n.jpg?_nc_ht=scontent-waw1-1.cdninstagram.com\u0026_nc_cat=111&_nc_ohc=GUEQvgt-bOUAX9Nij2A&oh=0cbf86dd7c1a8a2c05de385d66c21c81&oe=5EC1BA1D
            ' URL signature mismatch
        Catch ex As Exception
            CrashMessageAdd("GetInstagramFeed catch: " & sUrl, ex)
        End Try

    End Function

    Private Shared Async Function GetTwarzetnikFeed(sUrl As String, oTB As TextBlock) As Task

        Dim oResp As Windows.Web.Http.HttpResponseMessage = Nothing
        Dim sError = ""
        Dim iInd As Integer

        Try

            ' testowanie: see TestLoveKrakow:MainPage

            If sUrl.ToLower.Contains("/groups/") Then
                If oTB IsNot Nothing Then Await DialogBoxAsync("Facebook groups are not implemented yet")
                Return
            End If

            iInd = sUrl.LastIndexOf("-")
            If iInd < 5 Then
                If oTB IsNot Nothing Then Await DialogBoxAsync("There should be '-' in Facebook url")
                Return
            End If
            Dim sFBpageId As String = sUrl.Substring(iInd + 1)

            Dim oHttp As Windows.Web.Http.HttpClient = New Windows.Web.Http.HttpClient

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
            oHttp.DefaultRequestHeaders.Accept.Add(New Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("*/*", 0.8))

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
                If oTB IsNot Nothing Then Await DialogBoxAsync("ERROR " & oResp.StatusCode)
                Return
            End If

        Catch ex As Exception
            CrashMessageAdd("GetTwarzetnikFeed catch 1:" & sUrl, ex)
            Return
        End Try

        If oResp Is Nothing Then
            CrashMessageAdd("GetTwarzetnikFeed catch 1a:" & sUrl, "oResp is null")
            Return
        End If

        Dim sResp As String = ""

        Try

            Try
                sResp = Await oResp.Content.ReadAsStringAsync
            Catch ex As Exception
                sError = ex.Message
            End Try

            If sError <> "" Then
                If oTB IsNot Nothing Then Await DialogBoxAsync("error " & sError & " at ReadAsStringAsync " & sUrl & " page")
                Return
            End If

            Dim sPage As String
            iInd = sResp.IndexOf("PagesProfileHomePrimaryColumnPagelet")
            If iInd < 10 Then
                If oTB IsNot Nothing Then Await DialogBoxAsync("error interpreting page for " & sUrl)
                Return
            End If

            sPage = sResp.Substring(iInd)
            Dim iInd1 As Integer


            Dim sLastGuidSettings As String = App.Url2VarName(sUrl)
            Dim sLastGuid As String = GetSettingsString(sLastGuidSettings)
            Dim bFirst As Boolean = True


            iInd = sPage.IndexOf("story-subtitle")
            While iInd > 0
                'Debug.WriteLine(vbCrLf & vbCrLf & "nowy post: " & vbCrLf)
                Dim oNew As JedenItem = New JedenItem

                iInd = sPage.LastIndexOf("<img", iInd)
                iInd = sPage.IndexOf("src=", iInd) + 5  ' src="
                iInd1 = sPage.IndexOf("""", iInd)
                oNew.sPicLink = sPage.Substring(iInd, iInd1 - iInd).Replace("&amp;", "&")
                'Debug.WriteLine(" piclink = " & sPage.Substring(iInd, iInd1 - iInd + 2))    ' PLUS DWA kontroli konca

                iInd = sPage.IndexOf("a href", iInd)
                iInd = sPage.IndexOf(">", iInd) + 1
                iInd1 = sPage.IndexOf("<", iInd)
                'Debug.WriteLine(" feed = " & sPage.Substring(iInd, iInd1 - iInd + 2))    ' PLUS DWA kontroli konca
                oNew.sFeedName = sPage.Substring(iInd, iInd1 - iInd)

                iInd = sPage.IndexOf("/permalink", iInd)
                iInd1 = sPage.IndexOf("""", iInd + 1)
                'Debug.WriteLine(" linktodescr = " & sPage.Substring(iInd, iInd1 - iInd + 2))    ' PLUS DWA kontroli konca
                ' ewentualnie, przy dluzszych historiach...
                oNew.sLinkToDescr = sPage.Substring(iInd, iInd1 - iInd)

                iInd = sPage.IndexOf("data-utime", iInd) + "data-utime=""".Length
                iInd1 = sPage.IndexOf("""", iInd)
                Dim sTmp As String = sPage.Substring(iInd, iInd1 - iInd)
                'Debug.WriteLine(" guid = " & sTmp)
                oNew.sGuid = sTmp
                If sTmp = sLastGuid Then Exit While
                If bFirst Then
                    SetSettingsString(sLastGuidSettings, sTmp)
                    bFirst = False
                End If

                ' bedzie potrzebne później
                Dim oPostDate As DateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(sTmp)

                oNew.sDate = oPostDate.ToString("f")
                'Debug.WriteLine(" data = " & DateTimeOffset.FromUnixTimeSeconds(sTmp).ToString("f"))

                iInd = sPage.IndexOf("post_message", iInd)
                iInd = sPage.IndexOf("<p>", iInd)
                iInd1 = sPage.IndexOf("<", iInd + 3)
                sTmp = sPage.Substring(iInd + 3, iInd1 - iInd - 3)
                oNew.sTitle = sTmp
                'Debug.WriteLine(" title = " & sTmp)

                iInd1 = sPage.IndexOf("</div", iInd)
                sTmp = sPage.Substring(iInd, iInd1 - iInd)
                sTmp = sTmp.Replace("<span class=""text_exposed_hide"">...</span>", "") ' bo calosc pokazujemy

                ' 2020.04.22
                sTmp = sTmp.Replace("href=""/", "href=""https://www.facebook.com/") ' link dla duzo dluzszych (otwieranych w oddzielnej stronie)
                ' ewentualnie ściągać takie coś, i podmieniać sTmp na nowe?



                'Debug.WriteLine(" tekst = " & sTmp)
                oNew.sItemHtmlData = sTmp


                If Not NodeToIgnoreMain(oNew.sFeedName, oNew.sGuid, oNew.sTitle, oPostDate, oNew.sItemHtmlData) Then
                    App.glItems.Add(oNew)
                End If

                sPage = sPage.Substring(iInd1)

                iInd = sPage.IndexOf("story-subtitle")
            End While

        Catch ex As Exception
            CrashMessageAdd("GetTwarzetnikFeed catch:" & sUrl, ex)
        End Try

    End Function
    Private Shared Async Function GetTwitterFeed(sUrl As String, oTB As TextBlock) As Task


        If oTB IsNot Nothing Then Await DialogBoxAsync("Sorry, Twitter zmienil access")

        ' https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/follow-search-get-users/api-reference/get-users-show
        ' wymagana autoryzacja

        'Try

        '    sUrl = sUrl.Replace("//twitter.com/", "//mobile.twitter.com/")  ' przejscie na wersje normalniejszą

        '    Dim sTwitName As String = sUrl.Replace("//mobile.twitter.com/", "")
        '    sTwitName = sTwitName.Replace("https:", "")
        '    sTwitName = sTwitName.Replace("http:", "")
        '    sTwitName = "Twitter " & sTwitName

        '    Dim sPage As String = Await HttpPageAsync(sUrl, sTwitName, False)
        '    If sPage = "" Then
        '        If oTB IsNot Nothing Then Await DialogBoxAsync("Bad response from" & vbCrLf & sUrl)
        '        Return ' nie ma danych, ale i jakis komunikat jest pokazany
        '    End If

        '    Dim iInd, iInd1 As Integer

        '    ' ustalenie sFeedTitle (z uwzględnieniem rename z settings)
        '    Dim sFeedTitle As String
        '    sFeedTitle = GetSettingsString("FeedName_" & App.Url2VarName(sUrl)).Trim
        '    If sFeedTitle = "" Then
        '        ' odczytanie nazwy feed - z <head><title> 
        '        iInd = sPage.IndexOf("<title>")
        '        sPage = sPage.Substring(iInd + 7)
        '        iInd = sPage.IndexOf("</title>")
        '        sFeedTitle = sPage.Substring(0, iInd)
        '        iInd = sFeedTitle.IndexOf("(@")
        '        If iInd > 0 Then
        '            sFeedTitle = sFeedTitle.Substring(0, iInd - 1).Trim
        '        End If
        '    End If

        '    ' wyciecie samych wpisów - kontrola składni strony twitter
        '    iInd = sPage.IndexOf("class=""titlebar")
        '    If iInd < 10 Then
        '        If oTB IsNot Nothing Then Await DialogBoxAsync("Twitter changed page style?" & vbCrLf & sUrl)
        '        Return  ' nie ma danych, ale i jakis komunikat jest pokazany
        '    End If
        '    iInd = sPage.IndexOf("<table", iInd)
        '    sPage = sPage.Substring(iInd)

        '    iInd = sPage.IndexOf("class=""w-button-more")
        '    If iInd < 10 Then
        '        If oTB IsNot Nothing Then Await DialogBoxAsync("Twitter changed page style?" & vbCrLf & sUrl)
        '        Return  ' nie ma danych, ale i jakis komunikat jest pokazany
        '    End If
        '    iInd = sPage.LastIndexOf(">", iInd)
        '    sPage = sPage.Substring(0, iInd + 1)


        '    ' sPage to teraz seria entries <table> (po jednym na każdy wpis)
        '    Dim sLastGuidSettings As String = App.Url2VarName(sUrl)
        '    Dim sLastGuid As String = GetSettingsString(sLastGuidSettings)
        '    Dim bFirst As Boolean = True

        '    'Dim sTmp As String

        '    iInd = sPage.IndexOf("<table")
        '    While iInd > -1
        '        sPage = sPage.Substring(iInd + 5) ' tak zeby nastepny LOOP nie wskoczył na to samo :)

        '        Dim oNew As JedenItem = New JedenItem
        '        oNew.sFeedName = sFeedTitle

        '        'oNode.Id (powinien być link Do pelnych danych, ale niech bedzie na razie tylko ID
        '        iInd = sPage.IndexOf("tweet-text")
        '        iInd = sPage.IndexOf("data-id", iInd) + 9
        '        iInd1 = sPage.IndexOf("""", iInd)
        '        oNew.sGuid = sPage.Substring(iInd, iInd1 - iInd)
        '        If oNew.sGuid = sLastGuid Then Exit While
        '        If bFirst Then
        '            SetSettingsString(sLastGuidSettings, oNew.sGuid)
        '            bFirst = False
        '        End If


        '        ' teoretycznie datę można jakoś wyciągnąć stringową, ale "18h" jest przecież zmienne...
        '        ' <table ..><tbody><tr class="tweet-header ">..<td class="timestamp">
        '        '   <a name = "tweet_1249346190296186880" href="/JkmMikke/status/1249346190296186880?p=p">18h</a>
        '        'albo np.
        '        '   <a name="tweet_1248219606566789121" href="/JkmMikke/status/1248219606566789121?p=p">Apr 9</a>
        '        iInd = sPage.IndexOf("=""timestamp")
        '        If iInd > 0 Then
        '            iInd = sPage.IndexOf(">", iInd + 20) + 1    ' szukam drugiego, +1: bo sam ">"jest niepotrzebny
        '            iInd1 = sPage.IndexOf("<", iInd)
        '            oNew.sDate = sPage.Substring(iInd, iInd1 - iInd)
        '        End If



        '        ' potrzebujemy obrazek :)
        '        ' wpisu? nie każdy ma, może avatar?
        '        ' <table ..><tbody><tr class="tweet-header "><td class="avatar"><a href="/JkmMikke?p=i"><img alt="Janusz.Korwin.Mikke" src="https://pbs.twimg.com/profile_images/593430376443445248/m99ZwQgD_normal.jpg">
        '        iInd = sPage.IndexOf("<img ")
        '        iInd = sPage.IndexOf("http", iInd)
        '        iInd1 = sPage.IndexOf("""", iInd + 5)
        '        oNew.sPicLink = sPage.Substring(iInd, iInd1 - iInd)


        '        'Public Property sItemHtmlData As String ' w HTML
        '        iInd = sPage.IndexOf("tweet-text")
        '        iInd = sPage.IndexOf("<div", iInd)
        '        iInd = sPage.IndexOf(">", iInd) + 1
        '        iInd1 = sPage.IndexOf("</div", iInd)
        '        oNew.sItemHtmlData = sPage.Substring(iInd, iInd1 - iInd)


        '        'Public Property sTitle As String ' w liście, na początku HTML
        '        oNew.sTitle = RemoveHtmlTags(oNew.sItemHtmlData).Replace(vbCr, "").Replace(vbLf, "").Trim
        '        If oNew.sTitle.Length > 30 Then oNew.sTitle = oNew.sTitle.Substring(0, 30)

        '        ' pomijamy na razie
        '        'Public Property sLinkToDescr As String ' na dole do klikania

        '        '  i odatkowo sprawdzanie ignore, oraz "already seen"
        '        If Not NodeToIgnoreMain(sFeedTitle, oNew.sGuid, oNew.sTitle, DateTime.Now, oNew.sItemHtmlData) Then App.glItems.Add(oNew)

        '        iInd = sPage.IndexOf("<table")
        '    End While

        'Catch ex As Exception
        '    CrashMessageAdd("GetTwitterFeed catch: " & sUrl, ex)
        'End Try
    End Function
    Private Shared Function ShouldShowEmptyFeedWarning(sGuidsValueName As String) As Boolean
        ' nic nie ma... sprawdz czy to nie error! (czy nie za dlugo nie ma)
        Dim sCurrDate As String = Date.Now.ToString("yyMMdd")
        Dim sLastItemTimeStamp As String = GetSettingsString("TIME" & sGuidsValueName)
        If sLastItemTimeStamp = "" Then Return False

        Dim iCurrDate As Integer = Integer.Parse(sCurrDate)
        Dim iLastItem As Integer = Integer.Parse(sLastItemTimeStamp)
        If iLastItem + GetSettingsInt("MaxDays", 30) < iCurrDate Then Return True

        Return False
    End Function

    Private Shared Function ExtractPicLink(sContent As String) As String
        Dim sTmp As String = sContent
        Dim iInd As Integer

        iInd = sTmp.IndexOf("<media:thumbnail", StringComparison.Ordinal)
        If iInd > 0 Then
            sTmp = sTmp.Substring(iInd)
            iInd = sTmp.IndexOf("url=""http:", StringComparison.Ordinal)
            If iInd > 0 Then sTmp = sTmp.Substring(iInd + 5)
            iInd = sTmp.IndexOf("""", StringComparison.Ordinal)
            If iInd > 0 Then sTmp = sTmp.Substring(0, iInd)
            Return sTmp
        Else
            iInd = sTmp.IndexOf("<img src", StringComparison.Ordinal)
            If iInd > 0 Then
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

    Private Shared Async Function AddFeedItemsSyndic(sUrl As String, oTB As TextBlock) As Task
        ' przerobiona funkcja AddFeedItemsOwn, na wykorzystującą klienta Rss

        ' testowanie
        ' If Not sUrl.ToLower.Contains("facebook.com") Then Return

        Dim sGuidsValueName, sGuids As String
        Dim sTmp As String = ""

        Try

            If oTB IsNot Nothing Then
                sTmp = sUrl
                ' zabezpieczenie numer 2 (poza ustalaniem szerokości w Form_Resize)
                If sTmp.Length > 64 Then sTmp = sTmp.Substring(0, 64)
                oTB.Text = sTmp
            End If

            ' testowanie konkretnego feed
            'If Not sUrl.Contains("faceb") Then Return

            If sUrl.ToLower.Contains("twitter.com") Then
                Await GetTwitterFeed(sUrl, oTB)
                Return
            End If

            If sUrl.ToLower.Contains("facebook.com") Then
                Await GetTwarzetnikFeed(sUrl, oTB)
                Return
            End If

            If sUrl.ToLower.Contains("instagram.com") Then
                Await GetInstagramFeed(sUrl, oTB)
                Return
            End If

        Catch ex As Exception
            CrashMessageAdd("AddFeedItemsSyndic switch catch: " & sUrl, ex)
        End Try

        ' teraz już tylko RSS

        Try
            ' pobierz aktualna liste ostatnio widzianych w feed GUIDów

            sGuidsValueName = App.Url2VarName(sUrl)
            sGuids = GetSettingsString(sGuidsValueName)

            Dim oRssClnt As SyndicationClient = New SyndicationClient
            Dim oRssFeed As SyndicationFeed = Nothing
            'Dim bErr As Boolean = False

            Dim sErr As String = ""
            Dim iInd As Integer
            Try
                oRssClnt.SetRequestHeader("User-Agent", "FilteredRSS")      ' konieczne dla LoveKrakow
                oRssFeed = Await oRssClnt.RetrieveFeedAsync(New Uri(sUrl))
            Catch ex As Exception
                sErr = ex.Message
                'bErr = True
            End Try
            If sErr <> "" Then
                If oTB IsNot Nothing Then Await DialogBoxAsync("Bad response from" & vbCrLf & sUrl & vbCrLf & sErr)
                Return  ' nie ma danych, ale i jakis komunikat jest pokazany
            End If

            ' *TODO* konwersja - dodanie do kazdego Item wlasnych atrybutow
            ' feedURL, unread ?

            If oRssFeed.Items.Count < 1 Then
                ' nic nie ma... sprawdz czy to nie error! (czy nie za dlugo nie ma)
                If Not ShouldShowEmptyFeedWarning(sGuidsValueName) Then Return
                If oTB IsNot Nothing Then Await DialogBoxAsync("Seems like feed is dead?" & vbCrLf & sUrl)
                Return
            End If


            Dim iCurrId As Integer ' Holds signed 32-bit (4-byte) integers that range in value from -2,147,483,648 through 2,147,483,647
            Dim bChanged As Boolean = False
            Dim bSeen As Boolean
            bSeen = False

#Region "przepisanie feed do własnej listy"
            For Each oNode As SyndicationItem In oRssFeed.Items

                Dim oNew As JedenItem = New JedenItem

                'Public Property sFeedName As String
                oNew.sFeedName = GetSettingsString("FeedName_" & App.Url2VarName(sUrl)).Trim
                If oNew.sFeedName = "" Then oNew.sFeedName = oRssFeed.Title.Text.Trim


                'Public Property sDate As String  
                ' 2018.11.10, TomsHardware Atom ma Update nie ma Published
                If oNode.PublishedDate < New DateTime(1900, 1, 1) Then
                    If oNode.LastUpdatedTime > New DateTime(1900, 1, 1) Then oNode.PublishedDate = oNode.LastUpdatedTime
                End If
                oNew.sDate = oNode.PublishedDate.ToString


                'Public Property sTitle As String
                ' 20190807
                If oNode.Title Is Nothing Then
                    sTmp = "(no title)"
                Else
                    sTmp = oNode.Title.Text
                End If
                sTmp = sTmp.Replace("( Seedów: ", "(S:")
                sTmp = sTmp.Replace(" Peerów: ", "+P:")
                sTmp = sTmp.Replace(" )", ")")
                oNew.sTitle = sTmp.Trim


                'Public Property sLinkToDescr As String
                If oNode.Links IsNot Nothing AndAlso oNode.Links.Count > 0 Then
                    oNew.sLinkToDescr = oNode.Links.Item(0).Uri.AbsoluteUri
                Else
                    ' 2019.03.13: moze to doda linki dla tomshardware, ktory czasem ich nie ma?
                    oNew.sLinkToDescr = oNode.Id
                End If
                ' uwaga: dla TomsHardware jest później skrócenie linku


                'Public Property sPicLink As String
                sTmp = Net.WebUtility.HtmlDecode(oNode.GetXmlDocument(SyndicationFormat.Rss20).GetXml)
                oNew.sPicLink = ExtractPicLink(sTmp)

                'Public Property sItemHtmlData As String
                sTmp = ""
                If oNode.Summary IsNot Nothing Then sTmp = oNode.Summary.Text
                If sTmp = "" AndAlso oNode.Content IsNot Nothing Then sTmp = oNode.Content.Text

                If GetSettingsBool("LinksActive") Then sTmp = App.ActivateLinks(sTmp)
                oNew.sItemHtmlData = sTmp


                ' sGuid: <guid>http://devil-torrents.pl/torrent/145394</guid>
                '   lub <link>
                ' 

                'Public Property sGuid As String     ' do kasowania etc. - wywołania z tła
                Dim sGuid As String
                sGuid = oNode.Id

                If sUrl.IndexOf("devil-torrents") > 0 Then
                    sTmp = oNode.Id
                    iInd = sTmp.LastIndexOf("/")
                    iCurrId = CInt(sTmp.Substring(iInd + 1))
                    miLastRssGuid = Math.Max(miLastRssGuid, iCurrId)
                    'If iCurrId < iLastRssGuid - 200 Then bSkip = True
                    If iCurrId < miLastRssGuid - 200 Then
                        bSeen = True
                        Exit For
                    End If
                    sGuid = "DEVIL-" & iCurrId
                End If

                ' 2018.11.10, dla TomsHardware ktory ma długie ID
                ' https://www.tomshardware.com/news/sapphire-radeon-rx-590-nitro-special-edition-specs,38051.html#xtor=RSS-5
                ' dziala też https://www.tomshardware.com/news/moje,38051.html
                ' skrócenie powoduje oszczędność SettingString, i mieści się więcej
                If sUrl.IndexOf("tomshardware") > 0 Then
                    sGuid = sGuid.Replace("#xtor=RSS-5", "")    ' skracamy końcówkę
                    iInd = sGuid.IndexOf("/", "https://www.tomshardware.com/n".Length) ' to nie musi być news!
                    sTmp = sGuid.Substring(0, iInd + 1)   ' https://www.tomshardware.com/(news|reviews|...)
                    iInd = sGuid.LastIndexOf(",")
                    If iInd > 0 Then
                        sGuid = sGuid.Substring(iInd)
                        oNew.sLinkToDescr = sTmp & "moje" & sGuid
                    End If
                End If

                ' 2019.03.13, z Microsoftu malutki GUID robimy
                ' formalnie: https://support.microsoft.com/help/4343889
                ' teraz:     http://MS/4343889
                ' teraz:     4343889 [7]
                ' zysk:                       1234567890123456789012345 [25]
                ' jest:      12345678901234567 [17]
                ' zmiana: 7 zamiast 42, czyli mieszcze znacznie wiecej
                If sUrl.IndexOf("support.microsoft.com") > 0 Then
                    Try
                        If oNode.Links.Count > 0 Then
                            sGuid = oNode.Links.Item(0).NodeValue
                            oNew.sLinkToDescr = sGuid
                            Dim iTmp As Integer = sGuid.LastIndexOf("/")
                            sGuid = sGuid.Substring(iTmp + 1)
                            iCurrId = CInt(sGuid)
                            sGuid = "MS-" & sGuid
                            miLastRssGuidMs = Math.Max(miLastRssGuidMs, iCurrId)
                        End If
                    Catch ex As Exception
                    End Try
                End If

                ' 2019.03.13, onlyoldmovies - nie mają pustego, ale idiotyczny :)
                If sGuid.IndexOf("tag:") > -1 Then
                    For Each oLink As SyndicationLink In oNode.Links
                        If oLink.Relationship = "alternate" Then
                            ' http://onlyoldmovies.blogspot.com/2019/03/born-yesterday-1950-classic-comedydrama.html
                            oNew.sLinkToDescr = oLink.Uri.AbsoluteUri
                            Exit For
                        End If
                    Next
                End If

                If sGuid = "" Then
                    ' blogi Microsoft są RSS, nie mają GUID - użyj link do tego
                    ' 20181104: ale to jest złe dla onlyOld, powinien byc inny link...
                    ' (<link rel='alternate' type='text/html') - natomiast w Microsoft to jest jedyny link

                    ' dla onlyOld, szukamy wsrod linkow konkretnego
                    sGuid = ""
                    For Each oLink As SyndicationLink In oNode.Links
                        If oLink.Relationship = "alternate" Then
                            sGuid = oLink.NodeValue
                            Exit For
                        End If
                    Next

                    ' jesli nie ma, to tak jak poprzednio - po prostu pierwszy
                    If sGuid = "" Then sGuid = oNode.Links.Item(0).NodeValue
                    oNode.Id = sGuid
                End If

                oNew.sGuid = sGuid

                ' warunek dla normalnego - wcześniej go nie było widać; dla twittera zawsze spełniony (tam nie ma "|")
                If sGuids.IndexOf(sGuid & "|") < 0 Then
                    'If Not bSkip And Not NodeToIgnore(oNode, sUrl) Then

                    ' bo mogly byc jakies podmiany; a Id i Title jest dla Toast
                    oNode.Id = oNew.sGuid
                    oNode.Title = New SyndicationText(oNew.sTitle)

                    If Not NodeToIgnoreSyndic(oNode, oNew.sFeedName) Then
                        ' sResult = sResult & vbCrLf & Net.WebUtility.HtmlDecode(oNode.GetXmlDocument(SyndicationFormat.Rss20).GetXml)
                        If App.glItems Is Nothing Then
                            CrashMessageAdd("AddFeedItemsSyndic - oAllItems NULL?", "")
                        Else
                            'MakeDebugToast("AddFeedItemsSyndic - nie ignorujemy")

                            App.glItems.Insert(0, oNew)
                            ' lista guidów nie dotyczy twittera - tego załatwiamy wcześniej
                            If Not sUrl.ToLower.Contains("twitter.com") Then sGuids = sGuids & sGuid & "|"
                            bChanged = True
                            'MakeDebugToast("AddFeedItemsSyndic - po dodaniu")
                        End If
                    End If
                End If

            Next
#End Region

            ' MakeDebugToast("AddFeedItemsSyndic - po petli")

            ' jesli nic nie ma dodania (wszystko jest ignore), to wracaj bez zmian
            If Not bChanged Then Exit Function

            SetSettingsBool("ChangedXML", True)     ' zaznacz ze jest zmiana
            SetSettingsString("TIME" & sGuidsValueName, Date.Now.ToString("yyMMdd"))

            ' dla normalnego RSS, dla twittera tego w ogole nie robi
            If Not sUrl.ToLower.Contains("twitter.com") Then
                ' zapisz do nastepnego uruchomienia (okolo 100 torrentow)
                If sGuids.Length > 3900 Then
                    ' limit 8K - ale bajtow
                    sGuids = sGuids.Substring(sGuids.Length - 3900)
                    iInd = sGuids.IndexOf("|")
                    sGuids = sGuids.Substring(iInd)
                End If
                SetSettingsString(sGuidsValueName, sGuids)
            End If

        Catch ex As Exception
            CrashMessageAdd("AddFeedItemsSyndic catch: " & sUrl, ex)
        End Try



    End Function


    Private Shared Function NodeToIgnoreMain(sFeedName As String, sItemId As String, sItemTitle As String, oItemDate As DateTimeOffset, sItemXML As String) As Boolean
        'Dim sTitle As String = oNode.Title.Text
        'Dim sDesc As String = Net.WebUtility.HtmlDecode(oNode.GetXmlDocument(SyndicationFormat.Rss20).GetXml)

        DebugOut(1, "NodeToIgnoreMain(" & sFeedName & ", title=" & sItemTitle)

        ' 2018.11.09 RSS z TomsHardware nie ma daty (wszystkie to 1/1/1601)
        If oItemDate > New DateTime(1900, 1, 1) Then
            If oItemDate.AddDays(GetSettingsInt("MaxDays", 30)) < Date.Now Then
                DebugOut(2, "ignoring because of being too old")
                Return True
            End If
        End If

        Dim bIgnore As Boolean = False
        Dim bWhite As Boolean = False

        Dim sPhrases As String()
        sPhrases = GetSettingsString("BlackList").Split(vbCrLf)
        For Each sWord As String In sPhrases
            If App.TestNodeMatch(sWord, sItemTitle, sItemXML, sFeedName) Then
                bIgnore = True
                DebugOut(2, "ignoring because of BlackList entry: " & sWord)
                Exit For
            End If
        Next

        ' killfile
        Dim aKills As String() = mKillFile.Split(vbCrLf)
        DebugOut(3, "starting KillFile, entries " & aKills.Length)
        For Each sKillLine As String In mKillFile.Split(vbCrLf)
            DebugOut(4, "testing entry " & sKillLine)
            Dim iInd As Integer = sKillLine.IndexOf(vbTab)
            If iInd > 2 Then
                Dim sFiltr As String = sKillLine.Substring(iInd + 1).Replace(vbCr, "").Replace(vbLf, "").Trim
                If RegExpOrInstr(sItemTitle, sFiltr) Then
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

        sPhrases = GetSettingsString("WhiteList").Split(vbCrLf)
        For Each sWord As String In sPhrases
            If sWord = "" Then Continue For
            If sWord = "*" Then
                bWhite = True
                'bIgnore = False
            ElseIf sWord.Substring(1, 2).ToLower = "f:" Then

            Else
                If App.TestNodeMatch(sWord, sItemTitle, sItemXML, sFeedName) Then
                    bIgnore = False
                    bWhite = True
                    Exit For
                End If
            End If
        Next


        If bIgnore Then Return True

        If bWhite And GetSettingsBool("NotifyWhite") Then
            App.MakeToast(sItemId, sItemTitle, sFeedName)
        End If
        Return False
    End Function

    Private Shared Function NodeToIgnoreSyndic(oNode As SyndicationItem, sRssFeed As String) As Boolean
        Dim sDesc As String = Net.WebUtility.HtmlDecode(oNode.GetXmlDocument(SyndicationFormat.Rss20).GetXml)

        Return NodeToIgnoreMain(sRssFeed, oNode.Id, oNode.Title.Text, oNode.PublishedDate, sDesc)

    End Function
    Public Shared Sub OpenHttpBrowser(sUri As String)
#Disable Warning BC42358 ' Because this call is not awaited, execution of the current method continues before the call is completed
        If sUri.Substring(0, 5) = "DEVIL" Then sUri = sUri.Replace("DEVIL-", "http://devil-torrents.pl/torrent/")
        If sUri.Substring(0, 3) = "MS-" Then sUri = sUri.Replace("MS-", "https://support.microsoft.com/help/")
        Windows.System.Launcher.LaunchUriAsync(New Uri(sUri))  ' ucięcie komendy BROW
#Enable Warning BC42358
    End Sub



#Region "Toasty"

    'Private Shared Function ToastAction(sAType As String, sAct As String, sGuid As String, sContent As String) As String
    '    Dim sTmp As String = sContent
    '    If sTmp <> "" Then sTmp = GetSettingsString(sTmp, sTmp)

    '    Dim sTxt As String = "<action " &
    '        "activationType=""" & sAType & """ " &
    '        "arguments=""" & sAct & XmlSafeStringQt(sGuid) & """ " &
    '        "content=""" & sTmp & """/> "
    '    Return sTxt
    'End Function

    'Public Shared Sub MakeDebugToast(sMsg As String)
    '    miDebugCnt = miDebugCnt + 1
    '    MakeToast("debug" & miDebugCnt.ToString, sMsg, "debug")
    'End Sub

    Public Shared Sub MakeToast(sGuid As String, sMsg As String, sFeed As String)
        ' sFeed robi na dwa sposoby:
        ' >15063, creators update, jako toast header
        ' <15063, jako annotation

        Dim sHdr As String = ""
        Dim sAttrib As String = ""

        sFeed = XmlSafeString(sFeed)
        If WinVer() > 15062 Then
            ' jako header
            ' https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/toast-headers
            sHdr = "<header id=""" & sFeed & """ title=""" & sFeed & """ arguments="""" />"
        ElseIf WinVer() > 14392 Then
            ' https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/adaptive-interactive-toasts
            sAttrib = "<text placement=""attribution"">" & sFeed & "</text>"
        End If

        Dim sXml As String = XmlSafeString(sMsg)
        sXml = sXml.Replace("( Seedów: ", "(S:")
        sXml = sXml.Replace(" Peerów: ", "+P:")
        sXml = sXml.Replace(" )", ")")
        sXml = "<text>" & sXml & "</text>"

        Dim sAction As String = "<actions>" & vbCrLf &
            ToastAction("system", "", "dismiss", "") & vbCrLf &
            ToastAction("background", "DELE", sGuid, "resDelete") & vbCrLf &
            ToastAction("foreground", "OPEN", sGuid, "resOpen") & vbCrLf &
            ToastAction("background", "BROW", sGuid, "resBrowser") & vbCrLf &
            "</actions>"
        Dim sGlobalAction As String = " launch=""OPEN" & XmlSafeStringQt(sGuid) & """ "

        Dim oXml As XmlDocument = New XmlDocument
        Dim bError As Boolean = False
        Try
            oXml.LoadXml("<toast" & sGlobalAction & ">" & sHdr &
                         "<visual><binding template='ToastGeneric'>" &
                         sAttrib & sXml & "</binding></visual>" &
                         sAction & "</toast>")
        Catch ex As Exception
            bError = True
        End Try

        If bError Then
            Try
                oXml.LoadXml("<toast><visual><binding template='ToastGeneric'><text>Error creating Toast</text></binding></visual></toast>")
            Catch ex As Exception
                Exit Sub
            End Try
        End If

        ' The size of the notification tag is too large.
        ' tag: 16 chrs, od Creators (14971 ??) jest 64 chrs
        If WinVer() > 14970 Then
            ' https://docs.microsoft.com/en-us/uwp/api/windows.ui.notifications.toastnotification.tag#Windows_UI_Notifications_ToastNotification_Tag
            If sGuid.Length > 64 Then sGuid = sGuid.Substring(0, 64)
        Else
            ' nie powinno sie zdarzyc, bo min vers aplikacji = 15063
            If sGuid.Length > 16 Then sGuid = sGuid.Substring(0, 16)
        End If

        Dim oToast As ToastNotification = New ToastNotification(oXml)
        bError = False
        Try
            oToast.Tag = sGuid   ' żeby można było usunąć toast gdy sie usunie w aplikacji
        Catch ex As Exception
            bError = True
        End Try

        ' http://zikit.krakow.pl/ogolne/222956,1787,komunikat,zmiany_w_rozkladach_jazdy_dla_wygody_pasazerow.html
        ' 123456789_123456789_123456789_123456789_123456789_123456789_123456789_123456789_123456789_123456789_123456789_


        ToastNotificationManager.CreateToastNotifier().Show(oToast)
    End Sub
#End Region

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

    Public Shared gbInTimer As Boolean = False

    Public Shared Async Function SaveIndex(bForce As Boolean) As Task

        Try
            If bForce OrElse GetSettingsBool("ChangedXML") Then

                Dim oFile As StorageFile = Await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(
                FILENAME, CreationCollisionOption.ReplaceExisting)

                If oFile Is Nothing Then
                    CrashMessageAdd("ERROR expected impossible: cannot create index?", "")
                    Exit Function
                End If

                Dim oSer As Xml.Serialization.XmlSerializer
                oSer = New Xml.Serialization.XmlSerializer(GetType(List(Of JedenItem)))
                Dim oStream As Stream = Await oFile.OpenStreamForWriteAsync
                oSer.Serialize(oStream, App.glItems)
                oStream.Dispose()   ' == fclose

                SetSettingsBool("ChangedXML", False)
            End If
        Catch ex As Exception
            CrashMessageAdd("SaveIndex CatchBlock", ex)
        End Try
    End Function

    Public Shared Async Function LoadIndex() As Task

        Try

            ' 20171101: omijamy wczytywanie gdy zmienna nie jest wyczyszczona
            If glItems IsNot Nothing Then Return

            App.glItems = New List(Of JedenItem)

            Dim oStorItem As IStorageItem = Await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(FILENAME)
            If oStorItem Is Nothing Then Return

            Dim oFile As StorageFile
            oFile = TryCast(oStorItem, StorageFile)
            If (Await oFile.GetBasicPropertiesAsync).Size < 5 Then Return

            Dim oSer As Xml.Serialization.XmlSerializer
            oSer = New Xml.Serialization.XmlSerializer(GetType(List(Of JedenItem)))
            Dim oStream As Stream = Await oFile.OpenStreamForReadAsync
            Try
                App.glItems = TryCast(oSer.Deserialize(oStream), List(Of JedenItem))
            Catch ex As Exception
                CrashMessageAdd("LoadIndex:Deserialize catch", ex)
            End Try

            SetSettingsBool("ChangedXML", False)
        Catch ex As Exception
            CrashMessageAdd("LoadIndex catch", ex)
        End Try
    End Function

#Region "Killfile"

    Private Shared mKillFile As String = ""
    Private Shared Async Function KillFileGetFile() As Task(Of Windows.Storage.StorageFile)
        Dim oFold As Windows.Storage.StorageFolder = Windows.Storage.ApplicationData.Current.LocalCacheFolder
        If oFold Is Nothing Then Return Nothing
        Return Await oFold.CreateFileAsync("killfile.txt", CreationCollisionOption.OpenIfExists)
    End Function

    Public Shared Async Function KillFileAdd(sMask As String) As Task
        Dim oFile As Windows.Storage.StorageFile = Await KillFileGetFile()
        If oFile Is Nothing Then Return

        Dim sLine As String = DateTime.Now.ToString("yyyyMMdd") & vbTab & sMask
        mKillFile = mKillFile & sLine
        Await oFile.AppendLineAsync(sLine)
    End Function

    Private Shared Async Function KillFileLoadMain() As Task(Of String)
        DebugOut(0, "KillFileLoadMain")
        Dim oFile As Windows.Storage.StorageFile = Await KillFileGetFile()
        If oFile Is Nothing Then
            DebugOut(1, "KillFileLoadMain - file is empty")
            Return ""
        End If

        Return Await oFile.ReadAllTextAsync
    End Function

    Private Shared Async Function KillFileSaveMain(sContent As String) As Task
        Dim oFile As Windows.Storage.StorageFile = Await KillFileGetFile()
        If oFile Is Nothing Then Return

        Await oFile.WriteAllTextAsync(sContent)
    End Function

    Public Shared Async Function KillFileLoad() As Task
        DebugOut(0, "KillFileLoad")
        Dim sWpisy As String = Await KillFileLoadMain()
        DebugOut(1, "read len: " & sWpisy.Length)
        If sWpisy = "" Then Return
        Dim sDataLimit As String = DateTime.Now.AddDays(-30).ToString("yyyyMMdd")
        DebugOut(1, "sDataLimit: " & sDataLimit)
        Dim bUsunieto As Boolean = False

        mKillFile = ""
        For Each sLine In sWpisy.Split(vbCrLf)
            sLine = sLine.Replace(vbCr, "").Replace(vbLf, "").Trim
            DebugOut(3, "kill line: " & sLine)
            If sLine.Length < 10 Then
                DebugOut(5, "kill line too short, ignoring")
                Continue For
            End If
            If sLine > sDataLimit Then
                DebugOut(5, "added to kill memory kill file")
                mKillFile = mKillFile & sLine & vbCrLf
            Else
                DebugOut(5, "too old - ignoring")
                bUsunieto = True
            End If
        Next

        If bUsunieto Then
            DebugOut(2, "some kill entries timeouted - saving new version of kill file")
#Disable Warning BC42358 ' Because this call is not awaited, execution of the current method continues before the call is completed
            ' bo moze sobie to robic w tle, niczemu to nie przeszkadza
            KillFileSaveMain(mKillFile)
#Enable Warning BC42358
        End If
    End Function
#End Region

End Class
