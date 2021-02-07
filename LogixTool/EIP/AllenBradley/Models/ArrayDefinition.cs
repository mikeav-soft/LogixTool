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
                if (this.Length > 0 && (value > this.Length || value > UInt16.MaxValue))
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



        /// <summary>
        /// Возвращает или задает размерность "0" массива.
        /// Если значение равно 0, то данная размерность не существует.
        /// </summary>
        public UInt32 ArrayDim0 { get; set; }
        /// <summary>
        /// Возвращает или задает размерность "1" массива.
        /// Если значение равно 0, то данная размерность не существует.
        /// </summary>
        public UInt32 ArrayDim1 { get; set; }
        /// <summary>
        /// Возвращает или задает размерность "2" массива.
        /// Если значение равно 0, то данная размерность не существует.
        /// </summary>
        public UInt32 ArrayDim2 { get; set; }
        /// <summary>
        /// Возвращает кол-во размерностей массива данного тэга.
        /// При значении 0 данный тэг не является масиивом.
        /// </summary>
        public byte ArrayRank
        {
            get
            {
                if (this.ArrayDim0 == 0)
                {
                    return 0;
                }
                else if (this.ArrayDim1 == 0)
                {
                    return 1;
                }
                else if (this.ArrayDim2 == 0)
                {
                    return 2;
                }
                else
                {
                    return 3;
                }
            }
        }
        /// <summary>
        /// Возвращает или задает максимальное число элементов чтения массива
        /// определяемое удаленным устройством.
        /// </summary>
        public UInt32 Length
        {
            get
            {
                UInt32 result = 0;

                if (this.ArrayDim0 > 0)
                    result = this.ArrayDim0;

                if (this.ArrayDim1 > 0)
                    result *= this.ArrayDim1;

                if (this.ArrayDim2 > 0)
                    result *= this.ArrayDim2;

                return result;
            }
        }
        /* ======================================================================================== */
        #endregion

        /// <summary>
        /// Создает новое определение длины массива чтения.
        /// </summary>
        public ArrayDefinition()
        {
            this.ArrayDim0 = 0;
            this.ArrayDim1 = 0;
            this.ArrayDim2 = 0;
            this._Value = 0;
        }

        /// <summary>
        /// Преобразовывает текущий объект в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "";
            byte rank = this.ArrayRank;

            if (rank > 0)
                result += this.ArrayDim0.ToString();

            if (rank > 1)
                result += "," + this.ArrayDim1.ToString();

            if (rank > 2)
                result += "," + this.ArrayDim2.ToString();

            if (rank > 0)
                result = "[" + result + "]";

            return result;
        }
    }
}
