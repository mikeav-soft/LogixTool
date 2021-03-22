using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EIP.EthernetIP.Net
{
    internal class ClientReplyEventArgs : EventArgs
    {
        /// <summary>
        /// Возвращает или задает конечного отправителя данных по UDP протоколу.
        /// </summary>
        public IPEndPoint EndPoint { get; set; }
        /// <summary>
        /// Возвращает или задает 
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="data"></param>
        public ClientReplyEventArgs (IPEndPoint ep, byte [] data)
        {
            this.EndPoint = ep;
            this.Data = data;
        }
    }
}
