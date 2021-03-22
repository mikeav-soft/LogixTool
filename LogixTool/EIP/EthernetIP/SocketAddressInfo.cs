using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIP.EthernetIP
{
    /// <summary>
    /// Socket Address (see section 2-6.3.2)
    /// </summary>
    public class SocketAddressInfo
    {
        public const int LENGTH_IN_BYTES = 16;
        /// <summary>
        /// 
        /// </summary>
        public Int16 Family { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public UInt16 Port { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public UInt32 Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] Zero { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SocketAddressInfo()
        {
            this.Zero = new byte[8];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        public SocketAddressInfo(List<byte> bytes)
            : this()
        {
            this.Family = (Int16)(bytes[1] | bytes[0] << 8);
            this.Port = (UInt16)(bytes[3] | bytes[2] << 8);
            this.Address = (UInt32)(bytes[7] | bytes[6] << 8 | bytes[5] << 16 | bytes[4] << 24);
            this.Zero[0] = bytes[8];
            this.Zero[1] = bytes[9];
            this.Zero[2] = bytes[10];
            this.Zero[3] = bytes[11];
            this.Zero[4] = bytes[12];
            this.Zero[5] = bytes[13];
            this.Zero[6] = bytes[14];
            this.Zero[7] = bytes[15];
        }

        /// <summary>
        /// Преобразовывает данный объект в масив Байт.
        /// </summary>
        /// <returns></returns>
        public byte [] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(this.Family));
            bytes.AddRange(BitConverter.GetBytes(this.Port));
            bytes.AddRange(BitConverter.GetBytes(this.Address));
            bytes.AddRange(this.Zero);
            return bytes.ToArray();
        }
    }
}
