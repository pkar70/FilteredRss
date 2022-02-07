

Partial Public Class Feeds


    Private Const msFileName As String = "feeds.json"
    Public Shared glFeeds As New List(Of JedenFeed)

    Public Shared Function FeedsImportOld() As Boolean

        ' struktura ściągnięta z App:ReadFeed
        Dim aFeeds() As String
        Try
            aFeeds = GetSettingsString("KnownFeeds").Split(vbCrLf)
        Catch ex As Exception
            DebugOut("zapewne nie ma InitSettings, więc nie mogę zrobić importu ze zmiennej (migracja do JSON była w 2021.12.15)")
            Return False
        End Try

        Dim iCnt As Integer = 0
        For Each sFeed As String In aFeeds
            ' bez pustych linii
            If sFeed.Length > 10 AndAlso sFeed.Substring(0, 1) <> ";" Then
                Dim oNew As New VBlib.JedenFeed
                oNew.sUri = sFeed
                Dim sTmp As String = GetSettingsString("FeedName_" & App.Url2VarName(sFeed))
                If sTmp <> "" Then
                    oNew.sName = sTmp
                    oNew.iNameType = FeedNameType.UserDefined
                Else
                    oNew.sName = sFeed
                    Dim iInd As Integer = oNew.sName.LastIndexOf("/")
                    If iInd > 0 Then oNew.sName = oNew.sName.Substring(iInd + 1)
                    oNew.iNameType = FeedNameType.FromUri
                End If
                glFeeds.Add(oNew)
                iCnt += 1
            End If
        Next

        Return iCnt > 0
    End Function

    Public Shared Function FeedsLoad(sDirectory1 As String, sDirectory2 As String, Optional bForce As Boolean = False) As Boolean

        If Not bForce Then
            If glFeeds.Count > 0 Then Return True
        End If

        Dim sFilePathname As String = IO.Path.Combine(sDirectory1, msFileName)
        If Not IO.File.Exists(sFilePathname) Then
            sFilePathname = IO.Path.Combine(sDirectory2, msFileName)
            If Not IO.File.Exists(sFilePathname) Then
                Return FeedsImportOld()
            End If
        End If

        Dim sTxt As String = IO.File.ReadAllText(sFilePathname)

        Try
            glFeeds = Newtonsoft.Json.JsonConvert.DeserializeObject(sTxt, GetType(List(Of VBlib.JedenFeed)))
        Catch ex As Exception
            Return False
        End Try

        Return glFeeds.Count > 0

    End Function

    Public Shared Sub FeedsSave(sDirectory As String)
        Dim sFilePathname As String = IO.Path.Combine(sDirectory, msFileName)

        Dim sTxt As String = Newtonsoft.Json.JsonConvert.SerializeObject(glFeeds, Newtonsoft.Json.Formatting.Indented)
        IO.File.WriteAllText(sFilePathname, sTxt)
    End Sub

    Public Shared Function FeedsCreateNew(sUri As String, Optional sName As String = "") As VBlib.JedenFeed

        For Each oItem As VBlib.JedenFeed In glFeeds
            If oItem.sUri = sUri Then Return Nothing ' already exist
        Next

        Dim oNew As New VBlib.JedenFeed
        oNew.sUri = sUri

        If sName <> "" Then
            oNew.sName = sName
            oNew.iNameType = FeedNameType.UserDefined
        Else
            oNew.sName = sUri
            Dim iInd As Integer = oNew.sName.LastIndexOf("/")
            If iInd > 0 Then oNew.sName = oNew.sName.Substring(iInd + 1)
            oNew.iNameType = FeedNameType.FromUri
        End If
        oNew.sName2 = oNew.sName

        Return oNew
    End Function

    Public Shared Sub FeedsTryAddNew(sUri As String, Optional sName As String = "")
        Dim oNew As VBlib.JedenFeed = FeedsCreateNew(sUri, sName)
        If oNew IsNot Nothing Then glFeeds.Add(oNew)
    End Sub

End Class

' uwaga: sprawdź Setup:InitListaTypowToast, oraz resources.resw dla nazw
Public Enum FeedToastType
    NoToast
    NewExist
    ListItems
    Separate
End Enum

Public Enum FeedNameType
    FromUri = 0
    FromFeed = 1
    UserDefined = 2
End Enum

Public Class JedenFeed
    Public Property sName As String
    Public Property sUri As String
    Public Property iToastType As Integer = FeedToastType.Separate
    Public Property sBlacklist As String = "" ' niezalezne od Global
    Public Property sWhitelist As String = ""
    Public Property iNameType As FeedNameType
    Public Property iLinksActive As Integer = -1

    Public Property bLastError As Boolean = False
    Public Property sLastOkDate As String = Nothing
    Public Property sLastGuid As String
    Public Property iNotifyWhite As Integer = -1 ' -1 nieustawiony, 0/1 wartość
    Public Property sLastGuids As String    ' tego będzie duzo!
    Public Property iMaxDays As Integer = -1

    <Newtonsoft.Json.JsonIgnoreAttribute>
    Public Property sName2 As String
    <Newtonsoft.Json.JsonIgnoreAttribute>
    Public Property sToastType As String = ""
    '<Newtonsoft.Json.JsonIgnoreAttribute>
    'Public Property iToastCnt As Integer
    '<Newtonsoft.Json.JsonIgnoreAttribute>
    'Public Property sToastString As String = ""

    ' dla przekazywania Settings do VBlib
    <Newtonsoft.Json.JsonIgnoreAttribute>
    Public Property sGlobalBlack As String
    <Newtonsoft.Json.JsonIgnoreAttribute>
    Public Property sGlobalWhite As String
End Class

