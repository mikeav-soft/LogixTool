using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Logix.PSA
{
    public class RoutineName
    {
        /// <summary>
        /// Функциональны заголовок рутины.
        /// </summary>
        public LogicNamePrefix Prefix { get; set; }
        /// <summary>
        /// Функциональные элементы имени.
        /// </summary>
        public List<FunctionalElement> Items { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        public RoutineName (LogicNamePrefix prefix)
        {
            this.Items = new List<FunctionalElement>();
        }
    }
}
