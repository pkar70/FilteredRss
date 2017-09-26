'Set objFSO = CreateObject("Scripting.FileSystemObject")
'Set objFile = objFSO.OpenTextFile("rss - xvidy.txt", 1)	' forRead

'sTxt = objFile.ReadAll


 Dim oXml
 Set oXml = CreateObject("Microsoft.XMLDOM")

 oXml.Load("rss.xml")

 wscript.echo "after load"
