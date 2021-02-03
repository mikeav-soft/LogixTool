using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIP
{
    /// <summary>
    /// Table 2-4.4 CIP Identity Item
    /// </summary>
    public class ListIdentityResponse
    {
        /// <summary>
        /// Версия протокола.
        /// </summary>
        public UInt16 EncapsulationProtocolVersion { get; set; }
        /// <summary>
        /// Информация о сокете.
        /// </summary>
        public SocketAddressInfo SocketAddress { get; set; }
        /// <summary>
        /// Одентификационный номер производителя.
        /// </summary>
        public UInt16 VendorID { get; set; }
        /// <summary>
        /// Тип устройства.
        /// </summary>
        public UInt16 DeviceType { get; set; }
        /// <summary>
        /// Код продукта.
        /// </summary>
        public UInt16 ProductCode { get; set; }
        /// <summary>
        /// Ревизия устройства.
        /// </summary>
        public byte[] Revision { get; set; }
        /// <summary>
        /// Статус устройства.
        /// </summary>
        public UInt16 Status;
        /// <summary>
        /// Серийный номр устройства.
        /// </summary>
        public UInt32 SerialNumber;
        /// <summary>
        /// Длина строки названия продукта.
        /// </summary>
        public byte ProductNameLength;
        /// <summary>
        /// Название продукта.
        /// </summary>
        public string ProductName;
        /// <summary>
        /// Состояние устройства.
        /// </summary>
        public ListIdentityState State;

        /// <summary>
        /// 
        /// </summary>
        public ListIdentityResponse()
        {
            this.SocketAddress = new SocketAddressInfo();
            this.Revision = new byte[2];
            this.State = ListIdentityState.Nonexistent;
        }

        /// <summary>
        /// Разбирает последовательность байт в объект со значениями из данной последовательности.
        /// В случае неверной структуры, длины или ошибок возвращает значение null.
        /// </summary>
        /// <param name="bytes">Последовательность байт.</param>
        public static ListIdentityResponse Parse(List<byte> bytes)
        {
            ListIdentityResponse listIdentityResponse = new ListIdentityResponse();

            if (bytes == null)
            {
                return null;
            }

            if (bytes.Count < 33)
            {
                return null;
            }

            listIdentityResponse.EncapsulationProtocolVersion = (UInt16)(bytes[0] | bytes[1] << 8);
            listIdentityResponse.SocketAddress = new SocketAddressInfo(bytes.GetRange(2, 16));
            listIdentityResponse.VendorID = (UInt16)(bytes[18] | bytes[19] << 8);
            listIdentityResponse.DeviceType = (UInt16)(bytes[20] | bytes[21] << 8);
            listIdentityResponse.ProductCode = (UInt16)(bytes[22] | bytes[23] << 8);
            listIdentityResponse.Revision[0] = bytes[24];
            listIdentityResponse.Revision[1] = bytes[25];
            listIdentityResponse.Status = (UInt16)(bytes[26] | bytes[27] << 8);
            listIdentityResponse.SerialNumber = (UInt32)(bytes[28] | bytes[29] << 8 | bytes[30] << 16 | bytes[31] << 24);
            listIdentityResponse.ProductNameLength = bytes[32];

            if (bytes.Count < 33 + listIdentityResponse.ProductNameLength + 1)
            {
                return null;
            }

            listIdentityResponse.ProductName = new string(bytes.GetRange(33, listIdentityResponse.ProductNameLength).Select(t => (Char)t).ToArray());
            byte stateValue = bytes[33 + listIdentityResponse.ProductNameLength];

            foreach (ListIdentityState state in Enum.GetValues(typeof(ListIdentityState)))
            {
                if ((byte)state == stateValue)
                {
                    listIdentityResponse.State = state;
                    break;
                }
            }

            return listIdentityResponse;
        }
    }
}
