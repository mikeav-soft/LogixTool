namespace LogixTool
{
    partial class FormRecordSettings
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
            this.button_Apply = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_SeparationFilesByPeriod = new System.Windows.Forms.ComboBox();
            this.numericUpDown_SeparationFilesBySize = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboBox_PeriodRecordingUnit = new System.Windows.Forms.ComboBox();
            this.radioButton_SelectionRecording = new System.Windows.Forms.RadioButton();
            this.numericUpDown_PeriodRecordingValue = new System.Windows.Forms.NumericUpDown();
            this.radioButton_PeriodRecording = new System.Windows.Forms.RadioButton();
            this.radioButton_NormalRecording = new System.Windows.Forms.RadioButton();
            this.checkedListBox_SelectionRecording = new System.Windows.Forms.CheckedListBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioButton_TimeStampAsDateTime = new System.Windows.Forms.RadioButton();
            this.radioButton_TimeStampAsTicks = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_FileNamePrefix = new System.Windows.Forms.TextBox();
            this.button_ShowDirectoryDialog = new System.Windows.Forms.Button();
            this.textBox_FileDirectory = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_SeparationFilesBySize)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_PeriodRecordingValue)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Apply
            // 
            this.button_Apply.Location = new System.Drawing.Point(420, 156);
            this.button_Apply.Name = "button_Apply";
            this.button_Apply.Size = new System.Drawing.Size(81, 21);
            this.button_Apply.TabIndex = 0;
            this.button_Apply.Text = "OK";
            this.button_Apply.UseVisualStyleBackColor = true;
            this.button_Apply.Click += new System.EventHandler(this.button_Apply_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "By Size:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "By Period:";
            // 
            // comboBox_SeparationFilesByPeriod
            // 
            this.comboBox_SeparationFilesByPeriod.DisplayMember = "None";
            this.comboBox_SeparationFilesByPeriod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_SeparationFilesByPeriod.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBox_SeparationFilesByPeriod.FormattingEnabled = true;
            this.comboBox_SeparationFilesByPeriod.Location = new System.Drawing.Point(70, 43);
            this.comboBox_SeparationFilesByPeriod.Name = "comboBox_SeparationFilesByPeriod";
            this.comboBox_SeparationFilesByPeriod.Size = new System.Drawing.Size(106, 21);
            this.comboBox_SeparationFilesByPeriod.TabIndex = 4;
            this.comboBox_SeparationFilesByPeriod.ValueMember = "None";
            // 
            // numericUpDown_SeparationFilesBySize
            // 
            this.numericUpDown_SeparationFilesBySize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericUpDown_SeparationFilesBySize.Location = new System.Drawing.Point(70, 19);
            this.numericUpDown_SeparationFilesBySize.Maximum = new decimal(new int[] {
            4086,
            0,
            0,
            0});
            this.numericUpDown_SeparationFilesBySize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_SeparationFilesBySize.Name = "numericUpDown_SeparationFilesBySize";
            this.numericUpDown_SeparationFilesBySize.Size = new System.Drawing.Size(60, 20);
            this.numericUpDown_SeparationFilesBySize.TabIndex = 5;
            this.numericUpDown_SeparationFilesBySize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_SeparationFilesBySize.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.numericUpDown_SeparationFilesBySize);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.comboBox_SeparationFilesByPeriod);
            this.groupBox1.Location = new System.Drawing.Point(267, 64);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(183, 71);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "File Separation";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(136, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(22, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Mb";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.comboBox_PeriodRecordingUnit);
            this.groupBox2.Controls.Add(this.radioButton_SelectionRecording);
            this.groupBox2.Controls.Add(this.numericUpDown_PeriodRecordingValue);
            this.groupBox2.Controls.Add(this.radioButton_PeriodRecording);
            this.groupBox2.Controls.Add(this.radioButton_NormalRecording);
            this.groupBox2.Controls.Add(this.checkedListBox_SelectionRecording);
            this.groupBox2.Location = new System.Drawing.Point(9, 64);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(251, 140);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Recording Event";
            // 
            // comboBox_PeriodRecordingUnit
            // 
            this.comboBox_PeriodRecordingUnit.DisplayMember = "msec";
            this.comboBox_PeriodRecordingUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_PeriodRecordingUnit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBox_PeriodRecordingUnit.FormattingEnabled = true;
            this.comboBox_PeriodRecordingUnit.Location = new System.Drawing.Point(180, 42);
            this.comboBox_PeriodRecordingUnit.Name = "comboBox_PeriodRecordingUnit";
            this.comboBox_PeriodRecordingUnit.Size = new System.Drawing.Size(65, 21);
            this.comboBox_PeriodRecordingUnit.TabIndex = 8;
            this.comboBox_PeriodRecordingUnit.ValueMember = "msec";
            // 
            // radioButton_SelectionRecording
            // 
            this.radioButton_SelectionRecording.AutoSize = true;
            this.radioButton_SelectionRecording.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButton_SelectionRecording.Location = new System.Drawing.Point(13, 65);
            this.radioButton_SelectionRecording.Name = "radioButton_SelectionRecording";
            this.radioButton_SelectionRecording.Size = new System.Drawing.Size(181, 17);
            this.radioButton_SelectionRecording.TabIndex = 10;
            this.radioButton_SelectionRecording.Text = "By Selected Tag Value Changing";
            this.radioButton_SelectionRecording.UseVisualStyleBackColor = true;
            this.radioButton_SelectionRecording.CheckedChanged += new System.EventHandler(this.radioButton_SelectionRecording_CheckedChanged);
            // 
            // numericUpDown_PeriodRecordingValue
            // 
            this.numericUpDown_PeriodRecordingValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericUpDown_PeriodRecordingValue.Location = new System.Drawing.Point(104, 42);
            this.numericUpDown_PeriodRecordingValue.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown_PeriodRecordingValue.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_PeriodRecordingValue.Name = "numericUpDown_PeriodRecordingValue";
            this.numericUpDown_PeriodRecordingValue.Size = new System.Drawing.Size(70, 20);
            this.numericUpDown_PeriodRecordingValue.TabIndex = 8;
            this.numericUpDown_PeriodRecordingValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_PeriodRecordingValue.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // radioButton_PeriodRecording
            // 
            this.radioButton_PeriodRecording.AutoSize = true;
            this.radioButton_PeriodRecording.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButton_PeriodRecording.Location = new System.Drawing.Point(13, 42);
            this.radioButton_PeriodRecording.Name = "radioButton_PeriodRecording";
            this.radioButton_PeriodRecording.Size = new System.Drawing.Size(69, 17);
            this.radioButton_PeriodRecording.TabIndex = 9;
            this.radioButton_PeriodRecording.Text = "By Period";
            this.radioButton_PeriodRecording.UseVisualStyleBackColor = true;
            this.radioButton_PeriodRecording.CheckedChanged += new System.EventHandler(this.radioButton_PeriodRecording_CheckedChanged);
            // 
            // radioButton_NormalRecording
            // 
            this.radioButton_NormalRecording.AutoSize = true;
            this.radioButton_NormalRecording.Checked = true;
            this.radioButton_NormalRecording.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButton_NormalRecording.Location = new System.Drawing.Point(13, 19);
            this.radioButton_NormalRecording.Name = "radioButton_NormalRecording";
            this.radioButton_NormalRecording.Size = new System.Drawing.Size(166, 17);
            this.radioButton_NormalRecording.TabIndex = 8;
            this.radioButton_NormalRecording.TabStop = true;
            this.radioButton_NormalRecording.Text = "By Every Tag Value Changing";
            this.radioButton_NormalRecording.UseVisualStyleBackColor = true;
            this.radioButton_NormalRecording.CheckedChanged += new System.EventHandler(this.radioButton_NormalRecording_CheckedChanged);
            // 
            // checkedListBox_SelectionRecording
            // 
            this.checkedListBox_SelectionRecording.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.checkedListBox_SelectionRecording.CheckOnClick = true;
            this.checkedListBox_SelectionRecording.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.checkedListBox_SelectionRecording.FormattingEnabled = true;
            this.checkedListBox_SelectionRecording.Location = new System.Drawing.Point(6, 88);
            this.checkedListBox_SelectionRecording.Name = "checkedListBox_SelectionRecording";
            this.checkedListBox_SelectionRecording.Size = new System.Drawing.Size(239, 47);
            this.checkedListBox_SelectionRecording.TabIndex = 8;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(420, 183);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(81, 21);
            this.button_Cancel.TabIndex = 8;
            this.button_Cancel.Text = "Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radioButton_TimeStampAsDateTime);
            this.groupBox3.Controls.Add(this.radioButton_TimeStampAsTicks);
            this.groupBox3.Location = new System.Drawing.Point(267, 141);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(130, 63);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Time Stamp Format";
            // 
            // radioButton_TimeStampAsDateTime
            // 
            this.radioButton_TimeStampAsDateTime.AutoSize = true;
            this.radioButton_TimeStampAsDateTime.Checked = true;
            this.radioButton_TimeStampAsDateTime.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButton_TimeStampAsDateTime.Location = new System.Drawing.Point(13, 17);
            this.radioButton_TimeStampAsDateTime.Name = "radioButton_TimeStampAsDateTime";
            this.radioButton_TimeStampAsDateTime.Size = new System.Drawing.Size(73, 17);
            this.radioButton_TimeStampAsDateTime.TabIndex = 12;
            this.radioButton_TimeStampAsDateTime.TabStop = true;
            this.radioButton_TimeStampAsDateTime.Text = "Date Time";
            this.radioButton_TimeStampAsDateTime.UseVisualStyleBackColor = true;
            // 
            // radioButton_TimeStampAsTicks
            // 
            this.radioButton_TimeStampAsTicks.AutoSize = true;
            this.radioButton_TimeStampAsTicks.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButton_TimeStampAsTicks.Location = new System.Drawing.Point(13, 40);
            this.radioButton_TimeStampAsTicks.Name = "radioButton_TimeStampAsTicks";
            this.radioButton_TimeStampAsTicks.Size = new System.Drawing.Size(50, 17);
            this.radioButton_TimeStampAsTicks.TabIndex = 11;
            this.radioButton_TimeStampAsTicks.Text = "Ticks";
            this.radioButton_TimeStampAsTicks.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.textBox_FileNamePrefix);
            this.groupBox4.Controls.Add(this.button_ShowDirectoryDialog);
            this.groupBox4.Controls.Add(this.textBox_FileDirectory);
            this.groupBox4.Location = new System.Drawing.Point(9, 2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(508, 61);
            this.groupBox4.TabIndex = 13;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Output";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(28, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "File Location:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 38);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "File name Prefix:";
            // 
            // textBox_FileNamePrefix
            // 
            this.textBox_FileNamePrefix.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_FileNamePrefix.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBox_FileNamePrefix.Location = new System.Drawing.Point(104, 38);
            this.textBox_FileNamePrefix.Name = "textBox_FileNamePrefix";
            this.textBox_FileNamePrefix.Size = new System.Drawing.Size(167, 13);
            this.textBox_FileNamePrefix.TabIndex = 15;
            // 
            // button_ShowDirectoryDialog
            // 
            this.button_ShowDirectoryDialog.Location = new System.Drawing.Point(459, 14);
            this.button_ShowDirectoryDialog.Name = "button_ShowDirectoryDialog";
            this.button_ShowDirectoryDialog.Size = new System.Drawing.Size(33, 23);
            this.button_ShowDirectoryDialog.TabIndex = 14;
            this.button_ShowDirectoryDialog.Text = "...";
            this.button_ShowDirectoryDialog.UseVisualStyleBackColor = true;
            this.button_ShowDirectoryDialog.Click += new System.EventHandler(this.button_ShowDirectoryDialog_Click);
            // 
            // textBox_FileDirectory
            // 
            this.textBox_FileDirectory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_FileDirectory.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBox_FileDirectory.Location = new System.Drawing.Point(104, 19);
            this.textBox_FileDirectory.Name = "textBox_FileDirectory";
            this.textBox_FileDirectory.Size = new System.Drawing.Size(349, 13);
            this.textBox_FileDirectory.TabIndex = 0;
            // 
            // FormRecordSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(530, 213);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button_Apply);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "FormRecordSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Recording of Tag Values";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_SeparationFilesBySize)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_PeriodRecordingValue)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Apply;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_SeparationFilesByPeriod;
        private System.Windows.Forms.NumericUpDown numericUpDown_SeparationFilesBySize;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckedListBox checkedListBox_SelectionRecording;
        private System.Windows.Forms.RadioButton radioButton_SelectionRecording;
        private System.Windows.Forms.RadioButton radioButton_PeriodRecording;
        private System.Windows.Forms.RadioButton radioButton_NormalRecording;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.ComboBox comboBox_PeriodRecordingUnit;
        private System.Windows.Forms.NumericUpDown numericUpDown_PeriodRecordingValue;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioButton_TimeStampAsDateTime;
        private System.Windows.Forms.RadioButton radioButton_TimeStampAsTicks;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button button_ShowDirectoryDialog;
        private System.Windows.Forms.TextBox textBox_FileDirectory;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_FileNamePrefix;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    }
}