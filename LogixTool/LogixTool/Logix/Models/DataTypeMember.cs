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
    public class DataTypeMember : LogixTree
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        /// <summary>
        /// 
        /// </summary>
        private string MESSAGE_HEADER
        {
            get
            {
                return "Data Type Member";
            }
        }

        /// <summary>
        /// Тип данных текущего элемента.
        /// </summary>
        public string DataType { get; set; }
        /// <summary>
        /// Описание элемента.
        /// </summary>
        public LangText Description { get; set; }
        /// <summary>
        /// Вид отображения данных.
        /// </summary>
        public TagRadix Radix { get; set; }
        /// <summary>
        /// Тип доступа.
        /// </summary>
        public TagExternalAccess ExternalAccess { get; set; }
        /// <summary>
        /// Размерность массива в случае если элемент определяет массив.
        /// </summary>
        public ArrayDefinition Dimension { get; set; }
        /// <summary>
        /// Получает смещение в байтах для данного члена в пространстве определяющего его типа данных.
        /// </summary>
        public uint? ByteOffset
        {
            get
            {
                MemoryCoord coord = this.GetOffsetCoord();

                if (coord == null)
                {
                    return null;
                }

                coord.Dim = 8;

                return coord.Row;
            }
        }
        /// <summary>
        /// Получает смещение в битах текущего байта для данного члена в пространстве определяющего его типа данных.
        /// </summary>
        public byte? BitOffset
        {
            get
            {
                MemoryCoord coord = this.GetOffsetCoord();

                if (coord == null)
                {
                    return null;
                }

                coord.Dim = 8;

                return (byte)coord.Pos;
            }
        }
        /* ================================================================================================== */
        #endregion


        /// <summary>
        /// Инициализирует новый структурный элемент типа данных DataTypeMember.
        /// </summary>
        public DataTypeMember(string name)
            : base(name)
        {
            this.DataType = "";
            this.Description = null;
            this.Radix = TagRadix.NULL;
            this.ExternalAccess = TagExternalAccess.NULL;
            this.Dimension = null;
        }
        /// <summary>
        /// Создает новый структурный элемент типа данных на основе XML элемента Member.
        /// </summary>
        /// <param name="xmember"></param>
        public DataTypeMember(XElement xmember)
            : this("")
        {
            if (!xmember.ExistAs("Member"))
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "Source XML Element is not a \"Member\" or is Null."));
                return;
            }

            // Преобразование Name.
            this.ID = xmember.Attribute("Name").GetXValue("");
            // Преобразование Description.
            this.Description = new LangText(xmember.Element("Description"));
            // Преобразование DataType.
            this.DataType = xmember.Attribute("DataType").GetXValue("");
            if (this.DataType.ToUpper() == "BIT")
            {
                this.DataType = "BOOL";
            }
            // Преобразование Radix.
            string radixValue = xmember.Attribute("Radix").GetXValue("").ToLower();
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
                    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Member\", Name='" + this.ID + "', contains XML Element \"Radix\" with undefined value = '" + radixValue + "'"));
                    break;
            }

            // Преобразование ExternalAccess.
            string externalAccessValue = xmember.Attribute("ExternalAccess").GetXValue("").ToLower();
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
                    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Member\", Name='" + this.ID + "', contains XML Element \"ExternalAccess\" with undefined value = '" + externalAccessValue + "'"));
                    break;
            }

            // Преобразование Dimension.
            if (xmember.Attribute("Dimension") != null)
            {
                ArrayDefinition arrayDefinition = new ArrayDefinition(xmember.Attribute("Dimension").GetXValue(""));
                if (arrayDefinition.Length > 0)
                {
                    this.Dimension = arrayDefinition;
                }
                //else
                //{
                //    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Member\", Name='" + this.Name + "', contains XML Element \"Dimension\" with value = 0"));
                //}
            }
        }

        /// <summary>
        /// Получает объект с определением типа данных для текущего члена из определенного в головном объекте LogixL5X. 
        /// </summary>
        /// <returns></returns>
        public DataType GetDataTypeDefinition ()
        {
            // Проверяем исходные данные для поиска.
            if (this.DataType == null || this.DataType.Trim() == "")
            {
                return null;
            }

            // Ищем головной родительский объект в структуре дерева и далее ищем в нем определение
            // типа данных по заданному имени this.DataType.
            List<LogixL5X> foundedLogixL5X = this.FindParrents(t => t is LogixL5X).OfType<LogixL5X>().ToList();
            if (foundedLogixL5X.Count == 1 && foundedLogixL5X[0] != null)
            {
                DataType dataType;
                if (foundedLogixL5X[0].DataTypes.TryGetValue(this.DataType, out dataType))
                {
                    return dataType;
                }
            }
            return null;
        }

        /// <summary>
        /// Возвращает смещение в байтах расположения данной структуры.
        /// </summary>
        private MemoryCoord GetOffsetCoord()
        {
            // Проверяем, есть ли у данного элемента родитель, и есть ли у родителя разметка в области памяти.
            if (this.Parrent != null && this.Parrent is DataType && ((DataType)this.Parrent).MembersCell != null)
            {
                // Получаем разметку области памяти родителя.
                MemoryCell memoryCell = ((DataType)this.Parrent).MembersCell;

                // Получаем по имени (ID) стартовую позицию текущего элемента.
                if (memoryCell.Memory.ContainsKey(this.ID))
                {
                    return memoryCell.Memory[this.ID].StartCoord;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Копирует объект.
        /// </summary>
        /// <returns></returns>
        public new DataTypeMember Clone ()
        {
            DataTypeMember member = (DataTypeMember)this.MemberwiseClone();
            member.Childrens = new Dictionary<string, Tree>();
            member.Parrent = null;
            member.CrossRefference = new List<CrossReferenceItem>();
            return member;
        }
    }
}
