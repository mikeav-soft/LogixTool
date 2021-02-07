using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using LogixTool.Common;
using LogixTool.Common.Extension;
using EIP.AllenBradley;
using EIP.AllenBradley.Models.Events;

namespace LogixTool.Controls
{
    /// <summary>
    /// Пользовательский компонент управления подключением с устройствами Allen Breadley через сеть Ethernet IP.
    /// </summary>
    public partial class DeviceBrowserControl : UserControl
    {
        private const string MESSAGE_HEADER = "Device Browser";

        #region [ PROPERTIES ]
        /* ======================================================================================== */
        /// <summary>
        /// Текущее выделенное пользователем устройство.
        /// </summary>
        public TreeNode_EthernetDevice SelectedEthernetDeviceNode
        {
            get
            {
                TreeNode_EthernetDevice result = null;

                FormsExtensions.InvokeControl<TreeView>(treeView, l =>
                    {
                        if (l.SelectedNode != null && l.SelectedNode is TreeNode_EthernetDevice)
                        {
                            result = (TreeNode_EthernetDevice)l.SelectedNode;
                        }
                    });

                return result;
            }
        }
        /// <summary>
        /// Получает коллекцию с устройствами Allen Breadley.
        /// </summary>
        public List<TreeNode_EthernetDevice> EthernetDeviceNodes
        {
            get
            {
                List<TreeNode_EthernetDevice> result = new List<TreeNode_EthernetDevice>();

                foreach (TreeNode tn in this.treeView.Nodes)
                {
                    if (tn != null && tn is TreeNode_EthernetDevice)
                    {
                        TreeNode_EthernetDevice ethernetDeviceNode = (TreeNode_EthernetDevice)tn;
                        result.Add(ethernetDeviceNode);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Получает коллекцию с устройствами Allen Breadley сгрупированные по именам.
        /// </summary>
        private Dictionary<string, TreeNode_EthernetDevice> CLXDevicesByName
        {
            get
            {
                Dictionary<string, TreeNode_EthernetDevice> items = new Dictionary<string, TreeNode_EthernetDevice>();

                foreach (object tn in this.treeView.Nodes)
                {
                    if (tn != null && tn is TreeNode_EthernetDevice)
                    {
                        TreeNode_EthernetDevice tnDevice = (TreeNode_EthernetDevice)tn;
                        string key = tnDevice.Device.Name;

                        if (!items.ContainsKey(key))
                        {
                            items.Add(key, tnDevice);
                        }
                    }
                }

                return items;
            }
        }
        /// <summary>
        /// Получает коллекцию с устройствами Allen Breadley сгрупированные по IP адресам и слотам контроллера в виде "NNN.NNN.NNN.NNN/S".
        /// </summary>
        private Dictionary<string, TreeNode_EthernetDevice> CLXDevicesByAddress
        {
            get
            {
                Dictionary<string, TreeNode_EthernetDevice> items = new Dictionary<string, TreeNode_EthernetDevice>();

                foreach (object tn in this.treeView.Nodes)
                {
                    if (tn != null && tn is TreeNode_EthernetDevice)
                    {
                        TreeNode_EthernetDevice tnDevice = (TreeNode_EthernetDevice)tn;
                        string key = tnDevice.Device.Address + "/" + tnDevice.Device.ProcessorSlot.ToString();

                        if (!items.ContainsKey(key))
                        {
                            items.Add(key, tnDevice);
                        }
                    }
                }

                return items;
            }
        }
        /* ======================================================================================== */
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public DeviceBrowserControl()
        {
            InitializeComponent();

            this.RefreshStateOfHeadControl();
            this.RefreshStateOfPropertyControl();
        }

        #region [ EVENTS ]
        /* ======================================================================================== */
        /* 1. События */

        /// <summary>
        /// Возникает при нажатии клавиши пользователем "Сохранить настройки проекта".
        /// </summary>
        [Category("Специальные")]
        public event EventHandler SaveProjectClick;
        /// <summary>
        /// Возникает при обработке чтении одного из тэгов из контроллера, данные тэга которого изменились с момента последнего чтения.
        /// </summary>
        [Category("Специальные")]
        public event TagsEventHandler TagsValueWasChanged;
        /// <summary>
        /// Возникает при обработке чтения одного из тэгов устройства.
        /// </summary>
        [Category("Специальные")]
        public event TagsEventHandler TagsValueWasReaded;
        /// <summary>
        /// Возникает при обработке записи одного из тэгов устройства.
        /// </summary>
        [Category("Специальные")]
        public event TagsEventHandler TagsValueWasWrited;
        /// <summary>
        /// Возникает при изменении коллекции тэгов.
        /// </summary>
        [Category("Специальные")]
        public event TaskEventHandler TagCollectionOfTaskWasChanged;
        /// <summary>
        /// Возникает при изменения состояния задачи устройства.
        /// </summary>
        [Category("Специальные")]
        public event TaskEventHandler TaskStateWasChanged;
        /// <summary>
        /// Возникает при изменения состояния коллекции задач устройств устройств.
        /// </summary>
        [Category("Специальные")]
        public event EventHandler TaskCollectionWasChanged;
        /// <summary>
        /// Возникает при изменения значения одного из свойств устройства.
        /// </summary>
        [Category("Специальные")]
        public event DeviceEventHandler DevicePropertyWasChanged;
        /// <summary>
        /// Возникает при возникновении пользовательского сообщения.
        /// </summary>
        [Category("Специальные")]
        public event MessageEvent Message;

        /* 2. Методы генерации события */
        /// <summary>
        /// Вызывает "Событие при нажатии клавиши пользователем "Сохранить настройки проекта"".
        /// </summary>
        private void Event_SaveProjectClick()
        {
            if (this.SaveProjectClick != null)
            {
                this.SaveProjectClick(this, null);
            }
        }
        /// <summary>
        /// Вызывает "Событие при обработке чтении одного из тэгов из контроллера, данные тэга которого изменились с момента последнего чтения".
        /// </summary>
        /// <param name="tag"></param>
        private void Event_TagsValueWasChanged(TagsEventArgs e)
        {
            if (this.TagsValueWasChanged != null)
            {
                this.TagsValueWasChanged(this, e);
            }
        }
        /// <summary>
        /// Вызывает "Событие при обработке чтения одного из тэгов устройства".
        /// </summary>
        /// <param name="tag"></param>
        private void Event_TagsValueWasReaded(TagsEventArgs e)
        {
            if (this.TagsValueWasReaded != null)
            {
                this.TagsValueWasReaded(this, e);
            }
        }
        /// <summary>
        /// Вызывает "Событие при обработке записи одного из тэгов устройства".
        /// </summary>
        /// <param name="tag"></param>
        private void Event_TagsValueWasWrited(TagsEventArgs e)
        {
            if (this.TagsValueWasWrited != null)
            {
                this.TagsValueWasWrited(this, e);
            }
        }
        /// <summary>
        /// Вызывает "Событие при изменении коллекции тэгов".
        /// </summary>
        /// <param name="task"></param>
        private void Event_TagCollectionOfTaskWasChanged(LogixTask task)
        {
            if (this.TagCollectionOfTaskWasChanged != null)
            {
                this.TagCollectionOfTaskWasChanged(this, new TaskEventArgs(task));
            }
        }
        /// <summary>
        /// Вызывает "Событие при изменения состояния подключения с сервером".
        /// </summary>
        /// <param name="task"></param>
        private void Event_TaskStateWasChanged(LogixTask task)
        {
            if (this.TaskStateWasChanged != null)
            {
                this.TaskStateWasChanged(this, new TaskEventArgs(task));
            }
        }
        /// <summary>
        /// Вызывает "Событие при изменения состояния коллекции устройств".
        /// </summary>
        private void Event_TaskCollectionWasChanged()
        {
            if (this.TaskCollectionWasChanged!=null)
            {
                TaskCollectionWasChanged(this, null);
            }
        }
        /// <summary>
        /// Вызывает "Событие при изменении значения одного из свойств устройства".
        /// </summary>
        /// <param name="device"></param>
        private void Event_DevicePropertyWasChanged(LogixDevice device)
        {
            if (this.DevicePropertyWasChanged != null)
            {
                this.DevicePropertyWasChanged(this, new DeviceEventArgs(device));
            }
        }
        /// <summary>
        /// Вызывает "Событие при возникновении пользовательского сообщения".
        /// </summary>
        /// <param name="device"></param>
        private void Event_Message(MessageEventArgs messageEventArgs)
        {
            if (this.Message != null)
            {
                this.Message(this, messageEventArgs);
            }
        }
        /* ======================================================================================== */
        #endregion

        #region [ EVENT SUBSCRIPTIONS - CLX CONTROL ]
        /* ======================================================================================== */
        /// <summary>
        /// Подписка на событие : TreeNode_EthernetDevice : Одно из свойств устройства было изменено.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void treeNode_EthernetDevice_DevicePropertyWasChanged(object sender, DeviceEventArgs e)
        {
            // Вызываем перерисовывание списка серверов.
            FormsExtensions.InvokeControl<TreeView>(this.treeView, t => t.Refresh());

            // Устанвливаем блокировки окна редактирования свойств.
            if (this.SelectedEthernetDeviceNode == sender)
            {
                RefreshStateOfPropertyControl();
            }

            // Вызываем событие.
            Event_DevicePropertyWasChanged(e.Device);
        }
        /// <summary>
        /// Подписка на событие : TreeNode_EthernetDevice : Изменена коллекция тэгов.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void treeNode_EthernetDevice_TagCollectionWasChanged(object sender, TaskEventArgs e)
        {
            // Вызываем событие.
            Event_TagCollectionOfTaskWasChanged(e.Task);
        }
        /// <summary>
        /// Подписка на событие : TreeNode_EthernetDevice : Обработано чтение одного из тэгов из контроллера, данные тэга которого изменились с момента последнего чтения.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void treeNode_EthernetDevice_TagsValueWasChanged(object sender, TagsEventArgs e)
        {
            Event_TagsValueWasChanged(e);
        }
        /// <summary>
        /// Подписка на событие : TreeNode_EthernetDevice : Обработано чтение одного из тэгов устройства.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void treeNode_EthernetDevice_TagsValueWasReaded(object sender, TagsEventArgs e)
        {
            // Вызываем событие.
            Event_TagsValueWasReaded(e);
        }
        /// <summary>
        /// Подписка на событие : TreeNode_EthernetDevice : Обработана запись одного из тэгов устройства.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void treeNode_EthernetDevice_TagsValueWasWrited(object sender, TagsEventArgs e)
        {
            // Вызываем событие.
            Event_TagsValueWasWrited(e);
        }
        /// <summary>
        /// Подписка на событие : TreeNode_EthernetDevice : Изменилось состояние задачи устройства.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void treeNode_EthernetDevice_TaskStateWasChanged(object sender, TaskEventArgs e)
        {
            // Вызываем перерисовывание списка серверов.
            FormsExtensions.InvokeControl<TreeView>(this.treeView, t => t.Refresh());

            // Устанвливаем статус сервера в окне редактирования свойств.
            if (this.SelectedEthernetDeviceNode == sender)
            {
                RefreshStateOfPropertyControl();
                RefreshStateOfHeadControl();
            }

            // Вызываем событие.
            Event_TaskStateWasChanged(e.Task);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void treeNode_EthernetDevice_Message(object sender, MessageEventArgs e)
        {
            this.Event_Message(e);
        }
        /* ======================================================================================== */
        #endregion

        #region [ EVENT SUBSCRIPTIONS - FORMS ]
        /* ======================================================================================== */
        /// <summary>
        /// Подписка на событие : ToolStripButton : Нажата кнопка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton_Save_Click(object sender, EventArgs e)
        {
            Event_SaveProjectClick();
        }
        /// <summary>
        /// Подписка на событие : ToolStripButton : Нажата кнопка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton_Add_Click(object sender, EventArgs e)
        {
            string plcName = "NewPLC_";
            for (int ix = 0; ix < 1000; ix++)
            {
                if (!this.CLXDevicesByName.ContainsKey(plcName + ix.ToString()))
                {
                    plcName = plcName + ix.ToString();
                    break;
                }
            }

            LogixDevice newCLXDevice = new LogixDevice(plcName, new System.Net.IPAddress(new byte[] { 0, 0, 0, 0 }), 0);
            TreeNode_EthernetDevice treeNodeEthernetDevice = new TreeNode_EthernetDevice(newCLXDevice);
            this.Add(treeNodeEthernetDevice);
            this.treeView.SelectedNode = treeNodeEthernetDevice;
        }
        /// <summary>
        /// Подписка на событие : ToolStripButton : Нажата кнопка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton_Remove_Click(object sender, EventArgs e)
        {
            this.Remove(this.SelectedEthernetDeviceNode);
        }

        /// <summary>
        /// Подписка на событие : TreeView : Изменен выбранный узел.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RefreshStateOfHeadControl();
            RefreshStateOfPropertyControl();

            if (this.SelectedEthernetDeviceNode != null)
            {
                this.textBoxDeviceName.Text = this.SelectedEthernetDeviceNode.Device.Name;
                this.textBoxDeviceIpAddress.Text = this.SelectedEthernetDeviceNode.Device.Address.ToString();
                this.numericUpDownSlotNumber.Value = this.SelectedEthernetDeviceNode.Device.ProcessorSlot;
            }
            else
            {
                this.textBoxDeviceName.Text = "";
                this.textBoxDeviceIpAddress.Text = "";
                this.numericUpDownSlotNumber.Value = 0;
            }

            this.treeView.Refresh();
        }

        /// <summary>
        /// Подписка на событие : TextBox : Было изменен текст.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxDeviceName_TextChanged(object sender, EventArgs e)
        {
            this.buttonApply.Enabled = (this.SelectedEthernetDeviceNode != null && this.SelectedEthernetDeviceNode.Device.Name != this.textBoxDeviceName.Text);
        }
        /// <summary>
        /// Подписка на событие : TextBox : Было изменен текст.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxDeviceIpAddress_TextChanged(object sender, EventArgs e)
        {
            this.buttonApply.Enabled = (this.SelectedEthernetDeviceNode != null && this.SelectedEthernetDeviceNode.Device.Address.ToString() != this.textBoxDeviceIpAddress.Text);
        }
        /// <summary>
        /// Подписка на событие : NumericUpDown : Было изменено значение.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownSlotNumber_ValueChanged(object sender, EventArgs e)
        {
            this.buttonApply.Enabled = (this.SelectedEthernetDeviceNode != null && this.SelectedEthernetDeviceNode.Device.ProcessorSlot != this.numericUpDownSlotNumber.Value);
        }
        /// <summary>
        /// Подписка на событие : Button : Нажата кнопка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (this.SelectedEthernetDeviceNode != null)
            {
                Dictionary<string, TreeNode_EthernetDevice> devicesByName = this.CLXDevicesByName;
                Dictionary<string, TreeNode_EthernetDevice> devicesByIpAddress = this.CLXDevicesByAddress;

                string currentName = textBoxDeviceName.Text;
                string currentAddress = textBoxDeviceIpAddress.Text + "/" + this.numericUpDownSlotNumber.Value.ToString();
                
                // Проверка установки нового имени устройства.
                if (devicesByName.ContainsKey(currentName) && devicesByName[currentName] != this.SelectedEthernetDeviceNode)
                {
                    MessageBox.Show("Imposible to set name.\r\nController with this name already exist.", MESSAGE_HEADER, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.textBoxDeviceName.Text = this.SelectedEthernetDeviceNode.Device.Name;
                    this.textBoxDeviceName.Focus();
                    return;
                }

                // Проверка установки нового адреса устройства.
                if (devicesByIpAddress.ContainsKey(currentAddress) && devicesByIpAddress[currentAddress] != this.SelectedEthernetDeviceNode)
                {
                    MessageBox.Show("Imposible to set IP Address.\r\nController with this IP Address already exist:\r\n"
                        + devicesByIpAddress[currentAddress].Name + ", " + devicesByIpAddress[currentAddress].Device.Address + ", slot: " + devicesByIpAddress[currentAddress].Device.ProcessorSlot.ToString(), 
                        MESSAGE_HEADER, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    this.textBoxDeviceIpAddress.Text = this.SelectedEthernetDeviceNode.Device.Address.ToString();
                    this.numericUpDownSlotNumber.Value = this.SelectedEthernetDeviceNode.Device.ProcessorSlot;
                    this.textBoxDeviceIpAddress.Focus();
                    return;
                }

                System.Net.IPAddress ipAddress;
                if (!System.Net.IPAddress.TryParse(this.textBoxDeviceIpAddress.Text, out ipAddress))
                {
                    MessageBox.Show("Imposible to set IP Address.\r\n" + "Address not valid.",
                        MESSAGE_HEADER, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.textBoxDeviceIpAddress.Focus();
                    return;
                }


                this.SelectedEthernetDeviceNode.Device.Name = this.textBoxDeviceName.Text;
                this.SelectedEthernetDeviceNode.Device.Address = ipAddress;
                this.SelectedEthernetDeviceNode.Device.ProcessorSlot = (byte)this.numericUpDownSlotNumber.Value;

                this.buttonApply.Enabled = false;
            }
        }
        /* ======================================================================================== */
        #endregion

        #region [ PUBLIC METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Добавляет новое устройство по умолчанию в контейнер.
        /// </summary>
        /// <param name="namePrefix">Префикс имени по умолчанию для добваления. После текущего имени следуют символы цифр.</param>
        /// <param name="defaultIpAddress">IP Адрес устройства по умолчанию.</param>
        /// <returns></returns>
        public bool Add(string namePrefix, byte [] defaultIpAddress)
        {
            bool result;
                        string plcName = namePrefix;
            for (int ix = 0; ix < 1000; ix++)
            {
                if (!this.CLXDevicesByName.ContainsKey(plcName + ix.ToString()))
                {
                    plcName = plcName + ix.ToString();
                    break;
                }
            }

            LogixDevice newCLXDevice = new LogixDevice(plcName, new System.Net.IPAddress(defaultIpAddress), 0);
            TreeNode_EthernetDevice treeNodeEthernetDevice = new TreeNode_EthernetDevice(newCLXDevice);
            result = this.Add(treeNodeEthernetDevice);
            this.treeView.SelectedNode = treeNodeEthernetDevice;

            return result;
        }
        /// <summary>
        /// Добавляет новое устройство в контейнер.
        /// </summary>
        /// <param name="treeNode_EthernetDevice">Новый визуальный объект характеризующий устройство.</param>
        /// <returns></returns>
        public bool Add(TreeNode_EthernetDevice treeNode_EthernetDevice)
        {
            if (treeNode_EthernetDevice == null)
            {
                return false;
            }

            string name = treeNode_EthernetDevice.Device.Name;
            string address = treeNode_EthernetDevice.Device.Address + "/" + treeNode_EthernetDevice.Device.ProcessorSlot;

            if (this.CLXDevicesByName.ContainsKey(name) || this.CLXDevicesByAddress.ContainsKey(address))
            {
                return false;
            }

            this.treeView.Nodes.Add(treeNode_EthernetDevice);

            treeNode_EthernetDevice.DevicePropertyWasChanged += treeNode_EthernetDevice_DevicePropertyWasChanged;
            treeNode_EthernetDevice.TagsValueWasChanged += treeNode_EthernetDevice_TagsValueWasChanged;
            treeNode_EthernetDevice.TagsValueWasReaded += treeNode_EthernetDevice_TagsValueWasReaded;
            treeNode_EthernetDevice.TagsValueWasWrited += treeNode_EthernetDevice_TagsValueWasWrited;
            treeNode_EthernetDevice.TaskStateWasChanged += treeNode_EthernetDevice_TaskStateWasChanged;
            treeNode_EthernetDevice.Message += treeNode_EthernetDevice_Message;

            // Вызываем событие.
            Event_TaskCollectionWasChanged();

            return true;
        }
        /// <summary>
        /// Удаляет устройство из контейнера.
        /// </summary>
        /// <param name="clxDevice">Устройство которое необходимо удалить.</param>
        /// <returns></returns>
        public bool Remove(TreeNode_EthernetDevice treeNode_EthernetDevice)
        {
            if (treeNode_EthernetDevice == null)
            {
                return false;
            }

            if (!this.CLXDevicesByName.ContainsKey(treeNode_EthernetDevice.Device.Name))
            {
                return false;
            }

            if (treeNode_EthernetDevice.Task.ProcessState != TaskProcessState.Stop 
                || treeNode_EthernetDevice.Task.ServerState != ServerState.Off)
            {
                return false;
            }

            treeNode_EthernetDevice.DevicePropertyWasChanged -= treeNode_EthernetDevice_DevicePropertyWasChanged;
            treeNode_EthernetDevice.TagsValueWasChanged -= treeNode_EthernetDevice_TagsValueWasChanged;
            treeNode_EthernetDevice.TagsValueWasReaded -= treeNode_EthernetDevice_TagsValueWasReaded;
            treeNode_EthernetDevice.TagsValueWasWrited -= treeNode_EthernetDevice_TagsValueWasWrited;
            treeNode_EthernetDevice.TaskStateWasChanged -= treeNode_EthernetDevice_TaskStateWasChanged;
            treeNode_EthernetDevice.Message -= treeNode_EthernetDevice_Message;

            this.treeView.Nodes.Remove(treeNode_EthernetDevice);

            // Вызываем событие.
            Event_TaskCollectionWasChanged();

            return true;
        }
        /// <summary>
        /// Удаляет все устройства из контейнера.
        /// </summary>
        public void Clear()
        {
            this.treeView.Nodes.Clear();
            // Вызываем событие.
            Event_TaskCollectionWasChanged();
        }

        /// <summary>
        /// Получает настройки в виде xml объекта.
        /// </summary>
        public XElement GetXElement()
        {
            XElement xDevices = new XElement("Devices");
            foreach (TreeNode_EthernetDevice tnEthernetDevice in this.CLXDevicesByName.Values)
            {
                xDevices.Add(tnEthernetDevice.Device.GetXElement());
            }
            return xDevices;
        }
        /// <summary>
        /// Устанавливает настройки из xml объекта.
        /// </summary>
        /// <param name="xelem">Xml объект Settings.</param>
        /// <returns>Возвращает true при успешном результате.</returns>
        public bool SetXSettings(XElement xelem)
        {
            // Проверяем входные параметры.
            if (!xelem.ExistAs("Devices"))
            {
                return false;
            }

            // Очищаем все устройства в списке.
            this.Clear();

            // Добавляем новые устройства из списка.
            foreach (XElement xDevice in xelem.Elements("Device"))
            {
                LogixDevice newCLXDevice = new LogixDevice();
                if (newCLXDevice.SetXElement(xDevice))
                {
                    this.Add(new TreeNode_EthernetDevice(newCLXDevice));
                }
            }

            return true;
        }
        /* ======================================================================================== */
        #endregion

        #region [ PRIVATE METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Вызывает установку состояний окна редактирования свойств в соответствии с текущим выделенным объектом.
        /// </summary>
        private void RefreshStateOfPropertyControl()
        {
            string stateText = "";
            Color backColor = this.BackColor;
            if (this.SelectedEthernetDeviceNode != null)
            {
                switch (this.SelectedEthernetDeviceNode.Task.ServerState)
                {
                    case ServerState.Off:
                        {
                            stateText = "offline";
                            backColor = Color.LightGray;
                        }
                        break;
                    case ServerState.Init:
                    case ServerState.Run:
                        {
                            stateText = "online";
                            backColor = Color.Chartreuse;
                        }
                        break;
                    case ServerState.Error:
                        {
                            stateText = "error";
                            backColor = Color.Tomato;
                        }
                        break;
                    default:
                        {
                            stateText = "connection";
                            backColor = Color.Aqua;
                        }
                        break;
                }
            }

            FormsExtensions.InvokeControl<TextBox>(this.textBoxStatus, t => t.Text = stateText);
            FormsExtensions.InvokeControl<TextBox>(this.textBoxStatus, t => t.BackColor = backColor);

            bool value = this.SelectedEthernetDeviceNode != null && this.SelectedEthernetDeviceNode.Task.ProcessState == TaskProcessState.Stop;
            FormsExtensions.InvokeControl<TextBox>(this.textBoxDeviceName, t=>t.Enabled = value);
            FormsExtensions.InvokeControl<TextBox>(this.textBoxDeviceIpAddress, t => t.Enabled = value);
            FormsExtensions.InvokeControl<NumericUpDown>(this.numericUpDownSlotNumber,n=>n.Enabled = value);
            FormsExtensions.InvokeControl<NumericUpDown>(this.numericUpDownSlotNumber, n => n.Visible = this.SelectedEthernetDeviceNode != null);
        }
        /// <summary>
        /// Вызывает установку состояний головного окна управления в соответствии с текущим выделенным объектом.
        /// </summary>
        private void RefreshStateOfHeadControl()
        {
            bool removeButtonVisible = this.SelectedEthernetDeviceNode != null 
                && this.SelectedEthernetDeviceNode.Task.ProcessState == TaskProcessState.Stop
                && this.SelectedEthernetDeviceNode.Task.ServerState == ServerState.Off;

            string connectButtonText = (this.SelectedEthernetDeviceNode == null 
                || this.SelectedEthernetDeviceNode.Task.ProcessState == TaskProcessState.Stop) ? "Online" : "Offline";

            FormsExtensions.InvokeControl<ToolStrip>(this.toolStrip, b => 
            {
                this.toolStripButton_Remove.Enabled = removeButtonVisible;
            });        
        }
        /* ======================================================================================== */
        #endregion
    }
}
