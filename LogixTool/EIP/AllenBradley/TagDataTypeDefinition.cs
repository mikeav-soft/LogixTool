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
                    switch (this.Code)
                    {
                        case 0x00: return "";
                        case 0xC1: return "BOOL";
                        case 0xC2: return "SINT";
                        case 0xC3: return "INT";
                        case 0xC4: return "DINT";
                        case 0xC5: return "LINT";
                        case 0xCA: return "REAL";
                        case 0xD3: return "BOOL32";
                        default: return "STRUCT";
                    }
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
                        return TagDataTypeFamily.AtomicInteger;
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

        private UInt16 _ElementSize;
        /// <summary>
        /// Возвращает или задает размер в байтах текущего типа данных элемента.
        /// В случае если тип данных атомарный то возвращается их значения по умолчанию несмотря на то какие значения были присвоены данному свойству.
        /// В случае если тип данных является структурой то возвращается заданное значение данному свойству. 
        /// В случае если невозможно определить размер возвращается 0.
        /// </summary>
        public UInt16 ElementSize
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
                            return this._ElementSize;
                        }
                }
            }
            set
            {
                this._ElementSize = value;
            }
        }
        /// <summary>
        /// Возвращяет ожидаемый размер типа данных.
        /// </summary>
        public int TotalSize
        {
            get
            {
                if (this.ArrayDimension.HasValue)
                {
                    return this.ElementSize * this.ArrayDimension.Value;
                }
                else
                {
                    return this.ElementSize;
                }
            }
        }
        /// <summary>
        /// Возвращает или задает кол-во элементов чтения в случае если необходимо табличное чтение массива.
        /// </summary>
        public ArrayDefinition ArrayDimension { get; set; }

        /// <summary>
        /// Возвращает или задает название элемента структуры - держателя текущего элемента который является битом (BOOL, 0xC1).
        /// Значение может быть в случае если данный тип данных является битом (Code=0xC1).
        /// </summary>
        public string HiddenMemberName { get; set; }

        /// <summary>
        /// Возвращает или задает смещение байт/бит для извлечения данных бита атомарного числа.
        /// </summary>
        public BitOffsetPosition AtomicBitDefinition { get; set; }
        /// <summary>
        /// Возвращает или задает смещение байт/бит для извлечения данных бита структуры.
        /// </summary>
        public BitOffsetPosition StructureDefinition { get; set; }
        /// <summary>
        /// Возвращает или задает смещение байт/бит для извлечения данных бита булевого массива.
        /// </summary>
        public BitOffsetPosition BitArrayDefinition { get; set; }
        /* ================================================================================================== */
        #endregion

        /// Создает новое описание типа на основании значения кода и размера.
        /// </summary>
        /// <param name="code">Значение кода типа данных.</param>
        public TagDataTypeDefinition(UInt16 code)
        {
            this.Code = code;
            this.ArrayDimension = new ArrayDefinition();
            this._ElementSize = 0;
            this.HiddenMemberName = null;

            this.AtomicBitDefinition = null;
            this.StructureDefinition = null;
            this.BitArrayDefinition = null;
        }

        #region [ PUBLIC METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Производит сброс состояния содержимого объекта в первоначальное состояние.
        /// </summary>
        public void Init()
        {
            this.Code = 0;
            this._ElementSize = 0;
            this._Name = null;

            this.ArrayDimension.Init();
            this.HiddenMemberName = null;
            this.AtomicBitDefinition = null;
            this.StructureDefinition = null;
            this.BitArrayDefinition = null;
        }
        /// <summary>
        /// Клонирует содержимое внешнего объекта в текущий объект.
        /// </summary>
        /// <param name="typeDefinition"></param>
        public void CloneFrom(TagDataTypeDefinition typeDefinition)
        {
            this.Code = typeDefinition.Code;
            this._ElementSize = typeDefinition._ElementSize;
            this._Name = typeDefinition._Name;

            this.ArrayDimension.CloneFrom(typeDefinition.ArrayDimension);
            this.AtomicBitDefinition.CloneFrom(typeDefinition.AtomicBitDefinition);
            this.StructureDefinition.CloneFrom(typeDefinition.StructureDefinition);
            this.BitArrayDefinition.CloneFrom(typeDefinition.BitArrayDefinition);

            this.HiddenMemberName = typeDefinition.HiddenMemberName;
        }
        /// <summary>
        /// Преобразовывает текущий объект в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // Случай когда текущий тип является целым числом и задан конкретный номер бита.
            if (this.Family == TagDataTypeFamily.AtomicInteger)
            {
                if (this.AtomicBitDefinition != null)
                {
                    return "BOOL";
                }
            }

            // Случай когда текущий тип является битовым массивом.
            if (this.Family == TagDataTypeFamily.AtomicBoolArray)
            {
                if (this.BitArrayDefinition != null)
                {
                    return "BOOL";
                }
                else
                {
                    string dims = "";
                    byte rank = this.ArrayDimension.Rank;

                    if (rank > 0) dims = (this.ArrayDimension.Dim0 * 32).ToString();
                    if (rank > 1) dims += "," + (this.ArrayDimension.Dim1 * 32).ToString();
                    if (rank > 2) dims += "," + (this.ArrayDimension.Dim2 * 32).ToString();
                    if (rank > 0)
                        return "BOOL[" + dims + "]";
                    else
                        return "BOOL[?]";
                }
            }

            return this.Name + this.ArrayDimension.ToString();
        }
        /* ================================================================================================== */
        #endregion
    }
}
