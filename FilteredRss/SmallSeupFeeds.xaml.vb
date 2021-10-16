
Public NotInheritable Class SmallSeupFeeds
    Inherits Page

    Private Sub bSetupOk_Click(sender As Object, e As RoutedEventArgs)
        If uiFeeds.Text = "mama.jaga.init" Then uiFeeds.Text = "https://twitter.com/donaldtusk" & vbCrLf & "https://www.facebook.com/Roman-Giertych-strona-oficjalna-215392231834473"
        SetSettingsString("KnownFeeds", uiFeeds.Text, uiFeedsRoam.IsOn)
        Me.Frame.GoBack()
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        uiFeeds.Text = GetSettingsString("KnownFeeds", "")
    End Sub
End Class
