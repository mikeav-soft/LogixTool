using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LogixTool.Common;

namespace LogixTool.Logix.Models
{
    public class DataTypeAddonParameter : DataTypeMember
    {
        /// <summary>
        /// 
        /// </summary>
        private string MESSAGE_HEADER
        {
            get
            {
                return "Data Type Addon Parameter";
            }
        }

        /// <summary>
        /// Требуемый параметр.
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// Отображение параметра.
        /// </summary>
        public bool Visible { get; set; }
        /// <summary>
        /// Использование параметра.
        /// </summary>
        public ParameterUsage Usage { get; set; }

        /// <summary>
        /// Инициализирует новый структурный элемент типа данных (Add-On parameter).
        /// </summary>
        public DataTypeAddonParameter(string id)
            : base(id)
        {
            this.Required = false;
            this.Visible = false;
            this.Usage = ParameterUsage.Null;
        }

        /// <summary>
        /// Создает новый структурный элемент типа данных (Add-On parameter) на основе XML элемента Parameter.
        /// </summary>
        /// <param name="xmember"></param>
        public DataTypeAddonParameter(XElement xparameter)
            : this("")
        {
            if (xparameter.ExistAs("Parameter"))
            {
                #region [ PARAMETER ]
                /* ========================================================================== */
                // Преобразование Name.
                this.ID = xparameter.Attribute("Name").GetXValue("");
                // Преобразование Description.
                this.Description = new LangText(xparameter.Element("Description"));
                // Преобразование DataType.
                this.DataType = xparameter.Attribute("DataType").GetXValue("");
                // Преобразование Dimension.
                if (xparameter.Attribute("Dimensions") != null)
                {
                    ArrayDefinition dimensionDefinition = new ArrayDefinition(xparameter.Attribute("Dimensions").GetXValue(""));
                    if (dimensionDefinition.Length > 0)
                    {
                        this.Dimension = dimensionDefinition;
                    }
                    //else
                    //{
                    //    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Parameter\", Name='" + this.Name + "', contains XML Element \"Dimension\" with value = 0"));
                    //}
                }

                // Преобразование Radix.
                string radixValue = xparameter.Attribute("Radix").GetXValue("").ToLower();
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
                        LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Parameter\", Name='" + this.ID + "', contains XML Element \"Radix\" with undefined value = '" + radixValue + "'"));
                        break;
                }

                // Преобразование ExternalAccess.
                string externalAccessValue = xparameter.Attribute("ExternalAccess").GetXValue("").ToLower();
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
                        LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Parameter\", Name='" + this.ID + "', contains XML Element \"ExternalAccess\" with undefined value ='" + externalAccessValue + "'"));
                        break;
                }

                // Преобразование Required.
                this.Required = xparameter.Attribute("Required").GetXValue("false").ToLower() == "true";
                // Преобразование Visible.
                this.Visible = xparameter.Attribute("Visible").GetXValue("false").ToLower() == "true";
                // Преобразование Usage.
                string usageValue = xparameter.Attribute("Usage").GetXValue("").ToLower();
                switch (usageValue)
                {
                    case "input":
                        this.Usage = ParameterUsage.In;
                        break;

                    case "output":
                        this.Usage = ParameterUsage.Out;
                        break;

                    case "inout":
                        this.Usage = ParameterUsage.InOut;
                        break;

                    default:
                        LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Parameter\", Name='" + this.ID + "', contains XML Element \"Usage\" with undefined value ='" + usageValue + "'"));
                        break;
                }

                /* ========================================================================== */
                #endregion
            }
            else if (xparameter.ExistAs("LocalTag"))
            {
                #region [ LOCAL TAG ]
                /* ========================================================================== */
                // Преобразование Name.
                this.ID = xparameter.Attribute("Name").GetXValue("");
                // Преобразование Description.
                this.Description = new LangText(xparameter.Element("Description"));
                // Преобразование DataType.
                this.DataType = xparameter.Attribute("DataType").GetXValue("");
                // Преобразование Dimension.
                if (xparameter.Attribute("Dimensions") != null)
                {
                    ArrayDefinition dimensionDefinition = new ArrayDefinition(xparameter.Attribute("Dimensions").GetXValue(""));
                    if (dimensionDefinition.Length > 0)
                    {
                        this.Dimension = dimensionDefinition;
                    }
                    //else
                    //{
                    //    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"LocalTag\", Name='" + this.Name + "', contains XML Element \"Dimension\" with value = 0"));
                    //}
                }
                // Преобразование Radix.
                string radixValue = xparameter.Attribute("Radix").GetXValue("").ToLower();
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
                        LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"LocalTag\", Name='" + this.ID + "', contains XML Element \"Radix\" with undefined value ='" + radixValue + "'"));
                        break;
                }

                // Преобразование ExternalAccess.
                this.ExternalAccess = TagExternalAccess.NONE;
                // Преобразование Required.
                this.Required = false;
                // Преобразование Visible.
                this.Visible = false;
                // Преобразование Usage.
                this.Usage = ParameterUsage.Local;

                /* ========================================================================== */
                #endregion
            }
            else
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "Source XML Element is not a \"Parameter\" or \"LocalTag\" or is Null."));
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public new DataTypeAddonParameter Clone()
        {
            DataTypeAddonParameter member = (DataTypeAddonParameter)this.MemberwiseClone();
            member.Childrens = new Dictionary<string, Tree>();
            member.Parrent = null;
            member.CrossRefference = new List<CrossReferenceItem>();
            return member;
        }
    }
}
