using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Common
{
    /// <summary>
    /// Делегат описывающий событие сообщения.
    /// </summary>
    /// <param name="sender">Объект посылающий сообщение.</param>
    /// <param name="e">Аргумнты сообщения.</param>
    public delegate void MessageEvent(object sender, MessageEventArgs e);
}
