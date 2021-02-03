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
    public class EncapsulatedPacket
    {
        #region [ PROPERTIES ]
        /* ============================================================================== */
        /// <summary>
        /// Комманда пакета.
        /// </summary>
        public EncapsulatedPacketCommand Command { get; set; }
        /// <summary>
        /// Значение длины которое было заявлено в принятом пакете.
        /// </summary>
        public UInt16 ResposeLength { get; set; }
        /// <summary>
        /// Длина текущего пакета.
        /// </summary>
        public UInt16 Length
        {
            get
            {
                return (UInt16)(CommandSpecificData.Count & 0xFFFF);
            }
        }
        /// <summary>
        /// Идентификатор сессии.
        /// </summary>
        public UInt32 SessionHandle { get; set; }
        /// <summary>
        /// Статус.
        /// </summary>
        public EncapsulatedPacketStatus Status { get; set; }
        /// <summary>
        /// Контекст пакета который возвращается обратно от сервера.
        /// </summary>
        public byte [] SenderContext { get; set; }
        /// <summary>
        /// Опции пакета.
        /// </summary>
        public UInt32 Options { get; set; }
        /// <summary>
        /// Массив байт представляющих собой комманду.
        /// </summary>
        public List<byte> CommandSpecificData { get; set; }
        /* ============================================================================== */
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public EncapsulatedPacket()
        {
            this.Command = EncapsulatedPacketCommand.NOP;
            this.SessionHandle = 0;
            this.Status = EncapsulatedPacketStatus.Success;
            this.SenderContext = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            this.Options = 0;
            this.CommandSpecificData = new List<byte>();
        }

        /// <summary>
        /// Разбирает последовательность байт в объект со значениями из данной последовательности.
        /// В случае неверной структуры, длины или ошибок возвращает значение null.
        /// </summary>
        /// <param name="bytes">Последовательность байт.</param>
        public static EncapsulatedPacket Parse(List<byte> bytes)
        {
            EncapsulatedPacket encapsulatedPacket = new EncapsulatedPacket();

            if (bytes == null)
            {
                return null;
            }

            if (bytes.Count < 24)
            {
                return null;
            }

            // Command.
            ushort commandValue = (ushort)(bytes[0] | bytes[1] << 8);
            foreach (EncapsulatedPacketCommand command in Enum.GetValues(typeof(EncapsulatedPacketCommand)))
            {
                if ((ushort)command == commandValue)
                {
                    encapsulatedPacket.Command = command;
                    break;
                }
            }
            // Length.
            encapsulatedPacket.ResposeLength = (ushort)(bytes[2] | bytes[3] << 8);
            // Handle returned by RegisterSession.
            encapsulatedPacket.SessionHandle = (uint)(bytes[4] | bytes[5] << 8 | bytes[6] << 16 | bytes[7] << 24);
            // Status.
            uint statusValue = (uint)(bytes[8] | bytes[9] << 8 | bytes[10] << 16 | bytes[11] << 24);

            encapsulatedPacket.Status = EncapsulatedPacketStatus.Unknown;
            foreach (EncapsulatedPacketStatus status in Enum.GetValues(typeof(EncapsulatedPacketStatus)))
            {
                if ((uint)status == statusValue)
                {
                    encapsulatedPacket.Status = status;
                    break;
                }
            }
            // Sender Context.
            encapsulatedPacket.SenderContext[0] = bytes[12];
            encapsulatedPacket.SenderContext[1] = bytes[13];
            encapsulatedPacket.SenderContext[2] = bytes[14];
            encapsulatedPacket.SenderContext[3] = bytes[15];
            encapsulatedPacket.SenderContext[4] = bytes[16];
            encapsulatedPacket.SenderContext[5] = bytes[17];
            encapsulatedPacket.SenderContext[6] = bytes[18];
            encapsulatedPacket.SenderContext[7] = bytes[19];
            // Options.
            encapsulatedPacket.Options = (uint)(bytes[20] | bytes[21] << 8 | bytes[22] << 16 | bytes[23] << 24);
            // Command specific data.
            encapsulatedPacket.CommandSpecificData.AddRange(bytes.GetRange(24, bytes.Count - 24));

            if (encapsulatedPacket.ResposeLength != encapsulatedPacket.Length)
            {
                //return null;
            }

            return encapsulatedPacket;
        }
        /// <summary>
        /// Преобразовывает данный объект в масив Байт.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            List<byte> result = new List<byte>();
            result.AddRange(BitConverter.GetBytes((ushort)this.Command));
            result.AddRange(BitConverter.GetBytes(this.Length));
            result.AddRange(BitConverter.GetBytes(this.SessionHandle));
            result.AddRange(BitConverter.GetBytes((uint)this.Status));
            result.AddRange(this.SenderContext);
            result.AddRange(BitConverter.GetBytes(this.Options));
            result.AddRange(this.CommandSpecificData);
            return result.ToArray();
        }

    }
}
