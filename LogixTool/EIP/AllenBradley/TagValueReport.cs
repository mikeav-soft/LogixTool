using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EIP.AllenBradley
{
    public class TagValueReport
    {
        #region [ PROPERTIES ]
        /* ======================================================================================== */
        /// <summary>
        /// Возвращает информацию о том что операция проведена успешно.
        /// </summary>
        public bool? IsSuccessful { get; set; }
        /// <summary>
        /// Возвращает значение в тиках соответствующие последнему отправленному запросу.
        /// </summary>
        public long? ServerRequestTimeStamp { get; set; }
        /// <summary>
        /// Возвращает значение в тиках соответствующие последнему принятому ответу.
        /// </summary>
        public long? ServerResponseTimeStamp { get; set; }
        /// <summary>
        /// Возвращает текущее время между запросом и ответом на проведение операции (миллисекунд).
        /// </summary>
        public long? ServerReplyTime { get; set; }
        /// <summary>
        /// Возвращает период фактического интервала обновления значения тэга (милисекунд).
        /// </summary>
        public long? ActualUpdateRate { get; set; }
        /// <summary>
        /// Возвращает True в случае если текущее значение было изменено по сравнению с предыдущим.
        /// </summary>
        public bool? ValueChanged { get; set; }
        /// <summary>
        /// Текущее значение в виде двуразмерного массива, где главный массив представляет собой элементы массива,
        /// а внутренний массив представляет собой само значение в виде послеовательности байт.
        /// </summary>
        public List<byte[]> Data { get; set; }
        /* ======================================================================================== */
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public TagValueReport ()
        {
            Init();
        }

        #region [ PUBLIC METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Делает полную копию текущего объекта.
        /// </summary>
        /// <returns></returns>
        public TagValueReport Copy ()
        {
            // Создаем новый объект.
            TagValueReport result = new TagValueReport();

            // Производим прямое копирование значений.
            result.IsSuccessful = this.IsSuccessful;
            result.ServerRequestTimeStamp = this.ServerRequestTimeStamp;
            result.ServerResponseTimeStamp = this.ServerResponseTimeStamp;
            result.ServerReplyTime = this.ServerReplyTime;
            result.ActualUpdateRate = this.ActualUpdateRate;
            result.ValueChanged = this.ValueChanged;

            // Копируем данные тэга.
            if (this.Data!=null)
            {
                // Создаем копию текущих данных путем создания новых объектов.
                result.Data = this.Data.Select(elem => elem.ToArray()).ToList();
            }
            else
            {
                // Возвращаем Null.
                result.Data = null;
            }

            return result;
        }
        /// <summary>
        /// Производит преведение членов класса к значениям по умолчанию.
        /// </summary>
        public void Init()
        {
            this.IsSuccessful = null;
            this.ServerRequestTimeStamp = DateTime.Now.Ticks;
            this.ServerResponseTimeStamp = null;
            this.ServerReplyTime = null;
            this.ActualUpdateRate = null;
            this.ValueChanged = null;
            this.Data = null;
        }
        /* ======================================================================================== */
        #endregion
    }
}
