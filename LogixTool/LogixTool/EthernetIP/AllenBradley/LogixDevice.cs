using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net;
using LogixTool.EthernetIP;
using LogixTool.Common;
using LogixTool.Common.Extension;
using LogixTool.EthernetIP.AllenBradley.Models;

namespace LogixTool.EthernetIP.AllenBradley
{
    /// <summary>
    /// Представляет собой класс способный управлять, читать и записывать данные в контроллер типа ControlLogix и CompactLogix.
    /// </summary>
    public class LogixDevice
    {
        private const int ENCAPSULATED_PACKET_ITEM_REQUEST_HEADER_SIZE = 4;
        private const int ENCAPSULATED_PACKET_ITEM_RESPONSE_HEADER_SIZE = 2;

        #region [ PROPERTIES ]
        /* ================================================================================================== */
        private string _Name;
        /// <summary>
        /// Задает имя контроллера.
        /// Используется для именования в дальнейшем использовании.
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
                Event_PropertyWasChanged();
            }
        }
        /// <summary>
        /// IP адрес контроллера.
        /// </summary>
        public IPAddress Address
        {
            get
            {
                return this.eipClient.IPAddress;
            }
            set
            {
                this.eipClient.IPAddress = value;
                Event_PropertyWasChanged();
            }
        }
        /// <summary>
        /// Возвращает номер слота контроллера в рейке Backplane к которому производится подключение.
        /// </summary>
        public byte ProcessorSlot
        {
            get
            {
                return this.eipClient.ProcessorSlot;
            }
            set
            {
                this.eipClient.ProcessorSlot = value;
                Event_PropertyWasChanged();
            }

        }
        /// <summary>
        /// Возвращает True в случае установления подключения с удаленным сервером.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return (eipClient != null && eipClient.IsConnected);
            }
        }
        /// <summary>
        /// Возвращает макимальный размер пакета O -> T.
        /// </summary>
        public int MaxPacketSizeOtoT
        {
            get
            {
                if (eipClient != null)
                {
                    return eipClient.CurrentForwardOpen.OtoTParameters.ConnectionSize;
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// Возвращает макимальный размер пакета T -> O.
        /// </summary>
        public int MaxPacketSizeTtoO
        {
            get
            {
                if (eipClient != null)
                {
                    return eipClient.CurrentForwardOpen.TtoOParameters.ConnectionSize;
                }
                else
                {
                    return 0;
                }
            }
        }
        /* ================================================================================================== */
        #endregion

        private EIPClient eipClient;                    // Платформа для работы с EthernetIP.

        #region [ CONSTRUCTOR ]
        /* ================================================================================================== */
        /// <summary>
        /// Создает новый объект для работы с контроллерами Allen-Breadley на основании имени, IP адреса и номера слота в рейке.
        /// </summary>
        /// <param name="name">Имя контроллера.</param>
        /// <param name="address">IP адрес в сети Ethernet.</param>
        /// <param name="processorSlot">Номер слота в рейке Backplane.</param>
        public LogixDevice(string name, IPAddress address, byte processorSlot)
        {
            this.Name = name;
            this.eipClient = new EIPClient(address, processorSlot);
            this.Address = address;
            this.ProcessorSlot = processorSlot;
        }
        /// <summary>
        /// Создает новый объект для работы с контроллерами Allen-Breadley с заданными параметрами по умолчанию.
        /// </summary>
        public LogixDevice()
            : this("", new IPAddress(new byte[] { 0, 0, 0, 0 }), 0)
        {
        }
        /* ================================================================================================== */
        #endregion

        #region [ EVENTS ]
        /* ================================================================================================== */

        /* 1. События */

        /// <summary>
        /// Возникает при изменении одного из свойств.
        /// </summary>
        public event EventHandler PropertyWasChanged;
        /// <summary>
        /// Возникает при появлении каких либо сообщений при работе ядра.
        /// </summary>
        public static event MessageEvent Messages;
        /// <summary>
        /// Возникает при появлении каких либо сообщений при работе ядра.
        /// </summary>
        public event MessageEvent Message;

        /* 2. Методы генерации события */

        /// <summary>
        /// Вызывает "Событие при изменении одного из свойств". 
        /// </summary>
        private void Event_PropertyWasChanged()
        {
            if (PropertyWasChanged != null)
            {
                PropertyWasChanged(this, null);
            }
        }

        /// <summary>
        /// Вызывает "Событие с сообщением".
        /// </summary>
        /// <param name="e"></param>
        private void Event_Message(MessageEventArgs e)
        {
            MessageEventArgs messageEventArgs = e;
            string messageHeader = "[" + this.Name + "]." + messageEventArgs.Header;
            messageEventArgs.Header = messageHeader;

            if (this.Message != null)
            {
                this.Message(this, messageEventArgs);
            }

            if (Messages != null)
            {
                Messages(this, messageEventArgs);
            }
        }
        /* ================================================================================================== */
        #endregion

        #region [ PUBLIC METHODS ]
        /* ======================================================================================== */

        #region [ 1. СОЗДАНИЕ ПОДКЛЮЧЕНИЯ С УДАЛЕННЫМ УСТРОЙСТВОМ ]
        /* ======================================================================================== */
        /// <summary>
        /// Производит подключение с удаленным устройством.
        /// </summary>
        public bool Connect()
        {
            string messageEventHeaderText = "[Method='TCPConnect']";

            if (this.eipClient.IsConnected)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "Refused. Already connected."));
                return true;
            }

            // Connection to server.
            if (this.eipClient.Connect())
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                return true;
            }
            else
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed."));
                return false;
            }
        }
        /// <summary>
        /// Производит отключение от удаленного устройства.
        /// </summary>
        public bool Disconnect()
        {
            string messageEventHeaderText = "[Method='TCPDisconnect']";

            if (!this.eipClient.IsConnected)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "Refused. Already disconnected."));
                return true;
            }

            // Connection to server.
            if (this.eipClient.Disconnect())
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                return true;
            }
            else
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed."));
                return false;
            }
        }
        /// <summary>
        /// Регистрирует сессию с удаленным устройством.
        /// </summary>
        /// <returns></returns>
        public bool RegisterSession()
        {
            string messageEventHeaderText = "[Method='RegisterSession']";

            EncapsulatedPacket encapsulatedPacket;
            List<object> objects = this.eipClient.RegisterSession();

            encapsulatedPacket = this.GetObject<EncapsulatedPacket>(objects);
            if (encapsulatedPacket == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Imposible to recognize EncapsulatedPacket."));
                return false;
            }

            if (encapsulatedPacket.Status != 0)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. EncapsulatedPacket.Status=" + encapsulatedPacket.Status.ToString()));
                return false;
            }

            UInt32 sessionHandle = this.GetObject<UInt32>(objects);
            if (sessionHandle == 0)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "Failed."));
                return false;
            }
            else
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK. SessionHandle=" + sessionHandle.ToString()));
                return true;
            }
        }
        /// <summary>
        /// Закрывает сессию с удаленным устройством.
        /// </summary>
        /// <returns></returns>
        public bool UnregisterSession()
        {
            string messageEventHeaderText = "[Method='UnregisterSession']";

            if (this.eipClient.UnRegisterSession())
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                return true;
            }
            else
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed."));
                return false;
            }
        }
        /// <summary>
        /// Открывает подключение с удаленным устройством.
        /// </summary>
        /// <returns></returns>
        public bool ForwardOpen()
        {
            string messageEventHeaderText = "[Method='ForwardOpen']";

            List<object> objects = this.eipClient.ForwardOpen();
            EncapsulatedPacket encapsulatedPacket;
            MessageRouterResponse messageRouterResponse;
            ForwardOpenResponse forwardOpenResponse;

            encapsulatedPacket = this.GetObject<EncapsulatedPacket>(objects);
            if (encapsulatedPacket == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Imposible to recognize EncapsulatedPacket."));
                return false;
            }

            if (encapsulatedPacket.Status != 0)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. EncapsulatedPacket.Status=" + encapsulatedPacket.Status.ToString()));
                return false;
            }

            messageRouterResponse = this.GetObject<MessageRouterResponse>(objects);
            if (messageRouterResponse == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Imposible to recognize MessageRouterResponse."));
                return false;
            }

            if (messageRouterResponse.GeneralStatus != 0)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. MessageRouterResponse.GeneralStatus=" + messageRouterResponse.GeneralStatus.ToString() + " (" + messageRouterResponse.GeneralStatusText + ")"));
                return false;
            }

            forwardOpenResponse = this.GetObject<ForwardOpenResponse>(objects);
            if (forwardOpenResponse == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Imposible to recognize ForwardOpenResponse."));
                return false;
            }

            if (forwardOpenResponse.IsSuccessful != true)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed."));
                return false;
            }
            else
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                return true;
            }
        }
        /// <summary>
        /// Закрывает подключение с удаленным устройством.
        /// </summary>
        /// <returns></returns>
        public bool ForwardClose()
        {
            string messageEventHeaderText = "[Method='ForwardClose']";

            List<object> objects = this.eipClient.ForwardClose();
            EncapsulatedPacket encapsulatedPacket;
            MessageRouterResponse messageRouterResponse;
            ForwardCloseResponse forwardCloseResponse;

            encapsulatedPacket = this.GetObject<EncapsulatedPacket>(objects);
            if (encapsulatedPacket == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Imposible to recognize EncapsulatedPacket."));
                return false;
            }

            if (encapsulatedPacket.Status != 0)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. EncapsulatedPacket.Status=" + encapsulatedPacket.Status.ToString()));
                return false;
            }

            messageRouterResponse = this.GetObject<MessageRouterResponse>(objects);
            if (messageRouterResponse == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Imposible to recognize MessageRouterResponse."));
                return false;
            }

            if (messageRouterResponse.GeneralStatus != 0)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. MessageRouterResponse.GeneralStatus=" + messageRouterResponse.GeneralStatus.ToString() + " (" + messageRouterResponse.GeneralStatusText + ")"));
                return false;
            }

            forwardCloseResponse = this.GetObject<ForwardCloseResponse>(objects);
            if (forwardCloseResponse == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Imposible to recognize ForwardOpenResponse."));
                return false;
            }

            if (forwardCloseResponse.IsSuccessful != true)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed."));
                return false;
            }
            else
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                return true;
            }
        }
        /* ======================================================================================== */
        #endregion

        #region [ 2. ПОЛУЧЕНИЕ СПИСКА ТЭГОВ И ТИПОВ ДАННЫХ ИЗ УДАЛЕННОГО УСТРОЙСТВА ]
        /* ======================================================================================== */
        /// <summary>
        /// Получает список глобальных контроллерных тэгов или локальных тэгов программы удаленного устройства (контроллера).
        /// Service Code: "0x55"
        /// Class: "0x6B"
        /// </summary>
        /// <param name="programName">Имя программы для чтения из нее списка тэгов.
        /// При равенстве Null будут получены глобальные контроллерные тэги.</param>
        /// <param name="clxTags">Результат: Список полученных тэгов.</param>
        /// <returns></returns>
        public bool GetTagsAddreses(string programName, out List<CLXTag> clxTags)
        {
            string messageEventHeaderText = "[Method='GetTagsAddreses']";

            clxTags = null;
            Dictionary<uint, CLXTag> result = new Dictionary<uint, CLXTag>();
            uint offsetInstance = 0;

            while (true)
            {
                //fragment = new List<CLXTag>();
                MessageRouterRequest request = new MessageRouterRequest();
                request.ServiceCode = 0x55;
                if (programName != null)
                {
                    request.RequestPath.Segments.Add(new EPathSegment(programName));
                }
                request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 0x6B));
                request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, offsetInstance));
                request.RequestData.AddRange(new byte[] { 0x03, 0x00 }); //Number of attributes to retrieve
                request.RequestData.AddRange(new byte[] { 0x01, 0x00 }); //Attribute 1 – Symbol Name
                request.RequestData.AddRange(new byte[] { 0x02, 0x00 }); //Attribute 2 – Symbol Type
                request.RequestData.AddRange(new byte[] { 0x08, 0x00 }); //Attribute 7 - Array Dimension?

                List<object> responsedObjects = eipClient.SendUnitData(request);


                MessageRouterResponse response;
                // Проверяем ответ на успешность принятых данных.
                if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
                {
                    return false;
                }

                // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
                if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0, 6 }, null, null))
                {
                    return false;
                }

                List<byte> remainBytes = response.ResponseData.ToList();

                do
                {
                    CLXTag tag;
                    if (!ParseTagInformation(messageEventHeaderText, programName, remainBytes, out tag, out remainBytes))
                    {
                        return false;
                    }

                    if (!result.ContainsKey(tag.Instance))
                    {
                        result.Add(tag.Instance, tag);
                    }

                    offsetInstance = tag.Instance;
                }
                while (remainBytes.Count > 0);

                if (response.GeneralStatus == 0)
                {
                    break;
                }
            }

            clxTags = result.Values.ToList();

            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
            return true;
        }
        /// <summary>
        /// Получает список глобальных контроллерных тэгов удаленного устройства (контроллера).
        /// Service Code: "0x55"
        /// Class: "0x6B"
        /// </summary>
        /// <param name="clxTags">Результат: Список полученных тэгов.</param>
        /// <returns></returns>
        public bool GetTagsAddreses(out List<CLXTag> clxTags)
        {
            return this.GetTagsAddreses(null, out clxTags);
        }
        /// <summary>
        /// Получает список структур контроллера.
        /// Service Code: "0x4B"
        /// Class: "0x6C"
        /// </summary>
        /// <param name="typeCodes"></param>
        /// <returns></returns>
        public bool GetTemplateAddreses(out List<UInt16> typeCodes)
        {
            string messageEventHeaderText = "[Method='GetTemplateAddreses']";

            typeCodes = null;
            Dictionary<UInt16, UInt16> result = new Dictionary<ushort, ushort>();
            uint offsetInstance = 0;

            while (true)
            {
                MessageRouterRequest request = new MessageRouterRequest();
                request.ServiceCode = 0x4B;
                request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 0x6C));
                request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, offsetInstance));

                List<object> responsedObjects = eipClient.SendUnitData(request);

                MessageRouterResponse response;
                // Проверяем ответ на успешность принятых данных.
                if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
                {
                    return false;
                }

                // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
                if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0, 6 }, 4, null))
                {
                    return false;
                }

                List<UInt16> recievedTemplateInstances;
                if (ParseTemplateListResponse(messageEventHeaderText, response.ResponseData, out recievedTemplateInstances))
                {
                    // Добавляем полученные номера типов данных которые не существуют в словаре.
                    foreach (UInt16 templateInstance in recievedTemplateInstances)
                    {
                        if (!result.ContainsKey(templateInstance))
                        {
                            result.Add(templateInstance, templateInstance);
                        }
                    }

                    // В случае частичного приема данных (Status = 6) устанавливаем смещение намера типа данных для следующего запроса.
                    if (response.GeneralStatus == 6)
                    {
                        offsetInstance = recievedTemplateInstances.Last();
                    }

                    // В случае полного приема данных (Status = 0) выходим из цикла запросов.
                    if (response.GeneralStatus == 0)
                    {
                        break;
                    }
                }
                else
                {
                    return false;
                }
            }

            typeCodes = result.Keys.ToList();

            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
            return true;
        }
        /// <summary>
        /// Получает объект типа данных только с основной ифнормацией о типе данных из удаленного устройства (контроллера).
        /// Service Code: "0x03"
        /// Class: "0x6C"
        /// </summary>
        /// <param name="templateInstance">Код типа данных.</param>
        /// <returns></returns>
        public bool GetTemplateInformation(UInt16 templateInstance, out CLXTemplate template)
        {
            string messageEventHeaderText = "[Method='GetTemplateInformation'; TemplateInstance=" + templateInstance.ToString() + "]";

            template = null;

            // Подготавливаем пакет для отправки данных.
            MessageRouterRequest request = new MessageRouterRequest();
            request.ServiceCode = 0x03;
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 0x6C));
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, templateInstance));
            request.RequestData.AddRange(new byte[] { 0x04, 0x00 }); // Number of attributes to retrieve
            request.RequestData.AddRange(new byte[] { 0x04, 0x00 }); // Attribute 4 - Template Object Definition Size
            request.RequestData.AddRange(new byte[] { 0x05, 0x00 }); // Attribute 5 – Template Structure Size
            request.RequestData.AddRange(new byte[] { 0x02, 0x00 }); // Attribute 2 – Member Count
            request.RequestData.AddRange(new byte[] { 0x01, 0x00 }); // Attribute 1 – Structure Handle CRC

            List<object> responsedObjects = eipClient.SendUnitData(request);

            MessageRouterResponse response;
            // Проверяем ответ на успешность принятых данных.
            if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
            {
                return false;
            }

            // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
            if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0 }, 30, 30))
            {
                return false;
            }

            List<byte> remainBytes = response.ResponseData.ToList();
            List<byte> attributeValue = null;

            UInt32 templateSize = 0;            // Template Structure Size
            UInt32 templateBytes = 0;           // Template Object Definition Size
            UInt16 templateMemberCount = 0;     // Template Member Count
            UInt16 templateCRC = 0;             // Structure Handle CRC

            UInt16 attributeCount = BitConverter.ToUInt16(remainBytes.ToArray(), 0);
            remainBytes = remainBytes.GetRange(2, remainBytes.Count - 2);

            // Разибраем принятые байты. Проверяем что число возвращаемых атрибутов равно 4-м.
            if (attributeCount == 4)
            {
                // Получаем Attribute 4: "Template Structure Size".
                if (!ParseTemplateAttributeResponse(messageEventHeaderText, 4, 4, remainBytes, out attributeValue, out remainBytes))
                {
                    return false;
                }
                templateSize = BitConverter.ToUInt32(attributeValue.ToArray(), 0);

                // Получаем Attribute 5: "Template Object Definition Size".
                if (!ParseTemplateAttributeResponse(messageEventHeaderText, 5, 4, remainBytes, out attributeValue, out remainBytes))
                {
                    return false;
                }
                templateBytes = BitConverter.ToUInt32(attributeValue.ToArray(), 0);

                // Получаем Attribute 2: "Template Member Count".
                if (!ParseTemplateAttributeResponse(messageEventHeaderText, 2, 2, remainBytes, out attributeValue, out remainBytes))
                {
                    return false;
                }
                templateMemberCount = BitConverter.ToUInt16(attributeValue.ToArray(), 0);

                // Получаем Attribute 1: "Structure Handle CRC".
                if (!ParseTemplateAttributeResponse(messageEventHeaderText, 1, 2, remainBytes, out attributeValue, out remainBytes))
                {
                    return false;
                }
                templateCRC = BitConverter.ToUInt16(attributeValue.ToArray(), 0);

                // Создаем возвращаемый объект.
                template = new CLXTemplate(null, templateInstance, templateSize, templateBytes, templateMemberCount, templateCRC);

                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                return true;
            }
            else
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Ruturned value of attributes count <> 4."));
                return false;
            }
        }
        /// <summary>
        /// Дополняет объект типа данных членами данной структуры.
        /// Service Code: "0x4C"
        /// Class: "0x6C"
        /// </summary>
        /// <param name="templateInfo">Объект типа данных.</param>
        /// <returns></returns>
        public bool GetTemplateMembers(CLXTemplate templateInfo)
        {
            string messageEventHeaderText = "[Method='GetTemplateMembers'; TemplateInstance=" + templateInfo.SymbolTypeAttribute.Code.ToString() + "]";

            if (templateInfo == null)
            {
                return false;
            }

            UInt32 offset = 0;
            UInt16 count = (UInt16)((templateInfo.SizeOfMembersDefinition * 4) - 23 + 3);
            List<byte> recievedBytes = new List<byte>();

            // Запускаем цикл полного получения данных пока имеем возвращаемый статус 0x06 (неполное возвращение данных).
            while (true)
            {
                // Подготавливаем пакет для отправки данных.
                MessageRouterRequest request = new MessageRouterRequest();
                request.ServiceCode = 0x4C;
                request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 0x6C));
                request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, templateInfo.SymbolTypeAttribute.Code));
                request.RequestData.AddRange(BitConverter.GetBytes(offset));
                request.RequestData.AddRange(BitConverter.GetBytes(count));

                List<object> responsedObjects = eipClient.SendUnitData(request);


                MessageRouterResponse response;
                // Проверяем ответ на успешность принятых данных.
                if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Warning, messageEventHeaderText, "offset=" + offset.ToString() + ", count=" + count.ToString()));
                    return false;
                }

                // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
                if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0, 6 }, null, null))
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Warning, messageEventHeaderText, "offset=" + offset.ToString() + ", count=" + count.ToString()));
                    return false;
                }

                List<byte> bytes = response.ResponseData;

                if (response.GeneralStatus == 6)
                {
                    recievedBytes.AddRange(bytes);
                    offset += (UInt32)(bytes.Count);

                    if (count < bytes.Count)
                    {
                        Event_Message(new MessageEventArgs(this, MessageEventArgsType.Warning, messageEventHeaderText, "bytes.Count > count! " + "offset=" + offset.ToString() + ", count=" + count.ToString()));
                        return false;
                    }

                    count = (UInt16)(count - bytes.Count);
                }
                else if (response.GeneralStatus == 0)
                {
                    recievedBytes.AddRange(bytes);
                    break;
                }
                else
                {
                    return false;
                }
            }

            // Из полученных байт получаем:
            // 1. Информацию о членах структуры.
            templateInfo.ClearMembers();
            for (int ix = 0; ix < templateInfo.MemberCount; ix++)
            {
                CLXTemplateMember newTemplateMember;
                if (!ParseTemplateMemberInfoResponse(messageEventHeaderText, recievedBytes, out newTemplateMember, out recievedBytes))
                {
                    return false;
                }

                templateInfo.AddMember(newTemplateMember);
            }

            // 2. Название структуры.
            string structureName;
            if (!ParseTemplateNameResponse(messageEventHeaderText, recievedBytes, out structureName, out recievedBytes))
            {
                return false;
            }
            templateInfo.Name = structureName;

            // 3. Названия членов структуры.
            List<string> memberNames;
            if (!ParseTemplateMemberNamesResponse(messageEventHeaderText, templateInfo.MemberCount, recievedBytes, out memberNames))
            {
                return false;
            }

            // Полученные имена присваиваем соответствующим элементам членов типа данных. Соответствие кол-ва проверена на предыдущих этапах.
            for (int ix = 0; ix < memberNames.Count; ix++)
            {
                templateInfo.Members[ix].Name = memberNames[ix];
            }

            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
            return true;
        }
        /* ======================================================================================== */
        #endregion

        #region [ 3. ЧТЕНИЕ ЗНАЧЕНИЯ ТЭГОВ ИЗ УДАЛЕННОГО УСТРОЙСТВА - МЕТОД ГРУПОВОГО ЧТЕНИЯ ТАБЛИЦ ]
        /* ======================================================================================== */
        /// <summary>
        /// Создает в контроллере область памяти куда он будет перебрасывать значения определенных тэгов.
        /// Специальная функция используемая RSLinx.
        /// Service Code: "0x08"
        /// Class: "0xB2",
        /// Instance: "0x0000".
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool CreateTagReadingTable(out CLXCustomTagMemoryTable table)
        {
            string messageEventHeaderText = "[Method='CreateTagReadingTable']";

            table = null;

            // Подготавливаем пакет для отправки данных.
            MessageRouterRequest request = new MessageRouterRequest();
            request.ServiceCode = (byte)CIPCommonServices.Create;
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 0xB2));
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID_16bit, 0x0000));
            request.RequestData.Add(0x01);
            request.RequestData.Add(0x00);
            request.RequestData.Add(0x03);
            request.RequestData.Add(0x00);
            request.RequestData.Add(0x03);
            request.RequestData.Add(0x00);

            List<object> responsedObjects = eipClient.SendUnitData(request);

            /* ========================================================================== */
            // Описание принятой последовательности байт:
            // 0. - Разрешенная контроллером Instance (биты 0...7).
            // 1. - Разрешенная контроллером Instance (биты 8...15).
            // 2. - ??? Кол-во задаваемых атрибутов (биты 0...7).
            // 3. - ??? Кол-во задаваемых атрибутов (биты 8...15).
            // 4. - ?
            // 5. - ?
            // 6. - ?
            // 7. - ?
            /* ========================================================================== */
            MessageRouterResponse response;
            // Проверяем ответ на успешность принятых данных.
            if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
            {
                return false;
            }

            // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
            if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0 }, 8, 8))
            {
                return false;
            }
            else
            {
                // Присваиваем полученные результаты.
                table = new CLXCustomTagMemoryTable((UInt16)(response.ResponseData[1] << 8 | response.ResponseData[0]));
                
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK. Associated with Instance=" + table.Instance.ToString()));
                return true;
            }
        }
        /// <summary>
        /// Добавляет тэг для выполнения опеации добавления его значения в контроллерную область памяти.
        /// Специальная функция используемая RSLinx.
        /// Service Code: "0x4E"
        /// Class: "0xB2",
        /// Instance: задается специально.
        /// </summary>
        /// <returns></returns>
        public bool AddTagToReadingTable(CLXCustomTagMemoryTable table, LogixTag tag)
        {
            // Проверяем входные параметры.
            if (tag == null)
            {
                throw new ArgumentNullException("'AddTagToReadingTable': Argument of Tag is NULL");
            }

            string messageEventHeaderText = "[Method='AddTagToReadingTable']";

            // Устанавливаем начало редактирования значения тэга.
            tag.ReadValue.BeginEdition();

            // Проверка состояние тэга преде чтением.
            if (!tag.ReadEnable)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Warning, messageEventHeaderText, "Failed. Oparation was disabled by application."));
                tag.ReadValue.FinalizeEdition(false);
                return false;
            }

            if (tag.SymbolicEPath == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Incorrect Tag Name. Imposible to recognize."));
                tag.ReadValue.FinalizeEdition(false);
                return false;
            }

            if (tag.Type.Code == 0)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. TypeCode = 0x00."));
                tag.ReadValue.FinalizeEdition(false);
                return false;
            }

            // Подготавливаем пакет для отправки данных.
            MessageRouterRequest request = new MessageRouterRequest();
            request.ServiceCode = 0x4E;
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 0xB2));
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID_16bit, table.Instance));
            request.RequestData.Add(0x02);
            request.RequestData.Add(0x00);
            request.RequestData.Add(0x01);
            request.RequestData.Add(0x01);
            request.RequestData.Add(0x01);

            // Добавляем длину байт которую необходимо зарезервировать под данный тэг.
            request.RequestData.AddRange(BitConverter.GetBytes((UInt16)((tag.Type.Size * (tag.Type.ArrayDimension.HasValue ? tag.Type.ArrayDimension.Value : 1)) & 0xFFFF)));
            // Добавляем строковый путь к данному тэгу.
            request.RequestData.AddRange(tag.SymbolicEPath.ToBytes(EPathToByteMethod.Complete));
            List<object> responsedObjects = eipClient.SendUnitData(request);

            MessageRouterResponse response;
            // Проверяем ответ на успешность принятых данных.
            if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
            {
                return false;
            }

            // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
            if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0 }, 2, 2))
            {
                tag.ReadValue.FinalizeEdition(false);
                return false;
            }
            else
            {
                // Присваиваем полученные результаты.
                UInt16 id = (UInt16)(response.ResponseData[1] << 8 | response.ResponseData[0]);       

                table.Add(id, tag);

                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK. Tag Associated with ID=" + id.ToString()));
                return true;
            }
        }
        /// <summary>
        /// Удаляет тэг для выполнения опеации добавления его значения из контроллерной области памяти.
        /// Специальная функция используемая RSLinx.
        /// Service Code: "0x4F"
        /// Class: "0xB2",
        /// Instance: задается специально.
        /// </summary>
        /// <returns></returns>
        public bool RemoveTagFromReadingTable(CLXCustomTagMemoryTable table, CLXCustomTagMemoryItem item)
        {
            // Проверяем входные параметры.
            if (table == null)
            {
                throw new ArgumentNullException("'RemoveTagFromGroupReading': Argument of Table is NULL");
            }

            // Проверяем входные параметры.
            if (item == null)
            {
                throw new ArgumentNullException("'RemoveTagFromGroupReading': Argument of TableItem is NULL");
            }


            // Заголовок сообщений.
            string messageEventHeaderText = "[Method='RemoveTagFromGroupReading']";

            // Подготавливаем пакет для отправки данных.
            MessageRouterRequest request = new MessageRouterRequest();
            request.ServiceCode = 0x4F;
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 0xB2));
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID_16bit, table.Instance));
            request.RequestData.AddRange(BitConverter.GetBytes(item.ID));

            List<object> responsedObjects = eipClient.SendUnitData(request);

            MessageRouterResponse response;

            // Проверяем ответ на успешность принятых данных.
            if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
            {
                return false;
            }

            // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
            if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0 }, null, null))
            {
                return false;
            }

            table.Remove(item.ID);

            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK. Table Instance=" + table.Instance.ToString() + "."));
            return true;
        }
        /// <summary>
        /// Читает таблицу данных.
        /// Специальная функция используемая RSLinx.
        /// Service Code: "0x4С"
        /// Class: "0xB2",
        /// Instance: задается специально.
        /// </summary>
        /// <param name="instance"></param>
        public bool ReadTagReadingTable(CLXCustomTagMemoryTable table)
        {
            // Заголовок сообщений.
            string messageEventHeaderText = "[Method='ReadTagReadingTable']";

            MessageRouterRequest request = new MessageRouterRequest();
            request.ServiceCode = 0x4C;
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 0xB2));
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID_16bit, table.Instance));

            // Для каждого тэга:
            // 1. Устанавливаем начало редактирования значения тэга.
            // 2. Устанавливаем временную метку момента запроса данных от удаленного устройства (контроллера).
            DateTime requestDateTime = DateTime.Now;
            table.Items.ForEach(t =>
            {
                t.Tag.ReadValue.BeginEdition();
                t.Tag.ReadValue.SetRequestPoint(requestDateTime);
            });

            // Отправляем запрос и ожидаем ответа от удаленного устройства.
            List<object> responsedObjects = eipClient.SendUnitData(request);

            // Для каждого тэга:
            // Устанавливаем временную метку момента ответа от удаленного устройства (контроллера).
            DateTime updateDateTime = DateTime.Now;
            table.Items.ForEach(t => t.Tag.ReadValue.SetResponsePoint(updateDateTime));

            MessageRouterResponse response;
            // Проверяем ответ на успешность принятых данных.
            if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
            {
                return false;
            }

            // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
            if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0 }, null, null))
            {
                return false;
            }

            // Разбираем принятые значения таблицы и записываем статус тэгов таблицы.
            if (!table.SetBytesToItems(response.ResponseData))
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Imposible Recognize Packet. Table Instance=" + table.Instance.ToString() + "."));
                return false;
            }

            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK. Table Instance=" + table.Instance.ToString() + "."));
            return true;
        }
        /* ======================================================================================== */
        #endregion

        #region [ 4. ЧТЕНИЕ/ЗАПИСЬ ЗНАЧЕНИЯ ТЭГОВ ИЗ УДАЛЕННОГО УСТРОЙСТВА ОТКРЫТЫМ МЕТОДОМ ]
        /* ======================================================================================== */
        /// <summary>
        /// Получает значение простого тэга или структуры по заданному имени из контроллера.
        /// Service Code: "0x4C"
        /// </summary>
        /// <param name="tag">Запрашиваемый тэг на чтение.</param>
        /// <returns></returns>
        public void ReadTag(LogixTag tag)
        {
            // Проверяем входные параметры.
            if (tag == null)
            {
                throw new ArgumentNullException("tag", "Method 'ReadTag()': Argument 'tag' is NULL");
            }

            // Заголовок сообщений.
            string messageEventHeaderText = "[Method='ReadTag'; TagPath='" + tag.SymbolicEPath.ToString() + "']";

            #region [ 1. ПРЕДВАРИТЕЛЬНАЯ ПРОВЕРКА ТЭГА ]
            /* ======================================================================================== */
            // Устанавливаем начало редактирования значения тэга.
            tag.ReadValue.BeginEdition();

            // Проверка состояния тэга перед чтением на существование размера текущего типа данных.
            if (tag.Type.Size == 0)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Warning, messageEventHeaderText, "Tag Data Type Size not Defined."));
            }

            // Проверка состояние тэга преде чтением.
            if (tag.SymbolicEPath == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Incorrect Tag Name. Imposible to recognize."));
                tag.ReadValue.FinalizeEdition(false);
                return;
            }
            /* ======================================================================================== */
            #endregion

            #region [ 2. ФОРМИРОВАНИЕ ЗАПРОСА ]
            /* ======================================================================================== */
            // Подготавливаем пакет для отправки данных.
            ushort elementsForRead = 0x0001;
            if (tag.Type.ArrayDimension.HasValue)
            {
                elementsForRead = tag.Type.ArrayDimension.Value;
            }

            MessageRouterRequest request = new MessageRouterRequest();
            request.ServiceCode = 0x4C;
            request.RequestPath = tag.SymbolicEPath;
            request.RequestData.AddRange(BitConverter.GetBytes(elementsForRead));
            /* ======================================================================================== */
            #endregion

            #region [ 3. ПРОВЕРКА РАЗМЕРОВ ЗАПРОСА И ОТВЕТА ]
            /* ======================================================================================== */
            // Проверяем что отправленный и принятый пакеты не превысят пределенные максимальные размеры
            // определенные в созданном ранее подключении FrowardOpen.
            const int MESSAGE_ROUTER_RESPONSE_HEADER_SIZE = 6;  // Может быть равным 4 или 6(есть extented status). Взято максимальное значение.
            const int TAG_ATOMIC_TYPECODE_SIZE = 2;
            const int TAG_STRUCTURE_TYPECODE_SIZE = 4;

            int currentRequestPacketSize = ENCAPSULATED_PACKET_ITEM_REQUEST_HEADER_SIZE + request.ToBytes().Length;
            int currentResponsePacketSize = ENCAPSULATED_PACKET_ITEM_RESPONSE_HEADER_SIZE + MESSAGE_ROUTER_RESPONSE_HEADER_SIZE;

            // Прогнозируем размер дпринимаемых данных MessageRouterResponse.
            // Спрогнозировать ответ можно лишь в том случае если существует значение кода типа данных.
            if (tag.Type.Family == TagDataTypeFamily.AtomicBool
                || tag.Type.Family == TagDataTypeFamily.AtomicDecimal
                || tag.Type.Family == TagDataTypeFamily.AtomicFloat
                || tag.Type.Family == TagDataTypeFamily.AtomicBoolArray)
            {
                currentResponsePacketSize += TAG_ATOMIC_TYPECODE_SIZE + tag.Type.ExpectedTotalSize;
            }
            else
            {
                currentResponsePacketSize += TAG_STRUCTURE_TYPECODE_SIZE + tag.Type.ExpectedTotalSize;
            }

            // Проверяем размер запроса.
            if (this.MaxPacketSizeOtoT < currentRequestPacketSize)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag request size too much. Max Request Data Size = "
                    + this.MaxPacketSizeOtoT.ToString() + ", current Request Data Size = " + currentRequestPacketSize.ToString() + "."));
                tag.ReadValue.FinalizeEdition(false);
                return;
            }

            // Проверяем размер ответа.
            if (this.MaxPacketSizeTtoO < currentResponsePacketSize)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag response size too much. Max Response Data Size = "
                    + this.MaxPacketSizeTtoO.ToString() + ", current Response Data Size = " + currentResponsePacketSize.ToString() + "."));
                tag.ReadValue.FinalizeEdition(false);
                return;
            }
            /* ======================================================================================== */
            #endregion

            #region [ 4. ОТПРАВКА / ПОЛУЧЕНИЕ ДАННЫХ ]
            /* ======================================================================================== */
            // Устанавливаем временную метку момента запроса данных от удаленного сервера (контроллера).
            tag.ReadValue.SetRequestPoint();
            // Отправляем запрос и ожидаем ответа от удаленного устройства.
            List<object> responsedObjects = eipClient.SendUnitData(request);
            // Устанавливаем временную метку момента ответа от удаленного сервера (контроллера).
            tag.ReadValue.SetResponsePoint();
            /* ======================================================================================== */
            #endregion

            #region [ 5. РАЗБОР ПРИНЯТЫХ ДАННЫХ ]
            /* ======================================================================================== */
            MessageRouterResponse response;
            // Проверяем ответ на успешность принятых данных.
            if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
            {
                tag.ReadValue.FinalizeEdition(false);
                return;
            }

            // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
            if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0 }, 3, null))
            {
                tag.ReadValue.FinalizeEdition(false);
                return;
            }

            // Принятые данные.
            List<byte> bytes = response.ResponseData;
            // Код типа данных.
            UInt16 typeCode = BitConverter.ToUInt16(bytes.ToArray(), 0);

            // Разбор принятой последовательности байт значения тэга..       
            if (typeCode == 0x02A0)
            {
                #region [ ТИП ДАННЫХ ТЭГА : СТРУКТУРА ]
                /* ======================================================================================== */
                // Случай когда тип данных некая структура.
                if (bytes.Count > 4)
                {
                    // В случае если для запрашиваемого тэга код типа данных не определен то присваеваем его.
                    if (tag.Type.Code == 0)
                    {
                        tag.Type.Code = typeCode;
                    }

                    // Разбираем принятые данныые значения тэга.
                    // В случае если текущий тэг запрашивался с кол-вом элементов массива более 1,
                    // то укладываем принятые элементы 
                    List<byte> values = bytes.GetRange(4, bytes.Count - 4);
                    List<byte[]> recievedValue = new List<byte[]>();
                    if (!tag.Type.ArrayDimension.HasValue || tag.Type.Size == 0)
                    {
                        recievedValue.Add(values.ToArray());

                        tag.ReadValue.SetValueData(recievedValue);
                        tag.ReadValue.FinalizeEdition(true);

                        Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                    }
                    else
                    {
                        // Проверяем принятые данные перед началом разделения значений по элементам 
                        // на равенство ожидаемого размера текущего тэга.
                        if (tag.Type.ExpectedTotalSize != values.Count)
                        {
                            tag.ReadValue.FinalizeEdition(false);
                            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Recieved data Size = " + values.Count.ToString() + ", not equal to expected data Size = " + values.Count.ToString() + "."));
                        }
                        else
                        {
                            // Делим принятые данные по элементам массива.
                            for (int ixElement = 0; ixElement < values.Count; ixElement += tag.Type.Size)
                            {
                                recievedValue.Add(values.GetRange(ixElement, tag.Type.Size).ToArray());
                            }

                            tag.ReadValue.SetValueData(recievedValue);
                            tag.ReadValue.FinalizeEdition(true);

                            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                        }
                    }
                }
                else
                {
                    tag.ReadValue.FinalizeEdition(false);
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Recieved Data from Server has length less 4 bytes in case of structure Type (incorrect reply from Server)."));
                }
                /* ======================================================================================== */
                #endregion
            }
            else
            {
                #region [ ТИП ДАННЫХ ТЭГА : АТОМАРНЫЙ ]
                /* ======================================================================================== */
                // Случай когда тип данных атомарный.
                // В случае если для запрашиваемого тэга код типа данных не определен то присваеваем его.
                if (tag.Type.Code == 0)
                {
                    tag.Type.Code = typeCode;
                }

                // Разбираем принятые данныые значения тэга.
                // В случае если текущий тэг запрашивался с кол-вом элементов массива более 1,
                // то укладываем принятые элементы 
                List<byte> values = bytes.GetRange(2, bytes.Count - 2);
                List<byte[]> recievedValue = new List<byte[]>();
                if (!tag.Type.ArrayDimension.HasValue || tag.Type.Size == 0)
                {
                    recievedValue.Add(values.ToArray());

                    tag.ReadValue.SetValueData(recievedValue);
                    tag.ReadValue.FinalizeEdition(true);

                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                }
                else
                {
                    // Проверяем принятые данные перед началом разделения значений по элементам 
                    // на равенство ожидаемого размера текущего тэга.
                    if (tag.Type.ExpectedTotalSize != values.Count)
                    {
                        tag.ReadValue.FinalizeEdition(false);
                        Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Recieved data Size = " + values.Count.ToString() + ", not equal to expected data Size = " + values.Count.ToString() + "."));
                    }
                    else
                    {
                        // Делим принятые данные по элементам массива.
                        for (int ixElement = 0; ixElement < values.Count; ixElement += tag.Type.Size)
                        {
                            recievedValue.Add(values.GetRange(ixElement, tag.Type.Size).ToArray());
                        }

                        tag.ReadValue.SetValueData(recievedValue);
                        tag.ReadValue.FinalizeEdition(true);

                        Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                    }
                }
                /* ======================================================================================== */
                #endregion
            }
            /* ======================================================================================== */
            #endregion
        }
        /// <summary>
        /// Получает значение простого тэга или структуры по заданному имени из контроллера используя сервис мультизапроса.
        /// Service Code: "0x4C"
        /// </summary>
        /// <param name="validTags"></param>
        public void ReadTags(List<LogixTag> tags)
        {
            // Проверяем входные параметры.
            if (tags == null || tags.Any(t => t == null))
            {
                throw new ArgumentNullException("tags", "Method 'ReadTags()': Argument 'tags' is NULL");
            }

            // Заголовок сообщений.
            string messageEventHeaderText = "[Method='ReadTags(Multiply)']";

            const int MULTIPLY_ROUTER_REQUEST_HEADER_SIZE = 8;
            const int MULTIPLY_ROUTER_RESPONSE_HEADER_SIZE = 6;
            const int COMPLETE_HEADER_REQUEST_SIZE = ENCAPSULATED_PACKET_ITEM_REQUEST_HEADER_SIZE + MULTIPLY_ROUTER_REQUEST_HEADER_SIZE;
            const int COMPLETE_HEADER_RESPONSE_SIZE = ENCAPSULATED_PACKET_ITEM_RESPONSE_HEADER_SIZE + MULTIPLY_ROUTER_RESPONSE_HEADER_SIZE;

            // Список с запросами данных.
            List<LogixTag> validTags = new List<LogixTag>();
            Dictionary<LogixTag, MessageRouterRequest> requests = new Dictionary<LogixTag, MessageRouterRequest>();
            List<Dictionary<LogixTag, MessageRouterRequest>> groupedRequests = new List<Dictionary<LogixTag, MessageRouterRequest>>();

            #region [ 1. ПРЕДВАРИТЕЛЬНАЯ ПРОВЕРКА ТЭГА ]
            /* ======================================================================================== */
            foreach (LogixTag t in tags)
            {
                bool result = true;

                // Устанавливаем начало редактирования значения тэга.
                t.ReadValue.BeginEdition();

                // Проверка состояния тэга перед чтением на существование размера текущего типа данных.
                if (t.Type.Size == 0)
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Warning, messageEventHeaderText, "Tag Data Type Size not Defined."));
                }

                // Проверка состояние тэга преде чтением.
                if (t.SymbolicEPath == null)
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Incorrect Tag Name. Imposible to recognize."));
                    t.ReadValue.FinalizeEdition(false);
                    return;
                }

                if (result)
                {
                    validTags.Add(t);
                }
            }
            /* ======================================================================================== */
            #endregion

            #region [ 2. ПРЕДВАРИТЕЛЬНАОЕ ФОРМИРОВАНИЕ ЗАПРОСОВ ]
            /* ======================================================================================== */
            // Для добавленных тэгов
            foreach (LogixTag t in validTags)
            {
                ushort elementsForRead = 0x0001;
                if (t.Type.ArrayDimension.HasValue)
                {
                    elementsForRead = t.Type.ArrayDimension.Value;
                }

                MessageRouterRequest request = new MessageRouterRequest();
                request.ServiceCode = 0x4C;
                request.RequestPath = t.SymbolicEPath;
                request.RequestData.AddRange(BitConverter.GetBytes(elementsForRead));

                // Добавляем запрос в список только уникальные тэги.
                if (!requests.ContainsKey(t))
                {
                    requests.Add(t, request);
                }
                else
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. TagPath='" + t.SymbolicEPath.ToString() + "' already exist in requests."));
                    t.ReadValue.FinalizeEdition(false);
                }
            }
            /* ======================================================================================== */
            #endregion

            #region [ 3. ПОДСЧЕТ РАЗМЕРА СУММАРНОГО ЗАПРОСА И ГРУППИРОВКА ]
            /* ======================================================================================== */
            const int MESSAGE_ROUTER_RESPONSE_HEADER_SIZE = 6;  // Может быть равным 4 или 6(есть extented status). Взято максимальное значение.
            const int TAG_ATOMIC_TYPECODE_SIZE = 2;
            const int TAG_STRUCTURE_TYPECODE_SIZE = 4;

            int currentMultiRequestSize = COMPLETE_HEADER_REQUEST_SIZE;
            int currentMultiResponseSize = COMPLETE_HEADER_RESPONSE_SIZE;

            foreach (LogixTag tag in requests.Keys)
            {
                // 1. Подсчет запросов.
                int requestSize = 2 + requests[tag].ToBytes().Length;

                // 2. Подсчет ответов.
                // Прогнозируем размер дпринимаемых данных MessageRouterResponse.
                // Спрогнозировать ответ можно лишь в том случае если существует значение кода типа данных.
                int responseSize = MESSAGE_ROUTER_RESPONSE_HEADER_SIZE + 2;
                if (tag.Type.Family == TagDataTypeFamily.AtomicBool
                    || tag.Type.Family == TagDataTypeFamily.AtomicDecimal
                    || tag.Type.Family == TagDataTypeFamily.AtomicFloat
                    || tag.Type.Family == TagDataTypeFamily.AtomicBoolArray)
                {
                    responseSize += TAG_ATOMIC_TYPECODE_SIZE + tag.Type.ExpectedTotalSize;
                }
                else
                {
                    responseSize += TAG_STRUCTURE_TYPECODE_SIZE + tag.Type.ExpectedTotalSize;
                }

                // 3. Проверяем что данный тэг не имеет превышения размера мультизапроса и ответа будучи единственно отправленным.
                // Проверяем размер запроса.
                if (COMPLETE_HEADER_REQUEST_SIZE + requestSize > this.MaxPacketSizeOtoT)
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag request size too much. Max Request Data Size = "
                        + this.MaxPacketSizeOtoT.ToString() + ", current Request Data Size = " + (COMPLETE_HEADER_REQUEST_SIZE + requestSize).ToString() + "."));
                    tag.ReadValue.FinalizeEdition(false);
                }
                // Проверяем размер ответа.
                else if (COMPLETE_HEADER_RESPONSE_SIZE + responseSize > this.MaxPacketSizeTtoO)
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag response size too much. Max Response Data Size = "
                        + this.MaxPacketSizeTtoO.ToString() + ", current Response Data Size = " + (COMPLETE_HEADER_RESPONSE_SIZE + responseSize).ToString() + "."));
                    tag.ReadValue.FinalizeEdition(false);
                }
                else
                {
                    // 4. Проверяем сумму текущего общего мультизапроса и текущего последующего тэга на превышение максимального значения пакета.
                    // Аналогично проводим проверку для ответов.
                    // Если суммарный запрос или ответ более чем максимальные значения пакетов то создаем новый элемент списка
                    // и инициализируем заново значения размеров и запросов.
                    if (currentMultiRequestSize + requestSize > this.MaxPacketSizeOtoT
                        || currentMultiResponseSize + responseSize > this.MaxPacketSizeTtoO
                        || groupedRequests.Count() == 0)
                    {
                        groupedRequests.Add(new Dictionary<LogixTag, MessageRouterRequest>());
                        currentMultiRequestSize = COMPLETE_HEADER_REQUEST_SIZE;
                        currentMultiResponseSize = COMPLETE_HEADER_RESPONSE_SIZE;
                    }

                    // Добавляем текущую пару тэга и сформированного запроса в последний элемент.
                    groupedRequests.Last().Add(tag, requests[tag]);

                    // Увеличиваем полсчет текущего мультизапроса и соответствующего ответа.
                    currentMultiRequestSize += requestSize;
                    currentMultiResponseSize += responseSize;
                }
            }
            /* ======================================================================================== */
            #endregion

            #region [ 4. ОТПРАВКА ЗАПРОСОВ / РАЗБОР ОТВЕТОВ ]
            /* ======================================================================================== */
            foreach (Dictionary<LogixTag, MessageRouterRequest> groupedRequest in groupedRequests)
            {
                List<LogixTag> tagInProcess = groupedRequest.Keys.ToList();
                List<MessageRouterRequest> requestsInProcess = groupedRequest.Values.ToList();

                #region [ 4.1. ОТПРАВКА / ПОЛУЧЕНИЕ ДАННЫХ ]
                /* ======================================================================================== */
                // Для каждого тэга:
                // 1. Устанавливаем начало редактирования значения тэга.
                // 2. Устанавливаем временную метку момента запроса данных от удаленного устройства (контроллера).
                DateTime requestDateTime = DateTime.Now;
                tagInProcess.ForEach(t =>
                {
                    t.ReadValue.SetRequestPoint(requestDateTime);
                });

                // Отправляем запрос и ожидаем ответа от удаленного устройства.
                List<object> responsedObjects = eipClient.MultiplyServiceRequest(requestsInProcess);

                // Для каждого тэга:
                // Устанавливаем временную метку момента ответа от удаленного устройства (контроллера).
                DateTime updateDateTime = DateTime.Now;
                tagInProcess.ForEach(t =>
                {
                    t.ReadValue.SetResponsePoint(updateDateTime);
                });
                /* ======================================================================================== */
                #endregion

                #region [ 4.2. РАЗБОР ПРИНЯТЫХ ДАННЫХ ]
                /* ======================================================================================== */
                MessageRouterResponse multiplyResponse;
                List<MessageRouterResponse> responses;
                // Проверяем ответ на успешность принятых данных.
                if (!TryGetMultiplyMessageRouterResponse(messageEventHeaderText, responsedObjects, out multiplyResponse, out responses))
                {
                    tagInProcess.ForEach(t =>
                    {
                        t.ReadValue.FinalizeEdition(false);
                    });
                }
                // Проверяем ответ мультизапроса на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
                else if (!CheckMessageRouterResponse(messageEventHeaderText, multiplyResponse, new byte[] { 0, 30 }, null, null))
                {
                    tagInProcess.ForEach(t =>
                    {
                        t.ReadValue.FinalizeEdition(false);
                    });
                }
                // Проверяем соответствует ли кол-во запросам кол-ву ответов.
                // При несоответствии считаем что данные сопоставить невозможно.
                else if (tagInProcess.Count == responses.Count())
                {
                    for (int ix = 0; ix < tagInProcess.Count; ix++)
                    {
                        LogixTag tag = tagInProcess[ix];
                        MessageRouterResponse response = responses.ElementAt(ix);

                        // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
                        if (CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0 }, 3, null))
                        {
                            // Принятые данные.
                            List<byte> bytes = response.ResponseData;
                            // Код типа данных.
                            UInt16 typeCode = BitConverter.ToUInt16(bytes.ToArray(), 0);

                            // Разбор принятой последовательности байт значения тэга..       
                            if (typeCode == 0x02A0)
                            {
                                #region [ ТИП ДАННЫХ ТЭГА : СТРУКТУРА ]
                                /* ======================================================================================== */
                                // Случай когда тип данных некая структура.
                                if (bytes.Count > 4)
                                {
                                    // В случае если для запрашиваемого тэга код типа данных не определен то присваеваем его.
                                    if (tag.Type.Code == 0)
                                    {
                                        tag.Type.Code = typeCode;
                                    }

                                    // Разбираем принятые данныые значения тэга.
                                    // В случае если текущий тэг запрашивался с кол-вом элементов массива более 1,
                                    // то укладываем принятые элементы 
                                    List<byte> values = bytes.GetRange(4, bytes.Count - 4);
                                    List<byte[]> recievedValue = new List<byte[]>();
                                    if (!tag.Type.ArrayDimension.HasValue || tag.Type.Size == 0)
                                    {
                                        recievedValue.Add(values.ToArray());

                                        tag.ReadValue.SetValueData(recievedValue);
                                        tag.ReadValue.FinalizeEdition(true);

                                        Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                                    }
                                    else
                                    {
                                        // Проверяем принятые данные перед началом разделения значений по элементам 
                                        // на равенство ожидаемого размера текущего тэга.
                                        if (tag.Type.ExpectedTotalSize != values.Count)
                                        {
                                            tag.ReadValue.FinalizeEdition(false);
                                            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Recieved data Size = " + values.Count.ToString() + ", not equal to expected data Size = " + values.Count.ToString() + "."));
                                        }
                                        else
                                        {
                                            for (int ixElement = 0; ixElement < values.Count; ixElement += tag.Type.Size)
                                            {
                                                recievedValue.Add(values.GetRange(ixElement, tag.Type.Size).ToArray());
                                            }

                                            tag.ReadValue.SetValueData(recievedValue);
                                            tag.ReadValue.FinalizeEdition(true);

                                            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                                        }
                                    }
                                }
                                else
                                {
                                    tag.ReadValue.FinalizeEdition(false);
                                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Recieved Data from Server has length less 4 bytes in case of structure Type (incorrect reply from Server)."));
                                }
                                /* ======================================================================================== */
                                #endregion
                            }
                            else
                            {
                                #region [ ТИП ДАННЫХ ТЭГА : АТОМАРНЫЙ ]
                                /* ======================================================================================== */
                                // Случай когда тип данных атомарный.
                                // В случае если для запрашиваемого тэга код типа данных не определен то присваеваем его.
                                if (tag.Type.Code == 0)
                                {
                                    tag.Type.Code = typeCode;
                                }

                                // Разбираем принятые данныые значения тэга.
                                // В случае если текущий тэг запрашивался с кол-вом элементов массива более 1,
                                // то укладываем принятые элементы 
                                List<byte> values = bytes.GetRange(2, bytes.Count - 2);
                                List<byte[]> recievedValue = new List<byte[]>();
                                if (!tag.Type.ArrayDimension.HasValue || tag.Type.Size == 0)
                                {
                                    recievedValue.Add(values.ToArray());

                                    tag.ReadValue.SetValueData(recievedValue);
                                    tag.ReadValue.FinalizeEdition(true);

                                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                                }
                                else
                                {
                                    // Проверяем принятые данные перед началом разделения значений по элементам 
                                    // на равенство ожидаемого размера текущего тэга.
                                    if (tag.Type.ExpectedTotalSize != values.Count)
                                    {
                                        tag.ReadValue.FinalizeEdition(false);
                                        Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Recieved data Size = " + values.Count.ToString() + ", not equal to expected data Size = " + values.Count.ToString() + "."));
                                    }
                                    else
                                    {
                                        for (int ixElement = 0; ixElement < values.Count; ixElement += tag.Type.Size)
                                        {
                                            recievedValue.Add(values.GetRange(ixElement, tag.Type.Size).ToArray());
                                        }

                                        tag.ReadValue.SetValueData(recievedValue);
                                        tag.ReadValue.FinalizeEdition(true);

                                        Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                                    }
                                }
                                /* ======================================================================================== */
                                #endregion
                            }
                        }
                        else
                        {
                            tag.ReadValue.FinalizeEdition(false);
                        }
                    }
                }
                else
                {
                    tagInProcess.ForEach(t =>
                    {
                        t.ReadValue.FinalizeEdition(false);
                    });

                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Items Count Request and Reply of Multiply Service is not equal."));
                }
                /* ======================================================================================== */
                #endregion
            }
            /* ======================================================================================== */
            #endregion
        }
        /// <summary>
        /// Получает значение тэга массива по заданному имени из контроллера.
        /// Service Code: "0x52"
        /// </summary>
        /// <param name="tag">Запрашиваемый тэг на чтение.</param>
        public bool ReadTagFragment(LogixTag tag)
        {
            // Проверяем входные параметры.
            if (tag == null)
            {
                throw new ArgumentNullException("tag", "Method 'ReadTagFragment()': Argument 'tag' is NULL");
            }

            // Заголовок сообщений.
            string messageEventHeaderText = "[Method='ReadTagFragment'; TagPath='" + tag.SymbolicEPath.ToString() + "']";

            #region [ 1. ПРЕДВАРИТЕЛЬНАЯ ПРОВЕРКА ТЭГА ]
            /* ======================================================================================== */
            // Устанавливаем начало редактирования значения тэга.
            tag.ReadValue.BeginEdition();

            // Проверка состояние тэга перед чтением.
            if (tag.SymbolicEPath == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Incorrect Tag Name. Imposible to recognize."));
                tag.ReadValue.FinalizeEdition(false);
                return false;
            }

            if (!tag.Type.ArrayDimension.HasValue)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Current tag is not array."));
                tag.ReadValue.FinalizeEdition(false);
                return false;
            }
            /* ======================================================================================== */
            #endregion

            // Временные переменные.
            List<byte[]> recievedValue = new List<byte[]>();        // Общий результат в байтах.
            int dataSize = tag.Type.Size;                           // Размер в байтах одного элемента.
            UInt16 typeCode = 0;                                    // Текущий код типа данных.
            bool ressultOk = true;                                  // Общий результат операции.
            List<byte> values;                                      // Совокупность байт принятых от удаленного сервера (контроллера).
            UInt16 length = (UInt16)tag.Type.ArrayDimension.Value;  // Длина в элементах массива которую требуется прочитать.

            // Устанавливаем временную метку момента запроса данных от удаленного сервера (контроллера).
            tag.ReadValue.SetRequestPoint();

            do
            {
                #region [ 2. ФОРМИРОВАНИЕ ЗАПРОСА ]
                /* ======================================================================================== */
                // Вычисляем текущее смещение в байтах.
                uint offset = (uint)(recievedValue.Count * dataSize);

                // Формируем и отправляем запрос на получение частичного фрагмента массива.
                MessageRouterRequest request = new MessageRouterRequest();
                request.ServiceCode = 0x52;
                request.RequestPath = tag.SymbolicEPath;
                request.RequestData.AddRange(BitConverter.GetBytes(length)); // 2 байта. Длина элементов для чтения.
                request.RequestData.AddRange(BitConverter.GetBytes(offset)); // 4 байта. Смещение в пространстве массива в байтах.
                /* ======================================================================================== */
                #endregion

                #region [ 4. ОТПРАВКА / ПОЛУЧЕНИЕ ДАННЫХ ]
                /* ======================================================================================== */
                // Отправляем запрос и ожидаем ответа от удаленного устройства.
                List<object> responsedObjects = eipClient.SendUnitData(request);
                // Устанавливаем временную метку момента ответа от удаленного сервера (контроллера).
                tag.ReadValue.SetResponsePoint();
                /* ======================================================================================== */
                #endregion

                #region [ 5. РАЗБОР ПРИНЯТЫХ ДАННЫХ ]
                /* ======================================================================================== */
                MessageRouterResponse response;
                // Проверяем ответ на успешность принятых данных.
                if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
                {
                    ressultOk = false;
                    break;
                }

                // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
                if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0, 6 }, 3, null))
                {
                    ressultOk = false;
                    break;
                }

                // Разбор принятой последовательности байт.
                List<byte> bytes = response.ResponseData;
                // Код типа данных.
                typeCode = BitConverter.ToUInt16(bytes.ToArray(), 0);


                // Разбор принятой последовательности байт значения тэга..       
                if (typeCode == 0x02A0)
                {
                    #region [ ТИП ДАННЫХ ТЭГА : СТРУКТУРА ]
                    /* ======================================================================================== */
                    // Случай когда тип данных некая структура.
                    if (bytes.Count > 4)
                    {
                        // Выполняем разделение элементов из принятого массива байт 
                        // в случае если известен размер типа данных.
                        if (tag.Type.Code != 0)
                        {
                            // Принятые значения.
                            values = bytes.GetRange(4, bytes.Count - 4);

                            int bytesOffset = 0;
                            while ((values.Count / dataSize) > bytesOffset)
                            {
                                // Выделяем последовательность байт для текущего элемента массива.
                                byte[] element = values.GetRange(bytesOffset, dataSize).ToArray();
                                // Добавляем один элемент массива как совокупность байт.
                                recievedValue.Add(element.ToArray());
                                // Увеличивам смещение на кол-во байт.
                                bytesOffset += dataSize;
                            }
                        }
                        else
                        {
                            ressultOk = false;
                            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Data Size = 0, imposible to parse Structure array elements."));
                        }
                    }
                    else
                    {
                        ressultOk = false;
                        Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Recieved Data from Server has length less 4 bytes in case of structure Type (incorrect reply from Server)."));
                    }
                    /* ======================================================================================== */
                    #endregion
                }
                else
                {
                    #region [ ТИП ДАННЫХ ТЭГА : АТОМАРНЫЙ ]
                    /* ======================================================================================== */
                    // Случай когда тип данных некая структура.
                    if (bytes.Count > 2)
                    {
                        // В случае если для запрашиваемого тэга код типа данных не определен то присваеваем его.
                        if (tag.Type.Code == 0)
                        {
                            tag.Type.Code = typeCode;
                        }

                        // Получаем размер текущего типа данных.
                        dataSize = tag.Type.Size;
                        // Принятые значения.
                        values = bytes.GetRange(2, bytes.Count - 2);

                        int bytesOffset = 0;
                        while ((values.Count / dataSize) > bytesOffset)
                        {
                            // Выделяем последовательность байт для текущего элемента массива.
                            byte[] element = values.GetRange(bytesOffset, dataSize).ToArray();
                            // Добавляем один элемент массива как совокупность байт.
                            recievedValue.Add(element.ToArray());
                            // Увеличивам смещение на кол-во байт.
                            bytesOffset += dataSize;
                        }
                    }
                    else
                    {
                        ressultOk = false;
                        //Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Recieved Data from Server has length less 4 bytes in case of structure Type (incorrect reply from Server)."));
                    }
                    /* ======================================================================================== */
                    #endregion
                }
                /* ======================================================================================== */
                #endregion

                // Суммиеруем результат операции. В заключении для данной итерации считаем успешной 
                // если получена частично 0x06 или полностью завершенной 0x00.
                ressultOk &= ((response.GeneralStatus == 0x00 || response.GeneralStatus == 0x06)
                    && response.ReplyServiceCode == 0xD2);
            }
            while (ressultOk && recievedValue.Count < length);

            // Записываем результат операции в тэг в зависимости от результата операции.
            if (!ressultOk)
            {
                tag.ReadValue.FinalizeEdition(false);
                return false;
            }
            else
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                tag.ReadValue.SetValueData(recievedValue);
                tag.ReadValue.FinalizeEdition(true);

                return true;
            }
        }
        /// <summary>
        /// Записывает значение простого тэга по заданному имени в контроллер используя сервис мультизапроса.
        /// Service Code: "0x4D"
        /// </summary>
        /// <param name="tag">Запрашиваемый тэг на запись.</param>
        /// <returns></returns>
        public bool WriteTag(LogixTag tag)
        {
            // Проверяем входные параметры.
            if (tag == null)
            {
                throw new ArgumentNullException("tag", "Method 'WriteTag()': Argument 'tag' is NULL");
            }

            // Заголовок сообщений.
            string messageEventHeaderText = "[Method='WriteTag'; TagPath='" + tag.SymbolicEPath.ToString() + "']";

            #region [ 1. ПРЕДВАРИТЕЛЬНАЯ ПРОВЕРКА ТЭГА ]
            /* ======================================================================================== */
            // Устанавливаем начало редактирования значения тэга.
            tag.WriteValue.BeginEdition();

            if (tag.SymbolicEPath == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Incorrect Tag Name. Imposible to recognize."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            if ((tag.Type.Family != TagDataTypeFamily.AtomicBool)
                && (tag.Type.Family != TagDataTypeFamily.AtomicDecimal)
                && (tag.Type.Family != TagDataTypeFamily.AtomicFloat)
                && (tag.Type.Family != TagDataTypeFamily.AtomicBoolArray))
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. TypeCode must be as atomic."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            if (tag.WriteValue.RequestedData == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag Value is NULL."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            if (tag.WriteValue.RequestedData.All(f => tag.Type.Size != f.Length))
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag Value of items not equal to Data Type size."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            if (tag.Type.ArrayDimension.HasValue && tag.WriteValue.RequestedData.Count != tag.Type.ArrayDimension.Value)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag Value of items Count not equal to Fragment Length."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            if (tag.WriteValue.RequestedData.SelectMany(b => b).Count() != tag.Type.ExpectedTotalSize)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag Value of items not equal to Data Type size."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            if (tag.WriteValue.RequestedData.Count > 0xFFFF)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag Value dimension is too big, more than 65535 items"));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }
            /* ======================================================================================== */
            #endregion

            #region [ 2. ФОРМИРОВАНИЕ ЗАПРОСА ]
            /* ======================================================================================== */
            // Подготавливаем пакет для отправки данных.
            ushort elementsForWrite = 0x0001;
            if (tag.Type.ArrayDimension.HasValue)
            {
                elementsForWrite = tag.Type.ArrayDimension.Value;
            }

            // Подготавливаем данные для записи.
            byte[] value = tag.WriteValue.RequestedData.SelectMany(b => b).ToArray();

            // Подготавливаем пакет для отправки данных.
            MessageRouterRequest request = new MessageRouterRequest();
            request.ServiceCode = 0x4D;
            request.RequestPath = tag.SymbolicEPath;
            request.RequestData.AddRange(BitConverter.GetBytes(tag.Type.Code));
            request.RequestData.AddRange(BitConverter.GetBytes(elementsForWrite));
            request.RequestData.AddRange(value);
            /* ======================================================================================== */
            #endregion

            #region [ 3. ПРОВЕРКА РАЗМЕРОВ ЗАПРОСА И ОТВЕТА ]
            /* ======================================================================================== */
            // Проверяем что отправленный и принятый пакеты не превысят пределенные максимальные размеры
            // определенные в созданном ранее подключении FrowardOpen.
            const int MESSAGE_ROUTER_RESPONSE_HEADER_SIZE = 6;  // Может быть равным 4 или 6(есть extented status). Взято максимальное значение.

            int currentRequestPacketSize = ENCAPSULATED_PACKET_ITEM_REQUEST_HEADER_SIZE + request.ToBytes().Length;
            int currentResponsePacketSize = ENCAPSULATED_PACKET_ITEM_RESPONSE_HEADER_SIZE + MESSAGE_ROUTER_RESPONSE_HEADER_SIZE;

            // Проверяем размер запроса.
            if (this.MaxPacketSizeOtoT < currentRequestPacketSize)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag request size too much. Max Request Data Size = "
                    + this.MaxPacketSizeOtoT.ToString() + ", current Request Data Size = " + currentRequestPacketSize.ToString() + "."));
                tag.ReadValue.FinalizeEdition(false);
                return false;
            }

            // Проверяем размер ответа.
            if (this.MaxPacketSizeTtoO < currentResponsePacketSize)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag response size too much. Max Response Data Size = "
                    + this.MaxPacketSizeTtoO.ToString() + ", current Response Data Size = " + currentResponsePacketSize.ToString() + "."));
                tag.ReadValue.FinalizeEdition(false);
                return false;
            }
            /* ======================================================================================== */
            #endregion

            #region [ 4. ОТПРАВКА / ПОЛУЧЕНИЕ ДАННЫХ ]
            /* ======================================================================================== */
            // Устанавливаем временную метку момента запроса данных от удаленного сервера (контроллера).
            tag.WriteValue.SetRequestPoint();
            // Отправляем запрос и ожидаем ответа от удаленного устройства.
            List<object> responsedObjects = eipClient.SendUnitData(request);
            // Устанавливаем временную метку момента ответа от удаленного сервера (контроллера).
            tag.WriteValue.SetResponsePoint();
            /* ======================================================================================== */
            #endregion

            #region [ 5. РАЗБОР ПРИНЯТЫХ ДАННЫХ ]
            /* ======================================================================================== */
            MessageRouterResponse response;
            // Проверяем ответ на успешность принятых данных.
            if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
            {
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
            if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0 }, 0, null))
            {
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
            tag.WriteValue.SetValueData();
            tag.WriteValue.FinalizeEdition(true);
            tag.WriteEnable = false;

            return true;
            /* ======================================================================================== */
            #endregion
        }
        /// <summary>
        /// Записывает значения простых тэгов по заданному имени в контроллер.
        /// Service Code: "0x4D"
        /// </summary>
        /// <param name="tags"></param>
        public void WriteTags(List<LogixTag> tags)
        {
            // Проверяем входные параметры.
            if (tags == null || tags.Any(t => t == null))
            {
                throw new ArgumentNullException("tags", "Method 'WriteTags()': Argument 'tags' is NULL");
            }

            // Заголовок сообщений.
            string messageEventHeaderText = "[Method='WriteTags(Multiply)']";

            const int MULTIPLY_ROUTER_REQUEST_HEADER_SIZE = 8;
            const int MULTIPLY_ROUTER_RESPONSE_HEADER_SIZE = 6;
            const int COMPLETE_HEADER_REQUEST_SIZE = ENCAPSULATED_PACKET_ITEM_REQUEST_HEADER_SIZE + MULTIPLY_ROUTER_REQUEST_HEADER_SIZE;
            const int COMPLETE_HEADER_RESPONSE_SIZE = ENCAPSULATED_PACKET_ITEM_RESPONSE_HEADER_SIZE + MULTIPLY_ROUTER_RESPONSE_HEADER_SIZE;

            // Список с запросами данных.
            List<LogixTag> validTags = new List<LogixTag>();
            Dictionary<LogixTag, MessageRouterRequest> requests = new Dictionary<LogixTag, MessageRouterRequest>();
            List<Dictionary<LogixTag, MessageRouterRequest>> groupedRequests = new List<Dictionary<LogixTag, MessageRouterRequest>>();

            #region [ 1. ПРЕДВАРИТЕЛЬНАЯ ПРОВЕРКА ТЭГОВ ]
            /* ======================================================================================== */
            foreach (LogixTag t in tags)
            {
                bool result = true;

                if (t.SymbolicEPath == null)
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Incorrect Tag Name. Imposible to recognize."));
                    t.WriteValue.FinalizeEdition(false);
                    result = false;
                }

                // Устанавливаем начало редактирования значения тэга.
                t.WriteValue.BeginEdition();

                if ((t.Type.Family != TagDataTypeFamily.AtomicBool)
                    && (t.Type.Family != TagDataTypeFamily.AtomicDecimal)
                    && (t.Type.Family != TagDataTypeFamily.AtomicFloat)
                    && (t.Type.Family != TagDataTypeFamily.AtomicBoolArray))
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. TypeCode must be as atomic."));
                    t.WriteValue.FinalizeEdition(false);
                    result = false;
                }

                if (t.WriteValue.RequestedData == null)
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag Value is NULL."));
                    t.WriteValue.FinalizeEdition(false);
                    result = false;
                }

                if (t.WriteValue.RequestedData.All(f => t.Type.Size != f.Length))
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag Value of items not equal to Data Type size."));
                    t.WriteValue.FinalizeEdition(false);
                    result = false;
                }

                if (t.Type.ArrayDimension.HasValue && t.WriteValue.RequestedData.Count != t.Type.ArrayDimension.Value)
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag Value of items Count not equal to Fragment Length."));
                    t.WriteValue.FinalizeEdition(false);
                    result = false;
                }

                if (t.WriteValue.RequestedData.SelectMany(b => b).Count() != t.Type.ExpectedTotalSize)
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag Value of items not equal to Data Type size."));
                    t.WriteValue.FinalizeEdition(false);
                    result = false;
                }

                if (t.WriteValue.RequestedData.Count > 0xFFFF)
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag Value dimension is too big, more than 65535 items"));
                    t.WriteValue.FinalizeEdition(false);
                    result = false;
                }

                if (result)
                {
                    validTags.Add(t);
                }
            }
            /* ======================================================================================== */
            #endregion

            #region [ 2. ПРЕДВАРИТЕЛЬНАОЕ ФОРМИРОВАНИЕ ЗАПРОСОВ ]
            /* ======================================================================================== */
            // Для добавленных тэгов
            foreach (LogixTag t in validTags)
            {
                // Подготавливаем пакет для отправки данных.
                ushort elementsForWrite = 0x0001;
                if (t.Type.ArrayDimension.HasValue)
                {
                    elementsForWrite = t.Type.ArrayDimension.Value;
                }

                // Подготавливаем данные для записи.
                byte[] value = t.WriteValue.RequestedData.SelectMany(b => b).ToArray();

                // Подготавливаем пакет для отправки данных.
                MessageRouterRequest request = new MessageRouterRequest();
                request.ServiceCode = 0x4D;
                request.RequestPath = t.SymbolicEPath;
                request.RequestData.AddRange(BitConverter.GetBytes(t.Type.Code));
                request.RequestData.AddRange(BitConverter.GetBytes(elementsForWrite));
                request.RequestData.AddRange(value);

                // Добавляем запрос в список только уникальные тэги.
                if (!requests.ContainsKey(t))
                {
                    requests.Add(t, request);
                }
                else
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. TagPath='" + t.SymbolicEPath.ToString() + "' already exist in requests."));
                    t.WriteValue.FinalizeEdition(false);
                }
            }
            /* ======================================================================================== */
            #endregion

            #region [ 3. ПОДСЧЕТ РАЗМЕРА СУММАРНОГО ЗАПРОСА И ГРУППИРОВКА ]
            /* ======================================================================================== */
            const int MESSAGE_ROUTER_RESPONSE_HEADER_SIZE = 6;  // Может быть равным 4 или 6(есть extented status). Взято максимальное значение.

            int currentMultiRequestSize = COMPLETE_HEADER_REQUEST_SIZE;
            int currentMultiResponseSize = COMPLETE_HEADER_RESPONSE_SIZE;

            foreach (LogixTag tag in requests.Keys)
            {
                // 1. Подсчет запросов.
                int requestSize = 2 + requests[tag].ToBytes().Length;

                // 2. Подсчет ответов.
                // Прогнозируем размер дпринимаемых данных MessageRouterResponse.
                // Спрогнозировать ответ можно лишь в том случае если существует значение кода типа данных.
                int responseSize = MESSAGE_ROUTER_RESPONSE_HEADER_SIZE + 2;

                // 3. Проверяем что данный тэг не имеет превышения размера мультизапроса и ответа будучи единственно отправленным.
                // Проверяем размер запроса.
                if (COMPLETE_HEADER_REQUEST_SIZE + requestSize > this.MaxPacketSizeOtoT)
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag request size too much. Max Request Data Size = "
                        + this.MaxPacketSizeOtoT.ToString() + ", current Request Data Size = " + (COMPLETE_HEADER_REQUEST_SIZE + requestSize).ToString() + "."));
                    tag.WriteValue.FinalizeEdition(false);
                }
                // Проверяем размер ответа.
                else if (COMPLETE_HEADER_RESPONSE_SIZE + responseSize > this.MaxPacketSizeTtoO)
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag response size too much. Max Response Data Size = "
                        + this.MaxPacketSizeTtoO.ToString() + ", current Response Data Size = " + (COMPLETE_HEADER_RESPONSE_SIZE + responseSize).ToString() + "."));
                    tag.WriteValue.FinalizeEdition(false);
                }
                else
                {
                    // 4. Проверяем сумму текущего общего мультизапроса и текущего последующего тэга на превышение максимального значения пакета.
                    // Аналогично проводим проверку для ответов.
                    // Если суммарный запрос или ответ более чем максимальные значения пакетов то создаем новый элемент списка
                    // и инициализируем заново значения размеров и запросов.
                    if (currentMultiRequestSize + requestSize > this.MaxPacketSizeOtoT
                        || currentMultiResponseSize + responseSize > this.MaxPacketSizeTtoO
                        || groupedRequests.Count() == 0)
                    {
                        groupedRequests.Add(new Dictionary<LogixTag, MessageRouterRequest>());
                        currentMultiRequestSize = COMPLETE_HEADER_REQUEST_SIZE;
                        currentMultiResponseSize = COMPLETE_HEADER_RESPONSE_SIZE;
                    }

                    // Добавляем текущую пару тэга и сформированного запроса в последний элемент.
                    groupedRequests.Last().Add(tag, requests[tag]);

                    // Увеличиваем полсчет текущего мультизапроса и соответствующего ответа.
                    currentMultiRequestSize += requestSize;
                    currentMultiResponseSize += responseSize;
                }
            }
            /* ======================================================================================== */
            #endregion

            #region [ 4. ОТПРАВКА ЗАПРОСОВ / РАЗБОР ОТВЕТОВ ]
            /* ======================================================================================== */
            foreach (Dictionary<LogixTag, MessageRouterRequest> groupedRequest in groupedRequests)
            {
                List<LogixTag> tagInProcess = groupedRequest.Keys.ToList();
                List<MessageRouterRequest> requestsInProcess = groupedRequest.Values.ToList();

                #region [ 4.1. ОТПРАВКА / ПОЛУЧЕНИЕ ДАННЫХ ]
                /* ======================================================================================== */
                // Для каждого тэга:
                // 1. Устанавливаем начало редактирования значения тэга.
                // 2. Устанавливаем временную метку момента запроса данных от удаленного устройства (контроллера).
                DateTime requestDateTime = DateTime.Now;
                tagInProcess.ForEach(t =>
                {
                    t.WriteValue.SetRequestPoint(requestDateTime);
                });

                // Отправляем запрос и ожидаем ответа от удаленного устройства.
                List<object> responsedObjects = eipClient.MultiplyServiceRequest(requestsInProcess);

                // Для каждого тэга:
                // Устанавливаем временную метку момента ответа от удаленного устройства (контроллера).
                DateTime updateDateTime = DateTime.Now;
                tagInProcess.ForEach(t =>
                {
                    t.WriteValue.SetResponsePoint(updateDateTime);
                });
                /* ======================================================================================== */
                #endregion

                #region [ 4.2. РАЗБОР ПРИНЯТЫХ ДАННЫХ ]
                /* ======================================================================================== */
                MessageRouterResponse multiplyResponse;
                List<MessageRouterResponse> responses;
                // Проверяем ответ на успешность принятых данных.
                if (!TryGetMultiplyMessageRouterResponse(messageEventHeaderText, responsedObjects, out multiplyResponse, out responses))
                {
                    tagInProcess.ForEach(t =>
                    {
                        t.WriteValue.FinalizeEdition(false);
                    });
                }
                // Проверяем ответ мультизапроса на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
                else if (!CheckMessageRouterResponse(messageEventHeaderText, multiplyResponse, new byte[] { 0 }, null, null))
                {
                    tagInProcess.ForEach(t =>
                    {
                        t.WriteValue.FinalizeEdition(false);
                    });
                }
                // Проверяем соответствует ли кол-во запросам кол-ву ответов.
                // При несоответствии считаем что данные сопоставить невозможно.
                else if (tagInProcess.Count == responses.Count)
                {
                    for (int ix = 0; ix < tagInProcess.Count; ix++)
                    {
                        LogixTag tag = tagInProcess[ix];
                        MessageRouterResponse response = responses[ix];

                        // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
                        if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0 }, 0, null))
                        {
                            tag.WriteValue.FinalizeEdition(false);
                        }
                        else
                        {
                            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                            tag.WriteValue.SetValueData();
                            tag.WriteValue.FinalizeEdition(true);
                            tag.WriteEnable = false;
                        }
                    }
                }
                else
                {
                    tagInProcess.ForEach(t =>
                    {
                        t.WriteValue.FinalizeEdition(false);
                    });

                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Items Count Request and Reply of Multiply Service is not equal."));
                }
                /* ======================================================================================== */
                #endregion
            }
            /* ======================================================================================== */
            #endregion
        }
        /// <summary>
        /// Записывает значение тэга массива по заданному имени в контроллер.
        /// Service Code: "0x53"
        /// </summary>
        /// <param name="tag">Запрашиваемый тэг на запись.</param>
        /// <returns></returns>
        public bool WriteTagFragment(LogixTag tag)
        {
            // Проверяем входные параметры.
            if (tag == null)
            {
                throw new ArgumentNullException("tag", "Method 'WriteTagFragment()': Argument 'tag' is NULL");
            }

            // Заголовок сообщений.
            string messageEventHeaderText = "[Method='WriteTagFragment'; TagPath='" + tag.SymbolicEPath.ToString() + "']";

            int maxRequestDataBytesSize = 0;

            #region [ 1. ПРЕДВАРИТЕЛЬНАЯ ПРОВЕРКА ТЭГА ]
            /* ======================================================================================== */
            // Устанавливаем начало редактирования значения тэга.
            tag.WriteValue.BeginEdition();

            if (tag.Type.Code == 0)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. TypeCode = 0x00."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            if (tag.WriteValue.RequestedData == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag Value is NULL."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            if (tag.WriteValue.RequestedData.All(t => tag.Type.Size != t.Length))
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag Value of items not equal to Data Type size."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            if (tag.Type.ArrayDimension.HasValue && tag.WriteValue.RequestedData.Count != tag.Type.ArrayDimension.Value)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag Value of items Count not equal to Fragment Length."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            if (tag.WriteValue.RequestedData.Count > 0xFFFF)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Tag Value dimension is too big, more than 65535 items"));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }
            /* ======================================================================================== */
            #endregion

            #region [ 2. ОПРЕДЕЛЕНИЕ РАЗМЕРОВ ДОПУСТИМОГО ЗАПРОСА  ]
            /* ======================================================================================== */
            // Определяем максимальный размер (в байтах) данных которые можно передать за один запрос.
            maxRequestDataBytesSize = (this.MaxPacketSizeTtoO - ENCAPSULATED_PACKET_ITEM_REQUEST_HEADER_SIZE +
                (9 + tag.SymbolicEPath.ToBytes(EPathToByteMethod.Complete).Length));

            // Переопределяем полученное выше значения под размер кратный размеру текущего типа данных тэга.
            maxRequestDataBytesSize = (maxRequestDataBytesSize / tag.Type.Size) * tag.Type.Size;
            /* ======================================================================================== */
            #endregion

            List<byte> writeValue = tag.WriteValue.RequestedData.SelectMany(t => t).ToList();
            bool ressultOk = true;                                          // Результат операции.
            int offset = 0;                                                 // Смещение в байтах.
            UInt16 length = (UInt16)(tag.WriteValue.RequestedData.Count);   // Длина элементов массива для записи.
            List<byte> buffer = new List<byte>();

            // Устанавливаем временную метку момента запроса данных от удаленного сервера (контроллера).
            tag.WriteValue.SetRequestPoint();

            while (offset < writeValue.Count)
            {
                #region [ 3. ФОРМИРОВАНИЕ ЗАПРОСА ]
                /* ======================================================================================== */
                // Выбираем длину буфера равную BUFFER_LENGTH.
                // Если смещение такое что что длина буфера остается меньше BUFFER_LENGTH то берем остаток.
                if (offset + maxRequestDataBytesSize < writeValue.Count)
                {
                    buffer = writeValue.GetRange(offset, maxRequestDataBytesSize).ToList();
                }
                else
                {
                    buffer = writeValue.GetRange(offset, writeValue.Count - offset).ToList();
                }

                // Формируем и отправляем запрос на получение частичного фрагмента массива.
                MessageRouterRequest request = new MessageRouterRequest();
                request.ServiceCode = 0x53;
                request.RequestPath = tag.SymbolicEPath;
                request.RequestData.AddRange(BitConverter.GetBytes((UInt16)tag.Type.Code));  // Код типа данных.
                request.RequestData.AddRange(BitConverter.GetBytes(length));                            // Общая длина в элементах массива запланированная к записи.      
                request.RequestData.AddRange(BitConverter.GetBytes((UInt32)offset));                    // Смещение для записи в байтах.            
                request.RequestData.AddRange(buffer);                                                   // Данные для записи.
                /* ======================================================================================== */
                #endregion

                #region [ 4.1. ОТПРАВКА / ПОЛУЧЕНИЕ ДАННЫХ ]
                /* ======================================================================================== */
                // Отправляем запрос и ожидаем ответа от удаленного устройства.
                List<object> responsedObjects = eipClient.SendUnitData(request);
                // Устанавливаем временную метку момента ответа от удаленного сервера (контроллера).
                tag.WriteValue.SetResponsePoint();
                /* ======================================================================================== */
                #endregion

                #region [ 4.2. РАЗБОР ПРИНЯТЫХ ДАННЫХ ]
                /* ======================================================================================== */
                MessageRouterResponse response;
                // Проверяем ответ на успешность принятых данных.
                if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
                {
                    ressultOk = false;
                    break;
                }

                // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
                if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0 }, null, null))
                {
                    ressultOk = false;
                    break;
                }

                ressultOk &= (response.ReplyServiceCode == 0xD3 && response.GeneralStatus == 0);

                if (!ressultOk)
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Warning, messageEventHeaderText, "Failed. Fragment of value was writed unsuccessful. Response General Status := " + response.GeneralStatus.ToString() + " (" + response.GeneralStatusText + ")"));
                    break;
                }
                else
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "Fragment of value was writed successful. Offset = " + offset.ToString() + ", length = " + buffer.Count.ToString()));
                }

                // Вычисляем текущее смещение в байтах.
                offset += maxRequestDataBytesSize;
                /* ======================================================================================== */
                #endregion
            }

            if (ressultOk)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
                tag.WriteValue.SetValueData();
                tag.WriteValue.FinalizeEdition(true);
                tag.WriteEnable = false;

                return true;
            }
            else
            {
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }
        }
        /// <summary>
        /// Производит наложение масок с логическими операциями сначала OR затем AND на значение тэга.
        /// Service Code: "0x4E"
        /// </summary>
        /// <param name="tag">Запрашиваемый тэг на чтение/запись.</param>
        public bool ReadModifyWriteTag(LogixTag tag, byte[] mask_or, byte[] mask_and)
        {
            // Проверяем входные параметры.
            if (tag == null)
            {
                throw new ArgumentNullException("tag", "Method 'ReadModifyWriteTag()': Argument 'tag' is NULL");
            }

            // Заголовок сообщений.
            string messageEventHeaderText = "[Method='ReadModifyWriteTag'; TagPath='" + tag.SymbolicEPath.ToString() + "']";

            #region [ 1. ПРЕДВАРИТЕЛЬНАЯ ПРОВЕРКА ТЭГА ]
            /* ======================================================================================== */
            // Устанавливаем начало редактирования значения тэга.
            tag.WriteValue.BeginEdition();

            // Проверка состояние тэга перед чтением.
            if (tag.SymbolicEPath == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. Incorrect Tag Name. Imposible to recognize."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            if (!tag.WriteEnable)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Warning, messageEventHeaderText, "Failed. Oparation was disabled by application."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            if (tag.Type.Size == 0)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. TypeSize = 0x00."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            UInt16 elements = (UInt16)((tag.Type.ArrayDimension.HasValue ? tag.Type.ArrayDimension.Value : (UInt16)0x0001) * tag.Type.Size);

            if (mask_or.Length != elements)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. 'OR Mask' byte length not equal to TypeSize."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            if (mask_and.Length != elements)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageEventHeaderText, "Failed. 'AND Mask' byte length not equal to TypeSize."));
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }
            /* ======================================================================================== */
            #endregion

            #region [ 2. ФОРМИРОВАНИЕ ЗАПРОСА ]
            /* ======================================================================================== */
            MessageRouterRequest request = new MessageRouterRequest();
            request.ServiceCode = 0x4E;
            request.RequestPath = tag.SymbolicEPath;
            request.RequestData.AddRange(BitConverter.GetBytes(elements));
            request.RequestData.AddRange(mask_or);
            request.RequestData.AddRange(mask_and);
            /* ======================================================================================== */
            #endregion

            #region [ 4. ОТПРАВКА / ПОЛУЧЕНИЕ ДАННЫХ ]
            /* ======================================================================================== */
            // Устанавливаем временную метку момента запроса данных от удаленного сервера (контроллера).
            tag.WriteValue.SetRequestPoint();
            // Отправляем запрос и ожидаем ответа от удаленного устройства.
            List<object> responsedObjects = eipClient.SendUnitData(request);
            // Устанавливаем временную метку момента ответа от удаленного сервера (контроллера).
            tag.WriteValue.SetResponsePoint();
            /* ======================================================================================== */
            #endregion

            #region [ 5. РАЗБОР ПРИНЯТЫХ ДАННЫХ ]
            /* ======================================================================================== */
            MessageRouterResponse response;
            // Проверяем ответ на успешность принятых данных.
            if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
            {
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }

            // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
            if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0 }, null, null))
            {
                tag.WriteValue.FinalizeEdition(false);
                return false;
            }


            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
            tag.WriteValue.FinalizeEdition(true);
            tag.WriteEnable = false;

            return true;

            /* ======================================================================================== */
            #endregion
        }
        /* ======================================================================================== */
        #endregion

        #region [ 5. ЧТЕНИЕ/ЗАПИСЬ ВРЕМЕНИ УДАЛЕННОГО УСТРОЙСТВА ]
        /* ======================================================================================== */
        /// <summary>
        /// Читает системное время из устройства.
        /// Service Code: "0x03"
        /// Class: "0x8B"
        /// Instance: "0x01".
        /// Atribute: "0x0B"
        /// </summary>
        public DateTime? GetControllerTime()
        {
            string messageEventHeaderText = "[Method='GetControllerTime']";

            // Подготавливаем пакет для отправки данных.
            MessageRouterRequest request = new MessageRouterRequest();
            request.ServiceCode = 0x03;
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 0x8B));
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, 0x01));

            request.RequestData.Add(0x01);  // Длина списка атрибутов = 1. 
            request.RequestData.Add(0x00);  //
            request.RequestData.Add(0x0B);  // Атрибут для чтения 0x0B.
            request.RequestData.Add(0x00);  //

            // Отправляем запрос и ожидаем ответа от удаленного устройства.
            List<object> responsedObjects = eipClient.SendUnitData(request);

            MessageRouterResponse response;
            // Проверяем ответ на успешность принятых данных.
            if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
            {
                return null;
            }

            // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
            if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0 }, 14, 14))
            {
                return null;
            }

            DateTime dt = new DateTime(1970, 1, 1);

            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));

            return dt.AddTicks(BitConverter.ToInt64(response.ResponseData.ToArray(), 6) * 10);
        }
        /// <summary>
        /// Устанавливает системное время устройству.
        /// Service Code: "0x03"
        /// Class: "0x8B"
        /// Instance: "0x01".
        /// Atribute: "0x06"
        /// </summary>
        /// <returns></returns>
        public bool SetControllerTime(DateTime dateTime)
        {
            string messageEventHeaderText = "[Method='SetControllerTime']";

            DateTime dt = new DateTime(1970, 1, 1);
            long coltrollerTicks = (dateTime.Ticks - dt.Ticks) / 10;

            if (coltrollerTicks < 0)
            {
                return false;
            }

            // Подготавливаем пакет для отправки данных.
            MessageRouterRequest request = new MessageRouterRequest();
            request.ServiceCode = 0x04;
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 0x8B));
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, 0x01));

            request.RequestData.Add(0x01);  // Длина списка атрибутов = 1. 
            request.RequestData.Add(0x00);  //
            request.RequestData.Add(0x06);  // Атрибут для чтения 0x06.
            request.RequestData.Add(0x00);  //
            request.RequestData.AddRange(BitConverter.GetBytes(coltrollerTicks));

            // Отправляем запрос и ожидаем ответа от удаленного устройства.
            List<object> responsedObjects = eipClient.SendUnitData(request);

            MessageRouterResponse response;
            // Проверяем ответ на успешность принятых данных.
            if (!TryGetSimpleMessageRouterResponse(messageEventHeaderText, responsedObjects, out response))
            {
                return false;
            }

            // Проверяем ответ на заданные критерии статусов, и допустимых минимального и максимального размеров данных.
            if (!CheckMessageRouterResponse(messageEventHeaderText, response, new byte[] { 0 }, 6, 6))
            {
                return false;
            }

            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, messageEventHeaderText, "OK."));
            return true;
        }
        /* ======================================================================================== */
        #endregion

        #region [ 6. ЧТЕНИЕ ПАРМЕТРОВ ИЗ УДАЛЕННОГО УСТРОЙСТВА СЕТИ DEVICE NET]
        /* ======================================================================================== */
        /// <summary>
        /// Читает параметр из устройства.
        /// Service Code: "0x0E"
        /// Instance: "Необходимый параметр".
        /// </summary>
        /// <param name="parameter"></param>
        public void GetParameter(uint parameter)
        {
            UnconnectedSendRequest unconnectedSendRequest = new UnconnectedSendRequest();

            unconnectedSendRequest.TimeOutTicks = 154;
            unconnectedSendRequest.PriorityTimeTick = 6;
            unconnectedSendRequest.ConnectionPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Port_Backplane, 1));
            unconnectedSendRequest.ConnectionPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Port_Network, 4));

            MessageRouterRequest messageRouterRequest = new MessageRouterRequest();
            messageRouterRequest.ServiceCode = 0x0E;
            messageRouterRequest.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 0x93));
            messageRouterRequest.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, parameter));
            messageRouterRequest.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_AttributeID, 0x09));

            unconnectedSendRequest.MessageRequest = messageRouterRequest;

            List<object> objects = eipClient.UnconnectedMessageRequest(unconnectedSendRequest);
        }
        /* ======================================================================================== */
        #endregion

        /// <summary>
        /// Получает значение основных свойств объекта устройства в виде XML элемнта.
        /// </summary>
        /// <returns></returns>
        public XElement GetXElement()
        {
            XElement xdevice = new XElement("Device");
            xdevice.Add(new XAttribute("Name", this.Name));
            xdevice.Add(new XAttribute("IPAddress", this.Address));
            xdevice.Add(new XAttribute("ProcessorSlot", this.ProcessorSlot));
            return xdevice;
        }
        /// <summary>
        /// Устнавливает значение основных свойств объекта устройства из XML элемнта.
        /// </summary>
        /// <param name="xdevice"></param>
        /// <returns></returns>
        public bool SetXElement(XElement xdevice)
        {
            if (!xdevice.ExistAs("Device"))
            {
                return false;
            }

            string name;            // Название устройства.
            string ipAddress;       // IP адрес.
            string processorSlot;   // Номер слота процессора.

            // Получам значения атрибутов.
            name = xdevice.Attribute("Name").GetXValue(null);
            ipAddress = xdevice.Attribute("IPAddress").GetXValue(null);
            processorSlot = xdevice.Attribute("ProcessorSlot").GetXValue(null);

            // Проверяем промжуточные перемнные.
            if (name == null || ipAddress == null || processorSlot == null || !processorSlot.All(c => Char.IsDigit(c)))
            {
                return false;
            }

            // Преобразовываем IP адрес из текста.
            System.Net.IPAddress ipaddress;
            if (!System.Net.IPAddress.TryParse(ipAddress, out ipaddress))
            {
                return false;
            }

            // Устанавливаем значения свойств.
            this.Name = name;
            this.Address = ipaddress;
            this.ProcessorSlot = Convert.ToByte(processorSlot);

            return true;
        }
        /* ======================================================================================== */
        #endregion

        #region [ PRIVATE METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Получает первый объект заданного типа из списка объектов.
        /// </summary>
        /// <typeparam name="T">Искомый тип элемента.</typeparam>
        /// <param name="objects">Список объектов.</param>
        /// <returns></returns>
        private T GetObject<T>(IEnumerable<object> objects)
        {
            if (objects != null)
            {
                for (int ix = 0; ix < objects.Count(); ix++)
                {
                    if (objects.ElementAt(ix) is T)
                    {
                        return (T)objects.ElementAt(ix);
                    }
                }
            }

            return default(T);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageHeader">Заголовок текстового сообщения объекта.</param>
        /// <param name="responses">Список распознанных ответов.</param>
        /// <param name="response">Результат: Ответ.</param>
        /// <returns></returns>
        private bool TryGetSimpleMessageRouterResponse(string messageHeader, List<object> responses, out MessageRouterResponse response)
        {
            response = null;

            EncapsulatedPacket encapsulatedPacket = this.GetObject<EncapsulatedPacket>(responses);
            if (encapsulatedPacket == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible to recognize EncapsulatedPacket."));
                return false;
            }

            if (encapsulatedPacket.Status != EncapsulatedPacketStatus.Success)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. EncapsulatedPacket.Status=" + encapsulatedPacket.Status.ToString()));
                return false;
            }

            MessageRouterResponse messageRouterResponse = this.GetObject<MessageRouterResponse>(responses);
            if (messageRouterResponse == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible to recognize MessageRouterResponse."));
                return false;
            }

            response = messageRouterResponse;
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageHeader">Заголовок текстового сообщения объекта.</param>
        /// <param name="responses">Список распознанных ответов.</param>
        /// <param name="response">Результат: Заголовок ответа мультизапроса.</param>
        /// <param name="multiplyResponses">Результат: Список ответов запрошенных в мультизапросе.</param>
        /// <returns></returns>
        private bool TryGetMultiplyMessageRouterResponse(string messageHeader, List<object> responses, out MessageRouterResponse response, out List<MessageRouterResponse> multiplyResponses)
        {
            response = null;
            multiplyResponses = null;

            EncapsulatedPacket encapsulatedPacket = this.GetObject<EncapsulatedPacket>(responses);
            if (encapsulatedPacket == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible to recognize EncapsulatedPacket."));
                return false;
            }

            if (encapsulatedPacket.Status != EncapsulatedPacketStatus.Success)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. EncapsulatedPacket.Status=" + encapsulatedPacket.Status.ToString()));
                return false;
            }

            MessageRouterResponse messageRouterResponse = this.GetObject<MessageRouterResponse>(responses);
            if (messageRouterResponse == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible to recognize MessageRouterResponse."));
                return false;
            }

            List<MessageRouterResponse> messageRouterResponses = this.GetObject<List<MessageRouterResponse>>(responses);
            if (messageRouterResponses == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible to recognize MessageRouterResponses."));
                return false;
            }

            response = messageRouterResponse;
            multiplyResponses = messageRouterResponses;
            return true;
        }
        /// <summary>
        /// Проверяет ответ на запрос MessageRouterRequest по критериям указанных статусов и указанных допустимых минимумов и максимумов полученных данных.
        /// </summary>
        /// <param name="messageHeader">Заголовок текстового сообщения объекта.</param>
        /// <param name="response">Распознанный ответ.</param>
        /// <param name="validMessageRouterGeneralStatus"></param>
        /// <param name="validDataLengthMin"></param>
        /// <param name="validDataLengthMax"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool CheckMessageRouterResponse(string messageHeader, MessageRouterResponse response, byte[] validMessageRouterGeneralStatus, int? validDataLengthMin, int? validDataLengthMax)
        {
            if (validMessageRouterGeneralStatus != null && !validMessageRouterGeneralStatus.Contains(response.GeneralStatus))
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. MessageRouterResponse.GeneralStatus=" + response.GeneralStatus.ToString() + " (" + response.GeneralStatusText + ")"));
                return false;
            }

            if (response.ResponseData == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Recieved Data from Server is Null (incorrect reply from Server)."));
                return false;
            }


            if (validDataLengthMin != null && validDataLengthMax != null && validDataLengthMin == validDataLengthMax && validDataLengthMin != response.ResponseData.Count)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Recieved Data from Server Has incorrect Length=" + response.ResponseData.Count.ToString() + ", not eqal to " + validDataLengthMin.ToString() + ". (incorrect reply from Server)."));
                return false;
            }

            if (validDataLengthMin != null && validDataLengthMin > response.ResponseData.Count)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Recieved Data from Server Has incorrect Length=" + response.ResponseData.Count.ToString() + ", less than " + validDataLengthMin.ToString() + ". (incorrect reply from Server)."));
                return false;
            }

            if (validDataLengthMax != null && validDataLengthMax < response.ResponseData.Count)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Recieved Data from Server Has incorrect Length=" + response.ResponseData.Count.ToString() + ", more than " + validDataLengthMax.ToString() + ". (incorrect reply from Server)."));
                return false;
            }

            return true;
        }
        /// <summary>
        /// Разбирает ответы на запрос списка тэгов.
        /// </summary>
        /// <param name="messageHeader">Заголовок текстового сообщения объекта.</param>
        /// <param name="program">Название программы к которой относится данный тэг. В случае если значение равно Null текущий тэг считается глобальным.</param>
        /// <param name="inBytes">Входящий массив байт.</param>
        /// <param name="tag">Результат: Объект в виде тэга с именеи и информацией о типе данных. В случае ошибки возвращает Null.</param>
        /// <param name="remainBytes">Результат: Оставшийся массив байт.</param>
        /// <returns>Возвращяет </returns>
        private bool ParseTagInformation(string messageHeader, string program, List<byte> inBytes, out CLXTag tag, out List<byte> remainBytes)
        {
            tag = null;
            remainBytes = inBytes.ToList();

            if (inBytes == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Input Bytes is Null."));
                return false;
            }

            int expectedByteLength = 0;
            int currBytePos = 0;

            // Переменные.
            UInt32 tagInstance;     // Адресный номер (Istance) в удаленном устройстве.
            string tagName;         // Название тэга.
            UInt32 arrayDim0;
            UInt32 arrayDim1;
            UInt32 arrayDim2;

            // Получаем Instance.
            expectedByteLength = 4;
            if (inBytes.Count < expectedByteLength + currBytePos)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed of parse Tag Instance. Count of Input Bytes < " + (expectedByteLength + currBytePos).ToString()));
                return false;
            }
            tagInstance = BitConverter.ToUInt32(inBytes.ToArray(), currBytePos);
            currBytePos += expectedByteLength;

            // Получаем длину названия тэга.
            expectedByteLength = 2;
            if (inBytes.Count < expectedByteLength + currBytePos)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed of parse Tag Name Length. Count of Input Bytes < " + (expectedByteLength + currBytePos).ToString()));
                return false;
            }
            UInt16 tagNameLength = BitConverter.ToUInt16(inBytes.ToArray(), currBytePos);
            currBytePos += expectedByteLength;

            // Получаем название тэга.
            expectedByteLength = tagNameLength;
            if (inBytes.Count < expectedByteLength + currBytePos)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed of parse Tag Name Symbols. Count of Input Bytes < " + (expectedByteLength + currBytePos).ToString()));
                return false;
            }
            tagName = new string(inBytes.GetRange(currBytePos, tagNameLength).Select(b => (char)b).ToArray());
            currBytePos += tagNameLength;

            // Получаем 16-ти разрядное слово описания типа тэга SymbolTypeAttribute.
            expectedByteLength = 2;
            if (inBytes.Count < expectedByteLength + currBytePos)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed of parse Tag Symbol Type Attribute. Count of Input Bytes < " + (expectedByteLength + currBytePos).ToString()));
                return false;
            }
            UInt16 symbolTypeAttribute = BitConverter.ToUInt16(inBytes.ToArray(), currBytePos);
            currBytePos += expectedByteLength;

            // Получаем значения размерностей массива.
            expectedByteLength = 12;
            if (inBytes.Count < expectedByteLength + currBytePos)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed of parse Tag Array Dimensions. Count of Input Bytes < " + (expectedByteLength + currBytePos).ToString()));
                return false;
            }

            arrayDim0 = BitConverter.ToUInt32(inBytes.ToArray(), currBytePos);
            currBytePos += 4;
            arrayDim1 = BitConverter.ToUInt32(inBytes.ToArray(), currBytePos);
            currBytePos += 4;
            arrayDim2 = BitConverter.ToUInt32(inBytes.ToArray(), currBytePos);
            currBytePos += 4;

            // На основании полученной информации создаем тэг.
            tag = new CLXTag(program, tagName, tagInstance, symbolTypeAttribute, arrayDim0, arrayDim1, arrayDim2);

            // Получаем оставшиеся байты.
            remainBytes = inBytes.GetRange(currBytePos, inBytes.Count - currBytePos);
            return true;
        }
        /// <summary>
        /// Производит разбор байт с извлечением списка кодов структур типов данных.
        /// Данный метод является вспомогательным.
        /// </summary>
        /// <param name="messageHeader">Заголовок текстового сообщения объекта.</param>
        /// <param name="inBytes">Результат: Входящий массив байт.</param>
        /// <param name="templateInstances">Результат: Список кодов структур типов данных.</param>
        /// <returns>Возвращает True в случае успешного выполнения операции.</returns>
        private bool ParseTemplateListResponse(string messageHeader, List<byte> inBytes, out List<UInt16> templateInstances)
        {
            templateInstances = null;

            if (inBytes == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible Parse Attribute data. Bytes is Null."));
                return false;
            }

            if (inBytes.Count < 4)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible Parse Attribute data. Bytes length <4"));
                return false;
            }

            if ((inBytes.Count / 4) * 4 != inBytes.Count)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible Parse Attribute data. Bytes length not a multiple 4"));
                return false;
            }

            templateInstances = new List<ushort>();

            for (int ix = 0; ix < inBytes.Count; ix += 4)
            {
                templateInstances.Add(BitConverter.ToUInt16(inBytes.ToArray(), ix));
            }

            return true;
        }
        /// <summary>
        /// Производит разбор байт с извлечением значения ожидаемого атрибута об информации структуры типа данных.
        /// Данный метод является вспомогательным.
        /// </summary>
        /// <param name="messageHeader">Заголовок текстового сообщения объекта.</param>
        /// <param name="requestedAttribute">Ожидаемый номер атрибута.</param>
        /// <param name="valueLength">Длина байт значения атрибута.</param>
        /// <param name="inBytes">Входящий массив байт.</param>
        /// <param name="attributeValue">Результат: Значение атрибута.</param>
        /// <param name="remainBytes">Результат: Оставшийся массив байт.</param>
        /// <returns>Возвращает True в случае успешного выполнения операции.</returns>
        private bool ParseTemplateAttributeResponse(string messageHeader, UInt16 requestedAttribute, int valueLength, List<byte> inBytes, out List<byte> attributeValue, out List<byte> remainBytes)
        {
            remainBytes = null;
            attributeValue = null;

            int length = 2 + 2 + valueLength;

            if (inBytes == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible Parse Attribute data. Bytes is Null."));
                return false;
            }

            if (inBytes.Count < length)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible Parse Attribute data. Bytes length too short."));
                return false;
            }

            List<byte> bytes = inBytes.GetRange(0, length);

            UInt16 recievedAttribute = BitConverter.ToUInt16(bytes.ToArray(), 0);
            UInt16 status = BitConverter.ToUInt16(bytes.ToArray(), 2);

            if (recievedAttribute == requestedAttribute)
            {
                if (status == 0)
                {
                    attributeValue = bytes.GetRange(4, valueLength);
                    remainBytes = inBytes.GetRange(length, inBytes.Count - length);

                    return true;
                }
                else
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Requested Attribute " + requestedAttribute + " has usucessful Status=" + status.ToString()));
                    return false;
                }
            }
            else
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Requested Attribute =" + requestedAttribute + " is not equal recieved =" + recievedAttribute + "."));
                return false;
            }
        }
        /// <summary>
        /// Производит разбор байт с извлечением основной информацией об члене структуры типа данных без имени.
        /// Данный метод является вспомогательным.
        /// </summary>
        /// <param name="messageHeader">Заголовок текстового сообщения объекта.</param>
        /// <param name="inBytes">Входящий массив байт.</param>
        /// <param name="templateMember">Результат: Объект с основной информацией об члене структуры типа данных без имени.</param>
        /// <param name="remainBytes">Результат: Оставшийся массив байт.</param>
        /// <returns>Возвращает True в случае успешного выполнения операции.</returns>
        private bool ParseTemplateMemberInfoResponse(string messageHeader, List<byte> inBytes, out CLXTemplateMember templateMember, out List<byte> remainBytes)
        {
            templateMember = null;
            remainBytes = null;

            if (inBytes == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible Parse Template Member data. Bytes is Null."));
                return false;
            }

            if (inBytes.Count < 8)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible ParseTemplate Member data. Bytes length to <8."));
                return false;
            }

            List<byte> bytes = inBytes.GetRange(0, 8);

            templateMember = new CLXTemplateMember(
                null,
                BitConverter.ToUInt16(bytes.ToArray(), 0),
                BitConverter.ToUInt16(bytes.ToArray(), 2),
                BitConverter.ToUInt32(bytes.ToArray(), 4));

            remainBytes = inBytes.GetRange(8, inBytes.Count - 8);

            return true;
        }
        /// <summary>
        /// Производит разбор байт с извлечением имени структуры типа данных.
        /// Данный метод является вспомогательным.
        /// </summary>
        /// <param name="messageHeader">Заголовок текстового сообщения объекта.</param>
        /// <param name="inBytes">Входящий массив байт.</param>
        /// <param name="templateName">Результат: Имя структуры типа данных.</param>
        /// <param name="remainBytes">Результат: Оставшийся массив байт.</param>
        /// <returns>Возвращает True в случае успешного выполнения операции.</returns>
        private bool ParseTemplateNameResponse(string messageHeader, List<byte> inBytes, out string templateName, out List<byte> remainBytes)
        {
            templateName = null;
            remainBytes = null;

            if (inBytes == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible Parse Template Member data. Bytes is Null."));
                return false;
            }

            if (inBytes.Count == 0)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible ParseTemplate Member data. Bytes length =0."));
                return false;
            }

            remainBytes = new List<byte>();
            templateName = "";
            bool endOfStringWasFound = false;
            for (int ix = 0; ix < inBytes.Count; ix++)
            {
                if (!endOfStringWasFound)
                {
                    if (inBytes[ix] == 0x00)
                    {
                        endOfStringWasFound = true;
                    }
                    else
                    {
                        templateName += Convert.ToChar(inBytes[ix]);
                    }
                }
                else
                {
                    remainBytes.Add(inBytes[ix]);
                }
            }

            if (!endOfStringWasFound)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Symbol 0x00 of 'End' of Template Name string was not found."));
                return false;
            }

            return true;
        }
        /// <summary>
        /// Производит разбор байт с извлечением имен членов структуры типа данных.
        /// Данный метод является вспомогательным.
        /// </summary>
        /// <param name="messageHeader">Заголовок текстового сообщения объекта.</param>
        /// <param name="requiredMemberCount">Ожидаемое число членов типа данных.</param>
        /// <param name="inBytes">Входящий массив байт.</param>
        /// <param name="memberNames">Результат: Массив имен членов структуры типа данных.</param>
        /// <returns>Возвращает True в случае успешного выполнения операции.</returns>
        private bool ParseTemplateMemberNamesResponse(string messageHeader, UInt16 requiredMemberCount, List<byte> inBytes, out List<string> memberNames)
        {
            memberNames = null;

            if (inBytes == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible Parse Template Member data. Bytes is Null."));
                return false;
            }

            if (inBytes.Count == 0)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible ParseTemplate Member data. Bytes length =0."));
                return false;
            }

            List<string> recievedMemberNames = (new string(inBytes.Select(b => Convert.ToChar(b)).ToArray())).Split("\0".ToCharArray()).ToList();

            if (recievedMemberNames.Count == 0)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Imposible ParseTemplate Member data. Bytes not contains Symbols."));
                return false;
            }

            if (recievedMemberNames.Count < requiredMemberCount)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Recieved Member Names count " + recievedMemberNames.Count.ToString() + " not equal to required count " + requiredMemberCount.ToString() + "."));
                return false;
            }


            //if (recievedMemberNames.GetRange(requiredMemberCount, recievedMemberNames.Count - requiredMemberCount).Any(t => t != ""))
            //{
            //    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, messageHeader, "Failed. Recieved Member Names count " + recievedMemberNames.Count.ToString() + " not equal to required count " + requiredMemberCount.ToString() + "."));
            //    return false;
            //}

            memberNames = recievedMemberNames.GetRange(0, requiredMemberCount);

            return true;
        }
        /* ======================================================================================== */
        #endregion

        #region [ PRIVATE METHODS - В РАЗРАБОТКЕ ]
        /* ======================================================================================== */
        /// <summary>
        /// Отправляет комманду на остановку контроллера.
        /// </summary>
        /// <returns></returns>
        private bool SetPlcStopMode()
        {
            bool result = false;

            MessageRouterRequest request = new MessageRouterRequest();
            request.ServiceCode = (byte)CIPCommonServices.Stop;
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 0x8E));
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, 0x01));

            List<object> objects = eipClient.SendUnitData(request);

            if (objects != null && objects.Count > 0 && objects.Last() != null
                && objects.Last().GetType() == typeof(MessageRouterResponse))
            {
                MessageRouterResponse messageRouterResponse = (MessageRouterResponse)objects.Last();
                result = messageRouterResponse.GeneralStatus == 0;
            }

            return result;
        }
        /// <summary>
        /// Отправляет комманду на запуск контроллера.
        /// </summary>
        /// <returns></returns>
        private bool SetPlcRunMode()
        {
            bool result = false;

            MessageRouterRequest request = new MessageRouterRequest();
            request.ServiceCode = (byte)CIPCommonServices.Start;
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 0x8E));
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, 0x01));

            List<object> objects = eipClient.SendUnitData(request);

            if (objects != null && objects.Count > 0 && objects.Last() != null
                && objects.Last().GetType() == typeof(MessageRouterResponse))
            {
                MessageRouterResponse messageRouterResponse = (MessageRouterResponse)objects.Last();
                result = messageRouterResponse.GeneralStatus == 0;
            }

            return result;
        }
        /* ======================================================================================== */
        #endregion

        /// <summary>
        /// Преобразовывает текущий объект в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._Name;
        }
    }
}
