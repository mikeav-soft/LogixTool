using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.EthernetIP.AllenBradley.Models.Events
{
    /// <summary>
    /// Аргументы события с текущим тэгом.
    /// </summary>
    public class TagEventArgs : EventArgs
    {
        /// <summary>
        /// Текущий тэг.
        /// </summary>
        public TagHandler Tag { get; private set; }

        /// <summary>
        /// Создает новый аргумент события.
        /// </summary>
        /// <param name="tag"></param>
        public TagEventArgs(TagHandler tag)
        {
            this.Tag = tag;
        }
    }
}
