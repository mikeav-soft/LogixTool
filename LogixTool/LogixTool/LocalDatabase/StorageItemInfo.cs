using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LogixTool.LocalDatabase
{
    /// <summary>
    /// Класс представляющий собой запись в хранилище.
    /// </summary>
    public class StorageItemInfo
    {
        /// <summary>
        /// Владелец данных.
        /// </summary>
        public StoreOwner Owner { get; set; }
        /// <summary>
        /// Тип данных.
        /// </summary>
        public StoreType Type { get; set; }
        /// <summary>
        /// Строковый идентификатор элемента в базе данных.
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// Дата сохранения.
        /// </summary>
        public DateTime? Saved { get; set; }
        /// <summary>
        /// Возвращает значение что операция чтения была успешной.
        /// </summary>
        public bool? IsSuccessful { get; set; }
        /// <summary>
        /// Содержание в виде *.xml данных.
        /// </summary>
        public XElement XContent { get; set; }

        /// <summary>
        /// Создет элемент записи хранилища.
        /// </summary>
        /// <param name="id">Строковый идентификатор элемента в базе данных.</param>
        /// <param name="saved">Дата сохраненной информации в базу данных.</param>
        public StorageItemInfo(StoreOwner owner, StoreType type, string id, DateTime? saved, bool? isSuccessful, XElement xContent)
        {
            this.Owner = owner;
            this.Type = type;
            this.ID = id;
            this.Saved = saved;
            this.IsSuccessful = isSuccessful;
            this.XContent = xContent;
        }

        /// <summary>
        /// Преобразовывает текущий объект в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.Saved.HasValue)
            {
                return ((DateTime)this.Saved).ToString("dd.MM.yyyy HH:mm:ss");
            }
            else
            {
                return "??.??.???? ??:??:??";
            }
        }
    }
}
