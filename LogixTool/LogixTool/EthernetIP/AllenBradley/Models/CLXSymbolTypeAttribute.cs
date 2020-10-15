using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogixTool.EthernetIP.AllenBradley.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class CLXSymbolTypeAttribute
    {
        /// <summary>
        /// Возвращает или задает код типа данных. 
        /// </summary>
        public UInt16 Code
        {
            get
            {
                return (UInt16)(this.Value & 0xFFF);
            }
        }
        /// <summary>
        /// Возвращает или задает позицию данного бита в слове, 
        /// только в случае если данный тип данных является битом (Code=0xC1).
        /// </summary>
        public byte? BitPosition
        {
            get
            {
                if (!this.IsStructure && ((this.Code & 0xFF) == 0xC1))
                {
                    return (byte)((this.Code & 0x700) >> 8);
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Возвращает значение размерности массива от 1 до 3х.
        /// Если значение "0", то значит данный элемент не является массивом.
        /// </summary>
        public byte ArrayRank
        {
            get
            { return (byte)((this.Value >> 13) & 0x3); }
        }
        /// <summary>
        /// Возвращает значение определяющее является ли данный тэг структурой в случае значения true,
        /// в противном случае означает что тип данных является элементарным.
        /// </summary>
        public bool IsStructure
        {
            get
            {
                return (this.Value & 0x8000) == 0x8000;
            }
        }
        /// <summary>
        /// Возвращает значение true если тип данных данного тэга является системным.
        /// </summary>
        public bool IsSystem
        {
            get
            {
                return (this.Value & 0x1000) == 0x1000;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public UInt16 Value { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public CLXSymbolTypeAttribute(UInt16 value)
        {
            this.Value = value;
        }
    }
}
