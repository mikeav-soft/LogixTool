using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LogixTool.Logix.Models;
using LogixTool.Common;

namespace LogixTool.Logix
{
    /// <summary>
    /// Параметр инструкции RLL.
    /// Описывает значение и поведение параметра инструкции.
    /// </summary>
    public class LogicInstructionParameter : LogixTree
    {
        /// <summary>
        /// Название параметра.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Значение параметра.
        /// </summary>
        public Object Value { get; set; }
        /// <summary>
        /// Использование параметра.
        /// </summary>
        public ParameterUsage Usage { get; set; }

        /// <summary>
        /// Создает пустой параметр инструкции по умолчанию.
        /// </summary>
        public LogicInstructionParameter(string id, string name, ParameterUsage usage)
            : base(id)
        {
            this.Name = name;
            this.Usage = usage;
            this.Value = "";
        }
        /// <summary>
        /// Преобразует элемент в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
