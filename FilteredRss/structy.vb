﻿' entry_data / ProfilePage / graphql / user / full_name
'"full_name":" .... "
' entry_data / ProfilePage / graphql / user / profile_pic_url
' "profile_pic_url":"..."
' entry_data / ProfilePage / graphql / user / edge_owner_to_timeline_media / edges [ / node

Public Class JSONinstagram
    Public Property entry_data As JSONinstaEntryData
End Class

Public Class JSONinstaEntryData
    Public Property ProfilePage As List(Of JSONinstaProfileItem)
End Class
Public Class JSONinstaProfileItem
    Public Property graphql As JSONinstaGraph
End Class
Public Class JSONinstaGraph
        ' full implementation
        Public Property user As JSONinstaUser
    End Class

Public Class JSONinstaUser
    ' część danych tylko
    Public Property full_name As String
    Public Property profile_pic_url As String
    Public Property edge_owner_to_timeline_media As JSONinstaTimeline
End Class
Public Class JSONinstaTimeline
    ' część danych tylko
    Public Property count As Integer
    Public Property edges As List(Of JSONinstaPicEdge)
End Class
Public Class JSONinstaPicEdge
    ' kompletna
    Public Property node As JSONinstaPicNode
End Class
Public Class JSONinstaPicNode
    ' część danych tylko
    Public Property id As String
    Public Property display_url As String
    Public Property accessibility_caption As String
    Public Property location As JSONinstaLocation
    Public Property owner As JSONinstaOwner
    Public Property thumbnail_src As String
    Public Property taken_at_timestamp As Integer
    ' display_url
    ' accessibility_caption
    ' edge_media_to_caption / edges [ node / text
    ' location / name   - czyli gdzie zrobiona fotka
    ' thumbnail_src
End Class

Public Class JSONinstaOwner
    Public Property id As String
    Public Property username As String
End Class
Public Class JSONinstaLocation
    ' full implementation
    Public Property id As String
    Public Property name As String
    Public Property slug As String
    Public Property has_public_page As Boolean
End Class
