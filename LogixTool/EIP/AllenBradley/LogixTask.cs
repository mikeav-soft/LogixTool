using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EIP.AllenBradley.Models;
using EIP.AllenBradley.Models.Events;

namespace EIP.AllenBradley
{
    /// <summary>
    /// Задача циклического потока управления для автоматизированного получения данных
    /// тэгов и параметров.
    /// </summary>
    public class LogixTask
    {
        public const string MESSAGE_HEADER = "TAG TASK";

        #region [ INTERNAL CLASSES ]
        /* ================================================================================================== */
        /// <summary>
        /// Вложенный класс - контейнер для хранения отчетов текущих тэгов для динамического определения изменения данных.
        /// </summary>
        private class ReportContainer
        {
            /// <summary>
            /// Возвращает или задает отчет чтения тэга.
            /// </summary>
            public TagValueReport ReadReport { get; set; }
            /// <summary>
            /// Возвращает или задает отчет записи тэга.
            /// </summary>
            public TagValueReport WriteReport { get; set; }

            /// <summary>
            /// Создает новый контейнер хранения отчетов на основании заданного держателя тэга.
            /// </summary>
            /// <param name="tag">Текущий держатель тэга удаленного устройства.</param>
            public ReportContainer(LogixTagHandler tag)
            {
                SetReportsFromTag(tag);
            }
            /// <summary>
            /// устанавливает значения отчетов чтения/записи на основании заданного держателя тэга.
            /// </summary>
            /// <param name="tag">Текущий держатель тэга удаленного устройства.</param>
            public void SetReportsFromTag(LogixTagHandler tag)
            {
                this.ReadReport = tag.ReadValue.Report;
                this.WriteReport = tag.WriteValue.Report;
            }
        }
        /* ================================================================================================== */
        #endregion

        #region [ PROPERTIES ]
        /* ================================================================================================== */
        private ServerState _State;
        /// <summary>
        /// Состояние контроллера.
        /// </summary>
        public ServerState ServerState
        {
            get
            {
                return _State;
            }
            private set
            {
                _State = value;
                Event_StateWasChanged();
            }
        }
        /// <summary>
        /// Возвращает текущее состояние статуса задачи.
        /// </summary>
        public TaskProcessState ProcessState
        {
            get
            {
                if (this.processRunRequest)
                {
                    if (this.processRunEnable)
                    {
                        return TaskProcessState.Run;
                    }
                    else
                    {
                        return TaskProcessState.Running;
                    }
                }
                else
                {
                    if (this.processRunEnable)
                    {
                        return TaskProcessState.Stoping;
                    }
                    else
                    {
                        return TaskProcessState.Stop;
                    }
                }

            }
        }

        /// <summary>
        /// Возвращает или задает текущее устройство с которым работает задача.
        /// </summary>
        public LogixDevice Device { get; private set; }

        /// <summary>
        /// Возвращает все тэги которые находятся в процессе данной задачи чтения/записи значений.
        /// </summary>
        public List<LogixTagHandler> TagsInProcess
        {
            get
            {
                return this.mainProcessTags.Keys.ToList();
            }
        }
        /// <summary>
        /// Возвращает текущие таблицы группового чтения тэгов.
        /// </summary>
        public List<CLXCustomTagMemoryTable> TablesInProcess
        {
            get
            {
                return this.tables.ToList();
            }
        }

        /// <summary>
        /// Возвращат или задает все доступные тэги удаленного устройства (контроллера).
        /// </summary>
        public Dictionary<string, CLXTag> AvaliableControllerTags { get; set; }
        /// <summary>
        /// Возвращает или задает все структуры типов данных удаленного устройства (контроллера).
        /// </summary>
        public Dictionary<UInt16, CLXTemplate> AvaliableTemplateStructures { get; set; }
        /* ================================================================================================== */
        #endregion

        #region [ PRIVATE PROPERTIES ]
        /* ================================================================================================== */
        /// <summary>
        /// Возвращает значение true в случае работы основного процесса обробоки. 
        /// </summary>
        private bool ThreadRunned
        {
            get
            {
                return thread != null && (thread.ThreadState == ThreadState.Running || thread.ThreadState == ThreadState.Background);
            }
        }
        /* ================================================================================================== */
        #endregion

        #region [ FIELDS ]
        /* ================================================================================================== */
        private bool processRunRequest;                                         // Запрос фонового потока/процесса на запуск.
        private bool processRunEnable;                                          // Фоновый поток запущен.
        private Dictionary<LogixTagHandler, ReportContainer> mainProcessTags;   // Список тэгов которые участсвуют в циклическом записи/чтении.
        private Thread thread;                                                  // Поток в котором ведется управление со связью с удаленым контроллером.
        private bool runCyclicProcess;                                          // Разрешение на работу циклического процесса основной обработки данных.
        private List<CLXCustomTagMemoryTable> tables;                           // Таблицы для группового чтения данных с контроллера.
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// Создает новую задачу для автоматического процесса обновления данных устройства.
        /// </summary>
        /// <param name="device">Устрйоство с которым происходит взаимодействие.</param>
        public LogixTask(LogixDevice device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device", "Constructor 'LogixTask()': Argument 'device' is NULL");
            }

            this.processRunRequest = false;
            this.processRunEnable = false;

            this.Device = device;
            this.Device.PropertyWasChanged += Device_PropertyWasChanged;

            this.mainProcessTags = new Dictionary<LogixTagHandler, ReportContainer>();
            this.runCyclicProcess = true;
            this.tables = new List<CLXCustomTagMemoryTable>();
            this.AvaliableControllerTags = new Dictionary<string, CLXTag>();
            this.AvaliableTemplateStructures = new Dictionary<ushort, CLXTemplate>();

            this.RunTread();
        }

        #region [ EVENTS ]
        /* ================================================================================================== */

        /* 1. События */

        /// <summary>
        /// Возникает при обработке чтения тэгов из контроллера, данные тэга которого изменились с момента последнего чтения.
        /// </summary>
        public event TagsEventHandler TagsValueWasChanged;
        /// <summary>
        /// Возникает при успешной обработки чтения тэгов из контроллера.
        /// </summary>
        public event TagsEventHandler TagsValueWasReaded;
        /// <summary>
        /// Возникает при успешной обработки записи тэгов в контроллер.
        /// </summary>
        public event TagsEventHandler TagsValueWasWrited;

        /// <summary>
        /// Возникает при изменении состояния соединения с сервером.
        /// </summary>
        public event EventHandler StateWasChanged;
        /// <summary>
        /// Возникает при появлении каких либо сообщений при работе ядра.
        /// </summary>
        public static event MessageEvent Messages;
        /// <summary>
        /// Возникает при появлении каких либо сообщений при работе ядра.
        /// </summary>
        public event MessageEvent Message;

        /* 2. Методы генерации события */

        /// <summary>
        /// Вызывает "Событие при обработке чтения тэгов из контроллера, данные тэга которогоизменились с момента последнего чтения".
        /// </summary>
        /// <param name="tags"></param>
        private void Event_TagsValueWasChanged(List<LogixTagHandler> tags)
        {
            if (TagsValueWasChanged != null)
            {
                TagsValueWasChanged(this, new TagsEventArgs(tags));
            }
        }
        /// <summary>
        /// Вызывает "Событие при успешной обработки чтения тэгов из контроллера".
        /// </summary>
        /// <param name="tags"></param>
        private void Event_TagsValueWasReaded(List<LogixTagHandler> tags)
        {
            if (TagsValueWasReaded != null)
            {
                TagsValueWasReaded(this, new TagsEventArgs(tags));
            }
        }
        /// <summary>
        /// Вызывает "Событие при успешной обработки записи тэгов в контроллер".
        /// </summary>
        /// <param name="tags"></param>
        private void Event_TagsValueWasWrited(List<LogixTagHandler> tags)
        {
            if (TagsValueWasWrited != null)
            {
                TagsValueWasWrited(this, new TagsEventArgs(tags));
            }
        }

        /// <summary>
        /// Вызывает "Событие при установке соединения с сервером".
        /// </summary>
        private void Event_StateWasChanged()
        {
            if (StateWasChanged != null)
            {
                StateWasChanged(this, null);
            }
        }
        /// <summary>
        /// Вызывает "Событие с сообщением".
        /// </summary>
        /// <param name="e"></param>
        internal void Event_Message(MessageEventArgs e)
        {
            MessageEventArgs messageEventArgs = e;
            string messageHeader = "[" + MESSAGE_HEADER + "]." + messageEventArgs.Header;
            messageEventArgs.Header = messageHeader;

            if (this.Message != null)
            {
                this.Message(this, messageEventArgs);
            }

            if (Messages != null)
            {
                Messages(this, messageEventArgs);
            }
        }
        /* ================================================================================================== */
        #endregion

        #region [ EVENTS SUBSCRIPTIONS ]
        /* ================================================================================================== */
        /// <summary>
        /// Подписка на событие : CLXDevice : Значения тэга были записаны в контроллер.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Device_TagsValueWasWrited(object sender, TagsEventArgs e)
        {
            if (TagsValueWasWrited != null)
            {
                TagsValueWasWrited(this, e);
            }
        }
        /// <summary>
        /// Подписка на событие : CLXDevice : Значения тэга были прочитаны из контроллера.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Device_TagsValueWasReaded(object sender, TagsEventArgs e)
        {
            if (TagsValueWasReaded != null)
            {
                TagsValueWasReaded(this, e);
            }
        }
        /// <summary>
        /// Подписка на событие : CLXDevice : Одно из свойств устройства было изменено.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Device_PropertyWasChanged(object sender, EventArgs e)
        {
        }
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// Содержит основной процесс управления.
        /// </summary>
        protected void Process()
        {
            while (runCyclicProcess)
            {
                #region [ 1. CONNECTION TO SERVER ]
                /* ====================================================================================== */
                // В начале главного процесса присваиваем состояние разрешения подключения 
                // к удаленному устройству. Данная точка необходимо для того чтобы избежать отключения разрешения
                // в середине процесса.
                if (this.processRunRequest && !this.processRunEnable)
                {
                    this.processRunEnable = true;
                }

                // В случае разрешения подключения пытаемся поэтапно произвести действия с удаленным устройством.
                // А именно: 
                //      1. TcpConnection    - подключение TCP/IP.
                //      2. Register Session - регистрация сессии.
                //      3. ForwardOpen      - открытие подключения.
                // При обрыве подключения TCP/IP устанавливаем ошибочное подключение и начинаем процесс подключения заново.
                if (this.processRunEnable)
                {
                    // Проверка соедниения с сервером на наличие подключения.
                    if (!this.Device.IsConnected && this.ServerState > ServerState.TcpConnection)
                    {
                        this.ServerState = ServerState.Error;
                    }

                    switch (this.ServerState)
                    {
                        case ServerState.Off:
                            if (this.Device.IsConnected)
                            {
                                this.Device.Disconnect();
                            }
                            else
                            {
                                this.ServerState = ServerState.TcpConnection;
                            }
                            break;

                        case ServerState.TcpConnection:
                            {
                                if (this.Device.Connect())
                                {
                                    this.ServerState = ServerState.Register;
                                }
                                else
                                {
                                    this.ServerState = ServerState.Error;
                                }
                            }
                            break;

                        case ServerState.Register:
                            {
                                if (this.Device.RegisterSession())
                                {
                                    this.ServerState = ServerState.ForwardOpen;
                                }
                                else
                                {
                                    this.ServerState = ServerState.Error;
                                }
                            }
                            break;

                        case ServerState.ForwardOpen:
                            {
                                if (this.Device.ForwardOpen())
                                {
                                    this.ServerState = ServerState.Init;
                                }
                                else
                                {
                                    this.ServerState = ServerState.Error;
                                }
                            }
                            break;

                        case ServerState.Error:
                            {
                                Thread.Sleep(2000);
                                this.ServerState = ServerState.Off;
                            }
                            break;
                    }
                }

                /* ====================================================================================== */
                #endregion

                #region [ 2. PREPARATION OF TAGS ]
                /* ====================================================================================== */
                if (this.processRunEnable && this.ServerState == ServerState.Init)
                {
                    // Очищаем список доступных тэгов удаленного устройства и
                    this.AvaliableControllerTags.Clear();
                    // Очищаем список доступных структур типов данных удаленного устройства.
                    this.AvaliableTemplateStructures.Clear();
                    // Очищаем список таблиц.
                    this.tables.Clear();

                    // Инициализируем состояние тэгов перед чтением.
                    foreach (LogixTagHandler tag in this.mainProcessTags.Keys)
                    {
                        tag.InitState();
                        tag.OwnerTask = this;
                    }

                    #region [ 2.1. ПОЛУЧЕНИЕ СПИСКА ТЭГОВ УДАЛЕННОГО УСТРОЙСТВА ]
                    /* ====================================================================================== */
                    if (this.ServerState == ServerState.Init)
                    {
                        // Получаем список всех глобальных тэгов удаленного устройства.
                        List<CLXTag> recievedControllerTags;
                        List<CLXTag> recievedProgramTags;

                        #region [ 2.1.1 ПОЛУЧЕНИЕ ГЛОБАЛЬНЫХ ТЭГОВ УДАЛЕННОГО УСТРОЙСТВА ]
                        /* ====================================================================================== */
                        if (this.Device.GetTagsAddreses(out recievedControllerTags))
                        {
                            // Добавляем принятые тэги удаленного устройства.
                            foreach (CLXTag t in recievedControllerTags)
                            {
                                if (t.Name != null)
                                {
                                    if (!this.AvaliableControllerTags.ContainsKey(t.Name))
                                    {
                                        this.AvaliableControllerTags.Add(t.Name, t);
                                    }
                                    else
                                    {
                                        Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, "Tag List Reading", "Error! Name of recieved tag already exist."));
                                    }
                                }
                            }
                        }
                        else
                        {
                            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, "Tag List Reading", "Error! Imposible to read tags from Device."));
                            // В случае неудачи устанавливаем состояние ошибки.
                            this.ServerState = ServerState.Error;
                        }
                        /* ====================================================================================== */
                        #endregion

                        #region [ 2.1.2 ПОЛУЧЕНИЕ ПРОГРАММНЫХ ТЭГОВ УДАЛЕННОГО УСТРОЙСТВА ]
                        /* ====================================================================================== */
                        if (recievedControllerTags != null)
                        {
                            const string PROGRAM_PREFIX = "Program:";
                            IEnumerable<CLXTag> programTags = recievedControllerTags.Where(p => p != null && p.Name != null && p.Name.StartsWith(PROGRAM_PREFIX));

                            foreach (string program in programTags.Select(p => p.Name))
                            {
                                // Получаем список всех програмных тэгов удаленного устройства для каждой программы.
                                if (this.Device.GetTagsAddreses(program, out recievedProgramTags))
                                {
                                    // Добавляем принятые тэги удаленного устройства.
                                    foreach (CLXTag t in recievedProgramTags)
                                    {
                                        if (t.Name != null)
                                        {
                                            string key = program + "." + t.Name;
                                            if (!this.AvaliableControllerTags.ContainsKey(key))
                                            {
                                                this.AvaliableControllerTags.Add(key, t);
                                            }
                                            else
                                            {
                                                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, "Tag List Reading", "Error! Name of recieved tag already exist. Program :: '" + program + "'."));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, "Tag List Reading", "Error! Imposible to read tags from Device. Program :: '" + program + "'."));

                                    // В случае неудачи устанавливаем состояние ошибки.
                                    this.ServerState = ServerState.Error;
                                    break;
                                }
                            }
                        }
                        /* ====================================================================================== */
                        #endregion
                    }
                    /* ====================================================================================== */
                    #endregion

                    #region [ 2.2. ПОЛУЧЕНИЕ СПИСКА СТРУКТУР ТИПОВ ДАННЫХ УДАЛЕННОГО УСТРОЙСТВА ]
                    /* ====================================================================================== */
                    if (this.ServerState == ServerState.Init)
                    {
                        // Получаем полный доступный список кодов структур типов данных удаленного устройства.
                        List<UInt16> templateInstances;

                        if (this.Device.GetTemplateAddreses(out templateInstances))
                        {
                            // Для каждого кода структуры типа данных производим получение информации.
                            foreach (UInt16 templateInstance in templateInstances)
                            {
                                // Получаем информацию о структуре типа данных.
                                // В случае успешного получения данных проихводим получение информации о членах структуры типа данных,
                                // в случае неудачи устанавливаем ошибочное состояние.
                                CLXTemplate clxTemplate;
                                if (this.Device.GetTemplateInformation(templateInstance, out clxTemplate))
                                {
                                    // Получаем информацию о членах структуры типа данных,
                                    // в случае неудачи устанавливаем ошибочное состояние.
                                    if (this.Device.GetTemplateMembers(clxTemplate))
                                    {
                                        // В Случае успешного получения информации добаляем ип данных в список.
                                        if (clxTemplate.SymbolTypeAttribute.Code != 0 && !this.AvaliableTemplateStructures.ContainsKey(clxTemplate.SymbolTypeAttribute.Code))
                                        {
                                            this.AvaliableTemplateStructures.Add(clxTemplate.SymbolTypeAttribute.Code, clxTemplate);
                                        }
                                    }
                                    else
                                    {
                                        Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, "Template Members Reading", "Error! Imposible to read template Members from Device. Instance: " + templateInstance.ToString()));
                                        this.ServerState = ServerState.Error;
                                    }
                                }
                                else
                                {
                                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, "Template Info Reading", "Error! Imposible to read template Information from Device. Instance: " + templateInstance.ToString()));
                                    this.ServerState = ServerState.Error;
                                }
                            }
                        }
                        else
                        {
                            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, "Template List Reading", "Error! Imposible to read template Instances from Device."));
                            this.ServerState = ServerState.Error;
                        }
                    }
                    /* ====================================================================================== */
                    #endregion

                    #region [ 2.3. ИНИЦИАЛИЗАЦИЯ ПЕРЕМЕННЫХ ]
                    /* ====================================================================================== */

                    #region [ 2.3.1 ПОЛУЧЕНИЕ ТИПА ДАННЫХ ТЭГОВ ПО ИХ ИМЕНИ ]
                    /* ====================================================================================== */
                    // Инициализируем состояние тэгов перед чтением.
                    foreach (LogixTagHandler tag in mainProcessTags.Keys)
                    {
                        // Если удалось определить тип данных то присваиваем его текущему тэгу.
                        // В противном случае выводим сообщение о неудаче.
                        if (!this.DefineTagDataType(tag))
                        {
                            tag.ReadEnable = false;
                            tag.WriteEnable = false;
                            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, "Tag Definition", "Error! Invalid Name or Imposible to define Data Type of Tag: " + tag.Name));
                        }
                    }
                    /* ====================================================================================== */
                    #endregion

                    /* ====================================================================================== */
                    #endregion

                    // Устанавливам состояние запуска.
                    if (this.ServerState == ServerState.Init)
                    {
                        this.ServerState = ServerState.Run;
                    }
                }
                /* ====================================================================================== */
                #endregion

                #region [ 3. READING/WRITING OF TAG VALUES ]
                /* ====================================================================================== */
                if (this.processRunEnable && this.ServerState == ServerState.Run)
                {
                    // Промежуточный список тэгов для внутренних действий.
                    IEnumerable<LogixTagHandler> currentTags;
                    // Тэги получившие успешный результат после операции чтения.
                    List<LogixTagHandler> readedTags = new List<LogixTagHandler>();
                    // Тэги получившие успешный результат после операции записи.
                    List<LogixTagHandler> writedTags = new List<LogixTagHandler>();

                    #region [ 3.1. ФИКСАЦИЯ ЗНАЧЕНИЙ - ПЕРЕД ЧТЕНИЕМ/ЗАПИСЬЮ ]
                    /* ====================================================================================== */
                    // Предварительно фиксируем отчеты записи и чтения значений.
                    foreach (LogixTagHandler t in this.mainProcessTags.Keys.ToList())
                    {
                        this.mainProcessTags[t] = new ReportContainer(t);
                    }
                    /* ====================================================================================== */
                    #endregion

                    #region [ 3.2. ЧТЕНИЕ ЗНАЧЕНИЯ ТЭГА ]
                    /* ====================================================================================== */
                    IEnumerable<LogixTagHandler> simpleReadingTags = mainProcessTags.Keys.Where(t =>
                        t.ReadMethod == TagReadMethod.Simple
                        && t.ReadEnable
                        && t.Type.Family != TagDataTypeFamily.Null);

                    IEnumerable<LogixTagHandler> tableReadingTags = mainProcessTags.Keys.Where(t =>
                        t.ReadMethod == TagReadMethod.Table
                        && t.ReadEnable
                        && t.Type.Family != TagDataTypeFamily.Null);

                    IEnumerable<LogixTagHandler> fragmentReadingTags = mainProcessTags.Keys.Where(t =>
                        t.ReadMethod == TagReadMethod.Fragmet
                        && t.ReadEnable
                        && t.Type.Family != TagDataTypeFamily.Null);

                    #region [ 3.2.1 ЧТЕНИЕ - ОТКРЫТЫЙ МЕТОД ]
                    /* ====================================================================================== */
                    currentTags = simpleReadingTags.Where(t => t.NeedToRead);
                    if (currentTags.Count() == 1)
                    {
                        this.Device.ReadTag(currentTags.ElementAt(0));
                    }
                    else if (currentTags.Count() > 1)
                    {
                        this.Device.ReadTags(currentTags.Select(t => (LogixTag)t).ToList());
                    }
                    /* ====================================================================================== */
                    #endregion

                    #region [ 3.2.2 ЧТЕНИЕ - МЕТОД ТАБЛИЧНОГО ЧТЕНИЯ ]
                    /* ====================================================================================== */
                    /* 1. ДОБАВЛЕНИЕ ТЭГА В ТАБЛИЦУ */

                    // Получаем все атомарные тэги которые необходимо разместить в таблице для групового чтения.
                    currentTags = tableReadingTags.Where(t => t.OwnerTableItem == null);
                    foreach (LogixTagHandler tag in currentTags)
                    {
                        CLXCustomTagMemoryTable table;

                        if (!this.Device.IsConnected)
                        {
                            break;
                        }

                        // Получаем текущий размер в байтах который будет занят под возврат информации для данного тэга.
                        int headerPacketSize = 4;
                        int expectedResponsedTagSizeInTable = 2 + tag.Type.TotalSize;
                        int maxValidPacketSizeResponse = this.Device.MaxPacketSizeTtoO;
                        int encapsulatedHeader = 4;

                        // Ищмем подходящую таблицу со свободным местом для размещения текущего тэга.
                        List<CLXCustomTagMemoryTable> validTables = this.tables.Where(t =>
                            encapsulatedHeader + headerPacketSize + t.ExpectedTotalSize + expectedResponsedTagSizeInTable < maxValidPacketSizeResponse).ToList();

                        // В случае если подходящей таблицы не найдено то создаем новую.
                        // В противном случае работаем далее с найденной таблицей.
                        if (validTables.Count == 0)
                        {
                            // Запрашиваем создание новой таблицы у удаленного устройства (контроллера).
                            if (Device.CreateTagReadingTable(out table))
                            {
                                // Добавляем таблицу в коллекцию.
                                this.tables.Add(table);
                            }
                        }
                        else
                        {
                            // Получаем пеовую таблицу из существующей коллекции где имеется возможность для добавления нового тэга.
                            table = validTables[0];
                        }

                        if (table != null)
                        {
                            // Добавление тэга в таблицу в удаленном устройстве (контроллера) и синхронизация локальной.
                            Device.AddTagToReadingTable(table, tag);
                        }
                        else
                        {
                            // Усли таблицу получить не удалось то устанавливаем ошибочное состояние.
                            this.ServerState = ServerState.Error;
                        }
                    }

                    /* 2. ЧТЕНИЕ ТАБЛИЦЫ ДАННЫХ */

                    // Читаем те таблицы где хотя бы один тэг требует обновления.
                    foreach (CLXCustomTagMemoryTable table in this.tables.Where(j =>
                        j.Items.Any(h => h.Tag is LogixTagHandler && ((LogixTagHandler)h.Tag).NeedToRead)))
                    {
                        if (!this.Device.IsConnected)
                        {
                            break;
                        }

                        Device.ReadTagReadingTable(table);
                    }
                    /* ====================================================================================== */
                    #endregion

                    #region [ 3.2.3 ЧТЕНИЕ - ФРАГМЕНТАРНЫЙ МЕТОД ]
                    /* ====================================================================================== */
                    currentTags = fragmentReadingTags.Where(t => t.NeedToRead);
                    if (currentTags.Count() > 0)
                    {
                        // Массивы.
                        List<LogixTagHandler> readArrayTags = currentTags.Where(t => t.NeedToRead).ToList();
                        // Для каждого тэга производим операцию записи.
                        for (int ix = 0; ix < readArrayTags.Count; ix++)
                        {
                            this.Device.ReadTagFragment(readArrayTags[ix]);
                        }
                    }
                    /* ====================================================================================== */
                    #endregion

                    /* ====================================================================================== */
                    #endregion

                    #region [ 3.3. ЗАПИСЬ ЗНАЧЕНИЯ ТЭГА ]
                    /* ====================================================================================== */
                    currentTags = mainProcessTags.Keys.Where(t => t.WriteEnable && t.NeedToWrite);
                    List<LogixTagHandler> allWritingTags = new List<LogixTagHandler>();

                    // Проверяем что запрошенные тэги имеют знаяения зля записи.
                    foreach (LogixTagHandler tag in currentTags)
                    {
                        if (tag.WriteValue.RequestedData == null)
                            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, "Tag Value Write", "Error! Tag Value is Null. Tag: " + tag.Name));
                        else
                            allWritingTags.Add(tag);
                    }

                    IEnumerable<LogixTagHandler> simpleNumericWritingTags = allWritingTags.Where(t =>
                        t.WriteMethod == TagWriteMethod.Simple
                        && (t.Type.Family == TagDataTypeFamily.AtomicBool
                            || t.Type.Family == TagDataTypeFamily.AtomicFloat
                            || t.Type.Family == TagDataTypeFamily.AtomicInteger)
                        && t.Type.AtomicBitDefinition == null);

                    IEnumerable<LogixTagHandler> simpleBitWritingTags = allWritingTags.Where(t =>
                        t.WriteMethod == TagWriteMethod.Simple
                        && (t.Type.Family == TagDataTypeFamily.AtomicInteger && t.Type.AtomicBitDefinition != null
                            || t.Type.Family == TagDataTypeFamily.AtomicBoolArray && t.Type.BitArrayDefinition != null));

                    IEnumerable<LogixTagHandler> fragmentWritingTags = allWritingTags.Where(t =>
                        t.WriteMethod == TagWriteMethod.Fragmet
                        && t.Type.Family != TagDataTypeFamily.Null);


                    #region [ 3.3.1 ЗАПИСЬ - ПРОСТОЙ МЕТОД (ЗАПИСЬ ЗНАЧЕНИЯ)]
                    /* ====================================================================================== */
                    // Запись с использованием однократного запроса.
                    if (simpleNumericWritingTags.Count() == 1)
                    {
                        this.Device.WriteTag(simpleNumericWritingTags.ElementAt(0));
                    }
                    // Запись с использованием мультизапроса.
                    else if (simpleNumericWritingTags.Count() > 1)
                    {
                        this.Device.WriteTags(simpleNumericWritingTags.Select(t => (LogixTag)t).ToList());
                    }
                    /* ====================================================================================== */
                    #endregion

                    #region [ 3.3.2 ЗАПИСЬ - ПРОСТОЙ МЕТОД (МОДИФИКАЦИЯ БИТОВ)]
                    /* ====================================================================================== */
                    //
                    foreach (LogixTagHandler tag in simpleBitWritingTags)
                    {
                        if (!this.Device.IsConnected) break;

                        // Создаем маски для модификации значений OR/AND.
                        byte[] mask_or = new byte[tag.Type.TotalSize];
                        byte[] mask_and = new byte[tag.Type.TotalSize];

                        // Производим инициализацию масок OR/AND.
                        for (int ix = 0; ix < tag.Type.TotalSize; ix++)
                        {
                            mask_or[ix] = 0x00;
                            mask_and[ix] = 0xFF;
                        }

                        BitOffsetPosition bitpos = null;

                        if (tag.Type.Family == TagDataTypeFamily.AtomicInteger)
                        {
                            bitpos = new BitOffsetPosition(BytesInElement.OneByte);
                            bitpos.TotalBitOffset = tag.Type.AtomicBitDefinition.BitOffset.Value;
                        }
                        else if (tag.Type.Family == TagDataTypeFamily.AtomicBoolArray)
                        {
                            bitpos = new BitOffsetPosition(BytesInElement.OneByte);
                            bitpos.TotalBitOffset = tag.Type.BitArrayDefinition.BitOffset.Value;
                        }

                        if (bitpos != null)
                        {
                            if ((int)bitpos.ElementOffset <= tag.Type.TotalSize)
                            {
                                if (tag.WriteValue.RequestedData[0].Any(c => c != 0))
                                {
                                    // Bit == 1;
                                    mask_or[(int)bitpos.ElementOffset] = (byte)(0x01 << (int)bitpos.BitOffset.Value);
                                }
                                else
                                {
                                    // Bit == 0;
                                    mask_and[(int)bitpos.ElementOffset] = (byte)(~(0x01 << (int)bitpos.BitOffset.Value));
                                }

                                this.Device.ReadModifyWriteTag(tag, mask_or, mask_and);
                            }
                        }
                    }
                    /* ====================================================================================== */
                    #endregion

                    #region [ 3.3.3 ЗАПИСЬ - ФРАГМЕНТАРНЫЙ МЕТОД ]
                    /* ====================================================================================== */
                    // Запись каждого из тэгов фрагментарным методом.
                    foreach (LogixTagHandler tag in fragmentWritingTags)
                    {
                        if (!this.Device.IsConnected)
                        {
                            break;
                        }

                        this.Device.WriteTagFragment(tag);
                    }
                    /* ====================================================================================== */
                    #endregion

                    // Для всех тэгов которые требовали запись значений и имеющие нулевой период обновления (однократная запись) в данном цикле
                    // устанавливаем запрет на дальнейшее разрешение записи.
                    foreach (LogixTagHandler t in currentTags)
                    {
                        if (t.WriteUpdateRate == 0)
                            t.WriteEnable = false;
                    }
                    /* ====================================================================================== */
                    #endregion

                    #region [ 3.4. ФИКСАЦИЯ ЗНАЧЕНИЙ - ПОСЛЕ ЧТЕНИЯ/ЗАПИСИ ]
                    /* ====================================================================================== */
                    // Фиксируем отчеты записи и чтения значений после соответствующий операций.
                    foreach (LogixTagHandler t in this.mainProcessTags.Keys)
                    {
                        ReportContainer reportContainer = this.mainProcessTags[t];
                        if (t.ReadValue.Report != null
                            && t.ReadValue.Report.IsSuccessful == true
                            && t.ReadValue.Report != reportContainer.ReadReport)
                        {
                            readedTags.Add(t);
                        }

                        if (t.WriteValue.Report != null
                            && t.WriteValue.Report.IsSuccessful == true
                            && t.WriteValue.Report != reportContainer.WriteReport)
                        {
                            writedTags.Add(t);
                        }
                    }
                    /* ====================================================================================== */
                    #endregion

                    #region [ 3.5 ГЕНЕРАЦИЯ СОБЫТИЙ ЧТЕНИЯ / ЗАПИСИ ]
                    /* ====================================================================================== */

                    //
                    if (readedTags.Count > 0)
                    {
                        Event_TagsValueWasReaded(readedTags);
                    }

                    //
                    if (writedTags.Count > 0)
                    {
                        Event_TagsValueWasWrited(writedTags);
                    }

                    // Получаем все тэги значения которых было изменено в результате чтения из удаленного
                    // устройства с момента последнего чтения.

                    List<LogixTagHandler> tagsWithNewValues = readedTags.Where(t => t.ReadValue.Report.ValueChanged == true).ToList();
                    // Если кол-во тэгов с новым измененным значением отлично от нуля то вызываем событие
                    // изменения значения тэгов.
                    if (tagsWithNewValues.Count > 0)
                    {
                        Event_TagsValueWasChanged(tagsWithNewValues);
                    }
                    /* ====================================================================================== */
                    #endregion
                }
                /* ====================================================================================== */
                #endregion

                #region [ 4. DISCONNESTION FROM SERVER. ]
                /* ====================================================================================== */
                // В случае закрытия разрешения подключения пытаемся поэтапно произвести действия с удаленным устройством
                // при условии что сохранено подключение TCP/IP.
                // А именно: 
                //      1. ForwardClose         - закрытие подключения.
                //      2. Unregister Session   - закрытие сессии.
                //      3. Tcp Disconnection    - отключение TCP/IP.
                // При обрыве подключения TCP/IP устанавливаем ошибочное подключение и начинаем процесс подключения заново.
                if (!this.processRunEnable)
                {
                    if (this.ServerState != ServerState.Off)
                    {
                        if (this.Device.IsConnected)
                        {
                            this.Device.ForwardClose();
                            this.Device.UnregisterSession();
                            this.Device.Disconnect();
                        }

                        this.ServerState = ServerState.Off;
                        Thread.Sleep(1000);
                    }
                }

                // При запросе на отключение производим очистку списка тэгов процесса и
                // производим процесс завершения.
                if (!this.processRunRequest && this.processRunEnable)
                {
                    // Освобождаем ранее зарезервированные таблицы в удаленном устройстве.
                    foreach (CLXCustomTagMemoryTable table in this.TablesInProcess)
                    {
                        foreach (CLXCustomTagMemoryItem item in table.Items)
                        {
                            this.Device.RemoveTagFromReadingTable(table, item);
                        }
                    }

                    // Инициализируем состояние всех тэгов.
                    foreach (LogixTagHandler t in this.mainProcessTags.Keys)
                    {
                        t.InitState();
                        t.OwnerTableItem = null;
                        t.OwnerTask = null;
                    }

                    // Очищаем список таблиц.
                    this.tables.Clear();
                    // Очищаем список тэгов процесса.
                    this.mainProcessTags.Clear();
                    this.processRunEnable = false;
                }
                /* ====================================================================================== */
                #endregion

                Thread.Sleep(1);
            }
        }

        #region [ PUBLIC METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Запускает процесс циклического чтения данных для держателей тэга.
        /// </summary>
        /// <param name="tags">Текущий держатель тэга удаленного устройства.</param>
        /// <returns></returns>
        public bool Begin(List<LogixTagHandler> tags)
        {
            if (tags == null || tags.Count <= 0)
            {
                return false;
            }

            if (!this.processRunRequest && !this.processRunEnable)
            {
                foreach (LogixTagHandler t in tags)
                {
                    if (!this.mainProcessTags.ContainsKey(t))
                    {
                        this.mainProcessTags.Add(t, new ReportContainer(t));
                    }
                }

                this.processRunRequest = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Завершает процесс циклического чтения для всех держателей тэга.
        /// </summary>
        public void Finish()
        {
            this.processRunRequest = false;
        }

        /// <summary>
        /// Проверяет, содержится ли текущий объект тэга в списке для обработки.
        /// </summary>
        /// <param name="tag">Объект тэга.</param>
        /// <returns></returns>
        public bool ContainsTagObject(LogixTagHandler tag)
        {
            if (tag == null)
            {
                return false;
            }

            return this.mainProcessTags.ContainsKey(tag);
        }
        /* ======================================================================================== */
        #endregion

        #region [ PRIVATE METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Запускает основной процесс обработки.
        /// </summary>
        private void RunTread()
        {
            if (this.ThreadRunned)
            {
                return;
            }

            this.runCyclicProcess = true;
            this.thread = new Thread(new ThreadStart(Process));
            this.thread.IsBackground = true;
            this.thread.Start();
        }
        /// <summary>
        /// Останавливает основной процесс обработки.
        /// </summary>
        private void StopThread()
        {
            this.runCyclicProcess = false;
        }

        /// <summary>
        /// Производит попытку определения размера данных значения по его заданному коду.
        /// </summary>
        /// <param name="typeCode">Код типа данных.</param>
        /// <param name="size">Результат : Размер данный в байтах.</param>
        /// <returns>В случае успеха выполняемой операции возвращает True.</returns>
        private bool TryGetSizeByTypeCode(UInt16 typeCode, out UInt16 size)
        {
            size = 0;

            if (this.AvaliableTemplateStructures.ContainsKey(typeCode))
            {
                UInt32 sizeOfStructure = this.AvaliableTemplateStructures[typeCode].SizeOfStructure;

                if (sizeOfStructure > 0xFFFF || sizeOfStructure == 0)
                {
                    return false;
                }

                size = (UInt16)sizeOfStructure;
                return true;
            }
            else
            {
                switch (typeCode)
                {
                    case 0xC1:
                    case 0xC2:
                        {
                            size = 1;
                            return true;
                        }
                    case 0xC3:
                        {
                            size = 2;
                            return true;
                        }

                    case 0xC4:
                    case 0xCA:
                    case 0xD3:
                        {
                            size = 4;
                            return true;
                        }

                    case 0xC5:
                        {
                            size = 8;
                            return true;
                        }
                }
            }

            return false;
        }
        /// <summary>
        /// Производит подготовку параметров тэга для его дальнейшего использования.
        /// Для текущего тэга производится : генерация пути по имени, определение параметров типа данных.
        /// </summary>
        /// <param name="logixTag">Держатель тэга удаленного устройства.</param>
        /// <returns></returns>
        private bool DefineTagDataType(LogixTagHandler logixTag)
        {
            string typeName = null;                         // Текущее название типа данных.
            string hiddenMemberName = null;                 // Текущее имя скрытого члена структуры типа данных к которой относится бит (его место определения в данном члене).
            UInt16 typeCode = 0;                            // Текущий код типа данных.
            BitOffsetPosition atomicBitDefinition = null;   // Текущее значение позиции байта/бита атомарного численного типа данных.
            BitOffsetPosition structureDefinition = null;   // Текущее значение позиции байта/бита структурного типа данных.
            BitOffsetPosition bitArrayDefinition = null;    // Текущее значение позиции байта/бита битового массива типа данных.
            bool isBitArray;                                // Текущее значение характеризующее что данный тип данных является битовым массивом.
            UInt16 size = 0;                                // Текущее значение размера типа данных.

            // Присваивает начальные значения возвращаемым параметрам.
            bool isProgramTag = false;                      // Показывает на что данный тэг является программным.
            EPath epath = null;                             // Определение пути для текущего тэга.
            UInt32? arrayIndex0 = null;                     // Индекс массива размерности 0.
            UInt32? arrayIndex1 = null;                     // Индекс массива размерности 1.
            UInt32? arrayIndex2 = null;                     // Индекс массива размерности 2.
            ArrayDefinition arrayDefinition = new ArrayDefinition();

            if (logixTag == null)
            {
                return false;
            }

            // Предварительно проверяем корректность запрашиваемого имени тэга.
            if (logixTag.Name == null)
            {
                return false;
            }

            // Делим имя запрашиваемого тэга фрагменты разделенные точкой ".".
            string[] nameParts = logixTag.Name.Split(".".ToCharArray());
            // Создаем новый объект пути в удаленном устройстве для текущего тэга.
            epath = new EPath();
            // По названию тэга определяем является ли он программным.
            isProgramTag = nameParts.Length > 1 && nameParts[0].StartsWith("Program:");
            // Имя программы (если задана).
            string programName = (isProgramTag ? nameParts[0] : null);

            for (int ix = 0; ix < nameParts.Length; ix++)
            {
                bool isProgramPartIndex = (isProgramTag && ix == 0);    // Возвращает True если индекс сегмента имени является названием программы.
                bool isTagPartIndex = (ix == (isProgramTag ? 1 : 0));   // Возвращает True если индекс сегмента имени является названием тэга.
                bool isMemberPartIndex = (ix > (isProgramTag ? 1 : 0)); // Возвращает True если индекс сегмента имени является названием члена структуры или номером бита.
                bool isLastPartIndex = (ix == nameParts.Length - 1);    // Возвращает True если индекс сегмента имени является последним.
                string currentPartName = nameParts[ix];                 // Текущее значение имени сегмента имени по текущему индексу.

                int currentArrayIndexRank = 0;                          // Текущее значение размерности массива для текущего имени структуры может быть от 0 до 3.
                arrayIndex0 = null;                                     // Индекс элеменат массива размерности 0.
                arrayIndex1 = null;                                     // Индекс элеменат массива размерности 1.
                arrayIndex2 = null;                                     // Индекс элеменат массива размерности 2.
                bool isNumericPartName;                                 // Текущее значение в случае True означающее что текущий член имени тэга является числом.

                #region [ ARRAY INDEXES EXTRACTION ]
                /* ====================================================================================== */
                // Проверяем, содержится ли в текущем имени члена структуры символ "[".
                // При наличии такого символа делаем вывод что объялвен индекс массива вида "tagname[1,2,3]" или "tagname[8]".
                if (currentPartName.Contains("["))
                {
                    string[] splittedParts;

                    // Проверяем что последний символ является "]" и длина строки больше 3 (минимум при объявлении массива может быть 4 символа например: "a[1]").
                    if (currentPartName[currentPartName.Length - 1] != ']' || currentPartName.Length <= 3)
                    {
                        return false;
                    }
                    // Выделяем из строки все символы кроме последнего ']'.
                    currentPartName = currentPartName.Substring(0, currentPartName.Length - 1);

                    // Делим строку текущего члена имени структуры по символу начала объявления массива '['.
                    splittedParts = currentPartName.Split("[".ToCharArray());

                    // Проверяем что после разделения по символу '[' имеется только две части.
                    if (splittedParts.Length != 2)
                    {
                        return false;
                    }

                    // Если имя является объявлением индекса массива то выделяем строку находящуюся до символа '['.
                    currentPartName = splittedParts[0];

                    // Делим часть строки объявленную между имволами '[' и ']' на запятые разделяющие элеементы размерности массива.
                    splittedParts = splittedParts[1].Split(",".ToCharArray());

                    // Проверяем что в получившемся имени для первого члена имени тэга имеется размерность от 1 до 3,
                    // или для последующих элементов (если член структуры) имени только 1 размерность.
                    if (!(((splittedParts.Length > 0 && splittedParts.Length <= 3) && isTagPartIndex)
                        || (splittedParts.Length == 1 && !isTagPartIndex)))
                    {
                        return false;
                    }

                    // Проверяем что полученные индексы размерностей все являются цифрами.
                    if (splittedParts.Any(i => !i.All(c => Char.IsDigit(c))))
                    {
                        return false;
                    }

                    // Присваиваем текущие индексы массива возвращаемым переменным.
                    currentArrayIndexRank = splittedParts.Length;

                    // Обработка размерности 0.
                    if (currentArrayIndexRank >= 1)
                    {
                        arrayIndex0 = Convert.ToUInt16(splittedParts[0]);
                    }
                    // Обработка размерности 1.
                    if (currentArrayIndexRank >= 2)
                    {
                        arrayIndex1 = Convert.ToUInt16(splittedParts[1]);
                    }
                    // Обработка размерности 2.
                    if (currentArrayIndexRank == 3)
                    {
                        arrayIndex2 = Convert.ToUInt16(splittedParts[2]);
                    }
                    // Обработка превышения размерности
                    if (currentArrayIndexRank > 3)
                    {
                        return false;
                    }
                }
                /* ====================================================================================== */
                #endregion

                #region [ PARAMETERS INIT ]
                /* ====================================================================================== */
                // Проверяем что текущее имя члена структуры содержит символы отличные от пробела.
                if (currentPartName.Trim() == "")
                {
                    return false;
                }

                // Определяем, являются ли число текущее имя тэга.
                isNumericPartName = currentPartName.All(c => Char.IsDigit(c));

                if (!isNumericPartName)
                {
                    atomicBitDefinition = null;
                    structureDefinition = null;
                    bitArrayDefinition = null;

                    arrayDefinition.Dim0 = 0;
                    arrayDefinition.Dim1 = 0;
                    arrayDefinition.Dim2 = 0;
                    hiddenMemberName = null;
                }
                else if (!isLastPartIndex)
                {
                    return false;
                }
                /* ====================================================================================== */
                #endregion

                #region [ PART NAME : "PROGRAM NAME" ]
                /* ====================================================================================== */
                if (isProgramPartIndex)
                {
                    #region [ СОЗДАНИЕ EPATH ]
                    /* ====================================================================================== */
                    // Добавляем в путь EPath новый сегмент с текущим именем.
                    epath.Segments.Add(new EPathSegment(currentPartName));
                    /* ====================================================================================== */
                    #endregion
                }
                /* ====================================================================================== */
                #endregion

                #region [ PART NAME : "TAG NAME" ]
                /* ====================================================================================== */
                if (isTagPartIndex)
                {
                    // Проверяем что текущее заданное имя полностью стостоит из символов.
                    if (isNumericPartName)
                    {
                        return false;
                    }

                    #region [ ПОИСК ДАННОГО ЭЛЕМЕНТА В СПИСКЕ ТЭГОВ ПО ИМЕНИ ]
                    /* ====================================================================================== */
                    // Текущее имя тэга. В случае если тэг является программным то формируется имя "Program:ИМЯ_ПРОГРАММЫ.ИМЯ_ТЭГА".
                    string tagname = (isProgramTag ? programName + "." + currentPartName : currentPartName);

                    // Если текущий индекс первый (=0) то ищем по имени CLXTag удаленного устройства
                    // и зпоминаем его код типа данных.
                    if (!this.AvaliableControllerTags.ContainsKey(tagname))
                    {
                        return false;
                    }

                    CLXTag clxTag = this.AvaliableControllerTags[tagname];
                    // Получаем Код типа данных соответствующего текущему элементу имени.
                    typeCode = clxTag.SymbolTypeAttribute.Code;
                    // Определяем по типу данных является ли данный тип битовым массивом.
                    isBitArray = (typeCode == 0x00D3);

                    #region [ ПРОВЕРКА ИНДЕКСОВ ЭЛЕМЕНТА МАССИВА ]
                    /* ====================================================================================== */
                    // Проверяем что размерность индексов массива текущего элемента равна размерности 
                    // его определения в тэге удаленного устройства. Считается что Определение тэга может принимать
                    // значения только 0,1,2 или 3.
                    if (currentArrayIndexRank > 0)
                    {
                        if (clxTag.ArrayRank != currentArrayIndexRank)
                        {
                            return false;
                        }

                        // Размерность битового массива характеризует кол-во 32-разрядных слов
                        // Индекс массива характеризует позицию бита в последовательности.
                        // Проверяем размерность 0 массива тэга на критерий превышения значения индекса.
                        if (currentArrayIndexRank >= 1 && clxTag.ArrayDim0 * (isBitArray ? 32 : 1) <= arrayIndex0) return false;
                        // Проверяем размерность 1 массива тэга на критерий превышения значения индекса.
                        if (currentArrayIndexRank >= 2 && clxTag.ArrayDim1 * (isBitArray ? 32 : 1) <= arrayIndex1) return false;
                        // Проверяем размерность 2 массива тэга на критерий превышения значения индекса.
                        if (currentArrayIndexRank == 3 && clxTag.ArrayDim2 * (isBitArray ? 32 : 1) <= arrayIndex2) return false;
                        // Проверяем что размерность индекса не превышает 3.
                        if (currentArrayIndexRank > 3) return false;
                    }
                    /* ====================================================================================== */
                    #endregion

                    if (currentArrayIndexRank > 0)
                    {
                        #region [ ЧАСТНЫЙ СЛУЧАЙ : ТЕКУЩИЙ ТИП ДАННЫХ ЯВЛЯЕТСЯ БИТОВЫМ МАССИВОМ ]
                        /* ====================================================================================== */
                        if (isBitArray)
                        {
                            UInt32 arrayLinearIndex = 0;

                            // На основании данных о размере массива и о индексе элемента производим преобразование
                            // в линейное значение индекса. Следим за тем чтобы не произошло переполнение UInt32.

                            try
                            {
                                checked
                                {
                                    // Создаем линейный индекс массива для текущего элемента.
                                    // Для индекса размерности 1.
                                    if (currentArrayIndexRank >= 1) arrayLinearIndex = arrayIndex0.Value;
                                    // Для индекса размерности 2.
                                    if (currentArrayIndexRank >= 2) arrayLinearIndex += arrayIndex1.Value * clxTag.ArrayDim0;
                                    // Для индекса размерности 3.
                                    if (currentArrayIndexRank == 3) arrayLinearIndex += arrayIndex2.Value * clxTag.ArrayDim1 * clxTag.ArrayDim0;
                                }
                            }
                            catch
                            {
                                return false;
                            }

                            // Создаем определение смещения для битового массива.
                            bitArrayDefinition = new BitOffsetPosition(BytesInElement.FourBytes);
                            bitArrayDefinition.TotalBitOffset = arrayLinearIndex;
                        }
                        /* ====================================================================================== */
                        #endregion
                    }
                    else
                    {
                        #region [ МАССИВ ИЛИ ПРОСТОЙ ТЭГ ]
                        /* ====================================================================================== */
                        // Присваиваем текущей длине фрагмента значение размерности массива из полученного 
                        // найденного тэга.
                        arrayDefinition.Dim0 = clxTag.ArrayDim0;
                        arrayDefinition.Dim1 = clxTag.ArrayDim1;
                        arrayDefinition.Dim2 = clxTag.ArrayDim2;
                        /* ====================================================================================== */
                        #endregion
                    }
                    /* ====================================================================================== */
                    #endregion

                    #region [ СОЗДАНИЕ EPATH ]
                    /* ====================================================================================== */
                    // Добавляем в путь EPath новый сегмент с текущим именем.
                    epath.Segments.Add(new EPathSegment(currentPartName));

                    if (currentArrayIndexRank >= 1)
                        epath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_MemberID, isBitArray ? arrayIndex0.Value / 32 : arrayIndex0.Value));
                    if (currentArrayIndexRank >= 2)
                        epath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_MemberID, isBitArray ? arrayIndex1.Value / 32 : arrayIndex1.Value));
                    if (currentArrayIndexRank >= 3)
                        epath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_MemberID, isBitArray ? arrayIndex2.Value / 32 : arrayIndex2.Value));

                    /* ====================================================================================== */
                    #endregion
                }
                /* ====================================================================================== */
                #endregion

                #region [ PART NAME : "MEMBER NAME" ]
                /* ====================================================================================== */
                if (isMemberPartIndex)
                {
                    #region [ ПОИСК ДАННОГО ЭЛЕМЕНТА В СПИСКЕ СТРУКТУР ПО TYPE CODE ]
                    /* ====================================================================================== */
                    if (!isNumericPartName)
                    {
                        // Обнуляем каждый раз текущее название элемента структуры - держателя для текущего элемента являющегося байтом (BOOL, 0xC1).
                        hiddenMemberName = null;

                        // Если текущий индекс не первый (>0) то ищем по коду типа данных CLXTemplate удаленного устройства
                        if (!this.AvaliableTemplateStructures.ContainsKey(typeCode))
                        {
                            return false;
                        }

                        CLXTemplateMember currentTemplateMember;
                        if (!this.AvaliableTemplateStructures[typeCode].GetMember(currentPartName, out currentTemplateMember))
                        {
                            return false;
                        }

                        // Получаем Код типа данных соответствующего текущему элементу имени.
                        typeCode = currentTemplateMember.SymbolTypeAttribute.Code;
                        // Определяем по типу данных является ли данный тип битовым массивом.
                        isBitArray = (typeCode == 0x00D3);

                        // Получаем позицию байта/бита соответствующего текущему элементу имени.
                        structureDefinition = new BitOffsetPosition(BytesInElement.OneByte);
                        structureDefinition.ElementOffset = currentTemplateMember.Offset;
                        structureDefinition.BitOffset = currentTemplateMember.BitPosition;

                        // Если текущий индекс последний и элемент является типом BOOL с кодом 0xC1,
                        // то получаем 
                        if (isLastPartIndex && currentTemplateMember.CorrespondedHiddenMember != null)
                        {
                            hiddenMemberName = currentTemplateMember.CorrespondedHiddenMember.Name;
                        }

                        #region [ ПРОВЕРКА ИНДЕКСА МАССИВА ]
                        /* ====================================================================================== */
                        if (currentArrayIndexRank > 0)
                        {
                            // Если у текущего типа данных объявлен массив, то проверяем что
                            // запрашиваемый ранк равен 1, т.к. в элементе структуры ранк может быть только 1.
                            if (currentArrayIndexRank != 1)
                            {
                                return false;
                            }

                            // Проверяем что запрашиваемый индекс не превышает размерность массива 
                            // текущего типа данных.
                            if (currentTemplateMember.ArrayDimension * (isBitArray ? 32 : 1) <= arrayIndex0)
                            {
                                return false;
                            }
                        }
                        /* ====================================================================================== */
                        #endregion

                        if (currentArrayIndexRank > 0)
                        {
                            #region [ ЧАСТНЫЙ СЛУЧАЙ : ТЕКУЩИЙ ТИП ДАННЫХ ЯВЛЯЕТСЯ БИТОВЫМ МАССИВОМ ]
                            /* ====================================================================================== */
                            if (isBitArray)
                            {
                                UInt32 arrayLinearIndex = 0;
                                // Для индекса размерности 1.
                                if (currentArrayIndexRank >= 1) arrayLinearIndex = arrayIndex0.Value;

                                // Создаем определение смещения для битового массива.
                                bitArrayDefinition = new BitOffsetPosition(BytesInElement.FourBytes);
                                bitArrayDefinition.TotalBitOffset = arrayLinearIndex;
                            }
                            /* ====================================================================================== */
                            #endregion
                        }
                        else
                        {
                            #region [ МАССИВ ИЛИ ПРОСТОЙ ТЭГ ]
                            /* ====================================================================================== */
                            if (currentTemplateMember.ArrayDimension != null)
                            {
                                // Присваиваем текущей длине фрагмента значение размерности массива из полученного 
                                // члена найденной структуры.
                                arrayDefinition.Dim0 = currentTemplateMember.ArrayDimension.Value;
                                arrayDefinition.Dim1 = 0;
                                arrayDefinition.Dim2 = 0;
                            }
                            /* ====================================================================================== */
                            #endregion
                        }

                        #region [ СОЗДАНИЕ EPATH ]
                        /* ====================================================================================== */
                        if (!isNumericPartName)
                        {
                            // Добавляем в путь EPath новый сегмент с текущим именем.
                            epath.Segments.Add(new EPathSegment(currentPartName));

                            if (currentArrayIndexRank >= 1)
                                epath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_MemberID, isBitArray ? arrayIndex0.Value / 32 : arrayIndex0.Value));
                        }
                        /* ====================================================================================== */
                        #endregion
                    }
                    /* ====================================================================================== */
                    #endregion

                    #region [ НОМЕР БИТА АТОМАРНОГО ЧИСЛА ]
                    /* ====================================================================================== */
                    if (isNumericPartName)
                    {
                        if (!isLastPartIndex)
                        {
                            return false;
                        }

                        // Получаем размер в байтах для данного элемента по коду типа данных..
                        if (!TryGetSizeByTypeCode(typeCode, out size))
                        {
                            return false;
                        }

                        // Пытаемся преобразовать строку с номером бита в значение байта.
                        UInt16 bitIndex;
                        if (!UInt16.TryParse(currentPartName, out bitIndex))
                        {
                            return false;
                        }

                        // Проверяем что для соответствующих атомарных типов
                        // номера битов находятся в допустимом диапазоне и присваиваем значение.
                        if (typeCode == 0xC2)
                        {
                            if (bitIndex > 7) return false;
                            atomicBitDefinition = new BitOffsetPosition(BytesInElement.OneByte);
                            atomicBitDefinition.BitOffset = bitIndex;
                        }
                        else if (typeCode == 0xC3)
                        {
                            if (bitIndex > 15) return false;
                            atomicBitDefinition = new BitOffsetPosition(BytesInElement.TwoBytes);
                            atomicBitDefinition.BitOffset = bitIndex;
                        }
                        else if (typeCode == 0xC4)
                        {
                            if (bitIndex > 31) return false;
                            atomicBitDefinition = new BitOffsetPosition(BytesInElement.FourBytes);
                            atomicBitDefinition.BitOffset = bitIndex;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    /* ====================================================================================== */
                    #endregion
                }
                /* ====================================================================================== */
                #endregion

                #region [ FINALIZATION ]
                /* ====================================================================================== */
                if (isLastPartIndex)
                {
                    // Получаем название типа данных.
                    if (this.AvaliableTemplateStructures.ContainsKey(typeCode))
                    {
                        typeName = this.AvaliableTemplateStructures[typeCode].Name;
                    }

                    // Получаем размер в байтах для данного элемента.
                    if (!TryGetSizeByTypeCode(typeCode, out size))
                    {
                        return false;
                    }
                    else
                    {
                        // Присваиваем результат найденного типа данных.
                        logixTag.SymbolicEPath = epath;
                        logixTag.Type.Code = typeCode;
                        logixTag.Type.Name = typeName;
                        logixTag.Type.ElementSize = size;
                        logixTag.Type.AtomicBitDefinition = atomicBitDefinition;
                        logixTag.Type.StructureDefinition = structureDefinition;
                        logixTag.Type.BitArrayDefinition = bitArrayDefinition;
                        logixTag.Type.ArrayDimension = arrayDefinition;
                        logixTag.Type.ArrayDimension.Value = (ushort)logixTag.Type.ArrayDimension.LinearDim;
                        logixTag.Type.HiddenMemberName = hiddenMemberName;

                        return true;
                    }
                }
                /* ====================================================================================== */
                #endregion
            }

            return false;
        }
        /* ======================================================================================== */
        #endregion

        /// <summary>
        /// Проеобразовывает данный объект в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.Device != null)
            {
                return this.Device.Name;
            }
            else
            {
                return "???";
            }
        }
    }
}
