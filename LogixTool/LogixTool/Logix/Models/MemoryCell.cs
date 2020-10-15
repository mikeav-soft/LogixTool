using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Logix.Models
{
    public class MemoryCell
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        /// <summary>
        /// Размерность одного ряда в таблице.
        /// </summary>
        public readonly uint ROW_DIMESION;
        /// <summary>
        /// Таблица с объектами распределенных по позициям.
        /// </summary>
        public Dictionary<string, MemoryObject> Memory { get; set; }
        /// <summary>
        /// Получает последнюю свободную координату в таблице памяти.
        /// </summary>
        public MemoryCoord NextFreeCoord
        {
            get
            {
                MemoryCoord result = new MemoryCoord (this.ROW_DIMESION);

                if (Memory!=null && Memory.Count>0)
                {
                    result.Row = Memory.Values.Last().StartCoord.Row;
                    result.Pos = Memory.Values.Last().StartCoord.Pos + Memory.Values.Last().Length;
                }

                return result;
            }
        }
        /// <summary>
        /// Получает кол-во строк в таблице.
        /// </summary>
        public int RowCount
        {
            get
            {
                int rowCount = 0;
                if (Memory.Count > 0)
                {
                    MemoryCoord lastCoord = NextFreeCoord.Offset(-1);
                    rowCount = (int)(lastCoord.Row + 1);
                }
                return rowCount;
            }
        }
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// Создает новую таблицу с распределением памяти.
        /// </summary>
        /// <param name="rowDimension">Размерность одного ряда в таблице.</param>
        public MemoryCell(uint rowDimension)
        {
            this.Memory = new Dictionary<string, MemoryObject>();
            this.ROW_DIMESION = rowDimension;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="length"></param>
        /// <param name="row"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool Add(string key, object obj, uint length, uint row, uint pos)
        {
            MemoryCoord coord = new MemoryCoord(ROW_DIMESION);

            // Создаем смещение по позиции.
            coord.Pos = pos;
            coord.Row = row;

            if (obj != null && !this.Memory.ContainsKey(key))
            {
                // Добавляем новый объект в таблицу.
                this.Memory.Add(key, new MemoryObject(obj, coord, length));
                // Сортируем объекты в таблице по позиции.
                this.Memory = this.Memory.OrderBy(t => 
                    t.Value.StartCoord.Row * t.Value.StartCoord.Dim + t.Value.StartCoord.Pos)
                    .ToDictionary(k => k.Key, v => v.Value);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Добавляет объект в распределение памяти в конец таблицы.
        /// </summary>
        /// <param name="obj">Объект помещаемый в таблицу.</param>
        /// <param name="length">Длина объекта.</param>
        /// <param name="newRow">Определяет нужно ли начинать распределение в таблице с нового ряда.</param>
        /// <param name="offsetPos">Смещение по позиции.</param>
        public void AddToEnd(string key, object obj, uint length, bool newRow, int offsetPos)
        {
            MemoryCoord coord = this.NextFreeCoord;

            // Добавляем новый ряд.
            if (newRow && coord.Pos > 0)
            {
                coord.Row++;
                coord.Pos = 0;
            }

            // Создаем смещение по позиции.
            coord.Pos = (uint)(coord.Pos + offsetPos);

            this.Add(key, obj, length, coord.Row, coord.Pos);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="length"></param>
        /// <param name="step"></param>
        public void AddToEnd(string key, object obj, uint length, int step)
        {
            MemoryCoord coord = this.NextFreeCoord;

            // Создаем смещение по позиции.
            coord.Pos = (uint)this.GetNextPosByStep((int)coord.Pos, step);

            this.Add(key, obj, length, coord.Row, coord.Pos);
        }

        /// <summary>
        /// Получает значение позиции которое кратно укзанному шагу и больше или равно текущей позиции.
        /// </summary>
        /// <param name="value">Текущее входное значение позиции относительно которого необходимо найти новое значение.</param>
        /// <param name="step">Шаг, кратному которму необходим поиск значения.</param>
        /// <returns></returns>
        public int GetNextPosByStep(int value, int step)
        {
            if (step <= 0)
            {
                return value;
            }

            int result = (value / step) * step;

            if (result == value)
            {
                return result;
            }
            else
            {
                return result + step;
            }
        }
        /// <summary>
        /// Получает значение позиции которое кратно укзанному шагу и меньше или равно текущей позиции.
        /// </summary>
        /// <param name="value">Текущее входное значение относительно которого необходимо найти новое значение.</param>
        /// <param name="step">Шаг, кратному которму находим значение.</param>
        /// <returns></returns>
        public int GetPrevPosByStep(int value, int step)
        {
            if (step <= 0)
            {
                return value;
            }

            return (value / step) * step;
        }
    }
}
