using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.EthernetIP.AllenBradley.Models.Events
{
    /// <summary>
    /// Аргументы события с текущей задачей устройства.
    /// </summary>
    public class TaskEventArgs : EventArgs
    {
        /// <summary>
        /// Текущая задача устройства.
        /// </summary>
        public TagTask Task { get; private set; }

        /// <summary>
        /// Создает новый аргумент события.
        /// </summary>
        /// <param name="task"></param>
        public TaskEventArgs(TagTask task)
        {
            this.Task = task;
        }
    }
}
