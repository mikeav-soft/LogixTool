using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Common.Extension
{
    /// <summary>
    /// Класс для описания фрагмента строки.
    /// </summary>
    public class TextFragment
    {
        /// <summary>
        /// Содержание текста
        /// </summary>
        public string Value;
        /// <summary>
        /// Тип фрагмента строки.
        /// </summary>
        public TextType Type;

        /// <summary>
        /// 
        /// </summary>
        public TextFragment ()
        {
            this.Value = "";
            this.Type = TextType.Other;
        }

        /// <summary>
        /// Преобразует объект в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value;
        }
    }
}
