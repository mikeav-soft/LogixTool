using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.EthernetIP
{
    public class NetworkConnectionParameter
    {
        #region [ PROPERTIES ]
        /* ============================================================================== */
        /// <summary>
        /// Размер данных для подключения (Максимальное значение 0x1FF).
        /// </summary>
        public ushort ConnectionSize { get; set; }
        /// <summary>
        /// Означает что размер данных может быть переменным.
        /// </summary>
        public bool VariableConnectionSize { get; set; }
        /// <summary>
        /// Приоритет.
        /// </summary>
        public Priority Priority { get; set; }
        /// <summary>
        /// Тип подключения.
        /// </summary>
        public ConnectionType ConnectionType { get; set; }
        /// <summary>
        /// Indicates that multiple connections are allowed Target -> Originator for Implicit-Messaging
        /// </summary>
        public bool Owner { get; set; }
        /* ============================================================================== */
        #endregion

        /// <summary>
        /// Создает параметры подключения со значением по умолчанию.
        /// </summary>
        public NetworkConnectionParameter ()
        {
            this.ConnectionSize = 0;
            this.VariableConnectionSize = false;
            this.Priority = Priority.Low;
            this.ConnectionType = ConnectionType.Null;
            this.Owner = false;
        }
        /// <summary>
        /// Создает параметры подключения из набора байтов.
        /// </summary>
        /// <param name="value"></param>
        public NetworkConnectionParameter(ushort value)
        {
            this.ConnectionSize = (ushort)(value & 0x01FF);
            this.VariableConnectionSize = (value & 0x0200) != 0;

            byte priorityValue = (byte)((value & 0x0C00) >> 10);
            foreach (Priority priority in Enum.GetValues(typeof(Priority)))
            {
                if ((byte)priority == priorityValue)
                {
                    this.Priority = priority;
                    break;
                }
            }

            byte connectionTypeValue = (byte)((value & 0x6000) >> 13);
            foreach (ConnectionType connectionType in Enum.GetValues(typeof(ConnectionType)))
            {
                if ((byte)connectionType == connectionTypeValue)
                {
                    this.ConnectionType = connectionType;
                    break;
                }
            }

            this.Owner = (value & 0x8000) != 0;
        }

        /// <summary>
        /// Преобразовывает данный объект в масив Байт.
        /// </summary>
        /// <returns></returns>
        public byte [] ToBytes ()
        {
            ushort result = 0;

            result |= (ushort)(this.ConnectionSize & 0x01FF);
            result |= (ushort)(this.VariableConnectionSize ? 0x0200 : 0x0000);
            result |= (ushort)((byte)this.Priority << 10);
            result |= (ushort)((byte)this.ConnectionType << 13);
            result |= (ushort)(this.Owner ? 0x8000 : 0x0000);

            return BitConverter.GetBytes(result);
        }
    }
}
