' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

Imports Windows.System
''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class Info
    Inherits Page

    Private Async Sub bRateIt_Click(sender As Object, e As RoutedEventArgs)

        Dim sUri As New Uri("ms-windows-store://review/?PFN=" & Package.Current.Id.FamilyName)
        Await Launcher.LaunchUriAsync(sUri)

    End Sub

    Private Sub bAboutOk_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(MainPage))
    End Sub

    Private Sub OnSizeChanged(sender As Object, e As SizeChangedEventArgs)
        'uiWebInfo z uiInfoGrid
        uiWebInfo.Height = uiInfoGrid.ActualHeight - 40
        uiWebInfo.Width = uiInfoGrid.ActualWidth - 40
    End Sub

    Private Sub OnLoaded(sender As Object, e As RoutedEventArgs) Handles uiInfoGrid.Loaded
        Dim oFile As StreamReader
        Dim sTxt As String
        sTxt = "Assets\Guide-" & Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName & ".htm"
        If Not File.Exists(sTxt) Then sTxt = "Assets\Guide-En.htm"

        oFile = File.OpenText(sTxt)
        sTxt = ""
        While Not oFile.EndOfStream
            sTxt = sTxt & oFile.ReadLine
        End While
        oFile.Dispose()
        uiWebInfo.NavigateToString(sTxt)
    End Sub
End Class
