using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LogixTool.EthernetIP
{
    /// <summary>
    /// Структура ответа на закрытие подключения с удаленным устройством.
    /// </summary>
	public class ForwardCloseResponse
    {
        #region [ PROPERTIES ]
        /* ============================================================================== */
        /// <summary>
        /// Показывает был ли ответ успешным.
        /// </summary>
        public bool? IsSuccessful { get; set; }
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
        /// Специальные данны от приложения.
        /// </summary>
        public List<byte> ApplicationReply { get; set; }
        /* ============================================================================== */
        #endregion

        /// <summary>
        /// Создает структуру ответа на закрытие подключения с удаленным устройством.
        /// Используется для инициализации объекта.
        /// </summary>
        public ForwardCloseResponse()
        {
            this.IsSuccessful = null;
            this.ConnectionSerialNumber = 0;
            this.OriginatorVendorID = 0;
            this.OriginatorSerialNumber = 0;
            this.ApplicationReply = new List<byte>();
        }

        /// <summary>
        /// Разбирает последовательность байт в объект со значениями из данной последовательности.
        /// В случае неверной структуры, длины или ошибок возвращает значение null.
        /// </summary>
        /// <param name="responce">Ответ от удаленного сервера.</param>
        public static ForwardCloseResponse Parse(MessageRouterResponse responce)
        {
            ForwardCloseResponse forwardCloseResponse = new ForwardCloseResponse();
            if (responce == null || responce == null)
            {
                return null;
            }

            List<byte> bytes = responce.ResponseData;
            forwardCloseResponse.IsSuccessful = false;

            if (bytes.Count >= 9)
            {
                forwardCloseResponse.ConnectionSerialNumber = (ushort)(bytes[0] | bytes[1] << 8);
                forwardCloseResponse.OriginatorVendorID = (ushort)(bytes[2] | bytes[3] << 8);
                forwardCloseResponse.OriginatorSerialNumber = (uint)(bytes[4] | bytes[5] << 8 | bytes[6] << 16 | bytes[7] << 24);
                int applicationReplySize = bytes[8];

                if (responce.GeneralStatus == 0)
                {
                    for (int ix = 0; ix < applicationReplySize; ix++)
                    {
                        int index = ix * 2 + 10;
                        if (index >= bytes.Count || applicationReplySize * 2 != bytes.Count - 10)
                        {
                            return null;
                        }
                        forwardCloseResponse.ApplicationReply.Add(bytes[index]);
                        forwardCloseResponse.ApplicationReply.Add(bytes[index + 1]);
                    }

                    forwardCloseResponse.IsSuccessful = true;
                }
            }
            else
            {
                return null;
            }

            return forwardCloseResponse;
        }

        /// <summary>
        /// Преобразовывает данный объект в масив Байт.
        /// </summary>
        /// <returns></returns>
		public byte[] ToBytes()
        {
            List<byte> result = new List<byte>();

            // Connection Serial Number.
            result.AddRange(BitConverter.GetBytes(this.ConnectionSerialNumber));
            // Originator Vendor ID.
            result.AddRange(BitConverter.GetBytes(this.OriginatorVendorID));
            // Originator Serial Number.
            result.AddRange(BitConverter.GetBytes(this.OriginatorSerialNumber));
            // Number of 16 bit words in the Application Reply field.
            int appReplyCount = ApplicationReply.Count;
            if ((appReplyCount & 0x01) == 1)
            {
                appReplyCount++;
            }
            result.Add((byte)(appReplyCount / 2));
            // Reserved
            result.Add(0);
            // Application specific data
            foreach (byte appReply in this.ApplicationReply)
            {
                result.Add(appReply);
            }
            return result.ToArray();
        }
    }
}
