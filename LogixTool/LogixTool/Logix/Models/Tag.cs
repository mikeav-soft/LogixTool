using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LogixTool.Common;

namespace LogixTool.Logix.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Tag : TagMember
    {
        #region [ PROPERTIES ]
        /* ============================================================================== */
        /// <summary>
        /// 
        /// </summary>
        private string MESSAGE_HEADER
        {
            get
            {
                return "Tag : " + this.ID;
            }
        }

        /// <summary>
        /// Программа к которой принадлежит тэг в случае если это локальный тэг программы.
        /// </summary>
        public LogicProgram Program { get; set; }
        /// <summary>
        /// Специальные параметры тэга в случае типа Producer.
        /// </summary>
        public ProduceInfo ProduceInfo { get; set; }
        /// <summary>
        /// Специальные параметры тэга в случае типа Consumer.
        /// </summary>
        public ConsumeInfo ConsumeInfo { get; set; }
        /// <summary>
        /// Специальные параметры тэга типа Message.
        /// </summary>
        public MessageParameters MessageParameters { get; set; }
        /// <summary>
        /// Разновидность тэга.
        /// </summary>
        public TagType Type { get; set; }
        /// <summary>
        /// Класс тэга.
        /// </summary>
        public LogicClass Class { get; set; }
        /// <summary>
        /// Имя тэга на который ссылается данный тэг в случае Alias типа.
        /// </summary>
        public object AliasFor { get; set; }
        /// <summary>
        /// Комментарии к тэгам.
        /// </summary>
        public Dictionary<string, LangText> Comments { get; set; }
        /* ============================================================================== */
        #endregion

        /// <summary>
        /// Создает новый объект тэга.
        /// </summary>
        public Tag()
            : base("")
        {
            this.Program = null;
            this.ProduceInfo = null;
            this.ConsumeInfo = null;
            this.MessageParameters = null;
            this.Type = TagType.NULL;
            this.AliasFor = null;
            this.Comments = new Dictionary<string, LangText>();
            this.SourceMemorySpace = new List<byte>();
            this.Class = LogicClass.NULL;
        }
        /// <summary>
        /// Создает новый объект тэга из XML элемента.
        /// </summary>
        /// <param name="xmember"></param>
        public Tag(XElement xtag)
            : this()
        {
            if (!xtag.ExistAs("Tag") && !xtag.ExistAs("LocalTag"))
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "Source XML Element is not a \"Tag\" or \"LocalTag\" or is Null."));
                return;
            }

            string xTagElementName = xtag.Name.LocalName;

            // Преобразование Name.
            this.ID = xtag.Attribute("Name").GetXValue("");
            // Преобразование DataType.
            this.DataType = xtag.Attribute("DataType").GetXValue("");
            // Преобразование Description.
            this.Description = new LangText(xtag.Element("Description"));
            // Преобразование Radix.
            string radixValue = xtag.Attribute("Radix").GetXValue("").ToLower();
            switch (radixValue)
            {
                case "":
                    this.Radix = TagRadix.NULL;
                    break;

                case "nulltype":
                    this.Radix = TagRadix.NULL;
                    break;

                case "binary":
                    this.Radix = TagRadix.BINARY;
                    break;

                case "decimal":
                    this.Radix = TagRadix.DECIMAL;
                    break;

                case "octal":
                    this.Radix = TagRadix.OCTAL;
                    break;

                case "hex":
                    this.Radix = TagRadix.HEX;
                    break;

                case "float":
                    this.Radix = TagRadix.FLOAT;
                    break;

                case "exponential":
                    this.Radix = TagRadix.EXPONENTIAL;
                    break;

                case "ascii":
                    this.Radix = TagRadix.ASCII;
                    break;

                case "date/time":
                    this.Radix = TagRadix.DATE_TIME;
                    break;

                default:
                    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"" + xTagElementName + "\", Name='" + this.ID + "', contains XML Element \"Radix\" with undefined value = '" + radixValue + "'"));
                    break;
            }

            // Преобразование TagClass.
            string classValue = xtag.Attribute("Class").GetXValue("").ToLower();
            switch (classValue)
            {
                case "standard":
                    this.Class = LogicClass.STANDARD;
                    break;

                case "safety":
                    this.Class = LogicClass.SAFETY;
                    break;

                default:
                    if (xTagElementName == "Tag")
                    {
                        LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"" + xTagElementName + "\", Name='" + this.ID + "', contains XML Element \"Class\" with undefined value = '" + classValue + "'"));
                    }
                    break;
            }

            // Преобразование ExternalAccess.
            string externalAccessValue = xtag.Attribute("ExternalAccess").GetXValue("").ToLower();
            switch (externalAccessValue)
            {
                case "":
                    this.ExternalAccess = TagExternalAccess.NULL;
                    break;

                case "read/write":
                    this.ExternalAccess = TagExternalAccess.READ_WRITE;
                    break;

                case "read only":
                    this.ExternalAccess = TagExternalAccess.READ_ONLY;
                    break;

                case "none":
                    this.ExternalAccess = TagExternalAccess.NONE;
                    break;

                default:
                    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"" + xTagElementName + "\", Name='" + this.ID + "', contains XML Element \"ExternalAccess\" with undefined value = '" + externalAccessValue + "'"));
                    break;
            }
            // Преобразование Dimension.
            if (xtag.Attribute("Dimensions") != null)
            {
                ArrayDefinition dimensionDefinition = new ArrayDefinition(xtag.Attribute("Dimensions").GetXValue(""));
                if (dimensionDefinition.Length > 0)
                {
                    this.Dimension = dimensionDefinition;
                }
            }
            // Преобразование Типа тэга.
            string tagTypeValue = xtag.Attribute("TagType").GetXValue("").ToLower();
            switch (tagTypeValue)
            {
                case "base":
                    this.Type = TagType.BASE;
                    break;

                case "alias":
                    this.Type = TagType.ALIAS;
                    break;

                case "produced":
                    this.Type = TagType.PRODUCED;
                    break;

                case "consumed":
                    this.Type = TagType.CONSUMED;
                    break;

                default:
                    if (xTagElementName == "Tag")
                    {
                        LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"" + xTagElementName + "\", Name='" + this.ID + "', contains XML Element \"TagType\" with undefined value = '" + tagTypeValue + "'"));
                    }
                    break;
            }
            // Преобразование имени Alias.
            this.AliasFor = xtag.Attribute("AliasFor").GetXValue(null);
            // Преобразование Комментариев пользователя.
            if (xtag.Element("Comments") != null)
            {
                foreach (XElement xcomment in xtag.Element("Comments").Elements("Comment"))
                {
                    string operand = xcomment.Attribute("Operand").GetXValue("");
                    this.Comments.Set(operand, new LangText(xcomment));
                }
            }
            // Преобразование Values.
            this.SourceMemorySpace.Clear();
            IEnumerable<XElement> xdatas = xtag.Elements("Data").Where(t => t != null && t.Attributes().Count() == 0);
            if (xdatas.Count() == 1)
            {
                string data = xdatas.First().Value.ToUpper();
                string[] datas = data.Split(" \r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                if (datas.All(t => t.Length == 2) && datas.All(t => t.All(b => "0123456789ABCDEF".Contains(b))))
                {
                    this.SourceMemorySpace = RadixConverter.BytesFromHexString(String.Join("", datas)).ToList();
                }
                else
                {
                    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"" + xTagElementName + "\", Name='" + this.ID + "', contains XML Element \"Data\" with undefined value = '" + String.Join(" ", datas) + "'"));
                }
            }
            // Преобразование ProduceInfo.
            if (this.Type == TagType.PRODUCED && xtag.Element("ProduceInfo") != null)
            {
                this.ProduceInfo = new ProduceInfo(xtag.Element("ProduceInfo"));
            }
            // Преобразование ConsumeInfo.
            if (this.Type == TagType.CONSUMED && xtag.Element("ConsumeInfo") != null)
            {
                this.ConsumeInfo = new ConsumeInfo(xtag.Element("ConsumeInfo"));
            }
            // Преобразование MessageParameters.
            if (xtag.Element("Data") != null && xtag.Element("Data").Attribute("Format").ExisitWithXValue("Message") && xtag.Element("Data").Element("MessageParameters") != null)
            {
                this.MessageParameters = new MessageParameters(xtag.Element("Data").Element("MessageParameters"));
            }
        }

        #region [ PUBLIC METHODS METHODS ]
        /* ============================================================================== */
        /// <summary>
        /// Копирует поля в новый объект, где связи пусты.
        /// </summary>
        /// <returns></returns>
        public new Tag Clone()
        {
            Tag tag = (Tag)this.MemberwiseClone();
            tag.Childrens = new Dictionary<string, Tree>();
            tag.Parrent = null;
            tag.CrossRefference = new List<CrossReferenceItem>();

            return tag;
        }
        /// <summary>
        /// Строит полную структуру данного тэга до самых простых тэгов.
        /// </summary>
        /// <param name="dataTypes">Существующие типы данных.</param>
        public void BuildStructure()
        {
            LogixL5X root = this.GetRoot<LogixL5X>();
            if (root == null || root.DataTypes == null)
            {
                return;
            }

            Tag.Bulid(this, root.DataTypes);
        }
        /// <summary>
        /// Получает тэг по заданному имени.
        /// </summary>
        /// <param name="tagName">Название тэга.</param>
        /// <param name="program">Текущая программа в которой происходит поиск тэга.</param>
        /// <param name="logixL5X">Объект L5X.</param>
        /// <returns></returns>
        public static TagMember Get(string tagName, LogicProgram program, LogixL5X logixL5X)
        {
            TagMember result = null;
            if (tagName == null || tagName.Trim() == "")
            {
                return null;
            }

            if (logixL5X == null)
            {
                return null;
            }

            Dictionary<string, DataType> dataTypes = logixL5X.DataTypes;
            Dictionary<string, Tag> controllerTags = logixL5X.Tags;
            Dictionary<string, Tag> programTags = null;

            if (program!=null)
            {
                programTags = program.Tags;
            }

            string[] parts = tagName.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string lastDataType = "";

            List<TagMember> members = new List<TagMember>();
            for (int ix = 0; ix < parts.Length; ix++)
            {
                // Анализируем на присутствие задания элемента массива в виде "[nnn]"
                string arrayIndex = null;
                string memberName = parts[ix];
                ArrayIndex arrayIndexObj = null;
                bool elementWasFound = false;

                #region [ ПРОВЕРКА И ОБРАБОТКА ИНДЕКСА МАССИВА ]
                /* ================================================================== */
                if (parts[ix].Contains("[") && !parts[ix].StartsWith("[") && parts[ix].EndsWith("]"))
                {
                    TextFragment[] texts = parts[ix].Separate("[", "]", TextSelectMethod.ToCenter);
                    memberName = texts.First(t => t.Type == TextType.Other).Value;
                    arrayIndex = texts.First(t => t.Type == TextType.Fragment).Value;
                }
                /* ================================================================== */
                #endregion

                // Первый элемент ищем в тэгах, последующие в типах данных.
                if (ix == 0)
                {
                    #region [ ЭЛЕМЕНТ: 0 ]
                    /* ================================================================== */
                    Tag tag = null;

                    // [Приоритет поиска #1]: Поиск первого элемента в программных тэгах.
                    // Если тэг найден, добавляем его в контейнер и запоминаем его DataType.
                    if (program != null && programTags != null)
                    {
                        if (programTags.TryGetValue(memberName, out tag))
                        {
                            lastDataType = tag.DataType;
                            elementWasFound = true;
                        }
                    }

                    // [Приоритет поиска #2]: Поиск первого элемента в контроллерных тэгах.
                    // Если тэг найден, добавляем его в контейнер и запоминаем его DataType.
                    if (tag == null)
                    {
                        if (controllerTags.TryGetValue(memberName, out tag))
                        {
                            lastDataType = tag.DataType;
                            elementWasFound = true;
                        }
                    }

                    // Если искомый тэг не найден возвращаем NULL.
                    if (tag == null || !elementWasFound)
                    {
                        return null;
                    }

                    // Если тип не ALIAS, то добавляем элемент в полседовательность.
                    if (tag.Type != TagType.ALIAS)
                    {
                        members.Add(tag);
                    }
                    // В случае если найденный тэг типа ALIAS, то переопределяем его базовый тэг.
                    else
                    {
                        if (tag.AliasFor == null)
                        {
                            return null;
                        }

                        TagMember baseTag = Tag.Get(tag.AliasFor.ToString(), program, logixL5X);
                        if (baseTag == null)
                        {
                            return null;
                        }

                        tag.AliasFor = baseTag;
                        lastDataType = baseTag.DataType;
                        //members.Add(baseTag); // Случай когда в конечном итоге хотим получить базовый тэг.
                        members.Add(tag);
                    }

                    // Создание промежуточного элемента массива в случае его существования.
                    if (arrayIndex != null)
                    {
                        Tag arrayTag = tag.Clone();
                        arrayTag.SourceMemorySpace = null;
                        arrayTag.ID = "[" + arrayIndex + "]";

                        // Проверяем перед созданием элемента массива что в текущем тэге есть определение размерности.
                        // Ели размерности массива не существует, то зампрос тэга неверен.
                        if (arrayTag.Dimension == null)
                        {
                             return null;
                        }

                        arrayIndexObj = new ArrayIndex(
                            arrayTag.Dimension.Dim2,
                            arrayTag.Dimension.Dim1,
                            arrayTag.Dimension.Dim0,
                            arrayIndex);

                        // Проверяем есть ли в индексах текст (string), если да, то пытаемся распознать их как тэг.
                        for (int dimix = 0; dimix < arrayIndexObj.Rank; dimix++)
                        {
                            object obj = arrayIndexObj.Indexes[dimix];
                            if (obj != null && obj is string)
                            {
                                TagMember t = Tag.Get((string)obj, program, logixL5X);
                                if (t != null) { arrayIndexObj.Indexes[dimix] = t; }
                            }
                        }

                        arrayTag.ArrayIndex = arrayIndexObj;
                        members.Add(arrayTag);
                    }
                    /* ================================================================== */
                    #endregion
                }
                else
                {
                    #region [ ЭЛЕМЕНТ: N ]
                    /* ================================================================== */
                    // Поиск последующих элементов структуры в типах данных.
                    DataType dataType;
                    TagMember tagMember = null;
                    if (dataTypes.TryGetValue(lastDataType, out dataType))
                    {
                        DataTypeMember dataTypeMember = dataType.GetChild<DataTypeMember>(memberName);

                        if (dataTypeMember != null)
                        {
                            lastDataType = dataTypeMember.DataType;
                            elementWasFound = true;

                            // Идентифицируем тип члена типа данных. Для DataTypeAddonParameter
                            // берем все виды параметров кроме "InOut".
                            if (dataTypeMember.GetType() == typeof(DataTypeMember))
                            {
                                tagMember = new TagMember(dataTypeMember.Clone());
                            }
                            else if (dataTypeMember.GetType() == typeof(DataTypeAddonParameter) &&
                            ((DataTypeAddonParameter)dataTypeMember).Usage != ParameterUsage.InOut &&
                                ((DataTypeAddonParameter)dataTypeMember).Usage != ParameterUsage.Local)
                            {
                                tagMember = new TagAddonParameter(((DataTypeAddonParameter)dataTypeMember).Clone());
                            }

                            // Если удалось преобразовать Тип данных в Тэг, добавляем тэг.
                            // В противном случае считаем опрерацию поиска неудачной.
                            if (tagMember != null)
                            {
                                members.Add(tagMember);
                            }
                            else
                            {
                                elementWasFound = false;
                            }
                        }
                    }
                    // Если искомый тэг не найден возвращаем NULL.
                    if (!elementWasFound)
                    {
                        return null;
                    }

                    // Создание элемента массива в случае его существования.
                    if (arrayIndex != null && tagMember != null)
                    {
                        TagMember arrayTag = tagMember.Clone();
                        if (!arrayTag.ChangeId("[" + arrayIndex + "]"))
                        {
                            return null;
                        }

                        // Проверяем перед созданием элемента массива что в текущем тэге есть определение размерности.
                        // Ели размерности массива не существует, то зампрос тэга неверен.
                        if (arrayTag.Dimension == null)
                        {
                            return null;
                        }

                        arrayIndexObj = new ArrayIndex(
                           arrayTag.Dimension.Dim2,
                           arrayTag.Dimension.Dim1,
                           arrayTag.Dimension.Dim0,
                           arrayIndex);

                        // Проверяем есть ли в индексах текст (string), если да, то пытаемся распознать их как тэг.
                        for (int dimix = 0; dimix < arrayIndexObj.Rank; dimix++)
                        {
                            object obj = arrayIndexObj.Indexes[dimix];
                            if (obj != null && obj is string)
                            {
                                TagMember t = Tag.Get((string)obj, program, logixL5X);
                                if (t != null) { arrayIndexObj.Indexes[dimix] = t; }
                            }
                        }

                        arrayTag.ArrayIndex = arrayIndexObj;
                        members.Add(arrayTag);
                    }
                    /* ================================================================== */
                    #endregion
                }
            }

            // Сопоставление цепочки элементов тэга (если искомый тэг структура).
            for (int ix = 0; ix < members.Count - 1 && members.Count > 1; ix++)
            {
                // Сопоставления свойства External Access.
                if (members[ix].ExternalAccess > members[ix + 1].ExternalAccess)
                {
                    members[ix + 1].ExternalAccess = members[ix].ExternalAccess;
                }

                // Сопоставления последовательной структуры тэгов.
                TagMember currentTagMember = members[ix].GetChild<TagMember>(members[ix + 1].ID);
                if (currentTagMember == null)
                {
                    members[ix].Add(members[ix + 1]);
                }
                else
                {
                    members[ix + 1] = currentTagMember;
                }
            }

            // Возвращаем последний элемент тэга.
            if (members.Count > 0)
            {
                result = members.Last();
            }

            return result;
        }
        /* ============================================================================== */
        #endregion

        #region [ PRIVATE AND PROTECTED METHODS ]
        /* ============================================================================== */
        /// <summary>
        /// Рекурсивная функция для построения полной структуры данного тэга.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="dataTypes"></param>
        private static void Bulid(TagMember t, Dictionary<string, DataType> dataTypes)
        {
            if (t == null || dataTypes == null)
            {
                return;
            }

            if (t.Dimension != null && t.Dimension.Length > 0)
            {
                foreach (string index in t.Dimension.GetIndexSequence())
                {
                    TagMember tag = new TagMember("[" + index + "]");
                    tag.DataType = t.DataType;
                    tag.ArrayIndex = new ArrayIndex(t.Dimension,index);
                    t.Add(tag);
                    Bulid(tag, dataTypes);
                }
            }
            else
            {
                DataType dataType;
                if (dataTypes.TryGetValue(t.DataType, out dataType))
                {
                    foreach (DataTypeMember dataTypeMember in dataType.Childrens.Values)
                    {
                        // Идентифицируем тип члена типа данных. Для DataTypeAddonParameter
                        // берем все виды параметров кроме "InOut".
                        if (dataTypeMember.GetType() == typeof(DataTypeMember))
                        {
                            TagMember tagMember = new TagMember((DataTypeMember)dataTypeMember);
                            t.Add(tagMember);
                            Bulid(tagMember, dataTypes);
                        }
                        else if (dataTypeMember.GetType() == typeof(DataTypeAddonParameter) &&
                            ((DataTypeAddonParameter)dataTypeMember).Usage != ParameterUsage.InOut &&
                            ((DataTypeAddonParameter)dataTypeMember).Usage != ParameterUsage.Local)
                        {
                            TagAddonParameter tagParameter = new TagAddonParameter((DataTypeAddonParameter)dataTypeMember);
                            t.Add(tagParameter);
                            Bulid(tagParameter, dataTypes);
                        }
                    }
                }
            }
        }
        /* ============================================================================== */
        #endregion
    }
}
