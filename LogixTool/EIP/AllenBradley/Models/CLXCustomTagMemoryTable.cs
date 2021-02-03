using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EIP.AllenBradley.Models
{
    /// <summary>
    /// Таблица ассоциаций привязки области памяти возвращаемой контроллером с тэгами контроллера.
    /// </summary>
    public class CLXCustomTagMemoryTable
    {
        #region [ PROPERTIES ]
        /* ======================================================================================== */
        /// <summary>
        /// Возвращает или задает текущий Instance присвоенный удаленным устройством данной таблице.
        /// </summary>
        public UInt16 Instance { get; set; }

        private Dictionary<UInt16, CLXCustomTagMemoryItem> _Items;
        /// <summary>
        /// Возвращает элементы привязки области памяти возвращаемой контроллером с тэгом контроллера.
        /// </summary>
        public List<CLXCustomTagMemoryItem> Items
        {
            get
            {
                return this._Items.Values.ToList();
            }
        }
        /// <summary>
        /// Возвращает расчетный размер в байтах который будет возвращен контроллером для данной таблицы.
        /// </summary>
        public int ExpectedTotalSize
        {
            get
            {
                return this.Items.Sum(i => i.ExpectedSizeOfResponse);
            }
        }
        /* ======================================================================================== */
        #endregion

        /// <summary>
        /// Создает новую таблицу ассоциаций привязки области памяти возвращаемой контроллером с тэгами контроллера.
        /// </summary>
        public CLXCustomTagMemoryTable(UInt16 instance)
        {
            this.Instance = instance;
            this._Items = new Dictionary<ushort, CLXCustomTagMemoryItem>();
        }

        #region [ PUBLIC METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Добавляет новую привязку тэга контроллера с его условным идентификатором в таблицу.
        /// </summary>
        /// <param name="id">Идентификатор тэга в таблице (идентификатор возвращается удаленным устройством).</param>
        /// <param name="tag">Контроллерный тэг.</param>
        /// <returns></returns>
        public bool Add(UInt16 id, LogixTag tag)
        {
            if (this._Items.ContainsKey(id))
            {
                return false;
            }

            CLXCustomTagMemoryItem newItem = new CLXCustomTagMemoryItem(id, tag);
            newItem.ParrentTable = this;
            tag.OwnerTableItem = newItem;

            this._Items.Add(id, newItem);
            return true;
        }
        /// <summary>
        /// Удаляет элемент привязки области памяти возвращаемой контроллером с тэгом контроллера.
        /// </summary>
        /// <param name="id">Контроллерный тэг.</param>
        /// <returns></returns>
        public bool Remove(UInt16 id)
        {
            if (!this._Items.ContainsKey(id))
            {
                return false;
            }

            this.Items[id].ParrentTable = null;
            this.Items[id].Tag.OwnerTableItem = null;
            this._Items.Remove(id);
            return true;
        }
        /// <summary>
        /// Очищает все элементы привязки области памяти возвращаемой контроллером с тэгом контроллера.
        /// </summary>
        public void Clear()
        {
            foreach(CLXCustomTagMemoryItem item in this._Items.Values)
            {
                item.ParrentTable = null;
                item.Tag.OwnerTableItem = null;
            }

            this.Items.Clear();
        }
        /// <summary>
        /// Устанавливает массив байт по соответствующим элементам таблицы ассоциации памяти и с тэгом.
        /// Тем самым устанавливает значения тэгов.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool SetBytesToItems(List<byte> bytes)
        {
            if (bytes == null || bytes.Count == 0)
            {
                return false;
            }

            // Накапливаемое смещение в байтах.
            int offset = 0; 

            // Для каждого элемента таблицы извлекаем данные.
            foreach (CLXCustomTagMemoryItem item in this.Items)
            {
                // Проверка на допустимость границы индекса.
                if (bytes.Count < offset + item.ExpectedSizeOfResponse)
                {
                    return false;
                }

                // Извлекаем данные и проверяем на успешность проведенной операции.
                if (!item.GetTagValueFromBytes(bytes.GetRange(offset, item.ExpectedSizeOfResponse)))
                {
                    return false;
                }

                // Увеличиваем индекс смещения на длину элемента.
                offset += item.ExpectedSizeOfResponse;
            }

            return true;
        }
        /* ================================================================================================== */
        #endregion
    }
}
