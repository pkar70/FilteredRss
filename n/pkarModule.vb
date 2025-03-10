﻿' (...)
'            AddHandler rootFrame.NavigationFailed, AddressOf OnNavigationFailed
'
'            ' PKAR added wedle https://stackoverflow.com/questions/39262926/uwp-hardware-back-press-work-correctly-in-mobile-but-error-with-pc
'            AddHandler rootFrame.Navigated, AddressOf OnNavigatedAddBackButton
'            AddHandler Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested, AddressOf OnBackButtonPressed
' (...)


' 2022.04.03: sync z uzupelnionym pkarlibmodule, przerzucenie czesci rzeczy do Extensions

' PLIK DOŁĄCZANY
' założenie: jest VBlib z pkarlibmodule
' mklink pkarModule.vb ..\..\_mojeSuby\pkarModuleWithLib.vb
' PLIK DOŁACZANY

' historia:
' historia.pkarmodule.vb

' 2022.05.02: NetIsIPavail param bMsg jest teraz optional (default: bez pytania)

' 2024.01.13  + FrameworkElement.SetUiPropertiesFromLang, FrameworkElementSetFromResourcesTree, TextBlock.SetLangText


' Włącz "PK_USE_TRIGGERS" gdy ma być obsługa Triggerów
' PK_USE_TOASTS dla toastów


#If NETFX_CORE Then
Imports MsExtConfig = Microsoft.Extensions.Configuration
Imports MsExtPrim = Microsoft.Extensions.Primitives

Imports WinAppData = Windows.Storage.ApplicationData
#End If

#If Not NETFX_CORE And Not PK_WPF Then
Imports Microsoft.UI.Xaml
Imports Microsoft.UI.Xaml.Data ' dla IValueConverter
Imports Windows.Foundation.Collections  ' dla IPropertySet
Imports Windows.ApplicationModel    ' dla DataTransfer
#End If

#If Not NETFX_CORE Then
Imports System.IO   ' dla Stream oraz OpenStreamForWrite
Imports System.Runtime.CompilerServices
Imports System.Reflection
#End If

Imports pkar
Imports pkar.UI.Extensions
Imports pkar.DotNetExtensions
Imports Windows.Devices
Imports System.Reflection







#If PK_USE_TOASTS Then
Imports pkar.UI.Toasts
#End If



Partial Public Class App
    Inherits Application
#If NETFX_CORE Then

#Region "Back button"

#If NETFX_CORE Then

    ' PKAR added wedle https://stackoverflow.com/questions/39262926/uwp-hardware-back-press-work-correctly-in-mobile-but-error-with-pc
    Private Sub OnNavigatedAddBackButton(sender As Object, e As NavigationEventArgs)
        Try
            Dim oFrame As Frame = TryCast(sender, Frame)
            If oFrame Is Nothing Then Exit Sub

            Dim oNavig As Windows.UI.Core.SystemNavigationManager = Windows.UI.Core.SystemNavigationManager.GetForCurrentView

            If oFrame.CanGoBack Then
                oNavig.AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible
            Else
                oNavig.AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed
            End If

            Return

        Catch ex As Exception
            pkar.CrashMessageExit("@OnNavigatedAddBackButton", ex.Message)
        End Try

    End Sub

    Private Sub OnBackButtonPressed(sender As Object, e As Windows.UI.Core.BackRequestedEventArgs)
        Try
            TryCast(Window.Current.Content, Controls.Frame)?.GoBack()
            e.Handled = True
        Catch ex As Exception
        End Try
    End Sub
#Else
    Private Sub OnNavigatedAddBackButton(sender As Object, e As Object)
        ' tak naprawdę e to NavigationEventArgs, ale do tego trzeba imports Microsoft.UI.Xaml.Navigation (na WinUI3, bo na UWP nie trzeba)
        Try
            Dim oFrame As Controls.Frame = TryCast(sender, Controls.Frame)
            If oFrame Is Nothing Then Exit Sub
            If Not oFrame.CanGoBack Then Return

            Dim oPage As Controls.Page = TryCast(oFrame.Content, Controls.Page)
            If oPage Is Nothing Then Return

            Dim oGrid As Controls.Grid = TryCast(oPage.Content, Controls.Grid)
            If oGrid Is Nothing Then Return

            Dim oButton As New Controls.Button With {
            .Content = New Controls.SymbolIcon(Controls.Symbol.Back),
            .Name = "uiPkAutoBackButton",
                    .VerticalAlignment = VerticalAlignment.Top,
                    .HorizontalAlignment = HorizontalAlignment.Left}
            AddHandler oButton.Click, AddressOf OnBackButtonPressed

            Dim iCols As Integer = 0
            If oGrid.ColumnDefinitions IsNot Nothing Then iCols = oGrid.ColumnDefinitions.Count ' może być 0
            Dim iRows As Integer = 0
            If oGrid.RowDefinitions IsNot Nothing Then iRows = oGrid.RowDefinitions.Count ' może być 0
            If iRows > 1 Then
                Controls.Grid.SetRow(oButton, 0)
                Controls.Grid.SetRowSpan(oButton, iRows)
            End If
            If iCols > 1 Then
                Controls.Grid.SetColumn(oButton, 0)
                Controls.Grid.SetColumnSpan(oButton, iCols)
            End If
            oGrid.Children.Add(oButton)


        Catch ex As Exception
            pkar.CrashMessageExit("@OnNavigatedAddBackButton", ex.Message)
        End Try

    End Sub

    Private Sub OnBackButtonPressed(sender As Object, e As RoutedEventArgs)
        Dim oFE As FrameworkElement = sender
        Dim oPage As Controls.Page = Nothing

        While True
            oPage = TryCast(oFE, Controls.Page)
            If oPage IsNot Nothing Then Exit While
            oFE = oFE.Parent
            If oFE Is Nothing Then Return
        End While

        oPage.GoBack

    End Sub
#End If

#End Region

#Region "RemoteSystem/Background"
    Private moTaskDeferal As Windows.ApplicationModel.Background.BackgroundTaskDeferral = Nothing
    Private moAppConn As Windows.ApplicationModel.AppService.AppServiceConnection
    Private msLocalCmdsHelp As String = ""

    Private Sub RemSysOnServiceClosed(appCon As Windows.ApplicationModel.AppService.AppServiceConnection, args As Windows.ApplicationModel.AppService.AppServiceClosedEventArgs)
        If appCon IsNot Nothing Then appCon.Dispose()
        If moTaskDeferal IsNot Nothing Then
            moTaskDeferal.Complete()
            moTaskDeferal = Nothing
        End If
    End Sub

    Private Sub RemSysOnTaskCanceled(sender As Windows.ApplicationModel.Background.IBackgroundTaskInstance, reason As Windows.ApplicationModel.Background.BackgroundTaskCancellationReason)
        If moTaskDeferal IsNot Nothing Then
            moTaskDeferal.Complete()
            moTaskDeferal = Nothing
        End If
    End Sub

    ''' <summary>
    ''' do sprawdzania w OnBackgroundActivated
    ''' jak zwróci True, to znaczy że nie wolno zwalniać moTaskDeferal !
    ''' sLocalCmdsHelp: tekst do odesłania na HELP
    ''' </summary>
    Public Function RemSysInit(args As Activation.BackgroundActivatedEventArgs, sLocalCmdsHelp As String) As Boolean
        Dim oDetails As Windows.ApplicationModel.AppService.AppServiceTriggerDetails =
                TryCast(args.TaskInstance.TriggerDetails, Windows.ApplicationModel.AppService.AppServiceTriggerDetails)
        If oDetails Is Nothing Then Return False

        msLocalCmdsHelp = sLocalCmdsHelp

        AddHandler args.TaskInstance.Canceled, AddressOf RemSysOnTaskCanceled
        moAppConn = oDetails.AppServiceConnection
        AddHandler moAppConn.RequestReceived, AddressOf RemSysOnRequestReceived
        AddHandler moAppConn.ServiceClosed, AddressOf RemSysOnServiceClosed
        Return True

    End Function

    Public Async Function CmdLineOrRemSys(sCommand As String) As Task(Of String)
        Dim sResult As String = AppServiceStdCmd(sCommand, msLocalCmdsHelp)
        If String.IsNullOrEmpty(sResult) Then
#If NETFX_CORE Then
            sResult = Await AppServiceLocalCommand(sCommand)
#End If
        End If

        Return sResult
    End Function

    Public Async Function ObsluzCommandLine(sCommand As String) As Task

        Dim oFold As Windows.Storage.StorageFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder
        If oFold Is Nothing Then Return

        Dim sLockFilepathname As String = IO.Path.Combine(oFold.Path, "cmdline.lock")
        Dim sResultFilepathname As String = IO.Path.Combine(oFold.Path, "stdout.txt")

        Try
            IO.File.WriteAllText(sLockFilepathname, "lock")
        Catch ex As Exception
            Return
        End Try

        Dim sResult = Await CmdLineOrRemSys(sCommand)
        If String.IsNullOrEmpty(sResult) Then
            sResult = "(empty - probably unrecognized command)"
        End If

        IO.File.WriteAllText(sResultFilepathname, sResult)

        IO.File.Delete(sLockFilepathname)

    End Function

    Private Async Sub RemSysOnRequestReceived(sender As Windows.ApplicationModel.AppService.AppServiceConnection, args As Windows.ApplicationModel.AppService.AppServiceRequestReceivedEventArgs)
        '// 'Get a deferral so we can use an awaitable API to respond to the message 

        Dim sStatus As String
        Dim sResult As String = ""
        Dim messageDeferral As Windows.ApplicationModel.AppService.AppServiceDeferral = args.GetDeferral()

        If vblib.GetSettingsBool("remoteSystemDisabled") Then
            sStatus = "No permission"
        Else

            Dim oInputMsg As Windows.Foundation.Collections.ValueSet = args.Request.Message

            sStatus = "ERROR while processing command"

            If oInputMsg.ContainsKey("command") Then

                Dim sCommand As String = oInputMsg("command")
                sResult = Await CmdLineOrRemSys(sCommand)
            End If

            If sResult <> "" Then sStatus = "OK"

        End If

        Dim oResultMsg As New Windows.Foundation.Collections.ValueSet From {
            {"status", sStatus},
            {"result", sResult}
        }

        Await args.Request.SendResponseAsync(oResultMsg)

        messageDeferral?.Complete()
        moTaskDeferal?.Complete()

    End Sub


#End Region

#Else
#Region "Commandline"
    Private msLocalCmdsHelp As String = ""

    Sub app_Startup(sender As Object, e As StartupEventArgs)
        If e.Args.Length > 0 Then
            Dim sCmdLine As String = ""
            For Each oneCmd In e.Args
                If sCmdLine <> "" Then sCmdLine &= " "
                sCmdLine &= oneCmd
            Next

            ObsluzCommandLine(sCmdLine).Wait()

        End If
    End Sub

#Disable Warning BC42356 ' This async method lacks 'Await' operators and so will run synchronously
    Public Async Function CmdLineOrRemSys(sCommand As String) As Task(Of String)
#Enable Warning BC42356 ' This async method lacks 'Await' operators and so will run synchronously
        Dim sResult As String = AppServiceStdCmd(sCommand, msLocalCmdsHelp)
        If String.IsNullOrEmpty(sResult) Then
            ' *TODO* sResult = Await AppServiceLocalCommand(sCommand)
        End If

        Return sResult
    End Function


    Public Async Function ObsluzCommandLine(sCommand As String) As Task

        Dim sResult = Await CmdLineOrRemSys(sCommand)
        If String.IsNullOrEmpty(sResult) Then
            sResult = "(empty - probably unrecognized command)"
        End If

        Console.WriteLine(sResult)
    End Function

#End Region
#End If


#If NETFX_CORE Then
    Public Shared Sub OpenRateIt()
        Dim sUri As New Uri("ms-windows-store://review/?PFN=" & Package.Current.Id.FamilyName)
        sUri.OpenBrowser
    End Sub
#End If

End Class

Public Module pkar


#Region "import settings"

    ' nie dla UWP
#If Not NETFX_CORE Then

    ''' <summary>
    ''' import settingsów JSON z UWP, o ile tam są a tutaj nie ma - wywoływać przed InitLib!
    ''' </summary>
    ''' <param name="packageName">Zobacz w Manifest, Packaging, Package Name</param>
    Public Sub TryImportSettingsFromUwp(packageName As String)
        Dim sPath As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)

        Dim uwpPath As String = IO.Path.Combine(sPath, packageName)
        Dim wpfPath As String = IO.Path.Combine(sPath, GetAppName)

        ' normalne
        ' UWP = C:\Users\xxx\AppData\Local\Packages\xxx\LocalState)
        ' WPF = WinUI3 = MAUI = C:\Users\xxx\AppData\Local
        TryImportSettingsFromDir(IO.Path.Combine(uwpPath, "LocalState"), wpfPath)

        ' roaming
        ' UWP = C:\Users\xxx\AppData\Local\Packages\xxx\RoamingState)
        ' WPF = WinUI3 = MAUI = C:\Users\xxx\AppData\Roaming
        Dim dirsep As String = IO.Path.DirectorySeparatorChar
        wpfPath = wpfPath.Replace(dirsep & "Local", dirsep & "Roaming")
        TryImportSettingsFromDir(IO.Path.Combine(uwpPath, "RoamingState"), wpfPath)
    End Sub

    Private Sub TryImportSettingsFromDir(srcDir As String, dstDir As String)
        Dim JSON_FILENAME As String = "AppSettings.json"

        Dim srcFile As String = IO.Path.Combine(srcDir, JSON_FILENAME)
        If Not IO.File.Exists(srcFile) Then Return

        Dim dstFile As String = IO.Path.Combine(dstDir, JSON_FILENAME)
        If IO.File.Exists(dstFile) Then Return

        If Not IO.Directory.Exists(dstDir) Then IO.Directory.CreateDirectory(dstDir)

        IO.File.Copy(srcFile, dstFile)
    End Sub

    Public Function GetAppName() As String
        Dim sAssemblyFullName = Reflection.Assembly.GetEntryAssembly().FullName
        Dim oAss As New Reflection.AssemblyName(sAssemblyFullName)
        Return oAss.Name
    End Function


#End If
#End Region



    ''' <summary>
    ''' dla starszych: InitLib(Nothing)
    ''' dla nowszych:  InitLib(Environment.GetCommandLineArgs)
    ''' inicjalizacja Settings, Toasts, DialogBoxes, Clip, Lang
    ''' </summary>
    Public Sub InitLib(aCmdLineArgs As List(Of String), Optional bUseOwnFolderIfNotSD As Boolean = True)
#If NETFX_CORE Then
        UI.Configs.InitSettings(vblib.IniLikeDefaults.sIniContent, False, aCmdLineArgs)


#Else

#If DEBUG Then
        UI.Configs.InitSettings(VBlib.IniLikeDefaults.sIniContent, True)
#Else
        ui.configs.InitSettings(Vblib.IniLikeDefaults.sIniContent, False)
#End If

#End If
#If PK_USE_TOASTS Then
        VBlib.LibInitToast(AddressOf FromLibMakeToast)
#End If

        vblib.LibInitDialogBox(AddressOf FromLibDialogBoxAsync, AddressOf FromLibDialogBoxYNAsync, AddressOf FromLibDialogBoxInputAllDirectAsync)

        vblib.LibInitClip(AddressOf FromLibClipPut, AddressOf FromLibClipPutHtml)
        '#Disable Warning BC42358 ' Because this call is not awaited, execution of the current method continues before the call is completed
        '        ' InitDatalogFolder(bUseOwnFolderIfNotSD)
        '#Enable Warning BC42358 ' Because this call is not awaited, execution of the current method continues before the call is completed
#Disable Warning BC40000 ' Type or member is obsolete
        VBlib.LangEnsureInit()  ' od tej pory na pewno zadziała lokalizacja
#Enable Warning BC40000 ' Type or member is obsolete
    End Sub

#Region "CrashMessage"
    ' większość w VBlib

    ''' <summary>
    ''' Inicjalizacja modułu - nazwa app, oraz hostname. Wywołanie niekonieczne (bez tego po prostu prefix=Empty)
    ''' </summary>
    Public Sub CrashMessageInit()
        'vblib.CrashMessageSetPrefix(GetHostName(), Package.Current.DisplayName)
        vblib.CrashMessageSetPrefix(GetHostName(), Package.Current.DisplayName)

    End Sub

    ''' <summary>
    ''' DialogBox z dotychczasowym logiem i skasowanie logu. Odporne na brak inicjalizacji zmiennych
    ''' </summary>
    Public Async Function CrashMessageShowAsync() As Task
        CrashMessageInit()

        Dim sTxt As String = vblib.CrashMessageReset
        If sTxt = "" Then Return
        Await vblib.DialogBoxAsync("FAIL messages:" & vbCrLf & sTxt)
    End Function

    ''' <summary>
    ''' Dodaj do logu, ewentualnie toast, i zakończ App
    ''' </summary>
    Public Sub CrashMessageExit(sTxt As String, exMsg As String)
        vblib.CrashMessageAdd(sTxt, exMsg)
        ZamknijMnie()
    End Sub

    Private Sub ZamknijMnie()
#If PK_WPF Then
        System.Windows.Application.Current.Shutdown()
#Else
        Application.Current.Exit()
#End If
    End Sub

#If NETFX_CORE Then

    ''' <summary>
    ''' Do wywoływania jako handler "last resort", toast z błędu; zamyka app
    ''' </summary>
    Public Sub GlobalError(sender As Object, e As Windows.UI.Xaml.UnhandledExceptionEventArgs)
        ' e.Handled = True    ' // prevent the application from crashing
        Dim sTxt As String = CrashMessageGetSaveStackMessage(e)
        CrashMessageToastInternal(sTxt)
        VBlib.CrashMessageAdd(sTxt, e.Exception, True)
        ' Await pkar.DialogBox(sTxt) - bo teraz do toast idzie
        ZamknijMnie()
    End Sub


    ''' <summary>
    ''' "last resort" wysyłania Toastu, beż żadnych bibliotek (żeby zawsze zadziałało).
    ''' </summary>
    Public Sub CrashMessageToastInternal(sMsg As String)
        Dim sXml = "<visual><binding template='ToastGeneric'><text>Global catch:</text>"

        sXml = sXml & "<text>" & sMsg & "</text></binding></visual>"
        Dim oXml = New Windows.Data.Xml.Dom.XmlDocument
        oXml.LoadXml("<toast>" & sXml & "</toast>")
        Dim oToast = New Windows.UI.Notifications.ToastNotification(oXml)
        Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().Show(oToast)

    End Sub

    Private Function CrashMessageGetSaveStackMessage(e As Windows.UI.Xaml.UnhandledExceptionEventArgs) As String
        Dim ret As String = VBlib.CrashMessageGetStackMessage(e.Exception, True)
        ret = New XText(ret).ToString()
        ret = ret.Replace("""", "&quote;")
        Return ret

    End Function
#End If


#End Region

    ' -- CLIPBOARD ---------------------------------------------

#Region "ClipBoard"
    Private Sub FromLibClipPut(sTxt As String)
        sTxt.SendToClipboard
    End Sub

    Private Sub FromLibClipPutHtml(sHtml As String)
#If PK_WPF Then
        Clipboard.SetText(sHtml, TextDataFormat.Html)
#Else
        Dim oClipCont As New DataTransfer.DataPackage With {
            .RequestedOperation = DataTransfer.DataPackageOperation.Copy
        }
        oClipCont.SetHtmlFormat(sHtml)
        DataTransfer.Clipboard.SetContent(oClipCont)
#End If
    End Sub

#Disable Warning BC42356 ' This async method lacks 'Await' operators and so will run synchronously
    ''' <summary>
    ''' w razie Catch() zwraca ""
    ''' </summary>
    Public Async Function ClipGetAsync() As Task(Of String)
#Enable Warning BC42356 ' This async method lacks 'Await' operators and so will run synchronously

#If PK_WPF Then
        Return Clipboard.GetText()
#Else
        Dim oClipCont As DataTransfer.DataPackageView = DataTransfer.Clipboard.GetContent
        Try
            Return Await oClipCont.GetTextAsync()
        Catch ex As Exception
            Return ""
        End Try
#End If
    End Function

#End Region


    ' -- Testy sieciowe ---------------------------------------------

#Region "testy sieciowe"

    Public Function IsFamilyMobile() As Boolean
#If WINDOWS8_0_OR_GREATER Or NETFX_CORE Then
        Return (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily = "Windows.Mobile")
#Else
        return False
#End If
    End Function

    Public Function IsFamilyDesktop() As Boolean
#If WINDOWS8_0_OR_GREATER Or NETFX_CORE Then
        Return (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily = "Windows.Desktop")
#Else
        return True
#End If
    End Function



    ' <Obsolete("Jest w .Net Standard 2.0 (lib)")>
    Public Function NetIsIPavailable(Optional bMsg As Boolean = False) As Boolean
        If vblib.GetSettingsBool("offline") Then Return False

        If Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() Then Return True
        If bMsg Then
            vblib.DialogBox("ERROR: no IP network available")
        End If
        Return False
    End Function

    Public Function NetIsCellInet() As Boolean

#If WINDOWS8_0_OR_GREATER Or NETFX_CORE Then
        Return Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile().IsWwanConnectionProfile
#Else
        ' można tak sprawdzić wszystkie, i jeśli jes 
        For Each oNIC As Net.NetworkInformation.NetworkInterface In Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()

            ' jeśli nie jest to link aktywny, to ignorujemy go
            If oNIC.OperationalStatus <> Net.NetworkInformation.OperationalStatus.Up Then Continue For

            ' nie jest to pełna logika, bo mogą być dodatkowe typy kiedyś...
            Select Case oNIC.NetworkInterfaceType
                Case Net.NetworkInformation.NetworkInterfaceType.Wman
                    Return True
                Case Net.NetworkInformation.NetworkInterfaceType.Wwanpp
                    Return True
                Case Net.NetworkInformation.NetworkInterfaceType.Wwanpp2
                    Return True
                Case Net.NetworkInformation.NetworkInterfaceType.GenericModem
                    Return True
                Case Net.NetworkInformation.NetworkInterfaceType.HighPerformanceSerialBus
                    Return True
                Case Net.NetworkInformation.NetworkInterfaceType.Ppp
                    Return True
                Case Net.NetworkInformation.NetworkInterfaceType.Slip
                    Return True
            End Select
        Next

        Return False
#End If
    End Function


    Public Function GetHostName() As String
#If NETFX_CORE Then
        Dim hostNames As IReadOnlyList(Of Windows.Networking.HostName) =
                Windows.Networking.Connectivity.NetworkInformation.GetHostNames()
        For Each oItem As Windows.Networking.HostName In hostNames
            If oItem.DisplayName.Contains(".local") Then
                Return oItem.DisplayName.Replace(".local", "")
            End If
        Next
        Return ""
#Else
        Return System.Environment.MachineName
#End If
    End Function

    ''' <summary>
    ''' Ale to chyba przestało działać...
    ''' </summary>
    <Obsolete("Jest w .Net Standard 2.0 (lib)")>
    Public Function IsThisMoje() As Boolean
        Dim sTmp As String = GetHostName.ToLowerInvariant
        If sTmp = "home-pkar" Then Return True
        If sTmp = "lumia_pkar" Then Return True
        If sTmp = "kuchnia_pk" Then Return True
        If sTmp = "ppok_pk" Then Return True
        'If sTmp.Contains("pkar") Then Return True
        'If sTmp.EndsWith("_pk") Then Return True
        Return False
    End Function

#If WINDOWS8_0_OR_GREATER Or NETFX_CORE Then

    ''' <summary>
    ''' w razie Catch() zwraca false
    ''' </summary>
    Public Async Function NetWiFiOffOnAsync() As Task(Of Boolean)

        Try
            ' https://social.msdn.microsoft.com/Forums/ie/en-US/60c4a813-dc66-4af5-bf43-e632c5f85593/uwpbluetoothhow-to-turn-onoff-wifi-bluetooth-programmatically?forum=wpdevelop
            Dim result222 As Windows.Devices.Radios.RadioAccessStatus = Await Windows.Devices.Radios.Radio.RequestAccessAsync()
            If result222 <> Windows.Devices.Radios.RadioAccessStatus.Allowed Then Return False

            Dim radios As IReadOnlyList(Of Windows.Devices.Radios.Radio) = Await Windows.Devices.Radios.Radio.GetRadiosAsync()

            For Each oRadio In radios
                If oRadio.Kind = Windows.Devices.Radios.RadioKind.WiFi Then
                    Dim oStat As Windows.Devices.Radios.RadioAccessStatus =
                    Await oRadio.SetStateAsync(Windows.Devices.Radios.RadioState.Off)
                    If oStat <> Windows.Devices.Radios.RadioAccessStatus.Allowed Then Return False
                    Await Task.Delay(3 * 1000)
                    oStat = Await oRadio.SetStateAsync(Windows.Devices.Radios.RadioState.On)
                    If oStat <> Windows.Devices.Radios.RadioAccessStatus.Allowed Then Return False
                End If
            Next

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

#End If

    Public Sub OpenBrowser(sLink As String)
        Dim oUri As New Uri(sLink)
        oUri.OpenBrowser
    End Sub

#Region "Bluetooth"

#If WINDOWS8_0_OR_GREATER Or NETFX_CORE Then

    ''' <summary>
    ''' Zwraca -1 (no radio), 0 (off), 1 (on), ale gdy bMsg to pokazuje dokładniej błąd (nie włączony, albo nie ma radia Bluetooth) - wedle stringów podanych, które mogą być jednak identyfikatorami w Resources
    ''' </summary>
    <Obsolete("do poprawienia MsgBox, skoro teraz res:")>
    Public Async Function NetIsBTavailableAsync(bMsg As Boolean,
                                    Optional bRes As Boolean = False,
                                    Optional sBtDisabled As String = "ERROR: Bluetooth is not enabled",
                                    Optional sNoRadio As String = "ERROR: Bluetooth radio not found") As Task(Of Integer)


        'Dim result222 As Windows.Devices.Radios.RadioAccessStatus = Await Windows.Devices.Radios.Radio.RequestAccessAsync()
        'If result222 <> Windows.Devices.Radios.RadioAccessStatus.Allowed Then Return -1

        Dim oRadios As IReadOnlyList(Of Windows.Devices.Radios.Radio) = Await Windows.Devices.Radios.Radio.GetRadiosAsync()

#If DEBUG Then
        vblib.DumpCurrMethod(", licznik=" & oRadios.Count)
        For Each oRadio As Windows.Devices.Radios.Radio In oRadios
            vblib.DumpMessage("NEXT RADIO")
            vblib.DumpMessage("name=" & oRadio.Name)
            vblib.DumpMessage("kind=" & oRadio.Kind)
            vblib.DumpMessage("state=" & oRadio.State)
        Next
#End If

        Dim bHasBT As Boolean = False

        For Each oRadio As Windows.Devices.Radios.Radio In oRadios
            If oRadio.Kind = Windows.Devices.Radios.RadioKind.Bluetooth Then
                If oRadio.State = Windows.Devices.Radios.RadioState.On Then Return 1
                bHasBT = True
            End If
        Next

        If bHasBT Then
            If bMsg Then
                If bRes Then
                    Await vblib.DialogBoxResAsync(sBtDisabled)
                Else
                    Await vblib.DialogBoxAsync(sBtDisabled)
                End If
            End If
            Return 0
        Else
            If bMsg Then
                If bRes Then
                    Await vblib.DialogBoxResAsync(sNoRadio)
                Else
                    Await vblib.DialogBoxAsync(sNoRadio)
                End If
            End If
            Return -1
        End If


    End Function

    ''' <summary>
    ''' Zwraca true/false czy State (po call) jest taki jak bOn; wymaga devCap=radios
    ''' </summary>
    Public Async Function NetTrySwitchBTOnAsync(bOn As Boolean) As Task(Of Boolean)
        Dim iCurrState As Integer = Await NetIsBTavailableAsync(False)
        If iCurrState = -1 Then Return False

        ' jeśli nie trzeba przełączać... 
        If bOn AndAlso iCurrState = 1 Then Return True
        If Not bOn AndAlso iCurrState = 0 Then Return True

        ' czy mamy prawo przełączyć? (devCap=radios)
        Dim result222 As Windows.Devices.Radios.RadioAccessStatus = Await Windows.Devices.Radios.Radio.RequestAccessAsync()
        If result222 <> Windows.Devices.Radios.RadioAccessStatus.Allowed Then Return False


        Dim radios As IReadOnlyList(Of Windows.Devices.Radios.Radio) = Await Windows.Devices.Radios.Radio.GetRadiosAsync()

        For Each oRadio In radios
            If oRadio.Kind = Windows.Devices.Radios.RadioKind.Bluetooth Then
                Dim oStat As Windows.Devices.Radios.RadioAccessStatus
                If bOn Then
                    oStat = Await oRadio.SetStateAsync(Windows.Devices.Radios.RadioState.On)
                Else
                    oStat = Await oRadio.SetStateAsync(Windows.Devices.Radios.RadioState.Off)
                End If
                If oStat <> Windows.Devices.Radios.RadioAccessStatus.Allowed Then Return False
            End If
        Next

        Return True
    End Function


#End If


#End Region

#End Region


    ' -- DialogBoxy - tylko jako wskok z VBLib ---------------------------------------------

#Region "DialogBoxy"

    Public Async Function FromLibDialogBoxAsync(sMsg As String) As Task
        Dim oPage As New Page
#If Not NETFX_CORE And Not PK_WPF Then
        oPage.XamlRoot = _XamlRoot
#End If
        Await oPage.MsgBoxAsync(sMsg)
    End Function

    ''' <summary>
    ''' Dla Cancel zwraca ""
    ''' </summary>
    Public Async Function FromLibDialogBoxYNAsync(sMsg As String, Optional sYes As String = "Tak", Optional sNo As String = "Nie") As Task(Of Boolean)
        Dim oPage As New Page
#If Not NETFX_CORE And Not PK_WPF Then
        oPage.XamlRoot = _XamlRoot
#End If
        Return Await oPage.DialogBoxYNAsync(sMsg, sYes, sNo)
    End Function

    Public Async Function FromLibDialogBoxInputAllDirectAsync(sMsg As String, Optional sDefault As String = "", Optional sYes As String = "Continue", Optional sNo As String = "Cancel") As Task(Of String)
        Dim oPage As New Page
#If Not NETFX_CORE And Not PK_WPF Then
        oPage.XamlRoot = _XamlRoot
#End If
        Return Await oPage.InputBoxAsync(sMsg, sDefault, sYes, sNo)
    End Function

#End Region


    ' --- INNE FUNKCJE ------------------------
#Region "Toasty itp"

#If PK_USE_TOASTS Then
    Private Sub FromLibMakeToast(sMsg As String, sMsg1 As String)
        Toasts.MakeToast(sMsg, sMsg1)
    End Sub
#End If

#If NETFX_CORE Then
    <Obsolete("użyj z nuget toasts")>
    Public Sub SetBadgeNo(iInt As Integer)
    End Sub

    <Obsolete("użyj z nuget toasts;Czy na pewno ma być GetSettingsString a nie GetLangString?")>
    Public Function ToastAction(sAType As String, sAct As String, sGuid As String, sContent As String) As String
        Dim sTmp As String = sContent
        If sTmp <> "" Then sTmp = vblib.GetSettingsString(sTmp, sTmp)
    End Function

    ''' <summary>
    ''' dwa kolejne teksty, sMsg oraz sMsg1
    ''' </summary>
    <Obsolete("użyj z nuget toasts")>
    Public Sub MakeToast(sMsg As String, Optional sMsg1 As String = "")
    End Sub
    <Obsolete("użyj z nuget toasts")>
    Public Sub MakeToast(oDate As DateTime, sMsg As String, Optional sMsg1 As String = "")
    End Sub

    <Obsolete("użyj z nuget toasts")>
    Public Sub RemoveScheduledToasts()
    End Sub

    <Obsolete("użyj z nuget toasts")>
    Public Sub RemoveCurrentToasts()
    End Sub

#End If

#End Region

#Region "WinVer, AppVer"


    ' <Obsolete("Jest w .Net Standard 2.0 (lib)")>
    Public Function GetAppVers() As String

#If NETFX_CORE Then
        Return Windows.ApplicationModel.Package.Current.Id.Version.Major & "." &
        Windows.ApplicationModel.Package.Current.Id.Version.Minor & "." &
        Windows.ApplicationModel.Package.Current.Id.Version.Build
#Else
        Return System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString()

#End If
    End Function

#If NETFX_CORE Then
    Public Function GetBuildTimestamp() As String
        Dim install_folder As String = Windows.ApplicationModel.Package.Current.InstalledLocation.Path
        Dim sManifestPath As String = Path.Combine(install_folder, "AppxManifest.xml")

        If File.Exists(sManifestPath) Then
            Return File.GetLastWriteTime(sManifestPath).ToString("yyyy.MM.dd HH:mm")
        End If

        Return ""
    End Function

    Public Function WinVer() As Integer
        'Unrecognized = 0,
        'Threshold1 = 1507,   // 10240
        'Threshold2 = 1511,   // 10586
        'Anniversary = 1607,  // 14393 Redstone 1
        'Creators = 1703,     // 15063 Redstone 2
        'FallCreators = 1709 // 16299 Redstone 3
        'April = 1803		// 17134
        'October = 1809		// 17763
        '? = 190?		// 18???

        'April  1803, 17134, RS5

        Dim u As ULong = ULong.Parse(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamilyVersion)
        u = (u And &HFFFF0000L) >> 16
        Return u
        'For i As Integer = 5 To 1 Step -1
        '    If Metadata.ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", i) Then Return i
        'Next

        'Return 0
    End Function

#End If

#End Region


#Region "RemoteSystem"

    ''' <summary>
    ''' jeśli na wejściu jest jakaś standardowa komenda, to na wyjściu będzie jej rezultat. Else = ""
    ''' </summary>
    Public Function AppServiceStdCmd(sCommand As String, sLocalCmds As String) As String
        Dim sTmp As String = vblib.LibAppServiceStdCmd(sCommand, sLocalCmds)
        If sTmp <> "" Then Return sTmp

        ' If sCommand.StartsWith("debug loglevel") Then - vbLib

        Select Case sCommand.ToLowerInvariant
            ' Case "ping" - vblib
            Case "ver"
                Return GetAppVers()
            Case "localdir"
                ' Case "appdir" - vblib
#If NETFX_CORE Then
                Return Windows.Storage.ApplicationData.Current.LocalFolder.Path
            Case "installeddate"
                Return Windows.ApplicationModel.Package.Current.InstalledDate.ToString("yyyy.MM.dd HH:mm:ss")
#Else
                Return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
#End If
            ' Case "help" - vblib
            ' Case "debug vars" - vblib
#If PK_USE_TOASTS Then
            Case "debug toasts"
                Return Toasts.DumpToasts()
#End If


#If NETFX_CORE Or WINDOWS8_0_OR_GREATER Then
            Case "debug memsize"
                Return Windows.System.MemoryManager.AppMemoryUsage.ToString() & "/" & Windows.System.MemoryManager.AppMemoryUsageLimit.ToString()
            Case "debug rungc"
                sTmp = "Memory usage before Global Collector call: " & Windows.System.MemoryManager.AppMemoryUsage.ToString() & vbCrLf
                GC.Collect()
                GC.WaitForPendingFinalizers()
                sTmp = sTmp & "After: " & Windows.System.MemoryManager.AppMemoryUsage.ToString() & "/" & Windows.System.MemoryManager.AppMemoryUsageLimit.ToString()
                Return sTmp
#End If
            ' Case "debug crashmsg"
            ' Case "debug crashmsg clear"

            Case "lib isfamilymobile"
                Return IsFamilyMobile().ToString()
            Case "lib isfamilydesktop"
                Return IsFamilyDesktop().ToString()
            Case "lib netisipavailable"
                Return NetIsIPavailable(False).ToString()
#If NETFX_CORE Then
            Case "lib netiscellinet"
                Return NetIsCellInet().ToString()
#End If
            Case "lib gethostname"
                Return GetHostName()
            Case "lib isthismoje"
                Return IsThisMoje().ToString()

                'Case "lib pkarmode 1"
                'Case "lib pkarmode 0"
                'Case "lib pkarmode"

#If PK_USE_TRIGGERS Then
            Case "debug triggers"
                Return UI.Triggers.DumpTriggers()
            Case "lib unregistertriggers"
                sTmp = UI.Triggers.DumpTriggers()
                UI.Triggers.UnregisterTriggers("") ' // całkiem wszystkie
                Return sTmp
            Case "lib istriggersregistered"
                Return UI.Triggers.IsTriggersRegistered().ToString()
#End If
        End Select

        Return ""  ' oznacza: to nie jest standardowa komenda
    End Function

#End Region



#Region "DataLog folder support"

#If PKAR_USEDATALOG Then

#If NETFX_CORE Then

    Private Async Function GetSDcardFolderAsync() As Task(Of Windows.Storage.StorageFolder)
        ' uwaga: musi być w Manifest RemoteStorage oraz fileext!

        Dim oRootDir As Windows.Storage.StorageFolder

        Try
            oRootDir = Windows.Storage.KnownFolders.RemovableDevices
        Catch ex As Exception
            Return Nothing ' brak uprawnień, może być także THROW
        End Try

        Try
            Dim oCards As IReadOnlyList(Of Windows.Storage.StorageFolder) = Await oRootDir.GetFoldersAsync()
            If oCards.Count < 1 Then Return Nothing
            Return oCards(0)
        Catch ex As Exception
            ' nie udało się folderu SD
        End Try

        Return Nothing


    End Function

    Public Async Function GetLogFolderRootAsync(Optional bUseOwnFolderIfNotSD As Boolean = True) As Task(Of Windows.Storage.StorageFolder)
#Disable Warning IDE0059 ' Unnecessary assignment of a value
        Dim oSdCard As Windows.Storage.StorageFolder = Nothing
#Enable Warning IDE0059 ' Unnecessary assignment of a value
        Dim oFold As Windows.Storage.StorageFolder

        If IsFamilyMobile() Then
            oSdCard = Await GetSDcardFolderAsync()

            If oSdCard IsNot Nothing Then
                oFold = Await oSdCard.CreateFolderAsync("DataLogs", Windows.Storage.CreationCollisionOption.OpenIfExists)
                If oFold Is Nothing Then Return Nothing

                Dim sAppName As String = Package.Current.DisplayName
                sAppName = sAppName.Replace(" ", "").Replace("'", "")

                oFold = Await oFold.CreateFolderAsync(sAppName, Windows.Storage.CreationCollisionOption.OpenIfExists)
                If oFold Is Nothing Then Return Nothing
            Else
                If Not bUseOwnFolderIfNotSD Then Return Nothing
                oSdCard = Windows.Storage.ApplicationData.Current.LocalFolder
                oFold = Await oSdCard.CreateFolderAsync("DataLogs", Windows.Storage.CreationCollisionOption.OpenIfExists)
                If oFold Is Nothing Then Return Nothing
            End If
        Else
            oSdCard = Windows.Storage.ApplicationData.Current.LocalFolder
            oFold = Await oSdCard.CreateFolderAsync("DataLogs", Windows.Storage.CreationCollisionOption.OpenIfExists)
            If oFold Is Nothing Then Return Nothing
        End If

        Return oFold
    End Function


    ''' <summary>
    ''' do wywolania raz, na poczatku - inicjalizacja zmiennych w VBlib (sciezki root)
    ''' </summary>
    Public Async Function InitDatalogFolder(Optional bUseOwnFolderIfNotSD As Boolean = True) As Task
        Dim oFold As Windows.Storage.StorageFolder = Await GetLogFolderRootAsync(bUseOwnFolderIfNotSD)
        If oFold Is Nothing Then Return
        VBlib.LibInitDataLog(oFold.Path)
    End Function

#Else
    ' datalog folder support, nie UWP

    ''' <summary>
    ''' do wywolania raz, na poczatku - inicjalizacja zmiennych w VBlib (sciezki root)
    ''' </summary>
    Public Async Function InitDatalogFolder(Optional bUseOwnFolderIfNotSD As Boolean = True) As Task
        Dim sPath As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        VBlib.LibInitDataLog(io.path.combine(sPath, "datalog")
    End Function

#End If

#End If

#End Region

    ''' <summary>
    ''' sprawdza czy to pełna wersja - gdyby próbować komercjalizować app
    ''' działa tylko na UWP-Release, w innych kompilacjach daje zawsze TRUE
    ''' </summary>
    ''' <returns></returns>
#Disable Warning BC42356 ' This async method lacks 'Await' operators and so will run synchronously
    Public Async Function IsFullVersionAsync() As Task(Of Boolean)
#Enable Warning BC42356 ' This async method lacks 'Await' operators and so will run synchronously

#If Not DEBUG And NETFX_CORE Then
        If IsThisMoje() Then Return True

        ' Windows.Services.Store.StoreContext: min 14393 (1607)
        Dim oLicencja = Await Windows.Services.Store.StoreContext.GetDefault().GetAppLicenseAsync()
        If Not oLicencja.IsActive Then Return False ' bez licencji? jakżeż to możliwe?

        If oLicencja.IsTrial Then Return False
#End If

        Return True

    End Function

#If Not NETFX_CORE Then

    ''' <summary>
    ''' set (or clear) HIDDEN attribute on given file
    ''' </summary>
    ''' <param name="filename"></param>
    ''' <param name="hidden"></param>
    Public Sub FileAttrHidden(filename As String, hide As Boolean)
        Dim attrs As IO.FileAttributes
        attrs = IO.File.GetAttributes(filename)

        If hide Then
            If attrs And IO.FileAttributes.Hidden Then Return
            attrs = attrs Or IO.FileAttributes.Hidden
        Else
            If Not (attrs And IO.FileAttributes.Hidden) Then Return
            attrs = attrs And &HFFFE
        End If

        IO.File.SetAttributes(filename, attrs)
    End Sub
#End If

End Module


#Region ".Net configuration - UWP settings"

#If NETFX_CORE Then

Public Class UwpConfigurationProvider
    ' Inherits MsExtConfig.ConfigurationProvider
    Implements MsExtConfig.IConfigurationProvider

    Private ReadOnly _roamPrefix1 As String = Nothing
    Private ReadOnly _roamPrefix2 As String = Nothing

    ''' <summary>
    ''' Create Configuration Provider, for LocalSettings and RoamSettings
    ''' </summary>
    ''' <param name="sRoamPrefix1">prefix for RoamSettings, use NULL if want only LocalSettings</param>
    ''' <param name="sRoamPrefix2">prefix for RoamSettings, use NULL if want only LocalSettings</param>
    Public Sub New(Optional sRoamPrefix1 As String = "[ROAM]", Optional sRoamPrefix2 As String = Nothing)
        Data = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        _roamPrefix1 = sRoamPrefix1
        _roamPrefix2 = sRoamPrefix2
    End Sub

    Private Sub LoadData(settSource As IPropertySet)
        For Each oItem In settSource
            Data(oItem.Key) = oItem.Value
        Next
    End Sub

    ''' <summary>
    ''' read current state of settings (all values); although it is not used in TryGet, but we should have Data property set for other reasons (e.g. for listing all variables)...
    ''' </summary>
    Public Sub Load() Implements MsExtConfig.IConfigurationProvider.Load
        LoadData(WinAppData.Current.RoamingSettings.Values)
        LoadData(WinAppData.Current.LocalSettings.Values)
    End Sub


    ''' <summary>
    ''' always set LocalSettings, and if value is prefixed with Roam prefix, also RoamSettings (prefix is stripped)
    ''' </summary>
    ''' <param name="key"></param>
    ''' <param name="value"></param>
    Public Sub [Set](key As String, value As String) Implements MsExtConfig.IConfigurationProvider.Set
        If value Is Nothing Then value = ""

        If _roamPrefix1 IsNot Nothing AndAlso value.ToUpperInvariant().StartsWith(_roamPrefix1, StringComparison.Ordinal) Then
            value = value.Substring(_roamPrefix1.Length)
            Try
                WinAppData.Current.RoamingSettings.Values(key) = value
            Catch
                ' probably length is too big
            End Try
        End If

        If _roamPrefix2 IsNot Nothing AndAlso value.ToUpperInvariant().StartsWith(_roamPrefix2, StringComparison.Ordinal) Then
            value = value.Substring(_roamPrefix2.Length)
            Try
                WinAppData.Current.RoamingSettings.Values(key) = value
            Catch
                ' probably length is too big
            End Try
        End If

        Data(key) = value
        Try
            WinAppData.Current.LocalSettings.Values(key) = value
        Catch
            ' probably length is too big
        End Try

    End Sub

    ''' <summary>
    ''' this is used only for iterating keys, not for Get/Set
    ''' </summary>
    ''' <returns></returns>
    Protected Property Data As IDictionary(Of String, String)

    ''' <summary>
    ''' gets current Value of Key; local value overrides roaming value
    ''' </summary>
    ''' <returns>True if Key is found (and Value is set)</returns>
    Public Function TryGet(key As String, ByRef value As String) As Boolean Implements MsExtConfig.IConfigurationProvider.TryGet

        Dim bFound As Boolean = False

        If WinAppData.Current.RoamingSettings.Values.ContainsKey(key) Then
            value = WinAppData.Current.RoamingSettings.Values(key).ToString
            bFound = True
        End If

        If WinAppData.Current.LocalSettings.Values.ContainsKey(key) Then
            value = WinAppData.Current.LocalSettings.Values(key).ToString
            bFound = True
        End If

        Return bFound

    End Function

    Public Function GetReloadToken() As MsExtPrim.IChangeToken Implements MsExtConfig.IConfigurationProvider.GetReloadToken
        Return New MsExtConfig.ConfigurationReloadToken
    End Function

    Public Function GetChildKeys(earlierKeys As IEnumerable(Of String), parentPath As String) As IEnumerable(Of String) Implements MsExtConfig.IConfigurationProvider.GetChildKeys
        ' in this configuration, we don't have structure - so just return list

        Dim results As New List(Of String)
        For Each kv As KeyValuePair(Of String, String) In Data
            results.Add(kv.Key)
        Next

        results.Sort()

        Return results
    End Function

End Class

Public Class UwpConfigurationSource
    Implements MsExtConfig.IConfigurationSource

    Private ReadOnly _roamPrefix1 As String = Nothing
    Private ReadOnly _roamPrefix2 As String = Nothing

    Public Function Build(builder As MsExtConfig.IConfigurationBuilder) As MsExtConfig.IConfigurationProvider Implements MsExtConfig.IConfigurationSource.Build
        Return New UwpConfigurationProvider(_roamPrefix1, _roamPrefix2)
    End Function

    Public Sub New(Optional sRoamPrefix1 As String = "[ROAM]", Optional sRoamPrefix2 As String = Nothing)
        _roamPrefix1 = sRoamPrefix1
        _roamPrefix2 = sRoamPrefix2
    End Sub
End Class

Partial Module Extensions
    <Runtime.CompilerServices.Extension()>
    Public Function AddUwpSettings(ByVal configurationBuilder As MsExtConfig.IConfigurationBuilder, Optional sRoamPrefix1 As String = "[ROAM]", Optional sRoamPrefix2 As String = Nothing) As MsExtConfig.IConfigurationBuilder
        configurationBuilder.Add(New UwpConfigurationSource(sRoamPrefix1, sRoamPrefix2))
        Return configurationBuilder
    End Function
End Module


#End If

#End Region

Partial Public Module Extensions

#If PK_WPF Or NETFX_CORE Then
    ''' <summary>
    ''' ustaw wskoki z vblib dla danej strony; dla UWP oraz WPF niepotrzebne (o ile użyto InitLib)
    ''' </summary>
    <Extension>
    Public Sub InitDialogs(element As FrameworkElement)
#Else
    ''' <summary>
    ''' ustaw wskoki z vblib dla danej strony; dla UWP oraz WPF niepotrzebne
    ''' </summary>
    <Extension>
    <Obsolete("ale tu coś nie do końca działa, do sprawdzenia!")>
    Public Sub InitDialogs(element As FrameworkElement)
#End If

#If Not NETFX_CORE And Not PK_WPF Then
        ' ta linijka NIE DZIAŁA przed Win 10.0.18362 !
        ' ona jest tylko dla WinUI3 chyba
        _xamlRoot = element.XamlRoot
#End If
        vblib.LibInitDialogBox(AddressOf FromLibDialogBoxAsync, AddressOf FromLibDialogBoxYNAsync, AddressOf FromLibDialogBoxInputAllDirectAsync)
    End Sub

#If Not NETFX_CORE And Not PK_WPF Then
    Public _xamlRoot As XamlRoot

#End If
End Module

#Region "Konwertery Bindings XAML"
' nie mogą być w VBlib, bo Implements Microsoft.UI.Xaml.Data.IValueConverter

#Region "to co dla innych UI może być w Nuget, a w UWP być nie może"
#If NETFX_CORE Then

' WinRT nie może mieć "mustoverride"

Public MustInherit Class ValueConverterOneWay
    Implements IValueConverter

    Public MustOverride Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class

''' <summary>
''' this class should be used to define your own ValueConverters; but it frees you from writing ConvertBack method, and simplyfies Convert method
''' </summary>
Public MustInherit Class ValueConverterOneWaySimple
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert
        Return Convert(value)
    End Function

    Protected MustOverride Function Convert(value As Object) As Object


    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class

Public MustInherit Class ValueConverterOneWayWithPar
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert
        Dim param As String = ""

        If parameter IsNot Nothing Then param = CType(parameter, String)

        Return Convert(value, param)
    End Function

    Protected MustOverride Function Convert(value As Object, param As String) As Object

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class


#End If

#End Region


' parameter = NEG robi negację
Public Class KonwersjaVisibility
    Inherits ValueConverterOneWayWithPar

    Protected Overrides Function Convert(value As Object, param As String) As Object

        Dim bTemp As Boolean = CType(value, Boolean)
        If param.EqualsCI("NEG") Then bTemp = Not bTemp

        Return If(bTemp, Visibility.Visible, Visibility.Collapsed)
    End Function

End Class

' ULONG to String
Public Class KonwersjaMAC
    Inherits ValueConverterOneWaySimple

    ' Define the Convert method to change a DateTime object to
    ' a month string.
    Protected Overrides Function Convert(ByVal value As Object) As Object

        ' value is the data from the source object.

        Dim uMAC As ULong = CType(value, ULong)
        If uMAC = 0 Then Return ""

        Return uMAC.ToHexBytesString()

    End Function

End Class

''' <summary>
''' konwersja ToString, ale używając parametru wymuszającego FORMAT (np. %2d)
''' </summary>

Public Class KonwersjaVal2StringFormat
    Inherits ValueConverterOneWayWithPar

    Protected Overrides Function Convert(value As Object, sFormat As String) As Object

        If value.GetType Is GetType(Integer) Then
            Dim temp = CType(value, Integer)
            Return If(sFormat = "", temp.ToString, temp.ToString(sFormat))
        End If

        If value.GetType Is GetType(Long) Then
            Dim temp = CType(value, Long)
            Return If(sFormat = "", temp.ToString, temp.ToString(sFormat))
        End If

        If value.GetType Is GetType(Double) Then
            Dim temp = CType(value, Double)
            Return If(sFormat = "", temp.ToString, temp.ToString(sFormat))
        End If

        If value.GetType Is GetType(String) Then
            Dim temp = CType(value, String)
            Return If(sFormat = "", temp.ToString, String.Format(sFormat, temp))
        End If

        Return "???"

    End Function

End Class

Public Class KonwersjaDaty
    Inherits ValueConverterOneWaySimple

    Protected Overrides Function Convert(value As Object) As Object
        Dim temp As DateTime = CType(value, DateTime)

        If temp.Year < 1000 Then Return "--"
        If temp.Year > 2100 Then Return "--"

        Return temp.ToString("yyyy-MM-dd")
    End Function

End Class

#End Region
