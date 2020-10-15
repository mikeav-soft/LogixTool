using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LogixTool.Common;
using LogixTool.Logix.Models;

namespace LogixTool.Logix
{
    public class LogicRung: LogicNode
    {
        /// <summary>
        /// 
        /// </summary>
        private string MESSAGE_HEADER
        {
            get
            {
                return "Logic Rung : " + this.ID;
            }
        }

        /// <summary>
        /// Тип RLL Ранга.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Входная (коренная) инструкция.
        /// </summary>
        public LogicInstruction RootInstruction { get; set; }
        /// <summary>
        /// Конечная (выходная) инструкция.
        /// </summary>
        public LogicInstruction OutInstruction { get; set; }
        /// <summary>
        /// Контейнер с инструкциями представляющий собой способ линейного хранения.
        /// </summary>
        public LogicContainer LogicContainer { get; set; }

        /// <summary>
        /// Создает новый пустой элемент Rung.
        /// </summary>
        public LogicRung()
            : base("")
        {
            this.Type = "";
            this.RootInstruction = new LogicInstruction("<root>","", null);
            this.OutInstruction = new LogicInstruction("<end>","", null);
            this.LogicContainer = new LogicContainer();
        }
        /// <summary>
        /// Создает новый элемент Rung из XML элемента Rung.
        /// </summary>
        /// <param name="xRung">XML элемент Rung.</param>
        public LogicRung(XElement xRung)
            : this()
        {
            if (!xRung.ExistAs("Rung"))
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "Source XML Element is not a \"Rung\" or is Null."));
                return;
            }

            this.ID = xRung.Attribute("Number").GetXValue("");
            this.Type = xRung.Attribute("Type").GetXValue("");

            XElement xRungText = xRung.Element("Text");
            if (!xRungText.ExistAs("Text"))
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Rung\" not contains XML Element \"Text\""));
                return;
            }

            // Преобразование Description.
            this.Description = new LangText(xRung.Element("Comment"));

            bool opertaionIsOk = false;
            string rllContent = "";
            const char MASK_PARAMETER = 'p';
            const char MASK_NAME = 'n';
            const char MASK_OTHER = ' ';

            rllContent = xRungText.GetXValue("");

            opertaionIsOk = (rllContent != "");             // RLL Text Content mus be not empty for parsing
            opertaionIsOk = rllContent.EndsWith(";");       // RLL Text content must be ended on ';'

            if (opertaionIsOk)
            {
                char[] rllMask = new char[rllContent.Length];   // RLL Content mask of format -> nnnn(pppppp)
                int bracketsCounter = 0;    // Counter of brakets '(' and ')', <+> or <-> counter accordingly
                bool isInstruction = false; // Cuttent index of string is RLL Instruction

                // Init of RLL Mask
                rllMask = rllMask.Select(c => c = MASK_OTHER).ToArray();

                #region STEP 1: Looking for parameters in instruction brackets
                // ================================================================================
                // STEP 1: Looking for parameters in instruction brackets (char index direction -->)
                for (int ix = 0; ix < rllContent.Length; ix++)
                {
                    if (!isInstruction)
                    {
                        // If current symbol is bracket and previews is latter or digit or '_' -> 'n('
                        if (ix > 0 && (Strings.IsLaterOrDigit(rllContent[ix - 1]) || rllContent[ix - 1] == '_')
                            && rllContent[ix] == '(')
                        {
                            isInstruction = true;
                            rllMask[ix] = '(';
                        }
                    }
                    else
                    {
                        // If is parameters filling of mask by 'p'
                        rllMask[ix] = MASK_PARAMETER;
                    }

                    // Increase brecket counter if current char is '('
                    if (rllContent[ix] == '(' && isInstruction)
                    {
                        bracketsCounter++;
                    }
                    // Decrease brecket counter if current char is ')'
                    if (rllContent[ix] == ')' && isInstruction)
                    {
                        bracketsCounter--;
                    }
                    // If Braket Counter zero, then instruction zone of parameters is end 
                    if (bracketsCounter == 0 && isInstruction)
                    {
                        isInstruction = false;
                        rllMask[ix] = ')';
                    }
                }
                // ================================================================================
                #endregion

                #region STEP 2: Looking for names of instruction in front of bracket
                // ================================================================================
                // STEP 2: Looking for names of instruction in front of bracket (char index direction <--)
                isInstruction = false;
                for (int ix = rllContent.Length - 1; ix >= 0; ix--)
                {
                    // if current symbol not later or digit or '_', then name of instruction was ended
                    if (isInstruction && !(Strings.IsLaterOrDigit(rllContent[ix]) || rllContent[ix] == '_'))
                    {
                        isInstruction = false;
                    }
                    // If is instruction filling mask by 'n'
                    if (isInstruction)
                    {
                        rllMask[ix] = MASK_NAME;
                    }
                    // Start of instruction name since symbol '(' of mask
                    if (!isInstruction && rllContent[ix] == '(')
                    {
                        isInstruction = true;
                    }
                }
                // ================================================================================
                #endregion

                #region STEP 3: Синтез результата, получение инструкций в соответствии с маской.
                // ================================================================================
                // STEP 3: Synthes of results, getting instructions according with mask (char index direction -->)
                string nameOfInstruction = "";          // current name of instruction
                string parametersOfInstruction = "";    // current parameters of instruction

                LogicContainer currentLogicContainer = LogicContainer;

                for (int ix = 0; ix < rllContent.Length; ix++)
                {
                    switch (rllMask[ix])
                    {
                        // Combinig of name instruction
                        case MASK_NAME:
                            {
                                nameOfInstruction += rllContent[ix];
                            }
                            break;

                        // Combinig of parameters instruction
                        case MASK_PARAMETER:
                            {
                                parametersOfInstruction += rllContent[ix];
                            }
                            break;

                        // End of instruction, writing results
                        case ')':
                            {
                                // Создаем объект RLL инструкции.
                                LogicInstruction newLogixRLLInstruction = new LogicInstruction(
                                    this.Childrens.Count.ToString(),
                                    nameOfInstruction, 
                                    parametersOfInstruction.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

                                // Добавляет текущую инструкцию в Logic Container.
                                currentLogicContainer.AddItem(newLogixRLLInstruction);

                                // Добавляем инструкцию в древовидную структуру.
                                this.Add(newLogixRLLInstruction);

                                // Инициализируем переменные.
                                nameOfInstruction = "";
                                parametersOfInstruction = "";

                            }
                            break;

                        // Other char, do nothing
                        case MASK_OTHER:
                            // ==========================================================
                            if (rllContent[ix] == '[')
                            {
                                // Create new Level and use as current
                                LogicContainer newRLLLogicTree = new LogicContainer();
                                currentLogicContainer.AddItem(newRLLLogicTree);
                                currentLogicContainer = newRLLLogicTree;
                            }
                            // ==========================================================
                            else if (rllContent[ix] == ']')
                            {
                                // Go back to the Parrent Logic tree
                                currentLogicContainer = currentLogicContainer.Parrent;
                            }
                            // ==========================================================
                            else if (rllContent[ix] == ',')
                            {
                                // Create new Level
                                currentLogicContainer.AddLine();
                            }
                            // ==========================================================
                            break;
                    }
                }
                // ================================================================================
                #endregion

                #region STEP 4: Построение взаимосвязей между добавленными инструкции внутри LogicContainer
                // ================================================================================
                currentLogicContainer.BuildNetworks(this.RootInstruction, this.OutInstruction);
                // ================================================================================
                #endregion
            }
            else
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"Text\" of RLL Rung contains uncorrect data."));
            }
        }
    }
}
