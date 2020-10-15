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
    public class LogicTask : LogicNode
    {
        /// <summary>
        /// 
        /// </summary>
        private string MESSAGE_HEADER
        {
            get
            {
                return "Logic Task : " + this.ID;
            }
        }
        /// <summary>
        /// Тип задачи.
        /// </summary>
        public TaskType Type { get; set; }
        /// <summary>
        /// Период выполнения временной задачи.
        /// </summary>
        public int Rate { get; set; }
        /// <summary>
        /// Приоритет выполнения временной задачи.
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// Период срабатывания сторожевого таймера.
        /// </summary>
        public int Watchdog { get; set; }
        /// <summary>
        /// Запрет обновления выходов.
        /// </summary>
        public bool DisableUpdateOutputs { get; set; }
        /// <summary>
        /// Отключение задачи.
        /// </summary>
        public bool InhibitTask { get; set; }
        /// <summary>
        /// Класс задачи.
        /// </summary>
        public TaskClass Class { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public LogicTask(string name)
            : base(name)
        {
            this.Type = TaskType.UNKNOWN;
            this.Rate = 0;
            this.Priority = 0;
            this.Watchdog = 0;
            this.DisableUpdateOutputs = false;
            this.InhibitTask = false;
            this.Class = TaskClass.UNKNOWN;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xtask"></param>
        public LogicTask(XElement xtask)
            : this("")
        {
            if (!xtask.ExistAs("Task"))
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "Source XML Element is not a \"Task\" or is Null."));
                return;
            }

            string text;

            // Преобразование Name.
            this.ID = xtask.Attribute("Name").GetXValue("");
            // Преобразование Type.
            string typeValue = xtask.Attribute("Type").GetXValue("").ToLower();
            switch (typeValue)
            {
                case "periodic":
                    this.Type = TaskType.PERIODIC;
                    break;

                case "continuous":
                    this.Type = TaskType.CONTINUOUS;
                    break;

                default:
                    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Task\", Name='" + this.ID + "', contains XML Element \"Type\" with undefined value = '" + typeValue + "'"));
                    break;
            }
            // Преобразование Rate.
            text = xtask.Attribute("Rate").GetXValue("");
            if (text.IsDigits())
            {
                this.Rate = Convert.ToInt32(text);
            }
            else
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Task\", Name='" + this.ID + "', contains XML Attribute \"Rate\" with undigitable value"));
            }
            // Преобразование Priority.
            text = xtask.Attribute("Priority").GetXValue("");
            if (text.IsDigits())
            {
                this.Priority = Convert.ToInt32(text);
            }
            else
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Task\", Name='" + this.ID + "', contains XML Attribute \"Priority\" with undigitable value"));
            }
            // Преобразование Watchdog.
            text = xtask.Attribute("Watchdog").GetXValue("");
            if (text.IsDigits())
            {
                this.Watchdog = Convert.ToInt32(text);
            }
            else
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Task\", Name='" + this.ID + "', contains XML Attribute \"Watchdog\" with undigitable value"));
            }
            // Преобразование DisableUpdateOutputs.
            this.DisableUpdateOutputs = xtask.Attribute("DisableUpdateOutputs").GetXValue("false").ToLower() == "true";
            // Преобразование InhibitTask.
            this.InhibitTask = xtask.Attribute("InhibitTask").GetXValue("false").ToLower() == "true";
            // Преобразование Class.
            string classValue = xtask.Attribute("Class").GetXValue("").ToLower();
            switch (classValue)
            {
                case "standard":
                    this.Class = TaskClass.STANDARD;
                    break;

                case "safety":
                    this.Class = TaskClass.SAFETY;
                    break;

                default:
                    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Task\", Name='" + this.ID + "', contains XML Element \"Class\" with undefined value = '" + classValue + "'"));
                    break;
            }

            // Преобразование Description.
            this.Description = new LangText(xtask.Element("Description"));
        }
    }
}
