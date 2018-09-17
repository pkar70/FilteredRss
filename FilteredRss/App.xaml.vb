
Imports Windows.Data.Xml.Dom
Imports Windows.ApplicationModel.Background
Imports Windows.Storage
Imports Windows.UI.Notifications
Imports Windows.UI.Popups
Imports Windows.Web.Syndication
''' <summary>
''' Provides application-specific behavior to supplement the default Application class.
''' </summary>
NotInheritable Class App
    Inherits Application

    'Public sFeed As String

    Shared rootFrame As Frame

    ' https://stefanwick.com/2017/06/24/uwp-app-with-systray-extension/
    ' opis jak zrobic minimalizacje do SysTray
    ' wymaga: WinForms

    Protected Async Function OnLaunchFragment(aes As ApplicationExecutionState) As Task
        rootFrame = TryCast(Window.Current.Content, Frame)

        ' Do not repeat app initialization when the Window already has content,
        ' just ensure that the window is active

        If rootFrame Is Nothing Then
            ' Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = New Frame()

            AddHandler rootFrame.NavigationFailed, AddressOf OnNavigationFailed

            If aes = ApplicationExecutionState.Terminated Then
                Await LoadIndex()
                ' TODO: Load state from previously suspended application
            End If
            ' Place the frame in the current Window
            Window.Current.Content = rootFrame
        End If

    End Function

    ''' <summary>
    ''' Invoked when the application is launched normally by the end user.  Other entry points
    ''' will be used when the application is launched to open a specific file, to display
    ''' search results, and so forth.
    ''' </summary>
    ''' <param name="e">Details about the launch request and process.</param>
    Protected Overrides Async Sub OnLaunched(e As LaunchActivatedEventArgs)
        ' Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        Await OnLaunchFragment(e.PreviousExecutionState)

        If e.PrelaunchActivated = False Then
            If rootFrame.Content Is Nothing Then
                ' When the navigation stack isn't restored navigate to the first page,
                ' configuring the new page by passing required information as a navigation
                ' parameter
                rootFrame.Navigate(GetType(MainPage), e.Arguments)
            End If

            ' Ensure the current window is active
            Window.Current.Activate()
        End If
    End Sub

    ' wedle https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/send-local-toast
    ' foreground activation
    Protected Overrides Async Sub OnActivated(e As IActivatedEventArgs)
        Await OnLaunchFragment(e.PreviousExecutionState)

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
                        MakeDebugToast("OnActivated - OPEN not null")
                    End If
                    'MakeDebugToast("OnActivated - OPEN po Navigate")
                    Dim oMPage As MainPage = TryCast(rootFrame.Content, MainPage)
                    Dim sGuid As String = sArgs.Substring(4)
                    'MakeDebugToast("OnActivated - sGuid=" & sGuid)
                    If oMPage Is Nothing Then
                        MakeDebugToast("OnActivated - oMPage NULL")
                    Else
                        'MakeDebugToast("OnActivated - oMPage OK")
                        oMPage.ShowTorrentData(sGuid)
                    End If
            End Select
        End If

        Window.Current.Activate()
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


#Region "Triggers set/reset"

    Public Shared Sub UnregisterTriggers()

        For Each oTask As KeyValuePair(Of Guid, IBackgroundTaskRegistration) In BackgroundTaskRegistration.AllTasks
            If oTask.Value.Name = "FilteredRSStimer" Then oTask.Value.Unregister(True)
            If oTask.Value.Name = "FilteredRSSservCompl" Then oTask.Value.Unregister(True)
            If oTask.Value.Name = "FilteredRSSToast" Then oTask.Value.Unregister(True)
        Next

        ' z innego wyszlo, ze RemoveAccess z wnetrza daje Exception
        ' If bAll Then BackgroundExecutionManager.RemoveAccess()

    End Sub

    Public Shared Async Function RegisterTriggers() As Task

        ' na pewno musza byc usuniete
        UnregisterTriggers()

        Dim oBAS As BackgroundAccessStatus
        oBAS = Await BackgroundExecutionManager.RequestAccessAsync()


        ' https://docs.microsoft.com/en-us/windows/uwp/launch-resume/create-And-register-an-inproc-background-task
        Dim builder As BackgroundTaskBuilder = New BackgroundTaskBuilder
        Dim oRet As BackgroundTaskRegistration

        If oBAS = BackgroundAccessStatus.AlwaysAllowed Or oBAS = BackgroundAccessStatus.AllowedSubjectToSystemPolicy Then
            builder.SetTrigger(New TimeTrigger(App.GetSettingsInt("TimerInterval", 15), False))  ' nie moze byc mniej niz 15 minut!
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

    End Function
#End Region

    Protected Overrides Async Sub OnBackgroundActivated(args As BackgroundActivatedEventArgs)
        Dim oTimerDeferal As BackgroundTaskDeferral
        oTimerDeferal = args.TaskInstance.GetDeferral()

        Await CalledFromBackground(args.TaskInstance)

        'If rootFrame IsNot Nothing Then
        '    MakeToast("onbackground rootframe", "onbackground rootframe", "onbackground rootframe")
        '    Dim oMain As MainPage = TryCast(rootFrame.Content, MainPage)
        '    If oMain IsNot Nothing Then Await oMain.CalledFromBackground(args.TaskInstance)
        'Else
        '    MakeToast("onbackground NULL", "onbackground NULL", "onbackground NULL")
        'End If

        oTimerDeferal.Complete()
    End Sub

    Private Sub Main_ShowTorrentData(sGuid As String)
        If rootFrame Is Nothing Then Exit Sub
        Dim oMain As MainPage = TryCast(rootFrame.Content, MainPage)
        If oMain IsNot Nothing Then
            Try
                oMain.ShowTorrentData(sGuid)  ' moze czasem sie nie uda?
            Catch ex As Exception
            End Try
        End If
    End Sub

    Private Shared Sub Main_ShowPostsList()
        If rootFrame Is Nothing Then Exit Sub
        Dim oMain As MainPage = TryCast(rootFrame.Content, MainPage)
        If oMain IsNot Nothing Then
            Try
                oMain.ShowPostsList()  ' moze czasem sie nie uda?
            Catch ex As Exception
            End Try
        End If
    End Sub

    Public Async Function CalledFromBackground(oTask As Windows.ApplicationModel.Background.IBackgroundTaskInstance) As Task

        Await LoadIndex()

        Select Case oTask.Task.Name     '  sTaskname
            Case "FilteredRSStimer"
                If App.GetSettingsBool("autoRead") Then Await ReadFeed(Nothing)
            Case "FilteredRSSservCompl"
                Await App.RegisterTriggers()
            Case "FilteredRSSToast"
                Dim oDetails As ToastNotificationActionTriggerDetail = oTask.TriggerDetails
                If oDetails IsNot Nothing Then
                    Dim sGuid As String = oDetails.Argument
                    Select Case sGuid.Substring(0, 4)
                        Case "DELE"
                            ' usunąć z listy
                            DelOnePost(sGuid.Substring(4))

                            ' odwrócenie XmlSafe
                            sGuid = sGuid.Replace("&amp;", "&")
                            sGuid = sGuid.Replace("&lt;", "<")
                            sGuid = sGuid.Replace("&gt;", ">")
                            Main_ShowTorrentData(sGuid)

                        Case "BROW"
                            App.OpenHttpBrowser(sGuid.Substring(4))
                    End Select

                End If
        End Select

        Await SaveIndex(False)

    End Function

    Public Shared oAllItems As SyndicationFeed

    Public Shared Function DelOnePost(sGuid As String) As String

        If sGuid = "" Then Return ""

        Dim iMode As Integer = 0
        Dim oDelNode As SyndicationItem = Nothing
        Dim sNextGuid As String = ""

        For Each oNode As SyndicationItem In App.oAllItems.Items
            Select Case iMode
                Case 0
                    If oNode.Id = sGuid Then
                        oDelNode = oNode
                        iMode = 1
                    End If
                Case 1
                    sNextGuid = oNode.Id
                    iMode = 2
                    Exit For
            End Select

        Next

        If iMode = 0 Then Return "" ' nie znalazl?

        Try
            App.oAllItems.Items.Remove(oDelNode)
        Catch ex As Exception
        End Try

        App.SetSettingsBool("ChangedXML", True)

        If App.WinVer > 14970 Then
            ' wczesniej Len(tag) = 16, to za mało, nie umie skasowac - ale i minvers app > 15063
            Dim sTmp As String = sGuid
            If sTmp.Length > 64 Then sTmp = sTmp.Substring(0, 64)
            ToastNotificationManager.History.Remove(sTmp)
        End If

        Main_ShowPostsList()

        Return sNextGuid
    End Function

    Private Shared miLastRssGuid As Integer = 0

    Public Shared Async Function ReadFeed(oTB As TextBlock) As Task(Of String)
        Dim sTmp As String

        miLastRssGuid = App.GetSettingsInt("iLastRssGuid")

        Dim aFeeds() As String
        aFeeds = App.GetSettingsString("KnownFeeds").Split(vbCrLf)

        For Each sFeed As String In aFeeds
            ' bez pustych linii
            If sFeed.Length > 10 Then Await AddFeedItemsSyndic(sFeed, oTB)
        Next

        sTmp = "Last read: " & Date.Now().ToString
        App.SetSettingsString("lastRead", sTmp)

        ' specjane dla DevilTorrents; musi byc po wczytaniu wszystkich
        App.SetSettingsInt("iLastRssGuid", miLastRssGuid)
        ' AppSuspending() ' ewentualnie zapisz dane także tu (na wypadek crash programu)

        Return sTmp
    End Function


    Private Shared Async Function AddFeedItemsSyndic(sUrl As String, oTB As TextBlock) As Task
        ' przerobiona funkcja AddFeedItemsOwn, na wykorzystującą klienta Rss

        Dim sGuidsValueName, sGuids As String
        Dim sTmp As String = ""

        If oTB IsNot Nothing Then
            sTmp = sUrl
            ' zabezpieczenie numer 2 (poza ustalaniem szerokości w Form_Resize)
            If sTmp.Length > 64 Then sTmp = sTmp.Substring(0, 64)
            oTB.Text = sTmp
        End If

        ' pobierz aktualna liste ostatnio widzianych w feed GUIDów

        sGuidsValueName = App.Url2VarName(sUrl)
        sGuids = App.GetSettingsString(sGuidsValueName)


        Dim oRssClnt As SyndicationClient = New SyndicationClient
        Dim oRssFeed As SyndicationFeed = Nothing
        Dim bErr As Boolean = False
        Try
            oRssFeed = Await oRssClnt.RetrieveFeedAsync(New Uri(sUrl))
        Catch ex As Exception
            bErr = True
        End Try
        If bErr Then
            If Not App.GetSettingsBool("autoRead") AndAlso oTB IsNot Nothing Then App.DialogBox("Unrecognized or no response from" & vbCrLf & sUrl)
            Exit Function
        End If

        ' *TODO* konwersja - dodanie do kazdego Item wlasnych atrybutow
        ' feedURL, unread ?

        ' Dim sResult As String = ""

        If oRssFeed.Items.Count < 1 Then Exit Function

        Dim iInd As Integer
        Dim iCurrId As Integer
        Dim bChanged As Boolean = False
        Dim bSeen As Boolean
        bSeen = False

        For Each oNode As SyndicationItem In oRssFeed.Items

            ' wiekszosc to Rss2.0
            ' oldmovies to Atom1 - i dlatego przerabiam na wersje z Syndication

            ' sGuid: <guid>http://devil-torrents.pl/torrent/145394</guid>
            '   lub <link>
            ' 

            'bSkip = False
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
            End If

            Dim sGuid As String
            sGuid = oNode.Id
            If sGuid = "" Then
                ' blogi Microsoft są RSS, nie mają GUID - użyj link do tego
                sGuid = oNode.Links.Item(0).NodeValue
                oNode.Id = sGuid
            End If

            ' dla takich co nie mają guid jako http, np. OnlyOldMovies (atom)
            ' efekt: kliknięcie na itemie pokazuje "select app to open link", bo link jest robiony z .guid
            If sGuid.Substring(0, 4) <> "http" Then
                Dim bMam As Boolean = False
                For Each oLink As SyndicationLink In oNode.Links
                    If oLink.Relationship = "alternate" Then
                        sGuid = oLink.Uri.ToString
                        bMam = True
                        Exit For
                    End If

                    ' ostateczny fallback - tak, by webviewer uruchomiło OnNavStart
                    If Not bMam Then sGuid = "http://host.com/app?" & Net.WebUtility.HtmlEncode(sGuid)
                Next
            End If

            If sGuids.IndexOf(sGuid & "|") < 0 Then
                'If Not bSkip And Not NodeToIgnore(oNode, sUrl) Then
                Dim sFeedTitle As String
                sFeedTitle = App.GetSettingsString("FeedName_" & App.Url2VarName(sUrl))
                If sFeedTitle = "" Then sFeedTitle = oRssFeed.Title.Text

                If Not NodeToIgnoreSyndic(oNode, sFeedTitle) Then
                    ' sResult = sResult & vbCrLf & Net.WebUtility.HtmlDecode(oNode.GetXmlDocument(SyndicationFormat.Rss20).GetXml)
                    If App.oAllItems Is Nothing Then
                        MakeDebugToast("AddFeedItemsSyndic - oAllItems NULL?")
                    Else
                        'MakeDebugToast("AddFeedItemsSyndic - nie ignorujemy")
                        App.oAllItems.Items.Insert(0, oNode)
                        sGuids = sGuids & sGuid & "|"
                        bChanged = True
                        'MakeDebugToast("AddFeedItemsSyndic - po dodaniu")
                    End If
                End If
            End If

        Next

        ' MakeDebugToast("AddFeedItemsSyndic - po petli")

        ' jesli nic nie ma dodania (wszystko jest ignore), to wracaj bez zmian
        If Not bChanged Then Exit Function

        App.SetSettingsBool("ChangedXML", True)     ' zaznacz ze jest zmiana

        ' zapisz do nastepnego uruchomienia (okolo 100 torrentow)
        If sGuids.Length > 3900 Then
            ' limit 8K - ale bajtow
            sGuids = sGuids.Substring(sGuids.Length - 3900)
            iInd = sGuids.IndexOf("|")
            sGuids = sGuids.Substring(iInd)
        End If
        App.SetSettingsString(sGuidsValueName, sGuids)


        Exit Function
    End Function

    Private Shared Function NodeToIgnoreSyndic(oNode As SyndicationItem, sRssFeed As String) As Boolean
        Dim sTitle As String = oNode.Title.Text
        Dim sDesc As String = Net.WebUtility.HtmlDecode(oNode.GetXmlDocument(SyndicationFormat.Rss20).GetXml)

        If oNode.PublishedDate.AddDays(App.GetSettingsInt("MaxDays", 30)) < Date.Now Then Return True

        Dim bIgnore As Boolean = False
        Dim bWhite As Boolean = False

        Dim sPhrases As String()
        sPhrases = App.GetSettingsString("BlackList").Split(vbCrLf)
        For Each sWord As String In sPhrases
            If App.TestNodeMatch(sWord, sTitle, sDesc, sRssFeed) Then
                bIgnore = True
                Exit For
            End If
        Next

        sPhrases = App.GetSettingsString("WhiteList").Split(vbCrLf)
        For Each sWord As String In sPhrases
            If sWord = "*" Then
                bWhite = True
                'bIgnore = False
            ElseIf sWord.Substring(1, 2).ToLower = "f:" Then

            Else
                If App.TestNodeMatch(sWord, sTitle, sDesc, sRssFeed) Then
                    bIgnore = False
                    bWhite = True
                    Exit For
                End If
            End If
        Next


        NodeToIgnoreSyndic = bIgnore
        If bIgnore Then Exit Function

        If bWhite And App.GetSettingsBool("NotifyWhite") Then
            App.MakeToast(oNode.Id, oNode.Title.Text, sRssFeed)
        End If

    End Function
    Public Shared Sub OpenHttpBrowser(sUri As String)
#Disable Warning BC42358 ' Because this call is not awaited, execution of the current method continues before the call is completed
        Windows.System.Launcher.LaunchUriAsync(New Uri(sUri))  ' ucięcie komendy BROW
#Enable Warning BC42358
    End Sub

#Region "Get/Set Settings"

    Public Shared Function GetSettingsString(sName As String, Optional sDefault As String = "") As String
        Dim sTmp As String

        sTmp = sDefault

        If ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName) Then
            sTmp = ApplicationData.Current.RoamingSettings.Values(sName).ToString
        End If
        If ApplicationData.Current.LocalSettings.Values.ContainsKey(sName) Then
            sTmp = ApplicationData.Current.LocalSettings.Values(sName).ToString
        End If

        Return sTmp

    End Function

    Public Shared Function GetSettingsInt(sName As String, Optional iDefault As Integer = 0) As Integer
        Dim sTmp As Integer

        sTmp = iDefault

        If ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName) Then
            sTmp = CInt(ApplicationData.Current.RoamingSettings.Values(sName).ToString)
        End If
        If ApplicationData.Current.LocalSettings.Values.ContainsKey(sName) Then
            sTmp = CInt(ApplicationData.Current.LocalSettings.Values(sName).ToString)
        End If

        Return sTmp

    End Function

    Public Shared Function GetSettingsBool(sName As String, Optional iDefault As Boolean = False) As Boolean
        Dim sTmp As Boolean

        sTmp = iDefault

        If ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName) Then
            sTmp = CBool(ApplicationData.Current.RoamingSettings.Values(sName).ToString)
        End If
        If ApplicationData.Current.LocalSettings.Values.ContainsKey(sName) Then
            sTmp = CBool(ApplicationData.Current.LocalSettings.Values(sName).ToString)
        End If

        Return sTmp

    End Function

    Public Shared Sub SetSettingsString(sName As String, sValue As String, Optional bRoam As Boolean = False)
        If bRoam Then ApplicationData.Current.RoamingSettings.Values(sName) = sValue
        ApplicationData.Current.LocalSettings.Values(sName) = sValue
    End Sub

    Public Shared Sub SetSettingsInt(sName As String, sValue As Integer, Optional bRoam As Boolean = False)
        If bRoam Then ApplicationData.Current.RoamingSettings.Values(sName) = sValue.ToString
        ApplicationData.Current.LocalSettings.Values(sName) = sValue.ToString
    End Sub

    Public Shared Sub SetSettingsBool(sName As String, sValue As Boolean, Optional bRoam As Boolean = False)
        If bRoam Then ApplicationData.Current.RoamingSettings.Values(sName) = sValue.ToString
        ApplicationData.Current.LocalSettings.Values(sName) = sValue.ToString
    End Sub
#End Region

    Public Shared Sub SetBadgeNo(iInt As Integer)
        ' https://docs.microsoft.com/en-us/windows/uwp/controls-and-patterns/tiles-and-notifications-badges
        Dim oXmlBadge As XmlDocument = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber)
        Dim oXmlNum As XmlElement = CType(oXmlBadge.SelectSingleNode("/badge"), XmlElement)
        oXmlNum.SetAttribute("value", iInt.ToString)
        BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(New BadgeNotification(oXmlBadge))
    End Sub



#Region "Toasty"

    Public Shared Function XmlSafeString(sInput As String) As String
        Dim sTmp As String
        sTmp = sInput.Replace("&", "&amp;")
        sTmp = sTmp.Replace("<", "&lt;")
        sTmp = sTmp.Replace(">", "&gt;")
        Return sTmp
    End Function

    Public Shared Function XmlSafeStringQt(sInput As String) As String
        Dim sTmp As String
        sTmp = XmlSafeString(sInput)
        sTmp = sTmp.Replace("""", "&quote;")
        Return sTmp
    End Function

    Private Shared Function ToastAction(sAType As String, sAct As String, sGuid As String, sContent As String) As String
        Dim sTmp As String = sContent
        If sTmp <> "" Then sTmp = GetSettingsString(sTmp, sTmp)

        Dim sTxt As String = "<action " &
            "activationType=""" & sAType & """ " &
            "arguments=""" & sAct & XmlSafeStringQt(sGuid) & """ " &
            "content=""" & sTmp & """/> "
        Return sTxt
    End Function

    Public Shared Function WinVer() As Integer
        'Unknown = 0,
        'Threshold1 = 1507,   // 10240
        'Threshold2 = 1511,   // 10586
        'Anniversary = 1607,  // 14393 Redstone 1
        'Creators = 1703,     // 15063 Redstone 2
        'FallCreators = 1709 // 16299 Redstone 3
        'April  1803, 17134, RS5

        Dim u As ULong = ULong.Parse(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamilyVersion)
        u = (u And &HFFFF0000L) >> 16
        Return u
        'For i As Integer = 5 To 1 Step -1
        '    If Metadata.ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", i) Then Return i
        'Next

        'Return 0
    End Function

    Private Shared miDebugCnt As Integer = 1
    Public Shared Sub MakeDebugToast(sMsg As String)
        miDebugCnt = miDebugCnt + 1
        MakeToast("debug" & miDebugCnt.ToString, sMsg, "debug")
    End Sub

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
    'Public Shared Async Function MsgBox(sMsg As String) As Task(Of String)
    '    Dim oMD As MessageDialog = New MessageDialog(sMsg)
    '    Await oMD.ShowAsync()
    '    Return ""

    'End Function

    Public Shared Async Sub DialogBox(sMsg As String)
        Dim oMsg As MessageDialog = New MessageDialog(sMsg)
        Await oMsg.ShowAsync
    End Sub
    Public Shared Function GetLangString(sMsg As String) As String
        If sMsg = "" Then Return ""

        Dim sRet As String = sMsg
        Try
            sRet = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView().GetString(sMsg)
        Catch
        End Try
        Return sRet
    End Function

    Public Shared Async Sub DialogBoxRes(sMsg As String)
        Dim oMsg As MessageDialog = New MessageDialog(GetLangString(sMsg))
        Await oMsg.ShowAsync
    End Sub

    Public Shared Function Url2VarName(sUrl As String) As String
        Dim sGuidsValueName As String = sUrl.Replace("/", "")
        sGuidsValueName = sGuidsValueName.Replace("?", "")
        sGuidsValueName = sGuidsValueName.Replace(":", "")
        Return sGuidsValueName
    End Function

    Public Shared Async Function SaveIndex(bForce As Boolean) As Task
        If bForce OrElse App.GetSettingsBool("ChangedXML") Then
            Dim sampleFile As StorageFile = Await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(
            "oAllItems.xml", CreationCollisionOption.ReplaceExisting)
            Await App.oAllItems.GetXmlDocument(SyndicationFormat.Rss20).SaveToFileAsync(sampleFile)
            App.SetSettingsBool("ChangedXML", False)
        End If
    End Function

    Public Shared Async Function LoadIndex() As Task
        Dim sTxt As String

        ' 20171101: omijamy wczytywanie gdy zmienna nie jest wyczyszczona
        If App.oAllItems IsNot Nothing Then Exit Function

        App.oAllItems = New SyndicationFeed

        Dim oStorItem As IStorageItem = Await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync("oAllItems.xml")
        If oStorItem Is Nothing Then Exit Function

        Dim oFile As StorageFile
        oFile = TryCast(oStorItem, StorageFile)

        Dim oXml As XmlDocument = New XmlDocument

        Try
            sTxt = Await FileIO.ReadTextAsync(oFile)
            If sTxt = "" Then Exit Try
            oXml.LoadXml(sTxt)
            App.oAllItems.LoadFromXml(oXml)

            App.SetSettingsBool("ChangedXML", False)
        Catch ex As Exception
            ' zapewne ze filesa nie ma - ignorujemy
        End Try

    End Function

End Class
