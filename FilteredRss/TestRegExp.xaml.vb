' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class TestRegExp
    Inherits Page

    Dim moDefBrush, moBrushR, moBrushG As Brush
    Dim moDefThick, moThickSel As Thickness

    Private Sub uiOk_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.GoBack()
    End Sub


    Private Sub uiTb1_TextChange(sender As Object, e As TextChangedEventArgs) Handles uiTRtext1.TextChanged
        If App.RegExp(uiTRtext1.Text, uiTRregExp.Text) = 1 Then
            uiTRtext1.BorderThickness = moThickSel
            uiTRtext1.BorderBrush = moBrushG
        Else
            uiTRtext1.BorderThickness = moDefThick
            uiTRtext1.BorderBrush = moDefBrush
        End If

    End Sub

    Private Sub uiTb2_TextChange(sender As Object, e As TextChangedEventArgs) Handles uiTRtext2.TextChanged
        If App.RegExp(uiTRtext2.Text, uiTRregExp.Text) = 1 Then
            uiTRtext2.BorderThickness = moThickSel
            uiTRtext2.BorderBrush = moBrushG
        Else
            uiTRtext2.BorderThickness = moDefThick
            uiTRtext2.BorderBrush = moDefBrush
        End If

    End Sub

    Private Sub uiTb3_TextChange(sender As Object, e As TextChangedEventArgs) Handles uiTRtext3.TextChanged
        If App.RegExp(uiTRtext3.Text, uiTRregExp.Text) = 1 Then
            uiTRtext3.BorderThickness = moThickSel
            uiTRtext3.BorderBrush = moBrushG
        Else
            uiTRtext3.BorderThickness = moDefThick
            uiTRtext3.BorderBrush = moDefBrush
        End If
    End Sub

    Private Sub uiTR_TextChange(sender As Object, e As TextChangedEventArgs) Handles uiTRregExp.TextChanged

        If uiTRregExp.Text.Length < 1 Then Exit Sub

        If App.RegExp("alamakota", uiTRregExp.Text) > 1 Then
            uiTRregExp.BorderThickness = moThickSel
            uiTRregExp.BorderBrush = moBrushR
            Exit Sub
        End If

        uiTRregExp.BorderThickness = moDefThick
        uiTRregExp.BorderBrush = moDefBrush
        uiTb1_TextChange(sender, e)
        uiTb2_TextChange(sender, e)
        uiTb3_TextChange(sender, e)

    End Sub

    Private Sub uiGrid_Loaded(sender As Object, e As RoutedEventArgs)
        moDefThick = uiTRregExp.BorderThickness
        moThickSel = New Thickness(4)
        moDefBrush = uiTRregExp.BorderBrush
        moBrushR = New SolidColorBrush(Windows.UI.Colors.Red)
        moBrushG = New SolidColorBrush(Windows.UI.Colors.Green)
    End Sub
End Class
