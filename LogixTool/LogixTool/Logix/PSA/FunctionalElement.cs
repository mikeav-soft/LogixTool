using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Logix.PSA
{
    public class FunctionalElement
    {
        /// <summary>
        /// Показывает приоритет элемента (меньшее имеет больший приоритет).
        /// </summary>
        public int Priority { get; private set; }
        /// <summary>
        /// Название элемента.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Короткое имя элемента.
        /// </summary>
        public string Prefix { get; private set; }
        /// <summary>
        /// Индекс элемента.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Создает новый функциональный элемент.
        /// </summary>
        public FunctionalElement()
        {
            this.Priority = -1;
            this.Name = "";
            this.Prefix = "";
            this.Index = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static FunctionalElement Parse(string s)
        {
            FunctionalElement element = new FunctionalElement();
            if (s == null && s.Trim().Length == 0)
            {
                return null;
            }

            string text = s.Trim();

            Dictionary<string, List<string>> keys = new Dictionary<string, List<string>>();
            keys.Add("PLC", new List<string>() { "W", "P" });
            keys.Add("CELL", new List<string>() { "C" });
            keys.Add("RMG", new List<string>() { "X" });
            keys.Add("ENT", new List<string>());
            keys.Add("ROBOT", new List<string>() { "R" });
            keys.Add("WELDING", new List<string>() { "D" });
            keys.Add("PFL", new List<string>() { "U" });

            return element;
        }

    }
}
