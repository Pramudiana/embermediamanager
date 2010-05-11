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

Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports System.Drawing.Drawing2D
Imports System.Threading

Public Class dlgNMTMovies

#Region "Fields"

    Friend WithEvents bwBuildHTML As New System.ComponentModel.BackgroundWorker

    Private template_Path As String
    Private bCancelled As Boolean = False
    Private bexportFlags As Boolean = False
    Private bexportPosters As Boolean = False
    Private bexportBackDrops As Boolean = False
    Private bFiltered As Boolean = False
    Private DontSaveExtra As Boolean = False
    Dim FilterMovies As New List(Of Long)
    Dim FilterTVShows As New List(Of Long)
    Private HTMLMovieBody As New StringBuilder
    Private HTMLTVBody As New StringBuilder
    Private isCL As Boolean = False
    'Private TempPath As String = Path.Combine(Master.TempPath, "Export")
    Private use_filter As Boolean = False
    Private workerCanceled As Boolean = False

    Private sBasePath As String = Path.Combine(Path.Combine(Functions.AppPath, "Modules"), Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly.Location))
    Private conf As config
    Private confs As New List(Of config)
    Private selectedSources As New Hashtable
    Private outputFolder As String
    'Private dtMovieMedia As New List(Of Structures.DBMovie)
    Private Shared dtMovieMedia As DataTable = Nothing
    Private dtEpisodes As DataTable = Nothing
    Private dtSeasons As DataTable = Nothing
    Private dtShows As DataTable = Nothing

    Private MoviesGenres As New List(Of String)
#End Region 'Fields


#Region "Methods"
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        Try
            If Not dtMovieMedia Is Nothing Then
                dtMovieMedia = Nothing
            End If
            dtMovieMedia = New DataTable
            Master.DB.FillDataTable(dtMovieMedia, "SELECT * FROM movies ORDER BY ListTitle COLLATE NOCASE;")

            txtOutputFolder.Text = AdvancedSettings.GetSetting("BasePath", "")
            Dim fxml As String
            Dim di As DirectoryInfo = New DirectoryInfo(Path.Combine(sBasePath, "Templates"))
            For Each i As DirectoryInfo In di.GetDirectories
                If Not (i.Attributes And FileAttributes.Hidden) = FileAttributes.Hidden Then
                    fxml = Path.Combine(sBasePath, String.Concat("Templates", Path.DirectorySeparatorChar, i.Name))
                    conf = config.Load(Path.Combine(fxml, "config.xml"))
                    If Not String.IsNullOrEmpty(conf.Name) Then
                        conf.TemplatePath = fxml
                        confs.Add(conf)
                        cbTemplate.Items.Add(conf.Name)
                    End If
                End If
            Next
            If cbTemplate.Items.Count > 0 Then
                cbTemplate.SelectedIndex = 0
            End If
            dgvSources.ShowCellToolTips = True
            For Each s As Structures.MovieSource In Master.MovieSources
                Dim i As Integer = dgvSources.Rows.Add(New Object() {False, s.Name, My.Resources.film, String.Empty, "movie"})
                dgvSources.Rows(i).Cells(1).ToolTipText = s.Path
                dgvSources.Rows(i).Cells(3).Value = AdvancedSettings.GetSetting(String.Concat("Path.Movie.", s.Name), "")
                dgvSources.Rows(i).Cells(0).Value = AdvancedSettings.GetBooleanSetting(String.Concat("Path.Movie.Status.", s.Name), False)
            Next
            For Each s As Structures.TVSource In Master.TVSources
                Dim i As Integer = dgvSources.Rows.Add(New Object() {False, s.Name, My.Resources.television, String.Empty, "tv"})
                dgvSources.Rows(i).Cells(1).ToolTipText = s.Path
                dgvSources.Rows(i).Cells(3).Value = AdvancedSettings.GetSetting(String.Concat("Path.TV.", s.Name), "")
                dgvSources.Rows(i).Cells(0).Value = AdvancedSettings.GetBooleanSetting(String.Concat("Path.TV.Status.", s.Name), False)
            Next
            populateParams()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Public Sub SaveConfig()
        AdvancedSettings.SetSetting("BasePath", txtOutputFolder.Text)
        For Each r As config._Param In conf.Params
            AdvancedSettings.SetSetting(String.Concat("Param.", r.name), r.value)
        Next
        For Each r As DataGridViewRow In dgvSources.Rows
            AdvancedSettings.SetSetting(String.Concat("Path.Movie.", r.Cells(1).Value.ToString), r.Cells(3).Value.ToString)
            AdvancedSettings.SetBooleanSetting(String.Concat("Path.Movie.Status.", r.Cells(1).Value.ToString), Convert.ToBoolean(r.Cells(0).Value))
        Next
        'If Not conf Is Nothing Then conf.Save(Path.Combine(conf.TemplatePath, "config.xml"))
    End Sub

    Public Shared Sub ExportSingle()
        Try
            Dim MySelf As New dlgNMTMovies
            MySelf.isCL = True
            'MySelf.BuildHTML(False, String.Empty, String.Empty, template, False)
            'Dim srcPath As String = String.Concat(Functions.AppPath, "Langs", Path.DirectorySeparatorChar, "html", Path.DirectorySeparatorChar, template, Path.DirectorySeparatorChar)
            'MySelf.SaveAll(String.Empty, srcPath, filename, resizePoster)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Shared Sub CopyDirectory(ByVal SourcePath As String, ByVal DestPath As String, Optional ByVal Overwrite As Boolean = False)
        Dim SourceDir As DirectoryInfo = New DirectoryInfo(SourcePath)
        Dim DestDir As DirectoryInfo = New DirectoryInfo(DestPath)
        Dim IsRoot As Boolean = False

        ' the source directory must exist, otherwise throw an exception
        If SourceDir.Exists Then

            'is this a root directory?
            If DestDir.Root.FullName = DestDir.FullName Then
                IsRoot = True
            End If

            ' if destination SubDir's parent SubDir does not exist throw an exception (also check it isn't the root)
            If Not IsRoot AndAlso Not DestDir.Parent.Exists Then
                Throw New DirectoryNotFoundException _
                    ("Destination directory does not exist: " + DestDir.Parent.FullName)
            End If

            If Not DestDir.Exists Then
                DestDir.Create()
            End If

            ' copy all the files of the current directory
            Dim ChildFile As FileInfo
            For Each ChildFile In SourceDir.GetFiles()
                If (ChildFile.Attributes And FileAttributes.Hidden) = FileAttributes.Hidden OrElse Path.GetExtension(ChildFile.FullName) = ".htm" Then Continue For
                If Overwrite Then
                    ChildFile.CopyTo(Path.Combine(DestDir.FullName, ChildFile.Name), True)
                Else
                    ' if Overwrite = false, copy the file only if it does not exist
                    ' this is done to avoid an IOException if a file already exists
                    ' this way the other files can be copied anyway...
                    If Not File.Exists(Path.Combine(DestDir.FullName, ChildFile.Name)) Then
                        ChildFile.CopyTo(Path.Combine(DestDir.FullName, ChildFile.Name), False)
                    End If
                End If
            Next

            ' copy all the sub-directories by recursively calling this same routine
            Dim SubDir As DirectoryInfo
            For Each SubDir In SourceDir.GetDirectories()
                If (SubDir.Attributes And FileAttributes.Hidden) = FileAttributes.Hidden Then Continue For
                CopyDirectory(SubDir.FullName, Path.Combine(DestDir.FullName, _
                    SubDir.Name), Overwrite)
            Next
        Else
            Throw New DirectoryNotFoundException("Source directory does not exist: " + SourceDir.FullName)
        End If
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        'Me.Close()
        'If bwSaveAll.IsBusy Then
        'bwSaveAll.CancelAsync()
        'End If
        btnCancel.Enabled = False
    End Sub

    Private Sub BuildMovieHTML(ByVal template As String, ByVal outputbase As String, ByVal doNavigate As Boolean)
        Try
            ' Build HTML Documment in Code ... ugly but will work until new option
            Dim destPathShort As String = Path.Combine(outputbase, GetUserParam("MoviesDetailsPath", "html/").Replace("/", Path.DirectorySeparatorChar))
            HTMLMovieBody.Length = 0
            Dim sBasePath As String = Path.Combine(Path.Combine(Functions.AppPath, "Modules"), Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly.Location))
            Dim htmlPath As String = Path.Combine(template, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "movieindex").Name)
            Dim htmlDetailsPath As String = Path.Combine(template, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "movie").Name)
            Dim pattern As String = String.Empty
            Dim patternDetails As String = String.Empty
            Dim movieheader As String = String.Empty
            Dim moviefooter As String = String.Empty
            Dim movierow As String = String.Empty

            pattern = File.ReadAllText(htmlPath)
            patternDetails = File.ReadAllText(htmlDetailsPath)
            Dim s = pattern.IndexOf("<$MOVIE>")
            If s >= 0 Then
                Dim e = pattern.IndexOf("<$/MOVIE>")
                If e >= 0 Then
                    movieheader = pattern.Substring(0, s)
                    movierow = pattern.Substring(s + 8, e - s - 8)
                    moviefooter = pattern.Substring(e + 9, pattern.Length - e - 9)
                Else
                    'error
                End If
            Else
                'error
            End If

            HTMLMovieBody.Append(movieheader)
            Dim counter As Integer = 1
            FilterMovies.Clear()

            MoviesGenres.Clear()

            For Each _curMovie As DataRow In dtMovieMedia.Rows
                'now check if we need to include this movie
                If Not selectedSources.Contains(_curMovie.Item("Source").ToString) Then
                    bwBuildHTML.ReportProgress(1)
                    Continue For
                End If

                FilterMovies.Add(Convert.ToInt32(_curMovie.Item("ID")))
                HTMLMovieBody.Append(ProcessMovieTags(_curMovie, outputbase, counter, _curMovie.Item("ID").ToString, movierow))
                Dim detailsoutput As String = Path.Combine(Path.Combine(outputbase, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "movie").DestPath.Replace("/", Path.DirectorySeparatorChar)), String.Concat(_curMovie.Item("ID").ToString, ".htm"))
                File.WriteAllText(detailsoutput, ProcessMovieTags(_curMovie, detailsoutput, counter, _curMovie.Item("ID").ToString, patternDetails, GetUserParam("RelativePathToBase", "../../")))
                counter += 1
                bwBuildHTML.ReportProgress(1)
            Next
            HTMLMovieBody.Append(moviefooter)
            HTMLMovieBody.Replace("<$GENRES_LIST>", StringUtils.HtmlEncode(Strings.Join(MoviesGenres.ToArray, ",")))
            If Not Me.isCL Then
                DontSaveExtra = False
                Me.SaveMovieImages(Path.GetDirectoryName(htmlPath), outputbase)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub BuildTVHTML(ByVal template As String, ByVal outputbase As String, ByVal doNavigate As Boolean)
        Try
            ' Build HTML Documment in Code ... ugly but will work until new option
            Dim destPathShort As String = Path.Combine(outputbase, GetUserParam("TVDetailsPath", "html/").Replace("/", Path.DirectorySeparatorChar))
            HTMLTVBody.Length = 0
            Dim sBasePath As String = Path.Combine(Path.Combine(Functions.AppPath, "Modules"), Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly.Location))
            Dim htmlPath As String = Path.Combine(template, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvindex").Name)
            Dim htmlShowDetailsPath As String = Path.Combine(template, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvshow").Name)
            Dim htmlSeasonDetailsPath As String = Path.Combine(template, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvseason").Name)
            Dim htmlEpDetailsPath As String = Path.Combine(template, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvepisode").Name)
            Dim pattern As String = String.Empty
            Dim patternSeasonDetails As String = String.Empty
            Dim patternEpDetails As String = String.Empty
            Dim tvheader As String = String.Empty
            Dim tvfooter As String = String.Empty
            Dim tvrow As String = String.Empty

            pattern = File.ReadAllText(htmlPath)
            patternSeasonDetails = File.ReadAllText(htmlSeasonDetailsPath)
            patternEpDetails = File.ReadAllText(htmlEpDetailsPath)
            Dim s = pattern.IndexOf("<$TVSHOW>")
            If s >= 0 Then
                Dim e = pattern.IndexOf("<$/TVSHOW>")
                If e >= 0 Then
                    tvheader = pattern.Substring(0, s)
                    tvrow = pattern.Substring(s + 8, e - s - 8)
                    tvfooter = pattern.Substring(e + 9, pattern.Length - e - 9)
                Else
                    'error
                End If
            Else
                'error
            End If

            HTMLTVBody.Append(tvheader)
            Dim counter As Integer = 1
            FilterTVShows.Clear()

            'MoviesGenres.Clear()

            For Each _curShow As DataRow In dtShows.Rows
                'now check if we need to include this movie
                If Not selectedSources.Contains(_curShow.Item("Source").ToString) Then
                    bwBuildHTML.ReportProgress(1)
                    Continue For
                End If
                FilterTVShows.Add(Convert.ToInt32(_curShow.Item("ID")))
                HTMLTVBody.Append(ProcessTVShowsTags(_curShow, outputbase, counter, _curShow.Item("ID").ToString, tvrow))
                Dim detailsShowOutput As String = Path.Combine(Path.Combine(outputbase, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvshow").DestPath.Replace("/", Path.DirectorySeparatorChar)), String.Concat(_curShow.Item("ID").ToString, ".htm"))
                File.WriteAllText(detailsShowOutput, ProcessTVShowsTags(_curShow, detailsShowOutput, counter, _curShow.Item("ID").ToString, patternEpDetails, GetUserParam("RelativePathToBase", "../../")))

                For Each _curSeason As DataRow In dtSeasons.Select(String.Format("TVShowID = {0}", _curShow.Item("ID").ToString))
                    Dim detailsSeasonOutput As String = Path.Combine(Path.Combine(outputbase, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "tvshow").DestPath.Replace("/", Path.DirectorySeparatorChar)), String.Concat(_curShow.Item("ID").ToString, ".htm"))
                    File.WriteAllText(detailsShowOutput, ProcessTVShowsTags(_curShow, detailsShowOutput, counter, _curShow.Item("ID").ToString, patternEpDetails, GetUserParam("RelativePathToBase", "../../")))

                    For Each _curEp As DataRow In dtEpisodes.Select(String.Format("TVShowID = {0} AND Season = {1}", _curShow.Item("ID").ToString, _curSeason.Item("Season").ToString))

                        counter += 1
                        bwBuildHTML.ReportProgress(1)
                    Next
                Next
            Next
            HTMLTVBody.Append(tvfooter)
            'HTMLTVBody.Replace("<$GENRES_LIST>", StringUtils.HtmlEncode(Strings.Join(MoviesGenres.ToArray, ",")))
            If Not Me.isCL Then
                DontSaveExtra = False
                'Me.SaveMovieImages(Path.GetDirectoryName(htmlPath), outputbase)
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub


    Public Function GetMovieActorForID(ByVal id As String) As String
        Dim dtActorMedia = New DataTable
        Dim actors As String = String.Empty
        Master.DB.FillDataTable(dtActorMedia, String.Format("SELECT ActorName FROM MoviesActors WHERE MovieID = {0}", id))
        For Each s As DataRow In dtActorMedia.Rows
            actors = String.Concat(actors, If(Not String.IsNullOrEmpty(actors), " / ", String.Empty), s.Item("ActorNAme").ToString)
        Next
        Return actors
    End Function

    Function ProcessMovieTags(ByVal _curMovie As DataRow, ByVal outputbase As String, ByVal counter As Integer, ByVal id As String, ByVal movierow As String, Optional ByVal relpath As String = "") As String
        Dim row As String = movierow
        Try
            Dim tVid As New MediaInfo.Video
            Dim tAud As New MediaInfo.Audio
            Dim tRes As String = String.Empty
            Dim ThumbsPath As String = GetUserParam("TVThumbsPath", "TVThumbs/")
            Dim BackdropPath As String = GetUserParam("TVBackdropPath", "TVThumbs/")
            Dim uni As New UnicodeEncoding()
            Dim mapPath As String = If(String.IsNullOrEmpty(selectedSources(_curMovie.Item("Source").ToString).ToString), String.Concat(GetUserParam("RelativePathToBase", "../../"), Path.GetFileName(_curMovie.Item("Source").ToString)), selectedSources(_curMovie.Item("Source").ToString).ToString)
            Dim sourcePath As String = Master.MovieSources.FirstOrDefault(Function(y) y.Name = _curMovie.Item("Source").ToString).Path
            row = row.Replace("<$ID>", id.ToString)
            row = row.Replace("<$COUNTER>", counter.ToString)
            row = row.Replace("<$MOVIE_PATH>", _curMovie.Item("MoviePath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            row = row.Replace("<$POSTER_THUMB>", String.Concat(relpath, ThumbsPath, id.ToString, ".jpg"))
            row = row.Replace("<$BACKDROP_THUMB>", String.Concat(relpath, BackdropPath, id.ToString, "-backdrop.jpg"))
            row = row.Replace("<$POSTER_FILE>", _curMovie.Item("PosterPath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            row = row.Replace("<$FANART_FILE>", _curMovie.Item("FanartPath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            If Not String.IsNullOrEmpty(_curMovie.Item("Title").ToString) Then
                row = row.Replace("<$MOVIENAME>", StringUtils.HtmlEncode(_curMovie.Item("Title").ToString))
            Else
                row = row.Replace("<$MOVIENAME>", StringUtils.HtmlEncode(_curMovie.Item("ListTitle").ToString))
            End If
            row = row.Replace("<$ACTORS>", StringUtils.HtmlEncode(GetMovieActorForID(_curMovie.Item("ID").ToString)))
            row = row.Replace("<$DIRECTOR>", StringUtils.HtmlEncode(_curMovie.Item("Director").ToString))
            row = row.Replace("<$CERTIFICATION>", StringUtils.HtmlEncode(_curMovie.Item("Certification").ToString))
            row = row.Replace("<$IMDBID>", StringUtils.HtmlEncode(_curMovie.Item("IMDB").ToString))
            row = row.Replace("<$MPAA>", StringUtils.HtmlEncode(_curMovie.Item("MPAA").ToString))
            row = row.Replace("<$RELEASEDATE>", StringUtils.HtmlEncode(_curMovie.Item("ReleaseDate").ToString))
            row = row.Replace("<$RUNTIME>", StringUtils.HtmlEncode(_curMovie.Item("Runtime").ToString))
            row = row.Replace("<$TAGLINE>", StringUtils.HtmlEncode(_curMovie.Item("Tagline").ToString))
            row = row.Replace("<$RATING>", StringUtils.HtmlEncode(_curMovie.Item("Rating").ToString))
            row = row.Replace("<$VOTES>", StringUtils.HtmlEncode(_curMovie.Item("Votes").ToString))
            row = row.Replace("<$LISTTITLE>", StringUtils.HtmlEncode(_curMovie.Item("ListTitle").ToString))
            row = row.Replace("<$YEAR>", _curMovie.Item("Year").ToString)
            'row = row.Replace("<$COUNT>", counter.ToString)
            row = row.Replace("<$FILENAME>", StringUtils.HtmlEncode(Path.GetFileName(_curMovie.Item("MoviePath").ToString)))
            row = row.Replace("<$DIRNAME>", StringUtils.HtmlEncode(Path.GetDirectoryName(_curMovie.Item("MoviePath").ToString)))
            row = row.Replace("<$OUTLINE>", StringUtils.HtmlEncode(_curMovie.Item("Outline").ToString))
            row = row.Replace("<$PLOT>", StringUtils.HtmlEncode(_curMovie.Item("Plot").ToString))
            row = row.Replace("<$GENRES>", StringUtils.HtmlEncode(_curMovie.Item("Genre").ToString))
            For Each s As String In _curMovie.Item("Genre").ToString.Split(New String() {"/"}, StringSplitOptions.RemoveEmptyEntries)
                If Not MoviesGenres.Contains(s.Trim) Then MoviesGenres.Add(s.Trim)
            Next
            row = row.Replace("<$SIZE>", StringUtils.HtmlEncode(MovieSize(_curMovie.Item("MoviePath").ToString).ToString))
            row = row.Replace("<$DATEADD>", StringUtils.HtmlEncode(Functions.ConvertFromUnixTimestamp(Convert.ToDouble(_curMovie.Item("DateAdd").ToString)).ToShortDateString))
            Dim fiAV As MediaInfo.Fileinfo = GetMovieFileInfo(_curMovie.Item("ID").ToString)
            Dim _vidDetails As String = String.Empty
            Dim _vidDimensions As String = String.Empty
            If Not IsNothing(fiAV) Then
                If fiAV.StreamDetails.Video.Count > 0 Then
                    tVid = NFO.GetBestVideo(fiAV)
                    tRes = NFO.GetResFromDimensions(tVid)
                    _vidDimensions = NFO.GetDimensionsFromVideo(tVid)
                    _vidDetails = String.Format("{0} / {1}", If(String.IsNullOrEmpty(tRes), Master.eLang.GetString(283, "Unknown", True), tRes), If(String.IsNullOrEmpty(tVid.Codec), Master.eLang.GetString(283, "Unknown", True), tVid.Codec)).ToUpper
                End If
            End If
            Dim _audDetails As String = String.Empty
            If fiAV.StreamDetails.Audio.Count > 0 Then
                tAud = NFO.GetBestAudio(fiAV, False)
                _audDetails = String.Format("{0} / {1}ch", If(String.IsNullOrEmpty(tAud.Codec), Master.eLang.GetString(283, "Unknown", True), tAud.Codec), If(String.IsNullOrEmpty(tAud.Channels), Master.eLang.GetString(283, "Unknown", True), tAud.Channels)).ToUpper
            End If
            row = row.Replace("<$VIDEO>", _vidDetails)
            row = row.Replace("<$VIDEO_DIMENSIONS>", _vidDimensions)
            row = row.Replace("<$AUDIO>", _audDetails)
            row = GetAVImages(fiAV, row, _curMovie.Item("MoviePath").ToString, relpath)
        Catch ex As Exception
        End Try

        Return row
    End Function

    Function ProcessTVShowsTags(ByVal _curShow As DataRow, ByVal outputbase As String, ByVal counter As Integer, ByVal id As String, ByVal movierow As String, Optional ByVal relpath As String = "") As String
        Dim row As String = movierow
        Try
            Dim tVid As New MediaInfo.Video
            Dim tAud As New MediaInfo.Audio
            Dim tRes As String = String.Empty
            Dim ThumbsPath As String = GetUserParam("ThumbsPath", "Thumbs/")
            Dim BackdropPath As String = GetUserParam("BackdropPath", "Thumbs/")
            Dim uni As New UnicodeEncoding()
            Dim mapPath As String = If(String.IsNullOrEmpty(selectedSources(_curShow.Item("Source").ToString).ToString), String.Concat(GetUserParam("RelativePathToBase", "../../"), Path.GetFileName(_curShow.Item("Source").ToString)), selectedSources(_curShow.Item("Source").ToString).ToString)
            Dim sourcePath As String = Master.MovieSources.FirstOrDefault(Function(y) y.Name = _curShow.Item("Source").ToString).Path
            row = row.Replace("<$ID>", id.ToString)
            row = row.Replace("<$COUNTER>", counter.ToString)
            row = row.Replace("<$MOVIE_PATH>", _curShow.Item("MoviePath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            row = row.Replace("<$POSTER_THUMB>", String.Concat(relpath, ThumbsPath, id.ToString, ".jpg"))
            row = row.Replace("<$BACKDROP_THUMB>", String.Concat(relpath, BackdropPath, id.ToString, "-backdrop.jpg"))
            row = row.Replace("<$POSTER_FILE>", _curShow.Item("PosterPath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            row = row.Replace("<$FANART_FILE>", _curShow.Item("FanartPath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            If Not String.IsNullOrEmpty(_curShow.Item("Title").ToString) Then
                row = row.Replace("<$MOVIENAME>", StringUtils.HtmlEncode(_curShow.Item("Title").ToString))
            Else
                row = row.Replace("<$MOVIENAME>", StringUtils.HtmlEncode(_curShow.Item("ListTitle").ToString))
            End If
            row = row.Replace("<$ACTORS>", StringUtils.HtmlEncode(GetMovieActorForID(_curShow.Item("ID").ToString)))
            row = row.Replace("<$DIRECTOR>", StringUtils.HtmlEncode(_curShow.Item("Director").ToString))
            row = row.Replace("<$CERTIFICATION>", StringUtils.HtmlEncode(_curShow.Item("Certification").ToString))
            row = row.Replace("<$IMDBID>", StringUtils.HtmlEncode(_curShow.Item("IMDB").ToString))
            row = row.Replace("<$MPAA>", StringUtils.HtmlEncode(_curShow.Item("MPAA").ToString))
            row = row.Replace("<$RELEASEDATE>", StringUtils.HtmlEncode(_curShow.Item("ReleaseDate").ToString))
            row = row.Replace("<$RUNTIME>", StringUtils.HtmlEncode(_curShow.Item("Runtime").ToString))
            row = row.Replace("<$TAGLINE>", StringUtils.HtmlEncode(_curShow.Item("Tagline").ToString))
            row = row.Replace("<$RATING>", StringUtils.HtmlEncode(_curShow.Item("Rating").ToString))
            row = row.Replace("<$VOTES>", StringUtils.HtmlEncode(_curShow.Item("Votes").ToString))
            row = row.Replace("<$LISTTITLE>", StringUtils.HtmlEncode(_curShow.Item("ListTitle").ToString))
            row = row.Replace("<$YEAR>", _curShow.Item("Year").ToString)
            'row = row.Replace("<$COUNT>", counter.ToString)
            row = row.Replace("<$FILENAME>", StringUtils.HtmlEncode(Path.GetFileName(_curShow.Item("MoviePath").ToString)))
            row = row.Replace("<$DIRNAME>", StringUtils.HtmlEncode(Path.GetDirectoryName(_curShow.Item("MoviePath").ToString)))
            row = row.Replace("<$OUTLINE>", StringUtils.HtmlEncode(_curShow.Item("Outline").ToString))
            row = row.Replace("<$PLOT>", StringUtils.HtmlEncode(_curShow.Item("Plot").ToString))
            row = row.Replace("<$GENRES>", StringUtils.HtmlEncode(_curShow.Item("Genre").ToString))
            For Each s As String In _curShow.Item("Genre").ToString.Split(New String() {"/"}, StringSplitOptions.RemoveEmptyEntries)
                If Not MoviesGenres.Contains(s.Trim) Then MoviesGenres.Add(s.Trim)
            Next
            row = row.Replace("<$SIZE>", StringUtils.HtmlEncode(MovieSize(_curShow.Item("MoviePath").ToString).ToString))
            row = row.Replace("<$DATEADD>", StringUtils.HtmlEncode(Functions.ConvertFromUnixTimestamp(Convert.ToDouble(_curShow.Item("DateAdd").ToString)).ToShortDateString))
            Dim fiAV As MediaInfo.Fileinfo = GetMovieFileInfo(_curShow.Item("ID").ToString)
            Dim _vidDetails As String = String.Empty
            Dim _vidDimensions As String = String.Empty
            If Not IsNothing(fiAV) Then
                If fiAV.StreamDetails.Video.Count > 0 Then
                    tVid = NFO.GetBestVideo(fiAV)
                    tRes = NFO.GetResFromDimensions(tVid)
                    _vidDimensions = NFO.GetDimensionsFromVideo(tVid)
                    _vidDetails = String.Format("{0} / {1}", If(String.IsNullOrEmpty(tRes), Master.eLang.GetString(283, "Unknown", True), tRes), If(String.IsNullOrEmpty(tVid.Codec), Master.eLang.GetString(283, "Unknown", True), tVid.Codec)).ToUpper
                End If
            End If
            Dim _audDetails As String = String.Empty
            If fiAV.StreamDetails.Audio.Count > 0 Then
                tAud = NFO.GetBestAudio(fiAV, False)
                _audDetails = String.Format("{0} / {1}ch", If(String.IsNullOrEmpty(tAud.Codec), Master.eLang.GetString(283, "Unknown", True), tAud.Codec), If(String.IsNullOrEmpty(tAud.Channels), Master.eLang.GetString(283, "Unknown", True), tAud.Channels)).ToUpper
            End If
            row = row.Replace("<$VIDEO>", _vidDetails)
            row = row.Replace("<$VIDEO_DIMENSIONS>", _vidDimensions)
            row = row.Replace("<$AUDIO>", _audDetails)
            row = GetAVImages(fiAV, row, _curShow.Item("MoviePath").ToString, relpath)
        Catch ex As Exception
        End Try

        Return row
    End Function

    Function ProcessTVSeasonTags(ByVal _curSeason As DataRow, ByVal outputbase As String, ByVal counter As Integer, ByVal id As String, ByVal movierow As String, Optional ByVal relpath As String = "") As String
        Dim row As String = movierow
        Try
            Dim tVid As New MediaInfo.Video
            Dim tAud As New MediaInfo.Audio
            Dim tRes As String = String.Empty
            Dim ThumbsPath As String = GetUserParam("TVThumbsPath", "TVThumbs/")
            Dim BackdropPath As String = GetUserParam("TVBackdropPath", "TVThumbs/")
            Dim uni As New UnicodeEncoding()
            Dim mapPath As String = If(String.IsNullOrEmpty(selectedSources(_curSeason.Item("Source").ToString).ToString), String.Concat(GetUserParam("RelativePathToBase", "../../"), Path.GetFileName(_curSeason.Item("Source").ToString)), selectedSources(_curSeason.Item("Source").ToString).ToString)
            Dim sourcePath As String = Master.MovieSources.FirstOrDefault(Function(y) y.Name = _curSeason.Item("Source").ToString).Path
            row = row.Replace("<$ID>", id.ToString)
            row = row.Replace("<$COUNTER>", counter.ToString)
            row = row.Replace("<$MOVIE_PATH>", _curSeason.Item("MoviePath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            row = row.Replace("<$POSTER_THUMB>", String.Concat(relpath, ThumbsPath, id.ToString, ".jpg"))
            row = row.Replace("<$BACKDROP_THUMB>", String.Concat(relpath, BackdropPath, id.ToString, "-backdrop.jpg"))
            row = row.Replace("<$POSTER_FILE>", _curSeason.Item("PosterPath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            row = row.Replace("<$FANART_FILE>", _curSeason.Item("FanartPath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            If Not String.IsNullOrEmpty(_curSeason.Item("Title").ToString) Then
                row = row.Replace("<$MOVIENAME>", StringUtils.HtmlEncode(_curSeason.Item("Title").ToString))
            Else
                row = row.Replace("<$MOVIENAME>", StringUtils.HtmlEncode(_curSeason.Item("ListTitle").ToString))
            End If
            row = row.Replace("<$ACTORS>", StringUtils.HtmlEncode(GetMovieActorForID(_curSeason.Item("ID").ToString)))
            row = row.Replace("<$DIRECTOR>", StringUtils.HtmlEncode(_curSeason.Item("Director").ToString))
            row = row.Replace("<$CERTIFICATION>", StringUtils.HtmlEncode(_curSeason.Item("Certification").ToString))
            row = row.Replace("<$IMDBID>", StringUtils.HtmlEncode(_curSeason.Item("IMDB").ToString))
            row = row.Replace("<$MPAA>", StringUtils.HtmlEncode(_curSeason.Item("MPAA").ToString))
            row = row.Replace("<$RELEASEDATE>", StringUtils.HtmlEncode(_curSeason.Item("ReleaseDate").ToString))
            row = row.Replace("<$RUNTIME>", StringUtils.HtmlEncode(_curSeason.Item("Runtime").ToString))
            row = row.Replace("<$TAGLINE>", StringUtils.HtmlEncode(_curSeason.Item("Tagline").ToString))
            row = row.Replace("<$RATING>", StringUtils.HtmlEncode(_curSeason.Item("Rating").ToString))
            row = row.Replace("<$VOTES>", StringUtils.HtmlEncode(_curSeason.Item("Votes").ToString))
            row = row.Replace("<$LISTTITLE>", StringUtils.HtmlEncode(_curSeason.Item("ListTitle").ToString))
            row = row.Replace("<$YEAR>", _curSeason.Item("Year").ToString)
            'row = row.Replace("<$COUNT>", counter.ToString)
            row = row.Replace("<$FILENAME>", StringUtils.HtmlEncode(Path.GetFileName(_curSeason.Item("MoviePath").ToString)))
            row = row.Replace("<$DIRNAME>", StringUtils.HtmlEncode(Path.GetDirectoryName(_curSeason.Item("MoviePath").ToString)))
            row = row.Replace("<$OUTLINE>", StringUtils.HtmlEncode(_curSeason.Item("Outline").ToString))
            row = row.Replace("<$PLOT>", StringUtils.HtmlEncode(_curSeason.Item("Plot").ToString))
            row = row.Replace("<$GENRES>", StringUtils.HtmlEncode(_curSeason.Item("Genre").ToString))
            For Each s As String In _curSeason.Item("Genre").ToString.Split(New String() {"/"}, StringSplitOptions.RemoveEmptyEntries)
                If Not MoviesGenres.Contains(s.Trim) Then MoviesGenres.Add(s.Trim)
            Next
            row = row.Replace("<$SIZE>", StringUtils.HtmlEncode(MovieSize(_curSeason.Item("MoviePath").ToString).ToString))
            row = row.Replace("<$DATEADD>", StringUtils.HtmlEncode(Functions.ConvertFromUnixTimestamp(Convert.ToDouble(_curSeason.Item("DateAdd").ToString)).ToShortDateString))
            Dim fiAV As MediaInfo.Fileinfo = GetMovieFileInfo(_curSeason.Item("ID").ToString)
            Dim _vidDetails As String = String.Empty
            Dim _vidDimensions As String = String.Empty
            If Not IsNothing(fiAV) Then
                If fiAV.StreamDetails.Video.Count > 0 Then
                    tVid = NFO.GetBestVideo(fiAV)
                    tRes = NFO.GetResFromDimensions(tVid)
                    _vidDimensions = NFO.GetDimensionsFromVideo(tVid)
                    _vidDetails = String.Format("{0} / {1}", If(String.IsNullOrEmpty(tRes), Master.eLang.GetString(283, "Unknown", True), tRes), If(String.IsNullOrEmpty(tVid.Codec), Master.eLang.GetString(283, "Unknown", True), tVid.Codec)).ToUpper
                End If
            End If
            Dim _audDetails As String = String.Empty
            If fiAV.StreamDetails.Audio.Count > 0 Then
                tAud = NFO.GetBestAudio(fiAV, False)
                _audDetails = String.Format("{0} / {1}ch", If(String.IsNullOrEmpty(tAud.Codec), Master.eLang.GetString(283, "Unknown", True), tAud.Codec), If(String.IsNullOrEmpty(tAud.Channels), Master.eLang.GetString(283, "Unknown", True), tAud.Channels)).ToUpper
            End If
            row = row.Replace("<$VIDEO>", _vidDetails)
            row = row.Replace("<$VIDEO_DIMENSIONS>", _vidDimensions)
            row = row.Replace("<$AUDIO>", _audDetails)
            row = GetAVImages(fiAV, row, _curSeason.Item("MoviePath").ToString, relpath)
        Catch ex As Exception
        End Try

        Return row
    End Function

    Function ProcessTVEpisodeTags(ByVal _curEpisode As DataRow, ByVal outputbase As String, ByVal counter As Integer, ByVal id As String, ByVal movierow As String, Optional ByVal relpath As String = "") As String
        Dim row As String = movierow
        Try
            Dim tVid As New MediaInfo.Video
            Dim tAud As New MediaInfo.Audio
            Dim tRes As String = String.Empty
            Dim ThumbsPath As String = GetUserParam("TVThumbsPath", "TVThumbs/")
            Dim BackdropPath As String = GetUserParam("TVBackdropPath", "TVThumbs/")
            Dim uni As New UnicodeEncoding()
            Dim mapPath As String = If(String.IsNullOrEmpty(selectedSources(_curEpisode.Item("Source").ToString).ToString), String.Concat(GetUserParam("RelativePathToBase", "../../"), Path.GetFileName(_curEpisode.Item("Source").ToString)), selectedSources(_curEpisode.Item("Source").ToString).ToString)
            Dim sourcePath As String = Master.MovieSources.FirstOrDefault(Function(y) y.Name = _curEpisode.Item("Source").ToString).Path
            row = row.Replace("<$ID>", id.ToString)
            row = row.Replace("<$COUNTER>", counter.ToString)
            row = row.Replace("<$MOVIE_PATH>", _curEpisode.Item("MoviePath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            row = row.Replace("<$POSTER_THUMB>", String.Concat(relpath, ThumbsPath, id.ToString, ".jpg"))
            row = row.Replace("<$BACKDROP_THUMB>", String.Concat(relpath, BackdropPath, id.ToString, "-backdrop.jpg"))
            row = row.Replace("<$POSTER_FILE>", _curEpisode.Item("PosterPath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            row = row.Replace("<$FANART_FILE>", _curEpisode.Item("FanartPath").ToString.Replace(sourcePath, mapPath).Replace(Path.DirectorySeparatorChar, "/"))
            If Not String.IsNullOrEmpty(_curEpisode.Item("Title").ToString) Then
                row = row.Replace("<$MOVIENAME>", StringUtils.HtmlEncode(_curEpisode.Item("Title").ToString))
            Else
                row = row.Replace("<$MOVIENAME>", StringUtils.HtmlEncode(_curEpisode.Item("ListTitle").ToString))
            End If
            row = row.Replace("<$ACTORS>", StringUtils.HtmlEncode(GetMovieActorForID(_curEpisode.Item("ID").ToString)))
            row = row.Replace("<$DIRECTOR>", StringUtils.HtmlEncode(_curEpisode.Item("Director").ToString))
            row = row.Replace("<$CERTIFICATION>", StringUtils.HtmlEncode(_curEpisode.Item("Certification").ToString))
            row = row.Replace("<$IMDBID>", StringUtils.HtmlEncode(_curEpisode.Item("IMDB").ToString))
            row = row.Replace("<$MPAA>", StringUtils.HtmlEncode(_curEpisode.Item("MPAA").ToString))
            row = row.Replace("<$RELEASEDATE>", StringUtils.HtmlEncode(_curEpisode.Item("ReleaseDate").ToString))
            row = row.Replace("<$RUNTIME>", StringUtils.HtmlEncode(_curEpisode.Item("Runtime").ToString))
            row = row.Replace("<$TAGLINE>", StringUtils.HtmlEncode(_curEpisode.Item("Tagline").ToString))
            row = row.Replace("<$RATING>", StringUtils.HtmlEncode(_curEpisode.Item("Rating").ToString))
            row = row.Replace("<$VOTES>", StringUtils.HtmlEncode(_curEpisode.Item("Votes").ToString))
            row = row.Replace("<$LISTTITLE>", StringUtils.HtmlEncode(_curEpisode.Item("ListTitle").ToString))
            row = row.Replace("<$YEAR>", _curEpisode.Item("Year").ToString)
            'row = row.Replace("<$COUNT>", counter.ToString)
            row = row.Replace("<$FILENAME>", StringUtils.HtmlEncode(Path.GetFileName(_curEpisode.Item("MoviePath").ToString)))
            row = row.Replace("<$DIRNAME>", StringUtils.HtmlEncode(Path.GetDirectoryName(_curEpisode.Item("MoviePath").ToString)))
            row = row.Replace("<$OUTLINE>", StringUtils.HtmlEncode(_curEpisode.Item("Outline").ToString))
            row = row.Replace("<$PLOT>", StringUtils.HtmlEncode(_curEpisode.Item("Plot").ToString))
            row = row.Replace("<$GENRES>", StringUtils.HtmlEncode(_curEpisode.Item("Genre").ToString))
            For Each s As String In _curEpisode.Item("Genre").ToString.Split(New String() {"/"}, StringSplitOptions.RemoveEmptyEntries)
                If Not MoviesGenres.Contains(s.Trim) Then MoviesGenres.Add(s.Trim)
            Next
            row = row.Replace("<$SIZE>", StringUtils.HtmlEncode(MovieSize(_curEpisode.Item("MoviePath").ToString).ToString))
            row = row.Replace("<$DATEADD>", StringUtils.HtmlEncode(Functions.ConvertFromUnixTimestamp(Convert.ToDouble(_curEpisode.Item("DateAdd").ToString)).ToShortDateString))
            Dim fiAV As MediaInfo.Fileinfo = GetMovieFileInfo(_curEpisode.Item("ID").ToString)
            Dim _vidDetails As String = String.Empty
            Dim _vidDimensions As String = String.Empty
            If Not IsNothing(fiAV) Then
                If fiAV.StreamDetails.Video.Count > 0 Then
                    tVid = NFO.GetBestVideo(fiAV)
                    tRes = NFO.GetResFromDimensions(tVid)
                    _vidDimensions = NFO.GetDimensionsFromVideo(tVid)
                    _vidDetails = String.Format("{0} / {1}", If(String.IsNullOrEmpty(tRes), Master.eLang.GetString(283, "Unknown", True), tRes), If(String.IsNullOrEmpty(tVid.Codec), Master.eLang.GetString(283, "Unknown", True), tVid.Codec)).ToUpper
                End If
            End If
            Dim _audDetails As String = String.Empty
            If fiAV.StreamDetails.Audio.Count > 0 Then
                tAud = NFO.GetBestAudio(fiAV, False)
                _audDetails = String.Format("{0} / {1}ch", If(String.IsNullOrEmpty(tAud.Codec), Master.eLang.GetString(283, "Unknown", True), tAud.Codec), If(String.IsNullOrEmpty(tAud.Channels), Master.eLang.GetString(283, "Unknown", True), tAud.Channels)).ToUpper
            End If
            row = row.Replace("<$VIDEO>", _vidDetails)
            row = row.Replace("<$VIDEO_DIMENSIONS>", _vidDimensions)
            row = row.Replace("<$AUDIO>", _audDetails)
            row = GetAVImages(fiAV, row, _curEpisode.Item("MoviePath").ToString, relpath)
        Catch ex As Exception
        End Try

        Return row
    End Function

    Private Function GetMovieFileInfo(ByVal MovieID As String) As MediaInfo.Fileinfo
        Dim fi As New MediaInfo.Fileinfo
        Using SQLcommand As SQLite.SQLiteCommand = Master.DB.SQLcn.CreateCommand
            SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesVStreams WHERE MovieID = ", MovieID, ";")
            Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                Dim video As MediaInfo.Video
                While SQLreader.Read
                    video = New MediaInfo.Video
                    If Not DBNull.Value.Equals(SQLreader("Video_Width")) Then video.Width = SQLreader("Video_Width").ToString
                    If Not DBNull.Value.Equals(SQLreader("Video_Height")) Then video.Height = SQLreader("Video_Height").ToString
                    If Not DBNull.Value.Equals(SQLreader("Video_Codec")) Then video.Codec = SQLreader("Video_Codec").ToString
                    If Not DBNull.Value.Equals(SQLreader("Video_Duration")) Then video.Duration = SQLreader("Video_Duration").ToString
                    If Not DBNull.Value.Equals(SQLreader("Video_ScanType")) Then video.Scantype = SQLreader("Video_ScanType").ToString
                    If Not DBNull.Value.Equals(SQLreader("Video_AspectDisplayRatio")) Then video.Aspect = SQLreader("Video_AspectDisplayRatio").ToString
                    If Not DBNull.Value.Equals(SQLreader("Video_Language")) Then video.Language = SQLreader("Video_Language").ToString
                    If Not DBNull.Value.Equals(SQLreader("Video_LongLanguage")) Then video.LongLanguage = SQLreader("Video_LongLanguage").ToString
                    fi.StreamDetails.Video.Add(video)
                End While
            End Using
        End Using

        Using SQLcommand As SQLite.SQLiteCommand = Master.DB.SQLcn.CreateCommand
            SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesAStreams WHERE MovieID = ", MovieID, ";")
            Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                Dim audio As MediaInfo.Audio
                While SQLreader.Read
                    audio = New MediaInfo.Audio
                    If Not DBNull.Value.Equals(SQLreader("Audio_Language")) Then audio.Language = SQLreader("Audio_Language").ToString
                    If Not DBNull.Value.Equals(SQLreader("Audio_LongLanguage")) Then audio.LongLanguage = SQLreader("Audio_LongLanguage").ToString
                    If Not DBNull.Value.Equals(SQLreader("Audio_Codec")) Then audio.Codec = SQLreader("Audio_Codec").ToString
                    If Not DBNull.Value.Equals(SQLreader("Audio_Channel")) Then audio.Channels = SQLreader("Audio_Channel").ToString
                    fi.StreamDetails.Audio.Add(audio)
                End While
            End Using
        End Using
        Using SQLcommand As SQLite.SQLiteCommand = Master.DB.SQLcn.CreateCommand
            SQLcommand.CommandText = String.Concat("SELECT * FROM MoviesSubs WHERE MovieID = ", MovieID, ";")
            Using SQLreader As SQLite.SQLiteDataReader = SQLcommand.ExecuteReader()
                Dim subtitle As MediaInfo.Subtitle
                While SQLreader.Read
                    subtitle = New MediaInfo.Subtitle
                    If Not DBNull.Value.Equals(SQLreader("Subs_Language")) Then subtitle.Language = SQLreader("Subs_Language").ToString
                    If Not DBNull.Value.Equals(SQLreader("Subs_LongLanguage")) Then subtitle.LongLanguage = SQLreader("Subs_LongLanguage").ToString
                    If Not DBNull.Value.Equals(SQLreader("Subs_Type")) Then subtitle.SubsType = SQLreader("Subs_Type").ToString
                    If Not DBNull.Value.Equals(SQLreader("Subs_Path")) Then subtitle.SubsPath = SQLreader("Subs_Path").ToString
                    fi.StreamDetails.Subtitle.Add(subtitle)
                End While
            End Using
        End Using
        Return fi
    End Function


    Private Sub cbTemplate_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbTemplate.MouseHover

        lblHelpa.Text = Master.eLang.GetString(5, "Choose a template")
    End Sub

    Private Sub cbTemplate_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbTemplate.MouseLeave
        lblHelpa.Text = ""
    End Sub

    Private Sub cbTemplate_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbTemplate.SelectedIndexChanged
        If cbTemplate.SelectedIndex >= 0 Then
            conf = confs(cbTemplate.SelectedIndex)
            template_Path = conf.TemplatePath
            populateParams()
            lblTemplateInfo.Text = conf.Description
            DontSaveExtra = False
        End If

    End Sub


    Private Sub dlgExportMovies_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        'FileUtils.Delete.DeleteDirectory(Me.TempPath)
    End Sub

    Private Sub dlgExportMovies_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.SetUp()
        btnSave.Enabled = False
    End Sub

    Private Sub populateParams()
        dgvSettings.Rows.Clear()
        For Each c As config._Param In conf.Params.OrderByDescending(Function(y) y.access)
            Dim i As Integer
            i = dgvSettings.Rows.Add(New Object() {c.name})
            If c.access = "user" AndAlso c.type = "bool" Then
                Dim cCell As New DataGridViewComboBoxCell()
                dgvSettings.Rows(i).Cells(1) = cCell
                Dim dcb As DataGridViewComboBoxCell = DirectCast(dgvSettings.Rows(i).Cells(1), DataGridViewComboBoxCell)
                dcb.DataSource = New String() {"true", "false"}
                dcb.Value = AdvancedSettings.GetSetting(String.Concat("Param.", c.name), c.value)
            Else
                Dim cCell As New DataGridViewTextBoxCell()
                dgvSettings.Rows(i).Cells(1) = cCell
                Dim dcb As DataGridViewTextBoxCell = DirectCast(dgvSettings.Rows(i).Cells(1), DataGridViewTextBoxCell)
                dcb.Value = AdvancedSettings.GetSetting(String.Concat("Param.", c.name), c.value)
                If c.access = "hidden" Then
                    dgvSettings.Rows(i).Visible = False
                End If
            End If
            If Not c.access = "user" Then
                dgvSettings.Rows(i).ReadOnly = True
                dgvSettings.Rows(i).DefaultCellStyle.ForeColor = Color.Gray
                dgvSettings.Rows(i).DefaultCellStyle.SelectionBackColor = Color.White
                dgvSettings.Rows(i).DefaultCellStyle.SelectionForeColor = Color.Gray
            End If
        Next
    End Sub
    Private Sub SetAllUserParam()
        For Each r As DataGridViewRow In dgvSettings.Rows
            Dim r0 As DataGridViewRow = r
            Dim c As config._Param = conf.Params.FirstOrDefault(Function(y) y.name = r0.Cells(0).Value.ToString)
            If Not c Is Nothing Then c.value = r.Cells(1).Value.ToString
        Next
    End Sub

    Private Sub SetUserParam(ByVal param As String, ByVal value As String)
        Dim c As config._Param = conf.Params.FirstOrDefault(Function(y) y.name = param)
        If Not c Is Nothing Then
            c.value = value
        End If
    End Sub

    Private Function GetUserParam(ByVal param As String, ByVal defvalue As String) As String
        Dim c As config._Param = conf.Params.FirstOrDefault(Function(y) y.name = param)
        If Not c Is Nothing Then
            Return c.value
        Else
            Return defvalue
        End If
    End Function

    Private Sub dlgMoviesReport_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Activate()
    End Sub

    Private Sub DoCancel()
        btnCancel.Visible = False
        lblCompiling.Visible = False
        pbCompile.Style = ProgressBarStyle.Marquee
        pbCompile.MarqueeAnimationSpeed = 25
        lblCanceling.Visible = True
    End Sub
    Private Sub RotateImage(ByVal src As String, ByVal dst As String, ByVal rot As Integer)

    End Sub

    Private Sub ExportPoster(ByVal fpath As String, ByVal new_width As Integer)
        Try
            Dim finalpath As String = Path.Combine(fpath, GetUserParam("ThumbsPath", "Thumbs/").Replace("/", Path.DirectorySeparatorChar))
            Dim counter As Integer = 1
            Directory.CreateDirectory(finalpath)
            For Each _curMovie As DataRow In dtMovieMedia.Rows
                If Not FilterMovies.Contains(Convert.ToInt32(_curMovie.Item("ID"))) Then Continue For

                Try
                    Dim posterfile As String = Path.Combine(finalpath, String.Concat(_curMovie.Item("ID").ToString, ".jpg"))
                    If File.Exists(_curMovie.Item("PosterPath").ToString) Then
                        Dim im As New Images
                        If new_width > 0 Then

                            im.FromFile(_curMovie.Item("PosterPath").ToString)
                            ImageUtils.ResizeImage(im.Image, new_width, new_width, False, Color.Black.ToArgb)
                            im.Save(posterfile)

                            'RotateImage(im.Image, 5).Save(Path.Combine(finalpath, String.Concat("r-", counter.ToString, ".jpg")))
                        Else
                            File.Copy(_curMovie.Item("PosterPath").ToString, posterfile, True)
                        End If
                    End If
                Catch ex As Exception
                End Try
                counter += 1
                If bwBuildHTML.CancellationPending Then
                    Return
                End If
                bwBuildHTML.ReportProgress(1)
            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub


    Private Sub ExportBackDrops(ByVal fpath As String, ByVal new_width As Integer)
        Try
            Dim counter As Integer = 1
            Dim finalpath As String = Path.Combine(fpath, GetUserParam("BackdropPath", "Thumbs/").Replace("/", Path.DirectorySeparatorChar))
            Directory.CreateDirectory(finalpath)
            For Each _curMovie As DataRow In dtMovieMedia.Rows
                If Not FilterMovies.Contains(Convert.ToInt32(_curMovie.Item("ID"))) Then Continue For
                Try
                    Dim Fanartfile As String = Path.Combine(finalpath, String.Concat(_curMovie.Item("ID").ToString, "-backdrop.jpg"))
                    If File.Exists(_curMovie.Item("FanartPath").ToString) Then
                        If new_width > 0 Then
                            Dim im As New Images
                            im.FromFile(_curMovie.Item("FanartPath").ToString)
                            ImageUtils.ResizeImage(im.Image, new_width, new_width, False, Color.Black.ToArgb)
                            im.Save(Fanartfile, 65)
                        Else
                            File.Copy(_curMovie.Item("FanartPath").ToString, Fanartfile, True)
                        End If
                    End If

                Catch
                End Try
                counter += 1
                If bwBuildHTML.CancellationPending Then
                    Return
                End If
                bwBuildHTML.ReportProgress(1)
            Next
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Function GetAVImages(ByVal fiAV As MediaInfo.Fileinfo, ByVal line As String, ByVal filename As String, Optional ByVal relpath As String = "") As String
        If APIXML.lFlags.Count > 0 Then
            Try
                Dim flagspath As String = GetUserParam("FlagsPath", "Flags/")
                Dim tVideo As MediaInfo.Video = NFO.GetBestVideo(fiAV)
                Dim tAudio As MediaInfo.Audio = NFO.GetBestAudio(fiAV, False)

                Dim vresFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = NFO.GetResFromDimensions(tVideo).ToLower AndAlso f.Type = APIXML.FlagType.VideoResolution)
                If Not IsNothing(vresFlag) Then
                    line = line.Replace("<$FLAG_VRES>", String.Concat(relpath, flagspath, Path.GetFileName(vresFlag.Path))).Replace("\", "/")
                Else
                    vresFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = APIXML.FlagType.VideoResolution)
                    If Not IsNothing(vresFlag) Then
                        line = line.Replace("<$FLAG_VRES>", String.Concat(relpath, flagspath, Path.GetFileName(vresFlag.Path))).Replace("\", "/")
                    End If
                End If

                Dim vsourceFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = APIXML.GetFileSource(filename) AndAlso f.Type = APIXML.FlagType.VideoSource)
                If Not IsNothing(vsourceFlag) Then
                    line = line.Replace("<$FLAG_VSOURCE>", String.Concat(relpath, flagspath, Path.GetFileName(vsourceFlag.Path))).Replace("\", "/")
                Else
                    vsourceFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = APIXML.FlagType.VideoSource)
                    If Not IsNothing(vsourceFlag) Then
                        line = line.Replace("<$FLAG_VSOURCE>", String.Concat(relpath, flagspath, Path.GetFileName(vsourceFlag.Path))).Replace("\", "/")
                    End If
                End If

                Dim vcodecFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = tVideo.Codec.ToLower AndAlso f.Type = APIXML.FlagType.VideoCodec)
                If Not IsNothing(vcodecFlag) Then
                    line = line.Replace("<$FLAG_VTYPE>", String.Concat(relpath, flagspath, Path.GetFileName(vcodecFlag.Path))).Replace("\", "/")
                Else
                    vcodecFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultscreen" AndAlso f.Type = APIXML.FlagType.VideoCodec)
                    If Not IsNothing(vcodecFlag) Then
                        line = line.Replace("<$FLAG_VTYPE>", String.Concat(relpath, flagspath, Path.GetFileName(vcodecFlag.Path))).Replace("\", "/")
                    End If
                End If

                Dim acodecFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = tAudio.Codec.ToLower AndAlso f.Type = APIXML.FlagType.AudioCodec)
                If Not IsNothing(acodecFlag) Then
                    line = line.Replace("<$FLAG_ATYPE>", String.Concat(relpath, flagspath, Path.GetFileName(acodecFlag.Path))).Replace("\", "/")
                Else
                    acodecFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultaudio" AndAlso f.Type = APIXML.FlagType.AudioCodec)
                    If Not IsNothing(acodecFlag) Then
                        line = line.Replace("<$FLAG_ATYPE>", String.Concat(relpath, flagspath, Path.GetFileName(acodecFlag.Path))).Replace("\", "/")
                    End If
                End If

                Dim achanFlag As APIXML.Flag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = tAudio.Channels AndAlso f.Type = APIXML.FlagType.AudioChan)
                If Not IsNothing(achanFlag) Then
                    line = line.Replace("<$FLAG_ACHAN>", String.Concat(relpath, flagspath, Path.GetFileName(achanFlag.Path))).Replace("\", "/")
                Else
                    achanFlag = APIXML.lFlags.FirstOrDefault(Function(f) f.Name = "defaultaudio" AndAlso f.Type = APIXML.FlagType.AudioChan)
                    If Not IsNothing(achanFlag) Then
                        line = line.Replace("<$FLAG_ACHAN>", String.Concat(relpath, flagspath, Path.GetFileName(achanFlag.Path))).Replace("\", "/")
                    End If
                End If

            Catch ex As Exception
                Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
            End Try
        End If
        Return line
    End Function

    Function MovieSize(ByVal spath As String) As Long
        Dim MovieFilesSize As Long = 0
        If StringUtils.IsStacked(Path.GetFileNameWithoutExtension(spath), True) OrElse FileUtils.Common.isVideoTS(spath) OrElse FileUtils.Common.isBDRip(spath) Then
            Try
                Dim sExt As String = Path.GetExtension(spath).ToLower
                Dim oFile As String = StringUtils.CleanStackingMarkers(spath, False)
                Dim sFile As New List(Of String)
                Dim bIsVTS As Boolean = False

                If sExt = ".ifo" OrElse sExt = ".bup" OrElse sExt = ".vob" Then
                    bIsVTS = True
                End If

                If bIsVTS Then
                    Try
                        sFile.AddRange(Directory.GetFiles(Directory.GetParent(spath).FullName, "VTS*.VOB"))
                    Catch
                    End Try
                ElseIf sExt = ".m2ts" Then
                    Try
                        sFile.AddRange(Directory.GetFiles(Directory.GetParent(spath).FullName, "*.m2ts"))
                    Catch
                    End Try
                Else
                    Try
                        sFile.AddRange(Directory.GetFiles(Directory.GetParent(spath).FullName, StringUtils.CleanStackingMarkers(Path.GetFileName(spath), True)))
                    Catch
                    End Try
                End If

                For Each File As String In sFile
                    MovieFilesSize += FileLen(File)
                Next
            Catch ex As Exception
            End Try
        End If
        Return MovieFilesSize
    End Function

    Private Sub SetUp()
        Me.Text = Master.eLang.GetString(1, "Jukebox Builder")
        Me.Close_Button.Text = Master.eLang.GetString(19, "Close", True)
        Me.lblCompiling.Text = Master.eLang.GetString(2, "Compiling Movie List...")
        Me.lblCanceling.Text = Master.eLang.GetString(3, "Canceling Compilation...")
        Me.btnCancel.Text = Master.eLang.GetString(167, "Cancel", True)
        Me.Label2.Text = Master.eLang.GetString(4, "Template")
        Me.Label1.Text = Master.eLang.GetString(14, "Output Folder")
        Me.btnSave.Text = Master.eLang.GetString(15, "Save Template Settings")
        Me.btnBuild.Text = Master.eLang.GetString(16, "Build")
        Me.gbHelp.Text = String.Concat("     ", Master.eLang.GetString(17, "Help"))
    End Sub

    Private Structure Arguments
        Dim destPath As String
        Dim resizePoster As Integer
        Dim srcPath As String
    End Structure

    Private Sub btnBuild_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBuild.Click
        Me.bexportPosters = If(GetUserParam("ExportPosters", "true").ToLower = "true", True, False)
        Me.bexportBackDrops = If(GetUserParam("ExportBackdrops", "true").ToLower = "true", True, False)
        Me.bexportFlags = If(GetUserParam("ExportFlags", "true").ToLower = "true", True, False)
        cbTemplate.Enabled = False
        txtOutputFolder.Enabled = False
        btnBuild.Enabled = False
        dgvSettings.Enabled = False
        dgvSources.Enabled = False
        pbCompile.Maximum = dtMovieMedia.Rows.Count + If(bexportPosters, dtMovieMedia.Rows.Count, 0) + If(bexportBackDrops, dtMovieMedia.Rows.Count, 0)
        pbCompile.Value = pbCompile.Minimum
        btnCancel.Visible = True
        btnCancel.Enabled = True
        lblCompiling.Visible = True
        pbCompile.Visible = True
        lblCompiling.Text = Master.eLang.GetString(2, "Compiling Movie List...")
        pbCompile.Style = ProgressBarStyle.Continuous
        pnlCancel.Visible = True
        pnlCancel.BringToFront()
        outputFolder = txtOutputFolder.Text
        Try
            If GetUserParam("CleanBasePath", "true").ToLower = "true" Then
                Dim mythread As New Thread(AddressOf DoDelete)
                Dim bpath As String = Path.Combine(outputFolder, GetUserParam("BasePath", ".Ember/").Replace("/", Path.DirectorySeparatorChar))
                If bpath = outputFolder Then
                    MessageBox.Show(Master.eLang.GetString(6, "BasePath can not be the same as Output Folder"), Master.eLang.GetString(7, "Warning"), MessageBoxButtons.OK)
                    Return
                End If
                mythread.Start(bpath)
                While mythread.IsAlive
                    Application.DoEvents()
                End While
            End If
            For Each s As config._Param In conf.Params.Where(Function(y) y.type = "path")
                If Not Directory.Exists(Path.Combine(outputFolder, s.value.Replace("/", Path.DirectorySeparatorChar))) Then
                    Try
                        Directory.CreateDirectory(Path.Combine(outputFolder, s.value.Replace("/", Path.DirectorySeparatorChar)))
                    Catch ex As Exception
                        Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
                        Return
                    End Try
                End If
            Next
            Me.bwBuildHTML.WorkerReportsProgress = True
            Me.bwBuildHTML.WorkerSupportsCancellation = True
            Me.bwBuildHTML.RunWorkerAsync()
            While bwBuildHTML.IsBusy
                Application.DoEvents()
            End While

        Catch ex As Exception
        End Try
        pnlCancel.Visible = False
        cbTemplate.Enabled = True
        txtOutputFolder.Enabled = True
        btnBuild.Enabled = True
        dgvSettings.Enabled = True
        dgvSources.Enabled = True
    End Sub
    Private Shared Sub DoDelete(ByVal state As Object)
        If DirectCast(state, String).Contains("..") Then Return
        Try
            Directory.Delete(DirectCast(state, String), True)
        Catch ex As Exception
        End Try
    End Sub
    Private Sub bwBuildHTML_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwBuildHTML.DoWork
        Try
            BuildMovieHTML(template_Path, outputFolder, False)
        Catch ex As Exception
        End Try
    End Sub
    Private Sub bwBuildHTML_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwBuildHTML.ProgressChanged
        If Not e.UserState Is Nothing AndAlso Not String.IsNullOrEmpty(e.UserState.ToString) Then
            'pbCompile.Style = ProgressBarStyle.Marquee
            'pbCompile.MarqueeAnimationSpeed = 25
            lblCompiling.Text = e.UserState.ToString '"Exporting Data..."
        Else
            pbCompile.Value += e.ProgressPercentage
        End If

    End Sub
    Private Sub SaveMovieImages(ByVal srcPath As String, ByVal destPath As String)
        Try
            bwBuildHTML.ReportProgress(0, Master.eLang.GetString(8, "Exporting Data..."))
            If Not DontSaveExtra Then
                For Each f As config._File In conf.Files.Where(Function(y) y.Type = "other")
                    File.Copy(Path.Combine(srcPath, f.Name), Path.Combine(Path.Combine(outputFolder, f.DestPath.Replace("/", Path.DirectorySeparatorChar)), f.Name), True)
                    'CopyDirectory(srcPath, Path.GetDirectoryName(destPath), True)
                Next
                For Each f As config._File In conf.Files.Where(Function(y) y.Type = "folder")
                    Dim srcf As String = Path.Combine(srcPath, f.Name)
                    Dim destf As String = Path.Combine(outputFolder, Path.Combine(f.DestPath.Replace("/", Path.DirectorySeparatorChar), f.Name))
                    If Not srcf.EndsWith(Path.DirectorySeparatorChar) Then srcf = String.Concat(srcf, Path.DirectorySeparatorChar)
                    If Not destf.EndsWith(Path.DirectorySeparatorChar) Then destf = String.Concat(destf, Path.DirectorySeparatorChar)
                    CopyDirectory(srcf, destf, True)
                Next
                If Me.bexportFlags Then
                    bwBuildHTML.ReportProgress(0, Master.eLang.GetString(9, "Exporting Flags..."))
                    srcPath = String.Concat(Functions.AppPath, "Images", Path.DirectorySeparatorChar, "Flags", Path.DirectorySeparatorChar)
                    Dim flagspath As String = Path.Combine(destPath, GetUserParam("FlagsPath", "Flags/").Replace("/", Path.DirectorySeparatorChar))
                    CopyDirectory(srcPath, flagspath, True)
                End If
                If bwBuildHTML.CancellationPending Then
                    Return
                End If
                If Me.bexportPosters Then
                    bwBuildHTML.ReportProgress(0, Master.eLang.GetString(10, "Exporting Posters..."))
                    Me.ExportPoster(destPath, Convert.ToInt32(GetUserParam("PostersThumbWidth", "160")))
                End If
                If Me.bexportBackDrops Then
                    bwBuildHTML.ReportProgress(0, Master.eLang.GetString(11, "Exporting Backdrops..."))
                    Me.ExportBackDrops(destPath, Convert.ToInt32(GetUserParam("BackdropWidth", "1280")))
                End If
                If bwBuildHTML.CancellationPending Then
                    Return
                End If
                DontSaveExtra = True
            End If
            Dim hfile As String = Path.Combine(Path.Combine(destPath, conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "movieindex").DestPath), conf.Files.FirstOrDefault(Function(y) y.Process = True AndAlso y.Type = "movieindex").Name)
            If File.Exists(hfile) Then
                System.IO.File.Delete(hfile)
            End If
            Dim myStream As Stream = File.OpenWrite(hfile)
            If Not IsNothing(myStream) Then
                myStream.Write(System.Text.Encoding.ASCII.GetBytes(HTMLMovieBody.ToString), 0, HTMLMovieBody.ToString.Length)
                myStream.Close()
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub dgvSources_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvSources.CellValueChanged
        ValidatedToBuild.Start()
        btnSave.Enabled = False
    End Sub

    Private Sub dgvSources_CurrentCellDirtyStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvSources.CurrentCellDirtyStateChanged
        If dgvSources.IsCurrentCellDirty Then
            dgvSources.CommitEdit(DataGridViewDataErrorContexts.Commit)
            btnSave.Enabled = True
        End If
    End Sub


    Private Sub dgvSources_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvSources.MouseHover
        lblHelpa.Text = String.Format(Master.eLang.GetString(12, "Use the NMT Path when the source is not on the same Drive/Share as the Output folder.{0}Some common paths are:{0}/opt/sybhttpd/localhost.drives/NETWORK_SHARE/[remote_filesystem_name]/[Path_to_Source]{0}/opt/sybhttpd/localhost.drives/HARD_DISK/USB_DRIVE_A-1/[Path_to_Source]"), vbCrLf)
    End Sub

    Private Sub dgvSources_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvSources.MouseLeave
        lblHelpa.Text = ""
    End Sub

    Private Sub ValidatedToBuild_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ValidatedToBuild.Tick
        ValidatedToBuild.Stop()
        selectedSources.Clear()
        For Each row As DataGridViewRow In dgvSources.Rows
            Dim dcb As DataGridViewCheckBoxCell = DirectCast(row.Cells(0), DataGridViewCheckBoxCell)
            If DirectCast(dcb.Value, Boolean) = True Then
                selectedSources.Add(row.Cells(1).Value.ToString, row.Cells(3).Value.ToString)
            End If
        Next
        If selectedSources.Count > 0 AndAlso Directory.Exists(txtOutputFolder.Text) Then
            btnBuild.Enabled = True
        Else
            btnBuild.Enabled = False
        End If
    End Sub

    Private Sub txtOutputFolder_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtOutputFolder.MouseHover
        lblHelpa.Text = Master.eLang.GetString(13, "Select Root Folder where Jukebox files will be exported")
    End Sub

    Private Sub txtOutputFolder_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtOutputFolder.MouseLeave
        lblHelpa.Text = ""
    End Sub

    Private Sub txtOutputFolder_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtOutputFolder.TextChanged
        ValidatedToBuild.Start()
        btnSave.Enabled = True
    End Sub

    Private Sub dgvSettings_CellMouseEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvSettings.CellMouseEnter
        If e.RowIndex >= 0 Then
            lblHelpa.Text = conf.Params.FirstOrDefault(Function(y) y.name = dgvSettings.Rows(e.RowIndex).Cells(0).Value.ToString).description
        End If

    End Sub

    Private Sub dgvSettings_CellMouseLeave(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvSettings.CellMouseLeave
        lblHelpa.Text = ""
    End Sub
    Private Sub lblTemplateInfo_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblTemplateInfo.MouseHover
        lblHelpa.Text = If(Not String.IsNullOrEmpty(conf.Author), conf.Author, String.Empty)
    End Sub

    Private Sub lblTemplateInfo_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblTemplateInfo.MouseLeave
        lblHelpa.Text = ""
    End Sub

    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click
        Try
            Using fbdBrowse As New System.Windows.Forms.FolderBrowserDialog
                If fbdBrowse.ShowDialog = Windows.Forms.DialogResult.OK Then
                    If Not String.IsNullOrEmpty(fbdBrowse.SelectedPath) Then
                        Me.txtOutputFolder.Text = fbdBrowse.SelectedPath
                    End If
                End If
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub
#End Region 'Methods

    Class config
        Public Name As String
        Public Description As String
        Public Author As String
        Public Version As String
        <XmlArrayItem("File")> _
        Public Files As New List(Of _File)
        <XmlArrayItem("Param")> _
        Public Params As New List(Of _Param)
        <XmlIgnore()> _
        Public TemplatePath As String
        Class _File
            Public Name As String
            Public DestPath As String
            Public Process As Boolean
            Public Type As String
        End Class
        Class _Param
            Public name As String
            Public type As String
            Public value As String
            Public access As String
            Public description As String
        End Class

        Public Sub Save(ByVal fpath As String)
            Dim xmlSer As New XmlSerializer(GetType(config))
            Using xmlSW As New StreamWriter(fpath)
                xmlSer.Serialize(xmlSW, Me)
            End Using
        End Sub
        Public Shared Function Load(ByVal fpath As String) As config
            If Not File.Exists(fpath) Then Return New config
            Dim xmlSer As XmlSerializer
            xmlSer = New XmlSerializer(GetType(config))
            Using xmlSW As New StreamReader(Path.Combine(Functions.AppPath, fpath))
                Return DirectCast(xmlSer.Deserialize(xmlSW), config)
            End Using
        End Function
    End Class

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        SaveConfig()
        btnSave.Enabled = False
    End Sub

    Private Sub dgvSettings_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvSettings.CellValueChanged
        btnSave.Enabled = True
        SetAllUserParam()
    End Sub

    Private Sub dgvSettings_CurrentCellDirtyStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvSettings.CurrentCellDirtyStateChanged
        If dgvSettings.IsCurrentCellDirty Then
            dgvSettings.CommitEdit(DataGridViewDataErrorContexts.Commit)
            btnSave.Enabled = True
        End If
    End Sub

End Class