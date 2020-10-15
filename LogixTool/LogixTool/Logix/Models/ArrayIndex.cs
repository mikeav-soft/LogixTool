using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Logix.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ArrayIndex : ArrayDefinition
    {
        /// <summary>
        /// Индексы массива.
        /// </summary>
        public object[] Indexes { get; set; }
        /// <summary>
        /// Возвращает информацию о том что данный индекс является явным.
        /// </summary>
        public bool IsExplict
        {
            get
            {
                return (this.Indexes != null && this.Indexes.All(t => t is uint));
            }
        }
        /// <summary>
        /// Возвращает линейную позицию текущего индекса.
        /// </summary>
        public uint LinearPosition
        {
            get
            {
                if (this.IsExplict)
                {
                    if (this.Rank == 1)
                    {
                        return (uint)this.Indexes[0];
                    }

                    if (this.Rank == 2)
                    {
                        return (uint)this.Indexes[1] * this.Dim0 + (uint)this.Indexes[0];
                    }

                    if (this.Rank == 3)
                    {
                        return (uint)this.Indexes[2] * this.Dim1 * this.Dim0 + (uint)this.Indexes[1] * this.Dim0 + (uint)this.Indexes[0];
                    }

                    return 0;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dim2"></param>
        /// <param name="dim1"></param>
        /// <param name="dim0"></param>
        /// <param name="index2"></param>
        /// <param name="index1"></param>
        /// <param name="index0"></param>
        public ArrayIndex(uint dim2, uint dim1, uint dim0, object index2, object index1, object index0)
            : base(dim2, dim1, dim0)
        {
            this.Indexes = new object[this.Rank];

            if (this.Indexes.Length >= 1)
            {
                this.Indexes[0] = index0;
            }

            if (this.Indexes.Length >= 2)
            {
                this.Indexes[1] = index1;
            }

            if (this.Indexes.Length == 3)
            {
                this.Indexes[2] = index2;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="def"></param>
        /// <param name="index2"></param>
        /// <param name="index1"></param>
        /// <param name="index0"></param>
        public ArrayIndex(ArrayDefinition def, object index2, object index1, object index0) :
            this(def.Dim2, def.Dim1, def.Dim0, index2, index1, index0)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dim2"></param>
        /// <param name="dim1"></param>
        /// <param name="dim0"></param>
        /// <param name="indexStr"></param>
        public ArrayIndex(uint dim2, uint dim1, uint dim0, string indexStr)
            : base(dim2, dim1, dim0)
        {
            this.Indexes = new object[this.Rank];
            for (int ix = 0; ix < this.Indexes.Length; ix++)
            {
                this.Indexes[ix] = 0;
            }

            if (indexStr == null || indexStr.Trim() == "")
            {
                return;
            }

            string[] parts = indexStr.Replace(" ", "").Split(",".ToCharArray()).Reverse().ToArray();

            for (int ix = 0; ix < parts.Length && ix < this.Indexes.Length; ix++)
            {
                if (parts[ix].IsDigits())
                {
                    this.Indexes[ix] = Convert.ToUInt32(parts[ix]);
                }
                else
                {
                    this.Indexes[ix] = parts[ix];
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="def"></param>
        /// <param name="index2"></param>
        /// <param name="index1"></param>
        /// <param name="index0"></param>
        public ArrayIndex(ArrayDefinition def, string indexStr) :
            this(def.Dim2, def.Dim1, def.Dim0, indexStr)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void CheckAndOptimize ()
        {
            if (!this.IsExplict)
            {
                return;
            }

            uint[] dim = this.DimToArray();

            // 1. Проверяем индексы и размерность на равенственства пространства.
            if (this.Indexes.Length != this.Rank)
            {
                object[] indexes = new object[this.Rank];
                for (int ix = 0; ix < indexes.Length; ix++)
                {
                    if (this.Indexes.Length > ix)
                    {
                        indexes[ix] = (uint)this.Indexes[ix];
                    }
                    else
                    {
                        indexes[ix] = (uint)0;
                    }
                }
                this.Indexes = indexes;
            }

            // 2. Проверяем индексы на переполнение значения в сравнении с Dim.
            for (int ix = 0; ix < dim.Length && ix < this.Indexes.Length; ix++)
            {
                bool lastIx = ix == dim.Length - 1 || ix == this.Indexes.Length - 1;

                if (dim[ix] < (uint)this.Indexes[ix])
                {
                    if (lastIx)
                    {
                        this.Indexes[ix] = dim[ix];
                    }
                    else
                    {
                        uint a = (uint)this.Indexes[ix] / dim[ix];
                        uint f = (uint)this.Indexes[ix] - a * dim[ix];

                        this.Indexes[ix] = f;
                        this.Indexes[ix + 1] = (uint)this.Indexes[ix + 1] + a;
                    }
                }
            }
        }

        /// <summary>
        /// Преобразование объекта в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Join(",", this.Indexes.Reverse());
        }
    }
}
