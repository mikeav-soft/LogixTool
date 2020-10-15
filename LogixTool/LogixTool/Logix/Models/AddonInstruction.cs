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
    /// Модель описывающая Add-On инструкцию.
    /// </summary>
    public class AddonInstruction : LogixTree
    {
        /// <summary>
        /// 
        /// </summary>
        private string MESSAGE_HEADER
        {
            get
            {
                return "Addon Instruction : " + this.ID;
            }
        }

        /// <summary>
        /// Ревизия инструкции.
        /// </summary>
        public string Revision { get; set; }
        /// <summary>
        /// Производитель.
        /// </summary>
        public string Vendor { get; set; }
        /// <summary>
        /// Запускать при запуске специальную Routine.
        /// </summary>
        public string ExecutePrescan { get; set; }
        /// <summary>
        /// Запускать при завершении специальную Routine.
        /// </summary>
        public string ExecutePostscan { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ExecuteEnableInFalse { get; set; }
        /// <summary>
        /// Дата создания.
        /// </summary>
        public string CreatedDate { get; set; }
        /// <summary>
        /// Создатель.
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// Дата последнего редактирования.
        /// </summary>
        public string EditedDate { get; set; }
        /// <summary>
        /// Пользователь производивший последнюю редакцию.
        /// </summary>
        public string EditedBy { get; set; }
        /// <summary>
        /// Ревизия программного обеспечения.
        /// </summary>
        public string SoftwareRevision { get; set; }
        /// <summary>
        /// Текст описания.
        /// </summary>
        public LangText Description { get; set; }
        /// <summary>
        /// Замечания по текущей версии.
        /// </summary>
        public LangText RevisionNote { get; set; }
        /// <summary>
        /// Рутины данной инструкции.
        /// </summary>
        public Dictionary<string, LogicRoutine> Routines { get; set; }
        /// <summary>
        /// Локальный тэги.
        /// </summary>
        public Dictionary<string, Tag> LocalTags { get; set; }
        /// <summary>
        /// Параметры данной инструкции.
        /// </summary>
        public Dictionary<string, DataTypeAddonParameter> Parameters { get; set; }

        /// <summary>
        /// Создает новый объект описывающий Add-On Instruction.
        /// </summary>
        public AddonInstruction()
            : base("")
        {
            this.Revision = "";
            this.Vendor = "";
            this.ExecutePrescan = "";
            this.ExecutePostscan = "";
            this.ExecuteEnableInFalse = "";
            this.CreatedDate = "";
            this.CreatedBy = "";
            this.EditedDate = "";
            this.EditedBy = "";
            this.SoftwareRevision = "";
            this.Description = null;
            this.Routines = new Dictionary<string, LogicRoutine>();
            this.LocalTags = new Dictionary<string, Tag>();
            this.Parameters = new Dictionary<string, DataTypeAddonParameter>();
        }

        /// <summary>
        /// Создает новый объект описывающий Add-On Instruction из элемента XML.
        /// </summary>
        /// <param name="xAddon">XML элемент AddOnInstructionDefinition.</param>
        public AddonInstruction(XElement xAddon)
            : this()
        {
            if (!xAddon.ExistAs("AddOnInstructionDefinition"))
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "Source XML Element is not a \"AddOnInstructionDefinition\" or is Null."));
                return;
            }

            // Преобразование полей.
            this.ID = xAddon.Attribute("Name").GetXValue("");
            this.Revision = xAddon.Attribute("Revision").GetXValue("");
            this.Vendor = xAddon.Attribute("Vendor").GetXValue("");
            this.ExecutePrescan = xAddon.Attribute("ExecutePrescan").GetXValue("");
            this.ExecutePostscan = xAddon.Attribute("ExecutePostscan").GetXValue("");
            this.ExecuteEnableInFalse = xAddon.Attribute("ExecuteEnableInFalse").GetXValue("");
            this.CreatedDate = xAddon.Attribute("CreatedDate").GetXValue("");
            this.CreatedBy = xAddon.Attribute("CreatedBy").GetXValue("");
            this.EditedDate = xAddon.Attribute("EditedDate").GetXValue("");
            this.EditedBy = xAddon.Attribute("EditedBy").GetXValue("");
            this.SoftwareRevision = xAddon.Attribute("SoftwareRevision").GetXValue("");
            // Преобразование Description.
            this.Description = new LangText(xAddon.Element("Description"));
            // Преобразование RevisionNote.
            this.RevisionNote = new LangText(xAddon.Element("RevisionNote"));
            // Преобразование Routines.
            if (xAddon.Element("Routines") != null)
            {
                foreach (XElement xroutine in xAddon.Element("Routines").Elements("Routine"))
                {
                    LogicRoutine routine = new LogicRoutine(xroutine);
                    routine.Parrent = this;
                    this.Routines.Set(routine.ID, routine);
                }
            }
            else
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"AddOnInstructionDefinition\" not contains \"Routines\""));
            }

            // Преобразование Tags.
            if (xAddon.Element("LocalTags") != null)
            {
                foreach (XElement xlocaltag in xAddon.Element("LocalTags").Elements("LocalTag"))
                {
                    Tag localtag = new Tag(xlocaltag);
                    localtag.Parrent = this;
                    this.LocalTags.Set(localtag.ID, localtag);
                }
            }
            else
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"AddOnInstructionDefinition\" not contains \"LocalTags\""));
            }

            // Преобразование Parameters.
            if (xAddon.Element("Parameters") != null)
            {
                foreach (XElement xparameter in xAddon.Element("Parameters").Elements("Parameter"))
                {
                    DataTypeAddonParameter parameter = new DataTypeAddonParameter(xparameter);
                    parameter.Parrent = this;
                    this.Parameters.Set(parameter.ID, parameter);
                }
            }
            else
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"AddOnInstructionDefinition\" not contains \"Parameters\""));
            }
        }
    }
}
