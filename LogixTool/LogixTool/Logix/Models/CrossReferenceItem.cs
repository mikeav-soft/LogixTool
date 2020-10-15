using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Logix.Models
{
    /// <summary>
    /// Элемент перекрестных ссылок который содержит ссылку на объект и ее свойства.
    /// </summary>
    public class CrossReferenceItem
    {
        /// <summary>
        /// Тип ссылки.
        /// </summary>
        public CrossReferenceType Type { get; set; }
        /// <summary>
        /// Ссылка на ссылаемый объект.
        /// </summary>
        public Object LinkToObject { get; set; }

        /// <summary>
        /// Создает новый элемент перекрестных ссылок.
        /// </summary>
        /// <param name="type">Тип ссылки.</param>
        /// <param name="obj">JОбъект на который идет ссылка.</param>
        public CrossReferenceItem(CrossReferenceType type, object obj)
        {
            this.Type = type;
            this.LinkToObject = obj;
        }

    }
}
