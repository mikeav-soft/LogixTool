using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EIP.AllenBradley;
using LogixTool.Common;

namespace LogixTool
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FormRecordSettings : Form
    {
        /// <summary>
        /// Возвращает значение true в случае если операция данного диалогового окна была подтверждена.
        /// </summary>
        public bool IsApplied { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        private TagRecorder CurrentRecorder { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recorder"></param>
        public FormRecordSettings(TagRecorder recorder)
        {
            InitializeComponent();

            this.IsApplied = false;

            // Устанавливаем коллекции ComboBox из перечислений.
            this.comboBox_PeriodRecordingUnit.SetCollectionFromEnumeration<RecordingPeriodUnits>();
            this.comboBox_SeparationFilesByPeriod.SetCollectionFromEnumeration<SeparationFilePeriodBy>();

            // Присваиваем текущий редактируемый объект рекордера данных.
            this.CurrentRecorder = recorder;

            // Устанавливаем значения элементов управления.

            // Место расположения файла.
            this.textBox_FileDirectory.Text = this.CurrentRecorder.FileLocation;
            // Префикс названия файла.
            this.textBox_FileNamePrefix.Text = this.CurrentRecorder.FilePrefix;
            // Значения RadioButton типа записи.
            this.radioButton_NormalRecording.Checked = this.CurrentRecorder.RecordingType == RecordingEventType.All;
            this.radioButton_PeriodRecording.Checked = this.CurrentRecorder.RecordingType == RecordingEventType.ByPeriod;
            this.radioButton_SelectionRecording.Checked = this.CurrentRecorder.RecordingType == RecordingEventType.BySelectedTags;
            // Значение временного промежутка периодичной записи.
            this.numericUpDown_PeriodRecordingValue.Value = this.CurrentRecorder.RecordingPeriodValue;
            // Единица измерения временного промежутка периодичной записи.
            this.comboBox_PeriodRecordingUnit.SetItemFromText(Enum.GetName(typeof(RecordingPeriodUnits), this.CurrentRecorder.RecordingPeriodUnit));
            // Максимальный размер файла при котором создается новая часть.
            this.numericUpDown_SeparationFilesBySize.Value = this.CurrentRecorder.SeparationFileSize;
            // Временной промежуток деления файла на части.
            this.comboBox_SeparationFilesByPeriod.SetItemFromText(Enum.GetName(typeof(SeparationFilePeriodBy), this.CurrentRecorder.SeparationPeriod));
            // Формат временной метки одной записи.
            this.radioButton_TimeStampAsDateTime.Checked = !this.CurrentRecorder.TickTimeFormat;
            this.radioButton_TimeStampAsTicks.Checked = this.CurrentRecorder.TickTimeFormat;
            // Добавляем в список полный список тэгов и отмечаем выбранные.
            Dictionary<LogixTagHandler, LogixTagHandler> selectedTags = recorder.SelectedTags.ToDictionary(k => k, v => v);   
            this.checkedListBox_SelectionRecording.Items.Clear();
            for (int ix = 0; ix< recorder.RecordedTags.Count; ix++)
            {
                LogixTagHandler tag = recorder.RecordedTags[ix];
                if (tag.OwnerTask != null)
                {
                    string deviceName = tag.OwnerTask.ToString();
                    this.checkedListBox_SelectionRecording.Items.Add(new CheckBoxItem<LogixTagHandler>("[" + deviceName + "]" + tag.Name, tag));
                    if (selectedTags.ContainsKey(tag))
                    {
                        this.checkedListBox_SelectionRecording.SetItemChecked(this.checkedListBox_SelectionRecording.Items.Count - 1, true);
                    }
                }
            }


            DefineControlEnable();
        }


        #region [ EVENT SUBSCRIPTIONS - FORMS ]
        /* ======================================================================================== */
        /// <summary>
        /// Подписка на событие : Button : Нажата кнопка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_ShowDirectoryDialog_Click(object sender, EventArgs e)
        {
            this.folderBrowserDialog.SelectedPath = this.CurrentRecorder.FileLocation;
            this.folderBrowserDialog.ShowDialog();
            this.textBox_FileDirectory.Text = this.folderBrowserDialog.SelectedPath;
        }
        /// <summary>
        /// Подписка на событие : Button : Нажата кнопка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Apply_Click(object sender, EventArgs e)
        {
            // Место расположения файла.
            this.CurrentRecorder.FileLocation = textBox_FileDirectory.Text;
            // Префикс названия файла.
            this.CurrentRecorder.FilePrefix = textBox_FileNamePrefix.Text;

            // Значения RadioButton типа записи.
            if (this.radioButton_NormalRecording.Checked)
            {
                this.CurrentRecorder.RecordingType = RecordingEventType.All;
            }
            else if (this.radioButton_PeriodRecording.Checked)
            {
                this.CurrentRecorder.RecordingType = RecordingEventType.ByPeriod;
            }
            else if (this.radioButton_SelectionRecording.Checked)
            {
                this.CurrentRecorder.RecordingType = RecordingEventType.BySelectedTags;
            }

            // Единица измерения временного промежутка периодичной записи.
            RecordingPeriodUnits recordingPeriodUnits;
            if (Enum.TryParse(this.comboBox_PeriodRecordingUnit.SelectedItem.ToString(), out recordingPeriodUnits))
            {
                this.CurrentRecorder.RecordingPeriodUnit = recordingPeriodUnits;
            }
            // Значение временного промежутка периодичной записи.
            this.CurrentRecorder.RecordingPeriodValue = (int)this.numericUpDown_PeriodRecordingValue.Value;
            // Максимальный размер файла при котором создается новая часть.
            this.CurrentRecorder.SeparationFileSize = (int)this.numericUpDown_SeparationFilesBySize.Value;
            // Временной промежуток деления файла на части.
            SeparationFilePeriodBy separationFilePeriod;
            if (Enum.TryParse(this.comboBox_SeparationFilesByPeriod.SelectedItem.ToString(), out separationFilePeriod))
            {
                this.CurrentRecorder.SeparationPeriod = separationFilePeriod;
            }
            // Формат временной метки одной записи.
            this.CurrentRecorder.TickTimeFormat = this.radioButton_TimeStampAsTicks.Checked;

            // Устанавливаем выделенные тэги.
            List<LogixTagHandler> selectedTags = new List<LogixTagHandler> ();
            foreach (object obj in this.checkedListBox_SelectionRecording.CheckedItems)
            {
                if (obj is CheckBoxItem<LogixTagHandler>)
                {
                    CheckBoxItem<LogixTagHandler> checkBoxItem = (CheckBoxItem<LogixTagHandler>)obj;
                    selectedTags.Add(checkBoxItem.Value);
                }
            }

            this.CurrentRecorder.SelectedTags = selectedTags;
            
            this.IsApplied = true;
            this.Close();
        }
        /// <summary>
        /// Подписка на событие : Button : Нажата кнопка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.IsApplied = false;
            this.Close();
        }
        /// <summary>
        /// Подписка на событие : RadioButton : Измнено состояние.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton_NormalRecording_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_NormalRecording.Checked)
            {
                DefineControlEnable();
            }
        }
        /// <summary>
        /// Подписка на событие : RadioButton : Измнено состояние.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton_PeriodRecording_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_PeriodRecording.Checked)
            {
                DefineControlEnable();
            }
        }
        /// <summary>
        /// Подписка на событие : RadioButton : Измнено состояние.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton_SelectionRecording_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_SelectionRecording.Checked)
            {
                DefineControlEnable();
            }
        }
        /* ======================================================================================== */
        #endregion

        /// <summary>
        /// Устанавливает блокировки визуальных компонентов на форме в зависимости от условий.
        /// </summary>
        private void DefineControlEnable()
        {
            this.numericUpDown_PeriodRecordingValue.Enabled = radioButton_PeriodRecording.Checked;
            this.comboBox_PeriodRecordingUnit.Enabled = radioButton_PeriodRecording.Checked;
            this.checkedListBox_SelectionRecording.Enabled = radioButton_SelectionRecording.Checked;
        }
    }
}
