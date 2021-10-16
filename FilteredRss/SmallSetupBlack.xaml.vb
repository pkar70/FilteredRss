
Public NotInheritable Class SmallSetupBlack
    Inherits Page

    Private Sub bRegExpTest_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(TestRegExp))
    End Sub

    Private Sub bSetupOk_Click(sender As Object, e As RoutedEventArgs)
        SetSettingsString("BlackList", uiBlacklist.Text, uiBlackRoam.IsOn)
        Me.Frame.GoBack()
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        uiBlacklist.Text = GetSettingsString("BlackList")
    End Sub
End Class
