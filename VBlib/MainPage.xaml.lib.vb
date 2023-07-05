
' 2022.02.04 migracja z UWP.VB
' tu historia zawiera tylko zmiany po tej dacie


Public Class MainPage

    Public Delegate Function UsunToast(sGuid As String) As Boolean
    Private oUsunToast As UsunToast

    Public Sub New(oMetoda As UsunToast)
        oUsunToast = oMetoda
    End Sub


    Public Async Function DeleteFromContextMenu(oItem As JedenItem, iMode As Integer) As Task

        DumpCurrMethod()
        If oItem Is Nothing Then Return

        Select Case iMode
            Case 1  ' jeden post
                App.glItems.Remove(oItem)
            Case 2  ' wedle tematu - ask!
                Dim sSubj As String = Await DialogBoxInputResAsync("msgSubjectToRemove", oItem.sTitle)
                If sSubj = "" Then Return

                For iLoop As Integer = App.glItems.Count - 1 To 0 Step -1
                    If App.glItems.Item(iLoop).sTitle.Contains(sSubj) Then
                        oUsunToast(App.glItems.Item(iLoop).sGuid)
                        App.glItems.RemoveAt(iLoop)
                    End If
                Next

            Case 3  ' all from feed - askYN
                If Not Await DialogBoxResYNAsync("msgDelAllFromThisFeed") Then Return
                For iLoop As Integer = App.glItems.Count - 1 To 0 Step -1
                    If oItem.sFeedName = App.glItems.Item(iLoop).sFeedName Then
                        oUsunToast(App.glItems.Item(iLoop).sGuid)
                        App.glItems.RemoveAt(iLoop)
                    End If
                Next
            Case 4  ' kill file
                Dim sKill As String = Await DialogBoxInputDirectAsync(GetLangString("msgKillFile14days"), oItem.sTitle)
                If String.IsNullOrEmpty(sKill) Then Return
                For iLoop As Integer = App.glItems.Count - 1 To 0 Step -1
                    If App.RegExpOrInstr(App.glItems.Item(iLoop).sTitle, sKill) Then
                        oUsunToast(App.glItems.Item(iLoop).sGuid)
                        App.glItems.RemoveAt(iLoop)
                    End If
                Next
                App.KillFileAddEntry(sKill)

            Case 5  ' usunięcie do tego (wszystkie poprzednie)

                Dim lista As List(Of JedenItem)
                Select Case GetSettingsInt("uiSortOrder")
                    Case 1  ' by name
                        lista = (From c In VBlib.App.glItems Order By c.sTitle).ToList
                    Case 2  ' by thread
                        lista = (From c In VBlib.App.glItems Order By c.sFeedName, c.sTitle).ToList
                    Case Else ' 0, i nieznane: jak dotychczas
                        lista = (From c In VBlib.App.glItems).ToList
                End Select

                Dim listaToDel As New List(Of JedenItem)
                For iLp As Integer = 0 To lista.Count
                    If lista(iLp).sGuid = oItem.sGuid Then Exit For
                    listaToDel.Add(lista(iLp))
                Next

                For Each oDelItem As JedenItem In listaToDel
                    App.glItems.Remove(oDelItem)
                Next

            Case Else
                Return
        End Select

    End Function


End Class
