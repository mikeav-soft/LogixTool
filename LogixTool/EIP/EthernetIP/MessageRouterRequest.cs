using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EIP.EthernetIP
{
    /// <summary>
    /// CIP запрос.
    /// </summary>
	public class MessageRouterRequest
    {
        /// <summary>
        /// Номер запрашиваемого сервисного кода.
        /// </summary>
        public byte ServiceCode { get; set; }
        /// <summary>
        /// Запрашиваемый путь.
        /// </summary>
        public EPath RequestPath { get; set; }
        /// <summary>
        /// Передаваемые данные.
        /// </summary>
        public List<byte> RequestData { get; set; }

        /// <summary>
        /// 
        /// </summary>
		public MessageRouterRequest()
        {
            this.ServiceCode = 0;
            this.RequestPath = new EPath();
            this.RequestData = new List<byte>();
        }

        /// <summary>
        /// Преобразовывает данный объект в масив Байт.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            List<byte> list = new List<byte>();
            list.Add(this.ServiceCode);

            if (this.RequestPath != null)
            {
                list.AddRange(this.RequestPath.ToBytes(EPathToByteMethod.Complete));
            }

            if (this.RequestData != null)
            {
                list.AddRange(this.RequestData);
            }
            return list.ToArray();
        }
    }
}
