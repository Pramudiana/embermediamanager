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
Imports EmberAPI

Public Class OfflineHolderModule
    Implements Interfaces.EmberExternalModule

    Private _enabled As Boolean = False
    Private _Name As String = "Offline Media Manager"
    Public Event ModuleSettingsChanged() Implements Interfaces.EmberExternalModule.ModuleSettingsChanged
    Public Event ModuleEnabledChanged(ByVal Name As String, ByVal State As Boolean) Implements Interfaces.EmberExternalModule.ModuleEnabledChanged
    Public Event GenericEvent(ByVal _params As List(Of Object)) Implements Interfaces.EmberExternalModule.GenericEvent

    Public ReadOnly Property ModuleType() As List(Of Enums.ModuleType) Implements Interfaces.EmberExternalModule.ModuleType
        Get
            Return New List(Of Enums.ModuleType)(New Enums.ModuleType() {Enums.ModuleType.Generic})
        End Get
    End Property

    Property Enabled() As Boolean Implements Interfaces.EmberExternalModule.Enabled
        Get
            Return _enabled
        End Get
        Set(ByVal value As Boolean)
            _enabled = value
            If _enabled Then
                Enable()
            Else
                Disable()
            End If
        End Set
    End Property

    Function InjectSetup() As Containers.SettingsPanel Implements Interfaces.EmberExternalModule.InjectSetup
        Dim SPanel As New Containers.SettingsPanel
        SPanel.Name = Me._Name
        SPanel.Text = Me._Name
        SPanel.Type = Master.eLang.GetString(999, "Modules")
        SPanel.ImageIndex = If(Me._enabled, 9, 10)
        SPanel.Order = 100
        SPanel.Panel = New Panel
        Return SPanel
    End Function

    Sub SaveSetup(ByVal DoDispose As Boolean) Implements Interfaces.EmberExternalModule.SaveSetup

    End Sub

    Sub Enable()

        Dim tmpOfflineHolder As New dlgOfflineHolder
        MyMenu.Image = New Bitmap(tmpOfflineHolder.Icon.ToBitmap)
        MyMenu.Text = "Offline Media Manager"
        Dim tsi As ToolStripMenuItem = DirectCast(ModulesManager.Instance.RuntimeObjects.TopMenu.Items("ToolsToolStripMenuItem"), ToolStripMenuItem)
        tsi.DropDownItems.Add(MyMenu)
        tmpOfflineHolder.Dispose()

    End Sub
    Sub Disable()

        Dim tsi As ToolStripMenuItem = DirectCast(ModulesManager.Instance.RuntimeObjects.TopMenu.Items("ToolsToolStripMenuItem"), ToolStripMenuItem)
        tsi.DropDownItems.Remove(MyMenu)

    End Sub
    Sub Init() Implements Interfaces.EmberExternalModule.Init
        'Master.eLang.LoadLanguage(Master.eSettings.Language)
    End Sub

    ReadOnly Property ModuleName() As String Implements Interfaces.EmberExternalModule.ModuleName
        Get
            Return _Name
        End Get
    End Property
    ReadOnly Property ModuleVersion() As String Implements Interfaces.EmberExternalModule.ModuleVersion
        Get
            Return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FilePrivatePart.ToString
        End Get
    End Property

    'Dim MyMenu As New System.Windows.Forms.ToolStripMenuItem
    Dim WithEvents MyMenu As New System.Windows.Forms.ToolStripMenuItem
    Private Sub MyMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyMenu.Click
        Using dOfflineHolder As New dlgOfflineHolder
            If dOfflineHolder.ShowDialog() = Windows.Forms.DialogResult.OK Then
                ModulesManager.Instance.RuntimeObjects.InvokeLoadMedia(New Structures.Scans With {.Movies = True}, String.Empty)
            End If
        End Using
    End Sub

    Public Function RunGeneric(ByVal mType As Enums.ModuleType, ByVal _params As List(Of Object)) As Interfaces.ModuleResult Implements Interfaces.EmberExternalModule.RunGeneric
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function
End Class