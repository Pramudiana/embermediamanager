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

Public Class Database
    Public SQLcn As New SQLite.SQLiteConnection()

    Public Function CreateCommand() As SQLite.SQLiteCommand
        Return SQLcn.CreateCommand
    End Function

    Public Function BeginTransaction() As SQLite.SQLiteTransaction
        Return SQLcn.BeginTransaction
    End Function

    Public Sub FillDataTable(ByRef dTable As DataTable, ByVal Command As String)
        dTable.Clear()
        Dim sqlDA As New SQLite.SQLiteDataAdapter(Command, SQLcn)
        Dim sqlCB As New SQLite.SQLiteCommandBuilder(sqlDA)
        sqlDA.Fill(dTable)
    End Sub

    Public Sub Close()
        SQLcn.Close()
    End Sub

    Public Sub Connect(ByVal Reset As Boolean, ByVal Delete As Boolean)
        Try
            Dim NewDB As Boolean = False
            'create database if it doesn't exist
            If Not File.Exists(Path.Combine(Application.StartupPath, "Media.emm")) Then
                NewDB = True
            ElseIf Delete Then
                NewDB = True
                File.Delete(Path.Combine(Application.StartupPath, "Media.emm"))
            End If

            SQLcn.ConnectionString = String.Format("Data Source=""{0}"";Compress=True", Path.Combine(Application.StartupPath, "Media.emm"))
            SQLcn.Open()
            Dim cQuery As String = String.Empty
            Using SQLtransaction As SQLite.SQLiteTransaction = SQLcn.BeginTransaction
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
                                "ListTitle TEXT NOT NULL, " & _
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
                                "Title TEXT, " & _
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
                                "ExtraPath TEXT, " & _
                                "NfoPath TEXT, " & _
                                "TrailerPath TEXT, " & _
                                "SubPath TEXT, " & _
                                "FanartURL TEXT, " & _
                                "UseFolder BOOL NOT NULL DEFAULT False, " & _
                                "OutOfTolerance BOOL NOT NULL DEFAULT False, " & _
                                "NeedsSave BOOL NOT NULL DEFAULT False" & _
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
                                "Audio_LongLanguage TEXT, " & _
                                "Audio_Codec TEXT, " & _
                                "Audio_Channel TEXT, " & _
                                "PRIMARY KEY (MovieID,StreamID) " & _
                                ");"
                    SQLcommand.ExecuteNonQuery()
                    SQLcommand.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS Index_MoviesVStreams ON MoviesAStreams (MovieID);"
                    SQLcommand.ExecuteNonQuery()

                    SQLcommand.CommandText = "CREATE TABLE IF NOT EXISTS MoviesSubs(" & _
                                "MovieID INTEGER NOT NULL, " & _
                                "StreamID INTEGER NOT NULL, " & _
                                "Subs_Language TEXT, " & _
                                "Subs_LongLanguage TEXT, " & _
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

    Public Function LoadMovieFromDB(ByVal id As Long) As Master.DBMovie
        Dim _movieDB As New Master.DBMovie
        Try
            _movieDB.ID = id
            Using SQLcommand As SQLite.SQLiteCommand = SQLcn.CreateCommand
                SQLcommand.CommandText = String.Concat("SELECT * FROM movies WHERE id = ", _movieDB.ID, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()

                    If Not DBNull.Value.Equals(SQLreader("ListTitle")) Then _movieDB.ListTitle = SQLreader("ListTitle")
                    If Not DBNull.Value.Equals(SQLreader("MoviePath")) Then _movieDB.Filename = SQLreader("MoviePath")
                    _movieDB.isSingle = SQLreader("type")
                    If Not DBNull.Value.Equals(SQLreader("FanartPath")) Then _movieDB.FanartPath = SQLreader("FanartPath")
                    If Not DBNull.Value.Equals(SQLreader("PosterPath")) Then _movieDB.PosterPath = SQLreader("PosterPath")
                    If Not DBNull.Value.Equals(SQLreader("TrailerPath")) Then _movieDB.TrailerPath = SQLreader("TrailerPath")
                    If Not DBNull.Value.Equals(SQLreader("NfoPath")) Then _movieDB.NfoPath = SQLreader("NfoPath")
                    If Not DBNull.Value.Equals(SQLreader("SubPath")) Then _movieDB.SubPath = SQLreader("SubPath")
                    If Not DBNull.Value.Equals(SQLreader("ExtraPath")) Then _movieDB.ExtraPath = SQLreader("ExtraPath")
                    If Not DBNull.Value.Equals(SQLreader("source")) Then _movieDB.Source = SQLreader("source")
                    _movieDB.IsMark = SQLreader("mark")
                    _movieDB.IsLock = SQLreader("lock")
                    _movieDB.UseFolder = SQLreader("UseFolder")
                    _movieDB.OutOfTolerance = SQLreader("OutOfTolerance")
                    _movieDB.NeedsSave = SQLreader("NeedsSave")
                    _movieDB.Movie = New Media.Movie
                    With _movieDB.Movie
                        .Clear()
                        If Not DBNull.Value.Equals(SQLreader("IMDB")) Then .ID = SQLreader("IMDB")
                        If Not DBNull.Value.Equals(SQLreader("Title")) Then .Title = SQLreader("Title")
                        If Not DBNull.Value.Equals(SQLreader("OriginalTitle")) Then .OriginalTitle = SQLreader("OriginalTitle")
                        If Not DBNull.Value.Equals(SQLreader("Year")) Then .Year = SQLreader("Year")
                        If Not DBNull.Value.Equals(SQLreader("Rating")) Then .Rating = SQLreader("Rating")
                        If Not DBNull.Value.Equals(SQLreader("Votes")) Then .Votes = SQLreader("Votes")
                        If Not DBNull.Value.Equals(SQLreader("MPAA")) Then .MPAA = SQLreader("MPAA")
                        If Not DBNull.Value.Equals(SQLreader("Top250")) Then .Top250 = SQLreader("Top250")
                        If Not DBNull.Value.Equals(SQLreader("Outline")) Then .Outline = SQLreader("Outline")
                        If Not DBNull.Value.Equals(SQLreader("Plot")) Then .Plot = SQLreader("Plot")
                        If Not DBNull.Value.Equals(SQLreader("Tagline")) Then .Tagline = SQLreader("Tagline")
                        If Not DBNull.Value.Equals(SQLreader("Trailer")) Then .Trailer = SQLreader("Trailer")
                        If Not DBNull.Value.Equals(SQLreader("Certification")) Then .Certification = SQLreader("Certification")
                        If Not DBNull.Value.Equals(SQLreader("Genre")) Then .Genre = SQLreader("Genre")
                        If Not DBNull.Value.Equals(SQLreader("Runtime")) Then .Runtime = SQLreader("Runtime")
                        If Not DBNull.Value.Equals(SQLreader("ReleaseDate")) Then .ReleaseDate = SQLreader("ReleaseDate")
                        If Not DBNull.Value.Equals(SQLreader("Studio")) Then .Studio = SQLreader("Studio")
                        If Not DBNull.Value.Equals(SQLreader("Director")) Then .Director = SQLreader("Director")
                        If Not DBNull.Value.Equals(SQLreader("Credits")) Then .Credits = SQLreader("Credits")
                        If Not DBNull.Value.Equals(SQLreader("PlayCount")) Then .PlayCount = SQLreader("PlayCount")
                        If Not DBNull.Value.Equals(SQLreader("Watched")) Then .Watched = SQLreader("Watched")
                        If Not DBNull.Value.Equals(SQLreader("File")) Then .File = SQLreader("File")
                        If Not DBNull.Value.Equals(SQLreader("Path")) Then .Path = SQLreader("Path")
                        If Not DBNull.Value.Equals(SQLreader("FileNameAndPath")) Then .FileNameAndPath = SQLreader("FileNameAndPath")
                        If Not DBNull.Value.Equals(SQLreader("Status")) Then .Status = SQLreader("Status")
                        If Not DBNull.Value.Equals(SQLreader("FanartURL")) Then .Fanart.URL = SQLreader("FanartURL")
                    End With

                End Using
            End Using

            Using SQLcommand As SQLite.SQLiteCommand = SQLcn.CreateCommand
                SQLcommand.CommandText = String.Concat("SELECT MA.MovieID, MA.ActorName , MA.Role ,Act.Name,Act.thumb FROM MoviesActors AS MA ", _
                                                       "INNER JOIN Actors AS Act ON (MA.ActorName = Act.Name) WHERE MA.MovieID = ", _movieDB.ID, " ORDER BY MA.ROWID;")
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

            Using SQLcommand As SQLite.SQLiteCommand = SQLcn.CreateCommand
                SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesVStreams WHERE MovieID = ", _movieDB.ID, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    Dim video As MediaInfo.Video
                    While SQLreader.Read
                        video = New MediaInfo.Video
                        If Not DBNull.Value.Equals(SQLreader("Video_Width")) Then video.Width = SQLreader("Video_Width")
                        If Not DBNull.Value.Equals(SQLreader("Video_Height")) Then video.Height = SQLreader("Video_Height")
                        If Not DBNull.Value.Equals(SQLreader("Video_Codec")) Then video.Codec = SQLreader("Video_Codec")
                        If Not DBNull.Value.Equals(SQLreader("Video_Duration")) Then video.Duration = SQLreader("Video_Duration")
                        If Not DBNull.Value.Equals(SQLreader("Video_ScanType")) Then video.Scantype = SQLreader("Video_ScanType")
                        If Not DBNull.Value.Equals(SQLreader("Video_AspectDisplayRatio")) Then video.Aspect = SQLreader("Video_AspectDisplayRatio")
                        _movieDB.Movie.FileInfo.StreamDetails.Video.Add(video)
                    End While
                End Using
            End Using

            Using SQLcommand As SQLite.SQLiteCommand = SQLcn.CreateCommand
                SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesAStreams WHERE MovieID = ", id, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    Dim audio As MediaInfo.Audio
                    While SQLreader.Read
                        audio = New MediaInfo.Audio
                        If Not DBNull.Value.Equals(SQLreader("Audio_Language")) Then audio.Language = SQLreader("Audio_Language")
                        If Not DBNull.Value.Equals(SQLreader("Audio_LongLanguage")) Then audio.LongLanguage = SQLreader("Audio_LongLanguage")
                        If Not DBNull.Value.Equals(SQLreader("Audio_Codec")) Then audio.Codec = SQLreader("Audio_Codec")
                        If Not DBNull.Value.Equals(SQLreader("Audio_Channel")) Then audio.Channels = SQLreader("Audio_Channel")
                        _movieDB.Movie.FileInfo.StreamDetails.Audio.Add(audio)
                    End While
                End Using
            End Using
            Using SQLcommand As SQLite.SQLiteCommand = SQLcn.CreateCommand
                SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesSubs WHERE MovieID = ", _movieDB.ID, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    Dim subtitle As MediaInfo.Subtitle
                    While SQLreader.Read
                        subtitle = New MediaInfo.Subtitle
                        If Not DBNull.Value.Equals(SQLreader("Subs_Language")) Then subtitle.Language = SQLreader("Subs_Language")
                        If Not DBNull.Value.Equals(SQLreader("Subs_LongLanguage")) Then subtitle.LongLanguage = SQLreader("Subs_LongLanguage")
                        _movieDB.Movie.FileInfo.StreamDetails.Subtitle.Add(subtitle)
                    End While
                End Using
            End Using
            Using SQLcommand As SQLite.SQLiteCommand = SQLcn.CreateCommand
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
            Using SQLcommand As SQLite.SQLiteCommand = SQLcn.CreateCommand
                SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesFanart WHERE MovieID = ", _movieDB.ID, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                    Dim thumb As Media.Thumb
                    While SQLreader.Read
                        thumb = New Media.Thumb
                        If Not DBNull.Value.Equals(SQLreader("preview")) Then thumb.Preview = SQLreader("preview")
                        If Not DBNull.Value.Equals(SQLreader("thumbs")) Then thumb.Text = SQLreader("thumbs")
                        _movieDB.Movie.Fanart.Thumb.Add(thumb)
                    End While
                End Using
            End Using
            Using SQLcommand As SQLite.SQLiteCommand = SQLcn.CreateCommand
                SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesPosters WHERE MovieID = ", _movieDB.ID, ";")
                Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()

                    Dim poster As Media.Posters
                    If SQLreader.Read Then
                        _movieDB.Movie.Thumbs = New Media.Poster
                    End If
                    While SQLreader.Read
                        poster = New Media.Posters
                        If Not DBNull.Value.Equals(SQLreader("thumbs")) Then poster.URL = SQLreader("thumbs")
                        _movieDB.Movie.Thumbs.Thumb.Add(poster)
                    End While
                End Using
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            _movieDB.ID = -1
        End Try
        Return _movieDB
    End Function

    Public Function LoadMovieFromDB(ByVal sPath As String) As Master.DBMovie
        Try
            Using SQLcommand As SQLite.SQLiteCommand = SQLcn.CreateCommand
                ' One more Query Better then re-write all function again
                SQLcommand.CommandText = String.Concat("SELECT ID FROM movies WHERE MoviePath = ", sPath, ";")
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

    Public Function SaveMovieToDB(ByVal _movieDB As Master.DBMovie, ByVal IsNew As Boolean, Optional ByVal BatchMode As Boolean = False, Optional ByVal ToNfo As Boolean = False) As Master.DBMovie

        Try
            Dim SQLtransaction As SQLite.SQLiteTransaction = Nothing
            If Not BatchMode Then SQLtransaction = SQLcn.BeginTransaction
            Using SQLcommand As SQLite.SQLiteCommand = SQLcn.CreateCommand
                If IsNew Then
                    SQLcommand.CommandText = String.Concat("INSERT OR REPLACE INTO movies (", _
                        "MoviePath, type, ListTitle, HasPoster, HasFanart, HasNfo, HasTrailer, HasSub, HasExtra, new, mark, source, imdb, lock,", _
                        "Title, OriginalTitle, Year, Rating, Votes, MPAA, Top250, Outline, Plot, Tagline, Certification, Genre,", _
                        "Studio, Runtime, ReleaseDate, Director, Credits, Playcount, Watched, Status, File, Path, FileNameAndPath, Trailer, ", _
                        "PosterPath, FanartPath, NfoPath, TrailerPath, SubPath, ExtraPath, FanartURL, UseFolder, OutOfTolerance, NeedsSave", _
                        ") VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?); SELECT LAST_INSERT_ROWID() FROM movies;")
                Else
                    SQLcommand.CommandText = String.Concat("INSERT OR REPLACE INTO movies (", _
                        "ID, MoviePath, type, ListTitle, HasPoster, HasFanart, HasNfo, HasTrailer, HasSub, HasExtra, new, mark, source, imdb, lock,", _
                        "Title, OriginalTitle, Year, Rating, Votes, MPAA, Top250, Outline, Plot, Tagline, Certification, Genre,", _
                        "Studio, Runtime, ReleaseDate, Director, Credits, Playcount, Watched, Status, File, Path, FileNameAndPath, Trailer, ", _
                        "PosterPath, FanartPath, NfoPath, TrailerPath, SubPath, ExtraPath, FanartURL, UseFolder, OutOfTolerance, NeedsSave", _
                        ") VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?); SELECT LAST_INSERT_ROWID() FROM movies;")
                    Dim parMovieID As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parMovieID", DbType.Int32, 0, "ID")
                    parMovieID.Value = _movieDB.ID
                End If

                Dim parMoviePath As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parMoviePath", DbType.String, 0, "MoviePath")
                Dim parType As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parType", DbType.Boolean, 0, "type")
                Dim parListTitle As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parListTitle", DbType.String, 0, "ListTitle")
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

                Dim parTitle As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parTitle", DbType.String, 0, "Title")
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
                Dim parSubPath As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parSubPath", DbType.String, 0, "SubPath")
                Dim parExtraPath As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parExtraPath", DbType.String, 0, "ExtraPath")
                Dim parFanartURL As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parFanartURL", DbType.String, 0, "FanartURL")
                Dim parUseFolder As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parUseFolder", DbType.Boolean, 0, "UseFolder")
                Dim parOutOfTolerance As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parOutOfTolerance", DbType.Boolean, 0, "OutOfTolerance")
                Dim parNeedsSave As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parNeedsSave", DbType.Boolean, 0, "NeedsSave")

                ' First let's save it to NFO, even because we will need the NFO path
                If ToNfo Then NFO.SaveMovieToNFO(_movieDB)

                parMoviePath.Value = _movieDB.Filename
                parType.Value = _movieDB.isSingle
                parListTitle.Value = _movieDB.ListTitle

                parPosterPath.Value = _movieDB.PosterPath
                parFanartPath.Value = _movieDB.FanartPath
                parNfoPath.Value = _movieDB.NfoPath
                parTrailerPath.Value = _movieDB.TrailerPath
                parSubPath.Value = _movieDB.SubPath
                parExtraPath.Value = _movieDB.ExtraPath
                parFanartURL.Value = _movieDB.Movie.Fanart.URL

                parHasPoster.Value = Not String.IsNullOrEmpty(_movieDB.PosterPath)
                parHasFanart.Value = Not String.IsNullOrEmpty(_movieDB.FanartPath)
                parHasNfo.Value = Not String.IsNullOrEmpty(_movieDB.NfoPath)
                parHasTrailer.Value = Not String.IsNullOrEmpty(_movieDB.TrailerPath)
                parHasSub.Value = Not String.IsNullOrEmpty(_movieDB.SubPath)
                parHasExtra.Value = Not String.IsNullOrEmpty(_movieDB.ExtraPath)

                parNew.Value = _movieDB.IsNew
                parMark.Value = _movieDB.IsMark
                parLock.Value = _movieDB.IsLock

                parIMDB.Value = _movieDB.Movie.IMDBID
                parTitle.Value = _movieDB.Movie.Title
                parOriginalTitle.Value = _movieDB.Movie.OriginalTitle
                parYear.Value = _movieDB.Movie.Year
                parRating.Value = _movieDB.Movie.Rating
                parVotes.Value = _movieDB.Movie.Votes
                parMPAA.Value = _movieDB.Movie.MPAA
                parTop250.Value = _movieDB.Movie.Top250
                parOutline.Value = _movieDB.Movie.Outline
                parPlot.Value = _movieDB.Movie.Plot
                parTagline.Value = _movieDB.Movie.Tagline
                parCertification.Value = _movieDB.Movie.Certification
                parGenre.Value = _movieDB.Movie.Genre
                parStudio.Value = _movieDB.Movie.Studio
                parRuntime.Value = _movieDB.Movie.Runtime
                parReleaseDate.Value = _movieDB.Movie.ReleaseDate
                parDirector.Value = _movieDB.Movie.Director
                parCredits.Value = _movieDB.Movie.Credits
                parPlaycount.Value = _movieDB.Movie.PlayCount
                parWatched.Value = _movieDB.Movie.Watched
                parStatus.Value = _movieDB.Movie.Status
                parFile.Value = _movieDB.Movie.File
                parPath.Value = _movieDB.Movie.Path
                parFileNameAndPath.Value = _movieDB.Movie.FileNameAndPath
                parTrailer.Value = _movieDB.Movie.Trailer

                parUseFolder.Value = _movieDB.UseFolder
                parOutOfTolerance.Value = _movieDB.OutOfTolerance
                parNeedsSave.Value = _movieDB.NeedsSave

                parSource.Value = _movieDB.Source
                If IsNew Then
                    If Master.eSettings.MarkNew Then
                        parMark.Value = True
                    Else
                        parMark.Value = False
                    End If
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
                    Using SQLOthercommands As SQLite.SQLiteCommand = Master.DB.CreateCommand
                        Dim parId As SQLite.SQLiteParameter = SQLOthercommands.Parameters.Add("parId", DbType.UInt64, 0, "MovieID")
                        SQLOthercommands.CommandText = "DELETE FROM MoviesAStreams WHERE MovieID = (?);"
                        parId.Value = _movieDB.ID
                        SQLOthercommands.ExecuteNonQuery()
                        SQLOthercommands.CommandText = "DELETE FROM MoviesVStreams WHERE MovieID = (?);"
                        parId.Value = _movieDB.ID
                        SQLOthercommands.ExecuteNonQuery()
                        SQLOthercommands.CommandText = "DELETE FROM MoviesActors WHERE MovieID = (?);"
                        parId.Value = _movieDB.ID
                        SQLOthercommands.ExecuteNonQuery()
                        SQLOthercommands.CommandText = "DELETE FROM MoviesSubs WHERE MovieID = (?);"
                        parId.Value = _movieDB.ID
                        SQLOthercommands.ExecuteNonQuery()
                        SQLOthercommands.CommandText = "DELETE FROM MoviesPosters WHERE MovieID = (?);"
                        parId.Value = _movieDB.ID
                        SQLOthercommands.ExecuteNonQuery()
                        SQLOthercommands.CommandText = "DELETE FROM MoviesFanart WHERE MovieID = (?);"
                        parId.Value = _movieDB.ID
                        SQLOthercommands.ExecuteNonQuery()
                        SQLOthercommands.CommandText = "DELETE FROM MoviesSets WHERE MovieID = (?);"
                        parId.Value = _movieDB.ID
                        SQLOthercommands.ExecuteNonQuery()
                    End Using

                    Using SQLcommandActor As SQLite.SQLiteCommand = SQLcn.CreateCommand
                        SQLcommandActor.CommandText = String.Concat("INSERT OR REPLACE INTO Actors (Name,thumb) VALUES (?,?)")
                        Dim parActorName As SQLite.SQLiteParameter = SQLcommandActor.Parameters.Add("parActorName", DbType.String, 0, "Name")
                        Dim parActorThumb As SQLite.SQLiteParameter = SQLcommandActor.Parameters.Add("parActorThumb", DbType.String, 0, "thumb")
                        For Each actor As Media.Person In _movieDB.Movie.Actors
                            parActorName.Value = actor.Name
                            parActorThumb.Value = actor.Thumb
                            SQLcommandActor.ExecuteNonQuery()
                            Using SQLcommandMoviesActors As SQLite.SQLiteCommand = SQLcn.CreateCommand
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
                    Using SQLcommandMoviesVStreams As SQLite.SQLiteCommand = SQLcn.CreateCommand
                        SQLcommandMoviesVStreams.CommandText = String.Concat("INSERT OR REPLACE INTO MoviesVStreams (", _
                                "MovieID, StreamID, Video_Width,Video_Height,Video_Codec,Video_Duration,", _
                                "Video_ScanType, Video_AspectDisplayRatio", _
                                ") VALUES (?,?,?,?,?,?,?,?);")
                        Dim parVideo_MovieID As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_MovieID", DbType.String, 0, "MovieID")
                        Dim parVideo_StreamID As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_StreamID", DbType.String, 0, "StreamID")
                        Dim parVideo_Width As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_Width", DbType.String, 0, "Video_Width")
                        Dim parVideo_Height As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_Height", DbType.String, 0, "Video_Height")
                        Dim parVideo_Codec As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_Codec", DbType.String, 0, "Video_Codec")
                        Dim parVideo_Duration As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_Duration", DbType.String, 0, "Video_Duration")
                        Dim parVideo_ScanType As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_ScanType", DbType.String, 0, "Video_ScanType")
                        Dim parVideo_AspectDisplayRatio As SQLite.SQLiteParameter = SQLcommandMoviesVStreams.Parameters.Add("parVideo_AspectDisplayRatio", DbType.String, 0, "Video_AspectDisplayRatio")
                        For i As Integer = 0 To _movieDB.Movie.FileInfo.StreamDetails.Video.Count - 1
                            parVideo_MovieID.Value = _movieDB.ID
                            parVideo_StreamID.Value = i
                            parVideo_Width.Value = _movieDB.Movie.FileInfo.StreamDetails.Video(i).Width
                            parVideo_Height.Value = _movieDB.Movie.FileInfo.StreamDetails.Video(i).Height
                            parVideo_Codec.Value = _movieDB.Movie.FileInfo.StreamDetails.Video(i).Codec
                            parVideo_Duration.Value = _movieDB.Movie.FileInfo.StreamDetails.Video(i).Duration
                            parVideo_ScanType.Value = _movieDB.Movie.FileInfo.StreamDetails.Video(i).Scantype
                            parVideo_AspectDisplayRatio.Value = _movieDB.Movie.FileInfo.StreamDetails.Video(i).Aspect
                            SQLcommandMoviesVStreams.ExecuteNonQuery()
                        Next
                    End Using
                    Using SQLcommandMoviesAStreams As SQLite.SQLiteCommand = SQLcn.CreateCommand
                        SQLcommandMoviesAStreams.CommandText = String.Concat("INSERT OR REPLACE INTO MoviesAStreams (", _
                                "MovieID, StreamID, Audio_Language, Audio_LongLanguage, Audio_Codec, Audio_Channel", _
                                ") VALUES (?,?,?,?,?,?);")
                        Dim parAudio_MovieID As SQLite.SQLiteParameter = SQLcommandMoviesAStreams.Parameters.Add("parAudio_MovieID", DbType.String, 0, "MovieID")
                        Dim parAudio_StreamID As SQLite.SQLiteParameter = SQLcommandMoviesAStreams.Parameters.Add("parAudio_StreamID", DbType.String, 0, "StreamID")
                        Dim parAudio_Language As SQLite.SQLiteParameter = SQLcommandMoviesAStreams.Parameters.Add("parAudio_Language", DbType.String, 0, "Audio_Language")
                        Dim parAudio_LongLanguage As SQLite.SQLiteParameter = SQLcommandMoviesAStreams.Parameters.Add("parAudio_LongLanguage", DbType.String, 0, "Audio_LongLanguage")
                        Dim parAudio_Codec As SQLite.SQLiteParameter = SQLcommandMoviesAStreams.Parameters.Add("parAudio_Codec", DbType.String, 0, "Audio_Codec")
                        Dim parAudio_Channel As SQLite.SQLiteParameter = SQLcommandMoviesAStreams.Parameters.Add("parAudio_Channel", DbType.String, 0, "Audio_Channel")
                        For i As Integer = 0 To _movieDB.Movie.FileInfo.StreamDetails.Audio.Count - 1
                            parAudio_MovieID.Value = _movieDB.ID
                            parAudio_StreamID.Value = i
                            parAudio_Language.Value = _movieDB.Movie.FileInfo.StreamDetails.Audio(i).Language
                            parAudio_LongLanguage.Value = _movieDB.Movie.FileInfo.StreamDetails.Audio(i).LongLanguage
                            parAudio_Codec.Value = _movieDB.Movie.FileInfo.StreamDetails.Audio(i).Codec
                            parAudio_Channel.Value = _movieDB.Movie.FileInfo.StreamDetails.Audio(i).Channels
                            SQLcommandMoviesAStreams.ExecuteNonQuery()
                        Next
                    End Using
                    Using SQLcommandMoviesSubs As SQLite.SQLiteCommand = SQLcn.CreateCommand
                        SQLcommandMoviesSubs.CommandText = String.Concat("INSERT OR REPLACE INTO MoviesSubs (", _
                                "MovieID, StreamID, Subs_Language, Subs_LongLanguage", _
                                ") VALUES (?,?,?,?);")
                        Dim parSubs_MovieID As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parSubs_MovieID", DbType.String, 0, "MovieID")
                        Dim parSubs_StreamID As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parSubs_StreamID", DbType.String, 0, "StreamID")
                        Dim parSubs_Language As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parSubs_Language", DbType.String, 0, "Subs_Language")
                        Dim parSubs_LongLanguage As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parSubs_LongLanguage", DbType.String, 0, "Subs_LongLanguage")
                        For i As Integer = 0 To _movieDB.Movie.FileInfo.StreamDetails.Subtitle.Count - 1
                            parSubs_MovieID.Value = _movieDB.ID
                            parSubs_StreamID.Value = i
                            parSubs_Language.Value = _movieDB.Movie.FileInfo.StreamDetails.Subtitle(i).Language
                            parSubs_LongLanguage.Value = _movieDB.Movie.FileInfo.StreamDetails.Subtitle(i).LongLanguage
                            SQLcommandMoviesSubs.ExecuteNonQuery()
                        Next
                    End Using
                    ' For what i understand this is used from Poster/Fanart Modules... will not be read/wrtire directly when load/save Movie
                    Using SQLcommandMoviesPosters As SQLite.SQLiteCommand = SQLcn.CreateCommand
                        SQLcommandMoviesPosters.CommandText = String.Concat("INSERT OR REPLACE INTO MoviesPosters (", _
                                "MovieID, thumbs", _
                                ") VALUES (?,?);")
                        Dim parPosters_MovieID As SQLite.SQLiteParameter = SQLcommandMoviesPosters.Parameters.Add("parPosters_MovieID", DbType.String, 0, "MovieID")
                        Dim parPosters_thumb As SQLite.SQLiteParameter = SQLcommandMoviesPosters.Parameters.Add("parPosters_thumb", DbType.String, 0, "thumb")
                        For Each p As Media.Posters In _movieDB.Movie.Thumbs.Thumb
                            parPosters_MovieID.Value = _movieDB.ID
                            parPosters_thumb.Value = p.URL
                            SQLcommandMoviesPosters.ExecuteNonQuery()
                        Next
                    End Using
                    Using SQLcommandMoviesFanart As SQLite.SQLiteCommand = SQLcn.CreateCommand
                        SQLcommandMoviesFanart.CommandText = String.Concat("INSERT OR REPLACE INTO MoviesFanart (", _
                                "MovieID, preview, thumbs", _
                                ") VALUES (?,?,?);")
                        Dim parFanart_MovieID As SQLite.SQLiteParameter = SQLcommandMoviesFanart.Parameters.Add("parFanart_MovieID", DbType.String, 0, "MovieID")
                        Dim parFanart_Preview As SQLite.SQLiteParameter = SQLcommandMoviesFanart.Parameters.Add("parFanart_Preview", DbType.String, 0, "Preview")
                        Dim parFanart_thumb As SQLite.SQLiteParameter = SQLcommandMoviesFanart.Parameters.Add("parFanart_thumb", DbType.String, 0, "thumb")
                        For Each p As Media.Thumb In _movieDB.Movie.Fanart.Thumb
                            parFanart_MovieID.Value = _movieDB.ID
                            parFanart_Preview.Value = p.Preview
                            parFanart_thumb.Value = p.Text
                            SQLcommandMoviesFanart.ExecuteNonQuery()
                        Next
                    End Using
                    Using SQLcommandMoviesSubs As SQLite.SQLiteCommand = SQLcn.CreateCommand
                        SQLcommandMoviesSubs.CommandText = String.Concat("INSERT OR REPLACE INTO Sets (", _
                                "SetName", _
                                ") VALUES (?);")
                        Dim parSets_SetName As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parSets_SetName", DbType.String, 0, "SetName")
                        For Each s As Media.Set In _movieDB.Movie.Sets
                            parSets_SetName.Value = s.SetContainer.Set
                            SQLcommandMoviesSubs.ExecuteNonQuery()
                        Next
                    End Using
                    Using SQLcommandMoviesSubs As SQLite.SQLiteCommand = SQLcn.CreateCommand
                        SQLcommandMoviesSubs.CommandText = String.Concat("INSERT OR REPLACE INTO MoviesSets (", _
                                "MovieID,SetName,SetOrder", _
                                ") VALUES (?,?,?);")
                        Dim parMovieSets_MovieID As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parMovieSets_MovieID", DbType.UInt64, 0, "MovieID")
                        Dim parMovieSets_SetName As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parMovieSets_SetName", DbType.String, 0, "SetName")
                        Dim parMovieSets_SetOrder As SQLite.SQLiteParameter = SQLcommandMoviesSubs.Parameters.Add("parMovieSets_SetOrder", DbType.String, 0, "SetOrder")
                        For Each s As Media.Set In _movieDB.Movie.Sets
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

    'Public Function SaveMovieToDB(ByVal _movie As Media.Movie, ByVal sPath As String, Optional ByVal IsMark As Boolean = False, Optional ByVal IsLock As Boolean = False) As Master.DBMovie
    '    Dim _movieDB As New Master.DBMovie
    '    Try
    '        _movieDB.Movie = _movie
    '        Dim _tmpMovieDB As Master.DBMovie = LoadMovieFromDB(sPath)
    '        If _tmpMovieDB.ID = -1 Then
    '            ' New Movie!!!! Can't Use this Function for new Movies need DBMovie for that
    '            Return _tmpMovieDB
    '        Else
    '            _movieDB.ID = _tmpMovieDB.ID
    '            _movieDB.FaS = _tmpMovieDB.FaS
    '        End If
    '        _movieDB.IsMark = IsMark
    '        _movieDB.IsLock = IsLock
    '        _movieDB = SaveMovieToDB(_movieDB, False)
    '    Catch ex As Exception
    '        Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
    '    End Try
    '    Return _movieDB
    'End Function

    Public Function DeleteFromDB(ByVal ID As Integer, Optional ByVal BatchMode As Boolean = False) As Boolean
        Try
            Dim SQLtransaction As SQLite.SQLiteTransaction = Nothing
            If Not BatchMode Then SQLtransaction = Master.DB.BeginTransaction
            Using SQLcommand As SQLite.SQLiteCommand = Master.DB.CreateCommand
                SQLcommand.CommandText = String.Concat("DELETE FROM movies WHERE id = ", ID, ";")
                SQLcommand.ExecuteNonQuery()
                Dim parId As SQLite.SQLiteParameter = SQLcommand.Parameters.Add("parId", DbType.UInt64, 0, "MovieID")
                SQLcommand.CommandText = "DELETE FROM MoviesAStreams WHERE MovieID = (?);"
                parId.Value = ID
                SQLcommand.ExecuteNonQuery()
                SQLcommand.CommandText = "DELETE FROM MoviesVStreams WHERE MovieID = (?);"
                parId.Value = ID
                SQLcommand.ExecuteNonQuery()
                SQLcommand.CommandText = "DELETE FROM MoviesActors WHERE MovieID = (?);"
                parId.Value = ID
                SQLcommand.ExecuteNonQuery()
                SQLcommand.CommandText = "DELETE FROM MoviesSubs WHERE MovieID = (?);"
                parId.Value = ID
                SQLcommand.ExecuteNonQuery()
                SQLcommand.CommandText = "DELETE FROM MoviesPosters WHERE MovieID = (?);"
                parId.Value = ID
                SQLcommand.ExecuteNonQuery()
                SQLcommand.CommandText = "DELETE FROM MoviesFanart WHERE MovieID = (?);"
                parId.Value = ID
                SQLcommand.ExecuteNonQuery()
                SQLcommand.CommandText = "DELETE FROM MoviesSets WHERE MovieID = (?);"
                parId.Value = ID
                SQLcommand.ExecuteNonQuery()
            End Using
            If Not BatchMode Then SQLtransaction.Commit()
        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

End Class
