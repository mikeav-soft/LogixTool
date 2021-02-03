using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIP.AllenBradley.Models.Events
{
    /// <summary>
    /// Аргументы события с текущей задачей устройства.
    /// </summary>
    public class TaskEventArgs : EventArgs
    {
        /// <summary>
        /// Текущая задача устройства.
        /// </summary>
        public LogixTask Task { get; private set; }

        /// <summary>
        /// Создает новый аргумент события.
        /// </summary>
        /// <param name="task"></param>
        public TaskEventArgs(LogixTask task)
        {
            this.Task = task;
        }
    }
}
