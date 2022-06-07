
' ponieważ NIE DZIAŁA pod Uno.Android wczytywanie pliku (apk nie jest rozpakowany?),
' to w ten sposób przekazywanie zawartości pliku INI
' wychodzi na to samo, edycja pliku defaults.ini albo defsIni.lib.vb

Public Class IniLikeDefaults

    Public Const sIniContent As String = "
[main]
TimerInterval=15
MaxDays=30

[debug]
key=value # remark

[app]
; lista z app (bez ustawiania)
DebugOutData=
autoRead=false
lastRead=Date.ToString
;resDelete
;resOpen
;resBrowser
LinksActive=false
NotifyWhite=false
' TIME & sGuidsValueName=sDate
iLastRssGuid=
sGuidsValueName=
BlackList=  ' global
WhiteList=  ' global
sLastId=
ChangedXML=false
; KnownFeeds=   ' poprzednia wersja

[libs]
; lista z pkarmodule
remoteSystemDisabled=false
appFailData=
offline=false
lastPolnocnyTry=
lastPolnocnyOk=

"

End Class
