using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIP
{
    /// <summary>
    /// 
    /// </summary>
    public class UnconnectedSendRequest
    {
        #region [ PROPERTIES ]
        /* ============================================================================== */
        /// <summary>
        /// Используется при расчете выхода за пределы временного интервала запроса.
        /// </summary>
        public byte PriorityTimeTick { get; set; }
        /// <summary>
        /// Используется при расчете выхода за пределы временного интервала запроса.
        /// </summary>
        public byte TimeOutTicks { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public MessageRouterRequest MessageRequest { get; set; }
        /// <summary>
        /// Путь до запрашиваемого объекта.
        /// </summary>
        public EPath ConnectionPath { get; set; }
        /* ============================================================================== */
        #endregion


        /// <summary>
        /// 
        /// </summary>
        public UnconnectedSendRequest ()
        {
            this.PriorityTimeTick = 7;
            this.TimeOutTicks = 155;
            this.MessageRequest = new MessageRouterRequest();
            this.ConnectionPath = new EPath();
        }

        /// <summary>
        /// Преобразовывает данный объект в масив Байт.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            List<byte> result = new List<byte>();
            byte[] messageRequestBytes = this.MessageRequest.ToBytes();

            // PriorityTimeTick.
            result.Add(this.PriorityTimeTick);
            // TimeOutTicks.
            result.Add(this.TimeOutTicks);
            // Message Request Size.
            result.AddRange(BitConverter.GetBytes((UInt16)(messageRequestBytes.Length & 0xFFFF)));
            // Message Request.
            result.AddRange(messageRequestBytes);
            // Only present if Message Request Size is an odd value.
            if ((messageRequestBytes.Length & 0x01) == 0x01)
            {
                result.Add(0);
            }
            // The number of 16 bit words in the Connection_Path field.
            result.Add(this.ConnectionPath.Size);
            // Reserved.
            result.Add(0);
            // Indicates the route to the Remote Target Device.
            result.AddRange(this.ConnectionPath.ToBytes(EPathToByteMethod.DataOnly));

            return result.ToArray();
        }

    }
}
