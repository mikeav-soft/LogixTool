using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EIP.AllenBradley.Models
{
    public class ArrayDefinition
    {
        #region [ PROPERTIES ]
        /* ======================================================================================== */
        /// <summary>
        /// Возвращает или задает максимальное число элементов чтения массива
        /// определяемое удаленным устройством.
        /// </summary>
        public UInt32 Max { get; set; }

        private UInt16 _Value;
        /// <summary>
        /// Возвращает или задает число элементов чтения массива. 
        /// </summary>
        public UInt16 Value
        {
            get
            {
                return this._Value;
            }

            set
            {
                if (this.Max > 0 && (value > this.Max || value > UInt16.MaxValue))
                {
                    return;
                }

                this._Value = value;
            }
        }

        /// <summary>
        /// Возращает значение True в случае если число элементов чтения отлично от 0.
        /// </summary>
        public bool HasValue
        {
            get
            {
                return this._Value > 0;
            }
        }
        /* ======================================================================================== */
        #endregion

        /// <summary>
        /// Создает новое определение длины массива чтения.
        /// </summary>
        public ArrayDefinition ()
        {
            this.Max = 0;
            this._Value = 0;
        }

        /// <summary>
        /// Преобразовывает текущий объект в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "";

            if (this.HasValue)
            {
                result = this._Value.ToString();

                if (this.Max > 0)
                {
                    result += " of " + this.Max;
                }
            }
            return result;
        }
    }
}
