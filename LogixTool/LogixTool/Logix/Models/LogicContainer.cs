using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Logix.Models
{
    /// <summary>
    /// Intermediate structure for building connection between RLL Instrucions.
    /// </summary>
    public class LogicContainer
    {
        /// <summary>
        /// Родительский эдемент.
        /// </summary>
        public LogicContainer Parrent { get; set; }
        /// <summary>
        /// Контейнер с объектами Instructions или LogicContainer.
        /// </summary>
        public List<List<object>> Items { get; set; }

        /// <summary>
        /// Init of this object
        /// </summary>
        public LogicContainer()
        {
            this.Parrent = null;
            this.Items = new List<List<object>>();
        }
        /// <summary>
        /// Add new item to last level
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(object item)
        {
            // First validation of input parameters
            if (item == null)
            {
                return;
            }

            // Checking of Child elements
            if (this.Items == null)
            {
                this.Items = new List<List<object>>();
            }

            // If Childs count is = 0, create least one element
            if (this.Items.Count == 0)
            {
                this.Items.Add(new List<object>());
            }

            // Add "child" element -> to last level, as new element
            this.Items.Last().Add(item);
            if (item is LogicContainer)
            {
                ((LogicContainer)item).Parrent = this;
            }
        }
        /// <summary>
        /// Add new Line of Items
        /// </summary>
        /// <param name="child"></param>
        public void AddLine()
        {
            // Checking of Child elements
            if (this.Items == null)
            {
                this.Items = new List<List<object>>();
            }

            // Create new Level
            this.Items.Add(new List<object>());
        }
        /// <summary>
        /// Строит взаимосвязи межде добавляемыми элементами.
        /// </summary>
        /// <param name="root">Корневая левая (первая) инструкция.</param>
        /// <param name="end">Корневая правая (последняя) инструкция.</param>
        public void BuildNetworks(LogicInstruction root, LogicInstruction end)
        {
            // First checking of input parameters
            if (root == null || end == null)
            {
                return;
            }

            // Variables
            List<LogicInstruction> inRLLInstructions;     // List of input instructions
            List<LogicInstruction> outRLLInstructions;    // List of right instructions

            // Add root instruction to List
            inRLLInstructions = new List<LogicInstruction>();
            inRLLInstructions.Add(root);

            // Строит взаимосвязи.
            LogicContainer.BuildNetworks(inRLLInstructions, this, out outRLLInstructions);

            // Connect all out instructions to [end]
            foreach (LogicInstruction outInstruct in outRLLInstructions)
            {
                // Directions -> Add [outInstruct] to [end] as EnableIn
                end.EnableIn.Add(outInstruct);

                // Directions <- Add [end] to [outInstruct] as EnableOut
                outInstruct.EnableOut.Add(end);
            }
        }

        /// <summary>
        /// Рекурсивная функция: Строит взаимосвязи между добавленными элементами.
        /// </summary>
        /// <param name="inInstructions">Входные инструкции (слева) для создания взаимосвязей.</param>
        /// <param name="currentLogicContainer">Текущий контейнер с логическими инструкциями или логическим контейнером.</param>
        /// <param name="outInstructions">Выходные инструкции (справа) для создания взаимосвязей.</param>
        private static void BuildNetworks(List<LogicInstruction> inInstructions, LogicContainer currentLogicContainer, out List<LogicInstruction> outInstructions)
        {
            outInstructions = new List<LogicInstruction>();                       // Output: Init of Output Instructions (common for each line)

            // First checking of input parameters
            if (inInstructions == null || currentLogicContainer == null)
            {
                return;
            }

            // Cycle for each Line in [currentLogicContainer] Items
            foreach (List<object> currLogicLine in currentLogicContainer.Items)
            {
                int logicContainerMaxIndex = currLogicLine.Count - 1;           // Max Index of Current Line
                List<LogicInstruction> prevInstrictions = inInstructions;         // Previews Instruction (one step ago)

                // Cycle for Items in Line
                for (int index = 0; index <= logicContainerMaxIndex; index++)
                {
                    // Case 1. Current Item is "RLLInstruction"
                    if (currLogicLine[index] is LogicInstruction)
                    {
                        LogicInstruction currRLLInstruction = (LogicInstruction)currLogicLine[index];
                        foreach (LogicInstruction prevInstr in prevInstrictions)
                        {
                            // Directions -> Add [prevInstr] to [currRLLInstruction] as EnableIn
                            currRLLInstruction.EnableIn.Add(prevInstr);

                            // Directions <- Add [currRLLInstruction] to [prevInstr] as EnableOut
                            prevInstr.EnableOut.Add(currRLLInstruction);
                        }

                        // Reinit [prevInstrictions] and add[currRLLInstruction]
                        prevInstrictions = new List<LogicInstruction>();
                        prevInstrictions.Add(currRLLInstruction);
                    }


                    // Case 2. Current Item is "LogicContainer"
                    if (currLogicLine[index] is LogicContainer)
                    {
                        LogicContainer currLogicContainer = (LogicContainer)currLogicLine[index];
                        BuildNetworks(prevInstrictions, currLogicContainer, out prevInstrictions);
                    }


                    // For Last Index Add [prevInstrictions] for current line to [outInstructions]
                    if (index == logicContainerMaxIndex)
                    {
                        outInstructions.AddRange(prevInstrictions);
                    }
                }
            }
        }

        /// <summary>
        /// Получает из Rung все пярмые участки с инструкциями без ответвлений.
        /// </summary>
        /// <param name="container">Текущий логический контейнер LAD.</param>
        /// <param name="result">Результат в виде строки.</param>
        private static void ConvertToString(LogicContainer container, ref string result)
        {
            result += "[";

            for (int lineIx = 0; lineIx < container.Items.Count; lineIx++)
            {
                List<object> objList = container.Items[lineIx];             // Текущая логическая линия.
                bool lineIxIsLast = (container.Items.Count - 1 == lineIx);  // Текущая логическая линия является последней.

                for (int objIx = 0; objIx < objList.Count; objIx++)
                {
                    object obj = objList[objIx];        // Текущий объект на логической линии.
                    bool objIxIsFirst = objIx == 0;     // Текущий объект логической линии является первым.

                    if (obj.GetType() == typeof(LogicInstruction))
                    {
                        result = result + " " + ((LogicInstruction)obj).ToString();
                    }

                    if (obj.GetType() == typeof(LogicContainer))
                    {
                        ConvertToString((LogicContainer)obj, ref result);
                    }
                }

                if (!lineIxIsLast)
                {
                    result = result + ", ";
                }
            }
            result += "]";
        }
        /// <summary>
        /// Преобразовывает текущий объект в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "";
            ConvertToString(this, ref result);
            return result + ";";
        }
    }
}
