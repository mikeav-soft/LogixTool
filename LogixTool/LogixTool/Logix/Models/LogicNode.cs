using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogixTool.Common;

namespace LogixTool.Logix.Models
{
    /// <summary>
    /// Универсальный элемнт описывающий любой узел логики контроллера.
    /// Унаследован от класса Tree обладающим методами и свойствами для построения структуры дерева.
    /// </summary>
    public abstract class LogicNode : LogixTree
    {
        /// <summary>
        /// Описание эелемента.
        /// </summary>
        public LangText Description { get; set; }

        /// <summary>
        /// Создает новый пустой логический узел с заданным именем.
        /// </summary>
        /// <param name="name">Имя логического узла.</param>
        public LogicNode(string name)
            : base(name)
        {
        }
    }
}
