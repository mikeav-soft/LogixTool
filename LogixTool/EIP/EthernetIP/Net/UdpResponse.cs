using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EIP.EthernetIP.Net
{
    /// <summary>
    /// 
    /// </summary>
    internal class UdpResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public IPEndPoint EndPoint { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] RecievedData { get; set; }
    }
}
