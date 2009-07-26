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

Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public Class dlgExportMovies
    Private isCL As Boolean = False
    Dim HTMLBody As New StringBuilder
    Dim _movies As New List(Of Master.DBMovie)
    Dim bFiltered As Boolean = False
    Dim bCancelled As Boolean = False
    Friend WithEvents bwLoadInfo As New System.ComponentModel.BackgroundWorker

    Public Shared Sub CLExport(ByVal filename As String, Optional ByVal template As String = "template")
        Dim MySelf As New dlgExportMovies
        MySelf.isCL = True
        MySelf.bwLoadInfo = New System.ComponentModel.BackgroundWorker
        MySelf.bwLoadInfo.WorkerSupportsCancellation = True
        MySelf.bwLoadInfo.WorkerReportsProgress = True
        MySelf.bwLoadInfo.RunWorkerAsync()
        Do While MySelf.bwLoadInfo.IsBusy
            Application.DoEvents()
        Loop
        MySelf.BuildHTML(False, "", "", template)
        File.WriteAllText(filename, System.Text.Encoding.ASCII.GetString(System.Text.Encoding.ASCII.GetBytes(MySelf.HTMLBody.ToString)))

    End Sub


    Private Sub dlgExportMovies_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Me.bwLoadInfo.IsBusy Then
            Me.DoCancel()
            Do While Me.bwLoadInfo.IsBusy
                Application.DoEvents()
            Loop
        End If
    End Sub

    Private Sub bwLoadInfo_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadInfo.DoWork
        '//
        ' Thread to load movieinformation (from nfo)
        '\\
        Try
            ' Clean up Movies List if any
            _movies.Clear()
            ' Load nfo movies using path from DB
            Using SQLNewcommand As SQLite.SQLiteCommand = Master.DB.CreateCommand
                Dim _tmpMovie As New Master.DBMovie
                Dim _ID As Integer
                Dim iProg As Integer = 0
                SQLNewcommand.CommandText = String.Concat("SELECT COUNT(id) AS mcount FROM movies;")
                Using SQLcount As SQLite.SQLiteDataReader = SQLNewcommand.ExecuteReader()
                    Me.bwLoadInfo.ReportProgress(-1, SQLcount("mcount")) ' set maximum
                End Using
                SQLNewcommand.CommandText = String.Concat("SELECT ID FROM movies ORDER BY ListTitle ASC;")
                Using SQLreader As SQLite.SQLiteDataReader = SQLNewcommand.ExecuteReader()
                    If SQLreader.HasRows Then
                        While SQLreader.Read()
                            _ID = SQLreader("ID")
                            _tmpMovie = Master.DB.LoadMovieFromDB(_ID)
                            _movies.Add(_tmpMovie)
                            Me.bwLoadInfo.ReportProgress(iProg, _tmpMovie.ListTitle) '  show File
                            iProg += 1
                            If bwLoadInfo.CancellationPending Then
                                e.Cancel = True
                                Return
                            End If
                        End While
                        If Not Me.isCL Then
                            BuildHTML()
                        End If

                        e.Result = True
                    Else
                        e.Cancel = True
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub
    Sub BuildHTML(Optional ByVal bSearch As Boolean = False, Optional ByVal strFilter As String = "", Optional ByVal strIn As String = "", Optional ByVal template As String = "template")
        Try
            ' Build HTML Documment in Code ... ugly but will work until new option
            Dim tVid As New MediaInfo.Video
            Dim tAud As New MediaInfo.Audio
            Dim tRes As String = String.Empty
            Dim pattern As String = File.ReadAllText(String.Concat(Application.StartupPath, Path.DirectorySeparatorChar, "Langs", Path.DirectorySeparatorChar, template, "-", Master.eSettings.Language, ".html"))
            Dim movieheader As String = String.Empty
            Dim moviefooter As String = String.Empty
            Dim movierow As String = String.Empty
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

            If bSearch Then
                bFiltered = True
            Else
                bFiltered = False
            End If

            HTMLBody.Append(movieheader)
            Dim counter As Integer = 1
            For Each _curMovie As Master.DBMovie In _movies
                Dim _vidDetails As String = String.Empty
                Dim _audDetails As String = String.Empty
                If Not IsNothing(_curMovie.Movie.FileInfo) Then
                    If _curMovie.Movie.FileInfo.StreamDetails.Video.Count > 0 Then
                        tVid = NFO.GetBestVideo(_curMovie.Movie.FileInfo)
                        tRes = NFO.GetResFromDimensions(tVid)
                        _vidDetails = String.Format("{0} / {1}", If(String.IsNullOrEmpty(tRes), Master.eLang.GetString(283, "Unknown"), tRes), If(String.IsNullOrEmpty(tVid.Codec), Master.eLang.GetString(283, "Unknown"), tVid.Codec))
                    End If

                    If _curMovie.Movie.FileInfo.StreamDetails.Audio.Count > 0 Then
                        tAud = NFO.GetBestAudio(_curMovie.Movie.FileInfo)
                        _audDetails = String.Format("{0} / {1}ch", If(String.IsNullOrEmpty(tAud.Codec), Master.eLang.GetString(283, "Unknown"), tAud.Codec), If(String.IsNullOrEmpty(tAud.Channels), Master.eLang.GetString(283, "Unknown"), tAud.Channels))
                    End If
                End If

                Dim row As String = movierow
                row = row.Replace("<$MOVIENAME>", Web.HttpUtility.HtmlEncode(_curMovie.ListTitle))
                row = row.Replace("<$YEAR>", _curMovie.Movie.Year)
                row = row.Replace("<$COUNT>", counter.ToString)
                row = row.Replace("<$FILENAME>", Path.GetFileName(_curMovie.Filename))
                row = row.Replace("<$DIRNAME>", Path.GetDirectoryName(_curMovie.Filename))
                row = row.Replace("<$OUTLINE>", Web.HttpUtility.HtmlEncode(_curMovie.Movie.Outline))
                row = row.Replace("<$PLOT>", Web.HttpUtility.HtmlEncode(_curMovie.Movie.Plot))
                row = row.Replace("<$GENRES>", Web.HttpUtility.HtmlEncode(_curMovie.Movie.Genre))
                row = row.Replace("<$VIDEO>", _vidDetails)
                row = row.Replace("<$AUDIO>", _audDetails)
                If bSearch Then
                    If (strIn = Master.eLang.GetString(279, "Video Flag") AndAlso _vidDetails.Contains(strFilter)) OrElse _
                       (strIn = Master.eLang.GetString(280, "Audio Flag") AndAlso _audDetails.Contains(strFilter)) OrElse _
                       (strIn = Master.eLang.GetString(21, "Title") AndAlso _curMovie.Movie.Title.Contains(strFilter)) OrElse _
                       (strIn = Master.eLang.GetString(278, "Year") AndAlso _curMovie.Movie.Year.Contains(strFilter)) Then

                        HTMLBody.Append(row)
                    End If
                Else

                    HTMLBody.Append(row)
                End If
                counter += 1
            Next
            HTMLBody.Append(moviefooter)
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub


    Private Sub bwLoadInfo_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadInfo.RunWorkerCompleted
        '//
        ' Thread finished: display it if not cancelled
        '\\
        If Not Me.isCL Then
            bCancelled = e.Cancelled
            If Not e.Cancelled Then
                wbMovieList.DocumentText = HTMLBody.ToString
            Else
                wbMovieList.DocumentText = String.Concat("<center><h1 style=""color:Red;"">", Master.eLang.GetString(284, "Cancelled"), "</h1></center>")
            End If
            Me.pnlCancel.Visible = False
        End If
    End Sub

    Private Sub bwLoadInfo_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bwLoadInfo.ProgressChanged
        If e.ProgressPercentage >= 0 Then
            Me.pbCompile.Value = e.ProgressPercentage
            Me.lblFile.Text = e.UserState
        Else
            Me.pbCompile.Maximum = Convert.ToInt32(e.UserState)
        End If

    End Sub

    Private Sub DoCancel()
        Me.bwLoadInfo.CancelAsync()
        btnCancel.Visible = False
        lblCompiling.Visible = False
        pbCompile.Style = ProgressBarStyle.Marquee
        pbCompile.MarqueeAnimationSpeed = 25
        lblCanceling.Visible = True
        lblFile.Visible = False
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub WebBrowser1_DocumentCompleted(ByVal sender As System.Object, ByVal e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles wbMovieList.DocumentCompleted
        If Not bCancelled Then
            wbMovieList.Visible = True
            Me.Save_Button.Enabled = True
            pnlSearch.Enabled = True
            Reset_Button.Enabled = bFiltered
        End If
    End Sub


    Private Sub Save_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Save_Button.Click
        Dim saveHTML As New SaveFileDialog()
        Dim myStream As Stream
        saveHTML.Filter = "HTML files (*.htm)|*.htm"
        saveHTML.FilterIndex = 2
        saveHTML.RestoreDirectory = True

        If saveHTML.ShowDialog() = DialogResult.OK Then

            myStream = saveHTML.OpenFile()
            If Not IsNothing(myStream) Then
                myStream.Write(System.Text.Encoding.ASCII.GetBytes(wbMovieList.DocumentText), 0, wbMovieList.DocumentText.Length)
                myStream.Close()
            End If
        End If

    End Sub

    Private Sub Search_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Search_Button.Click
        pnlSearch.Enabled = False
        BuildHTML(True, txtSearch.Text, cbSearch.Text)
        wbMovieList.DocumentText = HTMLBody.ToString
    End Sub

    Private Sub txtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSearch.TextChanged
        If txtSearch.Text <> "" And cbSearch.Text <> "" Then
            Search_Button.Enabled = True
        Else
            Search_Button.Enabled = False
        End If
    End Sub

    Private Sub cbSearch_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbSearch.SelectedIndexChanged
        If txtSearch.Text <> "" And cbSearch.Text <> "" Then
            Search_Button.Enabled = True
        Else
            Search_Button.Enabled = False
        End If
    End Sub

    Private Sub Reset_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Reset_Button.Click
        pnlSearch.Enabled = False
        BuildHTML(False)
        wbMovieList.DocumentText = HTMLBody.ToString
    End Sub

    Private Sub dlgExportMovies_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.SetUp()
    End Sub

    Private Sub dlgMoviesReport_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown

        ' Show Cancel Panel
        btnCancel.Visible = True
        lblCompiling.Visible = True
        pbCompile.Visible = True
        pbCompile.Style = ProgressBarStyle.Continuous
        lblCanceling.Visible = False
        pnlCancel.Visible = True
        Application.DoEvents()

        Me.Activate()

        'Start worker
        Me.bwLoadInfo = New System.ComponentModel.BackgroundWorker
        Me.bwLoadInfo.WorkerSupportsCancellation = True
        Me.bwLoadInfo.WorkerReportsProgress = True
        Me.bwLoadInfo.RunWorkerAsync()

    End Sub

    Private Sub SetUp()

        Me.Text = Master.eLang.GetString(272, "Export Movies")
        Me.Save_Button.Text = Master.eLang.GetString(272, "Save")
        Me.Close_Button.Text = Master.eLang.GetString(19, "Close")
        Me.Reset_Button.Text = Master.eLang.GetString(274, "Reset")
        Me.Label1.Text = Master.eLang.GetString(275, "Filter")
        Me.Search_Button.Text = Master.eLang.GetString(276, "Apply")
        Me.lblIn.Text = Master.eLang.GetString(277, "in")
        Me.lblCompiling.Text = Master.eLang.GetString(165, "Compiling Movie List...")
        Me.lblCanceling.Text = Master.eLang.GetString(166, "Canceling Compilation...")
        Me.btnCancel.Text = Master.eLang.GetString(167, "Cancel")

        Me.cbSearch.Items.AddRange(New Object() {Master.eLang.GetString(21, "Title"), Master.eLang.GetString(278, "Year"), Master.eLang.GetString(279, "Video Flag"), Master.eLang.GetString(280, "Audio Flag")})

    End Sub
End Class


