﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgBulkOfflineHolder
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgBulkOfflineHolder))
        Me.CLOSE_Button = New System.Windows.Forms.Button
        Me.pnlTop = New System.Windows.Forms.Panel
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label4 = New System.Windows.Forms.Label
        Me.PictureBox1 = New System.Windows.Forms.PictureBox
        Me.Create_Button = New System.Windows.Forms.Button
        Me.cdColor = New System.Windows.Forms.ColorDialog
        Me.tmrName = New System.Windows.Forms.Timer(Me.components)
        Me.cdFont = New System.Windows.Forms.FontDialog
        Me.tmrNameWait = New System.Windows.Forms.Timer(Me.components)
        Me.dgvCSV = New System.Windows.Forms.DataGridView
        Me.pnlTop.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgvCSV, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'CLOSE_Button
        '
        Me.CLOSE_Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.CLOSE_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.CLOSE_Button.Location = New System.Drawing.Point(553, 385)
        Me.CLOSE_Button.Name = "CLOSE_Button"
        Me.CLOSE_Button.Size = New System.Drawing.Size(80, 23)
        Me.CLOSE_Button.TabIndex = 0
        Me.CLOSE_Button.Text = "Close"
        '
        'pnlTop
        '
        Me.pnlTop.BackColor = System.Drawing.Color.SteelBlue
        Me.pnlTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlTop.Controls.Add(Me.Label2)
        Me.pnlTop.Controls.Add(Me.Label4)
        Me.pnlTop.Controls.Add(Me.PictureBox1)
        Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTop.Location = New System.Drawing.Point(0, 0)
        Me.pnlTop.Name = "pnlTop"
        Me.pnlTop.Size = New System.Drawing.Size(643, 64)
        Me.pnlTop.TabIndex = 58
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.ForeColor = System.Drawing.Color.White
        Me.Label2.Location = New System.Drawing.Point(64, 38)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(90, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Add Offline movie"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.BackColor = System.Drawing.Color.Transparent
        Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.ForeColor = System.Drawing.Color.White
        Me.Label4.Location = New System.Drawing.Point(61, 3)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(343, 29)
        Me.Label4.TabIndex = 1
        Me.Label4.Text = "Bulk Offline Media Manager "
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.Transparent
        Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
        Me.PictureBox1.Location = New System.Drawing.Point(12, 7)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(48, 48)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.PictureBox1.TabIndex = 0
        Me.PictureBox1.TabStop = False
        '
        'Create_Button
        '
        Me.Create_Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.Create_Button.Enabled = False
        Me.Create_Button.Location = New System.Drawing.Point(467, 385)
        Me.Create_Button.Name = "Create_Button"
        Me.Create_Button.Size = New System.Drawing.Size(80, 23)
        Me.Create_Button.TabIndex = 67
        Me.Create_Button.Text = "Create"
        '
        'tmrName
        '
        Me.tmrName.Interval = 250
        '
        'cdFont
        '
        Me.cdFont.Font = New System.Drawing.Font("Arial", 21.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'tmrNameWait
        '
        Me.tmrNameWait.Interval = 250
        '
        'dgvCSV
        '
        Me.dgvCSV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvCSV.Location = New System.Drawing.Point(12, 129)
        Me.dgvCSV.Name = "dgvCSV"
        Me.dgvCSV.Size = New System.Drawing.Size(620, 246)
        Me.dgvCSV.TabIndex = 68
        '
        'dlgBulkOfflineHolder
        '
        Me.AcceptButton = Me.CLOSE_Button
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(643, 420)
        Me.Controls.Add(Me.dgvCSV)
        Me.Controls.Add(Me.Create_Button)
        Me.Controls.Add(Me.pnlTop)
        Me.Controls.Add(Me.CLOSE_Button)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "dlgBulkOfflineHolder"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Bulk Offline Media Manager"
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgvCSV, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents CLOSE_Button As System.Windows.Forms.Button
    Friend WithEvents pnlTop As System.Windows.Forms.Panel
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents Create_Button As System.Windows.Forms.Button
    Friend WithEvents cdColor As System.Windows.Forms.ColorDialog
    Friend WithEvents tmrName As System.Windows.Forms.Timer
    Friend WithEvents cdFont As System.Windows.Forms.FontDialog
    Friend WithEvents tmrNameWait As System.Windows.Forms.Timer
    Friend WithEvents dgvCSV As System.Windows.Forms.DataGridView

End Class
