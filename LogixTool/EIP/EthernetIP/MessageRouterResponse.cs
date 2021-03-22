using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EIP.EthernetIP
{
    /// <summary>
    /// CIP ответ.
    /// </summary>
	public class MessageRouterResponse
    {
        /// <summary>
        /// Возвращаемый сервисный код.
        /// </summary>
        public byte ReplyServiceCode { get; set; }
        /// <summary>
        /// Возвращает или задает основной статус.
        /// </summary>
        public byte GeneralStatus { get; set; }
        /// <summary>
        /// Возвращает текстовое описание основного статуса.
        /// </summary>
        public string GeneralStatusText
        {
            get
            {
                return RecognizeGeneralStatus(this.GeneralStatus);
            }
        }
        /// <summary>
        /// Размер массива со списком дополнительных статусов.
        /// </summary>
        public byte SizeOfAdditionalStatus
        {
            get
            {
                return (byte)AdditionalStatus.Count;
            }
        }
        /// <summary>
        /// Список дополнительных статусов.
        /// </summary>
        public List<ushort> AdditionalStatus { get; set; }
        /// <summary>
        /// Возвращаемые данные.
        /// </summary>
        public List<byte> ResponseData { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MessageRouterResponse()
        {
            this.ReplyServiceCode = 0;
            this.GeneralStatus = 0;
            this.AdditionalStatus = new List<ushort>();
            this.ResponseData = new List<byte>();
        }

        /// <summary>
        /// Разбирает последовательность байт в объект со значениями из данной последовательности.
        /// В случае неверной структуры, длины или ошибок возвращает значение null.
        /// </summary>
        /// <param name="bytes">Последовательность байт.</param>
        public static MessageRouterResponse Parse(List<byte> bytes)
        {
            MessageRouterResponse messageRouterResponse = new MessageRouterResponse();

            if (bytes == null)
            {
                return null;
            }

            messageRouterResponse.ReplyServiceCode = bytes[0];
            messageRouterResponse.GeneralStatus = bytes[2];
            int additionalStatusSize = bytes[3];
            int index = 4;

            for (int ix = 0; ix < additionalStatusSize; ix++)
            {
                index = ix * 2 + 4;
                if (index >= bytes.Count || additionalStatusSize * 2 > bytes.Count - 4)
                {
                    return null;
                }
                messageRouterResponse.AdditionalStatus.Add((ushort)(bytes[index] | bytes[index + 1] << 8));
            }

            if (additionalStatusSize > 0)
            {
                index += 2;
            }


            if (index < bytes.Count)
            {
                messageRouterResponse.ResponseData.AddRange(bytes.GetRange(index, bytes.Count - index));
            }

            return messageRouterResponse;
        }
        /// <summary>
        /// Преобразовывает код статуса из байта в текст.
        /// </summary>
        /// <param name="status">Код статуса.</param>
        /// <returns></returns>
        private string RecognizeGeneralStatus(byte status)
        {
            switch (status)
            {
                case 0x00: return "Success";
                case 0x01: return "Connection failure";
                case 0x02: return "Resource unavailable";
                case 0x03: return "Invalid parameter value";
                case 0x04: return "Path segment error";
                case 0x05: return "Path destination unknown";
                case 0x06: return "Partial transfer";
                case 0x07: return "Connection lost";
                case 0x08: return "Service not supported";
                case 0x09: return "Invalid attribute";
                case 0x0A: return "Attribute list error";
                case 0x0B: return "Already in requested mode/state";
                case 0x0C: return "Object state conflict";
                case 0x0D: return "Object already exists";
                case 0x0E: return "Attribute not settable";
                case 0x0F: return "Privilege violation";
                case 0x10: return "Device state conflict";
                case 0x11: return "Reply data too large";
                case 0x12: return "Fragmentation of a primitive value";
                case 0x13: return "Not enough data";
                case 0x14: return "Attribute not supported";
                case 0x15: return "Too much data";
                case 0x16: return "Object does not exist";
                case 0x17: return "Service fragmentation sequence not in progress";
                case 0x18: return "No stored attribute data";
                case 0x19: return "Store operation failure";
                case 0x1A: return "Routing failure, request packet too large";
                case 0x1B: return "Routing failure, response packet too large";
                case 0x1C: return "Missing attribute list entry data";
                case 0x1D: return "Invalid attribute value list";
                case 0x1E: return "Embedded service error";
                case 0x1F: return "Vendor specific error";
                case 0x20: return "Invalid parameter";
                case 0x21: return "Write-once value or medium already written";
                case 0x22: return "Invalid Reply Received";
                case 0x23: return "Reserved by CIP for future extensions";
                case 0x24: return "Reserved by CIP for future extensions";
                case 0x25: return "Key Failure in path";
                case 0x26: return "Path Size Invalid";
                case 0x27: return "Unexpected attribute in list";
                case 0x28: return "Invalid Member ID";
                case 0x29: return "Member not settable";
                case 0x2A: return "Group 2 only server general failure";

                default:
                    if (status >= 0x2B && status <= 0xCF)
                    {
                        return "Reserved by CIP for future extensions";
                    }
                    if (status >= 0xD0 && status <= 0xFF)
                    {
                        return "Reserved for Object Class and service errors";
                    }
                    break;
            }
            return "?";
        }
        /// <summary>
        /// Преобразовывает данный объект в масив Байт.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            List<byte> result = new List<byte>();
            // Reply service code.
            result.Add(this.ReplyServiceCode);
            // Reserve.
            result.Add(0);
            // General Status.
            result.Add(this.GeneralStatus);
            // Size of Additional Status.
            result.Add(this.SizeOfAdditionalStatus);
            // Additional Status.
            foreach (ushort additionalStatus in this.AdditionalStatus)
            {
                result.AddRange(BitConverter.GetBytes(additionalStatus));
            }
            // Response Data.
            result.AddRange(this.ResponseData);

            return result.ToArray();
        }
    }
}
