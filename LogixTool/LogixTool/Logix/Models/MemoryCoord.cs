using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Logix.Models
{
    /// <summary>
    /// Представляет собой координату описвающий расположение того или иного объекта в простанстве памяти,
    /// где само пространство памяти представляет собой таблицу из множества рядов каждый из которых состоит 
    /// из множества позиций строго заданного значения.
    /// </summary>
    public class MemoryCoord
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        private uint _Dim;
        /// <summary>
        /// Размерность одного ряда.
        /// При присвоении нового значения размерности значения ряда и позиции соответственно изменятся.
        /// Минимальное значение размерности может быть равным 1.
        /// </summary>
        public uint Dim
        {
            get
            {
                return this._Dim;
            }
            set
            {
                if (value == 0)
                {
                    this._Dim = 1;
                }
                else
                {
                    this._Dim = value;
                }
            }
        }

        /// <summary>
        /// Возвращает или задает номер ряда.
        /// </summary>
        public uint Row
        {
            get
            {
                if (this._Dim == 0)
                {
                    return 0;
                }
                else
                {
                    return (uint)(AbsoluteLength / Dim);
                }
            }
            set
            {
                uint pos = this.Pos;
                AbsoluteLength = (uint)(value * Dim + pos);
            }
        }
        /// <summary>
        /// Возвращает или задает номер позиции в текущем ряде.
        /// При присвоении значения позиции превышающую или равную размерности одного ряда значение ряда соответственно увеличится.
        /// </summary>
        public uint Pos
        {
            get
            {
                return (uint)(AbsoluteLength - Row * Dim);
            }
            set
            {
                uint pos = this.Pos;
                AbsoluteLength -= pos;
                AbsoluteLength += value;
            }
        }
        /// <summary>
        /// Возвращает или задает абсолютную позиция в элементарных единицах памяти.
        /// </summary>
        public long AbsoluteLength { get; set; }
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// Создает новую координату ячейки памяти в виде двумерной таблицы.
        /// </summary>
        /// <param name="rowLength">Размерность одного ряда.</param>
        public MemoryCoord(uint rowLength)
        {
            this.Dim = rowLength;
            this.Row = 0;
            this.Pos = 0;
        }
        /// <summary>
        /// Создает новую координату ячейки памяти в виде двумерной таблицы.
        /// </summary>
        /// <param name="rowLength">Размерность одного ряда.</param>
        /// <param name="row">Значение ряда.</param>
        /// <param name="pos">Значение позиции.</param>
        public MemoryCoord(uint rowLength, uint row, uint pos)
        {
            this.Dim = rowLength;
            this.Row = row;
            this.Pos = pos;
        }

        #region [ PUBLIC METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Создает новую полную копию данного объекта учитывая абсолютное смещение позиции.
        /// </summary>
        /// <param name="offsetPosition">Значение абсолютного смещеняи позиции.</param>
        /// <returns></returns>
        public MemoryCoord Offset(int offsetPosition)
        {
            // Запоминаем текущее значение абсолютной позиции.
            long absolutePos = this.AbsoluteLength;

            // Перед вычислением смещения определяем не выйдем ли мы за рамки значений от 0 до 9223372036854775807 (максимальное значение long).
            if (offsetPosition < 0 && absolutePos < (long)(Math.Abs(offsetPosition)))
            {
                // Случай когда смещение слишком велико и уходим в отрицательную область менее 0.
                absolutePos = 0;
            }
            else if (offsetPosition > 0 && long.MaxValue - absolutePos < offsetPosition)
            {
                // Случай когда смещение слишком велико и уходим в положительную область более максимума long.
                absolutePos = 0;
            }
            else
            {
                absolutePos = absolutePos + offsetPosition;
            }

            // Создаем новый объект и присваеваем значение абсолютного значения позиции.
            MemoryCoord result = new MemoryCoord(this.Dim);
            result.AbsoluteLength = absolutePos;
            return result;
        }
        /// <summary>
        /// Проеобразует текущий объект в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Row.ToString() + ":[" + this.Pos.ToString() + "/" + this.Dim.ToString() + "]";
        }
        /* ================================================================================================== */
        #endregion
    }
}
