using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EIP.AllenBradley
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
            get { return this._Value; }
            set
            {
                if (this.LinearDim > 0 && (value > this.LinearDim || value > UInt16.MaxValue))
                    return;

                this._Value = value;
            }
        }

        /// <summary>
        /// Возращает значение True в случае если число элементов чтения отлично от 0.
        /// </summary>
        public bool HasValue
        {
            get { return this._Value > 0; }
        }
        /// <summary>
        /// Возвращает или задает размерность "0" массива.
        /// Если значение равно 0, то данная размерность не существует.
        /// </summary>
        public UInt32 Dim0 { get; set; }
        /// <summary>
        /// Возвращает или задает размерность "1" массива.
        /// Если значение равно 0, то данная размерность не существует.
        /// </summary>
        public UInt32 Dim1 { get; set; }
        /// <summary>
        /// Возвращает или задает размерность "2" массива.
        /// Если значение равно 0, то данная размерность не существует.
        /// </summary>
        public UInt32 Dim2 { get; set; }
        /// <summary>
        /// Возвращает кол-во размерностей массива данного тэга.
        /// При значении 0 данный тэг не является масиивом.
        /// </summary>
        public byte Rank
        {
            get
            {
                if (this.Dim0 == 0)
                {
                    return 0;
                }
                else if (this.Dim1 == 0)
                {
                    return 1;
                }
                else if (this.Dim2 == 0)
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
        public UInt32 LinearDim
        {
            get
            {
                UInt32 result = 0;

                if (this.Dim0 > 0)
                    result = this.Dim0;

                if (this.Dim1 > 0)
                    result *= this.Dim1;

                if (this.Dim2 > 0)
                    result *= this.Dim2;

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
            this.Dim0 = 0;
            this.Dim1 = 0;
            this.Dim2 = 0;
            this._Value = 0;
        }

        #region [ PUBLIC METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Производит сброс состояния содержимого объекта в первоначальное состояние.
        /// </summary>
        public void Init()
        {
            this.Dim0 = 0;
            this.Dim1 = 0;
            this.Dim2 = 0;
            this._Value = 0;
        }
        /// <summary>
        /// Создает новый объект с клонированием содержимого.
        /// </summary>
        /// <returns></returns>
        public ArrayDefinition Clone()
        {
            ArrayDefinition result = new ArrayDefinition();
            result.Dim0 = this.Dim0;
            result.Dim1 = this.Dim1;
            result.Dim2 = this.Dim2;
            result.Value = this.Value;
            return result;
        }
        /// <summary>
        /// Клонирует содержимое внешнего объекта в текущий объект.
        /// </summary>
        /// <returns></returns>
        public void CloneFrom(ArrayDefinition arrayDefinition)
        {
            if (arrayDefinition == null)
                throw new ArgumentNullException("Method='CloneFrom', Parameter='arrayDefinition', can not be Null.", "arrayDefinition");

            this.Dim0 = arrayDefinition.Dim0;
            this.Dim1 = arrayDefinition.Dim1;
            this.Dim2 = arrayDefinition.Dim2;
            this.Value = arrayDefinition.Value;
        }
        /// <summary>
        /// Преобразовывает текущий объект в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "";
            byte rank = this.Rank;

            if (rank > 0)
                result += this.Dim0.ToString();

            if (rank > 1)
                result += "," + this.Dim1.ToString();

            if (rank > 2)
                result += "," + this.Dim2.ToString();

            if (rank > 0)
                result = "[" + result + "]";

            return result;
        }
        /* ======================================================================================== */
        #endregion
    }
}
