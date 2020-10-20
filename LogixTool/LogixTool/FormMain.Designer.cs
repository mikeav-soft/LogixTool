namespace LogixTool
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.recorderStatusToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.recordCounterToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.fileSizeToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.fileNameToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_OpenTagList = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_SaveTagList = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_GoOnline = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_GoOffline = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_RunRecording = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_StopRecording = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_ShowDeviceBrowser = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_ShowEventLogs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_About = new System.Windows.Forms.ToolStripButton();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.deviceBrowserControl = new LogixTool.Controls.DeviceBrowserControl();
            this.tagBrowserControl = new LogixTool.Controls.TagBrowserControl();
            this.messageControl = new LogixTool.Common.MessageControl();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.recorderStatusToolStripStatusLabel,
            this.recordCounterToolStripStatusLabel,
            this.fileSizeToolStripStatusLabel,
            this.fileNameToolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 540);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1034, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip";
            // 
            // recorderStatusToolStripStatusLabel
            // 
            this.recorderStatusToolStripStatusLabel.Name = "recorderStatusToolStripStatusLabel";
            this.recorderStatusToolStripStatusLabel.Size = new System.Drawing.Size(77, 17);
            this.recorderStatusToolStripStatusLabel.Text = "Recorder: Off";
            // 
            // recordCounterToolStripStatusLabel
            // 
            this.recordCounterToolStripStatusLabel.Name = "recordCounterToolStripStatusLabel";
            this.recordCounterToolStripStatusLabel.Size = new System.Drawing.Size(49, 17);
            this.recordCounterToolStripStatusLabel.Text = "Records";
            // 
            // fileSizeToolStripStatusLabel
            // 
            this.fileSizeToolStripStatusLabel.Name = "fileSizeToolStripStatusLabel";
            this.fileSizeToolStripStatusLabel.Size = new System.Drawing.Size(27, 17);
            this.fileSizeToolStripStatusLabel.Text = "Size";
            // 
            // fileNameToolStripStatusLabel
            // 
            this.fileNameToolStripStatusLabel.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.fileNameToolStripStatusLabel.Name = "fileNameToolStripStatusLabel";
            this.fileNameToolStripStatusLabel.Size = new System.Drawing.Size(35, 17);
            this.fileNameToolStripStatusLabel.Text = "File";
            this.fileNameToolStripStatusLabel.Click += new System.EventHandler(this.fileNameToolStripStatusLabel_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Text files|*.txt|All files|*.*";
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "Text files|*.txt|All files|*.*";
            this.saveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog_FileOk);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.toolStrip);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(1034, 540);
            this.splitContainer1.SplitterDistance = 25;
            this.splitContainer1.TabIndex = 2;
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_OpenTagList,
            this.toolStripButton_SaveTagList,
            this.toolStripSeparator2,
            this.toolStripButton_GoOnline,
            this.toolStripButton_GoOffline,
            this.toolStripSeparator1,
            this.toolStripButton_RunRecording,
            this.toolStripButton_StopRecording,
            this.toolStripSeparator3,
            this.toolStripButton_ShowDeviceBrowser,
            this.toolStripButton_ShowEventLogs,
            this.toolStripSeparator4,
            this.toolStripButton_About});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(1034, 25);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip";
            // 
            // toolStripButton_OpenTagList
            // 
            this.toolStripButton_OpenTagList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_OpenTagList.Image = global::LogixTool.Properties.Resources.ico_folder;
            this.toolStripButton_OpenTagList.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_OpenTagList.Name = "toolStripButton_OpenTagList";
            this.toolStripButton_OpenTagList.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_OpenTagList.Text = "Open Tags";
            this.toolStripButton_OpenTagList.Click += new System.EventHandler(this.toolStripButton_OpenTagList_Click);
            // 
            // toolStripButton_SaveTagList
            // 
            this.toolStripButton_SaveTagList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_SaveTagList.Image = global::LogixTool.Properties.Resources.ico_disk;
            this.toolStripButton_SaveTagList.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_SaveTagList.Name = "toolStripButton_SaveTagList";
            this.toolStripButton_SaveTagList.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_SaveTagList.Text = "Save Tags";
            this.toolStripButton_SaveTagList.Click += new System.EventHandler(this.toolStripButton_SaveTagList_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_GoOnline
            // 
            this.toolStripButton_GoOnline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_GoOnline.Image = global::LogixTool.Properties.Resources.ico_logix_module_run;
            this.toolStripButton_GoOnline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_GoOnline.Name = "toolStripButton_GoOnline";
            this.toolStripButton_GoOnline.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_GoOnline.Text = "Go Online";
            this.toolStripButton_GoOnline.Click += new System.EventHandler(this.toolStripButton_GoOnline_Click);
            // 
            // toolStripButton_GoOffline
            // 
            this.toolStripButton_GoOffline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_GoOffline.Image = global::LogixTool.Properties.Resources.ico_logix_module;
            this.toolStripButton_GoOffline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_GoOffline.Name = "toolStripButton_GoOffline";
            this.toolStripButton_GoOffline.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_GoOffline.Text = "Go Offline";
            this.toolStripButton_GoOffline.Click += new System.EventHandler(this.toolStripButton_GoOffline_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_RunRecording
            // 
            this.toolStripButton_RunRecording.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_RunRecording.Image = global::LogixTool.Properties.Resources.ico_run;
            this.toolStripButton_RunRecording.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_RunRecording.Name = "toolStripButton_RunRecording";
            this.toolStripButton_RunRecording.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_RunRecording.Text = "Run Recording";
            this.toolStripButton_RunRecording.Click += new System.EventHandler(this.toolStripButton_RunRecording_Click);
            // 
            // toolStripButton_StopRecording
            // 
            this.toolStripButton_StopRecording.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_StopRecording.Image = global::LogixTool.Properties.Resources.ico_stop;
            this.toolStripButton_StopRecording.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_StopRecording.Name = "toolStripButton_StopRecording";
            this.toolStripButton_StopRecording.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_StopRecording.Text = "Stop Recording";
            this.toolStripButton_StopRecording.Click += new System.EventHandler(this.toolStripButton_StopRecording_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_ShowDeviceBrowser
            // 
            this.toolStripButton_ShowDeviceBrowser.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_ShowDeviceBrowser.Image = global::LogixTool.Properties.Resources.ico_tool;
            this.toolStripButton_ShowDeviceBrowser.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_ShowDeviceBrowser.Name = "toolStripButton_ShowDeviceBrowser";
            this.toolStripButton_ShowDeviceBrowser.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_ShowDeviceBrowser.Text = "Device Browser";
            this.toolStripButton_ShowDeviceBrowser.Click += new System.EventHandler(this.toolStripButton_ShowDeviceBrowser_Click);
            // 
            // toolStripButton_ShowEventLogs
            // 
            this.toolStripButton_ShowEventLogs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_ShowEventLogs.Image = global::LogixTool.Properties.Resources.ico_error;
            this.toolStripButton_ShowEventLogs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_ShowEventLogs.Name = "toolStripButton_ShowEventLogs";
            this.toolStripButton_ShowEventLogs.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_ShowEventLogs.Text = "toolStripButton1";
            this.toolStripButton_ShowEventLogs.ToolTipText = "Event Logs";
            this.toolStripButton_ShowEventLogs.Click += new System.EventHandler(this.toolStripButton_ShowEventLogs_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_About
            // 
            this.toolStripButton_About.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_About.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_About.Image = global::LogixTool.Properties.Resources.ico_question;
            this.toolStripButton_About.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_About.Name = "toolStripButton_About";
            this.toolStripButton_About.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_About.Text = "toolStripButton1";
            this.toolStripButton_About.ToolTipText = "About";
            this.toolStripButton_About.Click += new System.EventHandler(this.toolStripButton_About_Click);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.messageControl);
            this.splitContainer3.Size = new System.Drawing.Size(1034, 511);
            this.splitContainer3.SplitterDistance = 384;
            this.splitContainer3.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.deviceBrowserControl);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tagBrowserControl);
            this.splitContainer2.Size = new System.Drawing.Size(1034, 384);
            this.splitContainer2.SplitterDistance = 225;
            this.splitContainer2.TabIndex = 0;
            // 
            // deviceBrowserControl
            // 
            this.deviceBrowserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deviceBrowserControl.Location = new System.Drawing.Point(0, 0);
            this.deviceBrowserControl.MinimumSize = new System.Drawing.Size(210, 300);
            this.deviceBrowserControl.Name = "deviceBrowserControl";
            this.deviceBrowserControl.Size = new System.Drawing.Size(225, 384);
            this.deviceBrowserControl.TabIndex = 0;
            this.deviceBrowserControl.SaveProjectClick += new System.EventHandler(this.deviceBrowserControl_SaveProjectClick);
            // 
            // tagBrowserControl
            // 
            this.tagBrowserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tagBrowserControl.Location = new System.Drawing.Point(0, 0);
            this.tagBrowserControl.MonitorMode = false;
            this.tagBrowserControl.Name = "tagBrowserControl";
            this.tagBrowserControl.Size = new System.Drawing.Size(805, 384);
            this.tagBrowserControl.TabIndex = 1;
            this.tagBrowserControl.WriteModeEnable = false;
            // 
            // messageControl
            // 
            this.messageControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messageControl.Location = new System.Drawing.Point(0, 0);
            this.messageControl.MessageCapacity = 10000;
            this.messageControl.Name = "messageControl";
            this.messageControl.NewMessagesOnTop = true;
            this.messageControl.Size = new System.Drawing.Size(1034, 123);
            this.messageControl.TabIndex = 0;
            this.messageControl.TimeStampFormat = "dd.MM.yyyy HH:mm:ss:fffffff";
            this.messageControl.UpdateInterval = 1000D;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1034, 562);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMain";
            this.Text = "Logix Tool v0.2";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormRegistrator_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormRegistrator_FormClosed);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ToolStripStatusLabel recorderStatusToolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel fileNameToolStripStatusLabel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButton_OpenTagList;
        private System.Windows.Forms.ToolStripButton toolStripButton_SaveTagList;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_RunRecording;
        private System.Windows.Forms.ToolStripButton toolStripButton_StopRecording;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton_GoOnline;
        private System.Windows.Forms.ToolStripButton toolStripButton_GoOffline;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private Controls.TagBrowserControl tagBrowserControl;
        private Controls.DeviceBrowserControl deviceBrowserControl;
        private System.Windows.Forms.ToolStripButton toolStripButton_ShowDeviceBrowser;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.ToolStripButton toolStripButton_ShowEventLogs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton toolStripButton_About;
        private System.Windows.Forms.ToolStripStatusLabel recordCounterToolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel fileSizeToolStripStatusLabel;
        private Common.MessageControl messageControl;
    }
}