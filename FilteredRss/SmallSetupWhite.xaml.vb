
Public NotInheritable Class SmallSetupWhite
    Inherits Page
    Private Sub bRegExpTest_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(TestRegExp))
    End Sub

    Private Sub bSetupOk_Click(sender As Object, e As RoutedEventArgs)
        SetSettingsString("WhiteList", uiWhitelist.Text, uiWhiteRoam.IsOn)
        Me.Frame.GoBack()
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        uiWhitelist.Text = GetSettingsString("WhiteList")
    End Sub
End Class
