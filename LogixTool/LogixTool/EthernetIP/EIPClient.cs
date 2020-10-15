using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.EthernetIP
{
    /// <summary>
    /// Класс по работе с устройством на уровне CIP Ethernet IP.
    /// </summary>
    public class EIPClient
    {
        #region [ PROPERTIES ]
        /* ======================================================================================== */
        /// <summary>
        /// Информирует о том что имеется подключение с удаленным устройством.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return client != null && client.Connected;
            }
        }
        /// <summary>
        /// Получает или задает номер слота процессора.
        /// </summary>
        public byte ProcessorSlot { get; set; }
        /// <summary>
        /// Номер порта TCP соединения удаленного устройства.
        /// </summary>
        public ushort TargetTCPPort { get; set; }
        /// <summary>
        /// IP адрес устройства Ethernet/IP с которым ведется обмен данными.
        /// </summary>
        public IPAddress IPAddress { get; set; }
        /// <summary>
        /// Номер порта UDP соединения удаленного устройства.
        /// </summary>
        public ushort TargetUDPPort { get; set; }
        /// <summary>
        /// Номер порта UDP соединения инициатора (текущий объект).
        /// </summary>
        public ushort OriginatorUDPPort { get; set; }
        /// <summary>
        /// Текущие параметры для отрытия подключения.
        /// </summary>
        public ForwardOpenRequest CurrentForwardOpen { get; set; }
        /* ======================================================================================== */
        #endregion

        NetworkStream stream;                       //
        TcpClient client;                           //
        UInt32 sessionHandle;                       //
        UInt32 connectionID_O_T;                    //
        UInt32 connectionID_T_O;                    //
        UInt16 connectionSerialNumber;              //
        UInt16 connectionSequenceNumber;            //

        /// <summary>
        /// Создает новый объект на основании IP адреса и номером слота прибора с которым ведется подключение.
        /// </summary>
        public EIPClient(IPAddress ipAddress, byte processorSlot)
        {
            this.ProcessorSlot = processorSlot;
            this.TargetTCPPort = 0xAF12;
            this.IPAddress = ipAddress;

            this.client = new TcpClient();

            this.TargetUDPPort = 0x08AE;
            this.OriginatorUDPPort = 0x08AE;

            this.connectionSequenceNumber = 122;

            this.CurrentForwardOpen = new ForwardOpenRequest();
            this.CurrentForwardOpen.PriorityTimeTick = 7;
            this.CurrentForwardOpen.TimeOutTicks = 154;                     // TODO: до этого стояло 155
            this.CurrentForwardOpen.OriginatorVendorID = 0x004d;            //21057;
            this.CurrentForwardOpen.OriginatorSerialNumber = 0x1712346e;    //1162430531u;

            this.CurrentForwardOpen.OtoTParameters = new NetworkConnectionParameter();
            this.CurrentForwardOpen.OtoTParameters.ConnectionSize = 504;
            this.CurrentForwardOpen.OtoTParameters.ConnectionType = ConnectionType.Point_to_Point;
            this.CurrentForwardOpen.OtoTParameters.Owner = false;
            this.CurrentForwardOpen.OtoTParameters.Priority = Priority.Low;
            this.CurrentForwardOpen.OtoTParameters.VariableConnectionSize = true;

            this.CurrentForwardOpen.TtoOParameters = new NetworkConnectionParameter();
            this.CurrentForwardOpen.TtoOParameters.ConnectionSize = 504;
            this.CurrentForwardOpen.TtoOParameters.ConnectionType = ConnectionType.Point_to_Point;
            this.CurrentForwardOpen.TtoOParameters.Owner = false;
            this.CurrentForwardOpen.TtoOParameters.Priority = Priority.Low;
            this.CurrentForwardOpen.TtoOParameters.VariableConnectionSize = true;

            this.CurrentForwardOpen.OtoTRequestedPacketInterval = 2000000;
            this.CurrentForwardOpen.TtoORequestedPacketInterval = 2000000;

            this.CurrentForwardOpen.TransportClassAndTrigger = new TransportTypeAndTrigger(163);
            this.CurrentForwardOpen.ConnectionTimeOutMultiplier = 2;
        }

        #region [ EVENTS ]
        /* ================================================================================================== */
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler Connected;
        /// <summary>
        /// 
        /// </summary>
        private void Event_Connected()
        {
            if (this.Connected != null)
            {
                this.Connected(this, null);
            }
        }
        /* ================================================================================================== */
        #endregion

        #region [ PUBLIC METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Производит TCP подключение к удаленному устройству.
        /// </summary>
        public bool Connect()
        {
            try
            {
                client = new TcpClient();
                client.ReceiveTimeout = 5000;
                client.SendTimeout = 5000;
                client.Connect(this.IPAddress, this.TargetTCPPort);

                if (client.Connected)
                {
                    Event_Connected();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Производит закрытие соединения от удаленноого устройства.
        /// </summary>
        public bool Disconnect()
        {
            bool result = false;

            try
            {
                if (IsConnected)
                {
                    client.Close();
                    stream.Close();
                    result = true;
                }
            }
            catch
            {
            }
            return result;
        }

        /// <summary>
        /// Отправляет запрос идентификации объекта.
        /// </summary>
        /// <returns></returns>
        public List<object> RequestListIdentity()
        {
            List<byte> response;

            // Формируем пакет для отправки данных.
            EncapsulatedPacket encapsulatedPacket = new EncapsulatedPacket();
            encapsulatedPacket.Command = EncapsulatedPacketCommand.ListIdentity;

            if (!this.TCPRequestResponse(encapsulatedPacket.ToBytes(),out response))
            {
                return null;
            }

            return ParseEncapsulatedPacket(response);
        }
        /// <summary>
        /// Отправляет запрос идентификации доступных интерфейсов.
        /// </summary>
        /// <returns></returns>
        public List<object> RequestListInterfaces()
        {
            List<byte> response;

            // Формируем пакет для отправки данных.
            EncapsulatedPacket encapsulatedPacket = new EncapsulatedPacket();
            encapsulatedPacket.Command = EncapsulatedPacketCommand.ListInterfaces;

            if (!this.TCPRequestResponse(encapsulatedPacket.ToBytes(), out response))
            {
                return null;
            }

            return ParseEncapsulatedPacket(response);
        }
        /// <summary>
        /// Отправляет запрос идентификации доступных сервисов.
        /// </summary>
        /// <returns></returns>
        public List<object> RequestListServices()
        {
            List<byte> response;

            // Формируем пакет для отправки данных.
            EncapsulatedPacket encapsulatedPacket = new EncapsulatedPacket();
            encapsulatedPacket.Command = EncapsulatedPacketCommand.ListServices;

            if (!this.TCPRequestResponse(encapsulatedPacket.ToBytes(), out response))
            {
                return null;
            }

            return ParseEncapsulatedPacket(response);
        }
        /// <summary>
        /// Отправляет комманду Register Session удаленному устройству для инициализации открытия сессии.
        /// </summary>
        public List<object> RegisterSession()
        {
            List<byte> response;

            // Формируем пакет для отправки данных.
            EncapsulatedPacket encapsulatedPacket = new EncapsulatedPacket();
            encapsulatedPacket.Command = EncapsulatedPacketCommand.RegisterSession;
            encapsulatedPacket.CommandSpecificData.Add(1);       // Protocol version (should be set to 1)
            encapsulatedPacket.CommandSpecificData.Add(0);
            encapsulatedPacket.CommandSpecificData.Add(0);       // Session options shall be set to "0"
            encapsulatedPacket.CommandSpecificData.Add(0);

            if (!this.TCPRequestResponse(encapsulatedPacket.ToBytes(), out response))
            {
                return null;
            }

            List<object> objects = ParseEncapsulatedPacket(response);

            if (objects.Count == 2 && objects[0].GetType() == typeof(EncapsulatedPacket)
                && objects[1].GetType() == typeof(UInt32))
            {
                sessionHandle = (UInt32)objects[1];
                return objects;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Отправляет комманду Unregister Session удаленному устройству для инициализации закрытия сессии.
        /// </summary> 
        /// <returns></returns>
        public bool UnRegisterSession()
        {
            List<object> objects = new List<object>();

            EncapsulatedPacket encapsulatedPacket = new EncapsulatedPacket();
            encapsulatedPacket.Command = EncapsulatedPacketCommand.UnRegisterSession;
            encapsulatedPacket.SessionHandle = sessionHandle;

            if (this.TCPRequest(encapsulatedPacket.ToBytes()))
            {
                sessionHandle = 0;
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Открывает подключение с удаленным устройством со свойствами по умолчанию.
        /// </summary>
        /// <returns></returns>
        public List<object> ForwardOpen()
        {
            this.connectionID_O_T = 0;
            this.connectionID_T_O = Convert.ToUInt32(new Random().Next(0xfffffff));
            this.connectionSerialNumber = Convert.ToUInt16(new Random().Next(0xFFFF) + 2);

            // Самый последний в пакете элемент который описывает параметры установки подключения с удаленным устройством.
            this.CurrentForwardOpen.OtoTConnectionID = this.connectionID_O_T;
            this.CurrentForwardOpen.TtoOConnectionID = this.connectionID_T_O;
            this.CurrentForwardOpen.ConnectionSerialNumber = this.connectionSerialNumber;
            this.CurrentForwardOpen.ConnectionPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Port_Backplane, this.ProcessorSlot));
            this.CurrentForwardOpen.ConnectionPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 2));
            this.CurrentForwardOpen.ConnectionPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, 1));

            // Запрос маршрутизации на перевод данных в другой логический сегмент.
            MessageRouterRequest messageRouterRequest = new MessageRouterRequest();
            messageRouterRequest.ServiceCode = 0x54;
            messageRouterRequest.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 6));
            messageRouterRequest.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, 1));
            messageRouterRequest.RequestData.AddRange(this.CurrentForwardOpen.ToBytes());

            List<object> objects = SendRRData(messageRouterRequest);

            return objects;
        }
        /// <summary>
        /// Закрывает подключение с удаленным устройством.
        /// </summary>
        /// <returns></returns>
        public List<object> ForwardClose()
        {
            ForwardCloseRequest forwardCloseRequest = new ForwardCloseRequest();
            forwardCloseRequest.PriorityTimeTick = 7;
            forwardCloseRequest.TimeOutTicks = 155;
            forwardCloseRequest.ConnectionSerialNumber = connectionSerialNumber;
            forwardCloseRequest.OriginatorVendorID = this.CurrentForwardOpen.OriginatorVendorID;
            forwardCloseRequest.OriginatorSerialNumber = this.CurrentForwardOpen.OriginatorSerialNumber;
            forwardCloseRequest.ConnectionPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Port_Backplane, this.ProcessorSlot));
            forwardCloseRequest.ConnectionPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 2));
            forwardCloseRequest.ConnectionPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, 1));

            MessageRouterRequest messageRouterRequest = new MessageRouterRequest();
            messageRouterRequest.ServiceCode = 0x4E;
            messageRouterRequest.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 6));
            messageRouterRequest.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, 1));
            messageRouterRequest.RequestData.AddRange(forwardCloseRequest.ToBytes());

            List<object> objects = SendRRData(messageRouterRequest);

            return objects;
        }
        /// <summary>
        /// Отправляет Explict сообщение (не требующее подключения) удаленному устройству.
        /// </summary>
        /// <param name="request">Пакет запроса для удаленного устройства.</param>
        /// <returns></returns>
        public List<object> SendRRData(MessageRouterRequest request)
        {
            List<byte> response;

            // "Фрагмент CommonPacketFormat" : 
            // Стандартный пакет EIP состоящий из двух сегментов.
            CommonPacketFormat commonPacketFormat = new CommonPacketFormat();

            // - Type ID: Null Address Item (0x0000)
            CommonPacketItem commonPacket_NullAddressItem = new CommonPacketItem();
            commonPacket_NullAddressItem.TypeID = CommonPacketItemTypeID.Address_Null;

            // - Type ID: Unconnected Data Item (0x00b2)
            CommonPacketItem commonPacket_UnconnectedDataItem = new CommonPacketItem();
            commonPacket_UnconnectedDataItem.TypeID = CommonPacketItemTypeID.Data_UnconnectedMessage;
            commonPacket_UnconnectedDataItem.Data.AddRange(request.ToBytes());

            commonPacketFormat.Add(commonPacket_NullAddressItem);
            commonPacketFormat.Add(commonPacket_UnconnectedDataItem);

            EncapsulatedPacket encapsulatedPacket = new EncapsulatedPacket();
            encapsulatedPacket.SessionHandle = sessionHandle;
            encapsulatedPacket.Command = EncapsulatedPacketCommand.SendRRData;

            // Interface Handle CIP (4 байта).
            encapsulatedPacket.CommandSpecificData.Add(0);
            encapsulatedPacket.CommandSpecificData.Add(0);
            encapsulatedPacket.CommandSpecificData.Add(0);
            encapsulatedPacket.CommandSpecificData.Add(0);
            // Timeout (2 байта).
            encapsulatedPacket.CommandSpecificData.Add(0);
            encapsulatedPacket.CommandSpecificData.Add(0);
            encapsulatedPacket.CommandSpecificData.AddRange(commonPacketFormat.ToBytes());

            if (!this.TCPRequestResponse(encapsulatedPacket.ToBytes(), out response))
            {
                return null;
            }

            return ParseEncapsulatedPacket(response);
        }
        /// <summary>
        /// Отправляет Implict сообщение (требующее подключения) удаленному устройству.
        /// </summary>
        /// <param name="request">Пакет запроса для удаленного устройства.</param>
        /// <returns></returns>
        public List<object> SendUnitData(MessageRouterRequest request)
        {
            List<byte> response;

            // "Фрагмент CommonPacketFormat" : 
            // Стандартный пакет EIP состоящий из двух сегментов.
            CommonPacketFormat commonPacketFormat = new CommonPacketFormat();

            // - Type ID: Null Address Item (0x00A1)
            CommonPacketItem commonPacket_ConnectionAddressItem = new CommonPacketItem();
            commonPacket_ConnectionAddressItem.TypeID = CommonPacketItemTypeID.Address_ConnectionBased;
            commonPacket_ConnectionAddressItem.Data.AddRange(BitConverter.GetBytes(connectionID_O_T));

            // - Type ID: Unconnected Data Item (0x00B1)
            CommonPacketItem commonPacket_ConnectedDataItem = new CommonPacketItem();
            commonPacket_ConnectedDataItem.TypeID = CommonPacketItemTypeID.Data_ConnectionTransportPacket;
            commonPacket_ConnectedDataItem.Data.AddRange((BitConverter.GetBytes(connectionSequenceNumber++)));
            commonPacket_ConnectedDataItem.Data.AddRange(request.ToBytes());

            commonPacketFormat.Add(commonPacket_ConnectionAddressItem);
            commonPacketFormat.Add(commonPacket_ConnectedDataItem);

            EncapsulatedPacket encapsulatedPacket = new EncapsulatedPacket();
            encapsulatedPacket.SessionHandle = sessionHandle;
            encapsulatedPacket.Command = EncapsulatedPacketCommand.SendUnitData;

            // Interface Handle CIP (4 байта).
            encapsulatedPacket.CommandSpecificData.Add(0);
            encapsulatedPacket.CommandSpecificData.Add(0);
            encapsulatedPacket.CommandSpecificData.Add(0);
            encapsulatedPacket.CommandSpecificData.Add(0);
            // Timeout (2 байта).
            encapsulatedPacket.CommandSpecificData.Add(0);
            encapsulatedPacket.CommandSpecificData.Add(0);
            encapsulatedPacket.CommandSpecificData.AddRange(commonPacketFormat.ToBytes());

            if (!this.TCPRequestResponse(encapsulatedPacket.ToBytes(), out response))
            {
                return null;
            }

            return ParseEncapsulatedPacket(response);
        }
        /// <summary>
        /// Отправляет данные удаленному устройству Explict методом, который в свою очередь не требует 
        /// создания подключения (Запрос->Ответ).
        /// </summary>
        /// <param name="request">Структура данных для запроса.</param>
        /// <returns></returns>
        public List<object> UnconnectedMessageRequest(UnconnectedSendRequest request)
        {
            // 2 "Фрагмент MessageRouterRequest" : 
            // Запрос маршрутизации на перевод данных в другой логический сегмент.
            MessageRouterRequest messageRouterRequest = new MessageRouterRequest();
            messageRouterRequest.ServiceCode = 0x52;
            messageRouterRequest.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 6));
            messageRouterRequest.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, 1));
            messageRouterRequest.RequestData.AddRange(request.ToBytes());

            return SendRRData(messageRouterRequest);
        }
        /// <summary>
        /// Отправляет груповой разовый запрос с Implict сообщениями (требующее подключения) удаленному устройству.
        /// </summary>
        /// <param name="requests">Пакет запроса для удаленного устройства.</param>
        public List<object> MultiplyServiceRequest(List<MessageRouterRequest> requests)
        {
            // 1. ФОРМИРУЕМ ЗАПРОС.
            MessageRouterRequest request = new MessageRouterRequest();
            request.ServiceCode = 0x0A;
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_ClassID, 0x02));
            request.RequestPath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_InstanceID, 0x01));

            List<byte[]> requestsInBytes = requests.Select(t => t.ToBytes()).ToList();

            // Number of Services contained in this request.
            request.RequestData.AddRange(BitConverter.GetBytes((UInt16)requestsInBytes.Count));

            // Offsets for each Service; from the start of the Request Data.
            UInt16 currentServiceOffset = (UInt16)(2 + 2 * requestsInBytes.Count);
            for (int serviceIx = 0; serviceIx < requestsInBytes.Count; serviceIx++)
            {
                request.RequestData.AddRange(BitConverter.GetBytes(currentServiceOffset));
                currentServiceOffset += (UInt16)(requestsInBytes[serviceIx].Length);
            }

            // Bytes of services.
            for (int serviceIx = 0; serviceIx < requestsInBytes.Count; serviceIx++)
            {
                request.RequestData.AddRange(requestsInBytes[serviceIx]);
            }

            // 2. ОТПРАВЛЯЕМ ПАКЕТ И ОЖИДАЕМ ОТВЕТ.
            // Отправляем запрос и ожидаем ответа от удаленного устройства.
            List<object> objects = SendUnitData(request);

            return objects;
        }
        /* ================================================================================================== */
        #endregion

        #region [ PRIVATE METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Отправляет последовательность байт клиенту по протоколу TCP/IP и возвращает ожидаемый ответ.
        /// </summary>
        /// <param name="request">Последовательность байт для отправки.</param>
        /// <returns></returns>
        private bool TCPRequestResponse(byte[] request, out List<byte> response)
        {
            response = null;

            try
            {
                stream = client.GetStream();
                stream.Write(request, 0, request.Length);

                byte[] readData = new Byte[1024];
                Int32 bytes = stream.Read(readData, 0, readData.Length);
                response = readData.ToList().GetRange(0, bytes);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Отправляет последовательность байт клиенту по протоколу TCP/IP.
        /// </summary>
        /// <param name="writeData">Последовательность байт для отправки.</param>
        /// <returns></returns>
        private bool TCPRequest(byte[] writeData)
        {
            try
            {
                stream = client.GetStream();
                stream.Write(writeData, 0, writeData.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Производит разбор ответа представленного в виде байтового массива по объектам.
        /// </summary>
        /// <param name="recievedBytes">Ответ от удаленного устройства в байтах.</param>
        /// <returns></returns>
        private List<object> ParseEncapsulatedPacket(List<byte> recievedBytes)
        {
            List<object> obj = new List<object>();

            EncapsulatedPacket encapsPacketResponse = EncapsulatedPacket.Parse(recievedBytes);
            if (!AddObjectToContainer(encapsPacketResponse, ref obj))
            {
                obj.Add(recievedBytes);
                return obj;
            }

            switch (encapsPacketResponse.Command)
            {
                #region [ COMMAND: "LIST SERVICES" ]
                case EncapsulatedPacketCommand.ListServices:
                    /* ======================================================================== */
                    if (encapsPacketResponse.Status == EncapsulatedPacketStatus.Success)
                    {
                        CommonPacketFormat commonPacketFormat = CommonPacketFormat.Parse(encapsPacketResponse.CommandSpecificData);
                        if (!AddObjectToContainer(commonPacketFormat, ref obj) || commonPacketFormat.ItemCount < 1 || commonPacketFormat.Items[0].Length < 4) return obj;

                        UInt16 encapsulationVersion = (UInt16)(commonPacketFormat.Items[0].Data[0] | commonPacketFormat.Items[0].Data[1] << 8);
                        if (!AddObjectToContainer(encapsulationVersion, ref obj)) return obj;

                        UInt16 capabilityFlags = (UInt16)(commonPacketFormat.Items[0].Data[2] | commonPacketFormat.Items[0].Data[3] << 8);
                        if (!AddObjectToContainer(capabilityFlags, ref obj)) return obj;

                        string text = new String(commonPacketFormat.Items[0].Data.GetRange(4, commonPacketFormat.Items[0].Data.Count - 4).Select(t => Convert.ToChar(t)).ToArray());
                        if (!AddObjectToContainer(text, ref obj)) return obj;
                    }
                    /* ======================================================================== */
                    break;
                #endregion

                #region [ COMMAND: "LIST IDENTITY" ]
                case EncapsulatedPacketCommand.ListIdentity:
                    /* ======================================================================== */
                    if (encapsPacketResponse.Status == EncapsulatedPacketStatus.Success)
                    {
                        CommonPacketFormat commonPacketFormat = CommonPacketFormat.Parse(encapsPacketResponse.CommandSpecificData);
                        if (!AddObjectToContainer(commonPacketFormat, ref obj) && commonPacketFormat.ItemCount < 1) return obj;

                        ListIdentityResponse listIdentityResponse = ListIdentityResponse.Parse(commonPacketFormat.Items[0].Data);
                        if (!AddObjectToContainer(listIdentityResponse, ref obj)) return obj;
                    }
                    break;
                /* ======================================================================== */
                #endregion

                #region [ COMMAND: "LIST INTERFACES" ]
                case EncapsulatedPacketCommand.ListInterfaces:
                    /* ======================================================================== */
                    if (encapsPacketResponse.Status == EncapsulatedPacketStatus.Success)
                    {
                    }
                    /* ======================================================================== */
                    break;
                #endregion

                #region [ COMMAND: "REGISTER SESSION" ]
                case EncapsulatedPacketCommand.RegisterSession:
                    /* ======================================================================== */
                    UInt32 returnvalue = encapsPacketResponse.SessionHandle;
                    if (!AddObjectToContainer(returnvalue, ref obj)) return obj;
                    /* ======================================================================== */
                    break;
                #endregion

                #region [ COMMAND: "UNREGISTER SESSION" ]
                case EncapsulatedPacketCommand.UnRegisterSession:
                /* ======================================================================== */
                /* ======================================================================== */
                    break;
                #endregion

                #region [ COMMAND: "SEND RR DATA" ]
                case EncapsulatedPacketCommand.SendRRData:
                    /* ======================================================================== */
                    if (encapsPacketResponse.Status == EncapsulatedPacketStatus.Success)
                    {
                        UInt32 interfaceHandle = (UInt32)(encapsPacketResponse.CommandSpecificData[0] |
                            encapsPacketResponse.CommandSpecificData[1] << 8 |
                            encapsPacketResponse.CommandSpecificData[2] << 16 |
                            encapsPacketResponse.CommandSpecificData[3] << 24);
                        if (!AddObjectToContainer(interfaceHandle, ref obj)) return obj;

                        UInt16 timeOut = (UInt16)(encapsPacketResponse.CommandSpecificData[4] |
                            encapsPacketResponse.CommandSpecificData[5] << 8);
                        if (!AddObjectToContainer(timeOut, ref obj)) return obj;

                        CommonPacketFormat commonPacketFormat_rx = CommonPacketFormat.Parse(encapsPacketResponse.CommandSpecificData.GetRange(6, encapsPacketResponse.CommandSpecificData.Count - 6));
                        if (!AddObjectToContainer(commonPacketFormat_rx, ref obj) || commonPacketFormat_rx.ItemCount < 2) return obj;

                        MessageRouterResponse messageRouterResponse = MessageRouterResponse.Parse(commonPacketFormat_rx.Items[1].Data);
                        if (!AddObjectToContainer(messageRouterResponse, ref obj)) return obj;

                        if (messageRouterResponse.ReplyServiceCode == 0xD4)
                        {
                            ForwardOpenResponse forwardOpenResponse = ForwardOpenResponse.Parse(messageRouterResponse);
                            if (!AddObjectToContainer(forwardOpenResponse, ref obj)) return obj;

                            connectionID_O_T = forwardOpenResponse.OtoTConnectionID;
                        }

                        if (messageRouterResponse.ReplyServiceCode == 0xCE)
                        {
                            ForwardCloseResponse forwardCloseResponse = ForwardCloseResponse.Parse(messageRouterResponse);
                            if (!AddObjectToContainer(forwardCloseResponse, ref obj)) return obj;
                        }
                    }
                    /* ======================================================================== */
                    break;
                #endregion

                #region [ COMMAND: "SEND UNIT DATA" ]
                case EncapsulatedPacketCommand.SendUnitData:
                    /* ======================================================================== */
                    if (encapsPacketResponse.Status == EncapsulatedPacketStatus.Success)
                    {
                        UInt32 interfaceHandle = (UInt32)(encapsPacketResponse.CommandSpecificData[0] |
                            encapsPacketResponse.CommandSpecificData[1] << 8 |
                            encapsPacketResponse.CommandSpecificData[2] << 16 |
                            encapsPacketResponse.CommandSpecificData[3] << 24);
                        if (!AddObjectToContainer(interfaceHandle, ref obj)) return obj;

                        UInt16 timeOut = (UInt16)(encapsPacketResponse.CommandSpecificData[4] |
                            encapsPacketResponse.CommandSpecificData[5] << 8);
                        if (!AddObjectToContainer(timeOut, ref obj)) return obj;

                        CommonPacketFormat commonPacketFormat_rx = CommonPacketFormat.Parse(encapsPacketResponse.CommandSpecificData.GetRange(6, encapsPacketResponse.CommandSpecificData.Count - 6));
                        if (!AddObjectToContainer(commonPacketFormat_rx, ref obj) || commonPacketFormat_rx.ItemCount < 2) return obj;

                        Int16 cipSequenceCount = (Int16)(commonPacketFormat_rx.Items[1].Data[0] | commonPacketFormat_rx.Items[1].Data[1] << 8);
                        if (!AddObjectToContainer(cipSequenceCount, ref obj)) return obj;

                        MessageRouterResponse messageRouterResponse = MessageRouterResponse.Parse(commonPacketFormat_rx.Items[1].Data.GetRange(2, commonPacketFormat_rx.Items[1].Data.Count - 2));
                        if (!AddObjectToContainer(messageRouterResponse, ref obj)) return obj;

                        #region [ MULTIPLY SERVICE REQUEST = 0x8A]
                        if (messageRouterResponse.ReplyServiceCode == 0x8A && messageRouterResponse.ResponseData.Count >= 2)
                        {
                            List<MessageRouterResponse> multiplyMessageRouterResponces = new List<MessageRouterResponse>();

                            // Полечаем кол-во сервисов.
                            UInt16 responsesCount = (UInt16)(messageRouterResponse.ResponseData[0] | messageRouterResponse.ResponseData[1] << 8);

                            // Проверяем что полученные данные по размеру больше чем длина смещений в 16-ти разрядных словах.
                            if (messageRouterResponse.ResponseData.Count >= 2 + responsesCount * 2)
                            {
                                // Получаем лист со смещением позиций байт принятых сервисов.
                                List<UInt16> offsets = new List<UInt16>();
                                for (int ix = 0; ix < responsesCount * 2; ix += 2)
                                {
                                    offsets.Add((UInt16)(messageRouterResponse.ResponseData[2 + ix] | messageRouterResponse.ResponseData[2 + ix + 1] << 8));
                                }

                                // Получаем последовательность байт для каждого принятого сервиса на основании листа смещений.
                                for (int ix = 0; ix < responsesCount; ix++)
                                {
                                    List<byte> currBytes;
                                    int offset = offsets[ix];
                                    int count = 0;
                                    if (ix + 1 < responsesCount)
                                    {
                                        count = offsets[ix + 1] - offsets[ix];
                                    }
                                    else
                                    {
                                        count = messageRouterResponse.ResponseData.Count - offset;
                                    }

                                    currBytes = messageRouterResponse.ResponseData.GetRange(offset, count);
                                    multiplyMessageRouterResponces.Add(MessageRouterResponse.Parse(currBytes));
                                }

                                obj.Add(multiplyMessageRouterResponces);
                            }
                        }
                        #endregion
                    }
                    /* ======================================================================== */
                    break;
                #endregion
            }

            return obj;
        }
        /// <summary>
        /// Производит добавление текущего объекта в лист объектов в случае если объект не равен Null.
        /// </summary>
        /// <param name="obj">Объект для добавления.</param>
        /// <param name="list">Контейнер с объектами.</param>
        /// <returns></returns>
        private bool AddObjectToContainer(object obj, ref List<object> list)
        {
            if (list != null && obj != null)
            {
                list.Add(obj);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Возвращает строковое значение производителя из кодового знаяения.
        /// </summary>
        /// <param name="vendorCode">Код производителя.</param>
        /// <returns></returns>
        private string GetVendorTextFromCode(UInt16 vendorCode)
        {
            switch (vendorCode)
            {
                case 0: return "Reserved";
                case 1: return "Rockwell Automation/Allen-Bradley";
                case 2: return "Namco Controls Corp.";
                case 3: return "Honeywell Inc.";
                case 4: return "Parker Hannifin Corp. (Veriflo Division)";
                case 5: return "Rockwell Automation/Reliance Elec.";
                case 6: return "Reserved";
                case 7: return "SMC Corporation";
                case 8: return "Molex Incorporated";
                case 9: return "Western Reserve Controls Corp.";
                case 10: return "Advanced Micro Controls Inc. (AMCI)";
                case 11: return "ASCO Pneumatic Controls";
                case 12: return "Banner Engineering Corp.";
                case 13: return "Belden Wire & Cable Company";
                case 14: return "Cooper Interconnect";
                case 15: return "Reserved";
                case 16: return "Daniel Woodhead Co. (Woodhead Connectivity)";
                case 17: return "Dearborn Group Inc.";
                case 18: return "Reserved";
                case 19: return "Helm Instrument Company";
                case 20: return "Huron Net Works";
                case 21: return "Lumberg; Inc.";
                case 22: return "Online Development Inc.(Automation Value)";
                case 23: return "Vorne Industries; Inc.";
                case 24: return "ODVA Special Reserve";
                case 25: return "Reserved";
                case 26: return "Festo Corporation";
                case 27: return "Reserved";
                case 28: return "Reserved";
                case 29: return "Reserved";
                case 30: return "Unico; Inc.";
                case 31: return "Ross Controls";
                case 32: return "Reserved";
                case 33: return "Reserved";
                case 34: return "Hohner Corp.";
                case 35: return "Micro Mo Electronics; Inc.";
                case 36: return "MKS Instruments; Inc.";
                case 37: return "Yaskawa Electric America formerly Magnetek Drives";
                case 38: return "Reserved";
                case 39: return "AVG Automation (Uticor)";
                case 40: return "Wago Corporation";
                case 41: return "Kinetics (Unit Instruments)";
                case 42: return "IMI Norgren Limited";
                case 43: return "BALLUFF; Inc.";
                case 44: return "Yaskawa Electric America; Inc.";
                case 45: return "Eurotherm Controls Inc";
                case 46: return "ABB Industrial Systems";
                case 47: return "Omron Corporation";
                case 48: return "TURCk; Inc.";
                case 49: return "Grayhill Inc.";
                case 50: return "Real Time Automation (C&ID)";
                case 51: return "Reserved";
                case 52: return "Numatics; Inc.";
                case 53: return "Lutze; Inc.";
                case 54: return "Reserved";
                case 55: return "Reserved";
                case 56: return "Softing GmbH";
                case 57: return "Pepperl + Fuchs";
                case 58: return "Spectrum Controls; Inc.";
                case 59: return "D.I.P. Inc. MKS Inst.";
                case 60: return "Applied Motion Products; Inc.";
                case 61: return "Sencon Inc.";
                case 62: return "High Country Tek";
                case 63: return "SWAC Automation Consult GmbH";
                case 64: return "Clippard Instrument Laboratory";
                case 65: return "Reserved";
                case 66: return "Reserved";
                case 67: return "Reserved";
                case 68: return "Eaton Electrical";
                case 69: return "Reserved";
                case 70: return "Reserved";
                case 71: return "Toshiba International Corp.";
                case 72: return "Control Technology Incorporated";
                case 73: return "TCS (NZ) Ltd.";
                case 74: return "Hitachi; Ltd.";
                case 75: return "ABB Robotics Products AB";
                case 76: return "NKE Corporation";
                case 77: return "Rockwell Software; Inc.";
                case 78: return "Escort Memory Systems (A Datalogic Group Co.)";
                case 79: return "Reserved";
                case 80: return "Industrial Devices Corporation";
                case 81: return "IXXAT Automation GmbH";
                case 82: return "Mitsubishi Electric Automation; Inc.";
                case 83: return "OPTO-22";
                case 84: return "Reserved";
                case 85: return "Reserved";
                case 86: return "Horner Electric";
                case 87: return "Burkert Werke GmbH & Co. KG";
                case 88: return "Reserved";
                case 89: return "Industrial Indexing Systems; Inc.";
                case 90: return "HMS Industrial Networks AB";
                case 91: return "Robicon";
                case 92: return "Helix Technology (Granville-Phillips)";
                case 93: return "Arlington Laboratory";
                case 94: return "Advantech Co. Ltd.";
                case 95: return "Square D Company";
                case 96: return "Digital Electronics Corp.";
                case 97: return "Danfoss";
                case 98: return "Reserved";
                case 99: return "Reserved";
                case 100: return "Bosch Rexroth Corporation; Pneumatics";
                case 101: return "Applied Materials; Inc.";
                case 102: return "Showa Electric Wire & Cable Co.";
                case 103: return "Pacific Scientific (API Controls Inc.)";
                case 104: return "Sharp Manufacturing Systems Corp.";
                case 105: return "Olflex Wire & Cable; Inc.";
                case 106: return "Reserved";
                case 107: return "Unitrode";
                case 108: return "Beckhoff Automation GmbH";
                case 109: return "National Instruments";
                case 110: return "Mykrolis Corporations (Millipore)";
                case 111: return "International Motion Controls Corp.";
                case 112: return "Reserved";
                case 113: return "SEG Kempen GmbH";
                case 114: return "Reserved";
                case 115: return "Reserved";
                case 116: return "MTS Systems Corp.";
                case 117: return "Krones; Inc";
                case 118: return "Reserved";
                case 119: return "EXOR Electronic R & D";
                case 120: return "SIEI S.p.A.";
                case 121: return "KUKA Roboter GmbH";
                case 122: return "Reserved";
                case 123: return "SEC (Samsung Electronics Co.; Ltd)";
                case 124: return "Binary Electronics Ltd";
                case 125: return "Flexible Machine Controls";
                case 126: return "Reserved";
                case 127: return "ABB Inc. (Entrelec)";
                case 128: return "MAC Valves; Inc.";
                case 129: return "Auma Actuators Inc";
                case 130: return "Toyoda Machine Works; Ltd";
                case 131: return "Reserved";
                case 132: return "Reserved";
                case 133: return "Balogh T.A.G.; Corporation";
                case 134: return "TR Systemtechnik GmbH";
                case 135: return "UNIPULSE Corporation";
                case 136: return "Reserved";
                case 137: return "Reserved";
                case 138: return "Conxall Corporation Inc.";
                case 139: return "Reserved";
                case 140: return "Reserved";
                case 141: return "Kuramo Electric Co.; Ltd.";
                case 142: return "Creative Micro Designs";
                case 143: return "GE Industrial Systems";
                case 144: return "Leybold Vacuum GmbH";
                case 145: return "Siemens Energy & Automation/Drives";
                case 146: return "Kodensha Ltd";
                case 147: return "Motion Engineering; Inc.";
                case 148: return "Honda Engineering Co.; Ltd";
                case 149: return "EIM Valve Controls";
                case 150: return "Melec Inc.";
                case 151: return "Sony Manufacturing Systems Corporation";
                case 152: return "North American Mfg.";
                case 153: return "WATLOW";
                case 154: return "Japan Radio Co.; Ltd";
                case 155: return "NADEX Co.; Ltd";
                case 156: return "Ametek Automation & Process Technologies";
                case 157: return "Reserved";
                case 158: return "KVASER AB";
                case 159: return "IDEC IZUMI Corporation";
                case 160: return "Mitsubishi Heavy Industries Ltd";
                case 161: return "Mitsubishi Electric Corporation";
                case 162: return "Horiba-STEC Inc.";
                case 163: return "esd electronic system design gmbh";
                case 164: return "DAIHEN Corporation";
                case 165: return "Tyco Valves & Controls/Keystone";
                case 166: return "EBARA Corporation";
                case 167: return "Reserved";
                case 168: return "Reserved";
                case 169: return "Hokuyo Electric Co. Ltd";
                case 170: return "Pyramid Solutions; Inc.";
                case 171: return "Denso Wave Incorporated";
                case 172: return "HLS Hard-Line Solutions Inc";
                case 173: return "Caterpillar; Inc.";
                case 174: return "PDL Electronics Ltd.";
                case 175: return "Reserved";
                case 176: return "Red Lion Controls";
                case 177: return "ANELVA Corporation";
                case 178: return "Toyo Denki Seizo KK";
                case 179: return "Sanyo Denki Co.; Ltd";
                case 180: return "Advanced Energy Japan K.K. (Aera Japan)";
                case 181: return "Pilz GmbH & Co";
                case 182: return "Marsh Bellofram-Bellofram PCD Division";
                case 183: return "Reserved";
                case 184: return "M-SYSTEM Co. Ltd";
                case 185: return "Nissin Electric Co.; Ltd";
                case 186: return "Hitachi Metals Ltd.";
                case 187: return "Oriental Motor Company";
                case 188: return "A&D Co.; Ltd";
                case 189: return "Phasetronics; Inc.";
                case 190: return "Cummins Engine Company";
                case 191: return "Deltron Inc.";
                case 192: return "Geneer Corporation";
                case 193: return "Anatol Automation; Inc.";
                case 194: return "Reserved";
                case 195: return "Reserved";
                case 196: return "Medar; Inc.";
                case 197: return "Comdel Inc.";
                case 198: return "Advanced Energy Industries; Inc";
                case 199: return "Reserved";
                case 200: return "DAIDEN Co.; Ltd";
                case 201: return "CKD Corporation";
                case 202: return "Toyo Electric Corporation";
                case 203: return "Reserved";
                case 204: return "AuCom Electronics Ltd";
                case 205: return "Shinko Electric Co.; Ltd";
                case 206: return "Vector Informatik GmbH";
                case 207: return "Reserved";
                case 208: return "Moog Inc.";
                case 209: return "Contemporary Controls";
                case 210: return "Tokyo Sokki Kenkyujo Co.; Ltd";
                case 211: return "Schenck-AccuRate; Inc.";
                case 212: return "The Oilgear Company";
                case 213: return "Reserved";
                case 214: return "ASM Japan K.K.";
                case 215: return "HIRATA Corp.";
                case 216: return "SUNX Limited";
                case 217: return "Meidensha Corp.";
                case 218: return "NIDEC SANKYO CORPORATION (Sankyo Seiki Mfg. Co.; Ltd)";
                case 219: return "KAMRO Corp.";
                case 220: return "Nippon System Development Co.; Ltd";
                case 221: return "EBARA Technologies Inc.";
                case 222: return "Reserved";
                case 223: return "Reserved";
                case 224: return "SG Co.; Ltd";
                case 225: return "Vaasa Institute of Technology";
                case 226: return "MKS Instruments (ENI Technology)";
                case 227: return "Tateyama System Laboratory Co.; Ltd.";
                case 228: return "QLOG Corporation";
                case 229: return "Matric Limited Inc.";
                case 230: return "NSD Corporation";
                case 231: return "Reserved";
                case 232: return "Sumitomo Wiring Systems; Ltd";
                case 233: return "Group 3 Technology Ltd";
                case 234: return "CTI Cryogenics";
                case 235: return "POLSYS CORP";
                case 236: return "Ampere Inc.";
                case 237: return "Reserved";
                case 238: return "Simplatroll Ltd";
                case 239: return "Reserved";
                case 240: return "Reserved";
                case 241: return "Leading Edge Design";
                case 242: return "Humphrey Products";
                case 243: return "Schneider Automation; Inc.";
                case 244: return "Westlock Controls Corp.";
                case 245: return "Nihon Weidmuller Co.; Ltd";
                case 246: return "Brooks Instrument (Div. of Emerson)";
                case 247: return "Reserved";
                case 248: return " Moeller GmbH";
                case 249: return "Varian Vacuum Products";
                case 250: return "Yokogawa Electric Corporation";
                case 251: return "Electrical Design Daiyu Co.; Ltd";
                case 252: return "Omron Software Co.; Ltd";
                case 253: return "BOC Edwards";
                case 254: return "Control Technology Corporation";
                case 255: return "Bosch Rexroth";
                case 256: return "Turck";
                case 257: return "Control Techniques PLC";
                case 258: return "Hardy Instruments; Inc.";
                case 259: return "LS Industrial Systems";
                case 260: return "E.O.A. Systems Inc.";
                case 261: return "Reserved";
                case 262: return "New Cosmos Electric Co.; Ltd.";
                case 263: return "Sense Eletronica LTDA";
                case 264: return "Xycom; Inc.";
                case 265: return "Baldor Electric";
                case 266: return "Reserved";
                case 267: return "Patlite Corporation";
                case 268: return "Reserved";
                case 269: return "Mogami Wire & Cable Corporation";
                case 270: return "Welding Technology Corporation (WTC)";
                case 271: return "Reserved";
                case 272: return "Deutschmann Automation GmbH";
                case 273: return "ICP Panel-Tec Inc.";
                case 274: return "Bray Controls USA";
                case 275: return "Reserved";
                case 276: return "Status Technologies";
                case 277: return "Trio Motion Technology Ltd";
                case 278: return "Sherrex Systems Ltd";
                case 279: return "Adept Technology; Inc.";
                case 280: return "Spang Power Electronics";
                case 281: return "Reserved";
                case 282: return "Acrosser Technology Co.; Ltd";
                case 283: return "Hilscher GmbH";
                case 284: return "IMAX Corporation";
                case 285: return "Electronic Innovation; Inc. (Falter Engineering)";
                case 286: return "Netlogic Inc.";
                case 287: return "Bosch Rexroth Corporation; Indramat";
                case 288: return "Reserved";
                case 289: return "Reserved";
                case 290: return "Murata Machinery Ltd.";
                case 291: return "MTT Company Ltd.";
                case 292: return "Kanematsu Semiconductor Corp.";
                case 293: return "Takebishi Electric Sales Co.";
                case 294: return "Tokyo Electron Device Ltd";
                case 295: return "PFU Limited";
                case 296: return "Hakko Automation Co.; Ltd.";
                case 297: return "Advanet Inc.";
                case 298: return "Tokyo Electron Software Technologies Ltd.";
                case 299: return "Reserved";
                case 300: return "Shinagawa Electric Wire Co.; Ltd.";
                case 301: return "Yokogawa M&C Corporation";
                case 302: return "KONAN Electric Co.; Ltd.";
                case 303: return "Binar Elektronik AB";
                case 304: return "Furukawa Electric Co.";
                case 305: return "Cooper Energy Services";
                case 306: return "Schleicher GmbH & Co.";
                case 307: return "Hirose Electric Co.; Ltd";
                case 308: return "Western Servo Design Inc.";
                case 309: return "Prosoft Technology";
                case 310: return "Reserved";
                case 311: return "Towa Shoko Co.; Ltd";
                case 312: return "Kyopal Co.; Ltd";
                case 313: return "Extron Co.";
                case 314: return "Wieland Electric GmbH";
                case 315: return "SEW Eurodrive GmbH";
                case 316: return "Aera Corporation";
                case 317: return "STA Reutlingen";
                case 318: return "Reserved";
                case 319: return "Fuji Electric Co.; Ltd.";
                case 320: return "Reserved";
                case 321: return "Reserved";
                case 322: return "ifm efector; inc.";
                case 323: return "Reserved";
                case 324: return "IDEACOD-Hohner Automation S.A.";
                case 325: return "CommScope Inc.";
                case 326: return "GE Fanuc Automation North America; Inc.";
                case 327: return "Matsushita Electric Industrial Co.; Ltd";
                case 328: return "Okaya Electronics Corporation";
                case 329: return "KASHIYAMA Industries; Ltd";
                case 330: return "JVC";
                case 331: return "Interface Corporation";
                case 332: return "Grape Systems Inc.";
                case 333: return "Reserved";
                case 334: return "Reserved";
                case 335: return "Toshiba IT & Control Systems Corporation";
                case 336: return "Sanyo Machine Works; Ltd.";
                case 337: return "Vansco Electronics Ltd.";
                case 338: return "Dart Container Corp.";
                case 339: return "Livingston & Co.; Inc.";
                case 340: return "Alfa Laval LKM as";
                case 341: return "BF ENTRON Ltd. (British Federal)";
                case 342: return "Bekaert Engineering NV";
                case 343: return "Ferran Scientific Inc.";
                case 344: return "KEBA AG";
                case 345: return "Endress + Hauser";
                case 346: return "Reserved";
                case 347: return "ABB ALSTOM Power UK Ltd. (EGT)";
                case 348: return "Berger Lahr GmbH";
                case 349: return "Reserved";
                case 350: return "Federal Signal Corp.";
                case 351: return "Kawasaki Robotics (USA); Inc.";
                case 352: return "Bently Nevada Corporation";
                case 353: return "Reserved";
                case 354: return "FRABA Posital GmbH";
                case 355: return "Elsag Bailey; Inc.";
                case 356: return "Fanuc Robotics America";
                case 357: return "Reserved";
                case 358: return "Surface Combustion; Inc.";
                case 359: return "Reserved";
                case 360: return "AILES Electronics Ind. Co.; Ltd.";
                case 361: return "Wonderware Corporation";
                case 362: return "Particle Measuring Systems; Inc.";
                case 363: return "Reserved";
                case 364: return "Reserved";
                case 365: return "BITS Co.; Ltd";
                case 366: return "Japan Aviation Electronics Industry Ltd";
                case 367: return "Keyence Corporation";
                case 368: return "Kuroda Precision Industries Ltd.";
                case 369: return "Mitsubishi Electric Semiconductor Application";
                case 370: return "Nippon Seisen Cable; Ltd.";
                case 371: return "Omron ASO Co.; Ltd";
                case 372: return "Seiko Seiki Co.; Ltd.";
                case 373: return "Sumitomo Heavy Industries; Ltd.";
                case 374: return "Tango Computer Service Corporation";
                case 375: return "Technology Service; Inc.";
                case 376: return "Toshiba Information Systems (Japan) Corporation";
                case 377: return "TOSHIBA Schneider Inverter Corporation";
                case 378: return "Toyooki Kogyo Co.; Ltd.";
                case 379: return "XEBEC";
                case 380: return "Madison Cable Corporation";
                case 381: return "Hitati Engineering & Services Co.; Ltd";
                case 382: return "TEM-TECH Lab Co.; Ltd";
                case 383: return "International Laboratory Corporation";
                case 384: return "Dyadic Systems Co.; Ltd.";
                case 385: return "SETO Electronics Industry Co.; Ltd";
                case 386: return "Tokyo Electron Kyushu Limited";
                case 387: return "KEI System Co.; Ltd";
                case 388: return "Reserved";
                case 389: return "Asahi Engineering Co.; Ltd";
                case 390: return "Contrex Inc.";
                case 391: return "Paradigm Controls Ltd.";
                case 392: return "Reserved";
                case 393: return "Ohm Electric Co.; Ltd.";
                case 394: return "RKC Instrument Inc.";
                case 395: return "Suzuki Motor Corporation";
                case 396: return "Custom Servo Motors Inc.";
                case 397: return "PACE Control Systems";
                case 398: return "Reserved";
                case 399: return "Reserved";
                case 400: return "LINTEC Co.; Ltd.";
                case 401: return "Hitachi Cable Ltd.";
                case 402: return "BUSWARE Direct";
                case 403: return "Eaton Electric B.V. (former Holec Holland N.V.)";
                case 404: return "VAT Vakuumventile AG";
                case 405: return "Scientific Technologies Incorporated";
                case 406: return "Alfa Instrumentos Eletronicos Ltda";
                case 407: return "TWK Elektronik GmbH";
                case 408: return "ABB Welding Systems AB";
                case 409: return "BYSTRONIC Maschinen AG";
                case 410: return "Kimura Electric Co.; Ltd";
                case 411: return "Nissei Plastic Industrial Co.; Ltd";
                case 412: return "Reserved";
                case 413: return "Kistler-Morse Corporation";
                case 414: return "Proteous Industries Inc.";
                case 415: return "IDC Corporation";
                case 416: return "Nordson Corporation";
                case 417: return "Rapistan Systems";
                case 418: return "LP-Elektronik GmbH";
                case 419: return "GERBI & FASE S.p.A.(Fase Saldatura)";
                case 420: return "Phoenix Digital Corporation";
                case 421: return "Z-World Engineering";
                case 422: return "Honda R&D Co.; Ltd.";
                case 423: return "Bionics Instrument Co.; Ltd.";
                case 424: return "Teknic; Inc.";
                case 425: return "R.Stahl; Inc.";
                case 426: return "Reserved";
                case 427: return "Ryco Graphic Manufacturing Inc.";
                case 428: return "Giddings & Lewis; Inc.";
                case 429: return "Koganei Corporation";
                case 430: return "Reserved";
                case 431: return "Nichigoh Communication Electric Wire Co.; Ltd.";
                case 432: return "Reserved";
                case 433: return "Fujikura Ltd.";
                case 434: return "AD Link Technology Inc.";
                case 435: return "StoneL Corporation";
                case 436: return "Computer Optical Products; Inc.";
                case 437: return "CONOS Inc.";
                case 438: return "Erhardt + Leimer GmbH";
                case 439: return "UNIQUE Co. Ltd";
                case 440: return "Roboticsware; Inc.";
                case 441: return "Nachi Fujikoshi Corporation";
                case 442: return "Hengstler GmbH";
                case 443: return "Reserved";
                case 444: return "SUNNY GIKEN Inc.";
                case 445: return "Lenze Drive Systems GmbH";
                case 446: return "CD Systems B.V.";
                case 447: return "FMT/Aircraft Gate Support Systems AB";
                case 448: return "Axiomatic Technologies Corp";
                case 449: return "Embedded System Products; Inc.";
                case 450: return "Reserved";
                case 451: return "Mencom Corporation";
                case 452: return "Reserved";
                case 453: return "Matsushita Welding Systems Co.; Ltd.";
                case 454: return "Dengensha Mfg. Co. Ltd.";
                case 455: return "Quinn Systems Ltd.";
                case 456: return "Tellima Technology Ltd";
                case 457: return "MDT; Software";
                case 458: return "Taiwan Keiso Co.; Ltd";
                case 459: return "Pinnacle Systems";
                case 460: return "Ascom Hasler Mailing Sys";
                case 461: return "INSTRUMAR Limited";
                case 462: return "Reserved";
                case 463: return "Navistar International Transportation Corp";
                case 464: return "Huettinger Elektronik GmbH + Co. KG";
                case 465: return "OCM Technology Inc.";
                case 466: return "Professional Supply Inc.";
                case 468: return "Baumer IVO GmbH & Co. KG";
                case 469: return "Worcester Controls Corporation";
                case 470: return "Pyramid Technical Consultants; Inc.";
                case 471: return "Reserved";
                case 472: return "Apollo Fire Detectors Limited";
                case 473: return "Avtron Manufacturing; Inc.";
                case 474: return "Reserved";
                case 475: return "Tokyo Keiso Co.; Ltd.";
                case 476: return "Daishowa Swiki Co.; Ltd.";
                case 477: return "Kojima Instruments Inc.";
                case 478: return "Shimadzu Corporation";
                case 479: return "Tatsuta Electric Wire & Cable Co.; Ltd.";
                case 480: return "MECS Corporation";
                case 481: return "Tahara Electric";
                case 482: return "Koyo Electronics";
                case 483: return "Clever Devices";
                case 484: return "GCD Hardware & Software GmbH";
                case 485: return "Reserved";
                case 486: return "Miller Electric Mfg Co.";
                case 487: return "GEA Tuchenhagen GmbH";
                case 488: return "Riken Keiki Co.; LTD";
                case 489: return "Keisokugiken Corporation";
                case 490: return "Fuji Machine Mfg. Co.; Ltd";
                case 491: return "Reserved";
                case 492: return "Nidec-Shimpo Corp.";
                case 493: return "UTEC Corporation";
                case 494: return "Sanyo Electric Co. Ltd.";
                case 495: return "Reserved";
                case 496: return "Reserved";
                case 497: return "Okano Electric Wire Co. Ltd";
                case 498: return "Shimaden Co. Ltd.";
                case 499: return "Teddington Controls Ltd";
                case 500: return "Reserved";
                case 501: return "VIPA GmbH";
                case 502: return "Warwick Manufacturing Group";
                case 503: return "Danaher Controls";
                case 504: return "Reserved";
                case 505: return "Reserved";
                case 506: return "American Science & Engineering";
                case 507: return "Accutron Controls International Inc.";
                case 508: return "Norcott Technologies Ltd";
                case 509: return "TB Woods; Inc";
                case 510: return "Proportion-Air; Inc.";
                case 511: return "SICK Stegmann GmbH";
                case 512: return "Reserved";
                case 513: return "Edwards Signaling";
                case 514: return "Sumitomo Metal Industries; Ltd";
                case 515: return "Cosmo Instruments Co.; Ltd.";
                case 516: return "Denshosha Co.; Ltd.";
                case 517: return "Kaijo Corp.";
                case 518: return "Michiproducts Co.; Ltd.";
                case 519: return "Miura Corporation";
                case 520: return "TG Information Network Co.; Ltd.";
                case 521: return "Fujikin ; Inc.";
                case 522: return "Estic Corp.";
                case 523: return "GS Hydraulic Sales";
                case 524: return "Reserved";
                case 525: return "MTE Limited";
                case 526: return "Hyde Park Electronics; Inc.";
                case 527: return "Pfeiffer Vacuum GmbH";
                case 528: return "Cyberlogic Technologies";
                case 529: return "OKUMA Corporation FA Systems Division";
                case 530: return "Reserved";
                case 531: return "Hitachi Kokusai Electric Co.; Ltd.";
                case 532: return "SHINKO TECHNOS Co.; Ltd.";
                case 533: return "Itoh Electric Co.; Ltd.";
                case 534: return "Colorado Flow Tech Inc.";
                case 535: return "Love Controls Division/Dwyer Inst.";
                case 536: return "Alstom Drives and Controls";
                case 537: return "The Foxboro Company";
                case 538: return "Tescom Corporation";
                case 539: return "Reserved";
                case 540: return "Atlas Copco Controls UK";
                case 541: return "Reserved";
                case 542: return "Autojet Technologies";
                case 543: return "Prima Electronics S.p.A.";
                case 544: return "PMA GmbH";
                case 545: return "Shimafuji Electric Co.; Ltd";
                case 546: return "Oki Electric Industry Co.; Ltd";
                case 547: return "Kyushu Matsushita Electric Co.; Ltd";
                case 548: return "Nihon Electric Wire & Cable Co.; Ltd";
                case 549: return "Tsuken Electric Ind Co.; Ltd";
                case 550: return "Tamadic Co.";
                case 551: return "MAATEL SA";
                case 552: return "OKUMA America";
                case 554: return "TPC Wire & Cable";
                case 555: return "ATI Industrial Automation";
                case 557: return "Serra Soldadura; S.A.";
                case 558: return "Southwest Research Institute";
                case 559: return "Cabinplant International";
                case 560: return "Sartorius Mechatronics T&H GmbH";
                case 561: return "Comau S.p.A. Robotics & Final Assembly Division";
                case 562: return "Phoenix Contact";
                case 563: return "Yokogawa MAT Corporation";
                case 564: return "asahi sangyo co.; ltd.";
                case 565: return "Reserved";
                case 566: return "Akita Myotoku Ltd.";
                case 567: return "OBARA Corp.";
                case 568: return "Suetron Electronic GmbH";
                case 569: return "Reserved";
                case 570: return "Serck Controls Limited";
                case 571: return "Fairchild Industrial Products Company";
                case 572: return "ARO S.A.";
                case 573: return "M2C GmbH";
                case 574: return "Shin Caterpillar Mitsubishi Ltd.";
                case 575: return "Santest Co.; Ltd.";
                case 576: return "Cosmotechs Co.; Ltd.";
                case 577: return "Hitachi Electric Systems";
                case 578: return "Smartscan Ltd";
                case 579: return "Woodhead Software & Electronics France";
                case 580: return "Athena Controls; Inc.";
                case 581: return "Syron Engineering & Manufacturing; Inc.";
                case 582: return "Asahi Optical Co.; Ltd.";
                case 583: return "Sansha Electric Mfg. Co.; Ltd.";
                case 584: return "Nikki Denso Co.; Ltd.";
                case 585: return "Star Micronics; Co.; Ltd.";
                case 586: return "Ecotecnia Socirtat Corp.";
                case 587: return "AC Technology Corp.";
                case 588: return "West Instruments Limited";
                case 589: return "NTI Limited";
                case 590: return "Delta Computer Systems; Inc.";
                case 591: return "FANUC Ltd.";
                case 592: return "Hearn-Gu Lee";
                case 593: return "ABB Automation Products";
                case 594: return "Orion Machinery Co.; Ltd.";
                case 595: return "Reserved";
                case 596: return "Wire-Pro; Inc.";
                case 597: return "Beijing Huakong Technology Co. Ltd.";
                case 598: return "Yokoyama Shokai Co.; Ltd.";
                case 599: return "Toyogiken Co.; Ltd.";
                case 600: return "Coester Equipamentos Eletronicos Ltda.";
                case 601: return "Reserved";
                case 602: return "Electroplating Engineers of Japan Ltd.";
                case 603: return "ROBOX S.p.A.";
                case 604: return "Spraying Systems Company";
                case 605: return "Benshaw Inc.";
                case 606: return "ZPA-DP A.S.";
                case 607: return "Wired Rite Systems";
                case 608: return "Tandis Research; Inc.";
                case 609: return "SSD Drives GmbH";
                case 610: return "ULVAC Japan Ltd.";
                case 611: return "DYNAX Corporation";
                case 612: return "Nor-Cal Products; Inc.";
                case 613: return "Aros Electronics AB";
                case 614: return "Jun-Tech Co.; Ltd.";
                case 615: return "HAN-MI Co. Ltd.";
                case 616: return "uniNtech (formerly SungGi Internet)";
                case 617: return "Hae Pyung Electronics Reserch Institute";
                case 618: return "Milwaukee Electronics";
                case 619: return "OBERG Industries";
                case 620: return "Parker Hannifin/Compumotor Division";
                case 621: return "TECHNO DIGITAL CORPORATION";
                case 622: return "Network Supply Co.; Ltd.";
                case 623: return "Union Electronics Co.; Ltd.";
                case 624: return "Tritronics Services PM Ltd.";
                case 625: return "Rockwell Automation-Sprecher+Schuh";
                case 626: return "Matsushita Electric Industrial Co.; Ltd/Motor Co.";
                case 627: return "Rolls-Royce Energy Systems; Inc.";
                case 628: return "JEONGIL INTERCOM CO.; LTD";
                case 629: return "Interroll Corp.";
                case 630: return "Hubbell Wiring Device-Kellems (Delaware)";
                case 631: return "Intelligent Motion Systems";
                case 632: return "Reserved";
                case 633: return "INFICON AG";
                case 634: return "Hirschmann; Inc.";
                case 635: return "The Siemon Company";
                case 636: return "YAMAHA Motor Co. Ltd.";
                case 637: return "aska corporation";
                case 638: return "Woodhead Connectivity";
                case 639: return "Trimble AB";
                case 640: return "Murrelektronik GmbH";
                case 641: return "Creatrix Labs; Inc.";
                case 642: return "TopWorx";
                case 643: return "Kumho Industrial Co.; Ltd.";
                case 644: return "Wind River Systems; Inc.";
                case 645: return "Bihl & Wiedemann GmbH";
                case 646: return "Harmonic Drive Systems Inc.";
                case 647: return "Rikei Corporation";
                case 648: return "BL Autotec; Ltd.";
                case 649: return "Hana Information & Technology Co.; Ltd.";
                case 650: return "Seoil Electric Co.; Ltd.";
                case 651: return "Fife Corporation";
                case 652: return "Shanghai Electrical Apparatus Research Institute";
                case 653: return "Reserved";
                case 654: return "Parasense Development Centre";
                case 655: return "Reserved";
                case 656: return "Reserved";
                case 657: return "Six Tau S.p.A.";
                case 658: return "Aucos GmbH";
                case 659: return "Rotork Controls";
                case 660: return "Automationdirect.com";
                case 661: return "Thermo BLH";
                case 662: return "System Controls; Ltd.";
                case 663: return "Univer S.p.A.";
                case 664: return "MKS-Tenta Technology";
                case 665: return "Lika Electronic SNC";
                case 666: return "Mettler-Toledo; Inc.";
                case 667: return "DXL USA Inc.";
                case 668: return "Rockwell Automation/Entek IRD Intl.";
                case 669: return "Nippon Otis Elevator Company";
                case 670: return "Sinano Electric; Co.; Ltd.";
                case 671: return "Sony Manufacturing Systems";
                case 672: return "Reserved";
                case 673: return "Contec Co.; Ltd.";
                case 674: return "Automated Solutions";
                case 675: return "Controlweigh";
                case 676: return "Reserved";
                case 677: return "Fincor Electronics";
                case 678: return "Cognex Corporation";
                case 679: return "Qualiflow";
                case 680: return "Weidmuller; Inc.";
                case 681: return "Morinaga Milk Industry Co.; Ltd.";
                case 682: return "Takagi Industrial Co.; Ltd.";
                case 683: return "Wittenstein AG";
                case 684: return "Sena Technologies; Inc.";
                case 685: return "Reserved";
                case 686: return "APV Products Unna";
                case 687: return "Creator Teknisk Utvedkling AB";
                case 688: return "Reserved";
                case 689: return "Mibu Denki Industrial Co.; Ltd.";
                case 690: return "Takamastsu Machineer Section";
                case 691: return "Startco Engineering Ltd.";
                case 692: return "Reserved";
                case 693: return "Holjeron";
                case 694: return "ALCATEL High Vacuum Technology";
                case 695: return "Taesan LCD Co.; Ltd.";
                case 696: return "POSCON";
                case 697: return "VMIC";
                case 698: return "Matsushita Electric Works; Ltd.";
                case 699: return "IAI Corporation";
                case 700: return "Horst GmbH";
                case 701: return "MicroControl GmbH & Co.";
                case 702: return "Leine & Linde AB";
                case 703: return "Reserved";
                case 704: return "EC Elettronica Srl";
                case 705: return "VIT Software HB";
                case 706: return "Bronkhorst High-Tech B.V.";
                case 707: return "Optex Co.; Ltd.";
                case 708: return "Yosio Electronic Co.";
                case 709: return "Terasaki Electric Co.; Ltd.";
                case 710: return "Sodick Co.; Ltd.";
                case 711: return "MTS Systems Corporation-Automation Division";
                case 712: return "Mesa Systemtechnik";
                case 713: return "SHIN HO SYSTEM Co.; Ltd.";
                case 714: return "Goyo Electronics Co; Ltd.";
                case 715: return "Loreme";
                case 716: return "SAB Brockskes GmbH & Co. KG";
                case 717: return "Trumpf Laser GmbH + Co. KG";
                case 718: return "Niigata Electronic Instruments Co.; Ltd.";
                case 719: return "Yokogawa Digital Computer Corporation";
                case 720: return "O.N. Electronic Co.; Ltd.";
                case 721: return "Industrial Control	Communication; Inc.";
                case 722: return "ABB; Inc.";
                case 723: return "ElectroWave USA; Inc.";
                case 724: return "Industrial Network Controls; LLC";
                case 725: return "KDT Systems Co.; Ltd.";
                case 726: return "SEFA Technology Inc.";
                case 727: return "Nippon POP Rivets and Fasteners Ltd.";
                case 728: return "Yamato Scale Co.; Ltd.";
                case 729: return "Zener Electric";
                case 730: return "GSE Scale Systems";
                case 731: return "ISAS (Integrated Switchgear & Sys. Pty Ltd)";
                case 732: return "Beta LaserMike Limited";
                case 733: return "TOEI Electric Co.; Ltd.";
                case 734: return "Hakko Electronics Co.; Ltd";
                case 735: return "Reserved";
                case 736: return "RFID; Inc.";
                case 737: return "Adwin Corporation";
                case 738: return "Osaka Vacuum; Ltd.";
                case 739: return "A-Kyung Motion; Inc.";
                case 740: return "Camozzi S.P. A.";
                case 741: return "Crevis Co.; LTD";
                case 742: return "Rice Lake Weighing Systems";
                case 743: return "Linux Network Services";
                case 744: return "KEB Antriebstechnik GmbH";
                case 745: return "Hagiwara Electric Co.; Ltd.";
                case 746: return "Glass Inc. International";
                case 747: return "Reserved";
                case 748: return "DVT Corporation";
                case 749: return "Woodward Governor";
                case 750: return "Mosaic Systems; Inc.";
                case 751: return "Laserline GmbH";
                case 752: return "COM-TEC; Inc.";
                case 753: return "Weed Instrument";
                case 754: return "Prof-face European Technology Center";
                case 755: return "Fuji Automation Co.; Ltd.";
                case 756: return "Matsutame Co.; Ltd.";
                case 757: return "Hitachi Via Mechanics; Ltd.";
                case 758: return "Dainippon Screen Mfg. Co. Ltd.";
                case 759: return "FLS Automation A/S";
                case 760: return "ABB Stotz Kontakt GmbH";
                case 761: return "Technical Marine Service";
                case 762: return "Advanced Automation Associates; Inc.";
                case 763: return "Baumer Ident GmbH";
                case 764: return "Tsubakimoto Chain Co.";
                case 765: return "Reserved";
                case 766: return "Furukawa Co.; Ltd.";
                case 767: return "Active Power";
                case 768: return "CSIRO Mining Automation";
                case 769: return "Matrix Integrated Systems";
                case 770: return "Digitronic Automationsanlagen GmbH";
                case 771: return "SICK STEGMANN Inc.";
                case 772: return "TAE-Antriebstechnik GmbH";
                case 773: return "Electronic Solutions";
                case 774: return "Rocon L.L.C.";
                case 775: return "Dijitized Communications Inc.";
                case 776: return "Asahi Organic Chemicals Industry Co.; Ltd.";
                case 777: return "Hodensha";
                case 778: return "Harting; Inc. NA";
                case 779: return "Kubler GmbH";
                case 780: return "Yamatake Corporation";
                case 781: return "JEOL";
                case 782: return "Yamatake Industrial Systems Co.; Ltd.";
                case 783: return "HAEHNE Elektronische Messgerate GmbH";
                case 784: return "Ci Technologies Pty Ltd (for Pelamos Industries)";
                case 785: return "N. SCHLUMBERGER & CIE";
                case 786: return "Teijin Seiki Co.; Ltd.";
                case 787: return "DAIKIN Industries; Ltd";
                case 788: return "RyuSyo Industrial Co.; Ltd.";
                case 789: return "SAGINOMIYA SEISAKUSHO; INC.";
                case 790: return "Seishin Engineering Co.; Ltd.";
                case 791: return "Japan Support System Ltd.";
                case 792: return "Decsys";
                case 793: return "Metronix Messgerate u. Elektronik GmbH";
                case 794: return "Reserved";
                case 795: return "Vaccon Company; Inc.";
                case 796: return "Siemens Energy & Automation; Inc.";
                case 797: return "Ten X Technology; Inc.";
                case 798: return "Tyco Electronics";
                case 799: return "Delta Power Electronics Center";
                case 800: return "Denker";
                case 801: return "Autonics Corporation";
                case 802: return "JFE Electronic Engineering Pty. Ltd.";
                case 803: return "Reserved";
                case 804: return "Electro-Sensors; Inc.";
                case 805: return "Digi International; Inc.";
                case 806: return "Texas Instruments";
                case 807: return "ADTEC Plasma Technology Co.; Ltd";
                case 808: return "SICK AG";
                case 809: return "Ethernet Peripherals; Inc.";
                case 810: return "Animatics Corporation";
                case 811: return "Reserved";
                case 812: return "Process Control Corporation";
                case 813: return "SystemV. Inc.";
                case 814: return "Danaher Motion SRL";
                case 815: return "SHINKAWA Sensor Technology; Inc.";
                case 816: return "Tesch GmbH & Co. KG";
                case 817: return "Reserved";
                case 818: return "Trend Controls Systems Ltd.";
                case 819: return "Guangzhou ZHIYUAN Electronic Co.; Ltd.";
                case 820: return "Mykrolis Corporation";
                case 821: return "Bethlehem Steel Corporation";
                case 822: return "KK ICP";
                case 823: return "Takemoto Denki Corporation";
                case 824: return "The Montalvo Corporation";
                case 825: return "Reserved";
                case 826: return "LEONI Special Cables GmbH";
                case 827: return "Reserved";
                case 828: return "ONO SOKKI CO.;LTD.";
                case 829: return "Rockwell Samsung Automation";
                case 830: return "SHINDENGEN ELECTRIC MFG. CO. LTD";
                case 831: return "Origin Electric Co. Ltd.";
                case 832: return "Quest Technical Solutions; Inc.";
                case 833: return "LS Cable; Ltd.";
                case 834: return "Enercon-Nord Electronic GmbH";
                case 835: return "Northwire Inc.";
                case 836: return "Engel Elektroantriebe GmbH";
                case 837: return "The Stanley Works";
                case 838: return "Celesco Transducer Products; Inc.";
                case 839: return "Chugoku Electric Wire and Cable Co.";
                case 840: return "Kongsberg Simrad AS";
                case 841: return "Panduit Corporation";
                case 842: return "Spellman High Voltage Electronics Corp.";
                case 843: return "Kokusai Electric Alpha Co.; Ltd.";
                case 844: return "Brooks Automation; Inc.";
                case 845: return "ANYWIRE CORPORATION";
                case 846: return "Honda Electronics Co. Ltd";
                case 847: return "REO Elektronik AG";
                case 848: return "Fusion UV Systems; Inc.";
                case 849: return "ASI Advanced Semiconductor Instruments GmbH";
                case 850: return "Datalogic; Inc.";
                case 851: return "SoftPLC Corporation";
                case 852: return "Dynisco Instruments LLC";
                case 853: return "WEG Industrias SA";
                case 854: return "Frontline Test Equipment; Inc.";
                case 855: return "Tamagawa Seiki Co.; Ltd.";
                case 856: return "Multi Computing Co.; Ltd.";
                case 857: return "RVSI";
                case 858: return "Commercial Timesharing Inc.";
                case 859: return "Tennessee Rand Automation LLC";
                case 860: return "Wacogiken Co.; Ltd";
                case 861: return "Reflex Integration Inc.";
                case 862: return "Siemens AG; A&D PI Flow Instruments";
                case 863: return "G. Bachmann Electronic GmbH";
                case 864: return "NT International";
                case 865: return "Schweitzer Engineering Laboratories";
                case 866: return "ATR Industrie-Elektronik GmbH Co.";
                case 867: return "PLASMATECH Co.; Ltd";
                case 868: return "Reserved";
                case 869: return "GEMU GmbH & Co. KG";
                case 870: return "Alcorn McBride Inc.";
                case 871: return "MORI SEIKI CO.; LTD";
                case 872: return "NodeTech Systems Ltd";
                case 873: return "Emhart Teknologies";
                case 874: return "Cervis; Inc.";
                case 875: return "FieldServer Technologies (Div Sierra Monitor Corp)";
                case 876: return "NEDAP Power Supplies";
                case 877: return "Nippon Sanso Corporation";
                case 878: return "Mitomi Giken Co.; Ltd.";
                case 879: return "PULS GmbH";
                case 880: return "Reserved";
                case 881: return "Japan Control Engineering Ltd";
                case 882: return "Embedded Systems Korea (Former Zues Emtek Co Ltd.)";
                case 883: return "Automa SRL";
                case 884: return "Harms+Wende GmbH & Co KG";
                case 885: return "SAE-STAHL GmbH";
                case 886: return "Microwave Data Systems";
                case 887: return "Bernecker + Rainer Industrie-Elektronik GmbH";
                case 888: return "Hiprom Technologies";
                case 889: return "Reserved";
                case 890: return "Nitta Corporation";
                case 891: return "Kontron Modular Computers GmbH";
                case 892: return "Marlin Controls";
                case 893: return "ELCIS s.r.l.";
                case 894: return "Acromag; Inc.";
                case 895: return "Avery Weigh-Tronix";
                case 896: return "Reserved";
                case 897: return "Reserved";
                case 898: return "Reserved";
                case 899: return "Practicon Ltd";
                case 900: return "Schunk GmbH & Co. KG";
                case 901: return "MYNAH Technologies";
                case 902: return "Defontaine Groupe";
                case 903: return "Emerson Process Management Power & Water Solutions";
                case 904: return "F.A. Elec";
                case 905: return "Hottinger Baldwin Messtechnik GmbH";
                case 906: return "Coreco Imaging; Inc.";
                case 907: return "London Electronics Ltd.";
                case 908: return "HSD SpA";
                case 909: return "Comtrol Corporation";
                case 910: return "TEAM; S.A. (Tecnica Electronica de Automatismo Y Medida)";
                case 911: return "MAN B&W Diesel Ltd. Regulateurs Europa";
                case 912: return "Reserved";
                case 913: return "Reserved";
                case 914: return "Micro Motion; Inc.";
                case 915: return "Eckelmann AG";
                case 916: return "Hanyoung Nux";
                case 917: return "Ransburg Industrial Finishing KK";
                case 918: return "Kun Hung Electric Co. Ltd.";
                case 919: return "Brimos wegbebakening b.v.";
                case 920: return "Nitto Seiki Co.; Ltd";
                case 921: return "PPT Vision; Inc.";
                case 922: return "Yamazaki Machinery Works";
                case 923: return "SCHMIDT Technology GmbH";
                case 924: return "Parker Hannifin SpA (SBC Division)";
                case 925: return "HIMA Paul Hildebrandt GmbH";
                case 926: return "RivaTek; Inc.";
                case 927: return "Misumi Corporation";
                case 928: return "GE Multilin";
                case 929: return "Measurement Computing Corporation";
                case 930: return "Jetter AG";
                case 931: return "Tokyo Electronics Systems Corporation";
                case 932: return "Togami Electric Mfg. Co.; Ltd.";
                case 933: return "HK Systems";
                case 934: return "CDA Systems Ltd.";
                case 935: return "Aerotech Inc.";
                case 936: return "JVL Industrie Elektronik A/S";
                case 937: return "NovaTech Process Solutions LLC";
                case 938: return "Reserved";
                case 939: return "Cisco Systems";
                case 940: return "Grid Connect";
                case 941: return "ITW Automotive Finishing";
                case 942: return "HanYang System";
                case 943: return "ABB K.K. Technical Center";
                case 944: return "Taiyo Electric Wire & Cable Co.; Ltd.";
                case 945: return "Reserved";
                case 946: return "SEREN IPS INC";
                case 947: return "Belden CDT Electronics Division";
                case 948: return "ControlNet International";
                case 949: return "Gefran S.P.A.";
                case 950: return "Jokab Safety AB";
                case 951: return "SUMITA OPTICAL GLASS; INC.";
                case 952: return "Biffi Italia srl";
                case 953: return "Beck IPC GmbH";
                case 954: return "Copley Controls Corporation";
                case 955: return "Fagor Automation S. Coop.";
                case 956: return "DARCOM";
                case 957: return "Frick Controls (div. of York International)";
                case 958: return "SymCom; Inc.";
                case 959: return "Infranor";
                case 960: return "Kyosan Cable; Ltd.";
                case 961: return "Varian Vacuum Technologies";
                case 962: return "Messung Systems";
                case 963: return "Xantrex Technology; Inc.";
                case 964: return "StarThis Inc.";
                case 965: return "Chiyoda Co.; Ltd.";
                case 966: return "Flowserve Corporation";
                case 967: return "Spyder Controls Corp.";
                case 968: return "IBA AG";
                case 969: return "SHIMOHIRA ELECTRIC MFG.CO.;LTD";
                case 970: return "Reserved";
                case 971: return "Siemens L&A";
                case 972: return "Micro Innovations AG";
                case 973: return "Switchgear & Instrumentation";
                case 974: return "PRE-TECH CO.; LTD.";
                case 975: return "National Semiconductor";
                case 976: return "Invensys Process Systems";
                case 977: return "Ametek HDR Power Systems";
                case 978: return "Reserved";
                case 979: return "TETRA-K Corporation";
                case 980: return "C & M Corporation";
                case 981: return "Siempelkamp Maschinen";
                case 982: return "Reserved";
                case 983: return "Daifuku America Corporation";
                case 984: return "Electro-Matic Products Inc.";
                case 985: return "BUSSAN MICROELECTRONICS CORP.";
                case 986: return "ELAU AG";
                case 987: return "Hetronic USA";
                case 988: return "NIIGATA POWER SYSTEMS Co.; Ltd.";
                case 989: return "Software Horizons Inc.";
                case 990: return "B3 Systems; Inc.";
                case 991: return "Moxa Networking Co.; Ltd.";
                case 992: return "Reserved";
                case 993: return "S4 Integration";
                case 994: return "Elettro Stemi S.R.L.";
                case 995: return "AquaSensors";
                case 996: return "Ifak System GmbH";
                case 997: return "SANKEI MANUFACTURING Co.;LTD.";
                case 998: return "Emerson Network Power Co.; Ltd.";
                case 999: return "Fairmount Automation; Inc.";
                case 1000: return "Bird Electronic Corporation";
                case 1001: return "Nabtesco Corporation";
                case 1002: return "AGM Electronics; Inc.";
                case 1003: return "ARCX Inc.";
                case 1004: return "DELTA I/O Co.";
                case 1005: return "Chun IL Electric Ind. Co.";
                case 1006: return "N-Tron";
                case 1007: return "Nippon Pneumatics/Fludics System CO.;LTD.";
                case 1008: return "DDK Ltd.";
                case 1009: return "Seiko Epson Corporation";
                case 1010: return "Halstrup-Walcher GmbH";
                case 1011: return "ITT";
                case 1012: return "Ground Fault Systems bv";
                case 1013: return "Scolari Engineering S.p.A.";
                case 1014: return "Vialis Traffic bv";
                case 1015: return "Weidmueller Interface GmbH & Co. KG";
                case 1016: return "Shanghai Sibotech Automation Co. Ltd";
                case 1017: return "AEG Power Supply Systems GmbH";
                case 1018: return "Komatsu Electronics Inc.";
                case 1019: return "Souriau";
                case 1020: return "Baumuller Chicago Corp.";
                case 1021: return "J. Schmalz GmbH";
                case 1022: return "SEN Corporation";
                case 1023: return "Korenix Technology Co. Ltd";
                case 1024: return "Cooper Power Tools";
                case 1025: return "INNOBIS";
                case 1026: return "Shinho System";
                case 1027: return "Xm Services Ltd.";
                case 1028: return "KVC Co.; Ltd.";
                case 1029: return "Sanyu Seiki Co.; Ltd.";
                case 1030: return "TuxPLC";
                case 1031: return "Northern Network Solutions";
                case 1032: return "Converteam GmbH";
                case 1033: return "Symbol Technologies";
                case 1034: return "S-TEAM Lab";
                case 1035: return "Maguire Products; Inc.";
                case 1036: return "AC&T";
                case 1037: return "MITSUBISHI HEAVY INDUSTRIES; LTD. KOBE SHIPYARD & MACHINERY WORKS";
                case 1038: return "Hurletron Inc.";
                case 1039: return "Chunichi Denshi Co.; Ltd";
                case 1040: return "Cardinal Scale Mfg. Co.";
                case 1041: return "BTR NETCOM via RIA Connect; Inc.";
                case 1042: return "Base2";
                case 1043: return "ASRC Aerospace";
                case 1044: return "Beijing Stone Automation";
                case 1045: return "Changshu Switchgear Manufacture Ltd.";
                case 1046: return "METRONIX Corp.";
                case 1047: return "WIT";
                case 1048: return "ORMEC Systems Corp.";
                case 1049: return "ASATech (China) Inc.";
                case 1050: return "Controlled Systems Limited";
                case 1051: return "Mitsubishi Heavy Ind. Digital System Co.; Ltd. (M.H.I.)";
                case 1052: return "Electrogrip";
                case 1053: return "TDS Automation";
                case 1054: return "T&C Power Conversion; Inc.";
                case 1055: return "Robostar Co.; Ltd";
                case 1056: return "Scancon A/S";
                case 1057: return "Haas Automation; Inc.";
                case 1058: return "Eshed Technology";
                case 1059: return "Delta Electronic Inc.";
                case 1060: return "Innovasic Semiconductor";
                case 1061: return "SoftDEL Systems Limited";
                case 1062: return "FiberFin; Inc.";
                case 1063: return "Nicollet Technologies Corp.";
                case 1064: return "B.F. Systems";
                case 1065: return "Empire Wire and Supply LLC";
                case 1066: return "Reserved";
                case 1067: return "Elmo Motion Control LTD";
                case 1068: return "Reserved";
                case 1069: return "Asahi Keiki Co.; Ltd.";
                case 1070: return "Joy Mining Machinery";
                case 1071: return "MPM Engineering Ltd";
                case 1072: return "Wolke Inks & Printers GmbH";
                case 1073: return "Mitsubishi Electric Engineering Co.; Ltd.";
                case 1074: return "COMET AG";
                case 1075: return "Real Time Objects & Systems; LLC";
                case 1076: return "MISCO Refractometer";
                case 1077: return "JT Engineering Inc.";
                case 1078: return "Automated Packing Systems";
                case 1079: return "Niobrara R&D Corp.";
                case 1080: return "Garmin Ltd.";
                case 1081: return "Japan Mobile Platform Co.; Ltd";
                case 1082: return "Advosol Inc.";
                case 1083: return "ABB Global Services Limited";
                case 1084: return "Sciemetric Instruments Inc.";
                case 1085: return "Tata Elxsi Ltd.";
                case 1086: return "TPC Mechatronics; Co.; Ltd.";
                case 1087: return "Cooper Bussmann";
                case 1088: return "Trinite Automatisering B.V.";
                case 1089: return "Peek Traffic B.V.";
                case 1090: return "Acrison; Inc";
                case 1091: return "Applied Robotics; Inc.";
                case 1092: return "FireBus Systems; Inc.";
                case 1093: return "Beijing Sevenstar Huachuang Electronics";
                case 1094: return "Magnetek";
                case 1095: return "Microscan";
                case 1096: return "Air Water Inc.";
                case 1097: return "Sensopart Industriesensorik GmbH";
                case 1098: return "Tiefenbach Control Systems GmbH";
                case 1099: return "INOXPA S.A";
                case 1100: return "Zurich University of Applied Sciences";
                case 1101: return "Ethernet Direct";
                case 1102: return "GSI-Micro-E Systems";
                case 1103: return "S-Net Automation Co.; Ltd.";
                case 1104: return "Power Electronics S.L.";
                case 1105: return "Renesas Technology Corp.";
                case 1106: return "NSWCCD-SSES";
                case 1107: return "Porter Engineering Ltd.";
                case 1108: return "Meggitt Airdynamics; Inc.";
                case 1109: return "Inductive Automation";
                case 1110: return "Neural ID";
                case 1111: return "EEPod LLC";
                case 1112: return "Hitachi Industrial Equipment Systems Co.; Ltd.";
                case 1113: return "Salem Automation";
                case 1114: return "port GmbH";
                case 1115: return "B & PLUS";
                case 1116: return "Graco Inc.";
                case 1117: return "Altera Corporation";
                case 1118: return "Technology Brewing Corporation";
                case 1121: return "CSE Servelec";
                case 1124: return "Fluke Networks";
                case 1125: return "Tetra Pak Packaging Solutions SPA";
                case 1126: return "Racine Federated; Inc.";
                case 1127: return "Pureron Japan Co.; Ltd.";
                case 1130: return "Brother Industries; Ltd.";
                case 1132: return "Leroy Automation";
                case 1137: return "TR-Electronic GmbH";
                case 1138: return "ASCON S.p.A.";
                case 1139: return "Toledo do Brasil Industria de Balancas Ltda.";
                case 1140: return "Bucyrus DBT Europe GmbH";
                case 1141: return "Emerson Process Management Valve Automation";
                case 1142: return "Alstom Transport";
                case 1144: return "Matrox Electronic Systems";
                case 1145: return "Littelfuse";
                case 1146: return "PLASMART; Inc.";
                case 1147: return "Miyachi Corporation";
                case 1150: return "Promess Incorporated";
                case 1151: return "COPA-DATA GmbH";
                case 1152: return "Precision Engine Controls Corporation";
                case 1153: return "Alga Automacao e controle LTDA";
                case 1154: return "U.I. Lapp GmbH";
                case 1155: return "ICES";
                case 1156: return "Philips Lighting bv";
                case 1157: return "Aseptomag AG";
                case 1158: return "ARC Informatique";
                case 1159: return "Hesmor GmbH";
                case 1160: return "Kobe Steel; Ltd.";
                case 1161: return "FLIR Systems";
                case 1162: return "Simcon A/S";
                case 1163: return "COPALP";
                case 1164: return "Zypcom; Inc.";
                case 1165: return "Swagelok";
                case 1166: return "Elspec";
                case 1167: return "ITT Water & Wastewater AB";
                case 1168: return "Kunbus GmbH Industrial Communication";
                case 1170: return "Performance Controls; Inc.";
                case 1171: return "ACS Motion Control; Ltd.";
                case 1173: return "IStar Technology Limited";
                case 1174: return "Alicat Scientific; Inc.";
                case 1176: return "ADFweb.com SRL";
                case 1177: return "Tata Consultancy Services Limited";
                case 1178: return "CXR Ltd.";
                case 1179: return "Vishay Nobel AB";
                case 1181: return "SolaHD";
                case 1182: return "Endress+Hauser";
                case 1183: return "Bartec GmbH";
                case 1185: return "AccuSentry; Inc.";
                case 1186: return "Exlar Corporation";
                case 1187: return "ILS Technology";
                case 1188: return "Control Concepts Inc.";
                case 1190: return "Procon Engineering Limited";
                case 1191: return "Hermary Opto Electronics Inc.";
                case 1192: return "Q-Lambda";
                case 1194: return "VAMP Ltd";
                case 1195: return "FlexLink";
                case 1196: return "Office FA.com Co.; Ltd.";
                case 1197: return "SPMC (Changzhou) Co. Ltd.";
                case 1198: return "Anton Paar GmbH";
                case 1199: return "Zhuzhou CSR Times Electric Co.; Ltd.";
                case 1200: return "DeStaCo";
                case 1201: return "Synrad; Inc";
                case 1202: return "Bonfiglioli Vectron GmbH";
                case 1203: return "Pivotal Systems";
                case 1204: return "TKSCT";
                case 1205: return "Randy Nuernberger";
                case 1206: return "CENTRALP";
                case 1207: return "Tengen Group";
                case 1208: return "OES; Inc.";
                case 1209: return "Actel Corporation";
                case 1210: return "Monaghan Engineering; Inc.";
                case 1211: return "wenglor sensoric gmbh";
                case 1212: return "HSA Systems";
                case 1213: return "MK Precision Co.; Ltd.";
                case 1214: return "Tappan Wire and Cable";
                case 1215: return "Heinzmann GmbH & Co. KG";
                case 1216: return "Process Automation International Ltd.";
                case 1217: return "Secure Crossing";
                case 1218: return "SMA Railway Technology GmbH";
                case 1219: return "FMS Force Measuring Systems AG";
                case 1220: return "ABT Endustri Enerji Sistemleri Sanayi Tic. Ltd. Sti.";
                case 1221: return "MagneMotion Inc.";
                case 1222: return "STS Co.; Ltd.";
                case 1223: return "MERAK SIC; SA";
                case 1224: return "ABOUNDI; Inc.";
                case 1225: return "Rosemount Inc.";
                case 1226: return "GEA FES; Inc.";
                case 1227: return "TMG Technologie und Engineering GmbH";
                case 1228: return "embeX GmbH";
                case 1229: return "GH Electrotermia; S.A.";
                case 1230: return "Tolomatic";
                case 1231: return "Dukane";
                case 1232: return "Elco (Tian Jin) Electronics Co.; Ltd.";
                case 1233: return "Jacobs Automation";
                case 1234: return "Noda Radio Frequency Technologies Co.; Ltd.";
                case 1235: return "MSC Tuttlingen GmbH";
                case 1236: return "Hitachi Cable Manchester";
                case 1237: return "ACOREL SAS";
                case 1238: return "Global Engineering Solutions Co.; Ltd.";
                case 1239: return "ALTE Transportation; S.L.";
                case 1240: return "Penko Engineering B.V.";
                default: return "Reserved";
            }
        }
        /* ======================================================================================== */
        #endregion
    }
}
