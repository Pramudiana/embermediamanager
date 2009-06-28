﻿' ################################################################################
' #                             EMBER MEDIA MANAGER                              #
' ################################################################################
' ################################################################################
' # This file is part of Ember Media Manager.                                    #
' #                                                                              #
' # Ember Media Manager is free software: you can redistribute it and/or modify  #
' # it under the terms of the GNU General Public License as published by         #
' # the Free Software Foundation, either version 3 of the License, or            #
' # (at your option) any later version.                                          #
' #                                                                              #
' # Ember Media Manager is distributed in the hope that it will be useful,       #
' # but WITHOUT ANY WARRANTY; without even the implied warranty of               #
' # MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the                #
' # GNU General Public License for more details.                                 #
' #                                                                              #
' # You should have received a copy of the GNU General Public License            #
' # along with Ember Media Manager.  If not, see <http://www.gnu.org/licenses/>. #
' ################################################################################

Option Explicit On

Imports System
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Security.Cryptography
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Net
Imports System.Globalization
Imports Microsoft.Win32

Public Class Master

    'Global Variables
    Public Shared eSettings As New emmSettings
    Public Shared MediaList As New List(Of FileAndSource)
    Public Shared eLog As New ErrorLogger
    Public Shared SQLcn As New SQLite.SQLiteConnection()
    Public Shared DefaultOptions As New ScrapeOptions
    Public Shared alMoviePaths As New ArrayList

    Public Shared TempPath As String = Path.Combine(Application.StartupPath, "Temp")

    'need to get to the point where all of these can be removed (due to loading from db)
    Public Shared currNFO As String = String.Empty
    Public Shared currPath As String = String.Empty
    Public Shared currSingle As Boolean = False
    Public Shared currMovie As New Media.Movie
    Public Shared tmpMovie As New Media.Movie
    Public Shared scrapeMovie As New Media.Movie

    'Global Enums
    Public Enum PosterSize As Integer
        Xlrg = 0
        Lrg = 1
        Mid = 2
        Small = 3
        Wide = 4
    End Enum

    Public Enum FanartSize As Integer
        Lrg = 0
        Mid = 1
        Small = 3
    End Enum

    Public Enum ScrapeType As Integer
        SingleScrape = 0
        FullAuto = 1
        FullAsk = 2
        UpdateAuto = 3
        UpdateAsk = 4
        CleanFolders = 6
        NewAuto = 7
        NewAsk = 8
        MarkAuto = 9
        MarkAsk = 10
        CopyBD = 11
        RevertStudios = 12
    End Enum

    Public Enum ImageType As Integer
        Posters = 0
        Fanart = 1
    End Enum

    Public Enum TrailerPages As Integer
        YouTube = 0
        Imdb = 1
    End Enum

    Public Enum ScrapeModifier As Integer
        All = 0
        NFO = 1
        Poster = 2
        Fanart = 3
        Extra = 4
        Trailer = 5
        MI = 6
    End Enum
    Public Structure DBMovie
        Dim ID As Integer
        Dim Movie As Media.Movie
        Dim FaS As FileAndSource
        Dim IsNew As Boolean
        Dim IsMark As Boolean
        Dim IsLock As Boolean
    End Structure


    Public Structure ScrapeOptions
        Dim bTitle As Boolean
        Dim bYear As Boolean
        Dim bMPAA As Boolean
        Dim bRelease As Boolean
        Dim bRating As Boolean
        Dim bTrailer As Boolean
        Dim bVotes As Boolean
        Dim bCast As Boolean
        Dim bTagline As Boolean
        Dim bDirector As Boolean
        Dim bGenre As Boolean
        Dim bOutline As Boolean
        Dim bPlot As Boolean
        Dim bRuntime As Boolean
        Dim bStudio As Boolean
        Dim bWriters As Boolean
        Dim bProducers As Boolean
        Dim bMusicBy As Boolean
        Dim bOtherCrew As Boolean
    End Structure

    Public Structure CustomUpdaterStruct
        Dim Canceled As Boolean
        Dim ScrapeType As ScrapeType
        Dim Modifier As ScrapeModifier
        Dim Options As ScrapeOptions
    End Structure

    Public Class FileAndSource
        Dim _filename As String
        Dim _source As String
        Dim _single As Boolean
        Dim _usefolder As Boolean
        Dim _poster As String
        Dim _fanart As String
        Dim _nfo As String
        Dim _extra As String
        Dim _trailer As String
        Dim _subs As String

        Public Property Filename() As String
            Get
                Return _filename
            End Get
            Set(ByVal value As String)
                _filename = value
            End Set
        End Property

        Public Property Source() As String
            Get
                Return _source
            End Get
            Set(ByVal value As String)
                _source = value
            End Set
        End Property

        Public Property isSingle() As Boolean
            Get
                Return _single
            End Get
            Set(ByVal value As Boolean)
                _single = value
            End Set
        End Property

        Public Property UseFolder() As Boolean
            Get
                Return _usefolder
            End Get
            Set(ByVal value As Boolean)
                _usefolder = value
            End Set
        End Property

        Public Property Poster() As String
            Get
                Return _poster
            End Get
            Set(ByVal value As String)
                _poster = value
            End Set
        End Property

        Public Property Fanart() As String
            Get
                Return _fanart
            End Get
            Set(ByVal value As String)
                _fanart = value
            End Set
        End Property

        Public Property Nfo() As String
            Get
                Return _nfo
            End Get
            Set(ByVal value As String)
                _nfo = value
            End Set
        End Property

        Public Property Extra() As String
            Get
                Return _extra
            End Get
            Set(ByVal value As String)
                _extra = value
            End Set
        End Property

        Public Property Trailer() As String
            Get
                Return _trailer
            End Get
            Set(ByVal value As String)
                _trailer = value
            End Set
        End Property

        Public Property Subs() As String
            Get
                Return _subs
            End Get
            Set(ByVal value As String)
                _subs = value
            End Set
        End Property

        Public Sub New()
            Clear()
        End Sub

        Public Sub Clear()
            _filename = String.Empty
            _source = String.Empty
            _usefolder = False
            _poster = String.Empty
            _fanart = String.Empty
            _nfo = String.Empty
            _extra = String.Empty
            _trailer = String.Empty
            _subs = String.Empty
        End Sub
    End Class

    Public Class NonConf
        Private _imdbid As String
        Private _text As String

        Public Property IMDBID() As String
            Get
                Return Me._imdbid
            End Get
            Set(ByVal value As String)
                Me._imdbid = value
            End Set
        End Property

        Public Property Text() As String
            Get
                Return Me._text
            End Get
            Set(ByVal value As String)
                Me._text = value
            End Set
        End Property

        Public Sub New()
            Me.Clear()
        End Sub

        Public Sub Clear()
            Me._imdbid = String.Empty
            Me._text = String.Empty
        End Sub
    End Class

    Public Shared Sub ConnectDB(ByVal Reset As Boolean)
        Try
            Dim NewDB As Boolean = False
            'create database if it doesn't exist
            If Not File.Exists(Path.Combine(Application.StartupPath, "Media.emm")) Then
                NewDB = True
            End If

            SQLcn.ConnectionString = String.Format("Data Source=""{0}"";Compress=True", Path.Combine(Application.StartupPath, "Media.emm"))
            SQLcn.Open()
            Dim cQuery As String = String.Empty
            Using SQLtransaction As SQLite.SQLiteTransaction = Master.SQLcn.BeginTransaction
                Using SQLcommand As SQLite.SQLiteCommand = SQLcn.CreateCommand

                    If Not NewDB AndAlso Reset Then
                        Dim tColumns As New DataTable
                        Dim tRestrict() As String = New String(2) {Nothing, Nothing, "movies"}
                        Dim aCol As New ArrayList
                        tColumns = SQLcn.GetSchema("Columns", tRestrict)
                        For Each col As DataRow In tColumns.Rows
                            aCol.Add(col("column_name").ToString)
                        Next
                        cQuery = String.Format("({0})", Strings.Join(aCol.ToArray, ", "))
                        SQLcommand.CommandText = "DROP INDEX IF EXISTS UniquePath;"
                        SQLcommand.ExecuteNonQuery()
                        SQLcommand.CommandText = "ALTER TABLE Movies RENAME TO tmp_movies;"
                        SQLcommand.ExecuteNonQuery()
                    End If
                    SQLcommand.CommandText = "CREATE TABLE IF NOT EXISTS Movies(" & _
                                "ID INTEGER PRIMARY KEY AUTOINCREMENT, " & _
                                "MoviePath TEXT NOT NULL, " & _
                                "Type BOOL NOT NULL DEFAULT False , " & _
                                "Title TEXT NOT NULL, " & _
                                "HasPoster BOOL NOT NULL DEFAULT False, " & _
                                "HasFanart BOOL NOT NULL DEFAULT False, " & _
                                "HasNfo BOOL NOT NULL DEFAULT False, " & _
                                "HasTrailer BOOL NOT NULL DEFAULT False, " & _
                                "HasSub BOOL NOT NULL DEFAULT False, " & _
                                "HasExtra BOOL NOT NULL DEFAULT False, " & _
                                "New BOOL DEFAULT False, " & _
                                "Mark BOOL NOT NULL DEFAULT False, " & _
                                "Source TEXT NOT NULL, " & _
                                "Imdb TEXT, " & _
                                "Lock BOOL NOT NULL DEFAULT False, " & _
                                "OriginalTitle TEXT, " & _
                                "Year TEXT, " & _
                                "Rating TEXT, " & _
                                "Votes TEXT, " & _
                                "MPAA TEXT, " & _
                                "Top250 TEXT, " & _
                                "Outline TEXT, " & _
                                "Plot TEXT, " & _
                                "Tagline TEXT, " & _
                                "Certification TEXT, " & _
                                "Genre TEXT, " & _
                                "Studio TEXT, " & _
                                "Runtime TEXT, " & _
                                "ReleaseDate TEXT, " & _
                                "Director TEXT, " & _
                                "Credits TEXT, " & _
                                "Playcount TEXT, " & _
                                "Watched TEXT, " & _
                                "File TEXT, " & _
                                "Path TEXT, " & _
                                "FileNameAndPath TEXT, " & _
                                "Status TEXT, " & _
                                "Trailer TEXT, " & _
                                "PosterPath TEXT, " & _
                                "FanartPath TEXT, " & _
                                "NfoPath TEXT, " & _
                                "TrailerPath TEXT, " & _
                                "SubPath TEXT" & _
                                ");"
                    SQLcommand.ExecuteNonQuery()
                    SQLcommand.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS UniquePath ON Movies (MoviePath);"
                    SQLcommand.ExecuteNonQuery()

                    SQLcommand.CommandText = "CREATE TABLE IF NOT EXISTS Sets(" & _
                                "SetName TEXT NOT NULL PRIMARY KEY" & _
                                 ");"
                    SQLcommand.ExecuteNonQuery()

                    SQLcommand.CommandText = "CREATE TABLE IF NOT EXISTS MoviesSets(" & _
                                "MovieID INTEGER NOT NULL, " & _
                                "SetName TEXT NOT NULL, " & _
                                "SetOrder TEXT NOT NULL, " & _
                                "PRIMARY KEY (MovieID,SetName) " & _
                                 ");"
                    SQLcommand.ExecuteNonQuery()
                    'SQLcommand.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS Index_MoviesSets ON MoviesSets (MovieID);"



                    SQLcommand.CommandText = "CREATE TABLE IF NOT EXISTS MoviesVStreams(" & _
                                "MovieID INTEGER NOT NULL, " & _
                                "StreamID INTEGER NOT NULL, " & _
                                "Video_Width TEXT, " & _
                                "Video_Height TEXT," & _
                                "Video_Codec TEXT, " & _
                                "Video_Duration TEXT, " & _
                                "Video_CodecId TEXT, " & _
                                "Video_ScanType TEXT, " & _
                                "Video_AspectDisplayRatio TEXT, " & _
                                "PRIMARY KEY (MovieID,StreamID) " & _
                                ");"
                    SQLcommand.ExecuteNonQuery()
                    SQLcommand.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS Index_MoviesVStreams ON MoviesVStreams (MovieID);"
                    SQLcommand.ExecuteNonQuery()

                    SQLcommand.CommandText = "CREATE TABLE IF NOT EXISTS MoviesAStreams(" & _
                                "MovieID INTEGER NOT NULL, " & _
                                "StreamID INTEGER NOT NULL, " & _
                                "Audio_Language TEXT, " & _
                                "Audio_Codec TEXT, " & _
                                "Audio_Channel TEXT, " & _
                                "Audio_CodecID TEXT, " & _
                                "PRIMARY KEY (MovieID,StreamID) " & _
                                ");"
                    SQLcommand.ExecuteNonQuery()
                    SQLcommand.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS Index_MoviesVStreams ON MoviesAStreams (MovieID);"
                    SQLcommand.ExecuteNonQuery()

                    SQLcommand.CommandText = "CREATE TABLE IF NOT EXISTS MoviesSubs(" & _
                                "MovieID INTEGER NOT NULL, " & _
                                "StreamID INTEGER NOT NULL, " & _
                                "subs TEXT, " & _
                                "PRIMARY KEY (MovieID,StreamID) " & _
                                 ");"
                    SQLcommand.ExecuteNonQuery()
                    SQLcommand.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS Index_MoviesSubs ON MoviesSubs (MovieID);"
                    SQLcommand.ExecuteNonQuery()

                    SQLcommand.CommandText = "CREATE TABLE IF NOT EXISTS MoviesPosters(" & _
                                "ID INTEGER PRIMARY KEY AUTOINCREMENT, " & _
                                "MovieID INTEGER NOT NULL, " & _
                                "thumbs TEXT" & _
                                ");"
                    SQLcommand.ExecuteNonQuery()
                    SQLcommand.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS Index_MoviesPosters ON MoviesPosters (MovieID);"
                    SQLcommand.ExecuteNonQuery()

                    SQLcommand.CommandText = "CREATE TABLE IF NOT EXISTS MoviesFanart(" & _
                                "ID INTEGER PRIMARY KEY AUTOINCREMENT, " & _
                                "MovieID INTEGER NOT NULL, " & _
                                "preview TEXT, " & _
                                "thumbs TEXT" & _
                                ");"
                    SQLcommand.ExecuteNonQuery()
                    SQLcommand.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS Index_MoviesFanart ON MoviesFanart (MovieID);"
                    SQLcommand.ExecuteNonQuery()

                    SQLcommand.CommandText = "CREATE TABLE  IF NOT EXISTS Actors(" & _
                                "Name TEXT PRIMARY KEY, " & _
                                "thumb TEXT" & _
                                ");"
                    SQLcommand.ExecuteNonQuery()
                    SQLcommand.CommandText = "CREATE TABLE  IF NOT EXISTS MoviesActors(" & _
                                "MovieID INTEGER NOT NULL, " & _
                                "ActorName TEXT NOT NULL, " & _
                                "Role TEXT, " & _
                                "PRIMARY KEY (MovieID,ActorName) " & _
                                ");"
                    SQLcommand.ExecuteNonQuery()


                    If Not NewDB AndAlso Reset Then
                        SQLcommand.CommandText = String.Concat("INSERT INTO Movies ", cQuery, " SELECT * FROM tmp_movies;")
                        SQLcommand.ExecuteNonQuery()
                        SQLcommand.CommandText = "DROP TABLE tmp_movies;"
                        SQLcommand.ExecuteNonQuery()
                    End If
                    SQLcommand.CommandText = "CREATE TABLE IF NOT EXISTS Sources(ID INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, path TEXT NOT NULL, Recursive BOOL NOT NULL DEFAULT False , Foldername BOOL NOT NULL DEFAULT False, Single BOOL NOT NULL DEFAULT False);"
                    SQLcommand.ExecuteNonQuery()
                    SQLcommand.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS UniqueSource ON Sources (Path);"
                    SQLcommand.ExecuteNonQuery()
                End Using
                SQLtransaction.Commit()
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Sub CreateDefaultOptions()
        With DefaultOptions
            .bCast = True
            .bDirector = True
            .bGenre = True
            .bMPAA = True
            .bMusicBy = True
            .bOtherCrew = True
            .bOutline = True
            .bPlot = True
            .bProducers = True
            .bRating = True
            .bRelease = True
            .bRuntime = True
            .bStudio = True
            .bTagline = True
            .bTitle = True
            .bTrailer = True
            .bVotes = True
            .bWriters = True
            .bYear = True
        End With
    End Sub

    Public Shared Sub ResizePB(ByRef pbResize As PictureBox, ByRef pbCache As PictureBox, ByVal maxHeight As Integer, ByVal maxWidth As Integer)

        '//
        ' Resize the picture box based on the dimensions of the image and the dimensions
        ' of the available space... try to use the most screen real estate
        '
        ' Why not use "Zoom" for fanart background? - To keep the image at the top. Zoom centers vertically.
        '\\

        Try
            If Not IsNothing(pbCache.Image) Then
                pbResize.SizeMode = PictureBoxSizeMode.Normal
                Dim sPropPerc As Single = 1.0 'no default scaling

                pbResize.Size = New Size(maxWidth, maxHeight)

                ' Height
                If pbCache.Image.Height > pbResize.Height Then
                    ' Reduce height first
                    sPropPerc = CSng(pbResize.Height / pbCache.Image.Height)
                End If

                ' Width
                If (pbCache.Image.Width * sPropPerc) > pbResize.Width Then
                    ' Scaled width exceeds Box's width, recalculate scale_factor
                    sPropPerc = CSng(pbResize.Width / pbCache.Image.Width)
                End If

                ' Get the source bitmap.
                Dim bmSource As New Bitmap(pbCache.Image)
                ' Make a bitmap for the result.
                Dim bmDest As New Bitmap( _
                Convert.ToInt32(bmSource.Width * sPropPerc), _
                Convert.ToInt32(bmSource.Height * sPropPerc))
                ' Make a Graphics object for the result Bitmap.
                Dim grDest As Graphics = Graphics.FromImage(bmDest)
                ' Copy the source image into the destination bitmap.
                grDest.DrawImage(bmSource, 0, 0, _
                bmDest.Width + 1, _
                bmDest.Height + 1)
                ' Display the result.
                pbResize.Image = bmDest

                'tweak pb after resizing pic
                pbResize.Width = pbResize.Image.Width
                pbResize.Height = pbResize.Image.Height
                'center it

                'Clean up
                bmSource = Nothing
                bmDest = Nothing
                grDest = Nothing
            Else
                pbResize.Left = 0
                pbResize.Size = New Size(maxWidth, maxHeight)
            End If
        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Sub SetOverlay(ByRef pbOverlay As PictureBox)

        '//
        ' Put our crappy glossy overlay over the poster
        '\\

        Try
            Dim bmOverlay As New Bitmap(pbOverlay.Image)
            Dim grOverlay As Graphics = Graphics.FromImage(bmOverlay)
            Dim bmHeight As Integer = pbOverlay.Image.Height * 0.65

            grOverlay.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic

            grOverlay.DrawImage(My.Resources.overlay, 0, 0, pbOverlay.Image.Width, bmHeight)
            pbOverlay.Image = bmOverlay

            bmOverlay = New Bitmap(pbOverlay.Image)
            grOverlay = Graphics.FromImage(bmOverlay)

            grOverlay.DrawImage(My.Resources.overlay2, 0, 0, pbOverlay.Image.Width, pbOverlay.Image.Height)
            pbOverlay.Image = bmOverlay

            grOverlay.Dispose()
            bmOverlay = Nothing
        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Function FilterName(ByRef movieName As String) As String

        '//
        ' Clean all the crap out of the name
        '\\

        Dim strSplit() As String
        Try

            'run through each of the custom filters
            If eSettings.FilterCustom.Count > 0 Then
                For Each Str As String In eSettings.FilterCustom

                    If Strings.InStr(Str, "[->]") > 0 Then
                        strSplit = Strings.Split(Str, "[->]")
                        movieName = Strings.Replace(movieName, Regex.Match(movieName, strSplit(0)).ToString, strSplit(1))
                    Else
                        movieName = Strings.Replace(movieName, Regex.Match(movieName, Str).ToString, String.Empty)
                    End If
                Next
            End If

            'Convert String To Proper Case
            If eSettings.ProperCase Then
                movieName = ProperCase(movieName)
            End If

        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return CleanStackingMarkers(movieName.Trim)

    End Function

    Private Shared Function ProperCase(ByVal sString As String) As String
        Dim sReturn As String = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(sString)
        Dim toUpper As String = "\b(hd|cd|dvd|bc|b\.c\.|ad|a\.d\.|sw|nw|se|sw|ii|iii|iv|vi|vii|viii|ix|x)\b"

        Dim mcUp As MatchCollection = Regex.Matches(sReturn, toUpper, RegexOptions.IgnoreCase)
        For Each M As Match In mcUp
            sReturn = sReturn.Replace(M.Value, Strings.StrConv(M.Value, VbStrConv.Uppercase))
        Next

        Return sReturn
    End Function

    Public Shared Function IsStacked(ByVal sName As String) As Boolean
        If Regex.IsMatch(sName, "(?i)[ _\.-]+cd[ _\.-]*([0-9a-d]+)") OrElse Regex.IsMatch(sName, "(?i)[ _\.-]+dvd[ _\.-]*([0-9a-d]+)") OrElse _
        Regex.IsMatch(sName, "(?i)[ _\.-]+part[ _\.-]*([0-9a-d]+)") OrElse Regex.IsMatch(sName, "(?i)[ _\.-]+dis[ck][ _\.-]*([0-9a-d]+)") Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function CleanStackingMarkers(ByVal sPath As String) As String

        '//
        ' Removes the stacking indicators from the file name
        '\\

        Dim filename As String = String.Empty
        Dim strTemp As String = String.Empty

        strTemp = Regex.Match(sPath, "(?i)[ _\.-]+cd[ _\.-]*([0-9a-d]+)").ToString
        If Not String.IsNullOrEmpty(strTemp) Then
            sPath = sPath.Replace(strTemp, String.Empty)
        End If

        strTemp = Regex.Match(sPath, "(?i)[ _\.-]+dvd[ _\.-]*([0-9a-d]+)").ToString
        If Not String.IsNullOrEmpty(strTemp) Then
            sPath = sPath.Replace(strTemp, String.Empty)
        End If

        strTemp = Regex.Match(sPath, "(?i)[ _\.-]+part[ _\.-]*([0-9a-d]+)").ToString
        If Not String.IsNullOrEmpty(strTemp) Then
            sPath = sPath.Replace(strTemp, String.Empty)
        End If

        strTemp = Regex.Match(sPath, "(?i)[ _\.-]+dis[ck][ _\.-]*([0-9a-d]+)").ToString
        If Not String.IsNullOrEmpty(strTemp) Then
            sPath = sPath.Replace(strTemp, String.Empty)
        End If

        Return sPath.Trim
    End Function

    Public Shared Sub ScanSourceDir(ByVal sSource As String, ByVal sPath As String, ByVal bRecur As Boolean, ByVal bUseFolder As Boolean, ByVal bSingle As Boolean)

        '//
        ' Get all directories in the parent directory
        '\\

        Try
            Dim sMoviePath As String = String.Empty
            If Directory.Exists(sPath) Then

                'check if there are any movies in the parent folder
                ScanForFiles(sPath, sSource, bUseFolder, bSingle)

                Dim Dirs As String() = Directory.GetDirectories(sPath)

                For Each inDir As String In Dirs
                    If isValidDir(inDir) Then
                        ScanForFiles(inDir, sSource, bUseFolder, bSingle)
                    End If

                    If bRecur AndAlso isValidDir(inDir) Then
                        ScanSourceDir(sSource, inDir, bRecur, bUseFolder, bSingle)
                    End If
                Next
            End If
        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Sub ScanForFiles(ByVal sPath As String, ByVal sSource As String, ByVal bUseFolder As Boolean, ByVal bSingle As Boolean)

        '//
        ' Get all files in the directory
        '\\

        Try

            If Directory.Exists(sPath) Then
                Dim tmpList As New ArrayList
                Dim di As New DirectoryInfo(sPath)
                Dim lFi As New List(Of FileInfo)
                Dim aContents(6) As String
                Dim SkipStack As Boolean = False
                Dim fList As New List(Of FileAndSource)
                Dim tSingle As Boolean = False

                Try
                    lFi.AddRange(di.GetFiles())
                Catch
                End Try

                If lFi.Count > 0 Then
                    lFi.Sort(AddressOf SortFileNames)

                    For Each lFile As FileInfo In lFi
                        If alMoviePaths.Contains(lFile.FullName) Then
                            'it's already on the list, don't bother scanning
                            If Master.eSettings.NoStackExts.Contains(lFile.Extension.ToLower) Then
                                tmpList.Add(lFile.FullName)
                                SkipStack = True
                            Else
                                tmpList.Add(CleanStackingMarkers(lFile.FullName))
                            End If
                            fList.Add(New FileAndSource With {.Filename = lFile.FullName, .Source = "[!FROMDB!]"})
                            If bSingle AndAlso Not SkipStack Then Exit For
                        Else
                            If eSettings.ValidExts.Contains(lFile.Extension.ToLower) AndAlso Not tmpList.Contains(CleanStackingMarkers(lFile.FullName)) AndAlso _
                            Not lFile.Name.ToLower.Contains("-trailer") AndAlso Not lFile.Name.ToLower.Contains("[trailer") AndAlso Not lFile.Name.ToLower.Contains("sample") AndAlso _
                            ((eSettings.SkipStackSizeCheck AndAlso IsStacked(lFile.Name)) OrElse lFile.Length >= eSettings.SkipLessThan * 1048576) Then
                                If Master.eSettings.NoStackExts.Contains(lFile.Extension.ToLower) Then
                                    tmpList.Add(lFile.FullName)
                                    SkipStack = True
                                Else
                                    tmpList.Add(CleanStackingMarkers(lFile.FullName))
                                End If
                                aContents = GetFolderContents(lFile.FullName)
                                fList.Add(New FileAndSource With {.Filename = lFile.FullName, .Source = sSource, .isSingle = bSingle, .UseFolder = If(bSingle, bUseFolder, False), .Poster = aContents(0), .Fanart = aContents(1), .Nfo = aContents(2), .Trailer = aContents(3), .Subs = aContents(4), .Extra = aContents(5)})
                                If bSingle AndAlso Not SkipStack Then Exit For
                            End If
                        End If
                    Next

                    If fList.Count = 1 Then tSingle = True

                    If tSingle Then
                        fList(0).isSingle = True
                        fList(0).UseFolder = bUseFolder
                        MediaList.Add(fList(0))
                    Else
                        MediaList.AddRange(fList)
                    End If

                    fList = Nothing
                End If
            End If
        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Sub GetAVImages(ByVal fiAV As MediaInfo.Fileinfo, ByVal strPath As String)

        '//
        ' Parse the Flags XML and set the proper images
        '\\

        Dim mePath As String = String.Concat(Application.StartupPath, Path.DirectorySeparatorChar, "Images", Path.DirectorySeparatorChar, "Flags")
        If File.Exists(Path.Combine(mePath, "Flags.xml")) Then
            Try
                Dim atypeRef As String = String.Empty
                Dim vresImage As String = String.Empty
                Dim vsourceImage As String = String.Empty
                Dim vtypeImage As String = String.Empty
                Dim atypeImage As String = String.Empty
                Dim achanImage As String = String.Empty
                Dim tVideo As MediaInfo.Video = GetBestVideo(fiAV)
                Dim tAudio As MediaInfo.Audio = GetBestAudio(fiAV)

                Dim xmlFlags As XDocument = XDocument.Load(Path.Combine(mePath, "Flags.xml"))

                'video resolution
                Dim xVResDefault = From xDef In xmlFlags...<vres> Select xDef.Element("default").Element("icon").Value
                If xVResDefault.Count > 0 Then
                    vresImage = Path.Combine(mePath, xVResDefault(0).ToString)
                End If

                Dim strRes As String = GetResFromDimensions(tVideo).ToLower
                If Not String.IsNullOrEmpty(strRes) Then
                    Dim xVResFlag = From xVRes In xmlFlags...<vres>...<name> Where Regex.IsMatch(strRes, xVRes.@searchstring) Select xVRes.<icon>.Value
                    If xVResFlag.Count > 0 Then
                        vresImage = Path.Combine(mePath, xVResFlag(0).ToString)
                    End If
                End If

                'video source
                Dim xVSourceDefault = From xDef In xmlFlags...<vsource> Select xDef.Element("default").Element("icon").Value
                If xVSourceDefault.Count > 0 Then
                    vsourceImage = Path.Combine(mePath, xVSourceDefault(0).ToString)
                End If

                Dim xVSourceFlag = From xVSource In xmlFlags...<vsource>...<name> Where Regex.IsMatch(String.Concat(Directory.GetParent(strPath).Name.ToLower, Path.DirectorySeparatorChar, Path.GetFileName(strPath).ToLower), xVSource.@searchstring) Select xVSource.<icon>.Value
                If xVSourceFlag.Count > 0 Then
                    vsourceImage = Path.Combine(mePath, xVSourceFlag(0).ToString)
                End If

                'video type
                Dim xVTypeDefault = From xDef In xmlFlags...<vtype> Select xDef.Element("default").Element("icon").Value
                If xVTypeDefault.Count > 0 Then
                    vtypeImage = Path.Combine(mePath, xVTypeDefault(0).ToString)
                End If

                Dim vCodec As String = String.Empty
                vCodec = If(String.IsNullOrEmpty(tVideo.CodecID), tVideo.Codec, tVideo.CodecID)
                If Not String.IsNullOrEmpty(vCodec) Then
                    Dim xVTypeFlag = From xVType In xmlFlags...<vtype>...<name> Where Regex.IsMatch(vCodec, xVType.@searchstring) Select xVType.<icon>.Value
                    If xVTypeFlag.Count > 0 Then
                        vtypeImage = Path.Combine(mePath, xVTypeFlag(0).ToString)
                    End If
                End If

                'audio type
                Dim xATypeDefault = From xDef In xmlFlags...<atype> Select xDef.Element("default").Element("icon").Value
                If xATypeDefault.Count > 0 Then
                    atypeImage = Path.Combine(mePath, xATypeDefault(0).ToString)
                End If

                Dim aCodec As String = String.Empty
                aCodec = If(String.IsNullOrEmpty(tAudio.CodecID), tAudio.Codec, tAudio.CodecID)
                If Not String.IsNullOrEmpty(aCodec) Then
                    Dim xATypeFlag = From xAType In xmlFlags...<atype>...<name> Where Regex.IsMatch(aCodec, xAType.@searchstring) Select xAType.<icon>.Value, xAType.<ref>.Value
                    If xATypeFlag.Count > 0 Then
                        atypeImage = Path.Combine(mePath, xATypeFlag(0).icon.ToString)
                        If Not IsNothing(xATypeFlag(0).ref) Then
                            atypeRef = xATypeFlag(0).ref.ToString
                        End If
                    End If
                End If

                'audio channels
                Dim xAChanDefault = From xDef In xmlFlags...<achan> Select xDef.Element("default").Element("icon").Value
                If xAChanDefault.Count > 0 Then
                    achanImage = Path.Combine(mePath, xAChanDefault(0).ToString)
                End If

                If Not String.IsNullOrEmpty(tAudio.Channels) Then
                    Dim xAChanFlag = From xAChan In xmlFlags...<achan>...<name> Where Regex.IsMatch(tAudio.Channels, Regex.Replace(xAChan.@searchstring, "(\{[^\}]+\})", String.Empty)) And Regex.IsMatch(atypeRef, Regex.Match(xAChan.@searchstring, "\{atype=([^\}]+)\}").Groups(1).Value.ToString) Select xAChan.<icon>.Value
                    If xAChanFlag.Count > 0 Then
                        achanImage = Path.Combine(mePath, xAChanFlag(0).ToString)
                    End If
                End If

                If File.Exists(vresImage) Then
                    Using fsImage As New FileStream(vresImage, FileMode.Open, FileAccess.Read)
                        frmMain.pbResolution.Image = Image.FromStream(fsImage)
                    End Using
                End If

                If File.Exists(vsourceImage) Then
                    Using fsImage As New FileStream(vsourceImage, FileMode.Open, FileAccess.Read)
                        frmMain.pbVideo.Image = Image.FromStream(fsImage)
                    End Using
                End If

                If File.Exists(vtypeImage) Then
                    Using fsImage As New FileStream(vtypeImage, FileMode.Open, FileAccess.Read)
                        frmMain.pbVType.Image = Image.FromStream(fsImage)
                    End Using
                End If

                If File.Exists(atypeImage) Then
                    Using fsImage As New FileStream(atypeImage, FileMode.Open, FileAccess.Read)
                        frmMain.pbAudio.Image = Image.FromStream(fsImage)
                    End Using
                End If

                If File.Exists(achanImage) Then
                    Using fsImage As New FileStream(achanImage, FileMode.Open, FileAccess.Read)
                        frmMain.pbChannels.Image = Image.FromStream(fsImage)
                    End Using
                End If
            Catch ex As Exception
                eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        Else
            MsgBox(String.Concat("Cannot find Flags.xml.", vbNewLine, vbNewLine, "Expected path:", vbNewLine, Path.Combine(mePath, "Flags.xml")), MsgBoxStyle.Critical, "File Not Found")
        End If
    End Sub

    Public Shared Function GetStudioImage(ByVal strStudio As String) As Image

        '//
        ' Parse the Studio XML and set the proper image
        '\\

        Dim imgStudioStr As String = String.Empty
        Dim imgStudio As Image = Nothing
        Dim mePath As String = String.Concat(Application.StartupPath, Path.DirectorySeparatorChar, "Images", Path.DirectorySeparatorChar, "Studios")

        If File.Exists(Path.Combine(mePath, String.Concat(strStudio, ".png"))) Then
            Using fsImage As New FileStream(Path.Combine(mePath, String.Concat(strStudio, ".png")), FileMode.Open, FileAccess.Read)
                imgStudio = Image.FromStream(fsImage)
            End Using
        Else
            If File.Exists(Path.Combine(mePath, "Studios.xml")) Then
                Try
                    Dim xmlStudio As XDocument = XDocument.Load(Path.Combine(mePath, "Studios.xml"))

                    Dim xDefault = From xDef In xmlStudio...<default> Select xDef.<icon>.Value
                    If xDefault.Count > 0 Then
                        imgStudioStr = Path.Combine(mePath, xDefault(0).ToString)
                    End If

                    Dim xStudio = From xStu In xmlStudio...<name> Where Regex.IsMatch(Strings.Trim(strStudio).ToLower, xStu.@searchstring) Select xStu.<icon>.Value
                    If xStudio.Count > 0 Then
                        imgStudioStr = Path.Combine(mePath, xStudio(0).ToString)
                    End If

                    If Not String.IsNullOrEmpty(imgStudioStr) AndAlso File.Exists(imgStudioStr) Then
                        Using fsImage As New FileStream(imgStudioStr, FileMode.Open, FileAccess.Read)
                            imgStudio = Image.FromStream(fsImage)
                        End Using
                    End If

                Catch ex As Exception
                    eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                End Try
            Else
                MsgBox(String.Concat("Cannot find Studios.xml.", vbNewLine, vbNewLine, "Expected path:", vbNewLine, Path.Combine(mePath, "Studios.xml")), MsgBoxStyle.Critical, "File Not Found")
            End If
        End If

        Return imgStudio

    End Function

    Public Shared Function GetGenreImage(ByVal strGenre As String) As Image

        '//
        ' Set the proper images based on the genre string
        '\\

        Dim imgGenre As Image = Nothing
        Dim imgGenreStr As String = String.Empty

        Dim mePath As String = String.Concat(Application.StartupPath, Path.DirectorySeparatorChar, "Images", Path.DirectorySeparatorChar, "Genres")

        If File.Exists(Path.Combine(mePath, "Genres.xml")) Then
            Try
                Dim xmlGenre As XDocument = XDocument.Load(Path.Combine(mePath, "Genres.xml"))

                Dim xDefault = From xDef In xmlGenre...<default> Select xDef.<icon>.Value
                If xDefault.Count > 0 Then
                    imgGenreStr = Path.Combine(mePath, xDefault(0).ToString)
                End If

                Dim xGenre = From xGen In xmlGenre...<name> Where strGenre.ToLower = xGen.@searchstring.ToLower Select xGen.<icon>.Value
                If xGenre.Count > 0 Then
                    imgGenreStr = Path.Combine(mePath, xGenre(0).ToString)
                End If

                If Not String.IsNullOrEmpty(imgGenreStr) AndAlso File.Exists(imgGenreStr) Then
                    Using fsImage As New FileStream(imgGenreStr, FileMode.Open, FileAccess.Read)
                        imgGenre = Image.FromStream(fsImage)
                    End Using
                End If

            Catch ex As Exception
                eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        Else
            MsgBox(String.Concat("Cannot find Genres.xml.", vbNewLine, vbNewLine, "Expected path:", vbNewLine, Path.Combine(mePath, "Genres.xml")), MsgBoxStyle.Critical, "File Not Found")
        End If

        Return imgGenre
    End Function

    Public Shared Function GetRatingImage(ByVal strRating As String) As Image

        '//
        ' Parse the floating Rating box
        '\\

        Dim imgRating As Image = Nothing
        Dim imgRatingStr As String = String.Empty

        Dim mePath As String = String.Concat(Application.StartupPath, Path.DirectorySeparatorChar, "Images", Path.DirectorySeparatorChar, "Ratings")

        If File.Exists(Path.Combine(mePath, "Ratings.xml")) Then

            Try
                Dim xmlRating As XDocument = XDocument.Load(Path.Combine(mePath, "Ratings.xml"))

                If Master.eSettings.UseCertForMPAA AndAlso Not eSettings.CertificationLang = "USA" AndAlso xmlRating.Element("ratings").Descendants(Master.eSettings.CertificationLang.ToLower).Count > 0 Then
                    Dim xRating = From xRat In xmlRating.Element("ratings").Element(Master.eSettings.CertificationLang.ToLower)...<name> Where strRating.ToLower = xRat.@searchstring.ToLower Select xRat.<icon>.Value
                    If xRating.Count > 0 Then
                        imgRatingStr = Path.Combine(mePath, xRating(0).ToString)
                    End If
                Else
                    Dim xRating = From xRat In xmlRating...<usa>...<name> Where strRating.ToLower.Contains(xRat.@searchstring.ToLower) Select xRat.<icon>.Value
                    If xRating.Count > 0 Then
                        imgRatingStr = Path.Combine(mePath, xRating(0).ToString)
                    End If
                End If

                If Not String.IsNullOrEmpty(imgRatingStr) AndAlso File.Exists(imgRatingStr) Then
                    Using fsImage As New FileStream(imgRatingStr, FileMode.Open, FileAccess.Read)
                        imgRating = Image.FromStream(fsImage)
                    End Using
                End If

            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        Else
            MsgBox(String.Concat("Cannot find Ratings.xml.", vbNewLine, vbNewLine, "Expected path:", vbNewLine, Path.Combine(mePath, "Ratings.xml")), MsgBoxStyle.Critical, "File Not Found")
        End If

        Return imgRating
    End Function

    Public Shared Function isValidDir(ByVal sPath As String) As Boolean

        '//
        ' Make sure it's a valid directory
        '\\

        Try

            sPath = sPath.Remove(0, sPath.IndexOf("\"))
            If Path.GetDirectoryName(sPath).ToLower = "extrathumbs" OrElse _
            Path.GetDirectoryName(sPath).ToLower = "extras" OrElse _
            Path.GetDirectoryName(sPath).ToLower = "video_ts" OrElse _
            Path.GetDirectoryName(sPath).ToLower = "audio_ts" OrElse _
            Path.GetDirectoryName(sPath).ToLower = "recycler" OrElse _
            Path.GetDirectoryName(sPath).ToLower = "subs" OrElse _
            Path.GetDirectoryName(sPath).ToLower = "subtitles" OrElse _
            sPath.ToLower.Contains("-trailer") OrElse _
            sPath.ToLower.Contains("[trailer") OrElse _
            sPath.ToLower.Contains("temporary files") OrElse _
            sPath.ToLower.Contains("(noscan)") OrElse _
            sPath.ToLower.Contains("$recycle.bin") OrElse _
            sPath.ToLower.Contains("lost+found") OrElse _
            sPath.ToLower.Contains("system volume information") OrElse _
            sPath.ToLower.Contains("sample") OrElse _
            sPath.Contains(":") Then
                Return False
            End If

        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            Return False
        End Try
        Return True 'This is the Else
    End Function


    Public Shared Function RemoveExtFromPath(ByVal sPath As String)

        '//
        ' Get the entire path without the extension
        '\\

        Try
            Return Path.Combine(Directory.GetParent(sPath).FullName, Path.GetFileNameWithoutExtension(sPath))
        Catch
            Return String.Empty
        End Try

    End Function

    Public Shared Sub SaveSingleNFOItem(ByVal sPath As String, ByVal strToWrite As String, ByVal strNode As String)

        '//
        ' Save just one item of an NFO file
        '\\

        Try
            Dim xmlDoc As New XmlDocument()
            'use streamreader to open NFO so we don't get any access violations when trying to save
            Dim xmlSR As New StreamReader(sPath)
            'copy NFO to string
            Dim xmlString As String = xmlSR.ReadToEnd
            'close the streamreader... we're done with it
            xmlSR.Close()
            xmlSR = Nothing

            xmlDoc.LoadXml(xmlString)
            Dim xNode As XmlNode = xmlDoc.SelectSingleNode(strNode)
            xNode.InnerText = strToWrite
            xmlDoc.Save(sPath)

            xmlDoc = Nothing
        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Function LoadMovieFromNFO(ByVal sPath As String) As Media.Movie

        '//
        ' Deserialze the NFO to pass all the data to a Media.Movie
        '\\

        Dim xmlSer As XmlSerializer = Nothing
        Dim xmlMov As New Media.Movie
        Try
            If File.Exists(sPath) AndAlso Not Master.eSettings.ValidExts.Contains(Path.GetExtension(sPath).ToLower) Then
                Using xmlSR As StreamReader = New StreamReader(sPath)
                    xmlSer = New XmlSerializer(GetType(Media.Movie))
                    xmlMov = CType(xmlSer.Deserialize(xmlSR), Media.Movie)
                End Using
            Else
                If Not String.IsNullOrEmpty(sPath) Then
                    Dim sReturn As New NonConf
                    sReturn = GetIMDBFromNonConf(sPath)
                    xmlMov.IMDBID = sReturn.IMDBID
                    Try
                        If Not String.IsNullOrEmpty(sReturn.Text) Then
                            Using xmlSTR As StringReader = New StringReader(sReturn.Text)
                                xmlSer = New XmlSerializer(GetType(Media.Movie))
                                xmlMov = CType(xmlSer.Deserialize(xmlSTR), Media.Movie)
                                xmlMov.IMDBID = sReturn.IMDBID
                            End Using
                        End If
                    Catch
                    End Try
                End If
            End If

        Catch
            xmlMov = New Media.Movie
            If Not String.IsNullOrEmpty(sPath) Then
                Dim sReturn As New NonConf
                sReturn = GetIMDBFromNonConf(sPath)
                xmlMov.IMDBID = sReturn.IMDBID
                Try
                    If Not String.IsNullOrEmpty(sReturn.Text) Then
                        Using xmlSTR As StringReader = New StringReader(sReturn.Text)
                            xmlSer = New XmlSerializer(GetType(Media.Movie))
                            xmlMov = CType(xmlSer.Deserialize(xmlSTR), Media.Movie)
                            xmlMov.IMDBID = sReturn.IMDBID
                        End Using
                    End If
                Catch
                End Try
            End If
        End Try

        If Not IsNothing(xmlSer) Then
            xmlSer = Nothing
        End If

        Return xmlMov

    End Function

    Public Shared Function XMLToLowerCase(ByVal sXML As String) As String
        Dim sMatches As MatchCollection = Regex.Matches(sXML, "(?i)\<(.*?)\>")
        For Each sMatch As Match In sMatches
            sXML = sXML.Replace(sMatch.Groups(1).Value, sMatch.Groups(1).Value.ToLower)
        Next
        Return sXML
    End Function

    Public Shared Function GetIMDBFromNonConf(ByVal sPath As String) As NonConf
        Dim tNonConf As New NonConf
        Dim dirInfo As New DirectoryInfo(Directory.GetParent(sPath).FullName)
        Dim ioFi As New List(Of FileInfo)

        Try
            ioFi.AddRange(dirInfo.GetFiles("*.nfo"))
        Catch
        End Try

        Try
            ioFi.AddRange(dirInfo.GetFiles("*.info"))
        Catch
        End Try

        For Each sFile As FileInfo In ioFi
            Using srInfo As New StreamReader(sFile.FullName)
                Dim sInfo As String = srInfo.ReadToEnd
                Dim sIMDBID As String = Regex.Match(sInfo, "tt\d\d\d\d\d\d\d", RegexOptions.Multiline Or RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToString

                If Not String.IsNullOrEmpty(sIMDBID) Then
                    tNonConf.IMDBID = sIMDBID
                    'now lets try to see if the rest of the file is a proper nfo
                    If sInfo.ToLower.Contains("</movie>") Then
                        tNonConf.Text = XMLToLowerCase(sInfo.Substring(0, sInfo.ToLower.IndexOf("</movie>") + 8))
                    End If
                    Exit For
                End If

            End Using
        Next

        ioFi = Nothing

        Return tNonConf
    End Function

    Public Shared Function GetFolderContents(ByVal sPath As String) As String()

        '//
        ' Check if a folder has all the items (nfo, poster, fanart, etc)
        '\\

        Dim NfoPath As String = String.Empty
        Dim PosterPath As String = String.Empty
        Dim FanartPath As String = String.Empty
        Dim TrailerPath As String = String.Empty
        Dim SubPath As String = String.Empty
        Dim ExtraPath As String = String.Empty
        Dim aResults(6) As String
        Dim tmpName As String = String.Empty
        Dim tmpNameNoStack As String = String.Empty
        Dim currname As String = String.Empty
        Dim parPath As String = String.Empty
        Try

            Dim di As DirectoryInfo

            If Master.eSettings.VideoTSParent AndAlso Directory.GetParent(sPath).Name.ToLower = "video_ts" Then
                di = New DirectoryInfo(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName)
                If File.Exists(String.Concat(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName, Path.DirectorySeparatorChar, "extrathumbs", Path.DirectorySeparatorChar, "thumb1.jpg")) Then
                    ExtraPath = String.Concat(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName, Path.DirectorySeparatorChar, "extrathumbs", Path.DirectorySeparatorChar, "thumb1.jpg")
                End If
            Else
                di = New DirectoryInfo(Directory.GetParent(sPath).FullName)
                If File.Exists(String.Concat(Directory.GetParent(sPath).FullName, Path.DirectorySeparatorChar, "extrathumbs", Path.DirectorySeparatorChar, "thumb1.jpg")) Then
                    ExtraPath = String.Concat(Directory.GetParent(sPath).FullName, Path.DirectorySeparatorChar, "extrathumbs", Path.DirectorySeparatorChar, "thumb1.jpg")
                End If
            End If

            Dim lFi As New List(Of FileInfo)()
            Dim fList As New ArrayList

            Try
                lFi.AddRange(di.GetFiles())
            Catch
            End Try

            For Each sFile As FileInfo In lFi
                fList.Add(sFile.FullName)
            Next


            parPath = Directory.GetParent(sPath).FullName
            tmpName = Path.Combine(parPath, CleanStackingMarkers(Path.GetFileNameWithoutExtension(sPath)))
            tmpNameNoStack = Path.Combine(parPath, Path.GetFileNameWithoutExtension(sPath))

            'fanart
            Select Case True
                Case fList.Contains(Path.Combine(parPath, "fanart.jpg"))
                    FanartPath = Path.Combine(parPath, "fanart.jpg")
                Case fList.Contains(String.Concat(tmpName, "-fanart.jpg"))
                    FanartPath = String.Concat(tmpName, "-fanart.jpg")
                Case fList.Contains(String.Concat(tmpName, ".fanart.jpg"))
                    FanartPath = String.Concat(tmpName, ".fanart.jpg")
                Case fList.Contains(String.Concat(tmpNameNoStack, "-fanart.jpg"))
                    FanartPath = String.Concat(tmpNameNoStack, "-fanart.jpg")
                Case fList.Contains(String.Concat(tmpNameNoStack, ".fanart.jpg"))
                    FanartPath = String.Concat(tmpNameNoStack, ".fanart.jpg")
                Case fList.Contains(Path.Combine(parPath, "video_ts-fanart.jpg"))
                    FanartPath = Path.Combine(parPath, "video_ts-fanart.jpg")
                Case fList.Contains(Path.Combine(parPath, "video_ts.fanart.jpg"))
                    FanartPath = Path.Combine(parPath, "video_ts.fanart.jpg")
            End Select

            'poster
            Select Case True
                Case fList.Contains(Path.Combine(parPath, "movie.tbn"))
                    PosterPath = Path.Combine(parPath, "movie.tbn")
                Case fList.Contains(Path.Combine(parPath, "poster.tbn"))
                    PosterPath = Path.Combine(parPath, "poster.tbn")
                Case fList.Contains(Path.Combine(parPath, "movie.jpg"))
                    PosterPath = Path.Combine(parPath, "movie.jpg")
                Case fList.Contains(String.Concat(tmpName, ".tbn"))
                    PosterPath = String.Concat(tmpName, ".tbn")
                Case fList.Contains(String.Concat(tmpNameNoStack, ".tbn"))
                    PosterPath = String.Concat(tmpNameNoStack, ".tbn")
                Case fList.Contains(Path.Combine(parPath, "poster.jpg"))
                    PosterPath = Path.Combine(parPath, "poster.jpg")
                Case fList.Contains(String.Concat(tmpName, ".jpg"))
                    PosterPath = String.Concat(tmpName, ".jpg")
                Case fList.Contains(String.Concat(tmpNameNoStack, ".jpg"))
                    PosterPath = String.Concat(tmpNameNoStack, ".jpg")
                Case fList.Contains(Path.Combine(parPath, "video_ts.jpg"))
                    PosterPath = Path.Combine(parPath, "video_ts.jpg")
                Case fList.Contains(Path.Combine(parPath, "folder.jpg"))
                    PosterPath = Path.Combine(parPath, "folder.jpg")
            End Select

            'nfo
            Select Case True
                Case fList.Contains(Path.Combine(parPath, "movie.nfo"))
                    NfoPath = Path.Combine(parPath, "movie.nfo")
                Case fList.Contains(String.Concat(tmpName, ".nfo"))
                    NfoPath = String.Concat(tmpName, ".nfo")
                Case fList.Contains(String.Concat(tmpNameNoStack, ".nfo"))
                    NfoPath = String.Concat(tmpNameNoStack, ".nfo")
            End Select

            'sub
            Dim sExt() As String = Split(".sst,.srt,.sub,.ssa,.aqt,.smi,.sami,.jss,.mpl,.rt,.idx,.ass", ",")

            For Each t As String In sExt
                Select Case True
                    Case fList.Contains(String.Concat(tmpName, t))
                        SubPath = String.Concat(tmpName, t)
                        Exit For
                    Case fList.Contains(String.Concat(tmpNameNoStack, t))
                        SubPath = String.Concat(tmpNameNoStack, t)
                        Exit For
                End Select
            Next

            For Each t As String In Master.eSettings.ValidExts
                Select Case True
                    Case fList.Contains(String.Concat(tmpName, "-trailer", t))
                        TrailerPath = String.Concat(tmpName, "-trailer", t)
                    Case fList.Contains(String.Concat(tmpNameNoStack, "-trailer", t))
                        TrailerPath = String.Concat(tmpNameNoStack, "-trailer", t)
                    Case fList.Contains(String.Concat(tmpName, "[trailer]", t))
                        TrailerPath = String.Concat(tmpName, "[trailer]", t)
                    Case fList.Contains(String.Concat(tmpNameNoStack, "[trailer]", t))
                        TrailerPath = String.Concat(tmpNameNoStack, "[trailer]", t)
                End Select
            Next

            aResults(0) = PosterPath
            aResults(1) = FanartPath
            aResults(2) = NfoPath
            aResults(3) = TrailerPath
            aResults(4) = SubPath
            aResults(5) = ExtraPath
        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return aResults
    End Function


    Public Shared Function FIToString(ByVal miFI As MediaInfo.Fileinfo) As String

        '//
        ' Convert Fileinfo into a string to be displayed in the GUI
        '\\

        Dim strOutput As New StringBuilder
        Dim iVS As Integer = 1
        Dim iAS As Integer = 1
        Dim iSS As Integer = 1

        Try
            If Not IsNothing(miFI) Then

                If Not miFI.StreamDetails Is Nothing Then
                    If miFI.StreamDetails.Video.Count > 0 Then
                        strOutput.AppendFormat("Video Streams: {0}{1}", miFI.StreamDetails.Video.Count.ToString, vbNewLine)
                    End If

                    If miFI.StreamDetails.Audio.Count > 0 Then
                        strOutput.AppendFormat("Audio Streams: {0}{1}", miFI.StreamDetails.Audio.Count.ToString, vbNewLine)
                    End If

                    If miFI.StreamDetails.Subtitle.Count > 0 Then
                        strOutput.AppendFormat("Subtitle Streams: {0}{1}", miFI.StreamDetails.Subtitle.Count.ToString, vbNewLine)
                    End If

                    For Each miVideo As MediaInfo.Video In miFI.StreamDetails.Video
                        strOutput.AppendFormat("{0}Video Stream {1}{0}", vbNewLine, iVS)
                        If Not String.IsNullOrEmpty(miVideo.Width) AndAlso Not String.IsNullOrEmpty(miVideo.Height) Then
                            strOutput.AppendFormat("- Size: {0}x{1}{2}", miVideo.Width, miVideo.Height, vbNewLine)
                        End If
                        If Not String.IsNullOrEmpty(miVideo.Aspect) Then strOutput.AppendFormat("- Display Aspect Ratio: {0}{1}", miVideo.Aspect, vbNewLine)
                        If Not String.IsNullOrEmpty(miVideo.Scantype) Then strOutput.AppendFormat("- Scan Type: {0}{1}", miVideo.Scantype, vbNewLine)
                        If Not String.IsNullOrEmpty(miVideo.CodecID) Then strOutput.AppendFormat("- Codec ID: {0}{1}", miVideo.CodecID, vbNewLine)
                        If Not String.IsNullOrEmpty(miVideo.Codec) Then strOutput.AppendFormat("- Codec: {0}{1}", miVideo.Codec, vbNewLine)
                        If Not String.IsNullOrEmpty(miVideo.Duration) Then strOutput.AppendFormat("- Duration: {0}{1}", miVideo.Duration, vbNewLine)
                        iVS += 1
                    Next

                    For Each miAudio As MediaInfo.Audio In miFI.StreamDetails.Audio
                        'audio
                        strOutput.AppendFormat("{0}Audio Stream {1}{0}", vbNewLine, iAS.ToString)
                        If Not String.IsNullOrEmpty(miAudio.CodecID) Then strOutput.AppendFormat("- Codec ID: {0}{1}", miAudio.CodecID, vbNewLine)
                        If Not String.IsNullOrEmpty(miAudio.Codec) Then strOutput.AppendFormat("- Codec: {0}{1}", miAudio.Codec, vbNewLine)
                        If Not String.IsNullOrEmpty(miAudio.Channels) Then strOutput.AppendFormat("- Channels: {0}{1}", miAudio.Channels, vbNewLine)
                        If Not String.IsNullOrEmpty(miAudio.LongLanguage) Then strOutput.AppendFormat("- Language: {0}{1}", miAudio.LongLanguage, vbNewLine)
                        iAS += 1
                    Next

                    For Each miSub As MediaInfo.Subtitle In miFI.StreamDetails.Subtitle
                        'subtitles
                        strOutput.AppendFormat("{0}Subtitle {1}{0}", vbNewLine, iSS.ToString)
                        If Not String.IsNullOrEmpty(miSub.LongLanguage) Then strOutput.AppendFormat("- Language: {0}", miSub.LongLanguage)
                        iSS += 1
                    Next
                End If
            End If
        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        If strOutput.Length > 0 Then
            Return strOutput.ToString
        Else
            Return "Media Info is not available for this movie. Try rescanning."
        End If
    End Function

    Public Shared Function GetBestVideo(ByRef miFIV As MediaInfo.Fileinfo) As MediaInfo.Video

        '//
        ' Get the highest values from file info
        '\\

        Dim fivOut As New MediaInfo.Video
        Try
            Dim iWidest As Integer = 0
            Dim iWidth As Integer = 0

            'set some defaults to make it easy on ourselves
            fivOut.Width = String.Empty
            fivOut.Height = String.Empty
            fivOut.Aspect = String.Empty
            fivOut.Codec = String.Empty
            fivOut.Duration = String.Empty
            fivOut.Scantype = String.Empty

            For Each miVideo As MediaInfo.Video In miFIV.StreamDetails.Video
                If Not String.IsNullOrEmpty(miVideo.Width) Then
                    iWidth = Convert.ToInt32(miVideo.Width)
                    If iWidth > iWidest Then
                        iWidest = iWidth
                        fivOut.Width = miVideo.Width
                        fivOut.Height = miVideo.Height
                        fivOut.Aspect = miVideo.Aspect
                        fivOut.Codec = miVideo.Codec
                        fivOut.CodecID = miVideo.CodecID
                        fivOut.Duration = miVideo.Duration
                        fivOut.Scantype = miVideo.Scantype
                    End If
                End If
            Next

        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return fivOut

    End Function
    Public Shared Function GetBestAudio(ByRef miFIA As MediaInfo.Fileinfo) As MediaInfo.Audio

        '//
        ' Get the highest values from file info
        '\\

        Dim fiaOut As New MediaInfo.Audio
        Try
            Dim sinMostChannels As Single = 0
            Dim sinChans As Single = 0

            fiaOut.Codec = String.Empty
            fiaOut.Channels = String.Empty
            fiaOut.Language = String.Empty

            For Each miAudio As MediaInfo.Audio In miFIA.StreamDetails.Audio
                If Not String.IsNullOrEmpty(miAudio.Channels) Then
                    sinChans = ConvertToSingle(miAudio.Channels)
                    If sinChans > sinMostChannels Then
                        sinMostChannels = sinChans
                        fiaOut.Codec = miAudio.Codec
                        fiaOut.CodecID = miAudio.CodecID
                        fiaOut.Channels = sinChans
                        fiaOut.Language = miAudio.Language
                    End If
                End If

            Next

        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return fiaOut
    End Function

    Public Shared Function GetResFromDimensions(ByVal fiRes As MediaInfo.Video) As String

        '//
        ' Get the resolution of the video from the dimensions provided by MediaInfo.dll
        '\\

        Dim resOut As String = String.Empty
        Try
            If Not String.IsNullOrEmpty(fiRes.Width) AndAlso Not String.IsNullOrEmpty(fiRes.Height) AndAlso Not String.IsNullOrEmpty(fiRes.Aspect) Then
                Dim iWidth As Integer = fiRes.Width
                Dim iHeight As Integer = fiRes.Height
                Dim sinADR As Single = ConvertToSingle(fiRes.Aspect)

                Select Case True
                    Case iWidth < 640
                        resOut = "SD"
                        'exact
                    Case (iWidth = 1920 AndAlso (iHeight = 1080 OrElse iHeight = 800)) OrElse (iWidth = 1440 AndAlso iHeight = 1080) OrElse (iWidth = 1280 AndAlso iHeight = 1080)
                        resOut = "1080"
                    Case (iWidth = 1366 AndAlso iHeight = 768) OrElse (iWidth = 1024 AndAlso iHeight = 768)
                        resOut = "768"
                    Case (iWidth = 960 AndAlso iHeight = 720) OrElse (iWidth = 1280 AndAlso (iHeight = 720 OrElse iHeight = 544))
                        resOut = "720"
                    Case (iWidth = 1024 AndAlso iHeight = 576) OrElse (iWidth = 720 AndAlso iHeight = 576)
                        resOut = "576"
                    Case (iWidth = 720 OrElse iWidth = 960) AndAlso iHeight = 540
                        resOut = "540"
                    Case (iWidth = 852 OrElse iWidth = 720 OrElse iWidth = 704 OrElse iWidth = 640) AndAlso iHeight = 480
                        resOut = "480"
                        'by ADR
                    Case sinADR >= 1.4 AndAlso iWidth = 1920
                        resOut = "1080"
                    Case sinADR >= 1.4 AndAlso iWidth = 1366
                        resOut = "768"
                    Case sinADR >= 1.4 AndAlso iWidth = 1280
                        resOut = "720"
                    Case sinADR >= 1.4 AndAlso iWidth = 1024
                        resOut = "576"
                    Case sinADR >= 1.4 AndAlso iWidth = 960
                        resOut = "540"
                    Case sinADR >= 1.4 AndAlso iWidth = 852
                        resOut = "480"
                        'loose
                    Case iWidth >= 1200 AndAlso iHeight >= 768
                        resOut = "1080"
                    Case iWidth >= 1000 AndAlso iHeight >= 720
                        resOut = "768"
                    Case iWidth >= 1024 AndAlso iHeight >= 500
                        resOut = "720"
                    Case iWidth >= 700 AndAlso iHeight >= 540
                        resOut = "576"
                    Case iWidth >= 700 AndAlso iHeight >= 480
                        resOut = "540"
                    Case Else
                        resOut = "480"
                End Select
            End If
        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        If Not String.IsNullOrEmpty(resOut) Then
            If String.IsNullOrEmpty(fiRes.Scantype) Then
                Return String.Concat(resOut)
            Else
                Return String.Concat(resOut, If(fiRes.Scantype.ToLower = "progressive", "p", "i"))
            End If
        Else
            Return String.Empty
        End If
    End Function

    Public Shared Function TruncateURL(ByVal sString As String, ByVal MaxLength As Integer) As String

        '//
        ' Shorten a URL to fit on the GUI
        '\\

        Try
            Dim sEnd As String = sString.Substring(sString.LastIndexOf("/"), sString.Length - sString.LastIndexOf("/"))
            If ((MaxLength - sEnd.Length) - 3) > 0 Then
                Return String.Format("{0}...{1}", Strings.Left(sString, (MaxLength - sEnd.Length) - 3), sEnd)
            Else
                If sEnd.Length >= MaxLength Then
                    Return String.Format("...{0}", Strings.Right(sEnd, MaxLength - 3))
                Else
                    Return sEnd
                End If
            End If
        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return String.Empty
    End Function

    Public Shared Function GetNfoPath(ByVal sPath As String, ByVal isSingle As Boolean) As String

        '//
        ' Get the proper path to NFO
        '\\

        Dim nPath As String = String.Empty

        If Master.eSettings.VideoTSParent AndAlso Directory.GetParent(sPath).Name.ToLower = "video_ts" Then
            nPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName, Directory.GetParent(Directory.GetParent(sPath).FullName).Name), ".nfo")
            If File.Exists(nPath) Then
                Return nPath
            Else
                If isSingle Then
                    'return movie path so we can use it for looking for non-conforming nfos
                    Return sPath
                Else
                    Return String.Empty
                End If
            End If
        Else
            Dim tmpName As String = CleanStackingMarkers(Path.GetFileNameWithoutExtension(sPath))
            Dim tmpNameNoStack As String = Path.GetFileNameWithoutExtension(sPath)
            nPath = Path.Combine(Directory.GetParent(sPath).FullName, tmpName)
            Dim nPathWithStack As String = Path.Combine(Directory.GetParent(sPath).FullName, tmpNameNoStack)
            If eSettings.MovieNameNFO AndAlso File.Exists(String.Concat(nPathWithStack, ".nfo")) Then
                Return String.Concat(nPathWithStack, ".nfo")
            ElseIf eSettings.MovieNameNFO AndAlso File.Exists(String.Concat(nPath, ".nfo")) Then
                Return String.Concat(nPath, ".nfo")
            ElseIf eSettings.MovieNFO AndAlso File.Exists(Path.Combine(Directory.GetParent(sPath).FullName, "movie.nfo")) Then
                Return Path.Combine(Directory.GetParent(nPath).FullName, "movie.nfo")
            Else
                If isSingle Then
                    'return movie path so we can use it for looking for non-conforming nfos
                    Return sPath
                Else
                    Return String.Empty
                End If
            End If
        End If

    End Function

    Public Shared Sub SaveMovieToNFO(ByVal movieToSave As Media.Movie, ByVal sPath As String, ByVal isSingle As Boolean)

        '//
        ' Serialize Media.Movie to an NFO
        '\\

        Try

            Dim xmlSer As New XmlSerializer(GetType(Media.Movie))
            Dim tPath As String = String.Empty
            Dim nPath As String = String.Empty

            If Master.eSettings.VideoTSParent AndAlso Directory.GetParent(sPath).Name.ToLower = "video_ts" Then
                nPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName, Directory.GetParent(Directory.GetParent(sPath).FullName).Name), ".nfo")

                If Not eSettings.OverwriteNfo Then
                    RenameNonConfNfo(nPath)
                End If

                If Not File.Exists(nPath) OrElse (Not CBool(File.GetAttributes(nPath) And FileAttributes.ReadOnly)) Then
                    Using xmlSW As New StreamWriter(nPath)
                        xmlSer.Serialize(xmlSW, movieToSave)
                    End Using
                End If
            Else
                Dim tmpName As String = Path.GetFileNameWithoutExtension(sPath)
                nPath = Path.Combine(Directory.GetParent(sPath).FullName, tmpName)

                If eSettings.MovieNameNFO OrElse Not isSingle Then
                    If Directory.GetParent(sPath).Name.ToLower = "video_ts" Then
                        tPath = Path.Combine(Directory.GetParent(sPath).FullName, "video_ts.nfo")
                    Else
                        tPath = String.Concat(nPath, ".nfo")
                    End If

                    If Not eSettings.OverwriteNfo Then
                        RenameNonConfNfo(tPath)
                    End If

                    If Not File.Exists(tPath) OrElse (Not CBool(File.GetAttributes(tPath) And FileAttributes.ReadOnly)) Then
                        Using xmlSW As New StreamWriter(tPath)
                            xmlSer.Serialize(xmlSW, movieToSave)
                        End Using
                    End If
                End If

                If isSingle AndAlso eSettings.MovieNFO Then
                    tPath = Path.Combine(Directory.GetParent(nPath).FullName, "movie.nfo")

                    If Not eSettings.OverwriteNfo Then
                        RenameNonConfNfo(tPath)
                    End If

                    If Not File.Exists(tPath) OrElse (Not CBool(File.GetAttributes(tPath) And FileAttributes.ReadOnly)) Then
                        Using xmlSW As New StreamWriter(tPath)
                            xmlSer.Serialize(xmlSW, movieToSave)
                        End Using
                    End If
                End If
            End If

        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Shared Sub RenameNonConfNfo(ByVal sPath As String)
        'test if current nfo is non-conforming... rename per setting

        Try
            If Not IsConformingNfo(sPath) Then
                If File.Exists(sPath) Then
                    Dim i As Integer = 1
                    Dim strNewName As String = String.Concat(RemoveExtFromPath(sPath), ".info")
                    'in case there is already a .info file
                    If File.Exists(strNewName) Then
                        Do
                            strNewName = String.Format("{0}({1}).info", RemoveExtFromPath(sPath), i)
                            i += 1
                        Loop While File.Exists(strNewName)
                        strNewName = String.Format("{0}({1}).info", RemoveExtFromPath(sPath), i)
                    End If
                    My.Computer.FileSystem.RenameFile(sPath, Path.GetFileName(strNewName))
                End If
            End If
        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Shared Function IsConformingNfo(ByVal sPath As String) As Boolean
        Dim testSer As XmlSerializer = Nothing

        Try
            If File.Exists(sPath) AndAlso Not Master.eSettings.ValidExts.Contains(Path.GetExtension(sPath).ToLower) Then
                Using testSR As StreamReader = New StreamReader(sPath)
                    testSer = New XmlSerializer(GetType(Media.Movie))
                    Dim testMovie As Media.Movie = CType(testSer.Deserialize(testSR), Media.Movie)
                    testMovie = Nothing
                    testSer = Nothing
                End Using
                Return True
            Else
                Return False
            End If
        Catch
            If Not IsNothing(testSer) Then
                testSer = Nothing
            End If

            Return False
        End Try
    End Function

    Public Shared Function GetTrailerPath(ByVal sPath As String) As String

        '//
        ' Get the proper path to trailer
        '\\

        Dim tFile As String = String.Empty

        Dim parPath As String = Directory.GetParent(sPath).FullName
        Dim tmpName As String = Path.Combine(parPath, CleanStackingMarkers(Path.GetFileNameWithoutExtension(sPath)))
        Dim tmpNameNoStack As String = Path.Combine(parPath, Path.GetFileNameWithoutExtension(sPath))
        For Each t As String In Master.eSettings.ValidExts
            If File.Exists(String.Concat(tmpName, "-trailer", t)) Then
                tFile = String.Concat(tmpName, "-trailer", t)
                Exit For
            ElseIf File.Exists(String.Concat(tmpName, "[trailer]", t)) Then
                tFile = String.Concat(tmpName, "[trailer]", t)
                Exit For
            ElseIf File.Exists(String.Concat(tmpNameNoStack, "-trailer", t)) Then
                tFile = String.Concat(tmpNameNoStack, "-trailer", t)
                Exit For
            ElseIf File.Exists(String.Concat(tmpNameNoStack, "[trailer]", t)) Then
                tFile = String.Concat(tmpNameNoStack, "[trailer]", t)
                Exit For
            End If
        Next

        Return tFile

    End Function

    Public Shared Function SortFileNames(ByVal x As FileInfo, ByVal y As FileInfo) As Integer

        Try
            If String.IsNullOrEmpty(x.Name) Then
                Return -1
            End If
            If String.IsNullOrEmpty(y.Name) Then
                Return 1
            End If

            Return x.Name.CompareTo(y.Name)
        Catch
            Return 0
        End Try

    End Function

    Public Shared Function SortThumbFileNames(ByVal x As FileInfo, ByVal y As FileInfo) As Integer

        Try
            Dim ObjectCompare As New CaseInsensitiveComparer

            If String.IsNullOrEmpty(x.Name) Then
                Return -1
            End If
            If String.IsNullOrEmpty(y.Name) Then
                Return 1
            End If

            Return ObjectCompare.Compare(Convert.ToInt32(Regex.Match(x.Name, "(\d+)").Groups(0).ToString), Convert.ToInt32(Regex.Match(y.Name, "(\d+)").Groups(0).ToString))
        Catch
            Return 0
        End Try

    End Function

    Public Shared Function GetExtraModifier(ByVal sPath As String) As Integer

        '//
        ' Get the number of the last thumb#.jpg to make sure we're not overwriting current ones
        '\\

        Dim iMod As Integer = 0
        Dim alThumbs As New ArrayList

        Try
            Dim extraPath As String = Path.Combine(Directory.GetParent(sPath).FullName, "extrathumbs")

            If Not Directory.Exists(extraPath) Then
                iMod = -1
            Else
                Dim dirInfo As New DirectoryInfo(extraPath)
                Dim ioFi As New List(Of FileInfo)

                Try
                    ioFi.AddRange(dirInfo.GetFiles("thumb*.jpg"))
                Catch
                End Try

                For Each sFile As FileInfo In ioFi
                    alThumbs.Add(sFile.Name)
                Next

                ioFi = Nothing

                If alThumbs.Count > 0 Then
                    alThumbs.Sort()
                    iMod = Convert.ToInt32(Regex.Match(alThumbs.Item(alThumbs.Count - 1), "\d+").ToString)
                Else
                    iMod = -1
                End If
            End If

            alThumbs = Nothing
        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return iMod
    End Function

    Public Shared Sub MoveFileWithStream(ByVal sPathFrom As String, ByVal sPathTo As String)

        '//
        ' Copy a file from one location to another using a stream/buffer
        '\\

        Try
            Using SourceStream As FileStream = New FileStream(sPathFrom, FileMode.Open, FileAccess.Read)
                Using DestinationStream As FileStream = New FileStream(sPathTo, FileMode.OpenOrCreate, FileAccess.Write)
                    Dim StreamBuffer(SourceStream.Length - 1) As Byte

                    SourceStream.Read(StreamBuffer, 0, StreamBuffer.Length)
                    DestinationStream.Write(StreamBuffer, 0, StreamBuffer.Length)

                    StreamBuffer = Nothing
                End Using
            End Using
        Catch ex As Exception
            eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    Public Shared Function NumericOnly(ByVal KeyPressed As String, Optional ByVal isIP As Boolean = False) As Boolean
        If (KeyPressed >= 48 AndAlso KeyPressed <= 57) OrElse KeyPressed = 8 OrElse (isIP AndAlso KeyPressed = 46) Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Shared Function Encode(ByVal decText As String) As String

        Dim eByte() As Byte
        ReDim eByte(decText.Length)
        eByte = System.Text.Encoding.ASCII.GetBytes(decText)
        Dim encText As String
        encText = System.Convert.ToBase64String(eByte)
        Return encText

    End Function

    Public Shared Function Decode(ByVal encText As String) As String

        Dim dByte() As Byte
        dByte = System.Convert.FromBase64String(encText)
        Dim decText As String
        decText = System.Text.Encoding.ASCII.GetString(dByte)
        Return decText

    End Function

    Public Shared Function ConvertToSingle(ByVal sNumber As String) As Single
        If String.IsNullOrEmpty(sNumber) Then Return 0
        Dim numFormat As NumberFormatInfo = New NumberFormatInfo()
        numFormat.NumberDecimalSeparator = "."
        Return Double.Parse(sNumber.Replace(",", "."), NumberStyles.AllowDecimalPoint, numFormat)
    End Function

    Public Shared Sub DeleteFiles(ByVal isCleaner As Boolean, ByVal sPath As String, ByVal isSingle As Boolean)
        Dim dPath As String = String.Empty

        If eSettings.VideoTSParent AndAlso Directory.GetParent(sPath).Name.ToLower = "video_ts" Then
            dPath = String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName, Directory.GetParent(Directory.GetParent(sPath).FullName).Name), ".ext")
        Else
            dPath = sPath
        End If

        Dim sOrName As String = CleanStackingMarkers(Path.GetFileNameWithoutExtension(dPath))
        Dim sPathShort As String = Directory.GetParent(dPath).FullName
        Dim sPathNoExt As String = RemoveExtFromPath(dPath)

        If isCleaner And Master.eSettings.ExpertCleaner Then
            Dim dirInfo As New DirectoryInfo(sPathShort)
            Dim ioFi As New List(Of FileInfo)

            Try
                ioFi.AddRange(dirInfo.GetFiles())
            Catch
            End Try

            For Each sFile As FileInfo In ioFi
                Dim test As String = sFile.Extension
                If Not Master.eSettings.CleanWhitelistExts.Contains(sFile.Extension) AndAlso ((Master.eSettings.CleanWhitelistVideo AndAlso Not Master.eSettings.ValidExts.Contains(sFile.Extension)) OrElse Not Master.eSettings.CleanWhitelistVideo) Then
                    File.Delete(sFile.FullName)
                End If
            Next

            ioFi = Nothing
            dirInfo = Nothing
        Else
            If Not isCleaner Then
                Dim Fanart As New Images
                Dim fPath As String = Fanart.GetFanartPath(sPath, isSingle)
                Dim tPath As String = String.Empty
                If Not String.IsNullOrEmpty(fPath) Then
                    If Directory.GetParent(fPath).Name.ToLower = "video_ts" Then
                        If Master.eSettings.VideoTSParent Then
                            tPath = Path.Combine(Master.eSettings.BDPath, String.Concat(Path.Combine(Directory.GetParent(Directory.GetParent(fPath).FullName).FullName, Directory.GetParent(Directory.GetParent(fPath).FullName).Name), "-fanart.jpg"))
                        Else
                            If Path.GetFileName(fPath).ToLower = "fanart.jpg" Then
                                tPath = Path.Combine(Master.eSettings.BDPath, String.Concat(Directory.GetParent(Directory.GetParent(fPath).FullName).Name, "-fanart.jpg"))
                            Else
                                tPath = Path.Combine(Master.eSettings.BDPath, Path.GetFileName(fPath))
                            End If
                        End If
                    Else
                        If Path.GetFileName(fPath).ToLower = "fanart.jpg" Then
                            tPath = Path.Combine(Master.eSettings.BDPath, String.Concat(Path.GetFileNameWithoutExtension(sPath), "-fanart.jpg"))
                        Else
                            tPath = Path.Combine(Master.eSettings.BDPath, Path.GetFileName(fPath))
                        End If
                    End If
                End If
                If Not String.IsNullOrEmpty(tPath) AndAlso File.Exists(tPath) Then
                    File.Delete(tPath)
                End If
                Fanart = Nothing
            End If

            If Not isCleaner AndAlso isSingle Then
                If Directory.GetParent(sPath).Name.ToLower = "video_ts" Then
                    Directory.Delete(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName, True)
                Else
                    Directory.Delete(Directory.GetParent(sPath).FullName, True)
                End If
            Else
                If (Master.eSettings.CleanFolderJPG AndAlso isCleaner) Then
                    If File.Exists(Path.Combine(sPathShort, "folder.jpg")) Then
                        File.Delete(Path.Combine(sPathShort, "folder.jpg"))
                    End If
                End If

                If (Master.eSettings.CleanFanartJPG AndAlso isCleaner) Then
                    If File.Exists(Path.Combine(sPathShort, "fanart.jpg")) Then
                        File.Delete(Path.Combine(sPathShort, "fanart.jpg"))
                    End If
                End If

                If (Master.eSettings.CleanMovieTBN AndAlso isCleaner) Then
                    If File.Exists(Path.Combine(sPathShort, "movie.tbn")) Then
                        File.Delete(Path.Combine(sPathShort, "movie.tbn"))
                    End If
                End If

                If (Master.eSettings.CleanMovieNFO AndAlso isCleaner) Then
                    If File.Exists(Path.Combine(sPathShort, "movie.nfo")) Then
                        File.Delete(Path.Combine(sPathShort, "movie.nfo"))
                    End If
                End If

                If (Master.eSettings.CleanPosterTBN AndAlso isCleaner) Then
                    If File.Exists(Path.Combine(sPathShort, "poster.tbn")) Then
                        File.Delete(Path.Combine(sPathShort, "poster.tbn"))
                    End If
                End If

                If (Master.eSettings.CleanPosterJPG AndAlso isCleaner) Then
                    If File.Exists(Path.Combine(sPathShort, "poster.jpg")) Then
                        File.Delete(Path.Combine(sPathShort, "poster.jpg"))
                    End If
                End If

                If (Master.eSettings.CleanMovieJPG AndAlso isCleaner) Then
                    If File.Exists(Path.Combine(sPathShort, "movie.jpg")) Then
                        File.Delete(Path.Combine(sPathShort, "movie.jpg"))
                    End If
                End If

                If (Master.eSettings.CleanExtraThumbs AndAlso isCleaner) Then
                    If Directory.Exists(Path.Combine(sPathShort, "extrathumbs")) Then
                        Directory.Delete(Path.Combine(sPathShort, "extrathumbs"), True)
                    End If
                End If

                If (Master.eSettings.CleanMovieTBNB AndAlso isCleaner) OrElse (Not isCleaner) Then
                    If File.Exists(String.Concat(sPathNoExt, ".tbn")) Then
                        File.Delete(String.Concat(sPathNoExt, ".tbn"))
                    End If
                    If File.Exists(Path.Combine(sPathShort, "video_ts.tbn")) Then
                        File.Delete(Path.Combine(sPathShort, "video_ts.tbn"))
                    End If
                    If File.Exists(String.Concat(Path.Combine(sPathShort, sOrName), ".tbn")) Then
                        File.Delete(String.Concat(Path.Combine(sPathShort, sOrName), ".tbn"))
                    End If
                End If

                If (Master.eSettings.CleanMovieFanartJPG AndAlso isCleaner) OrElse (Not isCleaner) Then
                    If File.Exists(String.Concat(sPathNoExt, "-fanart.jpg")) Then
                        File.Delete(String.Concat(sPathNoExt, "-fanart.jpg"))
                    End If
                    If File.Exists(Path.Combine(sPathShort, "video_ts-fanart.jpg")) Then
                        File.Delete(Path.Combine(sPathShort, "video_ts-fanart.jpg"))
                    End If
                    If File.Exists(String.Concat(Path.Combine(sPathShort, sOrName), "-fanart.jpg")) Then
                        File.Delete(String.Concat(Path.Combine(sPathShort, sOrName), "-fanart.jpg"))
                    End If
                End If

                If (Master.eSettings.CleanMovieNFOB AndAlso isCleaner) OrElse (Not isCleaner) Then
                    If File.Exists(String.Concat(sPathNoExt, ".nfo")) Then
                        File.Delete(String.Concat(sPathNoExt, ".nfo"))
                    End If
                    If File.Exists(Path.Combine(sPathShort, "video_ts.nfo")) Then
                        File.Delete(Path.Combine(sPathShort, "video_ts.nfo"))
                    End If
                    If File.Exists(String.Concat(Path.Combine(sPathShort, sOrName), ".nfo")) Then
                        File.Delete(String.Concat(Path.Combine(sPathShort, sOrName), ".nfo"))
                    End If
                End If

                If (Master.eSettings.CleanDotFanartJPG AndAlso isCleaner) OrElse (Not isCleaner) Then
                    If File.Exists(String.Concat(sPathNoExt, ".fanart.jpg")) Then
                        File.Delete(String.Concat(sPathNoExt, ".fanart.jpg"))
                    End If
                    If File.Exists(Path.Combine(sPathShort, "video_ts.fanart.jpg")) Then
                        File.Delete(Path.Combine(sPathShort, "video_ts.fanart.jpg"))
                    End If
                    If File.Exists(String.Concat(Path.Combine(sPathShort, sOrName), ".fanart.jpg")) Then
                        File.Delete(String.Concat(Path.Combine(sPathShort, sOrName), ".fanart.jpg"))
                    End If
                End If

                If (Master.eSettings.CleanMovieNameJPG AndAlso isCleaner) OrElse (Not isCleaner) Then
                    If File.Exists(String.Concat(sPathNoExt, ".jpg")) Then
                        File.Delete(String.Concat(sPathNoExt, ".jpg"))
                    End If
                    If File.Exists(Path.Combine(sPathShort, "video_ts.jpg")) Then
                        File.Delete(Path.Combine(sPathShort, "video_ts.jpg"))
                    End If
                    If File.Exists(String.Concat(Path.Combine(sPathShort, sOrName), ".jpg")) Then
                        File.Delete(String.Concat(Path.Combine(sPathShort, sOrName), ".jpg"))
                    End If
                End If

                If Not isCleaner Then
                    Dim dirInfo As New DirectoryInfo(sPathShort)

                    Dim ioFi As New List(Of FileInfo)

                    Try
                        ioFi.AddRange(dirInfo.GetFiles(String.Concat(sOrName, "*.*")))
                    Catch
                    End Try

                    Try
                        ioFi.AddRange(dirInfo.GetFiles(String.Concat(Path.GetFileNameWithoutExtension(sPath), ".*")))
                    Catch
                    End Try

                    For Each sFile As FileInfo In ioFi
                        File.Delete(sFile.FullName)
                    Next

                    ioFi = Nothing
                    dirInfo = Nothing
                End If
            End If
        End If
    End Sub

    Public Shared Function CreateRandomThumbs(ByVal sPath As String, ByVal ThumbCount As Integer) As Boolean

        Dim didSetFA As Boolean = False

        Try
            Dim pExt As String = Path.GetExtension(sPath).ToLower
            If Not pExt = ".rar" AndAlso Not pExt = ".iso" AndAlso Not pExt = ".img" AndAlso _
            Not pExt = ".bin" AndAlso Not pExt = ".cue" Then

                Using ffmpeg As New Process()
                    Dim intSeconds As Integer = 0
                    Dim intAdd As Integer = 0
                    Dim tPath As String = String.Empty
                    Dim exImage As New Images

                    If Master.eSettings.VideoTSParent AndAlso Directory.GetParent(sPath).Name.ToLower = "video_ts" Then
                        tPath = Path.Combine(Directory.GetParent(Directory.GetParent(sPath).FullName).FullName, "extrathumbs")
                    Else
                        tPath = Path.Combine(Directory.GetParent(sPath).FullName, "extrathumbs")
                    End If

                    If Not Directory.Exists(tPath) Then
                        Directory.CreateDirectory(tPath)
                    End If

                    ffmpeg.StartInfo.FileName = String.Concat(Application.StartupPath, Path.DirectorySeparatorChar, "Bin", Path.DirectorySeparatorChar, "ffmpeg.exe")
                    ffmpeg.EnableRaisingEvents = False
                    ffmpeg.StartInfo.UseShellExecute = False
                    ffmpeg.StartInfo.CreateNoWindow = True
                    ffmpeg.StartInfo.RedirectStandardOutput = True
                    ffmpeg.StartInfo.RedirectStandardError = True

                    'first get the duration
                    ffmpeg.StartInfo.Arguments = String.Format("-i ""{0}"" -an", sPath)
                    ffmpeg.Start()
                    Dim d As StreamReader = ffmpeg.StandardError
                    Do
                        Dim s As String = d.ReadLine()
                        If s.Contains("Duration: ") Then
                            Dim sTime As String = Regex.Match(s, "Duration: (?<dur>.*?),").Groups("dur").ToString
                            Dim ts As TimeSpan = CDate(CDate(String.Format("{0} {1}", DateTime.Today.ToString("d"), sTime))).Subtract(CDate(DateTime.Today))
                            intSeconds = ((ts.Hours * 60) + ts.Minutes) * 60 + ts.Seconds
                        End If
                    Loop While Not d.EndOfStream

                    ffmpeg.WaitForExit()
                    ffmpeg.Close()

                    If intSeconds > 0 AndAlso ((Master.eSettings.AutoThumbsNoSpoilers AndAlso intSeconds / 2 > ThumbCount + 300) OrElse (Not Master.eSettings.AutoThumbsNoSpoilers AndAlso intSeconds > ThumbCount + 2)) Then
                        If Master.eSettings.AutoThumbsNoSpoilers Then
                            intSeconds = ((intSeconds / 2) - 300) / ThumbCount
                            intAdd = intSeconds
                            intSeconds += intAdd + 300
                        Else
                            intSeconds = intSeconds / (ThumbCount + 2)
                            intAdd = intSeconds
                            intSeconds += intAdd
                        End If

                        For i = 0 To (ThumbCount - 1)
                            'check to see if file already exists... if so, don't bother running ffmpeg since we're not
                            'overwriting current thumbs anyway
                            If Not File.Exists(Path.Combine(tPath, String.Concat("thumb", (i + 1), ".jpg"))) Then
                                ffmpeg.StartInfo.Arguments = String.Format("-ss {0} -i ""{1}"" -an -f rawvideo -vframes 1 -vcodec mjpeg ""{2}""", intSeconds, sPath, Path.Combine(tPath, String.Concat("thumb", (i + 1), ".jpg")))
                                ffmpeg.Start()
                                ffmpeg.WaitForExit()
                                ffmpeg.Close()

                                exImage = New Images
                                exImage.ResizeExtraThumb(Path.Combine(tPath, String.Concat("thumb", (i + 1), ".jpg")), Path.Combine(tPath, String.Concat("thumb", (i + 1), ".jpg")))
                                exImage.Dispose()
                                exImage = Nothing

                            End If
                            intSeconds += intAdd
                        Next
                    End If

                    Dim fThumbs As New ArrayList

                    Try
                        fThumbs.AddRange(Directory.GetFiles(tPath, "thumb*.jpg"))
                    Catch
                    End Try

                    If fThumbs.Count <= 0 Then
                        Directory.Delete(tPath, True)
                    Else
                        Dim exFanart As New Images
                        If Master.eSettings.UseETasFA AndAlso Not File.Exists(exFanart.GetFanartPath(sPath, False)) Then
                            exFanart.FromFile(Path.Combine(tPath, "thumb1.jpg"))
                            exFanart.SaveAsFanart(sPath, False)
                            didSetFA = True
                        End If
                        exFanart.Dispose()
                        exFanart = Nothing
                    End If

                End Using

            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

        Return didsetfa
    End Function

    Public Shared Function CheckUpdate() As Integer
        Try
            Dim sHTTP As New HTTP
            Dim updateXML As String = sHTTP.DownloadData("http://www.cube3studios.com/EMM/Update.xml")
            sHTTP = Nothing

            Dim xmlUpdate As XDocument
            Try
                xmlUpdate = XDocument.Parse(updateXML)
            Catch
                Return 0
            End Try

            Dim xUdpate = From xUp In xmlUpdate...<version> Select xUp.@current
            If xUdpate.Count > 0 Then
                Return Convert.ToInt32(xUdpate(0))
            Else
                Return 0
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            Return 0
        End Try
    End Function

    Public Shared Function GetChangelog() As String
        Dim sHTTP As New HTTP
        Dim strChangelog As String = sHTTP.DownloadData("http://www.cube3studios.com/EMM/Changelog.txt")
        sHTTP = Nothing

        If strChangelog.Length > 0 Then
            Return strChangelog
        Else
            Return "Unavailable"
        End If
    End Function

    Public Shared Function GetNETVersion() As Boolean
        Const regLocation As String = "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP"
        Dim masterKey As RegistryKey = Registry.LocalMachine.OpenSubKey(regLocation)
        Dim tempKey As RegistryKey
        Dim sVersion As String = String.Empty

        If Not IsNothing(masterKey) Then
            Dim SubKeyNames As String() = masterKey.GetSubKeyNames()
            For i As Integer = 0 To SubKeyNames.Count - 1
                tempKey = Registry.LocalMachine.OpenSubKey(String.Concat(regLocation, "\\", SubKeyNames(i)))
                sVersion = tempKey.GetValue("Version")
                If Not String.IsNullOrEmpty(sVersion) Then
                    Dim tVersion() As String = sVersion.Split(New Char() {"."})
                    If tVersion(0) >= 3 AndAlso tVersion(1) >= 5 Then
                        Return True
                    End If
                End If
            Next
        End If

        Return False
    End Function

    Public Shared Function CleanURL(ByVal sURL As String, Optional ByVal unClean As Boolean = False)
        If unClean Then
            Return sURL.Replace("$c$", ":").Replace("$s$", "/")
        Else
            Return sURL.Replace(":", "$c$").Replace("/", "$s$")
        End If
    End Function

    Public Shared Sub DrawGradEllipse(ByRef graphics As Graphics, ByVal bounds As Rectangle, ByVal color1 As Color, ByVal color2 As Color)
        Dim rPoints() As Point = { _
            New Point(bounds.X, bounds.Y), _
            New Point(bounds.X + bounds.Width, bounds.Y), _
            New Point(bounds.X + bounds.Width, bounds.Y + bounds.Height), _
            New Point(bounds.X, bounds.Y + bounds.Height) _
        }
        Dim pgBrush As New Drawing2D.PathGradientBrush(rPoints)
        Dim gPath As New Drawing2D.GraphicsPath
        gPath.AddEllipse(bounds.X, bounds.Y, bounds.Width, bounds.Height)
        pgBrush = New Drawing2D.PathGradientBrush(gPath)
        pgBrush.CenterColor = color1
        pgBrush.SurroundColors = New Color() {color2}
        graphics.FillEllipse(pgBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height)
    End Sub
    Public Shared Function LoadMovieFromDB(ByVal id As Integer) As DBMovie
        Dim _movieDB As New DBMovie
        _movieDB.FaS = New Master.FileAndSource
        Try
            _movieDB.ID = id
            Using SQLcommand As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                SQLcommand.CommandText = String.Concat("SELECT * FROM movies WHERE id = ", _movieDB.ID, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    _movieDB.FaS.Fanart = SQLreader("FanartPath")
                    _movieDB.FaS.Poster = SQLreader("PosterPath")
                    _movieDB.FaS.Trailer = SQLreader("TrailerPath")
                    _movieDB.FaS.Nfo = SQLreader("NfoPath")
                    _movieDB.Movie = New Media.Movie
                    With _movieDB.Movie
                        .Clear()
                        .ID = SQLreader("IMDB")
                        .Title = SQLreader("Title")
                        .OriginalTitle = SQLreader("OriginalTitle")
                        .Year = SQLreader("Year")
                        .Rating = SQLreader("Rating")
                        .Votes = SQLreader("Votes")
                        .MPAA = SQLreader("MPAA")
                        .Top250 = SQLreader("Top250")
                        .Outline = SQLreader("Outline")
                        .Plot = SQLreader("Plot")
                        .Tagline = SQLreader("Tagline")
                        .Trailer = SQLreader("Trailer")
                        .Certification = SQLreader("Certification")
                        .Genre = SQLreader("Genre")
                        .Runtime = SQLreader("Runtime")
                        .ReleaseDate = SQLreader("ReleaseDate")
                        .Studio = SQLreader("Studio")
                        .Director = SQLreader("Director")
                        .Credits = SQLreader("Credits")
                        .PlayCount = SQLreader("PlayCount")
                        .Watched = SQLreader("Watched")
                        .File = SQLreader("File")
                        .Path = SQLreader("Path")
                        .FileNameAndPath = SQLreader("FileNameAndPath")
                        .Status = SQLreader("Status")
                    End With

                End Using
            End Using

            Using SQLcommand As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                SQLcommand.CommandText = String.Concat("SELECT MA.MovieID, MA.ActorName , MA.Role ,Act.Name,Act.thumb FROM MoviesActors AS MA ", _
                                                       "INNER JOIN Actors AS Act ON (MA.ActorName = Act.Name) WHERE MA.MovieID = ", _movieDB.ID, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    Dim person As Media.Person
                    While SQLreader.Read
                        person = New Media.Person
                        person.Name = SQLreader("ActorName")
                        person.Role = SQLreader("Role")
                        person.Thumb = SQLreader("thumb")
                        _movieDB.Movie.Actors.Add(person)
                    End While
                End Using
            End Using

            Using SQLcommand As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesVStreams WHERE MovieID = ", _movieDB.ID, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    Dim video As MediaInfo.Video
                    While SQLreader.Read
                        video = New MediaInfo.Video
                        If Not DBNull.Value.Equals(SQLreader("Video_Width")) Then video.Width = SQLreader("Video_Width")
                        If Not DBNull.Value.Equals(SQLreader("Video_Height")) Then video.Height = SQLreader("Video_Height")
                        If Not DBNull.Value.Equals(SQLreader("Video_Codec")) Then video.Codec = SQLreader("Video_Codec")
                        If Not DBNull.Value.Equals(SQLreader("Video_Duration")) Then video.Duration = SQLreader("Video_Duration")
                        If Not DBNull.Value.Equals(SQLreader("Video_CodecId")) Then video.CodecID = SQLreader("Video_CodecId")
                        If Not DBNull.Value.Equals(SQLreader("Video_ScanType")) Then video.Scantype = SQLreader("Video_ScanType")
                        If Not DBNull.Value.Equals(SQLreader("Video_AspectDisplayRatio")) Then video.Aspect = SQLreader("Video_AspectDisplayRatio")
                        _movieDB.Movie.FileInfo.StreamDetails.Video.Add(video)
                    End While
                End Using
            End Using

            Using SQLcommand As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesAStreams WHERE MovieID = ", id, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    Dim audio As MediaInfo.Audio
                    While SQLreader.Read
                        audio = New MediaInfo.Audio
                        If Not DBNull.Value.Equals(SQLreader("Audio_Language")) Then audio.Language = SQLreader("Audio_Language")
                        If Not DBNull.Value.Equals(SQLreader("Audio_Codec")) Then audio.Codec = SQLreader("Audio_Codec")
                        If Not DBNull.Value.Equals(SQLreader("Audio_Channel")) Then audio.Channels = SQLreader("Audio_Channel")
                        If Not DBNull.Value.Equals(SQLreader("Audio_CodecID")) Then audio.CodecID = SQLreader("Audio_CodecID")
                        _movieDB.Movie.FileInfo.StreamDetails.Audio.Add(audio)
                    End While
                End Using
            End Using
            Using SQLcommand As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesSubs WHERE MovieID = ", _movieDB.ID, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    Dim subtitle As MediaInfo.Subtitle
                    While SQLreader.Read
                        subtitle = New MediaInfo.Subtitle
                        If Not DBNull.Value.Equals(SQLreader("subs")) Then subtitle.Language = SQLreader("subs")
                        _movieDB.Movie.FileInfo.StreamDetails.Subtitle.Add(subtitle)
                    End While
                End Using
            End Using
            Using SQLcommand As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesSets WHERE MovieID = ", _movieDB.ID, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    Dim sets As Media.Set
                    While SQLreader.Read
                        sets = New Media.Set
                        If Not DBNull.Value.Equals(SQLreader("SetName")) Then sets.SetContainer.Set = SQLreader("SetName")
                        If Not DBNull.Value.Equals(SQLreader("SetOrder")) Then sets.SetContainer.Order = SQLreader("SetOrder")
                        _movieDB.Movie.Sets.Add(sets)
                    End While
                End Using
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            _movieDB.ID = -1
        End Try
        Return _movieDB
    End Function
    Public Shared Function LoadMovieFromDB(ByVal sPath As String) As DBMovie
        Try
            Using SQLcommand As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                ' One more Query Better then re-write all function again
                SQLcommand.CommandText = String.Concat("SELECT ID FROM movies WHERE Path = ", sPath, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    If SQLreader.Read Then
                        Return LoadMovieFromDB(SQLreader("ID"))
                    Else
                        Return New Master.DBMovie With {.Id = -1} ' No Movie Found
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return New Master.DBMovie With {.Id = -1}
    End Function

    Public Shared Function SaveMovieToDB(ByVal _movieDB As DBMovie, ByVal IsNew As Boolean, Optional ByVal BatchMode As Boolean = False) As DBMovie

        Dim tmpMovie As Media.Movie

        Try
            Dim SQLtransaction As SQLite.SQLiteTransaction = Nothing
            If Not BatchMode Then SQLtransaction = Master.SQLcn.BeginTransaction
            Using SQLcommand As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                If IsNew Then
                    SQLcommand.CommandText = String.Concat("INSERT OR REPLACE INTO movies (", _
                        "MoviePath, type, title, HasPoster, HasFanart, HasNfo, HasTrailer, HasSub, HasExtra, new, mark, source, imdb, lock,", _
                        "OriginalTitle, Year, Rating, Votes, MPAA, Top250, Outline, Plot, Tagline, Certification, Genre,", _
                        "Studio, Runtime, ReleaseDate, Director, Credits, Playcount, Watched, Status, File, Path, FileNameAndPath, Trailer, ", _
                        "PosterPath, FanartPath, NfoPath, TrailerPath, SubPath", _
                        ") VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?); SELECT LAST_INSERT_ROWID() FROM movies;")
                Else
                    'SQLcommand.CommandText = String.Concat("UPDATE movies SET title = (?), HasPoster = (?), HasFanart = (?), HasNfo = (?), HasTrailer = (?), HasSub = (?), HasExtra = (?), ", _
                    '    "OriginalTitle = (?), Year = (?), Rating = (?), Votes = (?), MPAA = (?), Top250 = (?), Outline = (?), Plot = (?), Tagline = (?), Certification = (?), Genre = (?), ", _
                    '    "Studio = (?), Runtime = (?), ReleaseDate = (?), Director = (?), Credits = (?), Playcount = (?), Watched = (?), File = (?), Path = (?), FileNameAndPath = (?), Status = (?), ", _
                    '    "Trailer = (?), PosterPath = (?), FanartPath = (?), NfoPath = (?), TrailerPath = (?), SubPath = (?) WHERE id = ", _movieDB.ID.ToString, ";")
                    SQLcommand.CommandText = String.Concat("INSERT OR REPLACE INTO movies (", _
                        "ID, MoviePath, type, title, HasPoster, HasFanart, HasNfo, HasTrailer, HasSub, HasExtra, new, mark, source, imdb, lock,", _
                        "OriginalTitle, Year, Rating, Votes, MPAA, Top250, Outline, Plot, Tagline, Certification, Genre,", _
                        "Studio, Runtime, ReleaseDate, Director, Credits, Playcount, Watched, Status, File, Path, FileNameAndPath, Trailer, ", _
                        "PosterPath, FanartPath, NfoPath, TrailerPath, SubPath", _
                        ") VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?); SELECT LAST_INSERT_ROWID() FROM movies;")
                    Dim parMovieID As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parMovieID", DbType.String, 0, "ID")
                    parMovieID.Value = _movieDB.ID
                End If

                Dim parMoviePath As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parMoviePath", DbType.String, 0, "MoviePath")
                Dim parType As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parType", DbType.Boolean, 0, "type")
                Dim parTitle As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parTitle", DbType.String, 0, "title")
                Dim parHasPoster As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parHasPoster", DbType.Boolean, 0, "HasPoster")
                Dim parHasFanart As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parHasFanart", DbType.Boolean, 0, "HasFanart")
                Dim parHasNfo As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parHasInfo", DbType.Boolean, 0, "HasNfo")
                Dim parHasTrailer As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parHasTrailer", DbType.Boolean, 0, "HasTrailer")
                Dim parHasSub As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parHasSub", DbType.Boolean, 0, "HasSub")
                Dim parHasExtra As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parHasExtra", DbType.Boolean, 0, "HasExtra")
                Dim parNew As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parNew", DbType.Boolean, 0, "new")
                Dim parMark As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parMark", DbType.Boolean, 0, "mark")
                Dim parSource As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parSource", DbType.String, 0, "source")
                Dim parIMDB As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parIMDB", DbType.String, 0, "imdb")
                Dim parLock As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parLock", DbType.Boolean, 0, "lock")

                Dim parOriginalTitle As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parOriginalTitle", DbType.String, 0, "OriginalTitle")
                Dim parYear As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parYear", DbType.String, 0, "Year")
                Dim parRating As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parRating", DbType.String, 0, "Rating")
                Dim parVotes As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parVotes", DbType.String, 0, "Votes")
                Dim parMPAA As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parMPAA", DbType.String, 0, "MPAA")
                Dim parTop250 As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parTop250", DbType.String, 0, "Top250")
                Dim parOutline As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parOutline", DbType.String, 0, "Outline")
                Dim parPlot As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parPlot", DbType.String, 0, "Plot")
                Dim parTagline As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parTagline", DbType.String, 0, "Tagline")
                Dim parCertification As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parCertification", DbType.String, 0, "Certification")
                Dim parGenre As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parGenre", DbType.String, 0, "Genre")
                Dim parStudio As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parStudio", DbType.String, 0, "Studio")
                Dim parRuntime As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parRuntime", DbType.String, 0, "Runtime")
                Dim parReleaseDate As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parReleaseDate", DbType.String, 0, "ReleaseDate")
                Dim parDirector As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parDirector", DbType.String, 0, "Director")
                Dim parCredits As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parCredits", DbType.String, 0, "Credits")
                Dim parPlaycount As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parPlaycount", DbType.String, 0, "Playcount")
                Dim parWatched As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parWatched", DbType.String, 0, "Watched")
                Dim parFile As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parFile", DbType.String, 0, "File")
                Dim parPath As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parPath", DbType.String, 0, "Path")
                Dim parFileNameAndPath As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parFileNameAndPath", DbType.String, 0, "FileNameAndPath")
                Dim parStatus As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parStatus", DbType.String, 0, "Status")
                Dim parTrailer As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parTrailer", DbType.String, 0, "Trailer")

                Dim parPosterPath As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parPosterPath", DbType.String, 0, "PosterPath")
                Dim parFanartPath As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parFanartPath", DbType.String, 0, "FanartPath")
                Dim parNfoPath As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parNfoPath", DbType.String, 0, "NfoPath")
                Dim parTrailerPath As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parTrailerPath", DbType.String, 0, "TrailerPath")
                Dim parSubsPath As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parSubsPath", DbType.String, 0, "SubsPath")



                tmpMovie = _movieDB.Movie
                parMoviePath.Value = _movieDB.FaS.Filename
                parType.Value = _movieDB.FaS.isSingle
                parTitle.Value = tmpMovie.Title
                parHasPoster.Value = If(String.IsNullOrEmpty(_movieDB.FaS.Poster), False, True)
                parHasFanart.Value = If(String.IsNullOrEmpty(_movieDB.FaS.Fanart), False, True)
                parHasNfo.Value = If(String.IsNullOrEmpty(_movieDB.FaS.Nfo), False, True)
                parHasTrailer.Value = If(String.IsNullOrEmpty(_movieDB.FaS.Trailer), False, True)
                parHasSub.Value = If(String.IsNullOrEmpty(_movieDB.FaS.Subs), False, True)
                parHasExtra.Value = If(String.IsNullOrEmpty(_movieDB.FaS.Extra), False, True)
                parNew.Value = _movieDB.IsNew
                parLock.Value = _movieDB.IsMark

                parOriginalTitle.Value = tmpMovie.OriginalTitle
                parYear.Value = tmpMovie.Year
                parRating.Value = tmpMovie.Rating
                parVotes.Value = tmpMovie.Votes
                parMPAA.Value = tmpMovie.MPAA
                parTop250.Value = tmpMovie.Top250
                parOutline.Value = tmpMovie.Outline
                parPlot.Value = tmpMovie.Plot
                parTagline.Value = tmpMovie.Tagline
                parCertification.Value = tmpMovie.Certification
                parGenre.Value = tmpMovie.Genre
                parStudio.Value = tmpMovie.Studio
                parRuntime.Value = tmpMovie.Runtime
                parReleaseDate.Value = tmpMovie.ReleaseDate
                parDirector.Value = tmpMovie.Director
                parCredits.Value = tmpMovie.Credits
                parPlaycount.Value = tmpMovie.PlayCount
                parWatched.Value = tmpMovie.Watched
                parStatus.Value = tmpMovie.Status
                parFile.Value = tmpMovie.File
                parPath.Value = tmpMovie.Path
                parFileNameAndPath.Value = tmpMovie.FileNameAndPath
                parTrailer.Value = tmpMovie.Trailer

                parPosterPath.Value = _movieDB.FaS.Poster
                parFanartPath.Value = _movieDB.FaS.Fanart
                parNfoPath.Value = _movieDB.FaS.Nfo
                parTrailerPath.Value = _movieDB.FaS.Trailer
                parSubsPath.Value = _movieDB.FaS.Subs

                If Master.eSettings.MarkNew AndAlso IsNew Then
                    parMark.Value = True
                Else
                    parMark.Value = False
                End If
                parSource.Value = _movieDB.FaS.Source
                parIMDB.Value = tmpMovie.IMDBID
                If IsNew Then
                    Using rdrMovie As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                        If rdrMovie.Read Then
                            _movieDB.ID = rdrMovie(0)
                        Else
                            Master.eLog.WriteToErrorLog("Something very wrong here: SaveMovieToDB", _movieDB.ToString, "Error")
                            _movieDB.ID = -1
                            Return _movieDB
                        End If
                    End Using
                Else
                    SQLcommand.ExecuteNonQuery()
                End If

                If Not _movieDB.ID = -1 Then
                    Using SQLcommandActor As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                        SQLcommandActor.CommandText = String.Concat("INSERT OR REPLACE INTO Actors (Name,thumb) VALUES (?,?)")
                        Dim parActorName As SQLite.SQLiteParameter = SQLcommandActor.Parameters.Add("parActorName", DbType.String, 0, "Name")
                        Dim parActorThumb As SQLite.SQLiteParameter = SQLcommandActor.Parameters.Add("parActorThumb", DbType.String, 0, "thumb")
                        For Each actor As Media.Person In tmpMovie.Actors
                            parActorName.Value = actor.Name
                            parActorThumb.Value = actor.Thumb
                            SQLcommandActor.ExecuteNonQuery()
                            Using SQLcommandMoviesActors As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                                SQLcommandMoviesActors.CommandText = String.Concat("INSERT OR REPLACE INTO MoviesActors (MovieID,ActorName,Role) VALUES (?,?,?);")
                                Dim parMoviesActorsMovieID As SQLite.SQLiteParameter = SQLcommandMoviesActors.Parameters.Add("parMoviesActorsMovieID", DbType.UInt64, 0, "MovieID")
                                Dim parMoviesActorsActorName As SQLite.SQLiteParameter = SQLcommandMoviesActors.Parameters.Add("parMoviesActorsActorName", DbType.String, 0, "ActorNAme")
                                Dim parMoviesActorsActorRole As SQLite.SQLiteParameter = SQLcommandMoviesActors.Parameters.Add("parMoviesActorsActorRole", DbType.String, 0, "Role")
                                parMoviesActorsMovieID.Value = _movieDB.ID
                                parMoviesActorsActorName.Value = actor.Name
                                parMoviesActorsActorRole.Value = actor.Role
                                SQLcommandMoviesActors.ExecuteNonQuery()
                            End Using
                        Next
                    End Using
                    Using SQLcommandMoviesVStreams As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                        SQLcommandMoviesVStreams.CommandText = String.Concat("INSERT OR REPLACE INTO MoviesVStreams (", _
                                "MovieID, StreamID, Video_Width,Video_Height,Video_Codec,Video_Duration,", _
                                "Video_CodecId, Video_ScanType,Video_AspectDisplayRatio", _
                                ") VALUES (?,?,?,?,?,?,?,?,?);")
                        Dim parVideo_MovieID As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_MovieID", DbType.String, 0, "MovieID")
                        Dim parVideo_StreamID As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_StreamID", DbType.String, 0, "StreamID")
                        Dim parVideo_Width As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_Width", DbType.String, 0, "Video_Width")
                        Dim parVideo_Height As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_Height", DbType.String, 0, "Video_Height")
                        Dim parVideo_Codec As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_Codec", DbType.String, 0, "Video_Codec")
                        Dim parVideo_Duration As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_Duration", DbType.String, 0, "Video_Duration")
                        Dim parVideo_CodecId As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_CodecId", DbType.String, 0, "Video_CodecId")
                        Dim parVideo_ScanType As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_ScanType", DbType.String, 0, "Video_ScanType")
                        Dim parVideo_AspectDisplayRatio As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_AspectDisplayRatio", DbType.String, 0, "Video_AspectDisplayRatio")
                        For i As Integer = 0 To tmpMovie.FileInfo.StreamDetails.Video.Count - 1
                            parVideo_MovieID.Value = _movieDB.ID
                            parVideo_StreamID.Value = i
                            parVideo_Width.Value = tmpMovie.FileInfo.StreamDetails.Video(i).Width
                            parVideo_Height.Value = tmpMovie.FileInfo.StreamDetails.Video(i).Height
                            parVideo_Codec.Value = tmpMovie.FileInfo.StreamDetails.Video(i).Codec
                            parVideo_Duration.Value = tmpMovie.FileInfo.StreamDetails.Video(i).Duration
                            parVideo_CodecId.Value = tmpMovie.FileInfo.StreamDetails.Video(i).CodecID
                            parVideo_ScanType.Value = tmpMovie.FileInfo.StreamDetails.Video(i).Scantype
                            parVideo_AspectDisplayRatio.Value = tmpMovie.FileInfo.StreamDetails.Video(i).Aspect
                            SQLcommandMoviesVStreams.ExecuteNonQuery()
                        Next
                    End Using
                    Using SQLcommandMoviesAStreams As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                        SQLcommandMoviesAStreams.CommandText = String.Concat("INSERT OR REPLACE INTO MoviesAStreams (", _
                                "MovieID, StreamID, Audio_Language,Audio_Codec,Audio_Channel,Audio_CodecID", _
                                ") VALUES (?,?,?,?,?,?);")
                        Dim parAudio_MovieID As SQLite.SQLiteParameter = SQLcommandMoviesAStreams.Parameters.Add("parAudio_MovieID", DbType.String, 0, "MovieID")
                        Dim parAudio_StreamID As SQLite.SQLiteParameter = SQLcommandMoviesAStreams.Parameters.Add("parAudio_StreamID", DbType.String, 0, "StreamID")
                        Dim parAudio_Language As SQLite.SQLiteParameter = SQLcommandMoviesAStreams.Parameters.Add("parAudio_Language", DbType.String, 0, "Audio_Language")
                        Dim parAudio_Codec As SQLite.SQLiteParameter = SQLcommandMoviesAStreams.Parameters.Add("parAudio_Codec", DbType.String, 0, "Audio_Codec")
                        Dim parAudio_Channel As SQLite.SQLiteParameter = SQLcommandMoviesAStreams.Parameters.Add("parAudio_Channel", DbType.String, 0, "Audio_Channel")
                        Dim parAudio_CodecID As SQLite.SQLiteParameter = SQLcommandMoviesAStreams.Parameters.Add("parAudio_CodecID", DbType.String, 0, "Audio_CodecID")
                        For i As Integer = 0 To tmpMovie.FileInfo.StreamDetails.Audio.Count - 1
                            parAudio_MovieID.Value = _movieDB.ID
                            parAudio_StreamID.Value = i
                            parAudio_Language.Value = tmpMovie.FileInfo.StreamDetails.Audio(i).Language
                            parAudio_Codec.Value = tmpMovie.FileInfo.StreamDetails.Audio(i).Codec
                            parAudio_Channel.Value = tmpMovie.FileInfo.StreamDetails.Audio(i).Channels
                            parAudio_CodecID.Value = tmpMovie.FileInfo.StreamDetails.Audio(i).CodecID
                            SQLcommandMoviesAStreams.ExecuteNonQuery()
                        Next
                    End Using
                    Using SQLcommandMoviesSubs As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                        SQLcommandMoviesSubs.CommandText = String.Concat("INSERT OR REPLACE INTO MoviesSubs (", _
                                "MovieID, StreamID, subs", _
                                ") VALUES (?,?,?);")
                        Dim parSubs_MovieID As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parSubs_MovieID", DbType.String, 0, "MovieID")
                        Dim parSubs_StreamID As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parSubs_StreamID", DbType.String, 0, "StreamID")
                        Dim parSubs_Language As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parSubs_Language", DbType.String, 0, "subs")
                        For i As Integer = 0 To tmpMovie.FileInfo.StreamDetails.Subtitle.Count - 1
                            parSubs_MovieID.Value = _movieDB.ID
                            parSubs_StreamID.Value = i
                            parSubs_Language.Value = tmpMovie.FileInfo.StreamDetails.Subtitle(i).Language
                            SQLcommandMoviesSubs.ExecuteNonQuery()
                        Next
                    End Using
                    ' For what i understand this is used from Poster/Fanart Modules... will not be read/wrtire directly when load/save Movie
                    Using SQLcommandMoviesPosters As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                        SQLcommandMoviesPosters.CommandText = String.Concat("INSERT OR REPLACE INTO MoviesPosters (", _
                                "MovieID, thumb", _
                                ") VALUES (?,?,?);")
                        Dim parPosters_MovieID As SQLite.SQLiteParameter = SQLcommandMoviesPosters.Parameters.Add("parPosters_MovieID", DbType.String, 0, "MovieID")
                        Dim parPosters_thumb As SQLite.SQLiteParameter = SQLcommandMoviesPosters.Parameters.Add("parPosters_thumb", DbType.String, 0, "thumb")
                        ' It seems only one poster.. even if i found a list called Poster is not en Media.Movie
                        'For Each p As Media.Poster In tmpMovie.Thumbs
                        'parPosters_MovieID.Value = rdrMovie(0)
                        'parPosters_thumb.Value = p
                        'SQLcommandMoviesPosters.ExecuteNonQuery()
                        'Next
                    End Using
                    ' For what i understand this is used from Poster/Fanart Modules... will not be read/wrtire directly when load/save Movie
                    Using SQLcommandMoviesFanart As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                        SQLcommandMoviesFanart.CommandText = String.Concat("INSERT OR REPLACE INTO MoviesFanart (", _
                                "MovieID, preview, thumb", _
                                ") VALUES (?,?,?,?);")
                        Dim parFanart_MovieID As SQLite.SQLiteParameter = SQLcommandMoviesFanart.Parameters.Add("parFanart_MovieID", DbType.String, 0, "MovieID")
                        Dim parFanart_Preview As SQLite.SQLiteParameter = SQLcommandMoviesFanart.Parameters.Add("parFanart_Preview", DbType.String, 0, "Preview")
                        Dim parFanart_thumb As SQLite.SQLiteParameter = SQLcommandMoviesFanart.Parameters.Add("parFanart_thumb", DbType.String, 0, "thumb")
                        ' Same as Poster
                        'For Each p As Media.Fanart In tmpMovie.Fanart
                        'parFanart_MovieID.Value = rdrMovie(0)
                        'parFanart_thumb.Value = p
                        'SQLcommandMoviesFanart.ExecuteNonQuery()
                        'Next
                    End Using
                    Using SQLcommandMoviesSubs As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                        SQLcommandMoviesSubs.CommandText = String.Concat("INSERT OR REPLACE INTO Sets (", _
                                "SetName", _
                                ") VALUES (?);")
                        Dim parSets_SetName As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parSets_SetName", DbType.String, 0, "SetName")
                        For Each s As Media.Set In tmpMovie.Sets
                            parSets_SetName.Value = s.SetContainer.Set
                            SQLcommandMoviesSubs.ExecuteNonQuery()
                        Next
                    End Using
                    Using SQLcommandMoviesSubs As SQLite.SQLiteCommand = Master.SQLcn.CreateCommand
                        SQLcommandMoviesSubs.CommandText = String.Concat("INSERT OR REPLACE INTO MoviesSets (", _
                                "MovieID,SetName,SetOrder", _
                                ") VALUES (?,?,?);")
                        Dim parMovieSets_MovieID As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parMovieSets_MovieID", DbType.UInt64, 0, "MovieID")
                        Dim parMovieSets_SetName As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parMovieSets_SetName", DbType.String, 0, "SetName")
                        Dim parMovieSets_SetOrder As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parMovieSets_SetOrder", DbType.String, 0, "SetOrder")
                        For Each s As Media.Set In tmpMovie.Sets
                            parMovieSets_MovieID.Value = _movieDB.ID
                            parMovieSets_SetName.Value = s.SetContainer.Set
                            parMovieSets_SetOrder.Value = s.SetContainer.Order
                            SQLcommandMoviesSubs.ExecuteNonQuery()
                        Next
                    End Using
                End If
            End Using
            If Not BatchMode Then SQLtransaction.Commit()


        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return _movieDB
    End Function
    Public Shared Function SaveMovieToDB(ByVal _movie As Media.Movie, Optional ByVal IsMark As Boolean = False, Optional ByVal IsLock As Boolean = False) As DBMovie
        Dim _movieDB As New Master.DBMovie
        Try
            _movieDB.Movie = _movie
            Dim _tmpMovieDB As Master.DBMovie = LoadMovieFromDB(_movie.Path)
            If _tmpMovieDB.ID = -1 Then
                ' New Movie!!!! Can't Use this Function for new Movies need DBMovie for that
                Return _tmpMovieDB
            Else
                _movieDB.ID = _tmpMovieDB.ID
                _movieDB.FaS = _tmpMovieDB.FaS
            End If
            _movieDB.IsMark = IsMark
            _movieDB.IsLock = IsLock
            _movieDB = Master.SaveMovieToDB(_movieDB, False)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
        Return _movieDB
    End Function

End Class
