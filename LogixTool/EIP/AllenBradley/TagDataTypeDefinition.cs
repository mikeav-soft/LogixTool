using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EIP.AllenBradley.Models;

namespace EIP.AllenBradley
{
    /// <summary>
    /// Класс описывающий основную информацию типа данных контроллерного тэга.
    /// </summary>
    public class TagDataTypeDefinition
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        private string _Name;
        /// <summary>
        /// Возвращает или задает название типа данных.
        /// В случае если значение задаоно Null то возвращаться будут значения по умолчанию 
        /// для атамарных типов или общее значение структуры.
        /// </summary>
        public string Name
        {
            get
            {
                if (this._Name != null)
                {
                    return this._Name;
                }
                else
                {
                    string result = "";

                    switch (this.Code)
                    {
                        case 0x00: result = ""; return result;
                        case 0xC1: result = "BOOL"; break;
                        case 0xC2: result = "SINT"; break;
                        case 0xC3: result = "INT"; break;
                        case 0xC4: result = "DINT"; break;
                        case 0xC5: result = "LINT"; break;
                        case 0xCA: result = "REAL"; break;
                        case 0xD3:
                            {
                                result = "BOOL[..]";
                            }
                            break;

                        default: result = "STRUCT"; break;
                    }

                    if (this.AtomicBitPosition != null)
                    {
                        result += "." + this.AtomicBitPosition;
                    }

                    return result;
                }
            }
            set
            {
                this._Name = value;
            }
        }

        /// <summary>
        /// Возвращает или задает код типа данных тэга.
        /// </summary>
        public UInt16 Code { get; set; }
        /// <summary>
        /// Возвращает условное семейство типа данных определяющее ее характер.
        /// </summary>
        public TagDataTypeFamily Family
        {
            get
            {
                switch (this.Code)
                {
                    case 0x00:
                        return TagDataTypeFamily.Null;
                    case 0xC1:
                        return TagDataTypeFamily.AtomicBool;
                    case 0xC2:
                    case 0xC3:
                    case 0xC4:
                        return TagDataTypeFamily.AtomicDecimal;
                    case 0xC5:
                    case 0xD3:
                        return TagDataTypeFamily.AtomicBoolArray;
                    case 0xCA:
                        return TagDataTypeFamily.AtomicFloat;
                    default:
                        return TagDataTypeFamily.Structure;
                }
            }
        }

        private UInt16 _Size;
        /// <summary>
        /// Возвращает или задает размер в байтах текущего типа данных.
        /// В случае если тип данных атомарный то возвращается их значения по умолчанию несмотря на то какие значения были присвоены данному свойству.
        /// В случае если тип данных является структурой то возвращается заданное значение данному свойству. 
        /// В случае если невозможно определить размер возвращается 0.
        /// </summary>
        public UInt16 Size
        {
            get
            {
                switch (this.Code)
                {
                    case 0xC1:
                    case 0xC2:
                        {
                            return 1;
                        }
                    case 0xC3:
                        {
                            return 2;
                        }
                    case 0xC4:
                    case 0xCA:
                    case 0xD3:
                        {
                            return 4;
                        }
                    case 0xC5:
                        {
                            return 8;
                        }
                    default:
                        {
                            return this._Size;
                        }
                }
            }
            set
            {
                this._Size = value;
            }
        }

        /// <summary>
        /// Возвращает или задает текущий линейный индекс элемента массива если он таковым является.
        /// При значении Null определение не является элементом массива.
        /// </summary>
        public UInt32? ArrayIndex { get; set; }
        /// <summary>
        /// Возвращает или задает кол-во элементов чтения в случае если необходимо табличное чтение массива.
        /// </summary>
        public ArrayDefinition ArrayDimension { get; set; }
        /// <summary>
        /// Возвращает или задает возможность выбора номера бита в случае если задан целочисленном атомарном тип данных (SINT, INT, DINT).
        /// </summary>
        public UInt16? AtomicBitPosition { get; set; }
        /// <summary>
        /// Возвращает или задает смещение байт для извлечения данных структуры.
        /// </summary>
        public UInt32 StructureByteOffset { get; set; }
        /// <summary>
        /// Возвращает или задает позицию данного бита в слове для извлечения данных структуры.
        /// Задается в случае если данный тип данных является битом (Code=0xC1).
        /// </summary>
        public UInt32? StructureBitPosition { get; set; }
        /// <summary>
        /// Возвращает или задает название элемента структуры - держателя текущего элемента который является битом (BOOL, 0xC1).
        /// Значение может быть в случае если данный тип данных является битом (Code=0xC1).
        /// </summary>
        public string HiddenMemberName { get; set; }
        /// <summary>
        /// Возвращает или задает смещение 4-х байтных слов для извлечения данных битового массива.
        /// </summary>
        public UInt32? BitArrayDWordOffset { get; set; }
        /// <summary>
        /// Возвращает или задает позицию данного бита в 4-х байтном слове для извлечения данных битового массива.
        /// </summary>
        public UInt32? BitArrayDWordBitPosition { get; set; }

        /// <summary>
        /// Возвращяет ожидаемый размер типа данных.
        /// </summary>
        public int ExpectedTotalSize
        {
            get
            {
                if (this.ArrayDimension.HasValue)
                {
                    return this.Size * this.ArrayDimension.Value;
                }
                else
                {
                    return this.Size;
                }
            }
        }
        /* ================================================================================================== */
        #endregion

        /// Создает новое описание типа на основании значения кода и размера.
        /// </summary>
        /// <param name="code">Значение кода типа данных.</param>
        public TagDataTypeDefinition(UInt16 code)
        {
            this.Code = code;
            this.ArrayIndex = null;
            this.ArrayDimension = new ArrayDefinition();
            this.AtomicBitPosition = null;
            this._Size = 0;
            this.StructureByteOffset = 0;
            this.StructureBitPosition = null;
            this.BitArrayDWordOffset = null;
            this.BitArrayDWordBitPosition = null;
            this.HiddenMemberName = null;
        }

        #region [ METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            this.Code = 0;
            this._Size = 0;
            this._Name = null;

            this.ArrayIndex = null;
            this.ArrayDimension.Max = 0;
            this.AtomicBitPosition = null;
            this.StructureByteOffset = 0;
            this.StructureBitPosition = null;
            this.BitArrayDWordOffset = 0;
            this.BitArrayDWordBitPosition = 0;
            this.HiddenMemberName = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeDefinition"></param>
        public void CopyFrom(TagDataTypeDefinition typeDefinition)
        {
            this.Code = typeDefinition.Code;
            this._Size = typeDefinition._Size;
            this._Name = typeDefinition._Name;

            this.ArrayIndex = typeDefinition.ArrayIndex;
            this.ArrayDimension.Max = typeDefinition.ArrayDimension.Max;
            this.ArrayDimension.Value = typeDefinition.ArrayDimension.Value;
            this.AtomicBitPosition = typeDefinition.AtomicBitPosition;
            this.StructureByteOffset = typeDefinition.StructureByteOffset;
            this.StructureBitPosition = typeDefinition.StructureBitPosition;
            this.BitArrayDWordOffset = typeDefinition.BitArrayDWordOffset;
            this.BitArrayDWordBitPosition = typeDefinition.BitArrayDWordBitPosition;
            this.HiddenMemberName = typeDefinition.HiddenMemberName;
        }
        /* ================================================================================================== */
        #endregion
    }
}
