using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EIP.AllenBradley
{
    public class TagValueWriting : TagValueControl
    {

        private List<byte[]> _RequestedData;
        /// <summary>
        /// Возвращает или задает запрашиваемые данные тэга в виде массива байт 
        /// для записи в удаленное устройство (контроллер).
        /// </summary>
        public List<byte[]> RequestedData
        {
            get { return this._RequestedData; }
            set { this._RequestedData = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public TagValueWriting(TagDataTypeDefinition type)
            : base(type)
        {
        }

        /// <summary>
        /// Устанавливает текущее значение для записи в редактируемый отчет в виде массива байт и делает отметку о том, 
        /// является ли новое значение отличным от предыдущего значения.
        /// </summary>
        internal void SetValueData()
        {
            this.SetValueData(this._RequestedData);
        }
    }
}
