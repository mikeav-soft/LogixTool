using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LogixTool.EthernetIP
{
    /// <summary>
    /// Структура запроса на создание подключения с удаленным устройством.
    /// </summary>
	public class ForwardOpenRequest
    {
        #region [ PROPERTIES ]
        /* ============================================================================== */
        /// <summary>
        /// 
        /// </summary>
        public byte PriorityTimeTick { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte TimeOutTicks { get; set; }
        /// <summary>
        /// ID подключения Originator -> Target.
        /// </summary>
        public uint OtoTConnectionID { get; set; }
        /// <summary>
        /// ID подключения Target -> Originator.
        /// </summary>
        public uint TtoOConnectionID { get; set; }
        /// <summary>
        /// Серийный номер подключения.
        /// </summary>
        public ushort ConnectionSerialNumber { get; set; }
        /// <summary>
        /// Originator Vendor ID.
        /// </summary>
        public ushort OriginatorVendorID { get; set; }
        /// <summary>
        /// Серийный номер Originator-а.
        /// </summary>
        public uint OriginatorSerialNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte ConnectionTimeOutMultiplier { get; set; }
        /// <summary>
        /// Запрашиваемый период времени отправки пакетов (RPI) Originator -> Target.
        /// </summary>
        public uint OtoTRequestedPacketInterval { get; set; }
        /// <summary>
        /// Параметры сетевых подключений Originator -> Target.
        /// </summary>
        public NetworkConnectionParameter OtoTParameters { get; set; }
        /// <summary>
        /// Запрашиваемый период времени отправки пакетов (RPI) Target -> Originator.
        /// </summary>
        public uint TtoORequestedPacketInterval { get; set; }
        /// <summary>
        /// Параметры сетевых подключений Target -> Originator.
        /// </summary>
        public NetworkConnectionParameter TtoOParameters { get; set; }
        /// <summary>
        /// Возвращает или задает Тип транспорта и Триггер передачи данных.
        /// </summary>
        public TransportTypeAndTrigger TransportClassAndTrigger { get; set; }
        /// <summary>
        /// Путь до запрашиваемого объекта.
        /// </summary>
        public EPath ConnectionPath { get; set; }
        /* ============================================================================== */
        #endregion

        /// <summary>
        /// Создает структуру запроса на создание подключения с удаленным устройством.
        /// </summary>
        public ForwardOpenRequest()
        {
            this.PriorityTimeTick = 0;
            this.TimeOutTicks = 0;
            this.OtoTConnectionID = 0;
            this.TtoOConnectionID = 0;
            this.ConnectionSerialNumber = 0;
            this.OriginatorVendorID = 0;
            this.OriginatorSerialNumber = 0;
            this.ConnectionTimeOutMultiplier = 0;
            this.OtoTRequestedPacketInterval = 0;
            this.OtoTParameters = new NetworkConnectionParameter();
            this.TtoORequestedPacketInterval = 0;
            this.TtoOParameters = new NetworkConnectionParameter();
            this.TransportClassAndTrigger = new TransportTypeAndTrigger();
            this.ConnectionPath = new EPath();
        }

        /// <summary>
        /// Преобразовывает данный объект в масив Байт.
        /// </summary>
        /// <returns></returns>
		public byte[] ToBytes()
        {
            List<byte> result = new List<byte>();

            // PriorityTimeTick
            result.Add(this.PriorityTimeTick);
            // TimeOutTicks
            result.Add(this.TimeOutTicks);
            // Connection ID: O to T.
            result.AddRange(BitConverter.GetBytes(this.OtoTConnectionID));
            // Connection ID: T to O.
            result.AddRange(BitConverter.GetBytes(this.TtoOConnectionID));
            // Connection Serial Number.
            result.AddRange(BitConverter.GetBytes(this.ConnectionSerialNumber));
            // Originator Vendor ID.
            result.AddRange(BitConverter.GetBytes(this.OriginatorVendorID));
            // Originator Serial Number.
            result.AddRange(BitConverter.GetBytes(this.OriginatorSerialNumber));
            // Timeout Multiplier
            result.Add(this.ConnectionTimeOutMultiplier);
            // Reserved
            result.Add(0);
            // Reserved
            result.Add(0);
            // Reserved
            result.Add(0);
            // Requested Packet Rate O->T in Microseconds
            result.AddRange(BitConverter.GetBytes(this.OtoTRequestedPacketInterval));
            // O->T Network Connection Parameters
            result.AddRange(this.OtoTParameters.ToBytes());
            // Requested Packet Rate T->O in Microseconds
            result.AddRange(BitConverter.GetBytes(this.TtoORequestedPacketInterval));
            // T->O Network Connection Parameters
            result.AddRange(this.TtoOParameters.ToBytes());
            // Transport Type/Trigger
            result.AddRange(this.TransportClassAndTrigger.ToBytes());
            // Путь к подключаемому объекту.
            result.AddRange(this.ConnectionPath.ToBytes(EPathToByteMethod.Complete));

            return result.ToArray();
        }
    }
}
