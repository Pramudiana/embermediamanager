﻿Imports System.IO
Imports System.Text.RegularExpressions

Public Class ThumbGenerator

    Public _movie As Master.DBMovie
    Public _thumbcount As Integer
    Public _isedit As Boolean
    Public _setfa As String

    Private ffmpeg As New Process
    Private isAborting As Boolean = False

    Public WriteOnly Property Movie() As Master.DBMovie
        Set(ByVal value As Master.DBMovie)
            _movie = value
        End Set
    End Property

    Public WriteOnly Property ThumbCount() As Integer
        Set(ByVal value As Integer)
            _thumbcount = value
        End Set
    End Property

    Public WriteOnly Property isEdit() As Boolean
        Set(ByVal value As Boolean)
            _isedit = value
        End Set
    End Property

    Public ReadOnly Property SetFA() As String
        Get
            Return _setfa
        End Get
    End Property

    ''' <summary>
    ''' Start the thread which extracts the thumbs from the movie file.
    ''' </summary>
    ''' <remarks>Originally meant to keep the GUI from locking during frame extraction, but checking timeout locks the main thread anyway :/</remarks>
    Public Sub Start()
        Dim tThread As Threading.Thread = New Threading.Thread(AddressOf CreateRandom)

        Try
            tThread.Start()

            If Not tThread.Join(Math.Max(120000, 30000 * Master.eSettings.AutoThumbs)) Then 'give it 30 seconds per image with a minimum of two minutes
                'something went wrong and the thread is hung (movie is corrupt?)... kill it forcibly
                isAborting = True
                If Not ffmpeg.HasExited Then
                    ffmpeg.Kill()
                End If
            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    ''' <summary>
    ''' Extract thumbs from a movie file.
    ''' </summary>
    Private Sub CreateRandom()

        If Master.eSettings.AutoDetectBDMV AndAlso Directory.GetParent(_movie.Filename).Name.ToLower = "bdmv" Then Exit Sub

        Try
            Dim pExt As String = Path.GetExtension(_movie.Filename).ToLower
            If Not pExt = ".rar" AndAlso Not pExt = ".iso" AndAlso Not pExt = ".img" AndAlso _
            Not pExt = ".bin" AndAlso Not pExt = ".cue" Then

                Dim intSeconds As Integer = 0
                Dim intAdd As Integer = 0
                Dim tPath As String = String.Empty
                Dim exImage As New Images

                If _isedit Then
                    tPath = Path.Combine(Master.TempPath, "extrathumbs")
                Else
                    If Master.eSettings.VideoTSParent AndAlso Directory.GetParent(_movie.Filename).Name.ToLower = "video_ts" Then
                        tPath = Path.Combine(Directory.GetParent(Directory.GetParent(_movie.Filename).FullName).FullName, "extrathumbs")
                    ElseIf Master.eSettings.VideoTSParent AndAlso Master.eSettings.AutoDetectBDMV AndAlso Directory.GetParent(_movie.Filename).Name.ToLower = "bdmv" Then
                        tPath = Path.Combine(Directory.GetParent(Directory.GetParent(_movie.Filename).FullName).FullName, "extrathumbs")
                    Else
                        tPath = Path.Combine(Directory.GetParent(_movie.Filename).FullName, "extrathumbs")
                    End If
                End If

                If Not Directory.Exists(tPath) Then
                    Directory.CreateDirectory(tPath)
                End If

                ffmpeg.StartInfo.FileName = String.Concat(Master.AppPath, "Bin", Path.DirectorySeparatorChar, "ffmpeg.exe")
                ffmpeg.EnableRaisingEvents = False
                ffmpeg.StartInfo.UseShellExecute = False
                ffmpeg.StartInfo.CreateNoWindow = True
                ffmpeg.StartInfo.RedirectStandardOutput = True
                ffmpeg.StartInfo.RedirectStandardError = True

                'first get the duration

                If Master.eSettings.AutoDetectBDMV AndAlso Directory.GetParent(_movie.Filename).Name.ToLower = "bdmv" Then
                    ffmpeg.StartInfo.Arguments = String.Format("-i ""{0}"" -an", MediaInfo.GetBDMVMovieName(_movie.Filename))
                Else
                    ffmpeg.StartInfo.Arguments = String.Format("-i ""{0}"" -an", _movie.Filename)
                End If

                ffmpeg.Start()
                Dim d As StreamReader = ffmpeg.StandardError
                Do
                    Dim s As String = d.ReadLine()
                    If s.Contains("Duration: ") Then
                        Dim sTime As String = Regex.Match(s, "Duration: (?<dur>.*?),").Groups("dur").ToString
                        If Not sTime = "N/A" Then
                            Dim ts As TimeSpan = CDate(CDate(String.Format("{0} {1}", DateTime.Today.ToString("d"), sTime))).Subtract(CDate(DateTime.Today))
                            intSeconds = ((ts.Hours * 60) + ts.Minutes) * 60 + ts.Seconds
                        End If
                    End If
                Loop While Not d.EndOfStream AndAlso Not isAborting

                If isAborting Then Exit Sub

                ffmpeg.WaitForExit()
                ffmpeg.Close()

                If isAborting Then Exit Sub

                If intSeconds > 0 AndAlso ((Master.eSettings.AutoThumbsNoSpoilers AndAlso intSeconds / 2 > _thumbcount + 300) OrElse (Not Master.eSettings.AutoThumbsNoSpoilers AndAlso intSeconds > _thumbcount + 2)) Then
                    If Master.eSettings.AutoThumbsNoSpoilers Then
                        intSeconds = Convert.ToInt32(((intSeconds / 2) - 300) / _thumbcount)
                        intAdd = intSeconds
                        intSeconds += intAdd + 300
                    Else
                        intSeconds = Convert.ToInt32(intSeconds / (_thumbcount + 2))
                        intAdd = intSeconds
                        intSeconds += intAdd
                    End If

                    For i = 0 To (_thumbcount - 1)
                        'check to see if file already exists... if so, don't bother running ffmpeg since we're not
                        'overwriting current thumbs anyway
                        If Not File.Exists(Path.Combine(tPath, String.Concat("thumb", (i + 1), ".jpg"))) Then

                            If Master.eSettings.AutoDetectBDMV AndAlso Directory.GetParent(_movie.Filename).Name.ToLower = "bdmv" Then
                                ffmpeg.StartInfo.Arguments = String.Format("-ss {0} -i ""{1}"" -an -f rawvideo -vframes 1 -vcodec mjpeg ""{2}""", intSeconds, MediaInfo.GetBDMVMovieName(_movie.Filename), Path.Combine(tPath, String.Concat("thumb", (i + 1), ".jpg")))
                            Else
                                ffmpeg.StartInfo.Arguments = String.Format("-ss {0} -i ""{1}"" -an -f rawvideo -vframes 1 -vcodec mjpeg ""{2}""", intSeconds, _movie.Filename, Path.Combine(tPath, String.Concat("thumb", (i + 1), ".jpg")))
                            End If
                            
                            ffmpeg.Start()
                            ffmpeg.WaitForExit()
                            If isAborting Then Exit Sub
                            ffmpeg.Close()
                            exImage = New Images
                            exImage.ResizeExtraThumb(Path.Combine(tPath, String.Concat("thumb", (i + 1), ".jpg")), Path.Combine(tPath, String.Concat("thumb", (i + 1), ".jpg")))
                            exImage.Dispose()
                            exImage = Nothing
                        End If
                        intSeconds += intAdd
                    Next
                End If

                If isAborting Then Exit Sub

                Dim fThumbs As New List(Of String)
                Try
                    fThumbs.AddRange(Directory.GetFiles(tPath, "thumb*.jpg"))
                Catch
                End Try

                If fThumbs.Count <= 0 Then
                    FileManip.Delete.DeleteDirectory(tPath)
                Else
                    Dim exFanart As New Images
                    'always set to something if extrathumbs are created so we know during scrapers
                    _setfa = "TRUE"
                    If Master.eSettings.UseETasFA AndAlso String.IsNullOrEmpty(_movie.FanartPath) Then
                        exFanart.FromFile(Path.Combine(tPath, "thumb1.jpg"))
                        _setfa = exFanart.SaveAsFanart(_movie)
                    End If
                    exFanart.Dispose()
                    exFanart = Nothing
                End If

            End If

        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub
End Class