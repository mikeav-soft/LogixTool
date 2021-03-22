using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace EIP.EthernetIP.Net
{
    /// <summary>
    /// 
    /// </summary>
    internal class TcpSocket
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        /// <summary>
        /// Возвращает номер порта данного локального узла.
        /// </summary>
        public int LocalPort
        {
            get
            {
                if (this.tcpClient == null) return 0;
                return ((IPEndPoint)this.tcpClient.Client.LocalEndPoint).Port;
            }
        }
        /// <summary>
        /// Возвращает адрес данного локального узла.
        /// </summary>
        public IPAddress LocalAddress
        {
            get
            {
                if (this.tcpClient == null) return IPAddress.Any;
                return ((IPEndPoint)this.tcpClient.Client.LocalEndPoint).Address;
            }
        }
        /// <summary>
        /// Информирует о том что имеется подключение с удаленным устройством.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return tcpClient != null && tcpClient.Connected;
            }
        }

        /// <summary>
        /// Возвращает адрес узла с которым произвоедено подключение.
        /// </summary>
        public IPAddress TargetAddress { get; private set; }
        /// <summary>
        /// Возвращает порт узла с которым произвоедено подключение.
        /// </summary>
        public int TargetPort { get; private set; }
        /* ================================================================================================== */
        #endregion

        NetworkStream stream;                       //
        TcpClient tcpClient;                        //

        /// <summary>
        /// Создает новый TCP клиент.
        /// </summary>
        internal TcpSocket()
        {

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
        private void OnConnected()
        {
            if (this.Connected != null)
                this.Connected(this, null);
        }
        /* ================================================================================================== */
        #endregion


        #region [ PUBLIC METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Производит TCP подключение к удаленному устройству.
        /// </summary>
        /// <param name="endPoint">Конечный узел для подключения.</param>
        /// <returns>Возвращает True если данный метод отработал успешно.</returns>
        public bool Connect(IPEndPoint endPoint)
        {
            // Проверка входных параметров.
            if (endPoint == null)
                throw new ArgumentNullException("Method='SendRecieve', Parameter='endPoint' can not be Null.");

            if (this.IsConnected)
                return false;

            try
            {
                this.tcpClient = new TcpClient();
                this.tcpClient.ReceiveTimeout = 10000;
                this.tcpClient.SendTimeout = 10000;
                this.tcpClient.Connect(endPoint);

                if (this.IsConnected)
                {
                    this.TargetAddress = endPoint.Address;
                    this.TargetPort = endPoint.Port;

                    OnConnected();
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
                if (this.IsConnected)
                {
                    tcpClient.Close();
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
        /// Отправляет последовательность байт клиенту по протоколу TCP/IP и возвращает ожидаемый ответ.
        /// </summary>
        /// <param name="data">Последовательность байт для отправки.</param>
        /// <returns></returns>
        internal bool SendRecieve(byte[] data, out List<byte> response)
        {
            response = null;

            if (!this.IsConnected)
                return false;

            try
            {
                stream = tcpClient.GetStream();
                stream.Write(data, 0, data.Length);

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
        /// <param name="data">Последовательность байт для отправки.</param>
        /// <returns></returns>
        internal bool Send(byte[] data)
        {
            if (!this.IsConnected)
                return false;

            try
            {
                stream = tcpClient.GetStream();
                stream.Write(data, 0, data.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /* ================================================================================================== */
        #endregion
    }
}
