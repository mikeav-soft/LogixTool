using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LogixTool.Common;


namespace LogixTool.Logix.Models
{
    public class LogicRoutine : LogicNode
    {
        /// <summary>
        /// 
        /// </summary>
        private string MESSAGE_HEADER
        {
            get
            {
                return "Logic Routine : " + this.ID;
            }
        }

        /// <summary>
        /// Возвращает true в случае если данная рутина является основной.
        /// </summary>
        public bool IsMainRoutine
        {
            get
            {
                LogicProgram program = this.GetParrent<LogicProgram>();
                return (program != null && program.MainRoutine == this);
            }
        }
        /// <summary>
        /// Тип Рутины.
        /// </summary>
        public RoutineType Type { get; set; }

        /// <summary>
        /// Создает новый элемент.
        /// </summary>
        public LogicRoutine()
            : base("")
        {
            this.Type = RoutineType.NULL;
        }
        /// <summary>
        /// Создает новый элемент из XML элемента Routine.
        /// </summary>
        /// <param name="xroutine">XML элемент Routine.</param>
        public LogicRoutine(XElement xroutine)
            : this()
        {
            if (!xroutine.ExistAs("Routine"))
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "Source XML Element is not a \"Routine\" or is Null."));
                return;
            }

            // Преобразование Name.
            this.ID = xroutine.Attribute("Name").GetXValue("");
            // Преобразование Type и внутренней логики в зависимости от типа.
            switch (xroutine.Attribute("Type").GetXValue("").ToLower())
            {
                #region [ RLL ]
                /* ========================================================= */
                case "rll":
                    // Установка типа RLL.
                    this.Type = RoutineType.RLL;
                    // Преобразование контента RLL.
                    XElement rllContent = xroutine.Element("RLLContent");
                    if (rllContent != null)
                    {
                        foreach (XElement xrung in rllContent.Elements("Rung"))
                        {
                            LogicRung newLogicRung = new LogicRung(xrung);
                            this.Add(newLogicRung);
                        }
                    }
                    else
                    {
                        LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Routine\", Name='" + this.ID + "', not contains XML Element \"RLLContent\""));
                    }
                    break;
                /* ========================================================= */
                #endregion

                #region [ ST ]
                /* ========================================================= */
                case "st":
                    this.Type = RoutineType.ST;
                    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Routine\", Name='" + this.ID + "', not support at current version L5X core"));
                    break;
                /* ========================================================= */
                #endregion

                default:
                    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Routine\", Name='" + this.ID + "', not support at current version L5X core"));
                    break;
            }

            // Преобразование Description.
            this.Description = new LangText(xroutine.Element("Description"));
        }
    }
}
