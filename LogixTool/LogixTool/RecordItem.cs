using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool
{
    public class RecordItem
    {
        /// <summary>
        /// Возвращает или задает временную отметку записи.
        /// </summary>
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// Возвращает или задает значение записи.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Создает новую запись.
        /// </summary>
        public RecordItem(DateTime timeStamp, string value)
        {
            this.TimeStamp = timeStamp;
            this.Value = value;
        }

        /// <summary>
        /// Преобразовывает ткущую запись в строку.
        /// </summary>
        /// <param name="tickTimeFormat">Формат временного значения. При значении True возвращается Ticks.</param>
        /// <param name="separator">Символ - разделитель между записями.</param>
        /// <returns></returns>
        public string ToString(bool tickTimeFormat, char separator)
        {
            string dateTimeStamp;

            if (tickTimeFormat)
            {
                dateTimeStamp = this.TimeStamp.Ticks.ToString();
            }
            else
            {
                dateTimeStamp = "[" + this.TimeStamp.Day.ToString("00") + "." + this.TimeStamp.Month.ToString("00") + "." + this.TimeStamp.Year.ToString("0000") + "] " +
                    this.TimeStamp.Hour.ToString("00") + ":" + this.TimeStamp.Minute.ToString("00") + ":" + this.TimeStamp.Second.ToString("00") + ":" + this.TimeStamp.Millisecond.ToString("000");
            }

            return dateTimeStamp + separator + this.Value;
        }
    }
}
