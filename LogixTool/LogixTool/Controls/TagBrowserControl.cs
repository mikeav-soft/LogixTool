using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Timers;
using LogixTool.Common;
using LogixTool.Common.Extension;
using EIP.AllenBradley;
using EIP.AllenBradley.Models.Events;
using LogixTool.LocalDatabase;

namespace LogixTool.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TagBrowserControl : UserControl
    {
        private const string MESSAGE_BOX_HEADER = "Tag Browser";

        /// <summary>
        /// Представляет собой перечисления режимов отображения данного визуального компонета.
        /// </summary>
        public enum ViewMode { Edit, Monitor, Lock }

        #region [ PUBLIC PROPERTIES ]
        /* ======================================================================================== */
        /// <summary>
        /// Получает или задает коллекцию доступных устройств.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<LogixTask> TaskCollection
        {
            get
            {
                Dictionary<string, LogixTask> result = new Dictionary<string, LogixTask>();
                foreach (object obj in ColumnDevice.Items)
                {
                    if (obj is LogixTask)
                    {
                        LogixTask task = (LogixTask)obj;
                        result.Add(task.Device.Name, task);
                    }
                }
                return result.Values.ToList();
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                ColumnDevice.Items.Clear();
                Dictionary<string, LogixTask> reference = new Dictionary<string, LogixTask>();
                foreach (LogixTask task in value)
                {
                    string key = task.Device.Name;
                    if (!reference.ContainsKey(key))
                    {
                        reference.Add(key, task);
                    }
                }

                foreach (LogixTask task in reference.Values)
                {
                    ColumnDevice.Items.Add(task);
                }
            }
        }

        /// <summary>
        /// Получает коллекцию тэгов с соответствующими строками таблицы.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<LogixTagHandler, DataGridViewRow> TagsByRows
        {
            get
            {
                Dictionary<LogixTagHandler, DataGridViewRow> result = new Dictionary<LogixTagHandler, DataGridViewRow>();
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    object obj = row.Cells[this.ColumnTag.Index].Value;
                    if (!row.IsNewRow && obj != null && obj is LogixTagHandler)
                    {
                        LogixTagHandler tag = (LogixTagHandler)obj;
                        if (!result.ContainsKey(tag))
                        {
                            result.Add(tag, row);
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Получает коллекцию устройств с соответствующими строками таблицы.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<LogixTask, List<DataGridViewRow>> TasksByRows
        {
            get
            {
                Dictionary<LogixTask, List<DataGridViewRow>> result = new Dictionary<LogixTask, List<DataGridViewRow>>();
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    object obj = row.Cells[this.ColumnDevice.Index].Value;
                    if (!row.IsNewRow && obj != null && obj is LogixTask)
                    {
                        LogixTask task = (LogixTask)obj;
                        if (!result.ContainsKey(task))
                        {
                            result.Add(task, new List<DataGridViewRow>());
                        }

                        result[task].Add(row);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Получает коллекцию устройств с соответствующими тэгами.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<LogixTask, List<LogixTagHandler>> TasksByTags
        {
            get
            {
                Dictionary<LogixTask, List<LogixTagHandler>> result = new Dictionary<LogixTask, List<LogixTagHandler>>();
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    object taskObj = row.Cells[this.ColumnDevice.Index].Value;
                    object tagObj = row.Cells[this.ColumnTag.Index].Value;

                    if (taskObj != null && taskObj is LogixTask && tagObj != null && tagObj is LogixTagHandler)
                    {
                        LogixTask task = (LogixTask)taskObj;
                        LogixTagHandler tag = (LogixTagHandler)tagObj;

                        if (!result.ContainsKey(task))
                        {
                            result.Add(task, new List<LogixTagHandler>());
                        }

                        result[task].Add(tag);
                    }
                }

                return result;
            }
        }

        private ViewMode _Mode;
        /// <summary>
        /// Вовзращает режим отображения/редактирования элемента управления.
        /// </summary>
        public ViewMode Mode
        {
            get
            {
                return _Mode;
            }
            private set
            {
                // Запоминаем значение своства перед дальнейшими операциями.
                ViewMode lastMode = _Mode;
                // Присваиваем новое значение свойства.
                _Mode = value;
                // Устанавливаем режим отображения элемента управления.
                this.SetVisualizationMode();

                // Вызывам событие при изменении значения свояства.
                if (lastMode != _Mode)
                {
                    Event_ModeWasChanged();
                }
            }
        }
        /* ======================================================================================== */
        #endregion

        private LogixTask currentEditedTask;                        //
        private LogixTagHandler currentEditedTag;                   //
        private System.Windows.Threading.DispatcherTimer dtimer;    //
        private Color editBackColor = Color.White;                  //
        private Color lockBackColor = Color.Gainsboro;              //

        /// <summary>
        /// 
        /// </summary>
        public TagBrowserControl()
        {
            InitializeComponent();

            // Устаналиваем режим работы как редактирование.
            this.Mode = ViewMode.Edit;

            // Обновляем верхнюю панель инструментов.
            UpdateUpperPanelControls();

            // Устанавливаем коллекцию
            this.comboBox_CommonRadix.SetCollectionFromEnumeration<TagValueRadix>();
            // Устанавливаем начальное значение.
            this.comboBox_CommonRadix.SetItemFromText(Enum.GetName(typeof(TagValueRadix), TagValueRadix.Decimal));

            // Добавляем элементы перечисления в элементы выбора колонки ColumnRadix.
            this.ColumnRadix.Items.AddRange(Enum.GetNames(typeof(TagValueRadix)));
            // Добавляем элементы перечисления в элементы выбора колонки ColumnComMethod.
            this.ColumnComMethod.Items.AddRange(Enum.GetNames(typeof(TagReadMethod)));

            // Для всех колонок добавляем контекстное меню отображаемое при вызове на заголовке колонки.
            foreach (DataGridViewColumn gridViewColumn in this.dataGridView.Columns)
            {
                gridViewColumn.HeaderCell.ContextMenuStrip = contextMenuStrip_ColumnHeader;
            }

            this.currentEditedTask = null;

            // Конфигурируем таймер обновления значений визуальных компонентов.
            dtimer = new System.Windows.Threading.DispatcherTimer();
            dtimer.Interval = TimeSpan.FromMilliseconds(100);
            dtimer.Tick += dtimer_Tick;
            dtimer.IsEnabled = true;
        }

        #region [ EVENTS ]
        /* ======================================================================================== */
        /// <summary>
        /// Возникает при изменении свойств тэгов.
        /// </summary>
        [Category("Специальные")]
        public event TagEventHandler TagWasModified;
        /// <summary>
        /// Возникает при удалении тэгов из коллекции.
        /// </summary>
        [Category("Специальные")]
        public event TagsEventHandler TagsWasRemoved;
        /// <summary>
        /// Возникает при добавлении тэгов в коллекцию.
        /// </summary>
        [Category("Специальные")]
        public event TagsEventHandler TagsWasAdded;
        /// <summary>
        /// Возникает при появлении значения тэгов для записи.
        /// </summary>
        [Category("Специальные")]
        public event TagEventHandler TagHasValuesForWrite;
        /// <summary>
        /// Возникает при запросе на изменение устройства относящегося к тэгу.
        /// </summary>
        [Category("Специальные")]
        public event TagTaskBeginEditEventHandler TagDeviceBeginEdit;
        /// <summary>
        /// Возникает при изменении устройства относящегося к тэгу.
        /// </summary>
        [Category("Специальные")]
        public event TagTaskEndEditEventHandler TagDeviceEndEdit;
        /// <summary>
        /// Возникает при изменении режима отображения элемента управления.
        /// </summary>
        [Category("Специальные")]
        public event EventHandler ModeWasChanged;

        /// <summary>
        /// Вызывает "Событие при изменении свойств тэгов".
        /// </summary>
        private void Event_TagWasModified(LogixTagHandler tag)
        {
            if (this.TagWasModified != null)
            {
                this.TagWasModified(this, new TagEventArgs(tag));
            }
        }

        /// <summary>
        /// Вызывает "Событие при удалении тэгов из коллекции".
        /// </summary>
        private void Event_TagsWasRemoved(List<LogixTagHandler> tags)
        {
            if (this.TagsWasRemoved != null)
            {
                this.TagsWasRemoved(this, new TagsEventArgs(tags));
            }
        }
        /// <summary>
        /// Вызывает "Событие при удалении тэгов из коллекции".
        /// </summary>
        private void Event_TagsWasRemoved(LogixTagHandler tag)
        {
            if (this.TagsWasRemoved != null)
            {
                List<LogixTagHandler> tags = new List<LogixTagHandler>();
                tags.Add(tag);
                this.TagsWasRemoved(this, new TagsEventArgs(tags));
            }
        }
        /// <summary>
        /// Вызывает "Событие при добавлении тэгов в коллекцию".
        /// </summary>
        private void Event_TagsWasAdded(List<LogixTagHandler> tags)
        {
            if (this.TagsWasAdded != null)
            {
                this.TagsWasAdded(this, new TagsEventArgs(tags));
            }
        }
        /// <summary>
        /// Вызывает "Событие при добавлении тэгов в коллекцию".
        /// </summary>
        private void Event_TagsWasAdded(LogixTagHandler tag)
        {
            if (this.TagsWasAdded != null)
            {
                List<LogixTagHandler> tags = new List<LogixTagHandler>();
                tags.Add(tag);
                this.TagsWasAdded(this, new TagsEventArgs(tags));
            }
        }
        /// <summary>
        /// Вызывает "Событие при появлении значения тэгов для записи".
        /// </summary>
        private void Event_TagsHasValuesForWrite(LogixTagHandler tag)
        {
            if (this.TagHasValuesForWrite != null)
            {
                this.TagHasValuesForWrite(this, new TagEventArgs(tag));
            }
        }
        /// <summary>
        /// Вызывает "Событие при запросе на изменение устройства относящегося к тэгу".
        /// </summary>
        private void Event_TagDeviceBeginEdit(TagTaskBeginEditEventArgs e)
        {
            if (this.TagDeviceBeginEdit != null)
            {
                this.TagDeviceBeginEdit(this, e);
            }
        }
        /// <summary>
        /// Вызывает "Событие при изменении устройства относящегося к тэгу".
        /// </summary>
        private void Event_TagDeviceEndEdit(TagTaskEndEditEventArgs e)
        {
            if (this.TagDeviceEndEdit != null)
            {
                this.TagDeviceEndEdit(this, e);
            }
        }
        /// <summary>
        /// Вызывает "Событие при изменении режима отображения элемента управления".
        /// </summary>
        private void Event_ModeWasChanged()
        {
            if (this.ModeWasChanged != null)
            {
                this.ModeWasChanged(this, null);
            }
        }
        /* ======================================================================================== */
        #endregion

        #region [ EVENT SUBSCRIPTIONS - FORMS ]
        /* ======================================================================================== */
        /// <summary>
        /// Подписка на событие : DispatcherTimer : Установленное время вышло.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void dtimer_Tick(object sender, EventArgs e)
        {
            UpdateVisualElements();
        }
        /// <summary>
        /// Подписка на событие : DataGridView : События нажатия курсора на ячейку.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //DataGridViewRow row = this.dataGridView.Rows[e.RowIndex];
            //DataGridViewCell cell = row.Cells[e.ColumnIndex];

            //if (cell is DataGridViewCheckBoxCell)
            //{
            //    DataGridViewCheckBoxCell checkBoxCell = (DataGridViewCheckBoxCell)cell;

            //}
        }
        /// <summary>
        /// Подписка на событие : DataGridView : Начато редактирование ячейки.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // В случае начала редактирования ячейки устройства, запоминаем его.
            if (e.ColumnIndex == this.ColumnDevice.Index)
            {
                DataGridViewRow row = dataGridView.Rows[e.RowIndex];
                object value = (row.Cells[e.ColumnIndex] as DataGridViewComboBoxCell).Value;
                if (value != null && value is LogixTask)
                {
                    this.currentEditedTask = (LogixTask)value;
                }
                else
                {
                    this.currentEditedTask = null;
                }
            }

            if (e.ColumnIndex == this.ColumnTag.Index)
            {
                DataGridViewRow row = dataGridView.Rows[e.RowIndex];
                object value = row.Cells[e.ColumnIndex].Value;
                if (value != null && value is LogixTagHandler)
                {
                    this.currentEditedTag = (LogixTagHandler)value;
                }
                else
                {
                    this.currentEditedTag = null;
                }
            }
        }
        /// <summary>
        /// Подписка на событие : DataGridView : Завершено редактирование ячейки.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = dataGridView.Rows[e.RowIndex];

            if (e.ColumnIndex == this.ColumnDevice.Index)
            {
                #region [ 1. COLUMN DEVICE. ]
                /*===========================================================================================*/
                object value = (row.Cells[this.ColumnDevice.Index] as DataGridViewComboBoxCell).Value;
                if (value == null)
                {
                    return;
                }

                // Устанавливаем значение-объект устройства подвязанного к тэгу.
                row.Cells[this.ColumnDevice.Index].SetComboBoxCellValue(value.ToString());

                LogixTagHandler tag = null;
                LogixTask oldDevice = currentEditedTask;
                LogixTask newDevice = null;

                // Из текущей строки получаем значение устройства тэга.
                object deviceValue = row.Cells[this.ColumnDevice.Index].Value;
                if (deviceValue != null && deviceValue is LogixTask)
                {
                    newDevice = (LogixTask)deviceValue;
                }

                object tagValue = row.Cells[this.ColumnTag.Index].Value;
                if (tagValue != null && tagValue is LogixTagHandler)
                {
                    tag = (LogixTagHandler)tagValue;
                }

                // Вызываем событие на начало редактирования устройства тэга.
                TagTaskBeginEditEventArgs tagDeviceChangingEventArgs = new TagTaskBeginEditEventArgs(tag, oldDevice, newDevice);
                Event_TagDeviceBeginEdit(tagDeviceChangingEventArgs);

                // В случае внешнего отказа на редактирование возвращаем старое значение.
                if (tagDeviceChangingEventArgs.Cancel)
                {
                    row.Cells[this.ColumnDevice.Index].Value = oldDevice;
                }
                else
                {
                    Event_TagDeviceEndEdit(new TagTaskEndEditEventArgs(tag, oldDevice, newDevice));
                }

                /*===========================================================================================*/
                #endregion
            }
            else if (e.ColumnIndex == this.ColumnTag.Index)
            {
                #region [ 2. COLUMN TAG. ]
                /*===========================================================================================*/
                if (row.Cells[this.ColumnTag.Index].Value == null)
                {
                    return;
                }

                // Получаем объект который хранится в ячейке имени тэга.
                string tagName = (row.Cells[ColumnTag.Index].Value).ToString().Trim();

                // В случае существования объекта тэга редактируем его имя, в противном случае создаем новый объект.
                if (this.currentEditedTag != null)
                {
                    this.currentEditedTag.Name = tagName;
                    row.Cells[ColumnTag.Index].Value = this.currentEditedTag;

                    // Вызываем событие.
                    Event_TagWasModified(this.currentEditedTag);
                }
                else
                {
                    LogixTagHandler tag = new LogixTagHandler(tagName);
                    row.Cells[ColumnTag.Index].Value = tag;

                    // Вызываем событие.
                    Event_TagsWasAdded(tag);
                }
                /*===========================================================================================*/
                #endregion
            }
            else if (e.ColumnIndex == this.ColumnReadRate.Index)
            {
                #region [ 3. COLUMN TAG UPDATE RATE. ]
                /*===========================================================================================*/
                if (row.Cells[this.ColumnReadRate.Index].Value == null)
                {
                    return;
                }

                LogixTagHandler tag;

                if (row.Cells[ColumnTag.Index].Value != null && row.Cells[ColumnTag.Index].Value is LogixTagHandler)
                {
                    tag = (LogixTagHandler)row.Cells[ColumnTag.Index].Value;
                }
                else
                {
                    return;
                }

                string readRateValueText = row.Cells[ColumnReadRate.Index].Value.ToString();

                if (readRateValueText == null || readRateValueText.Trim() == "" || !readRateValueText.Trim().All(c => Char.IsDigit(c)))
                {
                    MessageBox.Show("Imposible to set Update Rate Value.\r\nInput value must be contains digits only.", MESSAGE_BOX_HEADER, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                UInt32 value;
                if (UInt32.TryParse(readRateValueText, out value))
                {
                    tag.ReadUpdateRate = value;
                }
                else
                {
                    MessageBox.Show("Imposible to set Update Rate Value.\r\nInput value must be in range " + UInt32.MinValue.ToString() + "..." + UInt32.MaxValue.ToString() + ".", MESSAGE_BOX_HEADER, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                /*===========================================================================================*/
                #endregion
            }
            else if (e.ColumnIndex == this.ColumnRadix.Index)
            {
                #region [ 4. COLUMN TAG RADIX VALUE. ]
                /*===========================================================================================*/
                if (row.Cells[this.ColumnRadix.Index].Value == null || !(row.Cells[ColumnTag.Index].Value is LogixTagHandler))
                {
                    return;
                }

                LogixTagHandler tag = (LogixTagHandler)row.Cells[ColumnTag.Index].Value;
                object obj = Enum.Parse(typeof(TagValueRadix), row.Cells[this.ColumnRadix.Index].Value.ToString(), true);
                tag.Radix = (TagValueRadix)obj;
                /*===========================================================================================*/
                #endregion
            }
            else if (e.ColumnIndex == this.ColumnWriteValue.Index)
            {
                #region [ 5. COLUMN WRITE VALUE. ]
                /*===========================================================================================*/
                if (row.Cells[ColumnTag.Index].Value is LogixTagHandler)
                {
                    bool resultIsOk = false;

                    LogixTagHandler tag = (LogixTagHandler)row.Cells[ColumnTag.Index].Value;

                    if (tag.Type == null || tag.Type.Code == 0)
                    {
                        MessageBox.Show("Imposible to write value.\r\nTag must be read before.", MESSAGE_BOX_HEADER, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (tag.Type.ArrayDimension.HasValue)
                    {
                        return;
                    }

                    object valueForWriteObject = row.Cells[ColumnWriteValue.Index].Value;

                    if (valueForWriteObject != null)
                    {
                        string valueForWriteText = valueForWriteObject.ToString();

                        // Производим инициализацию значений для записи если их не существует.
                        // Для этого проверяем существуют ли данные, если их нет то копируем из ранее прочитанных данных.
                        if (tag.WriteValue.RequestedData == null)
                        {
                            tag.WriteValue.RequestedData = tag.ReadValue.Report.Copy().Data;
                        }

                        // Устанавливаем текущее значение элемента из текста и получаем результат операции.
                        resultIsOk = tag.SetWritedValueText(valueForWriteText, 0);

                        if (resultIsOk)
                        {
                            // Вызывает событие.
                            Event_TagsHasValuesForWrite(tag);
                        }
                    }
                }
                /*===========================================================================================*/
                #endregion
            }
            else if (e.ColumnIndex == this.ColumnComMethod.Index)
            {
                #region [ 6. COLUMN TAG RADIX VALUE. ]
                /*===========================================================================================*/
                if (row.Cells[this.ColumnComMethod.Index].Value == null || !(row.Cells[ColumnTag.Index].Value is LogixTagHandler))
                {
                    return;
                }

                LogixTagHandler tag = (LogixTagHandler)row.Cells[ColumnTag.Index].Value;
                object obj = Enum.Parse(typeof(TagReadMethod), row.Cells[this.ColumnComMethod.Index].Value.ToString(), true);
                tag.ReadMethod = (TagReadMethod)obj;
                /*===========================================================================================*/
                #endregion
            }
            else if (e.ColumnIndex == this.ColumnWriteEnable.Index)
            {
                #region [ 7. COLUMN WRITE VALUE ENABLE. ]
                /*===========================================================================================*/
                if (!(row.Cells[ColumnWriteEnable.Index] is DataGridViewCheckBoxCell) || !(row.Cells[ColumnTag.Index].Value is LogixTagHandler))
                {
                    return;
                }

                LogixTagHandler tag = (LogixTagHandler)row.Cells[ColumnTag.Index].Value;

                if (tag == null)
                {
                    return;
                }

                DataGridViewCheckBoxCell checkBoxCell = (DataGridViewCheckBoxCell)row.Cells[ColumnWriteEnable.Index];
                bool writeEnable = (checkBoxCell.Value != null && (bool)checkBoxCell.Value == true);

                if (writeEnable && tag.WriteValue.RequestedData == null)
                {
                    tag.WriteValue.RequestedData = tag.ReadValue.Report.Data;
                }

                /*===========================================================================================*/
                #endregion
            }
        }
        /// <summary>
        /// Подписка на событие : DataGridView : Добавлена новая строка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            //for (int ix = 0; ix < e.RowCount; ix++)
            //{
            //    DataGridViewRow row = this.dataGridView.Rows[ix + e.RowIndex - 1];
            //    row.Cells[this.ColumnTag.Index].Value = new CLXTag("new_tag");
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            //DataGridViewRow row = e.Row;
            //row.Cells[this.ColumnTag.Index].Value = new CLXTag("new_tag");
        }
        /// <summary>
        /// Подписка на событие : DataGridView : Пользователь удалил строку.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            if (e.Row != null)
            {
                object value = e.Row.Cells[this.ColumnTag.Index].Value;
                if (value != null && value is LogixTagHandler)
                {
                    Event_TagsWasRemoved((LogixTagHandler)value);
                }
            }
        }
        /// <summary>
        /// Подписка на событие : DataGridView : Ширина колонки изменена.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            UpdateUpperPanelControls();
        }
        /// <summary>
        /// Подписка на событие : DataGridView : Состояние колонки изменено.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_ColumnStateChanged(object sender, DataGridViewColumnStateChangedEventArgs e)
        {
            UpdateUpperPanelControls();
        }
        /// <summary>
        /// Подписка на событие : DataGridView : Произведена прокрутка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                UpdateUpperPanelControls();
            }
        }
        /// <summary>
        /// Подписка на событие : Button : Нажата кнопка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_MoveRowsUp_Click(object sender, EventArgs e)
        {
            if (this.Mode != ViewMode.Edit)
            {
                return;
            }

            this.dataGridView.MoveSelectedRows(false);
        }
        /// <summary>
        /// Подписка на событие : Button : Нажата кнопка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_MoveRowsDown_Click(object sender, EventArgs e)
        {
            if (this.Mode != ViewMode.Edit)
            {
                return;
            }

            this.dataGridView.MoveSelectedRows(true);
        }
        /// <summary>
        /// Подписка на событие : TextBox : Нажата кнопка клавиатуры.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_CommonUpdateRate_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter)
            {
                return;
            }

            string text = textBox_CommonUpdateRate.Text.Trim();

            if (text == "" || !text.All(c => Char.IsDigit(c)))
            {
                MessageBox.Show("Imposible to set Update Rate Value.\r\nInput value must be contains digits only.", MESSAGE_BOX_HEADER, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UInt32 value;
            if (!UInt32.TryParse(text, out value))
            {
                MessageBox.Show("Imposible to set Update Rate Value.\r\nInput value must be in range " + UInt32.MinValue.ToString() + "..." + UInt32.MaxValue.ToString() + ".", MESSAGE_BOX_HEADER, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                object tagObject = row.Cells[this.ColumnTag.Index].Value;
                if (tagObject != null && tagObject is LogixTagHandler)
                {
                    ((LogixTagHandler)tagObject).ReadUpdateRate = value;
                }
            }
        }
        /// <summary>
        /// Подписка на событие : ComboBox : Был изменен выбранный элемент.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_CommonRadix_SelectedIndexChanged(object sender, EventArgs e)
        {
            TagValueRadix result;
            if (Enum.TryParse<TagValueRadix>(comboBox_CommonRadix.Text, out result))
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    object tagObject = row.Cells[this.ColumnTag.Index].Value;
                    if (tagObject != null && tagObject is LogixTagHandler)
                    {
                        ((LogixTagHandler)tagObject).Radix = result;
                    }
                }
            }
        }
        /// <summary>
        /// Подписка на событие : ComboBox : Был изменен размер.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_CommonRadix_SizeChanged(object sender, EventArgs e)
        {
            comboBox_CommonRadix.Refresh();
        }
        /// <summary>
        /// Подписка на событие : CheckBox : Было изменено состояние.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_CommonWriteEnable_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in this.dataGridView.Rows)
            {
                DataGridViewCell cell = row.Cells[this.ColumnWriteEnable.Index];
                if (cell is DataGridViewDisableCheckBoxCell)
                {
                    DataGridViewDisableCheckBoxCell disableCheckBoxCell = (DataGridViewDisableCheckBoxCell)cell;
                    if (disableCheckBoxCell.Hide)
                    {
                        disableCheckBoxCell.Value = false;
                    }
                    else
                    {
                        disableCheckBoxCell.Value = checkBox_CommonWriteEnable.Checked;
                    }
                }
            }
        }
        /// <summary>
        /// Подписка на событие : ContextMenuStrip : Меню открывается.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStrip_Cells_Opening(object sender, CancelEventArgs e)
        {
            this.toolStripMenuItem_Copy.Enabled = (dataGridView.SelectedRows.Count > 0);
            this.toolStripMenuItem_Paste.Enabled = Clipboard.ContainsText();
            this.toolStripMenuItem_Delete.Enabled = (dataGridView.SelectedRows.Count > 0);
        }
        /// <summary>
        /// Подписка на событие : ContextMenuStrip : Нажат элемент меню.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_Copy_Click(object sender, EventArgs e)
        {
            string text = dataGridView.GetTextFromSelectedRows();

            if (text == "")
            {
                return;
            }

            Clipboard.SetText(text);
        }
        /// <summary>
        /// Подписка на событие : ContextMenuStrip : Нажат элемент меню.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_Paste_Click(object sender, EventArgs e)
        {
            // Получаем текст из буфера обмена.
            string text = Clipboard.GetText();
            if (text == null)
            {
                return;
            }

            // Вставляем в соответствуюшие ячейки данные из текста.
            dataGridView.AddRowsFromText(text);

            // Создаем объекты тэгов в тех ячейках где имеется оьъект String.
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                object value = row.Cells[this.ColumnTag.Index].Value;
                if (value != null && value is string)
                {
                    string tagName = ((string)value).Trim();
                    if (tagName != "")
                    {
                        LogixTagHandler tag = new LogixTagHandler(tagName);
                        row.Cells[this.ColumnTag.Index].Value = tag;

                        // Вызываем событие.
                        Event_TagsWasAdded(tag);
                    }
                }
            }
        }
        /// <summary>
        /// Подписка на событие : ContextMenuStrip : Нажат элемент меню.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_Delete_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                if (!row.IsNewRow)
                {
                    dataGridView.Rows.Remove(row);
                }
            }
        }

        /// <summary>
        /// Подписка на событие : ContextMenuStrip : Меню открывается.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStrip_ColumnHeader_Opening(object sender, CancelEventArgs e)
        {
            this.requiredUpdateRateToolStripMenuItem.Checked = this.ColumnReadRate.Visible;
            this.actualUpdateRateToolStripMenuItem.Checked = this.ColumnActualUpdateRate.Visible;
            this.actualServerReplyToolStripMenuItem.Checked = this.ColumnActualServerReply.Visible;
            this.dataTypeVisibleToolStripMenuItem.Checked = this.ColumnDataType.Visible;
            this.tableInstanceIDToolStripMenuItem.Checked = this.ColumnTableNumber.Visible;
            this.writeValueToolStripMenuItem.Checked = this.ColumnWriteValue.Visible;
        }
        /// <summary>
        /// Подписка на событие : ContextMenuStrip : Нажатие на элемент меню.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void requiredUpdateRateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ColumnReadRate.Visible = !this.ColumnReadRate.Visible;
        }
        /// <summary>
        /// Подписка на событие : ContextMenuStrip : Нажатие на элемент меню.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void actualUpdateRateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ColumnActualUpdateRate.Visible = !this.ColumnActualUpdateRate.Visible;
        }
        /// <summary>
        /// Подписка на событие : ContextMenuStrip : Нажатие на элемент меню.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void actualServerReplyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ColumnActualServerReply.Visible = !this.ColumnActualServerReply.Visible;
        }
        /// <summary>
        /// Подписка на событие : ContextMenuStrip : Нажатие на элемент меню.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataTypeVisibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ColumnDataType.Visible = !this.ColumnDataType.Visible;
        }
        /// <summary>
        /// Подписка на событие : ContextMenuStrip : Нажатие на элемент меню.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tableInstanceIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ColumnTableNumber.Visible = !this.ColumnTableNumber.Visible;
        }
        /// <summary>
        /// Подписка на событие : ContextMenuStrip : Нажатие на элемент меню.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void writeValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool visible = !this.ColumnWriteValue.Visible;
            this.ColumnWriteValue.Visible = visible;
            this.ColumnWriteEnable.Visible = visible;
        }
        /* ======================================================================================== */
        #endregion

        #region [ PUBLIC METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Добавляет новый тэг в коллекцию.
        /// </summary>
        /// <param name="deviceName">Имя устройства.</param>
        /// <param name="tag">Новый тэг.</param>
        public void Add(string deviceName, LogixTagHandler tag)
        {
            // Проверяем входные параметры.
            if (tag == null || deviceName == null || this.Mode != ViewMode.Edit)
            {
                return;
            }

            List<LogixTagHandler> tags = new List<LogixTagHandler>();
            tags.Add(tag);
            this.AddRange(deviceName, tags);
        }
        /// <summary>
        /// Добавляет новые тэги в коллекцию.
        /// </summary>
        /// <param name="deviceName">Имя устройства.</param>
        /// <param name="tags">Новые тэги.</param>
        public void AddRange(string deviceName, IEnumerable<LogixTagHandler> tags)
        {
            // Проверяем входные параметры.
            if (tags == null || tags.Count() == 0 || this.Mode != ViewMode.Edit)
            {
                return;
            }

            bool tableIsEmpty = dataGridView.Rows.Count == 0;   // Указывает что таблица не имеет строк.
            IEnumerable<LogixTagHandler> validTags;                      // Список корректных тэгов для добавления.

            // Получаем все тэги распределенные по строками таблиц.
            Dictionary<LogixTagHandler, DataGridViewRow> currentRows = this.TagsByRows;

            // Получаем все тэги которые разрешено добавить.
            validTags = tags.Where(t => t != null && t.Name != null && !currentRows.ContainsKey(t));

            // Проверяем полученную коллекцию.
            if (validTags.Count() == 0)
            {
                return;
            }

            // Для того чтобы добавлять новые строки быстро необходимо как минимум одна строка.
            // В случае пустой таблицы добавляем одну строку чтобы с нее сделать копии.
            if (tableIsEmpty)
            {
                dataGridView.Rows.Add();
            }

            // Делаем копии строк из нулевой строки в конец таблицы.
            int lastIndex = dataGridView.Rows.AddCopies(0, tableIsEmpty ? validTags.Count() - 1 : validTags.Count());

            // Установка значения устройства.
            LogixTask task = null;
            Dictionary<string, LogixTask> tasks = this.TaskCollection.ToDictionary(k => k.Device.Name, v => v);
            if (deviceName != null && tasks.ContainsKey(deviceName))
            {
                task = tasks[deviceName];
            }

            // Вписываем значение тэга и устройства с последней строки вверх..
            foreach (LogixTagHandler tag in validTags.Reverse())
            {
                DataGridViewRow row = dataGridView.Rows[lastIndex];

                // Значение тэга.
                row.Cells[ColumnTag.Index].Value = tag;

                // Значение устройства.
                if (task != null)
                {
                    dataGridView.Rows[lastIndex].Cells[ColumnDevice.Index].SetComboBoxCellValue(deviceName);
                }

                lastIndex--;
            }

            // Вызываем событие.
            Event_TagsWasAdded(validTags.ToList());
        }
        /// <summary>
        /// Добавляет новый тэг в коллекцию.
        /// </summary>
        /// <param name="deviceName">Имя устройства.</param>
        /// <param name="tagName">Имя тэга.</param>
        public void Add(string deviceName, string tagName)
        {
            // Проверяем входные параметры.
            if (tagName == null || this.Mode != ViewMode.Edit)
            {
                return;
            }

            this.Add(deviceName, new LogixTagHandler(tagName.Trim()));
        }
        /// <summary>
        /// Добавляет новые тэги в коллекцию.
        /// </summary>
        /// <param name="deviceName">Имя устройства.</param>
        /// <param name="tagNames">Имена тэгов.</param>
        public void AddRange(string deviceName, IEnumerable<string> tagNames)
        {
            // Проверяем входные параметры.
            if (tagNames == null || tagNames.Count() == 0 || this.Mode != ViewMode.Edit)
            {
                return;
            }

            List<LogixTagHandler> tags = new List<LogixTagHandler>();

            foreach (string tagName in tagNames)
            {
                if (tagName != null && tagName.Trim() != "")
                {
                    tags.Add(new LogixTagHandler(tagName.Trim()));
                }
            }

            this.AddRange(deviceName, tags);
        }
        /// <summary>
        /// Удаляет текущий тэг из колллекции.
        /// </summary>
        /// <param name="tag">Тэг для удаления.</param>
        public void Remove(LogixTagHandler tag)
        {
            if (tag == null || tag.Name == null || this.Mode != ViewMode.Edit)
            {
                return;
            }

            // Получаем все тэги распределенные по строками таблиц.
            Dictionary<LogixTagHandler, DataGridViewRow> tagsByRow = this.TagsByRows;

            if (tagsByRow.ContainsKey(tag))
            {
                // Удаляем строку.
                dataGridView.Rows.Remove(tagsByRow[tag]);

                // Вызываем событие.
                List<LogixTagHandler> removedTags = new List<LogixTagHandler>();
                removedTags.Add(tag);
                Event_TagsWasRemoved(removedTags);
            }
        }
        /// <summary>
        /// Очищает все тэги из коллекции.
        /// </summary>
        public void Clear()
        {
            if (this.Mode != ViewMode.Edit)
            {
                return;
            }

            // Получаем все тэги распределенные по строками таблиц.
            Dictionary<LogixTagHandler, DataGridViewRow> tagsByRow = this.TagsByRows;

            // Очищаем все строки.
            this.dataGridView.Rows.Clear();

            // Вызываем событие.
            Event_TagsWasAdded(tagsByRow.Keys.ToList());
        }
        /// <summary>
        /// Запускает обновление всех элементов управления.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            this.dataGridView.Refresh();
        }
        /// <summary>
        /// Добавляет все тэги в соответствующие им устройства и переводит визуальный компонент в режим просмотра с частичной блокировкой.
        /// </summary>
        public void GoOnline()
        {
            foreach (LogixTask task in this.TasksByTags.Keys)
            {
                task.Begin(this.TasksByTags[task]);
            }

            this.Mode = ViewMode.Monitor;
        }
        /// <summary>
        /// Удаляет соответствующие тэги из устройств и переводит визуальный компонент в режим редактирования.
        /// </summary>
        public void GoOffline()
        {
            foreach (LogixTask task in this.TasksByTags.Keys)
            {
                task.Finish();
            }

            this.Mode = ViewMode.Edit;
        }
        /// <summary>
        /// Переводит визуальный компонент в режим просмотра с полной блокировкой.
        /// </summary>
        public void SetOnlineLock()
        {
            if (this.Mode == ViewMode.Monitor)
                this.Mode = ViewMode.Lock;
        }
        /// <summary>
        /// Переводит визуальный компонент в режим просмотра с частичной блокировкой.
        /// </summary>
        public void SetOnlineEdit()
        {
            if (this.Mode == ViewMode.Lock)
                this.Mode = ViewMode.Monitor;
        }
        /// <summary>
        /// Запрашивает выделенные тэги на запись заданных значений.
        /// </summary>
        public void WriteEnabledValues()
        {
            if (this.Mode == ViewMode.Edit)
            {
                return;
            }

            foreach (LogixTagHandler tagHandler in this.TagsByRows.Keys)
            {
                tagHandler.WriteValue.Reset();

                DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)this.TagsByRows[tagHandler].Cells[ColumnWriteEnable.Index];
                tagHandler.WriteEnable = (cell.Value != null && (bool)cell.Value == true);
                cell.Value = false;
            }

            this.checkBox_CommonWriteEnable.Checked = false;
        }

        /// <summary>
        /// Получает настройки в виде xml объекта.
        /// </summary>
        public XElement GetXSettings()
        {
            XElement xSettings = new XElement("Settings");

            XElement xColumns = new XElement("Columns");
            foreach (DataGridViewColumn column in this.dataGridView.Columns)
            {
                XElement xColumn = new XElement("Column");
                xColumn.Add(new XAttribute("Name", column.Name));
                xColumn.Add(new XAttribute("Width", column.Width.ToString()));
                xColumn.Add(new XAttribute("Visible", column.Visible.ToString().ToLower()));
                xColumns.Add(xColumn);
            }

            xSettings.Add(xColumns);

            return xSettings;
        }
        /// <summary>
        /// Устанавливает настройки из xml объекта.
        /// </summary>
        /// <param name="xSettings">Xml объект Settings.</param>
        /// <returns>Возвращает true при успешном результате.</returns>
        public bool SetXSettings(XElement xSettings)
        {
            // Проверяем входные параметры.
            if (!xSettings.ExistAs("Settings"))
            {
                return false;
            }

            XElement xDevices = xSettings.Element("Columns");
            if (xDevices != null)
            {
                foreach (XElement xColumn in xDevices.Elements("Column"))
                {
                    string columnName = xColumn.Attribute("Name").GetXValue("");
                    string columnWidth = xColumn.Attribute("Width").GetXValue("100");
                    string columnVisible = xColumn.Attribute("Visible").GetXValue("true");

                    // Пытаемся поолучить ссылку на объект колонки по имени.
                    DataGridViewColumn column = this.dataGridView.Columns[columnName];
                    if (column != null)
                    {
                        // Устанавливаем значение свойства Width.
                        int width;
                        if (Int32.TryParse(columnWidth, out width))
                        {
                            column.Width = width;
                        }

                        // Устанавливаем значение свойства Visible.
                        column.Visible = (columnVisible == "true");
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        /* ======================================================================================== */
        #endregion

        #region [ PRIVATE METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Устанавливает визуальные свойства для данного элемента управления в соответствии с выбранным режимом отображения.
        /// </summary>
        private void SetVisualizationMode()
        {
            this.comboBox_CommonRadix.Enabled = (this.Mode != ViewMode.Lock);
            this.textBox_CommonUpdateRate.Enabled = (this.Mode != ViewMode.Lock);

            this.SetDataGridColumnReadOnlyProperty(this.ColumnDevice, this.Mode != ViewMode.Edit);
            this.SetDataGridColumnReadOnlyProperty(this.ColumnTag, this.Mode != ViewMode.Edit);
            this.SetDataGridColumnReadOnlyProperty(this.ColumnDataType, true);
            this.SetDataGridColumnReadOnlyProperty(this.ColumnStatus, true);
            this.SetDataGridColumnReadOnlyProperty(this.ColumnReadRate, this.Mode == ViewMode.Lock);
            this.SetDataGridColumnReadOnlyProperty(this.ColumnActualServerReply, true);
            this.SetDataGridColumnReadOnlyProperty(this.ColumnActualUpdateRate, true);
            this.SetDataGridColumnReadOnlyProperty(this.ColumnRadix, this.Mode == ViewMode.Lock);
            this.SetDataGridColumnReadOnlyProperty(this.ColumnReadValue, true);
            this.SetDataGridColumnReadOnlyProperty(this.ColumnWriteValue, this.Mode == ViewMode.Lock);
            this.SetDataGridColumnReadOnlyProperty(this.ColumnComMethod, this.Mode != ViewMode.Edit);
            this.SetDataGridColumnReadOnlyProperty(this.ColumnTableNumber, true);

            this.dataGridView.AllowUserToAddRows = (this.Mode == ViewMode.Edit);
            this.dataGridView.AllowUserToDeleteRows = (this.Mode == ViewMode.Edit);
        }
        /// <summary>
        /// Устанавливает свойства возможности чтения/редактирования колонки DataGridViewColumn.
        /// </summary>
        /// <param name="column">Текушая колонка DataGridView.</param>
        /// <param name="readOnly">Значнение свойства означающее только чтенеи при равенстве True.</param>
        private void SetDataGridColumnReadOnlyProperty(DataGridViewColumn column, bool readOnly)
        {
            column.ReadOnly = readOnly;
            column.DefaultCellStyle.BackColor = readOnly ? lockBackColor : editBackColor;
        }
        /// <summary>
        /// Обновляет свойства и состояния визуальных компонентов на верхней вспомогательной панели.
        /// </summary>
        private void UpdateUpperPanelControls()
        {
            List<Rectangle> columnHeaderBounds = dataGridView.GetDisplayedColumnHeaderBounds();

            int xOffset = -dataGridView.HorizontalScrollingOffset;
            int borderDistance = 1;

            textBox_CommonUpdateRate.Location = new Point(
                columnHeaderBounds[this.ColumnReadRate.Index].X + xOffset + borderDistance,
                (splitContainer_Grid.Panel1.Height - textBox_CommonUpdateRate.Height) / 2);
            textBox_CommonUpdateRate.Width = columnHeaderBounds[this.ColumnReadRate.Index].Width - borderDistance * 2;
            textBox_CommonUpdateRate.Visible = this.ColumnReadRate.Visible;

            comboBox_CommonRadix.Location = new Point(
                columnHeaderBounds[this.ColumnRadix.Index].X + xOffset + borderDistance,
                (splitContainer_Grid.Panel1.Height - comboBox_CommonRadix.Height) / 2);
            comboBox_CommonRadix.Width = columnHeaderBounds[this.ColumnRadix.Index].Width - borderDistance * 2;
            comboBox_CommonRadix.Visible = this.ColumnRadix.Visible;

            checkBox_CommonWriteEnable.Location = new Point(
                columnHeaderBounds[this.ColumnWriteEnable.Index].X + xOffset + borderDistance + columnHeaderBounds[this.ColumnWriteEnable.Index].Width / 2 - checkBox_CommonWriteEnable.Width / 2,
                (splitContainer_Grid.Panel1.Height - checkBox_CommonWriteEnable.Height) / 2);
            checkBox_CommonWriteEnable.Visible = this.ColumnWriteEnable.Visible;
        }
        /// <summary>
        /// Обновляет все визуальные элементы таблицы.
        /// </summary>
        private void UpdateVisualElements()
        {
            // Обновляем все строки таблицы.
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row == null || row.IsNewRow)
                {
                    return;
                }

                if (row.GetType() == typeof(DataGridViewRow))
                {
                    #region [ DataGridViewRow ]
                    /* ======================================================================================== */
                    object deviceObj = row.Cells[this.ColumnDevice.Index].Value;
                    object tagObj = row.Cells[this.ColumnTag.Index].Value;

                    LogixTagHandler tag = null;
                    LogixTask task = null;

                    if (tagObj != null && (tagObj is LogixTagHandler))
                    {
                        tag = (LogixTagHandler)tagObj;
                    }

                    if (deviceObj != null && (deviceObj is LogixTask))
                    {
                        task = (LogixTask)deviceObj;
                    }

                    #region [ Column "Data Type" ]
                    /* ======================================================================================== */
                    string dataTypeText = "...";

                    if (tag != null)
                    {
                        dataTypeText = tag.Type.ToString();
                    }

                    row.Cells[this.ColumnDataType.Index].Value = dataTypeText;
                    /* ======================================================================================== */
                    #endregion

                    #region [ Column "Status" ]
                    /* ======================================================================================== */
                    Color statusColor = this.ColumnStatus.DefaultCellStyle.BackColor;
                    string statusText = "...";

                    if (tag != null && task != null)
                    {
                        if (task.ContainsTagObject(tag))
                        {
                            if (task.Device.IsConnected)
                            {
                                if (tag.ReadValue.Report.IsSuccessful == true)
                                {
                                    if (task.ServerState == ServerState.Run)
                                    {
                                        statusText = "Online";
                                        statusColor = Color.Chartreuse;
                                    }
                                    else
                                    {
                                        statusText = "Init";
                                        statusColor = Color.Yellow;
                                    }
                                }
                                else if (tag.ReadValue.Report.IsSuccessful == false)
                                {
                                    statusText = "Error";
                                    statusColor = Color.Tomato;
                                }
                                else
                                {
                                    statusText = "Waiting";
                                    statusColor = Color.YellowGreen;
                                }
                            }
                            else
                            {
                                statusText = "Offline";
                            }
                        }
                        else
                        {
                            statusText = "Edit";
                        }
                    }

                    // Колонка ColumnStatus
                    row.Cells[this.ColumnStatus.Index].Value = statusText;
                    row.Cells[this.ColumnStatus.Index].Style.BackColor = statusColor;
                    /* ======================================================================================== */
                    #endregion

                    #region [ Column "Requested Update Rate" ]
                    /* ======================================================================================== */
                    string requestedUpdateRateText = "...";

                    if (tag != null)
                    {
                        requestedUpdateRateText = tag.ReadUpdateRate.ToString();
                    }

                    row.Cells[this.ColumnReadRate.Index].Value = requestedUpdateRateText;
                    /* ======================================================================================== */
                    #endregion

                    #region [ Column "Actual Update Rate" ]
                    /* ======================================================================================== */
                    string actualUpdateRateText = "...";

                    if (tag != null)
                    {
                        if (tag.ReadValue.Report.ActualUpdateRate != null)
                        {
                            actualUpdateRateText = tag.ReadValue.Report.ActualUpdateRate.ToString();
                        }
                    }

                    row.Cells[this.ColumnActualUpdateRate.Index].Value = actualUpdateRateText;
                    /* ======================================================================================== */
                    #endregion

                    #region [ Column "Actual Server Reply" ]
                    /* ======================================================================================== */
                    string replyServerText = "...";

                    if (tag != null)
                    {
                        if (tag.ReadValue.Report.ServerReplyTime != null)
                        {
                            replyServerText = tag.ReadValue.Report.ServerReplyTime.ToString();
                        }
                    }

                    row.Cells[this.ColumnActualServerReply.Index].Value = replyServerText;
                    /* ======================================================================================== */
                    #endregion

                    #region [ Column "Radix" ]
                    /* ======================================================================================== */
                    TagValueRadix radix = TagValueRadix.Decimal;

                    if (tag != null)
                    {
                        radix = tag.Radix;
                    }

                    row.Cells[this.ColumnRadix.Index].SetComboBoxCellIndex((int)radix);
                    /* ======================================================================================== */
                    #endregion

                    #region [ Column "Read Value" ]
                    /* ======================================================================================== */
                    string valueText = "";

                    if (tag != null)
                    {
                        string text = tag.GetReadedValueText();
                        if (text != null) valueText = text;
                    }

                    row.Cells[this.ColumnReadValue.Index].Value = valueText;
                    /* ======================================================================================== */
                    #endregion

                    #region [ Column "Write Value / Write Enable" ]
                    /* ======================================================================================== */
                    string writeValueText = "";

                    Color currentCellBackColor = row.Cells[ColumnWriteValue.Index].Style.BackColor;
                    Color requiredCellBackColor = currentCellBackColor;

                    // Color.LightCyan;
                    // Color.PaleGreen;
                    // Color.LemonChiffon;
                    // Color.MistyRose;

                    if (tag != null)
                    {
                        if (!tag.Type.ArrayDimension.HasValue)
                        {
                            writeValueText = tag.GetWritedValueText();
                            if (writeValueText == null) writeValueText = "";
                        }

                        if (((tag.Type.Family == TagDataTypeFamily.AtomicBool ||
                            tag.Type.Family == TagDataTypeFamily.AtomicFloat ||
                            tag.Type.Family == TagDataTypeFamily.AtomicInteger)
                            && !tag.Type.ArrayDimension.HasValue)
                            || (tag.Type.Family == TagDataTypeFamily.AtomicBoolArray
                            && tag.Type.BitArrayDefinition != null))
                        {
                            if (tag.WriteValue.Report.IsSuccessful == true)
                            {
                                requiredCellBackColor = Color.PaleGreen;
                            }
                            else if (tag.WriteValue.Report.IsSuccessful == false)
                            {
                                requiredCellBackColor = Color.MistyRose;
                            }
                            else
                            {
                                requiredCellBackColor = editBackColor;
                            }

                            row.Cells[this.ColumnWriteValue.Index].ReadOnly = false;
                            row.Cells[this.ColumnWriteEnable.Index].ReadOnly = false;
                            ((DataGridViewDisableCheckBoxCell)row.Cells[this.ColumnWriteEnable.Index]).Hide = false;
                        }
                        else
                        {
                            requiredCellBackColor = lockBackColor;
                            row.Cells[this.ColumnWriteValue.Index].ReadOnly = true;
                            row.Cells[this.ColumnWriteEnable.Index].ReadOnly = true;
                            ((DataGridViewDisableCheckBoxCell)row.Cells[this.ColumnWriteEnable.Index]).Hide = true;

                            object checkBoxCellValue = row.Cells[this.ColumnWriteEnable.Index].Value;
                            if (checkBoxCellValue != null)
                                row.Cells[this.ColumnWriteEnable.Index].Value = false;
                        }
                    }

                    if (currentCellBackColor.ToArgb() != requiredCellBackColor.ToArgb())
                    {
                        row.Cells[ColumnWriteValue.Index].Style.BackColor = requiredCellBackColor;
                        row.Cells[ColumnWriteEnable.Index].Style.BackColor = requiredCellBackColor;
                    }

                    row.Cells[this.ColumnWriteValue.Index].Value = writeValueText;
                    /* ======================================================================================== */
                    #endregion

                    #region [ Column "Com Method" ]
                    /* ======================================================================================== */
                    TagReadMethod method = TagReadMethod.Simple;

                    if (tag != null)
                    {
                        method = tag.ReadMethod;
                    }

                    row.Cells[this.ColumnComMethod.Index].SetComboBoxCellIndex((int)method);
                    /* ======================================================================================== */
                    #endregion

                    #region [ Column "Table Number" ]
                    /* ======================================================================================== */
                    string value = "";
                    if (tag != null && tag.OwnerTableItem != null)
                    {
                        if (tag.OwnerTableItem.ParrentTable != null)
                        {
                            string tableId = tag.OwnerTableItem.ParrentTable.Instance.ToString();
                            string itemId = tag.OwnerTableItem.ID.ToString();
                            value = tableId + "::" + itemId;
                        }
                    }

                    row.Cells[this.ColumnTableNumber.Index].Value = value;
                    /* ======================================================================================== */
                    #endregion

                    /* ======================================================================================== */
                    #endregion
                }
            }
        }
        /* ======================================================================================== */
        #endregion
    }
}
