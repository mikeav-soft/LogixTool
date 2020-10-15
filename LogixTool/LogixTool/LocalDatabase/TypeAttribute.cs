using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.LocalDatabase
{
    /// <summary>
    /// 
    /// </summary>
    internal class TypeAttribute : Attribute
    {
        /// <summary>
        /// Название типа хранимой информации.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Создает новый атрибут.
        /// </summary>
        /// <param name="name">Название типа хранимой информации.</param>
        public TypeAttribute(string name)
        {
            this.Name = name;
        }

    }
}
