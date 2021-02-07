using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using System.Threading;
using EIP.AllenBradley;
using EIP.AllenBradley.Models.Events;
using LogixTool.Common;
using LogixTool.Common.Extension;

namespace LogixTool
{
    /// <summary>
    /// 
    /// </summary>
    public class TagRecorder
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        public const char SEPARATOR = '\t';
        /// <summary>
        /// Возвращает значение о том что имеется разрешение запуска процесса записи.
        /// </summary>
        public bool EnableRunning { get; private set; }
        /// <summary>
        /// Возвращает информацию о том что поток записи в файл активен.
        /// </summary>
        public bool WritingRunned
        {
            get
            {
                return (this.streamWriter != null && this.streamWriter.BaseStream != null);
            }
        }
        /// <summary>
        /// Возвращает true в случае если поток реализован и находится в состоянии работы.
        /// </summary>
        private bool ProcessRunned
        {
            get
            {
                return thread != null && (thread.ThreadState == ThreadState.Running || thread.ThreadState == ThreadState.Background);
            }
        }

        /// <summary>
        /// Возвращает полное текущее имя файла.
        /// </summary>
        public string FullFileName { get; private set; }
        /// <summary>
        /// Возвращает текущий размер файла в байтах.
        /// </summary>
        public long CurrentFileSize { get; private set; }
        /// <summary>
        /// Возвращает кол-во произведенных записей в файл.
        /// </summary>
        public int RecordCounter { get; private set; }

        /// <summary>
        /// Возвращает или задает место расположения файла для записи.
        /// </summary>
        public string FileLocation { get; set; }
        /// <summary>
        /// Возвращает или задает префикс имени файла.
        /// </summary>
        public string FilePrefix { get; set; }

        /// <summary>
        /// Возвращает или задает размер файла (Mb) при достижении которого произойдет создание нового.
        /// </summary>
        public int SeparationFileSize { get; set; }
        /// <summary>
        /// Возвращает или задает деление файлов по временному промежутку.
        /// </summary>
        public SeparationFilePeriodBy SeparationPeriod { get; set; }

        /// <summary>
        /// Возвращает или задает событие при котором будет происходить запись.
        /// </summary>
        public RecordingEventType RecordingType { get; set; }
        /// <summary>
        /// Возвращает или задает значение промежутка времени по которому будет происходить запись.
        /// </summary>
        public long RecordingPeriodValue { get; set; }
        /// <summary>
        /// Возвращает или задает единицу измерения промежутка времени по которому будет происходить запись.
        /// </summary>
        public RecordingPeriodUnits RecordingPeriodUnit { get; set; }
        /// <summary>
        /// Возвращает или задает запись с форматом времени записи в тиках при значении True, 
        /// в проивном случае будет происходить запись в виде строкового значения [dd.mm.yyy hh:mm:ss:uuuu].
        /// </summary>
        public bool TickTimeFormat { get; set; }


        /// <summary>
        /// Текущие тэги с которыми производится работа.
        /// </summary>
        public List<LogixTagHandler> RecordedTags { get; set; }
        /// <summary>
        /// Получает или задает список тэгов по изменению значений которых будет происходить запись.
        /// Данное свойство имеет смысл только при свойтве RecordingType равному значению BySelectedTags.
        /// </summary>
        public List<LogixTagHandler> SelectedTags
        {
            get
            {
                return selectedTags.Keys.ToList();
            }
            set
            {
                selectedTags.Clear();

                foreach(LogixTagHandler tag in value)
                {
                    if (!selectedTags.ContainsKey(tag))
                    {
                        this.selectedTags.Add(tag, tag);
                    }
                }
            }
        }
        /* ================================================================================================== */
        #endregion

        private List<RecordItem> writingBuffer;                     // Буфер данных которые необходимо записать в файл.
        private Dictionary<LogixTagHandler, LogixTagHandler> selectedTags;    // 
        private DateTime lastFileCreating;                          // Дата и время последнего создания файла.
        private DateTime lastRecord;                                // Дата и время последней записи.
        private StreamWriter streamWriter;                          // Поток записи в файл.
        private Thread thread;                                      // Основной поток процесса.

        /// <summary>
        /// Создает новый экземпляр класса для поточной записи тэгов.
        /// </summary>
        public TagRecorder()
        {
            this.RecordCounter = 0;
            this.CurrentFileSize = 0;


            this.writingBuffer = new List<RecordItem>();
            this.selectedTags = new Dictionary<LogixTagHandler, LogixTagHandler>();
            this.FileLocation = Environment.CurrentDirectory;
            this.FilePrefix = "new_recording";

            this.RecordingType = RecordingEventType.All;
            this.RecordingPeriodValue = 1;
            this.RecordingPeriodUnit = RecordingPeriodUnits.Sec;
            this.RecordedTags = new List<LogixTagHandler>();

            this.SeparationPeriod = SeparationFilePeriodBy.Day;
            this.SeparationFileSize = 1;

            this.TickTimeFormat = false;
        }

        #region [ EVENTS ]
        /* ================================================================================================== */
        /// <summary>
        /// 
        /// </summary>
        public static event MessageEvent Messages;
        /// <summary>
        /// 
        /// </summary>
        public event MessageEvent Message;
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler Error;

        /// <summary>
        /// Вызывает событие с сообщением.
        /// </summary>
        /// <param name="e"></param>
        private void Event_Message(MessageEventArgs e)
        {
            MessageEventArgs messageEventArgs = e;
            string messageHeader = "[Tag Data Recorder: " + this.FilePrefix + "]." + messageEventArgs.Header;
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
        /// <summary>
        /// 
        /// </summary>
        private void Error_Event()
        {
            if (this.Error!=null)
            {
                this.Error(this, null);
            }
        }
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// Основной процесс потока.
        /// </summary>
        private void Process()
        {
            bool processEnable = true;

            while (processEnable)
            {
                #region [ 0. INIT WRITING STREAM ]
                /* ================================================================================================== */
                if (!this.WritingRunned && this.EnableRunning)
                {
                    if (!InitWriting())
                    {
                        this.EnableRunning = false;
                    }
                }
                /* ================================================================================================== */
                #endregion;

                #region [ 1. PERIODIC RECORDING ]
                /* ================================================================================================== */
                if (this.WritingRunned && this.RecordingType == RecordingEventType.ByPeriod)
                {
                    // Получаем значение периода в миллисекундах.
                    long period = this.RecordingPeriodValue;
                    switch (this.RecordingPeriodUnit)
                    {
                        case RecordingPeriodUnits.mSec:
                            break;

                        case RecordingPeriodUnits.Sec:
                            period *= 1000;
                            break;

                        case RecordingPeriodUnits.Min:
                            period *= 60000;
                            break;

                        case RecordingPeriodUnits.Hour:
                            period *= 3600000;
                            break;
                    }

                    // Проверяем превысило ли текущее время заданный перод.
                    DateTime currDateTime = DateTime.Now;
                    if (lastRecord.Ticks == 0 || (currDateTime.Ticks - lastRecord.Ticks) > period * 10000)
                    {
                        lastRecord = currDateTime;
                        AddRecord(false);
                    }
                }
                /* ================================================================================================== */
                #endregion

                #region [ 2. WRITING MANANGMENT (Buffer to Stream) ]
                /* ================================================================================================== */
                if (this.WritingRunned && writingBuffer.Count > 0 && writingBuffer[0] != null)
                {
                    // Получаем самый устарелый элемент для записи.
                    RecordItem recordItem = writingBuffer[0];
                    // Получаем полностью строку для записи.
                    string recordingString = recordItem.ToString(this.TickTimeFormat, SEPARATOR);
                    // Увеличиваем текущий счетчик размера файла на кол-во байт в строке 
                    // плюс два байта переноса строки.
                    this.CurrentFileSize += recordingString.Length + 2;

                    // Контроль превышения размера.
                    if (this.SeparationFileSize > 0 && this.CurrentFileSize >= this.SeparationFileSize * 1048576)
                    {
                        InitWriting();
                    }

                    // Контроль разделения файлов по дате.
                    if ((this.SeparationPeriod == SeparationFilePeriodBy.Minute && this.lastFileCreating.Minute != recordItem.TimeStamp.Minute)
                    || (this.SeparationPeriod == SeparationFilePeriodBy.Hour && this.lastFileCreating.Hour != recordItem.TimeStamp.Hour)
                    || (this.SeparationPeriod == SeparationFilePeriodBy.Day && this.lastFileCreating.Day != recordItem.TimeStamp.Day)
                    || (this.SeparationPeriod == SeparationFilePeriodBy.Week && this.lastFileCreating.GetWeekNumber() != recordItem.TimeStamp.GetWeekNumber())
                    || (this.SeparationPeriod == SeparationFilePeriodBy.Month && this.lastFileCreating.Month != recordItem.TimeStamp.Month))
                    {
                        InitWriting();
                    }

                    // Записываем текущую строку в поток записи в файл.
                    streamWriter.WriteLine(recordingString);
                    // После обработки записи. Удаляем самый старый элемент.
                    writingBuffer.RemoveAt(0);
                }
                /* ================================================================================================== */
                #endregion

                #region [ 3. CLOSE WRITING STREAM ]
                /* ================================================================================================== */
                if (!EnableRunning)
                {
                    processEnable = !CloseWriting();
                }
                /* ================================================================================================== */
                #endregion
            }
        }

        #region [ PUBLIC METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Запускает процесс потоковой записи в файл.
        /// </summary>
        public void Run()
        {
            this.Event_Message(new MessageEventArgs(this,MessageEventArgsType.Info, "Run Process", "Request."));
            if (WritingRunned)
            {
                this.Event_Message(new MessageEventArgs(this, MessageEventArgsType.Warning, "Run Process", "Refused. Already has been runned."));
                return;
            }

            RunProcess();
        }
        /// <summary>
        /// Запрашивает добавление записи данных тэгов с которыми в данный момент производится работа в очередь потоковой записи в файл.
        /// </summary>
        /// <param name="tags">Список тэгов со значениями чтения которые были изменены.</param>
        public void RequestForRecording(List<LogixTagHandler> tags)
        {
            if (this.ProcessRunned && this.WritingRunned)
            {
                if ((this.RecordingType == RecordingEventType.All) ||
                    (this.RecordingType == RecordingEventType.BySelectedTags && tags.Any(t => selectedTags.ContainsKey(t))))
                {
                    AddRecord(true);
                }
            }
        }
        /// <summary>
        /// Останавливает процесс потоковой записи в файл.
        /// </summary>
        /// <returns></returns>
        public void Stop()
        {
            this.Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, "Stop Process", "Request."));
            this.EnableRunning = false;
        }

        /// <summary>
        /// Получает настройки в виде xml объекта.
        /// </summary>
        public XElement GetXSettings()
        {
            XElement xSettings = new XElement("Settings");
            xSettings.Add(new XAttribute("FileLocation", this.FileLocation));
            xSettings.Add(new XAttribute("FilePrefix", this.FilePrefix));
            xSettings.Add(new XAttribute("SeparationFileSize", this.SeparationFileSize.ToString()));
            xSettings.Add(new XAttribute("SeparationPeriod", this.SeparationPeriod.ToString()));
            xSettings.Add(new XAttribute("RecordingType", this.RecordingType.ToString()));
            xSettings.Add(new XAttribute("RecordingPeriodValue", this.RecordingPeriodValue.ToString()));
            xSettings.Add(new XAttribute("RecordingPeriodUnit", this.RecordingPeriodUnit.ToString()));
            xSettings.Add(new XAttribute("TickTimeFormat", this.TickTimeFormat.ToString().ToLower()));
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

            // Читаем каждый из атрибутов файла в промежуточные переменные соответствующие своим типам.
            string fileLocation = xSettings.Attribute("FileLocation").GetXValue(Environment.CurrentDirectory);
            string filePrefix = xSettings.Attribute("FilePrefix").GetXValue("newrec_");
            int separationFileSize;

            if (!int.TryParse(xSettings.Attribute("SeparationFileSize").GetXValue("1"), out separationFileSize))
            {
                separationFileSize = 1;
            }

            SeparationFilePeriodBy separationPeriod;
            if (!Enum.TryParse<SeparationFilePeriodBy>(xSettings.Attribute("SeparationPeriod").GetXValue(""), out separationPeriod))
            {
                separationPeriod = SeparationFilePeriodBy.None;
            }

            RecordingEventType recordingType;
            if (!Enum.TryParse<RecordingEventType>(xSettings.Attribute("RecordingType").GetXValue(""), out recordingType))
            {
                recordingType = RecordingEventType.All;
            }

            long recordingPeriodValue;
            if (!long.TryParse(xSettings.Attribute("RecordingPeriodValue").GetXValue("1"), out recordingPeriodValue))
            {
                recordingPeriodValue = 1;
            }

            RecordingPeriodUnits recordingPeriodUnit;
            if (!Enum.TryParse<RecordingPeriodUnits>(xSettings.Attribute("RecordingPeriodUnit").GetXValue(""), out recordingPeriodUnit))
            {
                recordingPeriodUnit = RecordingPeriodUnits.Sec;
            }

            bool tickTimeFormat = xSettings.Attribute("TickTimeFormat").GetXValue("false") == "true";

            // Устанавливаем значения свойств из выше полученных промежуточных переменных.
            this.FileLocation = fileLocation;
            this.FilePrefix = filePrefix;
            this.SeparationFileSize = separationFileSize;
            this.SeparationPeriod = separationPeriod;
            this.RecordingType = recordingType;
            this.RecordingPeriodValue = recordingPeriodValue;
            this.RecordingPeriodUnit = recordingPeriodUnit;
            this.TickTimeFormat = tickTimeFormat;

            return true;
        }
        /* ================================================================================================== */
        #endregion

        #region [ PRIVATE METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Запускает новый потоковый процесс обработки записи входных регистрационных данных.
        /// </summary>
        private void RunProcess()
        {
            if (!this.ProcessRunned)
            {
                try
                {
                    this.thread = new Thread(new ThreadStart(Process));
                    this.thread.IsBackground = true;
                    this.thread.Start();
                    this.EnableRunning = true;
                    this.Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, "Run Thread", "OK. New Thread was created. Cyclic process is enable."));
                }
                catch
                {
                    this.EnableRunning = false;
                    this.Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, "Run Thread", "Error! New Thread can not to be created."));
                    this.Error_Event();
                }
            }
            else
            {
                this.EnableRunning = true;
                this.Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, "Run Thread", "OK. Thread is exist. Cyclic process is enable."));
            }

        }
        /// <summary>
        /// Инициализирует новый поток записи в файл. При успешном выполнении операции возвращает true.
        /// </summary>
        /// <returns></returns>
        private bool InitWriting()
        {
            lastFileCreating = DateTime.Now;

            if (this.WritingRunned)
            {
                CloseWriting();
            }

            // Формируем вторую часть имени файла.
            string postfix = "";

            switch (this.SeparationPeriod)
            {
                case SeparationFilePeriodBy.Minute:
                    postfix = lastFileCreating.Year.ToString("0000") +  lastFileCreating.Month.ToString("00") +  lastFileCreating.Day.ToString("00")
                        + "-" + lastFileCreating.Hour.ToString("00") +  lastFileCreating.Minute.ToString("00");
                    break;

                case SeparationFilePeriodBy.Hour:
                    postfix = lastFileCreating.Year.ToString("0000") +  lastFileCreating.Month.ToString("00") + lastFileCreating.Day.ToString("00")
                        + "-" + lastFileCreating.Hour.ToString("00");
                    break;

                case SeparationFilePeriodBy.Day:
                    postfix = lastFileCreating.Year.ToString("0000") +  lastFileCreating.Month.ToString("00") + lastFileCreating.Day.ToString("00");
                    break;

                case SeparationFilePeriodBy.Week:
                    postfix = lastFileCreating.Year.ToString("0000") + "#" + lastFileCreating.GetWeekNumber().ToString("00");
                    break;

                case SeparationFilePeriodBy.Month:
                    postfix = lastFileCreating.Year.ToString("0000") + lastFileCreating.Month.ToString("00");
                    break;
            }

            if (this.SeparationPeriod != SeparationFilePeriodBy.None)
            {
                postfix = "-" + postfix;
            }

            int partNumber = 0;
            string part = "";
            FileInfo fi;
            do
            {
                part = "-" + partNumber.ToString();
                this.FullFileName = this.FileLocation + "\\" + this.FilePrefix + postfix + part + ".csv";
                fi = new FileInfo(this.FullFileName);
                partNumber++;
            }
            while (fi != null && fi.Exists);

            // Создаем поток записи в файл.
            try
            {
                streamWriter = new StreamWriter(this.FullFileName);

                string text = "Time" + SEPARATOR;

                List<string> tagNames = new List<string>();

                foreach (LogixTagHandler tag in this.RecordedTags)
                {
                    string name = (tag.OwnerTask!=null? ("[" + tag.OwnerTask.ToString() + "]"):"[?]") + tag.Name;

                    if (tag.Type.ArrayDimension.HasValue)
                    {
                        for (int ix = 0; ix < tag.Type.ArrayDimension.Value; ix++)
                        {
                            tagNames.Add(name + "[" + ix.ToString() + "]");
                        }
                    }
                    else
                    {
                        tagNames.Add(name);
                    }
                }

                text += String.Join(SEPARATOR.ToString(), tagNames);

                this.RecordCounter = 0;
                this.CurrentFileSize = text.Length + 2;
                streamWriter.WriteLine(text);

                this.Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, "Writing Init", "OK. File name: " + this.FullFileName));
                return true;
            }
            catch
            {
                this.Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, "Writing Init", "Error! File name: " + this.FullFileName));
                this.Error_Event();
                return false;
            }
        }
        /// <summary>
        /// Закрывает текущий поток записи в файл. При успешном выполнении операции возвращает true.
        /// </summary>
        /// <returns></returns>
        private bool CloseWriting()
        {
            try
            {
                if (!WritingRunned)
                {
                    return true;
                }

                streamWriter.Flush();
                streamWriter.Close();

                this.Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, "Writing Close", "OK. File name: " + this.FullFileName));
                return true;
            }
            catch
            {
                this.Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, "Writing Close", "Error! File name: " + this.FullFileName));
                this.Error_Event();
                return false;
            }
        }
        /// <summary>
        /// Добавляет запись в общий буфер.
        /// </summary>
        /// <returns></returns>
        private void AddRecord(bool toUseDateTimeFromLastUpdatedTag)
        {
            DateTime dt = DateTime.Now;

            if (toUseDateTimeFromLastUpdatedTag)
            {
                // Получаем все тэги которые были когда-либо обновлены.
                List<LogixTagHandler> updatedTags = this.RecordedTags.Where(g => g.ReadValue.Report.ServerResponseTimeStamp.HasValue).ToList();
                // В Случае если ни один тэг не был обновлем, уходим из метода.
                if (updatedTags.Count == 0)
                {
                    return;
                }

                // Получаем дату/время из последнего обновленного тэга и берем данное значение как дату/время записи.
                dt = new DateTime(updatedTags.Select(t => (long)t.ReadValue.Report.ServerResponseTimeStamp).Max());
            }

            // Получаем строку значений.
            string value = "";

            List<string> tagValues = new List<string>();

            foreach (LogixTagHandler tag in this.RecordedTags)
            {
                if (!tag.Type.ArrayDimension.HasValue)
                {
                    string s = tag.GetReadedValueText();
                    if (s == null)
                    {
                        s = "";
                    }

                    tagValues.Add(s);
                }
            }

            value += String.Join(SEPARATOR.ToString(), tagValues);

            // Добавляем запись и увеличиваем счетчик записей.
            writingBuffer.Add(new RecordItem(dt, value));
            this.RecordCounter++;

            this.Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, "Add Record", "OK. Line=" + this.RecordCounter));
        }
        /* ================================================================================================== */
        #endregion
    }
}
