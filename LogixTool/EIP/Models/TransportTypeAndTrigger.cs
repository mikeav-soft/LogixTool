using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIP
{
    /// <summary>
    /// Класс описывающий Тип транспорта и Триггер передачи данных.
    /// </summary>
    public class TransportTypeAndTrigger
    {
        #region [ PROPERTIES ]
        /* ============================================================================== */
        /// <summary>
        /// Возвращает или задает свойство определяющее характер как Cервер при значении True.
        /// </summary>
        public bool AsServer { get; set; }
        /// <summary>
        /// Возвращает или задает Триггер передачи данных.
        /// </summary>
        public ProductionTrigger ProductionTrigger { get; set; }
        /// <summary>
        /// Возвращает или задает Класс передачи данных.
        /// </summary>
        public TransportClass TransportClass { get; set; }
        /* ============================================================================== */
        #endregion

        /// <summary>
        /// Создает новый Тип транспорта и Триггер передачи данных со значениями по умолчанию.
        /// </summary>
        public TransportTypeAndTrigger ()
        {
            this.AsServer = false;
            this.ProductionTrigger = ProductionTrigger.Cyclic;
            this.TransportClass = TransportClass.Class0;
        }
        /// <summary>
        /// Создает новый Тип транспорта и Триггер передачи данных по задаваемому значению Байта.
        /// </summary>
        /// <param name="value">Байт содержащий сгруппированные биты полностью содержащие значения данного класса.</param>
        public TransportTypeAndTrigger (byte value)
        {
            //X------- = 0= Client; 1= Server
            //-XXX---- = Production Trigger, 0 = Cyclic, 1 = CoS, 2 = Application Object
            //----XXXX = Transport class, 0 = Class 0, 1 = Class 1, 2 = Class 2, 3 = Class 3

            this.AsServer = (value & 0x80) != 0;

            byte productionTriggerValue = (byte)((value & 0x30) >> 4);
            foreach (ProductionTrigger trigger in Enum.GetValues(typeof(ProductionTrigger)))
            {
                if ((byte)trigger == productionTriggerValue)
                {
                    this.ProductionTrigger = trigger;
                    break;
                }
            }

            byte productionTransportClass = (byte)(value & 0x03);
            foreach (TransportClass transportClass in Enum.GetValues(typeof(TransportClass)))
            {
                if ((byte)transportClass == productionTransportClass)
                {
                    this.TransportClass = transportClass;
                    break;
                }
            }
        }

        /// <summary>
        /// Преобразовывает данный объект в масив Байт.
        /// </summary>
        /// <returns></returns>
        public byte [] ToBytes()
        {
            byte[] array = new byte[1];
            array[0] = 0;
            array[0] |= (byte)((byte)this.TransportClass & 0x03);
            array[0] |= (byte)(((byte)this.ProductionTrigger & 0x03) << 4);
            array[0] |= (byte)(this.AsServer ? 0x80 : 0x00);
            return array;
        }
    }
}
