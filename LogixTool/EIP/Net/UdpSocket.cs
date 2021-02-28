using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace EIP.Net
{
    /// <summary>
    /// Представляет собой класс для работы с UDP подключением с указанием конкретного порта.
    /// Помогает прослушивать запросы от удаленных узлов по указанному конкретному порту.
    /// </summary>
    internal class UdpSocket
    {
        #region [ PUBLIC PROPERTIES ]
        /* ======================================================================================== */
        /// <summary>
        /// Возвращает номер порта данного локального узла.
        /// </summary>
        public int LocalPort
        {
            get
            {
                if (this.udpClient == null) return 0;
                return ((IPEndPoint)this.udpClient.Client.LocalEndPoint).Port;
            }
        }
        /// <summary>
        /// Возвращает адрес данного локального узла.
        /// </summary>
        public IPAddress LocalAddress
        {
            get
            {
                if (this.udpClient == null) return IPAddress.Any;
                return ((IPEndPoint)this.udpClient.Client.LocalEndPoint).Address;
            }
        }
        /// <summary>
        /// Возвращает True в случае если данное подключение активно, т.е. возможна отправка и прослушивание удаленных узлов.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this.udpClient != null
                    && this.udpThread != null
                    && (this.udpThread.ThreadState == ThreadState.Running
                    || this.udpThread.ThreadState == ThreadState.Background);
            }
        }
        /* ======================================================================================== */
        #endregion

        private UdpClient udpClient;    // Базовый UDP клиент с конкретным портом.
        private Thread udpThread;       // Фоновый поток для прослушивания входящих данных.
        private bool recieverEnabled;   // При значении True разрешает циклический процесс фонового прослушивания.
        private List<UdpResponse> replies; // Вспомогательный список ответов от удаленных узлов после отправленного запроса.

        /// <summary>
        /// Создает новый UDP клиент.
        /// Запускает процесс прослушивания конкретно заданного порта.
        /// </summary>
        /// <param name="port">Номер конкретного порта данного узла.</param>
        public UdpSocket(int port)
        {
            this.replies = new List<UdpResponse>();
            this.recieverEnabled = true;
            this.udpClient = new UdpClient(port);
            this.udpThread = new Thread(new ThreadStart(Process));
            this.udpThread.IsBackground = true;
            this.udpThread.Start();
        }
        /// <summary>
        /// Создает новый UDP клиент.
        /// Запускает процесс прослушивания порта выбираемого автоматически.
        /// </summary>
        public UdpSocket() : this(0) { }


        #region [ EVENTS ]
        /* ======================================================================================== */
        /// <summary>
        /// Возникает при поступлении новых данных от удаленных узлов.
        /// </summary>
        public event ClientReplyEventHandler DataRecieved;
        /// <summary>
        /// Вызывает событие при поступлении новых данных от удаленных узлов.
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="data"></param>
        private void OnDataRecieved(IPEndPoint ep, byte[] data)
        {
            if (this.DataRecieved != null)
                this.DataRecieved(this, new ClientReplyEventArgs(ep, data));
        }
        /* ======================================================================================== */
        #endregion

        #region [ PUBLIC METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Отправляет данные на указанный узел используя UDP протокол.
        /// </summary>
        /// <param name="data">Массив байт для отправки.</param>
        /// <param name="endPoint">Конечный узел для отправки.</param>
        /// <returns></returns>
        public bool Send(byte[] data, IPEndPoint endPoint)
        {
            // Проверка входных параметров.
            if (endPoint == null)
                throw new ArgumentNullException("Method='SendRecieve', Parameter='endPoint' can not be Null.");

            // Проверяем активен ли клиент.
            if (!this.IsActive)
                return false;

            try
            {
                udpClient.Send(data, data.Length, endPoint);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Производит запрос на удаленный узел с ожиданием ответа с заданным интервалом.
        /// Если запрос широковещательный то ожидание от всех узлов выдерживается с указанным временем тайматута.
        /// Если указан конкретный узел-получатель то ожидание производится до прлучения первого ответа 
        /// от данного узла но не более чем указанный интервал.
        /// </summary>
        /// <param name="data">Отправляемые данные.</param>
        /// <param name="endPoint">Конечный узел-получатель.</param>
        /// <param name="timeout">Таймаут ожидания ответа (миллисекунд).</param>
        /// <param name="responses">Результат: Полученные данные.</param>
        /// <returns></returns>
        public bool SendRecieve(byte[] data, IPEndPoint endPoint, int timeout, out List<UdpResponse> responses)
        {
            // Проверка входных параметров.
            if (endPoint == null)
                throw new ArgumentNullException("Method='SendRecieve', Parameter='endPoint' can not be Null.");

            if (timeout < 0)
                throw new ArgumentOutOfRangeException("Method='SendRecieve', Parameter='timeout' can not be less than 0.");

            // Инициализация переменных.
            responses = new List<UdpResponse>();
            replies.Clear();
            bool waiting = true;
            bool recieved = false;
            bool broadcast = endPoint.Address.Equals(IPAddress.Broadcast);
            long startPoint = DateTime.Now.Ticks;

            // Проверяем активен ли клиент.
            if (!this.IsActive)
                return false;

            // Отправляем данные на указанный узел.
            if (!this.Send(data, endPoint))
                return false;

            // Подписываемся на событие.
            this.DataRecieved += UdpSocket_DataRecieved;

            while (waiting)
            {
                // Производим проверку на заданный интервал ожидания.
                long time = DateTime.Now.Ticks;
                if (time < startPoint || (time - startPoint) / 10000 > timeout)
                {
                    waiting = false;

                    if (broadcast)
                    {
                        IEnumerable<UdpResponse> reply = replies.Where(r => r.EndPoint.Port == endPoint.Port);
                        responses.AddRange(reply);
                        recieved = reply.Count() > 0;
                    }
                }

                // Проверяем наличие данных от ожидаемых узлов.
                // Если запрос не широковещательный то ждем первого ответа от узла.
                if (replies.Count != 0)
                {
                    if (!broadcast)
                    {
                        IEnumerable<UdpResponse> reply = replies.Where(r =>
                        (r.EndPoint.Address.Equals(endPoint.Address))
                        && r.EndPoint.Port == endPoint.Port);

                        if (reply.Count() > 0)
                        {
                            responses.Add(reply.First());
                            waiting = false;
                            recieved = true;
                        }
                    }
                }
            }

            // Отписываемся от события.
            this.DataRecieved -= UdpSocket_DataRecieved;
            replies.Clear();

            return recieved;
        }
        /// <summary>
        /// Производит закрытие инициализированного ранее UDP клиента а также поток прослушивания порта.
        /// </summary>
        public void Close()
        {
            recieverEnabled = false;

            if (this.udpClient != null)
                this.udpClient.Close();

            if (this.replies != null)
                this.replies.Clear();

        }
        /* ======================================================================================== */
        #endregion

        /// <summary>
        /// Представляет собой процесс получения данных по протоколу UDP выполняющийся в фоновом потоке.
        /// </summary>
        private void Process()
        {
            try
            {
                while (recieverEnabled)
                {
                    IPEndPoint endPoint = null;

                    if (this.udpClient.Available > 0)
                    {
                        byte[] data = this.udpClient.Receive(ref endPoint);
                        if (data != null && endPoint != null)
                        {
                            this.OnDataRecieved(endPoint, data);
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (this.udpClient != null)
                    this.udpClient.Close();
            }
        }
        /// <summary>
        /// Подписка на событие Ж Получены новые данные от удаленного узла.
        /// Используется внутри класса для ожидания ответа от удаленного узла.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UdpSocket_DataRecieved(object sender, ClientReplyEventArgs e)
        {
            replies.Add(new UdpResponse() { EndPoint = e.EndPoint, RecievedData = e.Data });
        }
    }
}
