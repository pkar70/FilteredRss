﻿
Imports Windows.ApplicationModel.Background
Imports Windows.Storage
Imports Windows.UI.Notifications
Imports System.Linq.Expressions
Imports VBlib.Extensions


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
                    gbInTimer = True  ' a to po co? przecież nic z tego nie korzysta
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

                Dim oNewList As List(Of VBlib.JedenItem) = Await VBlib.App.GetAnyFeed(oFeed)

                feeds.FeedsSave(False) ' local, nie Roaming

                ' przeniesione z GetRssFeed
                If oNewList Is Nothing Then
                    ' był błąd
                    If oTB IsNot Nothing Then Await DialogBoxAsync("Bad response from " & oFeed.sName)
                    Continue For
                End If

                ' przeniesione z GetRssFeed
                If oNewList.Count < 1 Then
                    ' nic nie ma... sprawdz czy to nie error! (czy nie za dlugo nie ma)
                    If ShouldShowEmptyFeedWarning(oFeed.sLastOkDate) Then
                        If oTB IsNot Nothing Then Await DialogBoxAsync("Seems like feed " & oFeed.sName & " is dead?")
                    End If
                    Continue For
                End If

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

        Try
            Dim oBldr As New Microsoft.Toolkit.Uwp.Notifications.ToastContentBuilder()

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

            Dim oToast As New ToastNotification(oBldr.GetXml)

            ' The size of the notification tag is too large.
            ' tag: 16 chrs, od Creators (14971 ??) jest 64 chrs
            ' https://docs.microsoft.com/en-us/uwp/api/windows.ui.notifications.toastnotification.tag#Windows_UI_Notifications_ToastNotification_Tag
            If sGuid.Length > 64 Then sGuid = sGuid.Substring(0, 64)
            If sGuid <> "" Then oToast.Tag = sGuid

            ToastNotificationManager.CreateToastNotifier().Show(oToast)

        Catch ex As Exception
            CrashMessageAdd("Cannot create toast for feed: " & sFeed)
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

