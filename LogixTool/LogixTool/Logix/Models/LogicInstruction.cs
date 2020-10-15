using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LogixTool.Logix.Models;

namespace LogixTool.Logix
{
    /// <summary>
    /// RLL Instruction Model
    /// </summary>
    public class LogicInstruction : LogicNode
    {
        /// <summary>
        /// Название параметра.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Показывает что данная инструкция является Add-On.
        /// </summary>
        public bool IsAddon
        {
            get
            {
                bool result = false;
                LogixL5X root = this.GetRoot<LogixL5X>();

                if (root != null && root.AddonInstructions != null)
                {
                    return root.AddonInstructions.ContainsKey(this.ID);
                }

                return result;
            }
        }
        /// <summary>
        /// Параметры инструкции.
        /// </summary>
        public List<LogicInstructionParameter> Parameters { get; set; }
        /// <summary>
        /// Логический вход от предыдущих инструкций.
        /// </summary>
        public List<LogicInstruction> EnableIn { get; set; }
        /// <summary>
        /// Логический выход в следующую инструкцию.
        /// </summary>
        public List<LogicInstruction> EnableOut { get; set; }

        /// <summary>
        /// Создает новую логическую инструкцию.
        /// </summary>
        /// <param name="name">Имя инструкции.</param>
        /// <param name="param">Параметры интрукций в виде строковых значений.</param>
        public LogicInstruction(string id, string name, string[] param)
            : base(id)
        {
            this.Name = name;
            this.EnableIn = new List<LogicInstruction>();
            this.EnableOut = new List<LogicInstruction>();

            this.Parameters = new List<LogicInstructionParameter>();
            if (param != null)
            {
                for (int ix = 0; ix < param.Length; ix++)
                {
                    // Создаем новый объект параметра инструкции.
                    LogicInstructionParameter newLogicInstructionParameter = new LogicInstructionParameter(ix.ToString(), "", ParameterUsage.Null);

                    newLogicInstructionParameter.Value = param[ix];
                    // Добавляем параметр в контейнер.
                    this.Parameters.Add(newLogicInstructionParameter);
                    // Добавляем параметр в структуру дерева данной логической инструкции.
                    this.Add(newLogicInstructionParameter);
                }
            }
        }

        /// <summary>
        /// Ищет лигические инструкции в прямом или обратном логически связанном направлении.
        /// </summary>
        /// <param name="instr">Текущая инструкция.</param>
        /// <param name="abort">Условие прекращения поиска.</param>
        /// <param name="select">Условие выбора инструкции.</param>
        /// <param name="maxCount">Максимальное кол-во найденных компонентов (при значении 0 кол-во не учитывается).</param>
        /// <param name="backwardDirection">Направление назад.</param>
        /// <returns></returns>
        public List<LogicInstruction> SearchByLogicPath(Func<LogicInstruction, bool> abort, Func<LogicInstruction, bool> select,Func<LogicInstruction, bool> next, int maxCount = 0, bool backwardDirection = false)
        {
            List<LogicInstruction> result = new List<LogicInstruction>();
            FindNext(this, abort, select, next, maxCount, backwardDirection, result);
            return result;
        }

        /// <summary>
        /// Рекурсивная функция для поиска лигических инструкций в прямом или обратном логически связанном направлении.
        /// </summary>
        /// <param name="instr">Текущая инструкция.</param>
        /// <param name="abort">Условие прекращения поиска.</param>
        /// <param name="select">Условие выбора инструкции.</param>
        /// <param name="next">Условие продолжения поиска.</param>
        /// <param name="maxCount">Максимальное кол-во найденных компонентов (при значении 0 кол-во не учитывается).</param>
        /// <param name="backwardDirection">Направление назад.</param>
        /// <param name="resultInstructions">Сквозной параметр с контейнером результата.</param>
        private static void FindNext(LogicInstruction instr, Func<LogicInstruction, bool> abort, Func<LogicInstruction, bool> select, Func<LogicInstruction, bool> next, int maxCount, bool backwardDirection, List<LogicInstruction> resultInstructions)
        {
            // Проверка входных параметров.
            if (instr == null || resultInstructions == null || (maxCount > 0 && resultInstructions.Count >= maxCount))
            {
                return;
            }
            // Прекращение поиска по данной логической ветви.
            if (abort.Invoke(instr))
            {
                return;
            }

            // Проверка текущей инструкции по заданному условию.
            if (select.Invoke(instr))
            {
                resultInstructions.Add(instr);
            }

            // Условие продолжения поиска.
            if (!next.Invoke(instr))
            {
                return;
            }

            // Выбор направления поиска по условию.
            List<LogicInstruction> instructions;
            if (backwardDirection)
            {
                instructions = instr.EnableIn;
            }
            else
            {
                instructions = instr.EnableOut;
            }

            if (instructions == null || instructions.Count <= 0)
            {
                return;
            }

            // Рекурсивный вход для следующих функций.
            foreach (LogicInstruction nextInstruction in instructions)
            {
                FindNext(nextInstruction, abort, select, next, maxCount, backwardDirection, resultInstructions);
            }
        }

        /// <summary>
        /// Преобразование объекта в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name + "(" + String.Join(", ", this.Parameters.Select(t => t.ToString()).ToArray()) + ")";
        }
    }
}
