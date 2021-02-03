using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIP.AllenBradley.Models.Events
{
    /// <summary>
    /// Аргументы события с текущим устрйоством.
    /// </summary>
    public class DeviceEventArgs : EventArgs
    {
        /// <summary>
        /// Текущее устройство.
        /// </summary>
        public LogixDevice Device { get; private set; }

        /// <summary>
        /// Создает новый аргумент события.
        /// </summary>
        /// <param name="device"></param>
        public DeviceEventArgs(LogixDevice device)
        {
            this.Device = device;
        }
    }
}
