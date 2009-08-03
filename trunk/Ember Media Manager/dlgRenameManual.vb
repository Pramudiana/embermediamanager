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

Public Class dlgRenameManual

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Cursor.Current = Cursors.WaitCursor
        OK_Button.Enabled = False
        Cancel_Button.Enabled = False
        Application.DoEvents()
        FileFolderRenamer.RenameSingle(Master.currMovie, txtFolder.Text, txtFile.Text, True, True)
        Cursor.Current = Cursors.Default
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub dlgRenameManual_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.SetUp()
        Dim FileName = Path.GetFileNameWithoutExtension(StringManip.CleanStackingMarkers(Master.currMovie.Filename))
        Dim stackMark As String = Path.GetFileNameWithoutExtension(Master.currMovie.Filename).Replace(FileName, String.Empty).ToLower
        If Master.currMovie.Movie.Title.ToLower.EndsWith(stackMark) Then
            FileName = Path.GetFileNameWithoutExtension(Master.currMovie.Filename)
        End If
        If Master.currMovie.isSingle Then
            txtFolder.Text = Path.GetFileName(Path.GetDirectoryName(Master.currMovie.Filename))
        Else
            txtFolder.Text = "$D"
            txtFolder.Visible = False
        End If
        txtFile.Text = FileName
    End Sub

    Private Sub txtFolder_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFolder.TextChanged
        If Not String.IsNullOrEmpty(txtFolder.Text) AndAlso Not String.IsNullOrEmpty(txtFile.Text) Then
            OK_Button.Enabled = True
        Else
            OK_Button.Enabled = False
        End If
    End Sub

    Private Sub txtFile_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFile.TextChanged
        If Not String.IsNullOrEmpty(txtFolder.Text) AndAlso Not String.IsNullOrEmpty(txtFile.Text) Then
            OK_Button.Enabled = True
        Else
            OK_Button.Enabled = False
        End If
    End Sub

    Sub SetUp()
        Me.Text = String.Concat(Master.eLang.GetString(632, "Manual Rename"), " | ", Master.currMovie.Movie.Title)
        Me.Label1.Text = Master.eLang.GetString(633, "Folder Name")
        Me.Label2.Text = Master.eLang.GetString(634, "File Name")
        Me.OK_Button.Text = Master.eLang.GetString(179, "OK")
        Me.Cancel_Button.Text = Master.eLang.GetString(19, "Close")
        Me.lblTitle.Text = Master.eLang.GetString(246, "Title:")
        Me.txtTitle.Text = Master.currMovie.Movie.Title
    End Sub
End Class
