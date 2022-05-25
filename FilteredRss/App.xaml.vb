
Imports Windows.ApplicationModel.Background
Imports Windows.Storage
Imports Windows.UI.Notifications
Imports System.Linq.Expressions
Imports VBlib.Extensions

' gdy True, to Release będzie miał błąd 
#Const RSS_IN_VBLIB = True

#If RSS_IN_VBLIB Then
Imports TypRSS = System.ServiceModel.Syndication
#Else
Imports TypRSS = Windows.Web.Syndication
#End If

NotInheritable Class App
    Inherits Application

    ' #Const FILENAME = "oAllItems.xml"
    ' Const FILENAME As String = "glItems.xml"

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
            LoadIndex()
            KillFileLoad(False)
            ' TODO: Load state from previously suspended application
            'End If
            ' Place the frame in the current Window
            Window.Current.Content = mRootFrame
        End If
        gRootFrame = mRootFrame ' dla wyjscia z Torrent
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

        SetSettingsString("DebugOutData", "")    'na próbę wyjścia z problemmu

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

#End Region


    Private Async Function AppServiceLocalCommand(sCommand As String) As Task(Of String)
        Return ""
    End Function




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

    End Function
#End Region

    ' RemoteSystems, Timer
    Protected Overrides Async Sub OnBackgroundActivated(args As BackgroundActivatedEventArgs)
        Dim moTaskDeferal As BackgroundTaskDeferral
        moTaskDeferal = args.TaskInstance.GetDeferral()

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

    Public Shared bChangedXML As Boolean = False

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

    'Private Shared Sub Main_ShowPostsList()
    '    If gRootFrame Is Nothing Then Exit Sub
    '    Dim oMain As MainPage = TryCast(gRootFrame.Content, MainPage)
    '    If oMain IsNot Nothing Then
    '        Try
    '            oMain.ShowPostsList()  ' moze czasem sie nie uda?
    '        Catch ex As Exception
    '        End Try
    '    End If
    'End Sub

    ' dla blokady wywołania z dwu różnych ścieżek z CalledFromBackground (np. Timer oraz Toast)
    Private Shared gbInTimer As Boolean = False

    Public Async Function CalledFromBackground(oTask As Windows.ApplicationModel.Background.IBackgroundTaskInstance) As Task


        Try
            LoadIndex()
            KillFileLoad(False)

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

                                For iInd As Integer = 0 To VBlib.App.glItems.Count - 1
                                    If VBlib.App.glItems.Item(iInd).sGuid = sGuid Then
                                        App.OpenHttpBrowser(VBlib.App.glItems.Item(iInd).sLinkToDescr)
                                        Exit For
                                    End If
                                Next
                        End Select

                    End If
            End Select

            If Not gbInTimer Then SaveIndex(False)    ' jak jednoczesnie robi InTimer, to DelOnePost nie zapisuje (bo kolizja FileWrite moze sie pojawic - zmiany (usuniecie) zostaną zapisane przy zapisywaniu po  Timer)

        Catch ex As Exception
            CrashMessageAdd("CalledFromBackground", ex)
        End Try

    End Function

    Public Shared Function DelOnePost(sGuid As String) As String
        ' zwraca NextGuid (czyli to, co ma zostac pokazane)

        If sGuid = "" Then Return ""
        bChangedXML = True

        If WinVer() > 14970 Then
            ' wczesniej Len(tag) = 16, to za mało, nie umie skasowac - ale i minvers app > 15063
            Dim sTmp As String = sGuid
            If sTmp.Length > 64 Then sTmp = sTmp.Substring(0, 64)
            ToastNotificationManager.History.Remove(sTmp)
        End If

        Dim iMode As Integer = 0
        Dim sNextGuid As String = VBlib.App.DelOnePost(sGuid)
        Return sNextGuid
    End Function

    Private Shared Sub ZrobToasty(oFeed As VBlib.JedenFeed, oNewList As List(Of VBlib.JedenItem))
        ' zrobienie Toastów
        ' przeniesione z GetRssFeed:NodeToIgnore

        If oFeed.iToastType = VBlib.FeedToastType.NoToast Then Return

        Dim iToastCnt As Integer = 0
        Dim sToastString As String = ""

        For Each oItem As VBlib.JedenItem In oNewList

            ' 2021.12.15
            ' Toast w kilku sytuacjach, wedle oFeed.iToastType , istnienia na WhiteList, oraz GetSettingsBool("NotifyWhite")

            iToastCnt += 1

            Select Case oFeed.iToastType
                Case VBlib.FeedToastType.NoToast
                    ' wtedy nic nie pokazujemy

                Case VBlib.FeedToastType.NewExist
                    ' pokaże dopiero później

                Case VBlib.FeedToastType.ListItems
                    sToastString = sToastString & vbCrLf & oItem.sTitle

                Case VBlib.FeedToastType.Separate
                    App.MakeToast(oItem.sGuid, oItem.sTitle, oFeed.sName)

            End Select

        Next

        ' i teraz podsumowanie toastów

        Select Case oFeed.iToastType
            Case VBlib.FeedToastType.ListItems
                If sToastString <> "" Then
                    Try
                        MakeToast("", GetSettingsString("resNewItemsList") & " (" & iToastCnt & ")" & vbCrLf & sToastString, oFeed.sName)
                    Catch ex As Exception
                        ' too big (np. lista 88 sztuk)
                        MakeToast("", GetSettingsString("resNewItemsList") & " (" & iToastCnt & ")", oFeed.sName)
                    End Try
                End If
            Case VBlib.FeedToastType.NewExist
                If iToastCnt > 0 Then
                    MakeToast("", GetSettingsString("resNewItemsInFeed") & " (" & iToastCnt & ")", oFeed.sName)
                End If
        End Select

    End Sub

    ''' <summary>
    ''' ret: string, ogólne podsumowanie, do pokazania w TextBox
    ''' </summary>
    Public Shared Async Function ReadFeed(oTB As TextBlock) As Task(Of String)
        ' Poprzednia sekwencja: ReadFeed -> AddFeedItemsSyndic -> GetRssFeed

        Dim sTmp As String

        Try

            If Not FeedsLoad() Then Return "NO FEEDS" ' nie ma nic

            For Each oFeed As VBlib.JedenFeed In VBlib.Feeds.glFeeds
                oFeed.bLastError = False

                ' przeniesione z AddFeedItemsSyndic
                If oTB IsNot Nothing Then
                    sTmp = oFeed.sUri
                    ' zabezpieczenie numer 2 (poza ustalaniem szerokości w Form_Resize)
                    If sTmp.Length > 64 Then sTmp = sTmp.Substring(0, 64)
                    oTB.Text = sTmp
                End If

#If RSS_IN_VBLIB Then
                Dim oNewList As List(Of VBlib.JedenItem) = Await VBlib.App.GetAnyFeed(oFeed)
#Else
                Dim oNewList As List(Of VBlib.JedenItem) = Await GetAnyFeed(oFeed)
#End If

                ' przeniesione z GetRssFeed
                If oNewList Is Nothing Then
                    ' był błąd
                    If oTB IsNot Nothing Then Await DialogBoxAsync("Bad response from " & oFeed.sName)
                    Continue For
                End If

                If oNewList IsNot Nothing Then feeds.FeedsSave(False) ' local, nie Roaming

                ' przeniesione z GetRssFeed
                If oNewList.Count < 1 Then
                    ' nic nie ma... sprawdz czy to nie error! (czy nie za dlugo nie ma)
                    If ShouldShowEmptyFeedWarning(oFeed.sLastOkDate) Then
                        If oTB IsNot Nothing Then Await DialogBoxAsync("Seems like feed " & oFeed.sName & " is dead?")
                    End If
                    Continue For
                End If

                App.bChangedXML = True

                VBlib.App.glItems = VBlib.App.glItems.Concat(oNewList).ToList

                ' zrobienie Toastów
                ' przeniesione z GetRssFeed:NodeToIgnore
                If oFeed.iToastType <> VBlib.FeedToastType.NoToast Then ZrobToasty(oFeed, oNewList)
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


#If Not RSS_IN_VBLIB Then
    Private Shared Async Function GetAnyFeed(oFeed As VBlib.JedenFeed) As Task(Of List(Of VBlib.JedenItem))

        Try
            If oFeed.sUri.ToLower.Contains("twitter.com") Then Return Nothing
            'If oFeed.sUri.ToLower.Contains("facebook.com") Then Return Await vblib.App.GetTwarzetnikFeed(oFeed)
            'If oFeed.sUri.ToLower.Contains("instagram.com") Then Return Await vblib.App.GetInstagramFeed(oFeed)
            If oFeed.sUri.ToLower.Contains("facebook.com") Then Return Nothing
            If oFeed.sUri.ToLower.Contains("instagram.com") Then Return Nothing
    Return Await GetRssFeed(oFeed)

    Catch ex As Exception
            CrashMessageAdd("GetAnyFeed switch catch: " & oFeed.sUri, ex)
        End Try

        Return Nothing
    End Function
#End If

#Region "Rss feed"

#If Not RSS_IN_VBLIB Then

    Private Shared Function ExtractPicLink(sContent As String) As String
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

    Private Shared Function RssGetFeedName(oFeed As VBlib.JedenFeed, oNode As TypRSS.SyndicationItem) As String
        If oFeed.sName <> "" Then Return oFeed.sName
        ' Return oNode.SourceFeed.Title.Text.Trim
        Return ""
    End Function

    Private Shared Function RssGetDate(oNode As TypRSS.SyndicationItem) As String
        ' 2018.11.10, TomsHardware Atom ma Update nie ma Published

        Try
            ' TypRSS: dla .Net: PublishDate, dla UWP: PublishedDate
#If RSS_IN_VBLIB Then
            If oNode.PublishDate < New DateTime(1900, 1, 1) Then
                If oNode.LastUpdatedTime > New DateTime(1900, 1, 1) Then Return oNode.LastUpdatedTime.ToString
            End If
            Return oNode.PublishDate.ToString.ToString
#Else
                        If oNode.PublishedDate < New DateTime(1900, 1, 1) Then
                If oNode.LastUpdatedTime > New DateTime(1900, 1, 1) Then Return oNode.LastUpdatedTime.ToString
            End If
            Return oNode.PublishedDate.ToString.ToString
#End If
        Catch ex As Exception
            ' Devil daje tu błąd? 
            Return ""
        End Try

    End Function

    Private Shared Function RssGetTitle(oNode As TypRSS.SyndicationItem) As String
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

    Private Shared Function RssGetSummaryOrContent(oNode As TypRSS.SyndicationItem) As String
        Dim sTmp As String = ""
        If oNode.Summary IsNot Nothing Then sTmp = oNode.Summary.Text
        If sTmp = "" AndAlso oNode.Content IsNot Nothing Then

#If RSS_IN_VBLIB Then
            ' TypRSS: dla .Net
            Dim oTmp As TypRSS.TextSyndicationContent = TryCast(oNode.Content, ServiceModel.Syndication.TextSyndicationContent)
            If oTmp Is Nothing Then Return ""
            sTmp = oTmp.Text
#Else
            ' TypRSS: dla UWP
            sTmp = oNode.Content.Text
#End If
        End If

        Return sTmp
    End Function


    Private Shared Function RssGetPicLink(oNode As TypRSS.SyndicationItem) As String
        ' https://social.msdn.microsoft.com/Forums/en-US/8b6a4d60-b286-41fc-997b-9fde81d0f14a/how-to-get-content-from-syndicationitem

        Dim sTmp As String = RssGetSummaryOrContent(oNode)
        sTmp = Net.WebUtility.HtmlDecode(sTmp)
        Return ExtractPicLink(sTmp)
    End Function

    Private Shared Function RssGetHtmlData(oNode As TypRSS.SyndicationItem, iConvertLinks As Integer) As String
        Dim sTmp As String = RssGetSummaryOrContent(oNode)
        sTmp = Net.WebUtility.HtmlDecode(sTmp)
        If iConvertLinks > 0 Then sTmp = VBlib.App.ActivateLinks(sTmp)
        Return sTmp
    End Function
    Private Shared Function RssGetDescLink(oNode As TypRSS.SyndicationItem, oFeed As VBlib.JedenFeed) As String
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
            For Each oLink As TypRSS.SyndicationLink In oNode.Links
                ' dla TypRSS: .Net RelationshipType, dla UWP: Relationship
#If RSS_IN_VBLIB Then
                If oLink.RelationshipType = "alternate" Then
#Else
                If oLink.Relationship = "alternate" Then
#End If
                    ' http://onlyoldmovies.blogspot.com/2019/03/born-yesterday-1950-classic-comedydrama.html
                    sLinkToDescr = oLink.Uri.AbsoluteUri
                    Exit For
                End If
            Next
        End If

        Return sLinkToDescr
    End Function

    Private Shared Function RssGetGuid(oNode As TypRSS.SyndicationItem, oFeed As VBlib.JedenFeed) As String
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

        ' 2018.11.10, dla TomsHardware ktory ma długie ID
        ' https://www.tomshardware.com/news/sapphire-radeon-rx-590-nitro-special-edition-specs,38051.html#xtor=RSS-5
        ' dziala też https://www.tomshardware.com/news/moje,38051.html
        ' skrócenie powoduje oszczędność SettingString, i mieści się więcej
        If oFeed.sUri.ToLower.Contains("tomshardware") Then
            sGuid = sGuid.Replace("#xtor=RSS-5", "")    ' skracamy końcówkę
            iInd = sGuid.IndexOf("/", "https://www.tomshardware.com/n".Length) ' to nie musi być news!
            sTmp = sGuid.Substring(0, iInd + 1)   ' https://www.tomshardware.com/(news|reviews|...)
            iInd = sGuid.LastIndexOf(",")
            If iInd > 0 Then
                sGuid = sGuid.Substring(iInd)
            End If
        End If

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
            For Each oLink As TypRSS.SyndicationLink In oNode.Links
                ' dla TypRSS: .Net RelationshipType, dla UWP: Relationship
#If RSS_IN_VBLIB Then
                If oLink.RelationshipType = "alternate" Then
#Else
                If oLink.Relationship = "alternate" Then
#End If
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

        Return sGuid

    End Function

    Private Shared Function ConvertRssItemToJedenItem(oFeed As VBlib.JedenFeed, oNode As TypRSS.SyndicationItem) As VBlib.JedenItem

        Dim oNew As New VBlib.JedenItem

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
            Return Nothing
        End If

        Return oNew
    End Function

    Private Shared Async Function GetRssSyndFeed(oFeed As VBlib.JedenFeed) As Task(Of TypRSS.SyndicationFeed)

        Dim sPage As String
        ' wedle wersji TypRSS
        ' .Net
        Try
            VBlib.HttpPageSetAgent("FilteredRSS") '  konieczne dla LoveKrakow było w Windows.Web.Syndication.SyndicationClient, więc niech będzie
            sPage = Await VBlib.HttpPageAsync(oFeed.sUri)
            If sPage = "" Then Return Nothing
            ' wycięcie z DevilTorrents, do znaku "<"
            sPage = sPage.TrimBefore("<")
            sPage = sPage.Replace("CET</pubDate>", "</pubDate>") ' przykład nie ma strefy czasowej, więc wedle https://docs.microsoft.com/en-us/dotnet/api/system.servicemodel.syndication.syndicationitem.publishdate?f1url=%3FappId%3DDev16IDEF1%26l%3DEN-US%26k%3Dk(System.ServiceModel.Syndication.SyndicationItem.PublishDate);k(DevLang-VB)%26rd%3Dtrue&view=dotnet-plat-ext-6.0
            sPage = sPage.Replace("CEST</pubDate>", "</pubDate>")
        Catch ex As Exception
            'błąd czytania z WWW
            Return Nothing
        End Try

        Dim oRssFeed As TypRSS.SyndicationFeed = Nothing

        ' Try
        '    Using oReader As Xml.XmlReader = Xml.XmlReader.Create(New IO.StringReader(sPage))
        '        oRssFeed = TypRSS.SyndicationFeed.Load(oReader)
        '    End Using
        'Catch
        '    Return Nothing
        'End Try

        ' wersja TypRSS: UWP
        Try
            oRssFeed = New Windows.Web.Syndication.SyndicationFeed
            oRssFeed.Load(sPage)
        Catch ex As Exception
            Return Nothing
        End Try


        Return oRssFeed

    End Function


    ''' <summary>
    ''' ret NULL gdy empty HttpPage, lub lista nowych
    ''' </summary>
    Private Shared Async Function GetRssFeed(oFeed As VBlib.JedenFeed) As Task(Of List(Of VBlib.JedenItem))
        ' Public Shared Function GetRssFeed(oFeed As VBlib.JedenFeed, oRssFeed As ServiceModel.Syndication.SyndicationFeed) As List(Of JedenItem)
        Dim oRssFeed As TypRSS.SyndicationFeed = Await GetRssSyndFeed(oFeed)
        If oRssFeed Is Nothing Then Return Nothing

        Dim oRetList As New List(Of VBlib.JedenItem)

        For Each oNode As TypRSS.SyndicationItem In oRssFeed.Items
            Try
                ' dla TypRSS: .Net tak, UWP: comment
                ' oNode.SourceFeed = oRssFeed ' bo niestety sam tego nie robi

                Dim oNew As VBlib.JedenItem = App.ConvertRssItemToJedenItem(oFeed, oNode)

                If oNew Is Nothing Then
                    ' z Devila, za daleko w historii - koniec pętli
                    Exit For
                End If

                ' warunek dla normalnego - wcześniej go nie było widać; dla twittera zawsze spełniony (tam nie ma "|")
                If Not oFeed.sLastGuids.Contains(oNew.sGuid & "|") Then
                    ' dla TypRSS: .Net Publish, UWP Published
#If RSS_IN_VBLIB Then
                    If Not VBlib.App.NodeToIgnore(oFeed, oNode.PublishDate, oNew) Then
#Else
                    If Not VBlib.App.NodeToIgnore(oFeed, oNode.PublishedDate, oNew) Then
#End If
                        oFeed.sLastGuids = oFeed.sLastGuids & oNew.sGuid & "|"
                        If oNew.sGuid > oFeed.sLastGuid Then oFeed.sLastGuid = oNew.sGuid
                        oRetList.Add(oNew)
                    End If
                End If
            Catch
                'w razie błędu przy konwersji itemu, próbuj następne
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

#End If

#End Region


    Private Shared Function ShouldShowEmptyFeedWarning(sLastOkTimeStamp As String) As Boolean
        ' nic nie ma... sprawdz czy to nie error! (czy nie za dlugo nie ma)
        Dim sCurrDate As String = Date.Now.ToString("yyMMdd")
        If sLastOkTimeStamp = "" Then Return False

        Dim iCurrDate As Integer = Integer.Parse(sCurrDate)
        Dim iLastItem As Integer = Integer.Parse(sLastOkTimeStamp)
        If iLastItem + GetSettingsInt("MaxDays", 30) < iCurrDate Then Return True

        Return False
    End Function

    Public Shared Sub OpenHttpBrowser(sUri As String)
        If sUri.Substring(0, 5) = "DEVIL" Then sUri = sUri.Replace("DEVIL-", "http://devil-torrents.pl/torrent/")
        If sUri.Substring(0, 3) = "MS-" Then sUri = sUri.Replace("MS-", "https://support.microsoft.com/help/")
        OpenBrowser(sUri)
    End Sub



#Region "Toasty"


    Public Shared Sub MakeToast(sGuid As String, sMsg As String, sFeed As String)

        ' to są wszystkie starsze wersje, pewnie nie będą używane bo i tak MINBUILD 15063
        If WinVer() < 15063 Then Throw New NotImplementedException("Niestety, ToastContentBuilder tak nisko nie zadziała ")

        Dim oBldr As New Microsoft.Toolkit.Uwp.Notifications.ToastContentBuilder()

        Try
            ' jako header
            ' https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/toast-headers
            oBldr.AddHeader(sFeed, sFeed, "")

            sMsg = sMsg.Replace("( Seedów: ", "(S:")
            sMsg = sMsg.Replace(" Peerów: ", "+P:")
            sMsg = sMsg.Replace(" )", ")")
            oBldr.AddText(sMsg)

            oBldr.AddButton(New Microsoft.Toolkit.Uwp.Notifications.ToastButtonDismiss)

            If sGuid <> "" Then
                oBldr.AddButton(GetSettingsString("resDelete"),
                        Microsoft.Toolkit.Uwp.Notifications.ToastActivationType.Background, "DELE" & sGuid)
            End If

            oBldr.AddButton(GetSettingsString("resOpen"),
                        Microsoft.Toolkit.Uwp.Notifications.ToastActivationType.Foreground, "OPEN" & sGuid)

            'oBldr.AddButton(GetSettingsString("resBrowser"),
            '            Microsoft.Toolkit.Uwp.Notifications.ToastActivationType.Foreground, "BROW" & sGuid)

            oBldr.AddArgument("OPEN" & sGuid)
        Catch ex As Exception
            CrashMessageAdd("Cannot create Toast for feed: " & sFeed & " (builder)", ex)
            Return
        End Try

        Dim oToast As ToastNotification

        Try
            oToast = New ToastNotification(oBldr.GetXml)
        Catch ex As Exception
            CrashMessageAdd("Cannot create toast for feed: " & sFeed & " (notification)", ex)
            Return
        End Try

        Try
            Dim sTag As String = VBlib.App.ConvertGuidToTag(sGuid)
            If sTag <> "" Then oToast.Tag = sTag
        Catch ex As Exception
            CrashMessageAdd("Cannot create toast for feed: " & sFeed & " (guid)", ex)
        End Try

        Try
            ToastNotificationManager.CreateToastNotifier().Show(oToast)
        Catch ex As Exception
            CrashMessageAdd("Cannot create toast for feed: " & sFeed & " (sending)", ex)
        End Try

    End Sub

#End Region

#Region "przeskok do VBlib"


    Public Shared Sub SaveIndex(bForce As Boolean)
        Try
            If bForce OrElse bChangedXML Then
                VBlib.App.SaveIndex(ApplicationData.Current.LocalCacheFolder.Path)
                bChangedXML = False
            End If
        Catch ex As Exception
            CrashMessageAdd("SaveIndex CatchBlock", ex)
        End Try
    End Sub

    Public Shared Sub LoadIndex()
        VBlib.App.LoadIndex(ApplicationData.Current.LocalCacheFolder.Path)
    End Sub

#Region "Killfile"

    Public Shared Sub KillFileLoad(bMsg As Boolean)
        VBlib.App.KillFileLoad(bMsg, Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path)
    End Sub
#End Region


#End Region


End Class

