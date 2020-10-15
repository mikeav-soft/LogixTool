using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool
{
    /// <summary>
    /// Именованый элемент с объектом.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CheckBoxItem<T>
    {
        /// <summary>
        /// Название текущего элемента.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Хранимый объект.
        /// </summary>
        public T Value { get; set; }
        
        /// <summary>
        /// Создает новый именованый элемент с объектом.
        /// </summary>
        public CheckBoxItem (string text, T value)
        {
            this.Text = text;
            this.Value = value;
        }

        /// <summary>
        /// Преобразовывает данный объект в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Text;
        }
    }
}
