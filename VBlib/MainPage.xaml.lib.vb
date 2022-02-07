﻿
' 2022.02.04 migracja z UWP.VB
' tu historia zawiera tylko zmiany po tej dacie


Public Class MainPage


    Public Shared Async Function DeleteFromContextMenu(oItem As JedenItem, iMode As Integer) As Task
        DumpCurrMethod()
        If oItem Is Nothing Then Return

        Select Case iMode
            Case 1  ' jeden post
                App.glItems.Remove(oItem)
            Case 2  ' wedle tematu - ask!
                Dim sSubj As String = Await DialogBoxInputResAsync("msgSubjectToRemove", oItem.sTitle)
                If sSubj = "" Then Return

                For iLoop As Integer = App.glItems.Count - 1 To 0 Step -1
                    If App.glItems.Item(iLoop).sTitle.Contains(sSubj) Then App.glItems.RemoveAt(iLoop)
                Next

            Case 3  ' all from feed - askYN
                If Not Await DialogBoxResYNAsync("msgDelAllFromThisFeed") Then Return
                For iLoop As Integer = App.glItems.Count - 1 To 0 Step -1
                    If oItem.sFeedName = App.glItems.Item(iLoop).sFeedName Then App.glItems.RemoveAt(iLoop)
                Next
            Case 4  ' kill file
                Dim sKill As String = Await DialogBoxInputDirectAsync(GetLangString("msgKillFile14days"), oItem.sTitle)
                If String.IsNullOrEmpty(sKill) Then Return
                For iLoop As Integer = App.glItems.Count - 1 To 0 Step -1
                    If App.RegExpOrInstr(App.glItems.Item(iLoop).sTitle, sKill) Then App.glItems.RemoveAt(iLoop)
                Next
                App.KillFileAddEntry(sKill)

            Case Else
                Return
        End Select

    End Function


End Class
