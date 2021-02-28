namespace LogixTool.Controls
{
    partial class TagBrowserControl
    {
        /// <summary> 
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            this.contextMenuStrip_Cells = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Paste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Delete = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip_ColumnHeader = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.requiredUpdateRateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actualUpdateRateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actualServerReplyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataTypeVisibleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writeValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableInstanceIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textBox_CommonUpdateRate = new System.Windows.Forms.TextBox();
            this.splitContainer_Grid = new System.Windows.Forms.SplitContainer();
            this.checkBox_CommonWriteEnable = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button_MoveRowsUp = new System.Windows.Forms.Button();
            this.button_MoveRowsDown = new System.Windows.Forms.Button();
            this.comboBox_CommonRadix = new System.Windows.Forms.ComboBox();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.ColumnDevice = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ColumnTag = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnDataType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnReadRate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnActualUpdateRate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnActualServerReply = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnRadix = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ColumnReadValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnWriteValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnWriteEnable = new LogixTool.Controls.DataGridViewDisableCheckBoxColumn();
            this.ColumnComMethod = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ColumnTableNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewDisableCheckBoxColumn1 = new LogixTool.Controls.DataGridViewDisableCheckBoxColumn();
            this.dataGridViewDisableButtonColumn1 = new LogixTool.Controls.DataGridViewDisableButtonColumn();
            this.dataGridViewButtonColumn1 = new System.Windows.Forms.DataGridViewButtonColumn();
            this.contextMenuStrip_Cells.SuspendLayout();
            this.contextMenuStrip_ColumnHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Grid)).BeginInit();
            this.splitContainer_Grid.Panel1.SuspendLayout();
            this.splitContainer_Grid.Panel2.SuspendLayout();
            this.splitContainer_Grid.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuStrip_Cells
            // 
            this.contextMenuStrip_Cells.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_Copy,
            this.toolStripMenuItem_Paste,
            this.toolStripMenuItem_Delete});
            this.contextMenuStrip_Cells.Name = "contextMenuStrip";
            this.contextMenuStrip_Cells.Size = new System.Drawing.Size(108, 70);
            this.contextMenuStrip_Cells.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Cells_Opening);
            // 
            // toolStripMenuItem_Copy
            // 
            this.toolStripMenuItem_Copy.Name = "toolStripMenuItem_Copy";
            this.toolStripMenuItem_Copy.Size = new System.Drawing.Size(107, 22);
            this.toolStripMenuItem_Copy.Text = "Copy";
            this.toolStripMenuItem_Copy.Click += new System.EventHandler(this.toolStripMenuItem_Copy_Click);
            // 
            // toolStripMenuItem_Paste
            // 
            this.toolStripMenuItem_Paste.Name = "toolStripMenuItem_Paste";
            this.toolStripMenuItem_Paste.Size = new System.Drawing.Size(107, 22);
            this.toolStripMenuItem_Paste.Text = "Paste";
            this.toolStripMenuItem_Paste.Click += new System.EventHandler(this.toolStripMenuItem_Paste_Click);
            // 
            // toolStripMenuItem_Delete
            // 
            this.toolStripMenuItem_Delete.Name = "toolStripMenuItem_Delete";
            this.toolStripMenuItem_Delete.Size = new System.Drawing.Size(107, 22);
            this.toolStripMenuItem_Delete.Text = "Delete";
            this.toolStripMenuItem_Delete.Click += new System.EventHandler(this.toolStripMenuItem_Delete_Click);
            // 
            // contextMenuStrip_ColumnHeader
            // 
            this.contextMenuStrip_ColumnHeader.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.requiredUpdateRateToolStripMenuItem,
            this.actualUpdateRateToolStripMenuItem,
            this.actualServerReplyToolStripMenuItem,
            this.dataTypeVisibleToolStripMenuItem,
            this.writeValueToolStripMenuItem,
            this.tableInstanceIDToolStripMenuItem});
            this.contextMenuStrip_ColumnHeader.Name = "contextMenuStrip";
            this.contextMenuStrip_ColumnHeader.Size = new System.Drawing.Size(189, 158);
            this.contextMenuStrip_ColumnHeader.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_ColumnHeader_Opening);
            // 
            // requiredUpdateRateToolStripMenuItem
            // 
            this.requiredUpdateRateToolStripMenuItem.Name = "requiredUpdateRateToolStripMenuItem";
            this.requiredUpdateRateToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.requiredUpdateRateToolStripMenuItem.Text = "Required Update Rate";
            this.requiredUpdateRateToolStripMenuItem.Click += new System.EventHandler(this.requiredUpdateRateToolStripMenuItem_Click);
            // 
            // actualUpdateRateToolStripMenuItem
            // 
            this.actualUpdateRateToolStripMenuItem.Name = "actualUpdateRateToolStripMenuItem";
            this.actualUpdateRateToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.actualUpdateRateToolStripMenuItem.Text = "Actual Update Rate";
            this.actualUpdateRateToolStripMenuItem.Click += new System.EventHandler(this.actualUpdateRateToolStripMenuItem_Click);
            // 
            // actualServerReplyToolStripMenuItem
            // 
            this.actualServerReplyToolStripMenuItem.Name = "actualServerReplyToolStripMenuItem";
            this.actualServerReplyToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.actualServerReplyToolStripMenuItem.Text = "Actual Server Reply";
            this.actualServerReplyToolStripMenuItem.Click += new System.EventHandler(this.actualServerReplyToolStripMenuItem_Click);
            // 
            // dataTypeVisibleToolStripMenuItem
            // 
            this.dataTypeVisibleToolStripMenuItem.Name = "dataTypeVisibleToolStripMenuItem";
            this.dataTypeVisibleToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.dataTypeVisibleToolStripMenuItem.Text = "Data Type";
            this.dataTypeVisibleToolStripMenuItem.Click += new System.EventHandler(this.dataTypeVisibleToolStripMenuItem_Click);
            // 
            // writeValueToolStripMenuItem
            // 
            this.writeValueToolStripMenuItem.Name = "writeValueToolStripMenuItem";
            this.writeValueToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.writeValueToolStripMenuItem.Text = "Write Value";
            this.writeValueToolStripMenuItem.Click += new System.EventHandler(this.writeValueToolStripMenuItem_Click);
            // 
            // tableInstanceIDToolStripMenuItem
            // 
            this.tableInstanceIDToolStripMenuItem.Name = "tableInstanceIDToolStripMenuItem";
            this.tableInstanceIDToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.tableInstanceIDToolStripMenuItem.Text = "Table Instance/ID";
            this.tableInstanceIDToolStripMenuItem.Click += new System.EventHandler(this.tableInstanceIDToolStripMenuItem_Click);
            // 
            // textBox_CommonUpdateRate
            // 
            this.textBox_CommonUpdateRate.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_CommonUpdateRate.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBox_CommonUpdateRate.Location = new System.Drawing.Point(380, 5);
            this.textBox_CommonUpdateRate.Name = "textBox_CommonUpdateRate";
            this.textBox_CommonUpdateRate.Size = new System.Drawing.Size(97, 13);
            this.textBox_CommonUpdateRate.TabIndex = 2;
            this.textBox_CommonUpdateRate.Text = "1000";
            this.textBox_CommonUpdateRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox_CommonUpdateRate.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_CommonUpdateRate_KeyPress);
            // 
            // splitContainer_Grid
            // 
            this.splitContainer_Grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_Grid.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer_Grid.IsSplitterFixed = true;
            this.splitContainer_Grid.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_Grid.Name = "splitContainer_Grid";
            this.splitContainer_Grid.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_Grid.Panel1
            // 
            this.splitContainer_Grid.Panel1.Controls.Add(this.checkBox_CommonWriteEnable);
            this.splitContainer_Grid.Panel1.Controls.Add(this.panel1);
            this.splitContainer_Grid.Panel1.Controls.Add(this.comboBox_CommonRadix);
            this.splitContainer_Grid.Panel1.Controls.Add(this.textBox_CommonUpdateRate);
            // 
            // splitContainer_Grid.Panel2
            // 
            this.splitContainer_Grid.Panel2.Controls.Add(this.dataGridView);
            this.splitContainer_Grid.Size = new System.Drawing.Size(1242, 233);
            this.splitContainer_Grid.SplitterDistance = 25;
            this.splitContainer_Grid.TabIndex = 4;
            // 
            // checkBox_CommonWriteEnable
            // 
            this.checkBox_CommonWriteEnable.AutoSize = true;
            this.checkBox_CommonWriteEnable.Location = new System.Drawing.Point(1011, 5);
            this.checkBox_CommonWriteEnable.Name = "checkBox_CommonWriteEnable";
            this.checkBox_CommonWriteEnable.Size = new System.Drawing.Size(15, 14);
            this.checkBox_CommonWriteEnable.TabIndex = 8;
            this.checkBox_CommonWriteEnable.UseVisualStyleBackColor = true;
            this.checkBox_CommonWriteEnable.CheckedChanged += new System.EventHandler(this.checkBox_CommonWriteEnable_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button_MoveRowsUp);
            this.panel1.Controls.Add(this.button_MoveRowsDown);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(32, 25);
            this.panel1.TabIndex = 7;
            // 
            // button_MoveRowsUp
            // 
            this.button_MoveRowsUp.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button_MoveRowsUp.Location = new System.Drawing.Point(1, 1);
            this.button_MoveRowsUp.Name = "button_MoveRowsUp";
            this.button_MoveRowsUp.Size = new System.Drawing.Size(30, 11);
            this.button_MoveRowsUp.TabIndex = 5;
            this.button_MoveRowsUp.UseVisualStyleBackColor = true;
            this.button_MoveRowsUp.Click += new System.EventHandler(this.button_MoveRowsUp_Click);
            // 
            // button_MoveRowsDown
            // 
            this.button_MoveRowsDown.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button_MoveRowsDown.Location = new System.Drawing.Point(1, 13);
            this.button_MoveRowsDown.Name = "button_MoveRowsDown";
            this.button_MoveRowsDown.Size = new System.Drawing.Size(30, 11);
            this.button_MoveRowsDown.TabIndex = 6;
            this.button_MoveRowsDown.UseVisualStyleBackColor = true;
            this.button_MoveRowsDown.Click += new System.EventHandler(this.button_MoveRowsDown_Click);
            // 
            // comboBox_CommonRadix
            // 
            this.comboBox_CommonRadix.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_CommonRadix.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_CommonRadix.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.comboBox_CommonRadix.FormattingEnabled = true;
            this.comboBox_CommonRadix.Location = new System.Drawing.Point(674, 1);
            this.comboBox_CommonRadix.Name = "comboBox_CommonRadix";
            this.comboBox_CommonRadix.Size = new System.Drawing.Size(101, 21);
            this.comboBox_CommonRadix.TabIndex = 4;
            this.comboBox_CommonRadix.SelectedIndexChanged += new System.EventHandler(this.comboBox_CommonRadix_SelectedIndexChanged);
            this.comboBox_CommonRadix.SizeChanged += new System.EventHandler(this.comboBox_CommonRadix_SizeChanged);
            // 
            // dataGridView
            // 
            this.dataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnDevice,
            this.ColumnTag,
            this.ColumnDataType,
            this.ColumnStatus,
            this.ColumnReadRate,
            this.ColumnActualUpdateRate,
            this.ColumnActualServerReply,
            this.ColumnRadix,
            this.ColumnReadValue,
            this.ColumnWriteValue,
            this.ColumnWriteEnable,
            this.ColumnComMethod,
            this.ColumnTableNumber});
            this.dataGridView.ContextMenuStrip = this.contextMenuStrip_Cells;
            this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView.GridColor = System.Drawing.Color.LightGray;
            this.dataGridView.Location = new System.Drawing.Point(0, 0);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersWidth = 30;
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dataGridView.RowsDefaultCellStyle = dataGridViewCellStyle11;
            this.dataGridView.Size = new System.Drawing.Size(1242, 204);
            this.dataGridView.TabIndex = 1;
            this.dataGridView.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dataGridView_CellBeginEdit);
            this.dataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellClick);
            this.dataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellEndEdit);
            this.dataGridView.ColumnStateChanged += new System.Windows.Forms.DataGridViewColumnStateChangedEventHandler(this.dataGridView_ColumnStateChanged);
            this.dataGridView.ColumnWidthChanged += new System.Windows.Forms.DataGridViewColumnEventHandler(this.dataGridView_ColumnWidthChanged);
            this.dataGridView.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.dataGridView_RowsAdded);
            this.dataGridView.Scroll += new System.Windows.Forms.ScrollEventHandler(this.dataGridView_Scroll);
            this.dataGridView.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.dataGridView_UserAddedRow);
            this.dataGridView.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.dataGridView_UserDeletedRow);
            // 
            // ColumnDevice
            // 
            this.ColumnDevice.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ColumnDevice.HeaderText = "Device";
            this.ColumnDevice.Name = "ColumnDevice";
            this.ColumnDevice.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ColumnDevice.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.ColumnDevice.Width = 90;
            // 
            // ColumnTag
            // 
            this.ColumnTag.HeaderText = "Tag Name";
            this.ColumnTag.Name = "ColumnTag";
            // 
            // ColumnDataType
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Gainsboro;
            this.ColumnDataType.DefaultCellStyle = dataGridViewCellStyle1;
            this.ColumnDataType.HeaderText = "Data Type";
            this.ColumnDataType.Name = "ColumnDataType";
            this.ColumnDataType.ReadOnly = true;
            // 
            // ColumnStatus
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Gainsboro;
            this.ColumnStatus.DefaultCellStyle = dataGridViewCellStyle2;
            this.ColumnStatus.HeaderText = "Status";
            this.ColumnStatus.Name = "ColumnStatus";
            this.ColumnStatus.ReadOnly = true;
            this.ColumnStatus.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColumnStatus.Width = 60;
            // 
            // ColumnReadRate
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.ColumnReadRate.DefaultCellStyle = dataGridViewCellStyle3;
            this.ColumnReadRate.HeaderText = "Req. Update Rate (ms)";
            this.ColumnReadRate.Name = "ColumnReadRate";
            this.ColumnReadRate.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColumnReadRate.Width = 95;
            // 
            // ColumnActualUpdateRate
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.Gainsboro;
            this.ColumnActualUpdateRate.DefaultCellStyle = dataGridViewCellStyle4;
            this.ColumnActualUpdateRate.HeaderText = "Actual Update Rate (ms)";
            this.ColumnActualUpdateRate.Name = "ColumnActualUpdateRate";
            this.ColumnActualUpdateRate.ReadOnly = true;
            this.ColumnActualUpdateRate.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ColumnActualServerReply
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.Gainsboro;
            this.ColumnActualServerReply.DefaultCellStyle = dataGridViewCellStyle5;
            this.ColumnActualServerReply.HeaderText = "Actual Server Reply (ms)";
            this.ColumnActualServerReply.Name = "ColumnActualServerReply";
            this.ColumnActualServerReply.ReadOnly = true;
            this.ColumnActualServerReply.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ColumnRadix
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.ColumnRadix.DefaultCellStyle = dataGridViewCellStyle6;
            this.ColumnRadix.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ColumnRadix.HeaderText = "Radix";
            this.ColumnRadix.Name = "ColumnRadix";
            // 
            // ColumnReadValue
            // 
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.Gainsboro;
            this.ColumnReadValue.DefaultCellStyle = dataGridViewCellStyle7;
            this.ColumnReadValue.HeaderText = "Read Value";
            this.ColumnReadValue.Name = "ColumnReadValue";
            this.ColumnReadValue.ReadOnly = true;
            this.ColumnReadValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ColumnWriteValue
            // 
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.ColumnWriteValue.DefaultCellStyle = dataGridViewCellStyle8;
            this.ColumnWriteValue.HeaderText = "Write Value";
            this.ColumnWriteValue.Name = "ColumnWriteValue";
            this.ColumnWriteValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ColumnWriteEnable
            // 
            this.ColumnWriteEnable.HeaderText = "Write Enable";
            this.ColumnWriteEnable.Name = "ColumnWriteEnable";
            this.ColumnWriteEnable.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // ColumnComMethod
            // 
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.ColumnComMethod.DefaultCellStyle = dataGridViewCellStyle9;
            this.ColumnComMethod.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ColumnComMethod.HeaderText = "Method";
            this.ColumnComMethod.Name = "ColumnComMethod";
            this.ColumnComMethod.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // ColumnTableNumber
            // 
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.ColumnTableNumber.DefaultCellStyle = dataGridViewCellStyle10;
            this.ColumnTableNumber.HeaderText = "Table Number";
            this.ColumnTableNumber.Name = "ColumnTableNumber";
            this.ColumnTableNumber.ReadOnly = true;
            // 
            // dataGridViewDisableCheckBoxColumn1
            // 
            this.dataGridViewDisableCheckBoxColumn1.HeaderText = "Write Enable";
            this.dataGridViewDisableCheckBoxColumn1.Name = "dataGridViewDisableCheckBoxColumn1";
            this.dataGridViewDisableCheckBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // dataGridViewDisableButtonColumn1
            // 
            this.dataGridViewDisableButtonColumn1.HeaderText = "";
            this.dataGridViewDisableButtonColumn1.Name = "dataGridViewDisableButtonColumn1";
            this.dataGridViewDisableButtonColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewDisableButtonColumn1.Width = 25;
            // 
            // dataGridViewButtonColumn1
            // 
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle12.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dataGridViewButtonColumn1.DefaultCellStyle = dataGridViewCellStyle12;
            this.dataGridViewButtonColumn1.HeaderText = "*";
            this.dataGridViewButtonColumn1.Name = "dataGridViewButtonColumn1";
            this.dataGridViewButtonColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewButtonColumn1.Width = 25;
            // 
            // TagBrowserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer_Grid);
            this.Name = "TagBrowserControl";
            this.Size = new System.Drawing.Size(1242, 233);
            this.contextMenuStrip_Cells.ResumeLayout(false);
            this.contextMenuStrip_ColumnHeader.ResumeLayout(false);
            this.splitContainer_Grid.Panel1.ResumeLayout(false);
            this.splitContainer_Grid.Panel1.PerformLayout();
            this.splitContainer_Grid.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_Grid)).EndInit();
            this.splitContainer_Grid.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_Cells;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Copy;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Paste;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_ColumnHeader;
        private System.Windows.Forms.ToolStripMenuItem requiredUpdateRateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem actualUpdateRateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem actualServerReplyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dataTypeVisibleToolStripMenuItem;
        private System.Windows.Forms.TextBox textBox_CommonUpdateRate;
        private System.Windows.Forms.SplitContainer splitContainer_Grid;
        private System.Windows.Forms.ComboBox comboBox_CommonRadix;
        private System.Windows.Forms.Button button_MoveRowsDown;
        private System.Windows.Forms.Button button_MoveRowsUp;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Delete;
        private System.Windows.Forms.DataGridViewButtonColumn dataGridViewButtonColumn1;
        private DataGridViewDisableButtonColumn dataGridViewDisableButtonColumn1;
        private System.Windows.Forms.ToolStripMenuItem tableInstanceIDToolStripMenuItem;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColumnDevice;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTag;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDataType;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnReadRate;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnActualUpdateRate;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnActualServerReply;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColumnRadix;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnReadValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnWriteValue;
        private DataGridViewDisableCheckBoxColumn ColumnWriteEnable;
        private System.Windows.Forms.DataGridViewComboBoxColumn ColumnComMethod;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTableNumber;
        private System.Windows.Forms.ToolStripMenuItem writeValueToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBox_CommonWriteEnable;
        private DataGridViewDisableCheckBoxColumn dataGridViewDisableCheckBoxColumn1;
    }
}
