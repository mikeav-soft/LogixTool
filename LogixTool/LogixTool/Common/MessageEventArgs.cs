using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Common
{
    /// <summary>
    /// Тип пересылаемого сообщения.
    /// </summary>
    public enum MessageEventArgsType : byte { Success = 0x01, Info = 0x02, Warning = 0x04, Error = 0x08}
    /// <summary>
    /// Аргумент события характеризующий сообщение.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        public Object Owner { get; set; }
        /// <summary>
        /// Возвращает или задает время появления сообщения.
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// Возвращает или задает тип сообщения.
        /// </summary>
        public MessageEventArgsType Type { get; set; }
        /// <summary>
        /// Возвращает или задает заголовок пересылаемый в сообщении.
        /// </summary>
        public string Header { get; set; }
        /// <summary>
        /// Возвращает или задает текст пересылаемый в сообщении.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Создает аргумент события характеризующий сообщение.
        /// </summary>
        /// <param name="owner">Объект который сгенерировал данное сообщение.</param>
        /// <param name="time">Время появления сообщения.</param>
        /// <param name="type">Тип сообщения.</param>
        /// <param name="header">Заголовок сообщения.</param>
        /// <param name="text">Текст сообщения.</param>
        public MessageEventArgs (Object owner, DateTime time, MessageEventArgsType type, string header, string text)
        {
            this.Owner = owner;
            this.Time = time;
            this.Type = type;
            this.Header = header;
            this.Text = text;
        }
        /// <summary>
        /// Создает аргумент события характеризующий сообщение.
        /// </summary>
        /// <param name="type">Тип сообщения.</param>
        /// <param name="header">Заголовок сообщения.</param>
        /// <param name="text">Текст сообщения.</param>
        public MessageEventArgs(Object owner, MessageEventArgsType type, string header, string text)
        {
            this.Owner = owner;
            this.Time = DateTime.Now;
            this.Type = type;
            this.Header = header;
            this.Text = text;
        }
        ///// <summary>
        ///// Создает аргумент события характеризующий сообщение.
        ///// </summary>
        ///// <param name="type">Тип сообщения.</param>
        ///// <param name="header">Заголовок сообщения.</param>
        ///// <param name="text">Текст сообщения.</param>
        //public MessageEventArgs(MessageEventArgsType type, string header, string text)
        //{
        //    this.Owner = null;
        //    this.Time = DateTime.Now;
        //    this.Type = type;
        //    this.Header = header;
        //    this.Text = text;
        //}
    }
}
