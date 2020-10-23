using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Windows.Threading;
using LogixTool.LocalDatabase;
using LogixTool.EthernetIP;
using LogixTool.Controls;
using LogixTool.EthernetIP.AllenBradley;
using LogixTool.EthernetIP.AllenBradley.Models.Events;
using LogixTool.Common;

namespace LogixTool
{
    public partial class FormMain : Form
    {
        /// <summary>
        /// 
        /// </summary>
        private bool OnlineMode { get; set; }

        #region [ FIELDS ]
        /* ================================================================================================== */
        private TagRecorder recorder;
        private DispatcherTimer dt;
        private Storage storage;
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public FormMain()
        {
            InitializeComponent();

            // 1. Device Browser Control.
            this.deviceBrowserControl.Message += Message;
            this.deviceBrowserControl.TaskCollectionWasChanged+=deviceBrowserControl_DeviceCollectionWasChanged;
            this.deviceBrowserControl.DevicePropertyWasChanged += deviceBrowserControl_DevicePropertyWasChanged;
            this.deviceBrowserControl.TagsValueWasChanged += deviceBrowserControl_TagsValueWasChanged;
            
            // 2. Tag Browser Control.
            this.tagBrowserControl.TaskCollection = deviceBrowserControl.EthernetDeviceNodes.Select(t=>t.Task).ToList();
            
            // 3. Storage of data.
            this.storage = new Storage();
            this.storage.Message += Message;

            // Загружаем устройства.
            StorageItemInfo storageItemInfo;
            if (this.storage.Get(StoreOwner.AppRegistrator, StoreType.EipDevices, null, out storageItemInfo))
            {
                this.deviceBrowserControl.SetXSettings(storageItemInfo.XContent.Element("Devices"));
            }
            // Загружаем настройки браузера тэгов.
            StorageItemInfo browserStorageItem;
            this.storage.Get(StoreOwner.AppRegistrator, StoreType.Settings, "tagbrowser", out browserStorageItem);
            if (browserStorageItem.IsSuccessful == true && browserStorageItem.XContent!=null)
            {
                this.tagBrowserControl.SetXSettings(browserStorageItem.XContent.Element("Settings"));
            }
            // Загружаем настройки браузера сообщений.
            StorageItemInfo messageStorageItem;
            this.storage.Get(StoreOwner.AppRegistrator, StoreType.Settings, "message_control", out messageStorageItem);
            if (messageStorageItem.IsSuccessful == true && messageStorageItem.XContent != null)
            {
                this.messageControl.SetXSettings(messageStorageItem.XContent.Element("MessageControl"));
            }

            // 4. Recorder.
            this.recorder = new TagRecorder();
            this.recorder.Message += Message;

            StorageItemInfo recorderStorageItem;
            this.storage.Get(StoreOwner.AppRegistrator, StoreType.Settings, "recorder", out recorderStorageItem);

            if (recorderStorageItem.IsSuccessful == true && recorderStorageItem.XContent != null)
            {
                this.recorder.SetXSettings(recorderStorageItem.XContent.Element("Settings"));
            }

            // 5. Инициализация переменных.
            this.OnlineMode = false;

            this.dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds(500);
            dt.Tick += dt_Tick;
            dt.IsEnabled = true;
        }

        #region [ EVENT SUBSCRIPTIONS ]
        /* ================================================================================================== */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dt_Tick(object sender, EventArgs e)
        {
            Color recordingStatusColor;
            string recordingStatusText = "Logger: ";
            string fileSizeText = "";

            if (this.recorder.EnableRunning)
            {
                if (recorder.WritingRunned)
                {
                    recordingStatusText += "Recording";
                    recordingStatusColor = Color.Chartreuse;
                    fileSizeText = GetFileSizeText(recorder.CurrentFileSize);
                }
                else
                {
                    recordingStatusText += "Waiting";
                    recordingStatusColor = Color.Yellow;
                }
            }
            else
            {
                recordingStatusText += "Off";
                recordingStatusColor = Color.LightGray;
            }

            this.recorderStatusToolStripStatusLabel.Text = recordingStatusText;
            this.recorderStatusToolStripStatusLabel.BackColor = recordingStatusColor;

            // Присваиваем значение для отображения элементов в строке состояния.
            this.fileSizeToolStripStatusLabel.Text = "Size: " + fileSizeText;
            this.recordCounterToolStripStatusLabel.Text = "Lines: " + this.recorder.RecordCounter.ToString();
            this.fileNameToolStripStatusLabel.Text = "File: " + this.recorder.FullFileName;
            
            // Отображение элементов в строке состояния.
            this.fileSizeToolStripStatusLabel.Visible = this.recorder.EnableRunning && recorder.WritingRunned;
            this.recordCounterToolStripStatusLabel.Visible = this.recorder.EnableRunning && recorder.WritingRunned;
            this.fileNameToolStripStatusLabel.Visible = this.recorder.EnableRunning && recorder.WritingRunned;

            // Устанавливаем блокировки кнопок.
            this.toolStripButton_GoOnline.Enabled = !this.OnlineMode;
            this.toolStripButton_GoOffline.Enabled = this.OnlineMode;
            this.toolStripButton_RunRecording.Enabled = !this.recorder.EnableRunning && this.OnlineMode;
            this.toolStripButton_StopRecording.Enabled = this.recorder.EnableRunning;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Message(object sender, MessageEventArgs e)
        {
            this.messageControl.AddMessage(e);
        }

        #region [ 0. Form ]
        /* ================================================================================================== */
        /// <summary>
        /// Подписка на событие : Form : Форма в процессе закрытия.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormRegistrator_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.recorder.EnableRunning || this.recorder.WritingRunned)
            {
                MessageBox.Show("Recording process is active.\r\nPlease stop Record mode of tags.", "Registrator", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }
            else if (CheckTagsForOnline())
            {
                MessageBox.Show("Some tags have Online state.\r\nPlease stop Online mode.", "Registrator", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("Do you want to close application?", "Registrator", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Сохраняет настройки браузера тэгов.
            this.storage.Put(StoreOwner.AppRegistrator, StoreType.Settings, "tagbrowser", tagBrowserControl.GetXSettings());
            this.storage.Put(StoreOwner.AppRegistrator, StoreType.Settings, "recorder", recorder.GetXSettings());
            this.storage.Put(StoreOwner.AppRegistrator, StoreType.Settings, "message_control", messageControl.GetXSettings());
        }
        /// <summary>
        /// Подписка на событие : Form : Форма закрыта.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormRegistrator_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
        /* ================================================================================================== */
        #endregion

        #region [ 1. Device Browser ]
        /* ================================================================================================== */
        /// <summary>
        /// Подписка на событие : DeviceBrowserControl : Коллекция устройств была изменена.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deviceBrowserControl_DeviceCollectionWasChanged(object sender, EventArgs e)
        {
            this.tagBrowserControl.TaskCollection = deviceBrowserControl.EthernetDeviceNodes.Select(t=>t.Task).ToList();
        }
        /// <summary>
        /// Подписка на событие : DeviceBrowserControl : Свойства одного из устройств было изменено.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deviceBrowserControl_DevicePropertyWasChanged(object sender, DeviceEventArgs e)
        {
            this.tagBrowserControl.Refresh();
        }
        /// <summary>
        /// Подписка на событие : DeviceBrowserControl : Были успешно получены данные одного из тэгов, где значение тэгов изменено.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deviceBrowserControl_TagsValueWasChanged(object sender, TagsEventArgs e)
        {
            this.recorder.RequestForRecording(e.Tags.Where(t => t.ReadValue.Report.ValueChanged == true).ToList());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deviceBrowserControl_SaveProjectClick(object sender, EventArgs e)
        {
            this.storage.Put(StoreOwner.AppRegistrator, StoreType.EipDevices, null, deviceBrowserControl.GetXElement());
        }
        /* ================================================================================================== */
        #endregion

        #region [ 2. Tool Strip Button ]
        /* ================================================================================================== */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton_OpenTagList_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }
        /// <summary>
        /// Подписка на событие : OpenFileDialog : Нажат кнопка меню "OK".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
            {
                return;
            }

            List<string[]> items;
            if (CsvFile.Open(openFileDialog.FileName, '\t', out items))
            {
                tagBrowserControl.Clear();

                #region [ PARSE ROW ]

                foreach (string[] item in items)
                {
                    string deviceName = "";
                    string tagName = "";
                    string fragmentLength = null;
                    string readUpdateRate = null;
                    string radix = null;
                    string writeValue = null;

                    switch (item.Length)
                    {
                        case 1:
                            tagName = item[0];
                            break;

                        case 2:
                            deviceName = item[0];
                            tagName = item[1];
                            break;

                        case 3:
                            deviceName = item[0];
                            tagName = item[1];
                            fragmentLength = item[2];
                            break;

                        case 4:
                            deviceName = item[0];
                            tagName = item[1];
                            fragmentLength = item[2];
                            readUpdateRate = item[3];
                            break;

                        case 6:
                            deviceName = item[0];
                            tagName = item[1];
                            fragmentLength = item[2];
                            readUpdateRate = item[3];
                            radix = item[3];
                            writeValue = item[4];
                            break;
                    }


                    TagHandler tag = new TagHandler(tagName);

                    if (fragmentLength != null)
                    {
                        UInt16 value;
                        if (UInt16.TryParse(fragmentLength, out value))
                        {
                            tag.Type.ArrayDimension.Value = value;
                        }
                        else
                        {
                            // TODO Message.
                        }
                    }

                    if (readUpdateRate != null)
                    {
                        UInt16 value;
                        if (UInt16.TryParse(readUpdateRate, out value))
                        {
                            tag.ReadUpdateRate = value;
                        }
                        else
                        {
                            // TODO Message.
                        }
                    }

                    if (radix != null && writeValue != null)
                    {
                        TagValueRadix tagValueRadix;
                        if (Enum.TryParse<TagValueRadix>(radix, true, out tagValueRadix))
                        {
                            //if (!tag.WriteValueControl.SetValueText(0, tagValueRadix, writeValue))
                            //{
                            //    // TODO Message.
                            //}
                        }
                        else
                        {
                            // TODO Message.
                        }
                    }

                    tagBrowserControl.Add(deviceName, tag);
                }
                #endregion
            }
            else
            {
                MessageBox.Show("Error! Can't open file!", "Registrator", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton_SaveTagList_Click(object sender, EventArgs e)
        {
            saveFileDialog.ShowDialog();
        }
        /// <summary>
        /// Подписка на событие : SaveFileDialog : Нажат кнопка меню "OK".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
            {
                return;
            }

            List<string[]> items = new List<string[]> ();

            foreach (KeyValuePair<TagTask, List<TagHandler>> pair in tagBrowserControl.TasksByTags)
            {
                TagTask key = pair.Key;
                foreach (TagHandler tag in tagBrowserControl.TasksByTags[key])
                {
                    items.Add(new string[] { key.Device.Name, tag.Name });
                }
            }

            if (!CsvFile.Save(saveFileDialog.FileName,'\t',items))
            {
                MessageBox.Show("Error! Can't save file!", "Registrator", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton_RunRecording_Click(object sender, EventArgs e)
        {
            RunLogger();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton_StopRecording_Click(object sender, EventArgs e)
        {
            StopLogger();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton_GoOnline_Click(object sender, EventArgs e)
        {
            GoOnline();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton_GoOffline_Click(object sender, EventArgs e)
        {
            GoOffline();
            StopLogger();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton_ShowDeviceBrowser_Click(object sender, EventArgs e)
        {
            if (!splitContainer2.Panel1Collapsed)
            {
                splitContainer2.Panel1Collapsed = true;
                splitContainer2.Panel1.Hide();
            }
            else
            {
                splitContainer2.Panel1Collapsed = false;
                splitContainer2.Panel1.Show();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton_ShowEventLogs_Click(object sender, EventArgs e)
        {
            if (!splitContainer3.Panel2Collapsed)
            {
                splitContainer3.Panel2Collapsed = true;
                splitContainer3.Panel2.Hide();
            }
            else
            {
                splitContainer3.Panel2Collapsed = false;
                splitContainer3.Panel2.Show();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton_About_Click(object sender, EventArgs e)
        {
            FormAbout form = new FormAbout();
            form.Show();
        }
        /* ================================================================================================== */
        #endregion

        #region [ 3. Tool Strip Status Label ]
        /* ================================================================================================== */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileNameToolStripStatusLabel_Click(object sender, EventArgs e)
        {
            try
            {             
                System.Diagnostics.Process.Start("explorer.exe", recorder.FileLocation);
            }
            catch
            {
                MessageBox.Show("Error of opening explorer.exe", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /* ================================================================================================== */
        #endregion

        /* ================================================================================================== */
        #endregion

        #region [ PRIVATE METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Переводит состояние приложения в режим подключения к удаленным устройствам в соответствиии с выбранными тэгами.
        /// </summary>
        private void GoOnline()
        {
            tagBrowserControl.GoOnline();
            this.OnlineMode = true;
        }
        /// <summary>
        /// Переводит состояние приложения в режим отключения от всех имеющихся устройств.
        /// </summary>
        private void GoOffline()
        {
            tagBrowserControl.GoOffline();
            this.OnlineMode = false;
        }
        /// <summary>
        /// Запускает логирование данных в файл.
        /// </summary>
        private void RunLogger()
        {
            if (recorder.EnableRunning || recorder.WritingRunned)
            {
                return;
            }

            recorder.RecordedTags.Clear();
            recorder.RecordedTags.AddRange(this.tagBrowserControl.TagsByRows.Keys);

            FormRecordSettings formRecordSettings = new FormRecordSettings(recorder);
            formRecordSettings.ShowDialog();

            if (formRecordSettings.IsApplied)
            {
                recorder.Run();
                tagBrowserControl.SetOnlineLock();
            }
        }
        /// <summary>
        /// Останавливает логирование данных в файл.
        /// </summary>
        private void StopLogger()
        {
            recorder.Stop();
            tagBrowserControl.SetOnlineEdit();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool CheckTagsForOnline()
        {
            // Проверяем что в данный момент ни один из тэгов не обрабатывается устройствами.

            Dictionary<TagTask, List<TagHandler>> tasksByTags = this.tagBrowserControl.TasksByTags;
            foreach (KeyValuePair<TagTask, List<TagHandler>> deviceByTags in tasksByTags/*.Where(d=>d.Key.IsConnected)*/)
            {
                foreach (TagHandler tag in deviceByTags.Value)
                {
                    if (deviceByTags.Key.ContainsTagObject(tag))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Возвращает текст со значением размера памяти представленный максимальной единицей измерения больше нуля.
        /// </summary>
        /// <param name="byteSize">Размер памяти в байтах.</param>
        /// <returns></returns>
        private string GetFileSizeText(long byteSize)
        {
            if (byteSize >= 0 && byteSize < 1024) return byteSize + " Bytes";
            else if (byteSize >= 1024 && byteSize < 1048576)return (byteSize / 1024) + " KB";     
            else if (byteSize >= 1048576 && byteSize < 1073741824) return (byteSize / 1048576) + " MB";
            else return (byteSize / 1073741824) + " GB";
        }
        /* ================================================================================================== */
        #endregion
    }
}
