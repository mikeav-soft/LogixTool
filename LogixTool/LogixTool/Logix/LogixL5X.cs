using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Security.Cryptography;
using LogixTool.Logix.Models;
using LogixTool.Common;

namespace LogixTool.Logix
{
    /// <summary>
    /// 
    /// </summary>
    public enum TagBuildMethod { NONE, BY_INSTRUCTION_USING, COMPLETE }
    /// <summary>
    /// =========================================================================================================================
    /// Автор класса:   ВАУЛИН Михаил.
    /// Год создания:   2017г.
    /// Класс предоставляющий возможность читать файлы с расширением *.L5X для контроллеров Allen Breadley.
    /// Цель класса: чтение *.L5X (по сути *.xml файл со специальной структурой.) для дальнейшего анализа.
    /// Класс предоставляет возможности:
    /// - 1. Чтение и разложение на XML объекты.
    /// - 2. Расшифровка закодированных данных.
    /// - 3. Разложение по объектам со строгой структурой в виде дерева.
    /// - 4. Построение перекрестных ссылок.
    /// =========================================================================================================================
    /// </summary>
    public class LogixL5X : LogixTree
    {
        /// <summary>
        /// 
        /// </summary>
        private string MESSAGE_HEADER
        {
            get
            {
                return "L5X Core : " + this.ID;
            }
        }

        #region [ CONSTANTS ]
        /* ============================================================================== */
        // Криптографические ключи.
        private const string KEY_01 = "44006F0075006700270073004500780070006F007200";
        private const string KEY_02 = "5D002C0031006800610031004500580054002900240051005A003A005200370065006900390041004B0028005D005D00570034004800630031005C006A0040";
        private const string KEY_03 = "5900530033003F0043004E0021004000420073004900740039006C0070002D003D0043004A003200300065004C004500760021005A0064004900530033002500680052006B00470070005700720079004F00590021006C00690021004C002F006E0038005F0023002A00760034002E0048007A00570048002D00700034007600";
        private const string KEY_05 = "5300340079005400560049005A007A00240063003E005700380026005D0078002F003B004F00550065003F00660051006F007A003300620063005700260042007B0031005A00240068002B006F00460033005C004C003D0023004B005E00650055002500580032007300480048002B0055003D004D0063004E0037002900";
        private const string KEY_06 = "277d3a6f747c647b457d587a502c7a5c4c793137617d24762e6a482a6f54433d553e746c6638655f504f682b68485d695e352c2a2d6f343e325a5d71262961";
        private const string KEY_07 = "786d732a344075703d7d7556464065645769554c622e646b403b6e443c405372502e6c373f4a6b33326f5a457b697377552779286f483d4e53553a5e523f30";
        private const string KEY_08 = "4f2a6b7a713d39495432234c3f7e7c3a775b2a4d674773577a717331436d345b2429456a6761433c5a282c323226724b654632267c392c7442276a2d2e723d";
        /* ============================================================================== */
        #endregion

        #region [ PROPERTIES ]
        /* ============================================================================== */
        // Переменные.
        private XDocument xDocument;

        string _FilePath;
        /// <summary>
        /// Путь к файлу проекта.
        /// </summary>
        public string FilePath
        {
            get
            {
                return _FilePath;
            }
        }

        #region [ RAW XDATA ]
        public XElement XController { get; set; }
        public XElement XDescription { get; set; }
        public XElement XRedundancyInfo { get; set; }
        public XElement XSecurity { get; set; }
        public XElement XSafetyInfo { get; set; }
        public XElement XDataTypes { get; set; }
        public XElement XModules { get; set; }
        public XElement XAddOnInstructionDefinitions { get; set; }
        public XElement XTags { get; set; }
        public XElement XPrograms { get; set; }
        public XElement XTasks { get; set; }
        public XElement XCommPorts { get; set; }
        public XElement XCST { get; set; }
        public XElement XWallClockTime { get; set; }
        public XElement XTrends { get; set; }
        public XElement XTimeSynchronize { get; set; }
        #endregion

        /// <summary>
        /// Содержит закодированные элементы.
        /// </summary>
        public bool IsEncodedFile
        {
            get
            {
                return (xDocument != null && xDocument.Descendants("EncodedData").Count() > 0);
            }
        }
        /// <summary>
        /// Означает что файл загружен успешно.
        /// </summary>
        public bool IsLoadedSuccessful { get; set; }

        #region [ CREATED XDATA ]
        /// <summary>
        /// 
        /// </summary>
        public XElement XPredefinedDataTypes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public XElement XModuleDefinedDataTypes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public XElement XModuleTags { get; set; }
        #endregion

        /// <summary>
        /// Стандартные инструкции и их поведения.
        /// </summary>
        private static Dictionary<string, List<LogicInstructionParameter>> StandartInstructionDefinition { get; set; }
        /* ============================================================================== */
        /// <summary>
        /// 
        /// </summary>
        public List<SafetyTagMap> SafetyTagMap { get; set; }
        /// <summary>
        /// Задачи контроллера.
        /// </summary>
        public List<LogicTask> Tasks { get; set; }
        /// <summary>
        /// Неразмеченные (незапускаемые) программы.
        /// </summary>
        public List<LogicProgram> UnscheduledPrograms { get; set; }
        /// <summary>
        /// Двта типы проекта.
        /// </summary>
        public Dictionary<string, DataType> DataTypes { get; set; }
        /// <summary>
        /// Контроллерные тэги.
        /// </summary>
        public Dictionary<string, Tag> Tags { get; set; }
        /// <summary>
        /// Инструкции Add-On.
        /// </summary>
        public Dictionary<string, AddonInstruction> AddonInstructions { get; set; }
        /* ============================================================================== */
        #endregion

        /// <summary>
        /// Создает структуру элементов файла L5X для контроллеров Allen Breadley.
        /// </summary>
        public LogixL5X()
            : base("")
        {
            this._FilePath = "";
            this.xDocument = new XDocument();

            XElement xRSLogix5000Content = new XElement("RSLogix5000Content");

            this.XController = new XElement("Controller");
            this.XDescription = new XElement("Description");
            this.XRedundancyInfo = new XElement("RedundancyInfo");
            this.XSecurity = new XElement("Security");
            this.XSafetyInfo = new XElement("SafetyInfo");
            this.XDataTypes = new XElement("DataTypes");
            this.XModules = new XElement("Modules");
            this.XAddOnInstructionDefinitions = new XElement("AddOnInstructionDefinitions");
            this.XTags = new XElement("Tags");
            this.XPrograms = new XElement("Programs");
            this.XTasks = new XElement("Tasks");
            this.XCommPorts = new XElement("CommPorts");
            this.XCST = new XElement("CST");
            this.XWallClockTime = new XElement("WallClockTime");
            this.XTrends = new XElement("Trends");
            this.XTimeSynchronize = new XElement("TimeSynchronize");

            xDocument.Add(xRSLogix5000Content);
            xRSLogix5000Content.Add(XController);
            XController.Add(XDescription);
            XController.Add(XRedundancyInfo);
            XController.Add(XSecurity);
            XController.Add(XSafetyInfo);
            XController.Add(XDataTypes);
            XController.Add(XModules);
            XController.Add(XAddOnInstructionDefinitions);
            XController.Add(XTags);
            XController.Add(XPrograms);
            XController.Add(XTasks);
            XController.Add(XCommPorts);
            XController.Add(XCST);
            XController.Add(XWallClockTime);
            XController.Add(XTrends);
            XController.Add(XTimeSynchronize);

            this.XPredefinedDataTypes = null;
            this.XModuleDefinedDataTypes = null;
            this.XModuleTags = null;

            LoadPredefinedDataTypes();
            LoadStandardInstructionDefinition();

            // Новая концепция.
            this.ID = "[?]";
            this.SafetyTagMap = new List<SafetyTagMap>();
            this.Tasks = new List<LogicTask>();
            this.UnscheduledPrograms = new List<LogicProgram>();
            this.DataTypes = new Dictionary<string, DataType>();
            this.Tags = new Dictionary<string, Tag>();
            this.AddonInstructions = new Dictionary<string, AddonInstruction>();
        }

        #region [ EVENTS ]
        /* ============================================================================== */
        /// <summary>
        /// Возникает при появлении каких либо сообщений при работе ядра.
        /// </summary>
        public static event MessageEvent Messages;
        /// <summary>
        /// Возникает при появлении каких либо сообщений при работе ядра.
        /// </summary>
        public event MessageEvent Message;

        /// <summary>
        /// Вызывает событие с сообщением.
        /// </summary>
        /// <param name="e"></param>
        private void Event_Message(MessageEventArgs e)
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
        /// <summary>
        /// Вызывает событие с сообщением из внешнего объекта.
        /// </summary>
        /// <param name="e"></param>
        internal static void Event_ExternalMessage(object sender, MessageEventArgs e)
        {
            if (Messages != null)
            {
                Messages(sender, e);
            }
        }
        /* ============================================================================== */
        #endregion

        #region [ METHODS PUBLIC ]
        /* ============================================================================== */
        /// <summary>
        /// Загружает файл *.L5X.
        /// </summary>
        /// <param name="filepath">Путь к файлу.</param>
        /// <param name="buildTagStructure">Строит структуру тэгов .</param>
        /// <returns></returns>
        public bool Load(string filepath, TagBuildMethod tagBuildMethod)
        {
            try
            {
                #region [ Чтение узлов xml из файла *.l5x]
                // ====================================================================
                IsLoadedSuccessful = true;

                // Пересылаем сообщение.
                Event_Message(new MessageEventArgs(this,
                    MessageEventArgsType.Info, MESSAGE_HEADER, "Loading of L5X file: " + filepath));

                this.xDocument = XDocument.Load(filepath);
                _FilePath = filepath;
                if (this.xDocument != null)
                {
                    XElement xRSLogix5000Content = xDocument.Element("RSLogix5000Content");

                    if (xRSLogix5000Content != null)
                    {
                        this.XController = xRSLogix5000Content.Element("Controller");
                        this.ID = "[" + this.XController.Attribute("Name").GetXValue("?") + "]";

                        if (this.XController != null)
                        {
                            #region [ Loading of Main XElements ]
                            // ====================================================================
                            this.XDescription = XController.Element("Description");
                            this.XRedundancyInfo = XController.Element("RedundancyInfo");
                            this.XSecurity = XController.Element("Security");
                            this.XSafetyInfo = XController.Element("SafetyInfo");
                            this.XDataTypes = XController.Element("DataTypes");
                            this.XModules = XController.Element("Modules");
                            this.XAddOnInstructionDefinitions = XController.Element("AddOnInstructionDefinitions");
                            this.XTags = XController.Element("Tags");
                            this.XPrograms = XController.Element("Programs");
                            this.XTasks = XController.Element("Tasks");
                            this.XCommPorts = XController.Element("CommPorts");
                            this.XCST = XController.Element("CST");
                            this.XWallClockTime = XController.Element("WallClockTime");
                            this.XTrends = XController.Element("Trends");
                            this.XTimeSynchronize = XController.Element("TimeSynchronize");
                            // ====================================================================
                            #endregion

                            #region [ Checking of Main XElements ]
                            // ====================================================================
                            if (this.XDescription == null)
                            {
                                // Node Description was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/Description of L5X file is NULL!"));
                            }
                            if (this.XRedundancyInfo == null)
                            {
                                // Node Description was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/RedundancyInfo of L5X file is NULL!"));
                            }
                            if (this.XSecurity == null)
                            {
                                // Node Description was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/Security of L5X file is NULL!"));
                            }
                            if (this.XSafetyInfo == null)
                            {
                                // Node Description was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/SafetyInfo of L5X file is NULL!"));
                            }
                            if (this.XDataTypes == null)
                            {
                                // Node Description was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/DataTypes of L5X file is NULL!"));
                            }
                            if (this.XModules == null)
                            {
                                // Node Description was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/Modules of L5X file is NULL!"));
                            }
                            if (this.XAddOnInstructionDefinitions == null)
                            {
                                // Node AddOnInstructionDefinitions was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/AddOnInstructionDefinitions of L5X file is NULL!"));
                            }
                            if (this.XTags == null)
                            {
                                // Node Tags was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/Tags of L5X file is NULL!"));
                            }
                            if (this.XPrograms == null)
                            {
                                // Node Programs was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/Programs of L5X file is NULL!"));
                            }
                            if (this.XTasks == null)
                            {
                                // Node Tasks was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/Tasks of L5X file is NULL!"));
                            }
                            if (this.XCommPorts == null)
                            {
                                // Node CommPorts was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/CommPorts of L5X file is NULL!"));
                            }
                            if (this.XCST == null)
                            {
                                // Node CST was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/CST of L5X file is NULL!"));
                            }
                            if (this.XWallClockTime == null)
                            {
                                // Node WallClockTime was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/WallClockTime of L5X file is NULL!"));
                            }
                            if (this.XTrends == null)
                            {
                                // Node Trends was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/Trends of L5X file is NULL!"));
                            }
                            if (this.XTimeSynchronize == null)
                            {
                                // Node TimeSynchronize was not found
                                IsLoadedSuccessful = false;
                                // Пересылаем сообщение.
                                Event_Message(new MessageEventArgs(this,
                                    MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/TimeSynchronize of L5X file is NULL!"));
                            }
                            // ====================================================================
                            #endregion
                        }
                        else
                        {
                            IsLoadedSuccessful = false;
                            // Пересылаем сообщение.
                            Event_Message(new MessageEventArgs(this,
                                MessageEventArgsType.Error, MESSAGE_HEADER, "Node RSLogix5000Content/Controller of L5X file is NULL!"));
                        }
                    }
                    else
                    {
                        IsLoadedSuccessful = false;
                        // Пересылаем сообщение.
                        Event_Message(new MessageEventArgs(this,
                            MessageEventArgsType.Error, MESSAGE_HEADER, "Root node RSLogix5000Content of L5X file is NULL!"));
                    }
                }
                // ====================================================================
                #endregion
            }
            catch
            {
                IsLoadedSuccessful = false;
            }

            if (IsLoadedSuccessful)
            {
                // Пересылаем сообщение.
                Event_Message(new MessageEventArgs(this,
                    MessageEventArgsType.Info, MESSAGE_HEADER, "Loading of L5X file is succesful."));

                // Пересылаем сообщение.
                Event_Message(new MessageEventArgs(this,
                    MessageEventArgsType.Info, MESSAGE_HEADER, "Start of uncryption and creating models."));

                /* 1. Подготовка. */
                Uncrypt();                              // Расшифровка зашифрованных узлов XML.

                /* 2. Восстановление промежуточных XML объектов.*/
                CreateXModuleDefinedDataTypes();        // Восстановление (XML объекты) Типов Данных модулей.
                CreateXModuleTags();                    // Восстановление (XML объекты) Тэгов выделенных под модули I/O.
                /**/
                LoadDataTypes();                        // Загрузка всех типов данных (Predefined, UDT, Modules, Add-On).
                DefineDataTypeSize();                   // Расчитываем размер типов данных.
                LoadAddons();                           // Загружаем инструкции Add-On.
                LoadTags();                             // Загружаем тэги.
                LoadSefetyMap();                        // Загружаем карту привязки стандартных тэгов с тэгами безопасности.
                BuildTagsStructure(tagBuildMethod);
                /**/
                LoadLogic();                            // Загружаем и конвертируем логику контроллеров.
                DefineInstructionParameters(tagBuildMethod);
                /**/
                BuildCrossReference();                  // Создаем перекрестные ссылки для узлов логики, тэгов и прочего.
            }
            else
            {
                // Пересылаем сообщение.
                Event_Message(new MessageEventArgs(this,
                    MessageEventArgsType.Info, MESSAGE_HEADER, "Loading of L5X file is unsuccesful."));
            }

            return IsLoadedSuccessful;
        }
        /// <summary>
        /// Сохраняет открытые узлы xml обратно в файл *.L5X.
        /// </summary>
        /// <param name="filepath">Путь к файлу.</param>
        /// <returns></returns>
        public bool Save(string filepath)
        {
            bool result;
            try
            {
                xDocument.Save(filepath);
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }
        /// <summary>
        /// Удаляет атрибуты защиты данных.
        /// </summary>
        public void RemoveProtection()
        {
            // Remove attributes of Source Protection
            XAttribute xatr;    // Current XAttribute

            IEnumerable<XElement> xElementsWithSourceProtection = XController.Descendants()
                .Where(e => e.Attribute("SourceKey") != null || e.Attribute("SourceProtectionType") != null);


            foreach (XElement xelem in xElementsWithSourceProtection)
            {
                xatr = xelem.Attribute("SourceKey");
                if (xatr != null)
                {
                    xatr.Remove();
                }

                xatr = xelem.Attribute("SourceProtectionType");
                if (xatr != null)
                {
                    xatr.Remove();
                }
            }
        }
        /* ============================================================================== */
        #endregion

        #region [ ПРИВАТНЫЕ МЕТОДЫ : "ОБЩЕЕ" ]
        /* ============================================================================== */
        /// <summary>
        /// Расшифровывает *.L5X файл.
        /// </summary>
        /// <returns></returns>
        private bool Uncrypt()
        {
            // Variables
            bool resultOfOperation = false;         // Intermediate logic result of operation
            List<XElement> xAllEncodedDatas;        // All XEncodedData's
            List<XElement> xErrEncodedDatas;
            xAllEncodedDatas = new List<XElement>();
            xErrEncodedDatas = new List<XElement>();

            // 1. Loading xml file L5X
            try
            {
                // Getting all XEncodedData
                xAllEncodedDatas = xDocument.Descendants("EncodedData").ToList();
                resultOfOperation = this.IsEncodedFile;
            }
            catch
            {
                resultOfOperation = false;
                // error of opening file L5X
            }

            // 2. Analyzing of "EncodedData's"
            if (resultOfOperation)
            {
                string encodedstr = "";
                string decodedstr = "";
                string uncrConfig = "";

                foreach (XElement xEncodedData in xAllEncodedDatas)
                {
                    // Getting last node, encoded fragment
                    encodedstr = xEncodedData.LastNode.ToString();
                    if (encodedstr.Trim() != "")
                    {
                        #region -> Preparation of encoded string and Uncription
                        // ==================================================================================
                        // Correction of start of string (remooving symbols)
                        while (encodedstr.StartsWith("\n") || encodedstr.StartsWith("\r"))
                        {
                            encodedstr = encodedstr.Remove(0, 1);
                        }

                        // For multiplicity 4 adding additional symbols "="
                        int length = encodedstr.Length % 4;
                        if (length > 0)
                        {
                            encodedstr = encodedstr + "====".Substring(0, 4 - length);
                        }

                        // Selecting of decryption method, Selection by attribute "EncryptionConfig"

                        if (!xEncodedData.Attribute("EncryptionConfig").TryGetXValue(out uncrConfig))
                        {
                            resultOfOperation = UncryptEncodedDataMethod1(encodedstr, out decodedstr);
                        }
                        else if (uncrConfig == "2")
                        {
                            resultOfOperation = UncryptEncodedDataMethod2(encodedstr, out decodedstr);
                        }
                        else
                        {
                            resultOfOperation = false;
                        }
                        // ==================================================================================
                        #endregion

                        #region -> Checking of recieved string
                        // ==================================================================================
                        // Checking recieved information
                        if (resultOfOperation)
                        {
                            // Preset of result of operation
                            resultOfOperation = false;

                            if (decodedstr.Length > 0)
                            {
                                if (decodedstr.StartsWith("<?xml "))
                                {
                                    while (decodedstr[decodedstr.Length - 1] != '>')
                                    {
                                        decodedstr = decodedstr.Remove(decodedstr.Length - 1, 1);
                                        resultOfOperation = true;
                                    }
                                }
                                else
                                {
                                    // Is not valid xml fragment
                                }
                            }
                            else
                            {
                                // Recieved string is empty
                            }
                        }
                        else
                        {
                            // Error of uncription
                        }
                        // ==================================================================================
                        #endregion

                        #region -> Patch of L5X file
                        // ==================================================================================
                        if (resultOfOperation)
                        {
                            XElement newXelem;  // new XElement from decoded data
                            try
                            {
                                // New xelement from [decodedstr]
                                newXelem = XElement.Parse(decodedstr);

                                // Add xelement after xEncodedData
                                xEncodedData.AddAfterSelf(newXelem);

                                // Remove Encoded Data from xml file
                                xEncodedData.Remove();
                            }
                            catch
                            {
                                resultOfOperation = false;
                                // error of creating xml Element
                            }
                        }
                        // ==================================================================================
                        #endregion
                    }
                    else
                    {
                        // encoded string is empty
                        resultOfOperation = false;
                    }

                    // Add current xEncodedData to Error List if was happines some problems
                    if (!resultOfOperation)
                    {
                        xErrEncodedDatas.Add(xEncodedData);
                    }
                }
            }

            return xErrEncodedDatas.Count == 0;
        }
        /// <summary>
        /// Load Predefined Data Types from internal rescources
        /// </summary>
        /// <returns></returns>
        private bool LoadPredefinedDataTypes()
        {
            // Load predefined Data Types from internal resources
            try
            {
                XDocument xPredefinedDataTypesDocument = XDocument.Parse(Resources.LogixResources.PredefinedDataTypes);
                this.XPredefinedDataTypes = xPredefinedDataTypesDocument.Element("DataTypes");
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Load Module Defined Data Types from Decorated Data in Module Tags
        /// </summary>
        /// <returns></returns>
        private bool CreateXModuleDefinedDataTypes()
        {
            bool convertingResult = false;                                                  // Output: Init. Result of operation
            this.XModuleDefinedDataTypes = new XElement("DataTypes");
            List<XElement> xDecoratedElements = new List<XElement>();
            xDecoratedElements.AddRange(this.XModules.Descendants("Structure"));
            xDecoratedElements.AddRange(this.XModules.Descendants("StructureMember"));

            // Getting all existed data types from this controller. Dictionary used for control anti repeation
            Dictionary<string, string> referenceDataTypeNames = new Dictionary<string, string>();
            // In User Data Types
            if (this.XDataTypes != null)
            {
                foreach (string currname in this.XDataTypes.Elements("DataType").Select(t => t.Attribute("Name").GetXValue("?")))
                {
                    if (!referenceDataTypeNames.ContainsKey(currname))
                        referenceDataTypeNames.Add(currname, currname);
                }
            }
            // In Predefined Data Types
            if (this.XPredefinedDataTypes != null)
            {
                foreach (string currname in this.XPredefinedDataTypes.Elements("DataType").Select(t => t.Attribute("Name").GetXValue("?")))
                {
                    if (!referenceDataTypeNames.ContainsKey(currname))
                        referenceDataTypeNames.Add(currname, currname);
                }
            }

            foreach (XElement xDecoratedElement in xDecoratedElements)
            {
                // Load module Defined Data Types from Decarated elements
                string datatypename;

                // Creating New DataType XElement and filling
                XElement xDataType = new XElement("DataType");
                XElement xMembers = new XElement("Members");
                xDataType.Add(xMembers);

                convertingResult = true;
                convertingResult &= xDecoratedElement.Attribute("DataType").TryGetXValue(out datatypename);
                convertingResult &= !referenceDataTypeNames.ContainsKey(datatypename);

                if (convertingResult)
                {
                    xDataType.Add(new XAttribute("Name", datatypename));
                    xDataType.Add(new XAttribute("Class", "Module"));

                    XElement xmember;               // New XElement "Member"
                    string name = "";               // Temporary string "Name" for xMember Attribute @Name
                    string datatype = "";           // Temporary string "DataType" for xMember Attribute @DataType
                    string dimension = "";          // Temporary string "Dimension" for xMember Attribute @Dimension


                    foreach (XElement xcurr in xDecoratedElement.Elements())
                    {
                        // Getting values of attributes
                        convertingResult &= xcurr.Attribute("Name").TryGetXValue(out name);
                        convertingResult &= xcurr.Attribute("DataType").TryGetXValue(out datatype);
                        if (!xcurr.Attribute("Dimensions").TryGetXValue(out dimension))
                        {
                            dimension = "0";
                        }

                        // Creating, adding and filling xMember
                        if (convertingResult)
                        {
                            xmember = new XElement("Member");
                            xmember.Add(new XAttribute("Name", name));
                            xmember.Add(new XAttribute("DataType", datatype));
                            xmember.Add(new XAttribute("Dimension", dimension));
                            xmember.Add(new XAttribute("Hidden", "false"));
                            xmember.Add(new XAttribute("ExternalAccess", "Read/Write"));
                            xMembers.Add(xmember);
                        }
                        else
                        {
                            break;
                        }
                    }

                    // If result was ok, Add new Data Type to "XModuleDefinedDataTypes" and to "referenceDataTypeNames"
                    if (convertingResult)
                    {
                        this.XModuleDefinedDataTypes.Add(xDataType);
                        referenceDataTypeNames.Add(datatypename, datatypename);
                    }
                }
            }

            return convertingResult;
        }
        /// <summary>
        /// Синтезирует тэгов модулей вводы/вывода.
        /// </summary>
        /// <returns></returns>
        private bool CreateXModuleTags()
        {
            // First checking of input variables
            if (XModules == null)
            {
                return false;
            }

            // Init of variables
            bool result = true;                         // Result of this method
            IEnumerable<XElement> xModulesWithTags;     // All controller modules with Tag Content
            IEnumerable<XElement> xCurrentModuleTags;   // All Tag contents in current xModule
            XElement xCurrentTag;                       // Current Tag in creation
            string currentModuleTagName;                // Current Module Tag Name, like buffer

            // Creation xModule Tags Element
            this.XModuleTags = new XElement("Tags");

            // Get all modules with any Tag content
            xModulesWithTags = this.XModules.Elements("Module").Where(t => t.Descendants("InputTag").Count() > 0 ||
                    t.Descendants("OutputTag").Count() > 0 || t.Descendants("ConfigTag").Count() > 0);

            foreach (XElement xCurrentModule in xModulesWithTags)
            {
                IEnumerable<XElement> xLocalPorts = xCurrentModule.Element("Ports").Elements("Port")
                    .Where(n => n.Attribute("Type").ExisitWithXValue("ICP") || n.Attribute("Type").ExisitWithXValue("Compact"));

                currentModuleTagName = "";

                #region [ Defining of Tag Type ]
                /* ============================================================================== 
                * is Module in Rack        -> [name]:[0..N]:[I or O or S or C] ... or 
                * is Module Simple         -> [name]:[I,O,S or C] ....
                */

                if (xLocalPorts != null && xLocalPorts.Count() == 1)
                {
                    currentModuleTagName = xCurrentModule.Attribute("ParentModule").GetXValue("?");

                    string icpAddres = "";
                    if (xLocalPorts.ElementAt(0).Attribute("Address").TryGetXValue(out icpAddres))
                    {
                        currentModuleTagName += ":" + icpAddres;
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result &= xCurrentModule.Attribute("Name").TryGetXValue(out currentModuleTagName);
                }

                // Return with false value if result is not set
                if (!result)
                {
                    return false;
                }
                /* ============================================================================== */
                #endregion

                #region [ Module Tags Creation: I,O,S or C ]
                /* ============================================================================== 
                 * I    -> Input Tag
                 * O    -> Output Tag
                 * S    -> Status Tag
                 * C    -> Configuration Tag
                 */
                string currdatatype = "";                       // Current Data Type
                string tagclass = "";                           // Current Tag Type
                const string TAG_CLASS_STANDARD = "Standard";
                const string TAG_CLASS_SAFETY = "Safety";

                string[] moduleTagPrefixes = { "I", "O", "S", "C" };    // Tag prefixes, I,O,S or C

                if (result)
                {
                    foreach (string tagprefix in moduleTagPrefixes)
                    {
                        xCurrentModuleTags = null;

                        // Trying to find foreach prefix Module Tag XElement
                        switch (tagprefix)
                        {
                            case "I":
                                xCurrentModuleTags = xCurrentModule.Element("Communications").Descendants("InputTag")
                                    .Where(t => !t.Parent.Attribute("Name").ExisitWithXValue("Status"));
                                break;

                            case "O":
                                xCurrentModuleTags = xCurrentModule.Element("Communications").Descendants("OutputTag")
                                    .Where(t => !t.Parent.Attribute("Name").ExisitWithXValue("Status"));
                                break;

                            case "S":
                                xCurrentModuleTags = xCurrentModule.Element("Communications").Descendants("OutputTag")
                                    .Where(t => !t.Parent.Attribute("Name").ExisitWithXValue("Status"));
                                break;

                            case "C":
                                xCurrentModuleTags = xCurrentModule.Element("Communications").Elements("ConfigTag");
                                break;
                        }

                        // If Module Tag XElements was found ->
                        if (xCurrentModuleTags != null && xCurrentModuleTags.Count() == 1)
                        {
                            IEnumerable<XElement> xcurr;        // Current intermediate XElements enumerable 

                            xCurrentTag = new XElement("Tag");
                            xCurrentTag.Add(new XAttribute("Name", currentModuleTagName + ":" + tagprefix));

                            // Creation Tag Attribute = "Class"
                            tagclass = TAG_CLASS_STANDARD;
                            if (xCurrentModuleTags.ElementAt(0).Parent.Attribute("Type").GetXValue("").StartsWith("Safety"))
                            {
                                tagclass = TAG_CLASS_SAFETY;
                            }
                            xCurrentTag.Add(new XAttribute("Class", tagclass));
                            xCurrentTag.Add(new XAttribute("TagType", "Base"));

                            // Creation Tag Attribute = "DataType"
                            currdatatype = "";
                            xcurr = xCurrentModuleTags.Elements("Data").Where(t => t.Attribute("Format").ExisitWithXValue("Decorated"));
                            if (xcurr != null && xcurr.Count() == 1)
                            {
                                currdatatype = xcurr.ElementAt(0).Element("Structure").Attribute("DataType").GetXValue("?");
                            }
                            else
                            {
                                return false;
                            }

                            xCurrentTag.Add(new XAttribute("DataType", currdatatype));
                            xCurrentTag.Add(new XAttribute("Constant", "false"));
                            xCurrentTag.Add(new XAttribute("ExternalAccess", "Read/Write"));
                            this.XModuleTags.Add(xCurrentTag);
                        }
                    }
                }
                /* ============================================================================== */
                #endregion
            }

            return result;
        }
        /// <summary>
        /// Загружает использование параметров стандарнтых инструкций.
        /// </summary>
        private void LoadStandardInstructionDefinition()
        {
            if (StandartInstructionDefinition != null)
            {
                return;
            }

            StandartInstructionDefinition = new Dictionary<string, List<LogicInstructionParameter>>();
            List<LogicInstructionParameter> p;

            #region [ Вкладка: Bit ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","",ParameterUsage.In));
            StandartInstructionDefinition.Add("XIC", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.In));
            StandartInstructionDefinition.Add("XIO", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.Out));
            StandartInstructionDefinition.Add("OTE", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.Out));
            StandartInstructionDefinition.Add("OTU", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.Out));
            StandartInstructionDefinition.Add("OTL", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.InOut));
            StandartInstructionDefinition.Add("ONS", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Storage Bit", ParameterUsage.InOut));
            p.Add(new LogicInstructionParameter("1", "Output Bit", ParameterUsage.Out));
            StandartInstructionDefinition.Add("OSR", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Storage Bit", ParameterUsage.InOut));
            p.Add(new LogicInstructionParameter("1", "Output Bit", ParameterUsage.Out));
            StandartInstructionDefinition.Add("OSF", p);
            /* ==================================================================================== */
            #endregion

            #region [ Вкладка: Timer/Counter ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Timer", ParameterUsage.InOut));
            p.Add(new LogicInstructionParameter("1", "Preset", ParameterUsage.Immediate));
            p.Add(new LogicInstructionParameter("2", "Accum", ParameterUsage.Immediate));
            StandartInstructionDefinition.Add("TON", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Timer", ParameterUsage.InOut));
            p.Add(new LogicInstructionParameter("1", "Preset", ParameterUsage.Immediate));
            p.Add(new LogicInstructionParameter("2", "Accum", ParameterUsage.Immediate));
            StandartInstructionDefinition.Add("TOF", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Timer", ParameterUsage.InOut));
            p.Add(new LogicInstructionParameter("1", "Preset", ParameterUsage.Immediate));
            p.Add(new LogicInstructionParameter("2", "Accum", ParameterUsage.Immediate));
            StandartInstructionDefinition.Add("RTO", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Counter", ParameterUsage.InOut));
            p.Add(new LogicInstructionParameter("1", "Preset", ParameterUsage.Immediate));
            p.Add(new LogicInstructionParameter("2", "Accum", ParameterUsage.Immediate));
            StandartInstructionDefinition.Add("CTU", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Counter", ParameterUsage.InOut));
            p.Add(new LogicInstructionParameter("1", "Preset", ParameterUsage.Immediate));
            p.Add(new LogicInstructionParameter("2", "Accum", ParameterUsage.Immediate));
            StandartInstructionDefinition.Add("CTD", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.Out));
            StandartInstructionDefinition.Add("RES", p);
            /* ==================================================================================== */
            #endregion

            #region [ Вкладка: Input/Output ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Message Control", ParameterUsage.InOut));
            StandartInstructionDefinition.Add("MSG", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Class name", ParameterUsage.Const));
            p.Add(new LogicInstructionParameter("1", "Instance name", ParameterUsage.Const));
            p.Add(new LogicInstructionParameter("2", "Attribute Name", ParameterUsage.Const));
            p.Add(new LogicInstructionParameter("3", "Dest", ParameterUsage.Out));
            StandartInstructionDefinition.Add("GSV", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Class name", ParameterUsage.Const));
            p.Add(new LogicInstructionParameter("1", "Instance name", ParameterUsage.Const));
            p.Add(new LogicInstructionParameter("2", "Attribute Name", ParameterUsage.Const));
            p.Add(new LogicInstructionParameter("3", "Source", ParameterUsage.In));
            StandartInstructionDefinition.Add("SSV", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Update Tag", ParameterUsage.Out));
            StandartInstructionDefinition.Add("IOT", p);
            /* ==================================================================================== */
            #endregion

            #region [ Вкладка: Compare ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Expression", ParameterUsage.Const));
            StandartInstructionDefinition.Add("CMP", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Low Limit", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Test", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("2", "High Limit", ParameterUsage.In));
            StandartInstructionDefinition.Add("LIM", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Source", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Mask", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("2", "Compare", ParameterUsage.In));
            StandartInstructionDefinition.Add("MEQ", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Source A", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Source B", ParameterUsage.In));
            StandartInstructionDefinition.Add("EQU", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Source A", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Source B", ParameterUsage.In));
            StandartInstructionDefinition.Add("NEQ", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Source A", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Source B", ParameterUsage.In));
            StandartInstructionDefinition.Add("LES", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Source A", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Source B", ParameterUsage.In));
            StandartInstructionDefinition.Add("GRT", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Source A", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Source B", ParameterUsage.In));
            StandartInstructionDefinition.Add("LEQ", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Source A", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Source B", ParameterUsage.In));
            StandartInstructionDefinition.Add("GEQ", p);
            /* ==================================================================================== */
            #endregion

            #region [ Вкладка: Compute ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.Out));
            p.Add(new LogicInstructionParameter("1", "", ParameterUsage.Const));
            StandartInstructionDefinition.Add("CPT", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("2", "", ParameterUsage.Out));
            StandartInstructionDefinition.Add("ADD", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("2", "", ParameterUsage.Out));
            StandartInstructionDefinition.Add("SUB", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("2", "", ParameterUsage.Out));
            StandartInstructionDefinition.Add("MUL", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("2", "", ParameterUsage.Out));
            StandartInstructionDefinition.Add("DIV", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("2", "", ParameterUsage.Out));
            StandartInstructionDefinition.Add("MOD", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "", ParameterUsage.Out));
            StandartInstructionDefinition.Add("SQR", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "", ParameterUsage.Out));
            StandartInstructionDefinition.Add("NEG", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "", ParameterUsage.Out));
            StandartInstructionDefinition.Add("ABS", p);
            /* ==================================================================================== */
            #endregion

            #region [ Вкладка: Move/Logical ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Source", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Dest", ParameterUsage.Out));
            StandartInstructionDefinition.Add("MOV", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Mask", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("MVM", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source A", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Source B", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("AND", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source A", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Source B", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("OR", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source A", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Source B", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("XOR", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("NOT", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Order Mode", ParameterUsage.Const) );
            p.Add(new LogicInstructionParameter("2","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("SWPB", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("CLR", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Source Bit", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Dest", ParameterUsage.Out) );
            p.Add(new LogicInstructionParameter("3","Dest Bit", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("4","Length", ParameterUsage.In) );
            StandartInstructionDefinition.Add("BTD", p);
            /* ==================================================================================== */
            #endregion

            #region [ Вкладка: File/Misc ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("1","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("2","Position", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("3","Mode", ParameterUsage.Const) );
            p.Add(new LogicInstructionParameter("4","Dest", ParameterUsage.Out) );
            p.Add(new LogicInstructionParameter("5","Expression", ParameterUsage.Const) );
            StandartInstructionDefinition.Add("FAL", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("1","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("2","Position", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("3","Mode", ParameterUsage.Const) );
            p.Add(new LogicInstructionParameter("4","Expression", ParameterUsage.Const) );
            StandartInstructionDefinition.Add("FSC", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            p.Add(new LogicInstructionParameter("2","Length", ParameterUsage.In) );
            StandartInstructionDefinition.Add("COP", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            p.Add(new LogicInstructionParameter("2","Length", ParameterUsage.In) );
            StandartInstructionDefinition.Add("FLL", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Array", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dim. To Vary", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Dest", ParameterUsage.Out));
            p.Add(new LogicInstructionParameter("3","Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("4","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("5","Position", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("AVE", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Array", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("1","Dim. To Vary", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("3","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("4","Position", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("SRT", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Array", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dim. To Vary", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Dest", ParameterUsage.Out) );
            p.Add(new LogicInstructionParameter("3","Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("4","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("5","Position", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("STD", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dim. To Vary", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Size", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("SIZE", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            p.Add(new LogicInstructionParameter("2","Length", ParameterUsage.In) );
            StandartInstructionDefinition.Add("CPS", p);
            /* ==================================================================================== */
            #endregion

            #region [ Вкладка: File/Shift ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Array", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("1","Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("2","Source Bit", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("3","Length", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("BSL", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Array", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("1","Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("2","Source Bit", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("3","Length", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("BSR", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","FIFO", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("2","Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("3","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("4","Position", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("FFL", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","FIFO", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            p.Add(new LogicInstructionParameter("2","Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("3","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("4","Position", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("FFU", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","FIFO", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("2","Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("3","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("4","Position", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("LFL", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","FIFO", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            p.Add(new LogicInstructionParameter("2","Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("3","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("4","Position", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("LFU", p);
            /* ==================================================================================== */
            #endregion

            #region [ Вкладка: Sequencer ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Array", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Mask", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("3","Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("4","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("5","Position", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("SQI", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Array", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Mask", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Dest", ParameterUsage.Out) );
            p.Add(new LogicInstructionParameter("3","Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("4","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("5","Position", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("SQO", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Array", ParameterUsage.Out) );
            p.Add(new LogicInstructionParameter("1","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("3","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("4","Position", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("SQL", p);
            /* ==================================================================================== */
            #endregion

            #region [ Вкладка: Equipment Phase ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            StandartInstructionDefinition.Add("PSC", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","", ParameterUsage.In) );
            StandartInstructionDefinition.Add("PFL", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Phase Name", ParameterUsage.Const) );
            p.Add(new LogicInstructionParameter("1","Command", ParameterUsage.Const) );
            p.Add(new LogicInstructionParameter("2","Result", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("PCMD", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Phase Name", ParameterUsage.Const) );
            StandartInstructionDefinition.Add("PCLF", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Phase Instruction", ParameterUsage.Const) );
            p.Add(new LogicInstructionParameter("1","External Request", ParameterUsage.Const) );
            p.Add(new LogicInstructionParameter("2","Data Value", ParameterUsage.InOut) );
            StandartInstructionDefinition.Add("PXRQ", p);

            p = new List<LogicInstructionParameter>();
            StandartInstructionDefinition.Add("PPD", p);

            p = new List<LogicInstructionParameter>();
            StandartInstructionDefinition.Add("PRNP", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Phase Name", ParameterUsage.Const) );
            p.Add(new LogicInstructionParameter("1","Result", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("PATT", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Phase Name", ParameterUsage.Const) );
            StandartInstructionDefinition.Add("PDET", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Phase Name", ParameterUsage.Const) );
            p.Add(new LogicInstructionParameter("1","Command", ParameterUsage.Const) );
            p.Add(new LogicInstructionParameter("2","Result", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("POVR", p);
            /* ==================================================================================== */
            #endregion

            ///* Program Control */
            //// No realized yet

            ///* For/Break */
            //// No realized yet

            #region [ Вкладка: Special ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Reference", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Result", ParameterUsage.Out) );
            p.Add(new LogicInstructionParameter("3","Comp. Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("4","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("5","Position", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("6","Result Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("7","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("8","Position", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("FBC", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Reference", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Result", ParameterUsage.Out) );
            p.Add(new LogicInstructionParameter("3","Comp. Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("4","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("5","Position", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("6","Result Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("7","Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("8","Position", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("DDT", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Mask", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Reference", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("DTR", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","PID", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("1","Process Variable", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Tieback", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("3","Control Variable", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("4","PID Master Loop", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("5","Inhold Bit", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("6","Inhold Value", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("7","Setpoint", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("8","Process Variable", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("9","Output %", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("PID", p);
            /* ==================================================================================== */
            #endregion

            #region [ Вкладка: Trig Functions ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("SIN", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("COS", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("TAN", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("ASN", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("ACS", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("ATN", p);
            /* ==================================================================================== */
            #endregion

            #region [ Вкладка: Advanced Math ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("LN", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("LOG", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source X", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Source Y", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("XPY", p);
            /* ==================================================================================== */
            #endregion

            #region [ Вкладка: Math Conversations ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("DEG", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("RAD", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("TOD", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("FRD", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("TRN", p);
            /* ==================================================================================== */
            #endregion

            ///* Motion State */
            //// No realized yet

            ///* Motion Move */
            //// No realized yet

            ///* Motion Group */
            //// No realized yet

            ///* Motion Event */
            //// No realized yet

            ///* Motion Config */
            //// No realized yet

            ///* Motion Coordinated */
            //// No realized yet

            #region [ Вкладка: ASCII Serial Port ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Channel", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Serial Port Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("3","Serial Port Control Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("4","Characters Sent", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("AWT", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Channel", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Serial Port Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("3","Serial Port Control Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("4","Characters Sent", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("AWA", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Channel", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Destination", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Serial Port Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("3","Serial Port Control Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("4","Characters Sent", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("ARD", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Channel", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Destination", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Serial Port Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("3","Serial Port Control Length", ParameterUsage.Immediate) );
            p.Add(new LogicInstructionParameter("4","Characters Sent", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("ARL", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Channel", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Serial Port Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("2","Characters Count", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("ABL", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Channel", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Serial Port Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("2","Characters Count", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("ACB", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Channel", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","AND Mask", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","OR Mask", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("3","Serial Port Control", ParameterUsage.InOut) );
            p.Add(new LogicInstructionParameter("4","Channel Status(Decimal)", ParameterUsage.Immediate) );
            StandartInstructionDefinition.Add("AHL", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Channel", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Clear Serial Port Read", ParameterUsage.Const) );
            p.Add(new LogicInstructionParameter("2","Clear Serial Port Write", ParameterUsage.Const) );
            StandartInstructionDefinition.Add("ACL", p);
            /* ==================================================================================== */
            #endregion

            #region [ Вкладка: ASCII String ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Search", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Start", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("3","Result", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("FIND", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source A", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Source B", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Start", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("3","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("INSERT", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source A", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Source B", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("CONCAT", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Qty", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Start", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("3","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("MID", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0","Source", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("1","Qty", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("2","Start", ParameterUsage.In) );
            p.Add(new LogicInstructionParameter("3","Dest", ParameterUsage.Out) );
            StandartInstructionDefinition.Add("DELETE", p);
            /* ==================================================================================== */
            #endregion

            #region [ Вкладка: ASCII Conversation ]
            /* ==================================================================================== */
            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Source", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Dest", ParameterUsage.Out));
            StandartInstructionDefinition.Add("DTOS", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Source", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Dest", ParameterUsage.Out));
            StandartInstructionDefinition.Add("STOD", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Source", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Dest", ParameterUsage.Out));
            StandartInstructionDefinition.Add("RTOS", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Source", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Dest", ParameterUsage.Out));
            StandartInstructionDefinition.Add("STOR", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Source", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Dest", ParameterUsage.Out));
            StandartInstructionDefinition.Add("UPPER", p);

            p = new List<LogicInstructionParameter>();
            p.Add(new LogicInstructionParameter("0", "Source", ParameterUsage.In));
            p.Add(new LogicInstructionParameter("1", "Dest", ParameterUsage.Out));
            StandartInstructionDefinition.Add("LOWER", p);
            /* ==================================================================================== */
            #endregion

            ///* Debug */
            //// No realized yet
        }
        /// <summary>
        /// Конвертирование Типов данных из XML элементов.
        /// </summary>
        private void LoadDataTypes()
        {
            DataTypes.Clear();
            // Преобразование User Data Types.
            foreach (XElement xdt in this.XDataTypes.Elements())
            {
                DataType dt = new DataType(xdt);
                this.DataTypes.Set(dt.ID, dt);
                dt.Parrent = this;
            }
            // Преобразование Predefined Data Types.
            foreach (XElement xdt in this.XPredefinedDataTypes.Elements())
            {
                DataType dt = new DataType(xdt);
                this.DataTypes.Set(dt.ID, dt);
                dt.Parrent = this;
            }
            // Преобразование Add-On instruction Data Types.
            foreach (XElement xdt in this.XAddOnInstructionDefinitions.Elements())
            {
                DataType dt = new DataType(xdt);
                this.DataTypes.Set(dt.ID, dt);
                dt.Parrent = this;
            }
            // Преобразование Module Defined Data Types.
            foreach (XElement xdt in this.XModuleDefinedDataTypes.Elements())
            {
                DataType dt = new DataType(xdt);
                this.DataTypes.Set(dt.ID, dt);
                dt.Parrent = this;
            }
        }
        /// <summary>
        /// Определяет размер типа данных и просчитывает распределение элементов в пространстве памяти.
        /// </summary>
        private void DefineDataTypeSize()
        {
            foreach (DataType dt in this.DataTypes.Values)
            {
                // Рассчет размера типа данных и вычисление распределение памяти членов типа данных.
                dt.DefineMemoryCellAndSize(this.DataTypes);
            }
        }
        /// <summary>
        /// Конвертирование Контроллерных тэгов из XML элементов.
        /// </summary>
        private void LoadTags()
        {
            this.Tags.Clear();
            // Преобразование тэгов модулей ввода/вывода.
            foreach (XElement xtag in this.XModuleTags.Elements("Tag"))
            {
                Tag tag = new Tag(xtag);
                this.Tags.Set(tag.ID, tag);
                tag.Parrent = this;
            }
            // Преобразование контроллерных тэгов.
            foreach (XElement xtag in this.XTags.Elements("Tag"))
            {
                Tag tag = new Tag(xtag);
                this.Tags.Set(tag.ID, tag);
                tag.Parrent = this;
            }
        }
        /// <summary>
        /// Загружает карту привязки стандартных тэгов с тэгами безопасности.
        /// </summary>
        private void LoadSefetyMap()
        {
            this.SafetyTagMap.Clear();

            if (this.XSafetyInfo == null)
            {
                return;
            }

            if (this.XSafetyInfo.Element("SafetyTagMap") == null)
            {
                return;
            }

            // Загружаем строку разметки и удаляем все пробелы.
            string safetyTagMapString = this.XSafetyInfo.Element("SafetyTagMap").GetXValue("");
            safetyTagMapString = safetyTagMapString.Replace(" ", "");

            // Делим на парные равенства разделенные зяпятой.
            string[] safetyTagMapItems = safetyTagMapString.Split(",".ToCharArray());

            // Для каждой пары создаем новый объект равенства тэгов SafetyTagMap.
            foreach (string item in safetyTagMapItems)
            {
                string[] itemParts = item.Split("=".ToCharArray());
                if (itemParts.Length == 2 && itemParts.All(t => t.Length > 0))
                {
                    TagMember standardTag = Tag.Get(itemParts[0], null, this);
                    TagMember safetyTag = Tag.Get(itemParts[1], null, this);

                    this.SafetyTagMap.Add(new SafetyTagMap(standardTag, safetyTag));
                }
                else
                {
                    // TODO. Need to add error.
                }
            }
        }
        /// <summary>
        /// Строит полную структуру всех тэгов вплоть до элемента BOOL.
        /// </summary>
        /// <param name="tagBuildMethod"></param>
        private void BuildTagsStructure(TagBuildMethod tagBuildMethod)
        {
            if (tagBuildMethod == TagBuildMethod.COMPLETE)
            {
                List<Tag> allTags = new List<Tag>();
                allTags.AddRange(this.Tags.Values.ToList());
                allTags.AddRange(this.Tasks.SelectMany(p => p.FindByLines(j => true).OfType<LogicProgram>()).SelectMany(t => t.Tags.Values));

                allTags.ForEach(t => t.BuildStructure());
            }
        }
        /// <summary>
        /// Конвертирование Логики из XML элементов.
        /// </summary>
        private void LoadLogic()
        {
            this.Tasks.Clear();
            // Все программы, использованные будут исключены, оставшиеся будут определены как неразмеченные.
            Dictionary<string, LogicProgram> programs = this.XPrograms.Elements()
                .Select(p => new LogicProgram(p)).ToDictionary(k => k.ID, v => v);

            string programName;
            string taskName;

            // Ставим в соответствие прогаммы и задачи.
            foreach (XElement xtask in XTasks.Elements())
            {
                // Создаем новую задачу.
                LogicTask task = new LogicTask(xtask);
                // Просматриваем программы которые имеются в текущей задачи как вызываемые.
                XElement xScheduledPrograms = xtask.Element("ScheduledPrograms");
                if (xScheduledPrograms != null)
                {
                    foreach (XElement xSheduledProgram in xScheduledPrograms.Elements("ScheduledProgram"))
                    {
                        programName = xSheduledProgram.Attribute("Name").GetXValue("");
                        taskName = xtask.Attribute("Name").GetXValue("");
                        // 
                        if (programs.ContainsKey(programName))
                        {
                            task.Add(programs[programName]);
                            programs.Remove(programName);
                        }
                    }
                }
                // Добавляем задачу в контейнер задач.
                this.Add(task);
                this.Tasks.Add(task);
            }

            // Создаем новую специальную задачу "Power-Up Handler".
            LogicTask powerUpHandler = new LogicTask("Power-Up Handler");
            powerUpHandler.Type = TaskType.POWER_UP_HANDLER;
            powerUpHandler.Parrent = this;
            this.Tasks.Add(powerUpHandler);

            programName = this.XController.Attribute("PowerLossProgram").GetXValue("");
            if (programName.Trim() != "")
            {
                if (programs.ContainsKey(programName))
                {
                    powerUpHandler.Add(programs[programName]);
                    programs.Remove(programName);
                }
            }
            // Создаем новую специальную задачу "Controller Fault Handler".
            LogicTask controllerFaultHandler = new LogicTask("Controller Fault Handler");
            controllerFaultHandler.Type = TaskType.CONTROLLER_FAULT_HANDLER;
            controllerFaultHandler.Parrent = this;
            this.Tasks.Add(controllerFaultHandler);

            programName = this.XController.Attribute("MajorFaultProgram").GetXValue("");
            if (programName.Trim() != "")
            {
                if (programs.ContainsKey(programName))
                {
                    controllerFaultHandler.Add(programs[programName]);
                    programs.Remove(programName);
                }
            }

            this.UnscheduledPrograms = programs.Values.ToList();
        }
        /// <summary>
        /// Конвертирование Add-On instructions из XML элементов.
        /// </summary>
        private void LoadAddons()
        {
            this.AddonInstructions.Clear();
            foreach (XElement xaddon in this.XAddOnInstructionDefinitions.Elements("AddOnInstructionDefinition"))
            {
                AddonInstruction addonInstruction = new AddonInstruction(xaddon);
                this.AddonInstructions.Set(addonInstruction.ID, addonInstruction);
            }
        }
        /// <summary>
        /// Распознование строковых параметров инструкций.
        /// </summary>
        private void DefineInstructionParameters(TagBuildMethod tagBuildMethod)
        {
            // Распознавание параметров заданных как тэги в инструкциях и происвоенеи объекта тэга.
            for (int ix = 0; ix < this.Tasks.Count; ix++)
            {
                // Текущая обрабатываемая задача.
                LogicTask task = this.Tasks[ix];
                // Для всех созданных инструкций распознаем параметры и устанавливаем объекты тэгов если это есть тэг.
                List<LogicInstruction> allInstructionInTask = task.FindByLines(j => true).OfType<LogicInstruction>().ToList();

                foreach (LogicInstruction currLogicInstruction in allInstructionInTask)
                {
                    if (tagBuildMethod == TagBuildMethod.BY_INSTRUCTION_USING)
                    {
                        #region[ 1. Распознавание строковых значений как объекты тэгов. ]
                        /* ======================================================================== */
                        // 1. Распознавание строковых значений как объекты тэгов.
                        LogicProgram program = null;
                        List<LogicProgram> findedPrograms = currLogicInstruction.FindParrents(t => t is LogicProgram).OfType<LogicProgram>().ToList();
                        if (findedPrograms.Count == 1)
                        {
                            program = findedPrograms.First();
                        }

                        for (int paramIndex = 0; paramIndex < currLogicInstruction.Parameters.Count; paramIndex++)
                        {
                            LogicInstructionParameter instructionParameter = currLogicInstruction.Parameters[paramIndex];
                            if (instructionParameter.Value is string)
                            {
                                // Получаем тэг по заданному имени.
                                TagMember t = Tag.Get(instructionParameter.ToString(), program, this);
                                if (t != null)
                                {
                                    // Присваиваем значению параметра инструкции ссылку на тэг.
                                    instructionParameter.Value = t;
                                }
                            }
                        }
                        /* ======================================================================== */
                        #endregion
                    }

                    #region [ 2. Заполнение свойств параметров инструкций. ]
                    /* ======================================================================== */
                    // 2. Заполнение свойств параметров инструкций.
                    if (currLogicInstruction.IsAddon)
                    {
                        #region [ 2.1. Add-On Instruction. ]
                        /* ======================================================================== */
                        AddonInstruction currAddonInstruction;
                        if (this.AddonInstructions.TryGetValue(currLogicInstruction.Name, out currAddonInstruction))
                        {
                            List<DataTypeAddonParameter> addonRequredParameters = currAddonInstruction.Parameters.Values.Where(t => t.Required).ToList();
                            if (currLogicInstruction.Parameters.Count > 0 && currLogicInstruction.Parameters.Count == addonRequredParameters.Count + 1)
                            {
                                currLogicInstruction.Parameters[0].Name = "...";  // TODO: Необходимо реализовать присвоение имен.
                                currLogicInstruction.Parameters[0].Usage = ParameterUsage.InOut;


                                for (int paramIx = 0; paramIx < addonRequredParameters.Count; paramIx++)
                                {
                                    currLogicInstruction.Parameters[paramIx + 1].Name = addonRequredParameters[paramIx].ID;
                                    switch (addonRequredParameters[paramIx].Usage)
                                    {
                                        case ParameterUsage.In:
                                            currLogicInstruction.Parameters[paramIx + 1].Usage = ParameterUsage.In;
                                            break;

                                        case ParameterUsage.Out:
                                            currLogicInstruction.Parameters[paramIx + 1].Usage = ParameterUsage.Out;
                                            break;

                                        case ParameterUsage.InOut:
                                            currLogicInstruction.Parameters[paramIx + 1].Usage = ParameterUsage.InOut;
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                // TODO: Ошибка: Несоответствие между колличеством параметров.
                            }
                        }
                        /* ======================================================================== */
                        #endregion
                    }
                    else
                    {
                        #region [ 2.2. Standard Instruction. ]
                        /* ======================================================================== */
                        // Поиск в стандартных инструкциях по имени и присвоение свойств данного параметра.
                        if (LogixL5X.StandartInstructionDefinition != null && currLogicInstruction.Name != null)
                        {
                            List<LogicInstructionParameter> parameters;
                            if (LogixL5X.StandartInstructionDefinition.TryGetValue(currLogicInstruction.Name, out parameters))
                            {
                                if (currLogicInstruction.Parameters.Count == parameters.Count)
                                {
                                    for (int paramIx = 0; paramIx < parameters.Count; paramIx++)
                                    {
                                        // currLogicInstruction.Parameters[paramIx].Name = parameters[paramIx].Name;    // TODO: Необходимо реализовать присвоение имен.
                                        currLogicInstruction.Parameters[paramIx].Usage = parameters[paramIx].Usage;
                                    }
                                }
                                else
                                {
                                    // TODO: Ошибка: Несоответствие между колличеством параметров.
                                }
                            }
                        }
                                                /* ======================================================================== */
                        #endregion
                    }
                    /* ======================================================================== */
                    #endregion

                }
            }
        }
        /// <summary>
        /// Построение перекрестных ссылок.
        /// </summary>
        private void BuildCrossReference()
        {
            // Построение перекресных ссылок тэгов на логические узлы их использования.
            List<LogicInstruction> allInstructions = this.Tasks.SelectMany(task => task.FindByLines(j => true).OfType<LogicInstruction>()).ToList();
            List<LogicInstructionParameter> allParameters = allInstructions.SelectMany(i => i.FindByLines(j => true).OfType<LogicInstructionParameter>()).ToList();
            List<LogicInstructionParameter> allParametersWithTags = allParameters.Where(p => p.Value is TagMember).ToList();

            // Группируем все контроллерные тэги и тэги программ.
            List<Tag> allTags = this.Tags.Values.ToList();
            allTags.AddRange(this.Tasks.SelectMany(p => p.FindByLines(j => true).OfType<LogicProgram>()).SelectMany(t => t.Tags.Values));

            #region [ Tag -> Tag (Использование тэга как Alias) ]
            /* ==================================================================================== */
            foreach (Tag tag in allTags.Where(t => t.Type == TagType.ALIAS))
            {
                if (tag.AliasFor != null && tag.AliasFor is TagMember)
                {
                    ((TagMember)tag.AliasFor).CrossRefference.Add(new CrossReferenceItem(CrossReferenceType.ToTagAlias, tag));
                }
            }
            /* ==================================================================================== */
            #endregion

            #region [ Tag -> Logic Instruction Parameter ]
            /* ==================================================================================== */
            // Ссылка в "Тэгах" на "Парамтры инструкций" где используются эти тэги.
            for (int ix = 0; ix < allParametersWithTags.Count; ix++)
            {
                LogicInstructionParameter currParameter = (LogicInstructionParameter)allParametersWithTags[ix];
                LogicInstruction instr = currParameter.GetParrent<LogicInstruction>();
                TagMember parameterTag = (TagMember)allParametersWithTags[ix].Value;
                TagMember parrentOfTag = parameterTag.GetParrent<TagMember>();

                // В случае если используются инструкции с внутренней косвиной адресацией, например как COS, CPS,
                // где как раз для массивов указывается только стартовый индекс массива, то добавляем ссылку еще
                // к родительскому тэгу который как раз и является массивом.
                if (instr != null && (instr.ID == "COP" || instr.ID == "CPS"))
                {
                    if ((currParameter.ID == "Source" || currParameter.ID == "Dest") && parameterTag.ArrayIndex != null)
                    {
                        if (parrentOfTag != null && parrentOfTag.Dimension != null)
                        {
                            parrentOfTag.CrossRefference.Add(new CrossReferenceItem(CrossReferenceType.ToInstructionParameter, currParameter));
                        }
                    }
                }

                parameterTag.CrossRefference.Add(new CrossReferenceItem(CrossReferenceType.ToInstructionParameter, currParameter));
            }
            /* ==================================================================================== */
            #endregion

            #region [ Tag -> Tag (Использование в качестве индексации массива) ]
            /* ==================================================================================== */
            foreach (Tag tag in allTags)
            {
                foreach (TagMember member in tag.FindByLines(t => 
                    (t is TagMember) && ((TagMember)t).ArrayIndex != null && !((TagMember)t).ArrayIndex.IsExplict)
                    .OfType<TagMember>())
                {
                    foreach (TagMember indexMember in member.ArrayIndex.Indexes.Where(o => o is TagMember))
                    {
                        indexMember.CrossRefference.Add(new CrossReferenceItem(CrossReferenceType.ToTagArrayIndex, member));
                    }
                }
            }
            /* ==================================================================================== */
            #endregion

            #region [ DataType -> DataTypeMember, DataTypeMember -> DataType ]
            /* ==================================================================================== */
            List<DataTypeMember> dataTypeMembers = this.DataTypes.Values.SelectMany(t => t.GetChilds<DataTypeMember>()).ToList();

            // Добавляем в DataType ссылки на DataTypeMember которые их используют.
            // Добавляем в DataTypeMember ссылки на DataType которые он является.
            for (int ix = 0; ix < dataTypeMembers.Count; ix++)
            {
                DataTypeMember currDataTypeMember = dataTypeMembers[ix];
                DataType dataType;
                if (this.DataTypes.TryGetValue(currDataTypeMember.DataType, out dataType))
                {
                    dataType.CrossRefference.Add(new CrossReferenceItem(CrossReferenceType.ToDataTypeMember, currDataTypeMember));
                    currDataTypeMember.CrossRefference.Add(new CrossReferenceItem(CrossReferenceType.ToDataType, dataType));
                }
            }
            /* ==================================================================================== */
            #endregion

            #region [ Ссылки на "DATA TYPE" и "DATA TYPE MEMBER" ]
            /* ==================================================================================== */
            // Добавляем в DataType ссылки на TagMember которые их используют.
            // Добавляем в DataTypeMember ссылки на TagMember которые их используют.
            for (int ix = 0; ix < allTags.Count; ix++)
            {
                // Для текущего тэга находим все его построенные члены по всей структуры дерева.
                Tag currTag = allTags[ix];
                List<TagMember> tagMembers = currTag.FindByArcs(c => c is TagMember).OfType<TagMember>().ToList();

                // Перебираем все члены структуры дерева.
                foreach (TagMember tagMember in tagMembers)
                {
                    // Временная переменная текущего члена тэга.
                    TagMember currTagMember = tagMember;

                    // Для текущего члена тэга ищем тип данных и добавляем с список ссылок данный член.
                    DataType dataType;
                    if (this.DataTypes.TryGetValue(currTagMember.DataType, out dataType))
                    {
                        dataType.CrossRefference.Add(new CrossReferenceItem(CrossReferenceType.ToTag, currTagMember));
                    }

                    // Для текущего члена пытаемся получить родительский элемент.
                    TagMember memberParrent = currTagMember.GetParrent<TagMember>();

                    // Если элемент tagMember является элементом массива, а родительский элемент memberParrent должен являеться Массивом.
                    // То перемещаем memberParrent еще выше к родительскому элементу. И текущий тэг tagMember перемещаем еще выше на один.
                    if (currTagMember.ArrayIndex != null && memberParrent != null && memberParrent.Dimension != null)
                    {
                        currTagMember = memberParrent;
                        memberParrent = currTagMember.GetParrent<TagMember>();
                    }

                    if (memberParrent != null)
                    {
                        // Находим тип данных для родительского элемента и пытаемся получить член типа данных
                        // с таким же именем каким является член тэга.
                        DataType parrentDataType;
                        if (this.DataTypes.TryGetValue(memberParrent.DataType, out parrentDataType))
                        {
                            DataTypeMember findedDataTypeMember = parrentDataType.GetChild<DataTypeMember>(currTagMember.ID);
                            if (findedDataTypeMember != null)
                            {
                                findedDataTypeMember.CrossRefference.Add(new CrossReferenceItem(CrossReferenceType.ToTag, currTagMember));
                            }
                        }
                    }
                }
            }
            /* ==================================================================================== */
            #endregion

            #region [ Tag -> SafetyTagInfo ]
            /* ==================================================================================== */
            foreach (SafetyTagMap map in this.SafetyTagMap)
            {
                if (map.SafetyTag.Value is TagMember)
                {
                    ((TagMember)map.SafetyTag.Value).CrossRefference.Add(new CrossReferenceItem(CrossReferenceType.ToSafetyMap, map.SafetyTag));
                }

                if (map.StandardTag.Value is TagMember)
                {
                    ((TagMember)map.StandardTag.Value).CrossRefference.Add(new CrossReferenceItem(CrossReferenceType.ToSafetyMap, map.StandardTag));
                }
            }
            /* ==================================================================================== */
            #endregion


            // Для всех определенных Add-On Instruction определяем место их использования в логике.
            for (int ix = 0; ix < allInstructions.Count; ix++)
            {
                LogicInstruction currentInstruction = allInstructions[ix];
                AddonInstruction addonInstruction;
                if (this.AddonInstructions.TryGetValue(currentInstruction.Name, out addonInstruction))
                {
                    addonInstruction.CrossRefference.Add(new CrossReferenceItem(CrossReferenceType.ToInstruction, currentInstruction));
                }
            }
        }
        /* ============================================================================== */
        #endregion

        #region [ ПРИВАТНЫЕ МЕТОДЫ : "РАСШИФРОВКА ДАННЫХ" ]
        /* ============================================================================== */
        /// <summary>
        /// Try to decode Base 64 String to Unicode Encoding
        /// </summary>
        /// <param name="input64String">Input Base 64 String</param>
        /// <param name="key">Byte array - Key</param>
        /// <param name="outstr">Resulted Unicode String</param>
        /// <returns></returns>
        private static bool TryDecode64StringToUnicode(string input64String, Byte[] key, out string outstr)
        {
            bool result = false;                // Result of operation
            outstr = "";                        // Output string
            string tempstr = input64String;

            try
            {
                byte[] bytes = Convert.FromBase64String(tempstr);
                RC4(ref bytes, key);
                outstr = Encoding.Unicode.GetString(bytes);
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }
        /// <summary>
        /// Function for uncryption xml node "EncodingData" with @EncryptionConfig = null
        /// </summary>
        /// <param name="input64String">Input string</param>
        /// <param name="outputString">Output string</param>
        /// <returns></returns>
        private static bool UncryptEncodedDataMethod1(string input64String, out string outputString)
        {
            outputString = "";      // Preset empty string
            byte[] keyHash, key;    // Keys byte arrays

            // Creating SHA Hash from string constant key
            SHA1 sha = new SHA1CryptoServiceProvider();
            keyHash = sha.ComputeHash(RadixConverter.BytesFromHexString(KEY_01));

            // Creating of finished Key for RC4 encoding
            key = RadixConverter.BytesFromHexString(RadixConverter.ToHexString(keyHash).Substring(0, 10) + "0000000000000000000000");

            // Try to decode input string with Key by RC4
            return TryDecode64StringToUnicode(input64String, key, out outputString);
        }
        /// <summary>
        /// Function for uncryption xml node "EncodingData" with @EncryptionConfig = 2
        /// </summary>
        /// <param name="input64String">Input string</param>
        /// <param name="outputString">Output string</param>
        /// <returns></returns>
        private static bool UncryptEncodedDataMethod2(string input64String, out string outputString)
        {
            outputString = "";      // Preset empty string
            byte[] keyHash, key;    // Keys byte arrays

            // Creating SHA Hash from string constant key
            SHA1 sha = new SHA1CryptoServiceProvider();
            keyHash = sha.ComputeHash(RadixConverter.BytesFromHexString(KEY_02));

            // Creating of finished Key for RC4 encoding
            key = RadixConverter.BytesFromHexString(RadixConverter.ToHexString(keyHash).Substring(0, 10) + "0000000000000000000000");

            // Try to decode input string with Key by RC4
            return TryDecode64StringToUnicode(input64String, key, out outputString);
        }
        /// <summary>
        /// Main RC4 Crypto process for Encode/Decode
        /// </summary>
        /// <param name="bytes">Reference byte array for code/encode</param>
        /// <param name="key">Byte array - Key</param>
        private static void RC4(ref Byte[] bytes, Byte[] key)
        {
            Byte[] s = new Byte[256];
            Byte[] k = new Byte[256];
            Byte temp;
            int i, j;

            for (i = 0; i < 256; i++)
            {
                s[i] = (Byte)i;
                k[i] = key[i % key.GetLength(0)];
            }

            j = 0;
            for (i = 0; i < 256; i++)
            {
                j = (j + s[i] + k[i]) % 256;
                temp = s[i];
                s[i] = s[j];
                s[j] = temp;
            }

            i = j = 0;
            for (int x = 0; x < bytes.GetLength(0); x++)
            {
                i = (i + 1) % 256;
                j = (j + s[i]) % 256;
                temp = s[i];
                s[i] = s[j];
                s[j] = temp;
                int t = (s[i] + s[j]) % 256;
                bytes[x] ^= s[t];
            }
        }
        /* ============================================================================== */
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagname"></param>
        /// <returns></returns>
        public bool RecognizeTagGeneratsTag(string tagname)
        {
            // First checking of input parameters
            if (tagname == null || tagname.Trim() == "")
            {
                return false;
            }

            // 1. Checking on existing in Controller tags
            IEnumerable<XElement> xFindedTags = this.XTags.Elements("Tag").Where(t => t.Attribute("Name").ExisitWithXValue(tagname));
            if (!xFindedTags.ElementIsAlone())
            {
                return false;
            }

            // 2. Check if tag name start with [module name] + "_"
            string[] moduleNames = this.XModules.Elements("Module").Select(m => m.Attribute("Name").GetXValue("")).Where(s => s != "").ToArray();
            return moduleNames.Any(m => tagname.StartsWith(m + "_"));
        }
    }
}
