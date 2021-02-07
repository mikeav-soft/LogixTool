using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.Xml.Linq;
using EIP.AllenBradley.Models.Events;

namespace LogixTool.Common
{
    /// <summary>
    /// Пользовательский элемент управления для отображения потоковых сообщений.
    /// </summary>
    public partial class MessageControl : UserControl
    {
        /// <summary>
        /// Название объекта Xml.
        /// </summary>
        public const string XML_NAME = "MessageControl";

        #region [ PROPERTIES ]
        /* ======================================================================================== */
        /// <summary>
        /// Возвращает или задает список всех сообщений попавших в компонент управления.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<MessageEventArgs> Items { get; set; }

        /// <summary>
        /// Возвращает или задает интервавл между перерисовками элемента управления миллисекунд.
        /// </summary>
        [Browsable(true)]
        [Category("Специальные")]
        [DisplayName("UpdateInterval")]
        public double UpdateInterval
        {
            get
            {
                return this.timer.Interval;
            }
            set
            {
                this.timer.Interval = value;
            }
        }
        /// <summary>
        /// Возвращает или задает максимальное колличество сообщений для хранения.
        /// При превышении колличества сообщений более старые сообщения будут удаляться.
        /// </summary>
        [Browsable(true)]
        [Category("Специальные")]
        [DisplayName("MessageCapacity")]
        public int MessageCapacity { get; set; }
        /// <summary>
        /// Возвращает или задает формат отображения даты/времени сообщения.
        /// </summary>
        [Browsable(true)]
        [Category("Специальные")]
        [DisplayName("TimeStampFormat")]
        public string TimeStampFormat { get; set; }
        /// <summary>
        /// Возвращает или задает список типов сообщений для текущего отображения.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable<MessageEventArgsType> VisibleItems
        {
            get
            {
                return this._VisibleItems.Where(p => p.Value).Select(i => i.Key);
            }
            set
            {
                if (value == null)
                    return;

                foreach (MessageEventArgsType k in _VisibleItems.Keys.ToList())
                {
                    this._VisibleItems[k] = false;
                }

                foreach (MessageEventArgsType k in value)
                {
                    if (!this._VisibleItems.ContainsKey(k))
                        this._VisibleItems.Add(k, false);

                    this._VisibleItems[k] = true;
                }

                this.rebuildDisplayedItemsRequired = true;
            }
        }
        /// <summary>
        /// Возвращает или задает список типов сообщений для игнорирования.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable<MessageEventArgsType> IgnoreItems
        {
            get
            {
                return this._IgnoreItems.Where(p => p.Value).Select(i => i.Key);
            }
            set
            {
                if (value == null)
                    return;

                foreach (MessageEventArgsType k in _IgnoreItems.Keys.ToList())
                    this._IgnoreItems[k] = false;

                foreach (MessageEventArgsType k in value)
                {
                    if (!this._IgnoreItems.ContainsKey(k))
                        this._IgnoreItems.Add(k, false);

                    this._IgnoreItems[k] = true;
                }
            }
        }
        /// <summary>
        /// Возвращает или задает сортировку сообщений при значении True по дате добавления от новых сообщений к более старым.
        /// </summary>
        [Browsable(true)]
        [Category("Специальные")]
        [DisplayName("NewMessagesOnTop")]
        public bool NewMessagesOnTop { get; set; }
        /* ======================================================================================== */
        #endregion

        private List<ListViewItem> listViewItemCache;       // Текущий список - кэш для виртуального отображения элементов ListView.
        private int topIndex;                               // Индекс начала диапазона значений необходимый элементу управления ListView в виртуальном режиме.
        private System.Timers.Timer timer;                  // Системный таймер для перерисовки визуальных компонентов.
        private List<MessageEventArgs> displayedItems;      // Текущий полный список для отображения сообщений в элементе управления.
        private bool redrawListViewRequired;                // Содержит значение True при необходимости перерисовки элементов ListView.
        private bool rebuildDisplayedItemsRequired;         // Содержит значение True при необходимости перестроить список элементов для отображения.
        private Dictionary<MessageEventArgsType, bool> _IgnoreItems;
        private Dictionary<MessageEventArgsType, bool> _VisibleItems;

        /// <summary>
        /// 
        /// </summary>
        public MessageControl()
        {
            InitializeComponent();

            // Создаем контейнеры для хранения сообщений.
            this.Items = new List<MessageEventArgs>();
            this.displayedItems = new List<MessageEventArgs>();

            // Инициализируем переменные.
            this.rebuildDisplayedItemsRequired = false;
            this.redrawListViewRequired = false;

            // Инициализируем переменные для виртуализации отображения.
            this.listViewItemCache = new List<ListViewItem>();
            this.topIndex = -1;

            // Устанавливаем значения свойств по умолчанию.
            this.MessageCapacity = int.MaxValue - 1;
            this.TimeStampFormat = "dd.MM.yyyy HH:mm:ss:fffffff";

            // Инициализируем таймер синхронизации объекта управления.
            this.timer = new System.Timers.Timer();
            this.timer.Interval = 1000;
            this.timer.Elapsed += Timer_Elapsed;
            this.timer.SynchronizingObject = listViewMessages;
            this.timer.Enabled = true;

            // Устанавливаем значения по умолчанию видимость типов сообщений.
            this.NewMessagesOnTop = false;

            this._IgnoreItems = new Dictionary<MessageEventArgsType, bool>();
            this._VisibleItems = new Dictionary<MessageEventArgsType, bool>();

            foreach (MessageEventArgsType t in Enum.GetValues(typeof(MessageEventArgsType)))
            {
                this._IgnoreItems.Add(t, false);
                this._VisibleItems.Add(t, true);
            }
        }

        #region [ PUBLIC METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Добавляет новое сообщение в список для отображения.
        /// </summary>
        /// <param name="msg"></param>
        public void AddMessage(MessageEventArgs msg)
        {
            // Проверяем входные параметры на равенстов Null и входит ли тип данного сообщения в коллекцию типов для игнорирования.
            if (msg == null || this._IgnoreItems[msg.Type])
            {
                return;
            }

            // Проверяем что кол-во сообщений не превышает заданное максимальное кол-во сообщений.
            // Если имеет место превышение то удаляем наиболее старый элемент.
            if (this.Items.Count >= this.MessageCapacity)
            {
                int indexForRemove = this.Items.Count - 1;
                this.Items.RemoveAt(indexForRemove);
            }

            // Добавляем новое сообщение в общий список.
            this.Items.Add(msg);

            // Параллельно добавляем новое сообщение в список для отображения учитывая фильтрацию сообщений.
            if (this._VisibleItems[msg.Type])
            {
                if (this.NewMessagesOnTop)
                {
                    this.displayedItems.Insert(0, msg);
                }
                else
                {
                    this.displayedItems.Add(msg);
                }

                // Устанавливаем необходимость перерисовки элемента управления ListView.
                this.redrawListViewRequired = true;
            }
        }
        /// <summary>
        /// Очищает все сообщения находящиеся в контейнере.
        /// </summary>
        public void ClearMessages()
        {
            this.Items.Clear();

            this.rebuildDisplayedItemsRequired = true;
            this.redrawListViewRequired = true;
        }

        /// <summary>
        /// Создает новый xml элемент с настройками данного объекта.
        /// </summary>
        /// <returns>Возвращает xml элемент с настройками.</returns>
        public XElement GetXSettings()
        {
            // Создаем основной xml элемент.
            XElement xelem = new XElement(XML_NAME);
            xelem.Add(new XAttribute("MessageCapacity", this.MessageCapacity.ToString()));
            xelem.Add(new XAttribute("TimeStampFormat", this.TimeStampFormat));
            xelem.Add(new XAttribute("UpdateInterval", this.UpdateInterval.ToString()));
            xelem.Add(new XAttribute("NewMessagesOnTop", this.NewMessagesOnTop.ToString()));

            // Создает элемент с типами сообщений.
            XElement xMessageTypes = new XElement("MessageTypes");
            xelem.Add(xMessageTypes);

            foreach (MessageEventArgsType msgType in Enum.GetValues(typeof(MessageEventArgsType)))
            {
                XElement xMessageType = new XElement("MessageType");
                xMessageType.Add(new XAttribute("Name", msgType.ToString()));
                xMessageType.Add(new XAttribute("Visible", this._VisibleItems[msgType].ToString()));
                xMessageType.Add(new XAttribute("Ignored", this._IgnoreItems[msgType].ToString()));
                xMessageTypes.Add(xMessageType);
            }

            return xelem;
        }
        /// <summary>
        /// Устанавливает значение текущего объекта из xml элемента.
        /// </summary>
        /// <param name="xelem">xml элемент с настройками.</param>
        /// <returns></returns>
        public bool SetXSettings(XElement xelem)
        {
            // Проверяем входые параметры.
            if (xelem == null || xelem.Name != XML_NAME)
            {
                return false;
            }

            int messageCapacity = 1000;         // Извлеченное значение максимальное колличества сообщений для хранения.
            string timeStampFormat;             // Извлеченное значение формата даты/времени сообщений.
            double updateInterval = 1000;       // Извлеченное значение периода обновления/прорисовки сообщений.
            bool newMessagesOnTop = true;       // Извлеченное значение свойства означающее что новые сообщения будут отображаться всегда вверху.

            // Извлечение атрибута "MessageCapacity".
            XAttribute xattrMessageCapacity = xelem.Attribute("MessageCapacity");
            if (xattrMessageCapacity == null || !int.TryParse(xattrMessageCapacity.Value, out messageCapacity))
            {
                return false;
            }

            // Извлечение атрибута "TimeStampFormat".
            XAttribute xattrTimeStampFormat = xelem.Attribute("TimeStampFormat");
            if (xattrTimeStampFormat == null)
            {
                return false;
            }
            timeStampFormat = xattrTimeStampFormat.Value;

            // Извлечение атрибута "UpdateInterval".
            XAttribute xattrUpdateInterval = xelem.Attribute("UpdateInterval");
            if (xattrUpdateInterval == null || !double.TryParse(xattrUpdateInterval.Value, out updateInterval))
            {
                return false;
            }

            // Извлечение атрибута "NewMessagesOnTop".
            XAttribute xattrNewMessagesOnTop = xelem.Attribute("NewMessagesOnTop");
            if (xattrNewMessagesOnTop == null)
            {
                return false;
            }

            if (xattrNewMessagesOnTop.Value.ToLower() == "true")
            {
                newMessagesOnTop = true;
            }
            else if (xattrNewMessagesOnTop.Value.ToLower() == "false")
            {
                newMessagesOnTop = false;
            }
            else
            {
                return false;
            }

            // Проверяем на существование элемент MessageTypes.
            XElement xMessageTypes = xelem.Element("MessageTypes");
            if (xMessageTypes == null)
            {
                return false;
            }

            // Список типов сообщений для отображения.
            List<MessageEventArgsType> visibleMessageTypes = new List<MessageEventArgsType>();
            // Список типов сообщений для игнорирования.
            List<MessageEventArgsType> ignoredMessageTypes = new List<MessageEventArgsType>();

            // Извлекаем значения атрибутов из каждого сообщения.
            foreach (XElement xMessageType in xMessageTypes.Elements("MessageType"))
            {
                MessageEventArgsType type;      // Извлеченное значение типа сообщения.
                bool isVisibleType;             // Извлеченное значение видимости сообщения. 
                bool isIgnoredType;             // Извлеченное значение игнорируемости сообщения.

                // Извлечение атрибута "Name".
                XAttribute xattrName = xMessageType.Attribute("Name");
                if (xattrName == null)
                {
                    return false;
                }

                if (!Enum.TryParse<MessageEventArgsType>(xattrName.Value, out type))
                {
                    return false;
                }

                // Извлечение атрибута "Visible".
                XAttribute xattrVisible = xMessageType.Attribute("Visible");
                if (xattrVisible == null)
                {
                    return false;
                }

                if (xattrVisible.Value.ToLower() == "true")
                {
                    isVisibleType = true;
                }
                else if (xattrVisible.Value.ToLower() == "false")
                {
                    isVisibleType = false;
                }
                else
                {
                    return false;
                }

                // Извлечение атрибута "Ignored".
                XAttribute xattrIgnored = xMessageType.Attribute("Ignored");
                if (xattrIgnored == null)
                {
                    return false;
                }

                if (xattrIgnored.Value.ToLower() == "true")
                {
                    isIgnoredType = true;
                }
                else if (xattrIgnored.Value.ToLower() == "false")
                {
                    isIgnoredType = false;
                }
                else
                {
                    return false;
                }

                // Добавление текущий тип в список отображения.
                if (isVisibleType)
                    visibleMessageTypes.Add(type);

                // Добавление текущий тип в список игнорирования.
                if (isIgnoredType)
                    ignoredMessageTypes.Add(type);
            }

            // Присваиваем извлеченные значения.
            this.MessageCapacity = messageCapacity;
            this.TimeStampFormat = timeStampFormat;
            this.UpdateInterval = updateInterval;
            this.NewMessagesOnTop = newMessagesOnTop;

            this.VisibleItems = visibleMessageTypes;
            this.IgnoreItems = ignoredMessageTypes;

            return true;
        }
        /* ======================================================================================== */
        #endregion

        #region [ EVENT SUBSCRIPTIONS ]
        /* ======================================================================================== */
        /// <summary>
        /// Подписка на событие : Timer : Заданное время работы таймера истекло.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ListViewUpdate();
        }
        /// <summary>
        /// Подписка на событие : listViewMessages : Необходимосто создания элементов в кэше хранения в виртуальном режиме.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewMessages_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            topIndex = e.StartIndex;
            int needed = (e.EndIndex - e.StartIndex) + 1;
            if (listViewItemCache.Capacity < needed)
            {
                int toGrow = needed - listViewItemCache.Capacity;
                listViewItemCache.Capacity = needed;

                for (int ix = 0; ix < toGrow; ix++)
                {
                    ListViewItem newItem = new ListViewItem();
                    newItem.Text = "";

                    for (int colIx = newItem.SubItems.Count; colIx < listViewMessages.Columns.Count; colIx++)
                    {
                        newItem.SubItems.Add("");
                    }

                    listViewItemCache.Add(newItem);
                }
            }
        }
        /// <summary>
        /// Подписка на событие : listViewMessages : Необходимосто получения элемента для отображения в виртуальном режиме.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewMessages_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            int cacheIndex = e.ItemIndex - topIndex;
            if (cacheIndex >= 0 && cacheIndex < listViewItemCache.Count)
            {
                ListViewItem cacheItem = listViewItemCache[cacheIndex];

                SetValuesToRow(cacheItem, this.displayedItems[e.ItemIndex]);
                e.Item = cacheItem;
                //e.Item.Selected = false;
            }
            else
            {
                e.Item = listViewItemCache[0];
                foreach (ColumnHeader column in listViewMessages.Columns)
                {
                    if (column.DisplayIndex >= 0)
                    {
                        e.Item.SubItems[column.DisplayIndex].Text = "";
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            this.infoToolStripMenuItem.Checked = this._VisibleItems[MessageEventArgsType.Info];
            this.successToolStripMenuItem.Checked = this._VisibleItems[MessageEventArgsType.Success];
            this.warningToolStripMenuItem.Checked = this._VisibleItems[MessageEventArgsType.Warning];
            this.errorToolStripMenuItem.Checked = this._VisibleItems[MessageEventArgsType.Error];

            this.infoToolStripMenuItem.Visible = !this._IgnoreItems[MessageEventArgsType.Info];
            this.successToolStripMenuItem.Visible = !this._IgnoreItems[MessageEventArgsType.Success];
            this.warningToolStripMenuItem.Visible = !this._IgnoreItems[MessageEventArgsType.Warning];
            this.errorToolStripMenuItem.Visible = !this._IgnoreItems[MessageEventArgsType.Error];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuButton_Click(object sender, EventArgs e)
        {
            Point locationPoint = menuButton.Location;
            locationPoint.Offset(0, menuButton.Size.Height);
            contextMenuStrip.Show(this, locationPoint);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InverseVisibleOfTypes(MessageEventArgsType.Info);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void errorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InverseVisibleOfTypes(MessageEventArgsType.Error);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void warningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InverseVisibleOfTypes(MessageEventArgsType.Warning);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void successToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InverseVisibleOfTypes(MessageEventArgsType.Success);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ClearMessages();
        }
        /* ======================================================================================== */
        #endregion

        #region [ PRIVATE METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Производит обновление элемента управления ListView с конфигурированием длины виртуального списка.
        /// </summary>
        private void ListViewUpdate()
        {
            if (this.rebuildDisplayedItemsRequired)
            {
                this.rebuildDisplayedItemsRequired = false;
                this.redrawListViewRequired = true;

                // Предварительно очищаем список для отбражения.
                this.displayedItems.Clear();

                // Фиксируем текущий список для дальнейшего использования.
                List<MessageEventArgs> items = this.Items.OrderBy(t => t.Time.Ticks * (this.NewMessagesOnTop ? -1 : 1)).ToList();

                // Перестраиваем список для отображения учитывая примененные фильтра.
                foreach (MessageEventArgs msg in items)
                {
                    if (this._VisibleItems[msg.Type])
                        this.displayedItems.Add(msg);
                }
            }

            // Вызываем перерисовку объекта упроавления ListView.
            if (this.redrawListViewRequired)
            {
                if (this.listViewMessages.VirtualListSize != this.displayedItems.Count)
                    this.listViewMessages.VirtualListSize = this.displayedItems.Count;

                //this.listViewMessages.SelectedIndices.Clear();
                this.listViewMessages.Refresh();

                this.redrawListViewRequired = false;
            }
        }
        /// <summary>
        /// Устанавливает значения ячеек в строке.
        /// </summary>
        /// <param name="item">Элемент списка ListView.</param>
        /// <param name="arg">Сообщение для отображения.</param>
        private void SetValuesToRow(ListViewItem item, MessageEventArgs arg)
        {
            item.SubItems[dateTimeColumn.DisplayIndex].Text = arg.Time.ToString(this.TimeStampFormat);
            item.SubItems[typeColumn.DisplayIndex].Text = arg.Type.ToString();
            item.SubItems[headerColumn.DisplayIndex].Text = arg.Header;
            item.SubItems[textColumn.DisplayIndex].Text = arg.Text;

            Color backgroundColor = Color.White;

            switch (arg.Type)
            {
                case MessageEventArgsType.Info: backgroundColor = Color.LightCyan; break;
                case MessageEventArgsType.Success: backgroundColor = Color.PaleGreen; break;
                case MessageEventArgsType.Warning: backgroundColor = Color.LemonChiffon; break;
                case MessageEventArgsType.Error: backgroundColor = Color.MistyRose; break;
            }

            item.BackColor = backgroundColor;
        }

        /// <summary>
        /// Инвертирует видимость типа сообщения с заданным типом.
        /// </summary>
        /// <param name="type">Тип сообщения.</param>
        private void InverseVisibleOfTypes(MessageEventArgsType type)
        {
            // Сохраняем текущие типы сообщений для отображения.
            IEnumerable<MessageEventArgsType> curMsgTypes = this.VisibleItems;
            // Создаем список 
            List<MessageEventArgsType> newMsgTypes = new List<MessageEventArgsType>();

            // Перегруппировываем списки с типами сообщений для отображения.
            // В случае если искомое сообщение есть в текущем списке, то удаляем его.
            // В противном случае, добавляем тип сообщения в список.
            if (curMsgTypes.Contains(type))
            {
                foreach (MessageEventArgsType t in curMsgTypes)
                    if (t != type)
                        newMsgTypes.Add(t);
            }
            else
            {
                foreach (MessageEventArgsType t in curMsgTypes)
                    newMsgTypes.Add(t);

                newMsgTypes.Add(type);
            }

            // Присваиваем резкльтат.
            this.VisibleItems = newMsgTypes;
        }
        /* ======================================================================================== */
        #endregion


    }
}
