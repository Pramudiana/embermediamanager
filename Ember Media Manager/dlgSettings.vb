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

Public Class dlgSettings

#Region "Form/Controls"

    ' ########################################
    ' ############ FORMS/CONTROLS ############
    ' ########################################

    Private Sub btnInfoPanelText_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnInfoPanelText.Click

        Try
            With Me.cdColor
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    If Not .Color = Nothing Then
                        Me.btnInfoPanelText.BackColor = .Color
                        Me.btnApply.Enabled = True
                    End If
                End If
            End With
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try

    End Sub

    Private Sub btnHeaderText_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnHeaderText.Click

        Try
            With Me.cdColor
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    If Not .Color = Nothing Then
                        Me.btnHeaderText.BackColor = .Color
                        Me.btnApply.Enabled = True
                    End If
                End If
            End With
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnTopPanelText_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTopPanelText.Click
        Try
            With Me.cdColor
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    If Not .Color = Nothing Then
                        Me.btnTopPanelText.BackColor = .Color
                        Me.btnApply.Enabled = True
                    End If
                End If
            End With
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnHeaders_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Try
            With Me.cdColor
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    If Not .Color = Nothing Then
                        Me.btnHeaders.BackColor = .Color
                        Me.btnApply.Enabled = True
                    End If
                End If
            End With
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnBackground_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Try
            With Me.cdColor
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    If Not .Color = Nothing Then
                        Me.btnBackground.BackColor = .Color
                        Me.btnApply.Enabled = True
                    End If
                End If
            End With
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnInfoPanel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Try
            With Me.cdColor
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    If Not .Color = Nothing Then
                        Me.btnInfoPanel.BackColor = .Color
                        Me.btnApply.Enabled = True
                    End If
                End If
            End With
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnTopPanel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Try
            With Me.cdColor
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    If Not .Color = Nothing Then
                        Me.btnTopPanel.BackColor = .Color
                        Me.btnApply.Enabled = True
                    End If
                End If
            End With
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnApply_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnApply.Click
        Try
            Me.SaveSettings()
            Me.btnApply.Enabled = False
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnMovieAddFolders_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMovieAddFolder.Click
        Me.AddItem(True)
    End Sub

    Private Sub btnMovieAddFiles_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMovieAddFiles.Click
        Me.AddItem(False)
    End Sub

    Private Sub btnMovieRem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMovieRem.Click
        Try
            If Me.lvMovies.SelectedItems.Count > 0 Then
                Me.lvMovies.BeginUpdate()
                For Each lvItem As ListViewItem In Me.lvMovies.SelectedItems
                    lvItem.Remove()
                Next
                Me.lvMovies.Sort()
                Me.lvMovies.EndUpdate()
                Me.lvMovies.Refresh()
                Me.btnApply.Enabled = True
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        Me.SaveSettings()
        Me.Close()
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub dlgSettings_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Me.Dispose()
    End Sub

    Private Sub frmSettings_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            Dim iBackground As New Bitmap(Me.pnlTop.Width, Me.pnlTop.Height)
            Dim g As Graphics = Graphics.FromImage(iBackground)
            g.FillRectangle(New Drawing2D.LinearGradientBrush(Me.pnlTop.ClientRectangle, Color.SteelBlue, Color.LightSteelBlue, 20), pnlTop.ClientRectangle)
            Me.pnlTop.BackgroundImage = iBackground
            g.Dispose()

            Me.FillSettings()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnAddFilter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddFilter.Click
        If Not String.IsNullOrEmpty(Me.txtFilter.Text) Then
            Me.lstFilters.Items.Add(Me.txtFilter.Text)
            Me.txtFilter.Text = String.Empty
            Me.btnApply.Enabled = True
        End If

        Me.txtFilter.Focus()
    End Sub

    Private Sub btnRemoveFilter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveFilter.Click
        If Me.lstFilters.Items.Count > 0 Then
            For i As Integer = Me.lstFilters.SelectedItems.Count - 1 To 0 Step -1
                Me.lstFilters.Items.Remove(Me.lstFilters.SelectedItems(i))
                Me.btnApply.Enabled = True
            Next
        End If
    End Sub

    Private Sub chkStudio_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkStudio.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub cbCert_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbCert.SelectedIndexChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkCert_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCert.CheckedChanged
        Me.cbCert.SelectedIndex = -1
        Me.cbCert.Enabled = Me.chkCert.Checked
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkFullCast_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFullCast.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkFullCrew_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFullCrew.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkMovieMediaCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieMediaCol.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkMoviePosterCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMoviePosterCol.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkMovieFanartCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieFanartCol.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkMovieInfoCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieInfoCol.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkMovieTrailerCol_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMovieTrailerCol.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkCleanFolderJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanFolderJPG.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkCleanMovieTBN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanMovieTBN.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkCleanMovieTBNb_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanMovieTBNb.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkCleanFanartJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanFanartJPG.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkCleanMovieFanartJPG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanMovieFanartJPG.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkCleanMovieNFO_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanMovieNFO.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkCleanMovieNFOb_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCleanMovieNFOb.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkUseTMDB_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUseTMDB.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkUseIMPA_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUseIMPA.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub cbPosterSize_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbPosterSize.SelectedIndexChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub cbFanartSize_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFanartSize.SelectedIndexChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkOverwritePoster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOverwritePoster.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkOverwriteFanart_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOverwriteFanart.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkLogErrors_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkLogErrors.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkUseFolderNames_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkProperCase_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkProperCase.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub btnUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUp.Click
        Try
            If lstFilters.Items.Count > 0 AndAlso Not IsNothing(lstFilters.SelectedItem) AndAlso lstFilters.SelectedIndex > 0 Then
                Dim iIndex As Integer = lstFilters.SelectedIndices(0)
                lstFilters.Items.Insert(iIndex - 1, lstFilters.SelectedItems(0))
                lstFilters.Items.RemoveAt(iIndex + 1)
                btnApply.Enabled = True
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub btnDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDown.Click
        Try
            If lstFilters.Items.Count > 0 AndAlso Not IsNothing(lstFilters.SelectedItem) AndAlso lstFilters.SelectedIndex < (lstFilters.Items.Count - 1) Then
                Dim iIndex As Integer = lstFilters.SelectedIndices(0)
                lstFilters.Items.Insert(iIndex + 2, lstFilters.SelectedItems(0))
                lstFilters.Items.RemoveAt(iIndex)
                btnApply.Enabled = True
            End If
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub rbMovieName_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbMovieName.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub rbMovie_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbMovie.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkTitleFromNfo_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTitleFromNfo.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub

    Private Sub chkUseMPDB_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUseMPDB.CheckedChanged
        Me.btnApply.Enabled = True
    End Sub
#End Region '*** Form/Controls



#Region "Routines/Functions"

    ' ########################################
    ' ########## ROUTINES/FUNCTIONS ##########
    ' ########################################

    Private Sub AddItem(ByVal blnIsFolder As Boolean)

        '//
        ' Add folder to folder list. Check to make sure it exists before adding it
        '\\


        Try
            Dim iFound As Integer = -1
            Dim lvItem As ListViewItem

            With Me.fbdBrowse
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    If Not String.IsNullOrEmpty(.SelectedPath.ToString) AndAlso Directory.Exists(.SelectedPath) Then

                        If Me.lvMovies.Items.Count > 0 Then
                            For i As Integer = 0 To Me.lvMovies.Items.Count - 1
                                If Me.lvMovies.Items(i).ToString.ToLower = .SelectedPath.ToString.ToLower Then
                                    iFound = i
                                    Exit For
                                End If
                            Next
                        Else
                            iFound = -1
                        End If


                        If iFound >= 0 Then
                            Me.lvMovies.Items(iFound).Selected = True
                            Me.lvMovies.Items(iFound).Focused = True
                            Me.lvMovies.Focus()
                        Else
                            Me.lvMovies.BeginUpdate()
                            lvItem = Me.lvMovies.Items.Add(.SelectedPath)
                            If blnIsFolder = True Then
                                lvItem.SubItems.Add("Folders")
                            Else
                                lvItem.SubItems.Add("Files")
                            End If
                            lvItem = Nothing
                            Me.lvMovies.Sort()
                            Me.lvMovies.EndUpdate()
                            Me.lvMovies.Columns(0).Width = 388
                            Me.lvMovies.Columns(1).Width = 74
                            Me.lvMovies.Refresh()
                            Me.btnApply.Enabled = True
                        End If

                    End If
                End If
            End With
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub SaveSettings()

        Try
            '######## GENERAL TAB ########
            Master.uSettings.FilterCustom.Clear()
            For Each str As String In Me.lstFilters.Items
                Master.uSettings.FilterCustom.Add(str.ToString)
            Next

            Master.uSettings.HeaderColor = Me.btnHeaders.BackColor.ToArgb
            Master.uSettings.BackgroundColor = Me.btnBackground.BackColor.ToArgb
            Master.uSettings.InfoPanelColor = Me.btnInfoPanel.BackColor.ToArgb
            Master.uSettings.TopPanelColor = Me.btnTopPanel.BackColor.ToArgb
            Master.uSettings.PanelTextColor = Me.btnInfoPanelText.BackColor.ToArgb()
            Master.uSettings.TopPanelTextColor = Me.btnTopPanelText.BackColor.ToArgb
            Master.uSettings.HeaderTextColor = Me.btnHeaderText.BackColor.ToArgb
            Master.uSettings.CleanFolderJPG = Me.chkCleanFolderJPG.Checked
            Master.uSettings.CleanMovieTBN = Me.chkCleanMovieTBN.Checked
            Master.uSettings.CleanMovieTBNB = Me.chkCleanMovieTBNb.Checked
            Master.uSettings.CleanFanartJPG = Me.chkCleanFanartJPG.Checked
            Master.uSettings.CleanMovieFanartJPG = Me.chkCleanMovieFanartJPG.Checked
            Master.uSettings.CleanMovieNFO = Me.chkCleanMovieNFO.Checked
            Master.uSettings.CleanMovieNFOB = Me.chkCleanMovieNFOb.Checked
            Master.uSettings.LogErrors = Me.chkLogErrors.Checked
            Master.uSettings.ProperCase = Me.chkProperCase.Checked

            '######## MOVIES TAB ########
            Master.uSettings.MovieFolders.Clear()
            For Each lvItem As ListViewItem In Me.lvMovies.Items
                Master.uSettings.MovieFolders.Add(lvItem.Text.ToString & "|" & lvItem.SubItems(1).Text.ToString)
            Next

            Master.uSettings.CertificationLang = Me.cbCert.Text
            Master.uSettings.UseStudioTags = Me.chkStudio.Checked
            Master.uSettings.FullCast = Me.chkFullCast.Checked
            Master.uSettings.FullCrew = Me.chkFullCrew.Checked
            Master.uSettings.MovieMediaCol = Me.chkMovieMediaCol.Checked
            Master.uSettings.MoviePosterCol = Me.chkMoviePosterCol.Checked
            Master.uSettings.MovieFanartCol = Me.chkMovieFanartCol.Checked
            Master.uSettings.MovieInfoCol = Me.chkMovieInfoCol.Checked
            Master.uSettings.MovieTrailerCol = Me.chkMovieTrailerCol.Checked
            Master.uSettings.UseTMDB = Me.chkUseTMDB.Checked
            Master.uSettings.UseIMPA = Me.chkUseIMPA.Checked
            Master.uSettings.UseMPDB = Me.chkUseMPDB.Checked
            Master.uSettings.PreferredPosterSize = Me.cbPosterSize.SelectedIndex
            Master.uSettings.PreferredFanartSize = Me.cbFanartSize.SelectedIndex
            Master.uSettings.OverwritePoster = Me.chkOverwritePoster.Checked
            Master.uSettings.OverwriteFanart = Me.chkOverwriteFanart.Checked
            Master.uSettings.UseFolderName = Me.chkUseFolderNames.Checked
            Master.uSettings.MovieExt = Me.rbMovieName.Checked
            Master.uSettings.UseNameFromNfo = Me.chkTitleFromNfo.Checked

            Master.uSettings.Save()
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

    Private Sub FillSettings()

        Dim dirArray() As String
        Dim lvItem As ListViewItem

        Try
            '######## GENERAL TAB ########
            For Each strFilter As String In Master.uSettings.FilterCustom
                Me.lstFilters.Items.Add(strFilter)
            Next

            Me.btnHeaders.BackColor = Color.FromArgb(Master.uSettings.HeaderColor)
            Me.btnBackground.BackColor = Color.FromArgb(Master.uSettings.BackgroundColor)
            Me.btnInfoPanel.BackColor = Color.FromArgb(Master.uSettings.InfoPanelColor)
            Me.btnTopPanel.BackColor = Color.FromArgb(Master.uSettings.TopPanelColor)
            Me.btnInfoPanelText.BackColor = Color.FromArgb(Master.uSettings.PanelTextColor)
            Me.btnTopPanelText.BackColor = Color.FromArgb(Master.uSettings.TopPanelTextColor)
            Me.btnHeaderText.BackColor = Color.FromArgb(Master.uSettings.HeaderTextColor)
            Me.chkCleanFolderJPG.Checked = Master.uSettings.CleanFolderJPG
            Me.chkCleanMovieTBN.Checked = Master.uSettings.CleanMovieTBN
            Me.chkCleanMovieTBNb.Checked = Master.uSettings.CleanMovieTBNB
            Me.chkCleanFanartJPG.Checked = Master.uSettings.CleanFanartJPG
            Me.chkCleanMovieFanartJPG.Checked = Master.uSettings.CleanMovieFanartJPG
            Me.chkCleanMovieNFO.Checked = Master.uSettings.CleanMovieNFO
            Me.chkCleanMovieNFOb.Checked = Master.uSettings.CleanMovieNFOB
            Me.chkLogErrors.Checked = Master.uSettings.LogErrors
            Me.chkProperCase.Checked = Master.uSettings.ProperCase

            '######## MOVIES TAB ########
            For Each strFolders As String In Master.uSettings.MovieFolders
                dirArray = Split(strFolders, "|")
                lvItem = Me.lvMovies.Items.Add(dirArray(0).ToString)
                lvItem.SubItems.Add(dirArray(1).ToString)
            Next

            If Not String.IsNullOrEmpty(Master.uSettings.CertificationLang) Then
                Me.chkCert.Checked = True
                Me.cbCert.Enabled = True
                Me.cbCert.Text = Master.uSettings.CertificationLang
            End If
            Me.chkStudio.Checked = Master.uSettings.UseStudioTags
            Me.chkFullCast.Checked = Master.uSettings.FullCast
            Me.chkFullCrew.Checked = Master.uSettings.FullCrew
            Me.chkMovieMediaCol.Checked = Master.uSettings.MovieMediaCol
            Me.chkMoviePosterCol.Checked = Master.uSettings.MoviePosterCol
            Me.chkMovieFanartCol.Checked = Master.uSettings.MovieFanartCol
            Me.chkMovieInfoCol.Checked = Master.uSettings.MovieInfoCol
            Me.chkMovieTrailerCol.Checked = Master.uSettings.MovieTrailerCol
            Me.chkUseTMDB.Checked = Master.uSettings.UseTMDB
            Me.chkUseIMPA.Checked = Master.uSettings.UseIMPA
            Me.chkUseMPDB.Checked = Master.uSettings.UseMPDB
            Me.cbPosterSize.SelectedIndex = Master.uSettings.PreferredPosterSize
            Me.cbFanartSize.SelectedIndex = Master.uSettings.PreferredFanartSize
            Me.chkOverwritePoster.Checked = Master.uSettings.OverwritePoster
            Me.chkOverwriteFanart.Checked = Master.uSettings.OverwriteFanart
            Me.chkUseFolderNames.Checked = Master.uSettings.UseFolderName
            Me.rbMovieName.Checked = Master.uSettings.MovieExt
            Me.rbMovie.Checked = Not Master.uSettings.MovieExt
            Me.chkTitleFromNfo.Checked = Master.uSettings.UseNameFromNfo

            Me.lvMovies.Columns(0).Width = 388
            Me.lvMovies.Columns(1).Width = 74
        Catch ex As Exception
            Master.eLog.WriteToErrorLog(ex.Message, ex.StackTrace, "Error")
        End Try
    End Sub

#End Region '*** Routines/Functions


End Class