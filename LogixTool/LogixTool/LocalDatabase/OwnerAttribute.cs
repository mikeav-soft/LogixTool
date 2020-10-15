using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.LocalDatabase
{
    /// <summary>
    /// Атрибут владельца записи.
    /// </summary>
    internal class OwnerAttribute : Attribute
    {
        /// <summary>
        /// Название владельца данных.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Человекочитаемое описание владельца.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Создает новый атрибут элемента перечисления группы.
        /// </summary>
        /// <param name="name">Папка гда распологается данные относящиеся к владельцу данных.</param>
        /// <param name="description">Человекочитаемое описание владельца.</param>
        public OwnerAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
    }
}
