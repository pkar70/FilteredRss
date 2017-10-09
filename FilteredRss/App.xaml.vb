
Imports Windows.Data.Xml.Dom
Imports Windows.ApplicationModel.Background
Imports Windows.Storage
Imports Windows.UI.Notifications
''' <summary>
''' Provides application-specific behavior to supplement the default Application class.
''' </summary>
NotInheritable Class App
    Inherits Application

    'Public sFeed As String

    Dim rootFrame As Frame

    ''' <summary>
    ''' Invoked when the application is launched normally by the end user.  Other entry points
    ''' will be used when the application is launched to open a specific file, to display
    ''' search results, and so forth.
    ''' </summary>
    ''' <param name="e">Details about the launch request and process.</param>
    Protected Overrides Sub OnLaunched(e As Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)
        ' Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        rootFrame = TryCast(Window.Current.Content, Frame)

        ' Do not repeat app initialization when the Window already has content,
        ' just ensure that the window is active

        If rootFrame Is Nothing Then
            ' Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = New Frame()

            AddHandler rootFrame.NavigationFailed, AddressOf OnNavigationFailed

            If e.PreviousExecutionState = ApplicationExecutionState.Terminated Then
                ' TODO: Load state from previously suspended application
            End If
            ' Place the frame in the current Window
            Window.Current.Content = rootFrame
        End If

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

    Protected Overrides Sub OnBackgroundActivated(args As BackgroundActivatedEventArgs)
        If Not rootFrame Is Nothing Then rootFrame.Content.CalledFromBackground(args.TaskInstance.Task.Name)
    End Sub

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

    Public Shared Sub SetBadgeNo(iInt As Integer)
        ' https://docs.microsoft.com/en-us/windows/uwp/controls-and-patterns/tiles-and-notifications-badges
        Dim oXmlBadge = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber)
        Dim oXmlNum = CType(oXmlBadge.SelectSingleNode("/badge"), XmlElement)
        oXmlNum.SetAttribute("value", iInt.ToString)
        BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(New BadgeNotification(oXmlBadge))
    End Sub

    Public Shared Sub UnregisterTriggers(Optional bAll As Boolean = False)

        For Each oTask In BackgroundTaskRegistration.AllTasks
            If oTask.Value.Name = "FilteredRSStimer" Then oTask.Value.Unregister(True)
            If bAll And oTask.Value.Name = "FilteredRSSservCompl" Then oTask.Value.Unregister(True)
        Next

        If bAll Then BackgroundExecutionManager.RemoveAccess()

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

            builder.SetTrigger(New SystemTrigger(SystemTriggerType.ServicingComplete, True))
            builder.Name = "FilteredRSSservCompl"
            Try
                oRet = builder.Register()
            Catch ex As Exception
                Dim a As Integer
                a = 0
            End Try

            ' system trigger: ServicingComplete , po aktualizacji - deregister and reregister background
        End If

    End Function

    Public Shared Function XmlSafeString(sInput As String) As String
        Dim sTmp As String
        sTmp = sInput.Replace("&", "&amp;")
        sTmp = sTmp.Replace("<", "&lt;")
        sTmp = sTmp.Replace(">", "&gt;")
        Return sTmp
    End Function

    Public Shared Sub MakeToast(sMsg As String, Optional sMsg1 As String = "")
        Dim sXml = "<visual><binding template='ToastGeneric'><text>" & XmlSafeString(sMsg)
        If sMsg1 <> "" Then sXml = sXml & "</text><text>" & XmlSafeString(sMsg1)
        sXml = sXml & "</text></binding></visual>"
        Dim oXml = New XmlDocument
        oXml.LoadXml("<toast>" & sXml & "</toast>")
        Dim oToast = New ToastNotification(oXml)
        ToastNotificationManager.CreateToastNotifier().Show(oToast)
    End Sub

    Public Shared Function RegExp(sTxt As String, sPattern As String) As Integer

        Dim iRet = 99
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
        Dim aLines = sTxt.Split("<br>")    ' zapewne beda "text|<|br>text|"
        If aLines.Count < 2 Then aLines = sTxt.Split("<br/>")
        For Each sLine In aLines
            If App.RegExpOrInstr(sLine, sPattern) Then Return True
        Next
        Return False
    End Function

    Public Shared Function TestNodeMatch(sPattern As String, sT As String, sD As String) As Boolean
        If sPattern.Length < 3 Then Return False

        If sPattern.Substring(0, 2).ToLower = "t:" Then Return RegExpOrInstr(sT, sPattern.Substring(2))
        If sPattern.Substring(0, 2).ToLower = "d:" Then Return RegExpPerLine(sD, sPattern.Substring(2))

        If RegExpOrInstr(sT, sPattern) Then Return True
        Return RegExpPerLine(sD, sPattern)

    End Function
End Class
