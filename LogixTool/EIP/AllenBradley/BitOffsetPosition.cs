using System;
using System.Collections.Generic;
using System.Text;

namespace EIP.AllenBradley
{
    /// <summary>
    /// Определяет размерность поля элемента в байтах.
    /// </summary>
    public enum BytesInElement : byte { OneByte = 1, TwoBytes = 2, FourBytes = 4, EightBytes = 8 }

    /// <summary>
    /// Представляет собой класс описывающий положение данных (слова/байта а также бита) в некоторой последовательности байт.
    /// </summary>
    public class BitOffsetPosition
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        /// <summary>
        /// Возвращает или задает кол-во элементов смещения для определения положения бита.
        /// </summary>
        public UInt32 ElementOffset
        {
            get
            {
                return (UInt32)(absoluteBitOffset & ~GetBitPartMask()) >> GetBitPartSize();
            }
            set
            {

                absoluteBitOffset = (absoluteBitOffset & GetBitPartMask()) | (value << GetBitPartSize());
            }
        }
        /// <summary>
        /// Возвращает или задает кол-во бит смещения в текущем элементе.
        /// </summary>
        public UInt32? BitOffset
        {
            get
            {
                if (bitPartNotDefined) return null;

                return (UInt32)(absoluteBitOffset & GetBitPartMask());
            }
            set
            {
                bitPartNotDefined = value == null;

                UInt32 bitval = 0;

                if (!bitPartNotDefined)
                {
                    bitval = value.Value;
                }

                if (bitval > GetBitPartMask())
                    throw new ArgumentException("BitOffset value must be range 0..." + GetBitPartMask().ToString(), "BitOffset");

                absoluteBitOffset = (absoluteBitOffset & ~GetBitPartMask()) | bitval;
            }
        }
        /// <summary>
        /// Возвращает или задает размерность в байтах одного элемента.
        /// </summary>
        public BytesInElement ElementSize { get; private set; }
        /// <summary>
        /// Возвращает абсолютное кол-во бит.
        /// </summary>
        public UInt64 TotalBitOffset
        {
            get
            {
                return (UInt64)this.absoluteBitOffset;
            }
            set
            {
                this.absoluteBitOffset = value;
            }
        }
        /* ================================================================================================== */
        #endregion

        private bool bitPartNotDefined;         // Определяет что битовая часть не определена.
        private UInt64 absoluteBitOffset;       // Текущее абсолютное положение бита.

        /// <summary>
        /// Создает определение положения данных на основе смещения.
        /// </summary>
        /// <param name="elementSize"></param>
        public BitOffsetPosition(BytesInElement elementSize)
        {
            this.bitPartNotDefined = false;
            this.absoluteBitOffset = 0;
            this.ElementSize = elementSize;
        }

        #region [ PUBLIC METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Создает новый объект с клонированием содержимого с установкой новой длины размерности элемента.
        /// </summary>
        /// <param name="elementSize">Размерность элемента.</param>
        /// <returns></returns>
        public BitOffsetPosition Clone(BytesInElement elementSize)
        {
            BitOffsetPosition result = new BitOffsetPosition(elementSize);
            result.TotalBitOffset = this.TotalBitOffset;
            return result;
        }
        /// <summary>
        /// Создает новый объект с клонированием содержимого.
        /// </summary>
        /// <returns></returns>
        public BitOffsetPosition Clone()
        {
            return this.Clone(this.ElementSize);
        }
        /// <summary>
        /// Клонирует содержимое внешнего объекта в текущий объект.
        /// </summary>
        /// <param name="position"></param>
        public void CloneFrom(BitOffsetPosition position)
        {
            if (position == null)
                throw new ArgumentNullException("Method='CloneFrom', Parameter='position', can not be Null.", "position");

            this.TotalBitOffset = position.TotalBitOffset;
            this.ElementSize = position.ElementSize;
        }
        /* ================================================================================================== */
        #endregion

        #region [ PRIVATE METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private UInt32 GetBitPartMask()
        {
            switch (this.ElementSize)
            {
                case BytesInElement.OneByte: return 0x07;
                case BytesInElement.TwoBytes: return 0x0F;
                case BytesInElement.FourBytes: return 0x1F;
                case BytesInElement.EightBytes: return 0x3F;
            }

            return 0x00;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private byte GetBitPartSize()
        {
            switch (this.ElementSize)
            {
                case BytesInElement.OneByte: return 3;
                case BytesInElement.TwoBytes: return 4;
                case BytesInElement.FourBytes: return 5;
                case BytesInElement.EightBytes: return 6;
            }

            return 0x00;
        }
        /* ================================================================================================== */
        #endregion
    }
}
