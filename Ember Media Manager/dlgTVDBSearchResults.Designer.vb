﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgTVDBSearchResults
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(dlgTVDBSearchResults))
        Me.OK_Button = New System.Windows.Forms.Button
        Me.Cancel_Button = New System.Windows.Forms.Button
        Me.pbBanner = New System.Windows.Forms.PictureBox
        Me.lblTitle = New System.Windows.Forms.Label
        Me.lblAiredHeader = New System.Windows.Forms.Label
        Me.lblAired = New System.Windows.Forms.Label
        Me.lblPlotHeader = New System.Windows.Forms.Label
        Me.txtOutline = New System.Windows.Forms.TextBox
        Me.lvSearchResults = New System.Windows.Forms.ListView
        Me.colName = New System.Windows.Forms.ColumnHeader
        Me.colLang = New System.Windows.Forms.ColumnHeader
        Me.pnlLoading = New System.Windows.Forms.Panel
        Me.Label3 = New System.Windows.Forms.Label
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar
        Me.pnlTop = New System.Windows.Forms.Panel
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label1 = New System.Windows.Forms.Label
        Me.PictureBox1 = New System.Windows.Forms.PictureBox
        CType(Me.pbBanner, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlLoading.SuspendLayout()
        Me.pnlTop.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'OK_Button
        '
        Me.OK_Button.Enabled = False
        Me.OK_Button.Location = New System.Drawing.Point(486, 319)
        Me.OK_Button.Name = "OK_Button"
        Me.OK_Button.Size = New System.Drawing.Size(67, 23)
        Me.OK_Button.TabIndex = 0
        Me.OK_Button.Text = "OK"
        '
        'Cancel_Button
        '
        Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Cancel_Button.Location = New System.Drawing.Point(559, 319)
        Me.Cancel_Button.Name = "Cancel_Button"
        Me.Cancel_Button.Size = New System.Drawing.Size(67, 23)
        Me.Cancel_Button.TabIndex = 1
        Me.Cancel_Button.Text = "Cancel"
        '
        'pbBanner
        '
        Me.pbBanner.Location = New System.Drawing.Point(325, 69)
        Me.pbBanner.Name = "pbBanner"
        Me.pbBanner.Size = New System.Drawing.Size(300, 55)
        Me.pbBanner.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.pbBanner.TabIndex = 3
        Me.pbBanner.TabStop = False
        '
        'lblTitle
        '
        Me.lblTitle.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTitle.Location = New System.Drawing.Point(325, 132)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(300, 19)
        Me.lblTitle.TabIndex = 4
        Me.lblTitle.Text = "Title"
        Me.lblTitle.Visible = False
        '
        'lblAiredHeader
        '
        Me.lblAiredHeader.AutoSize = True
        Me.lblAiredHeader.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAiredHeader.Location = New System.Drawing.Point(325, 161)
        Me.lblAiredHeader.Name = "lblAiredHeader"
        Me.lblAiredHeader.Size = New System.Drawing.Size(40, 13)
        Me.lblAiredHeader.TabIndex = 63
        Me.lblAiredHeader.Text = "Aired:"
        Me.lblAiredHeader.Visible = False
        '
        'lblAired
        '
        Me.lblAired.AutoSize = True
        Me.lblAired.Location = New System.Drawing.Point(387, 161)
        Me.lblAired.Name = "lblAired"
        Me.lblAired.Size = New System.Drawing.Size(65, 13)
        Me.lblAired.TabIndex = 62
        Me.lblAired.Text = "00/00/0000"
        Me.lblAired.Visible = False
        '
        'lblPlotHeader
        '
        Me.lblPlotHeader.AutoSize = True
        Me.lblPlotHeader.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPlotHeader.Location = New System.Drawing.Point(325, 189)
        Me.lblPlotHeader.Name = "lblPlotHeader"
        Me.lblPlotHeader.Size = New System.Drawing.Size(87, 13)
        Me.lblPlotHeader.TabIndex = 67
        Me.lblPlotHeader.Text = "Plot Summary:"
        Me.lblPlotHeader.Visible = False
        '
        'txtOutline
        '
        Me.txtOutline.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtOutline.Location = New System.Drawing.Point(325, 205)
        Me.txtOutline.Multiline = True
        Me.txtOutline.Name = "txtOutline"
        Me.txtOutline.Size = New System.Drawing.Size(300, 100)
        Me.txtOutline.TabIndex = 66
        Me.txtOutline.TabStop = False
        Me.txtOutline.Visible = False
        '
        'lvSearchResults
        '
        Me.lvSearchResults.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colName, Me.colLang})
        Me.lvSearchResults.FullRowSelect = True
        Me.lvSearchResults.HideSelection = False
        Me.lvSearchResults.Location = New System.Drawing.Point(3, 69)
        Me.lvSearchResults.MultiSelect = False
        Me.lvSearchResults.Name = "lvSearchResults"
        Me.lvSearchResults.Size = New System.Drawing.Size(316, 236)
        Me.lvSearchResults.Sorting = System.Windows.Forms.SortOrder.Ascending
        Me.lvSearchResults.TabIndex = 68
        Me.lvSearchResults.UseCompatibleStateImageBehavior = False
        Me.lvSearchResults.View = System.Windows.Forms.View.Details
        '
        'colName
        '
        Me.colName.Text = "Name"
        Me.colName.Width = 250
        '
        'colLang
        '
        Me.colLang.Text = "Language"
        Me.colLang.Width = 63
        '
        'pnlLoading
        '
        Me.pnlLoading.BackColor = System.Drawing.Color.White
        Me.pnlLoading.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlLoading.Controls.Add(Me.Label3)
        Me.pnlLoading.Controls.Add(Me.ProgressBar1)
        Me.pnlLoading.Location = New System.Drawing.Point(380, 154)
        Me.pnlLoading.Name = "pnlLoading"
        Me.pnlLoading.Size = New System.Drawing.Size(200, 54)
        Me.pnlLoading.TabIndex = 69
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(3, 10)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(96, 13)
        Me.Label3.TabIndex = 1
        Me.Label3.Text = "Searching TVDB..."
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(3, 32)
        Me.ProgressBar1.MarqueeAnimationSpeed = 25
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(192, 17)
        Me.ProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee
        Me.ProgressBar1.TabIndex = 0
        '
        'pnlTop
        '
        Me.pnlTop.BackColor = System.Drawing.Color.LightSteelBlue
        Me.pnlTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlTop.Controls.Add(Me.Label2)
        Me.pnlTop.Controls.Add(Me.Label1)
        Me.pnlTop.Controls.Add(Me.PictureBox1)
        Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTop.Location = New System.Drawing.Point(0, 0)
        Me.pnlTop.Name = "pnlTop"
        Me.pnlTop.Size = New System.Drawing.Size(629, 64)
        Me.pnlTop.TabIndex = 70
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.ForeColor = System.Drawing.Color.White
        Me.Label2.Location = New System.Drawing.Point(61, 38)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(261, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "View details of each result to find the proper TV show."
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.White
        Me.Label1.Location = New System.Drawing.Point(58, 3)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(229, 29)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "TV Search Results"
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.Transparent
        Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
        Me.PictureBox1.Location = New System.Drawing.Point(7, 8)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(48, 48)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.PictureBox1.TabIndex = 0
        Me.PictureBox1.TabStop = False
        '
        'dlgTVDBSearchResults
        '
        Me.AcceptButton = Me.OK_Button
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.Cancel_Button
        Me.ClientSize = New System.Drawing.Size(629, 345)
        Me.Controls.Add(Me.pnlTop)
        Me.Controls.Add(Me.pnlLoading)
        Me.Controls.Add(Me.lvSearchResults)
        Me.Controls.Add(Me.lblPlotHeader)
        Me.Controls.Add(Me.txtOutline)
        Me.Controls.Add(Me.lblAiredHeader)
        Me.Controls.Add(Me.lblAired)
        Me.Controls.Add(Me.lblTitle)
        Me.Controls.Add(Me.pbBanner)
        Me.Controls.Add(Me.Cancel_Button)
        Me.Controls.Add(Me.OK_Button)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "dlgTVDBSearchResults"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Search Results"
        CType(Me.pbBanner, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlLoading.ResumeLayout(False)
        Me.pnlLoading.PerformLayout()
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents pbBanner As System.Windows.Forms.PictureBox
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents lblAiredHeader As System.Windows.Forms.Label
    Friend WithEvents lblAired As System.Windows.Forms.Label
    Friend WithEvents lblPlotHeader As System.Windows.Forms.Label
    Friend WithEvents txtOutline As System.Windows.Forms.TextBox
    Friend WithEvents lvSearchResults As System.Windows.Forms.ListView
    Friend WithEvents colName As System.Windows.Forms.ColumnHeader
    Friend WithEvents colLang As System.Windows.Forms.ColumnHeader
    Friend WithEvents pnlLoading As System.Windows.Forms.Panel
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents pnlTop As System.Windows.Forms.Panel
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox

End Class
