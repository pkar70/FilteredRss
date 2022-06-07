

Public NotInheritable Class Info
    Inherits Page

    Private Sub bRateIt_Click(sender As Object, e As RoutedEventArgs)
        App.OpenRateIt()
    End Sub

    Private Sub bAboutOk_Click(sender As Object, e As RoutedEventArgs)
        Me.GoBack
    End Sub

    Private Sub OnLoaded(sender As Object, e As RoutedEventArgs) Handles uiInfoGrid.Loaded
        Dim sFilename As String
        sFilename = "Assets\Guide-" & Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName & ".htm"
        If Not File.Exists(sFilename) Then sFilename = "Assets\Guide-En.htm"

        ' błąd, albo np. jesteśmy pod Android :)
        If Not File.Exists(sFilename) Then Return

        uiWebInfo.NavigateToString(File.ReadAllText(sFilename))
    End Sub
End Class
