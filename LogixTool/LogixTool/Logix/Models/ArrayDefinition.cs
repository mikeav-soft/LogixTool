using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Logix.Models
{
    /// <summary>
    /// Класс описывающий размерность массива RSLogix 5000.
    /// </summary>
    public class ArrayDefinition
    {
        /// <summary>
        /// Размер массива в размерности "0".
        /// Равенство индекса нулю равносильно отсутствием размерности.
        /// </summary>
        public uint Dim0 { get; private set; }
        /// <summary>
        /// Размер массива в размерности "1".
        /// Равенство индекса нулю равносильно отсутствием размерности.
        /// </summary>
        public uint Dim1 { get; private set; }
        /// <summary>
        /// Размер массива в размерности "2".
        /// Равенство индекса нулю равносильно отсутствием размерности.
        /// </summary>
        public uint Dim2 { get; private set; }

        /// <summary>
        /// Возвращает размерность массива.
        /// </summary>
        public int Rank
        {
            get
            {
                int result = 0;
                if (this.Dim0 > 0) result++; else return result;
                if (this.Dim1 > 0) result++; else return result;
                if (this.Dim2 > 0) result++; else return result;
                return result;
            }
        }
        /// <summary>
        /// Возвращает линейную длину массива.
        /// </summary>
        public int Length
        {
            get
            {
                uint [] dimarr = this.DimToArray();

                if (dimarr.Length > 0)
                {
                    int result = 1;
                    foreach (int d in dimarr)
                    {
                        result *= d;
                    }
                    return result;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Создает новое описание размрности массива с нулевыми значениями.
        /// </summary>
        public ArrayDefinition ()
        {
            this.Dim0 = 0;
            this.Dim1 = 0;
            this.Dim2 = 0;
        }
        /// <summary>
        /// Создает новое описание размрности массива с заданными значениями.
        /// </summary>
        /// <param name="dim2">Размер массива в размерности "2"</param>
        /// <param name="dim1">Размер массива в размерности "1"</param>
        /// <param name="dim0">Размер массива в размерности "0"</param>
        public ArrayDefinition(uint dim2, uint dim1, uint dim0)
        {
            this.Dim0 = dim0;
            this.Dim1 = dim1;
            this.Dim2 = dim2;
        }
        /// <summary>
        /// Создает новый объект размерности массива из заданных значений в виде строки разделенных занятой.
        /// </summary>
        /// <param name="instr">Строка с циферными индексами перечисленных через запятую.</param>
        public ArrayDefinition(string instr)
            : this()
        {
            // Проверка значений входных параметров.
            if (instr == null || instr.Trim() == "")
            {
                return;
            }

            int dimIndex = 0;
            foreach (string dimstr in instr.Replace(" ", "").Split(",".ToCharArray()).Reverse())
            {
                if (dimstr.IsDigits())
                {
                    uint value = Convert.ToUInt32(dimstr);

                    if (value == 0)
                    {
                        return;
                    }

                    switch (dimIndex)
                    {
                        case 0: this.Dim0 = value; break;
                        case 1: this.Dim1 = value; break;
                        case 2: this.Dim2 = value; break;
                        default: return;
                    }
                }
                dimIndex++;
            }
        }

        /// <summary>
        /// Получает последовательность индексов которые находятся в перечислении для данной размерности.
        /// </summary>
        public List<String> GetIndexSequence()
        {
            List<String> names = new List<String>();
            uint[] reference = this.DimToArray();
            uint[] indexes = new uint[this.Rank];

            while (true)
            {
                names.Add(string.Join(",", indexes.Reverse().Select(s => s.ToString())));
                indexes[0]++;

                for (int ix = 0; ix < this.Rank - 1; ix++)
                {
                    if (indexes[ix] >= reference[ix])
                    {
                        indexes[ix] = 0;
                        indexes[ix + 1]++;
                    }
                }

                if (indexes[indexes.Length - 1] >= reference[reference.Length - 1])
                {
                    break;
                }
            }

            return names;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected uint[] DimToArray()
        {
            uint[] result = new uint[this.Rank];

            if (this.Rank >= 1)
            {
                result[0] = this.Dim0;
            }
            if (this.Rank >= 2)
            {
                result[1] = this.Dim1;
            }
            if (this.Rank >= 3)
            {
                result[2] = this.Dim2;
            }

            return result;
        }
        /// <summary>
        /// Преобразование объекта в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = "";

            if (this.Rank == 0)
            {
                return result;
            }

            if (this.Rank >= 1)
            {
                result = this.Dim0.ToString() ;
            }
            if (this.Rank >= 2)
            {
                result = this.Dim1.ToString() + ","+ result;
            }
            if (this.Rank >= 3)
            {
                result = this.Dim2.ToString() + ","+ result;
            }

            return result;
        }
    }
}
