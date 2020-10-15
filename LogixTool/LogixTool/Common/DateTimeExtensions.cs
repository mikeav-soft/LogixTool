using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Common
{
    /// <summary>
    /// Расширения для DateTime.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Возвращает номер недели текущей даты.
        /// </summary>
        /// <param name="dt">Текущая дата.</param>
        /// <returns></returns>
        public static int GetWeekNumber (this DateTime dt)
        {
            DateTime firstDayOfYear = new DateTime(dt.Year, 1, 1);
            int offset = (int)firstDayOfYear.DayOfWeek - 1;
            int week = (dt.DayOfYear + offset) / 7;
            if (week * 7 < (dt.DayOfYear + offset)) week++;
            return week;
        }

    }
}
