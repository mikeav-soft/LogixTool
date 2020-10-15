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
    public class LogicProgram : LogicNode
    {
        /// <summary>
        /// 
        /// </summary>
        private string MESSAGE_HEADER
        {
            get
            {
                return "Logic Program : " + this.ID;
            }
        }

        /// <summary>
        /// Локальные тэги программы.
        /// </summary>
        public Dictionary<string, Tag> Tags { get; set; }
        /// <summary>
        /// Имеется тестовое редактирование логики.
        /// </summary>
        public bool TestEdits { get; set; }
        /// <summary>
        /// Основная вызываемая рутина.
        /// </summary>
        public LogicRoutine MainRoutine { get; set; }
        /// <summary>
        /// Программа отключена для исполнения.
        /// </summary>
        public bool Disabled { get; set; }
        /// <summary>
        /// Класс программы.
        /// </summary>
        public LogicClass Class { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public LogicProgram()
            : base("")
        {
            this.TestEdits = false;
            this.MainRoutine = null;
            this.Disabled = false;
            this.Class = LogicClass.STANDARD;
            this.Tags = new Dictionary<string, Tag>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xprogram"></param>
        public LogicProgram(XElement xprogram)
            : this()
        {
            if (!xprogram.ExistAs("Program"))
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "Source XML Element is not a \"Program\" or is Null."));
                return;
            }

            // Преобразование Name.
            this.ID = xprogram.Attribute("Name").GetXValue("");
            // Преобразование TestEdits.
            this.TestEdits = xprogram.Attribute("TestEdits").GetXValue("false").ToLower() == "true";
            // Преобразование Disabled.
            this.Disabled = xprogram.Attribute("Disabled").GetXValue("false").ToLower() == "true";
            // Преобразование TagClass.
            string classValue = xprogram.Attribute("Class").GetXValue("").ToLower();
            switch (classValue)
            {
                case "standard":
                    this.Class = LogicClass.STANDARD;
                    break;

                case "safety":
                    this.Class = LogicClass.SAFETY;
                    break;

                default:
                    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Program\", Name='" + this.ID + "', contains XML Element \"Class\" with undefined value = '" + classValue + "'"));
                    break;
            }

            // Преобразование Description.
            this.Description = new LangText(xprogram.Element("Description"));

            // Преобразование MainRoutineName.
            string mainRoutineName = xprogram.Attribute("MainRoutineName").GetXValue("");
            // Создание дочерних элементов Routine.
            XElement xroutines = xprogram.Element("Routines");
            if (xroutines != null)
            {
                foreach (XElement xroutine in xroutines.Elements("Routine"))
                {
                    LogicRoutine logicRoutine = new LogicRoutine(xroutine);
                    this.Add(logicRoutine);
                    if (logicRoutine.ID == mainRoutineName)
                    {
                        this.MainRoutine = logicRoutine;
                    }
                }
            }
            else
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Program\", Name='" + this.ID + "', not contains XML Element \"Routines\""));
            }

            // Создание локальных тэгов.
            if (xprogram.Element("Tags") != null)
            {
                foreach (XElement xtag in xprogram.Element("Tags").Elements("Tag"))
                {
                    Tag dt = new Tag(xtag);
                    dt.Program = this;
                    dt.Class = this.Class;

                    Tags.Set(dt.ID, dt);
                    dt.Parrent = this;
                }
            }
            else
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Program\", Name='" + this.ID + "', not contains XML Element \"Tags\""));
            }
        }

        /// <summary>
        /// Получает объект тэга по заданному имени.
        /// </summary>
        /// <param name="tagName">Название тэга.</param>
        /// <returns></returns>
        public TagMember GetTag(string tagName)
        {
            LogixL5X logixL5X = this.GetRoot<LogixL5X>();
            if (logixL5X == null || tagName == null || tagName.Trim() == "")
            {
                return null;
            }

            return Tag.Get(tagName, this, logixL5X);
        }
    }
}
