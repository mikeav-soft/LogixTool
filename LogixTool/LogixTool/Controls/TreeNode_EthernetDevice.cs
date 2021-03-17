using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EIP.AllenBradley;
using EIP.AllenBradley.Models.Events;
using LogixTool.Common;


namespace LogixTool.Controls
{
    /// <summary>
    /// Узел описывающий утройство в сети Ethernet IP.
    /// </summary>
    public class TreeNode_EthernetDevice : TreeNode
    {
        #region [ PROPERTIES ]
        /* ======================================================================================== */
        /// <summary>
        /// Текущее устройство с которым асссоциирован данный узел.
        /// </summary>
        public LogixDevice Device { get; set; }
        /// <summary>
        /// Автоматизированный процесс контроля и выполнения операций над устройством.
        /// </summary>
        public LogixTask Task { get; set; }
        /* ======================================================================================== */
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        public TreeNode_EthernetDevice(LogixDevice device)
            : base()
        {
            // Проверка входных параметров.
            if (device == null)
            {
                //throw new ArgumentNullException("TreeNode_EthernetDevice constructor contains a null argument!");
                return;
            }

            // Присваиваем устройство ассоциированное с данным узлом и создаем под него задачу.
            this.Device = device;
            this.Task = new LogixTask(device);

            // Вызываем прорисовку анимации узла.
            this.Refresh();

            // Подписываемся на события.
            this.Device.PropertyWasChanged += device_PropertyWasChanged;
            this.Device.Message += message;

            this.Task.StateWasChanged += task_StateWasChanged;
            this.Task.TagsValueWasChanged += task_TagsValueWasChanged;
            this.Task.TagsValueWasReaded += task_TagsValueWasReaded;
            this.Task.TagsValueWasWrited += task_TagsValueWasWrited;
            this.Task.Message += message;
        }

        #region [ EVENTS ]
        /* ======================================================================================== */
        /* 1. События */

        /// <summary>
        /// Возникает при обработке чтении одного из тэгов из контроллера, данные тэга которого изменились с момента последнего чтения.
        /// </summary>
        public event TagsEventHandler TagsValueWasChanged;
        /// <summary>
        /// Возникает при обработке чтения одного из тэгов устройства.
        /// </summary>
        public event TagsEventHandler TagsValueWasReaded;
        /// <summary>
        /// Возникает при обработке записи одного из тэгов устройства.
        /// </summary>
        public event TagsEventHandler TagsValueWasWrited;
        /// <summary>
        /// Возникает при изменении состояния задачи устройства.
        /// </summary>
        public event TaskEventHandler TaskStateWasChanged;
        /// <summary>
        /// Возникает при изменении значения одного из свойств устройства.
        /// </summary>
        public event DeviceEventHandler DevicePropertyWasChanged;
        /// <summary>
        /// Возникает при возникновении пользовательского сообщения.
        /// </summary>
        public event MessageEvent Message;

        /* 2. Методы генерации события */

        /// <summary>
        /// Вызывает "Событие при обработке чтении одного из тэгов из контроллера, данные тэга которого изменились с момента последнего чтения".
        /// </summary>
        /// <param name="tag"></param>
        private void Event_TagsValueWasChanged(TagsEventArgs e)
        {
            if (this.TagsValueWasChanged != null)
            {
                this.TagsValueWasChanged(this, e);
            }
        }
        /// <summary>
        /// Вызывает "Событие при обработке чтения одного из тэгов устройства".
        /// </summary>
        /// <param name="tag"></param>
        private void Event_TagValueWasReaded(TagsEventArgs e)
        {
            if (this.TagsValueWasReaded != null)
            {
                this.TagsValueWasReaded(this, e);
            }
        }
        /// <summary>
        /// Вызывает "Событие при обработке записи одного из тэгов устройства".
        /// </summary>
        /// <param name="tag"></param>
        private void Event_TagValueWasWrited(TagsEventArgs e)
        {
            if (this.TagsValueWasWrited != null)
            {
                this.TagsValueWasWrited(this, e);
            }
        }
        /// <summary>
        /// Вызывает "Событие при изменении состояния задачи устройства".
        /// </summary>
        /// <param name="task"></param>
        private void Event_TaskStateWasChanged(LogixTask task)
        {
            if (this.TaskStateWasChanged != null)
            {
                this.TaskStateWasChanged(this, new TaskEventArgs(task));
            }
        }
        /// <summary>
        /// Вызывает "Событие при изменении значения одного из свойств устройства".
        /// </summary>
        /// <param name="device"></param>
        private void Event_DevicePropertyWasChanged(LogixDevice device)
        {
            if (this.DevicePropertyWasChanged != null)
            {
                this.DevicePropertyWasChanged(this, new DeviceEventArgs(device));
            }
        }
        /// <summary>
        /// Вызывает "Событие при возникновении пользовательского сообщения".
        /// </summary>
        /// <param name="device"></param>
        private void Event_Message(MessageEventArgs messageEventArgs)
        {
            if (this.Message != null)
            {
                this.Message(this, messageEventArgs);
            }
        }
        /* ======================================================================================== */
        #endregion

        #region [ EVENT SUBSCRIPTIONS - CLX CONTROL ]
        /* ======================================================================================== */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void device_PropertyWasChanged(object sender, EventArgs e)
        {
            Refresh();
            this.Event_DevicePropertyWasChanged(this.Device);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void task_StateWasChanged(object sender, EventArgs e)
        {
            Refresh();
            this.Event_TaskStateWasChanged(this.Task);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void task_TagsValueWasReaded(object sender, TagsEventArgs e)
        {
            this.Event_TagValueWasReaded(e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void task_TagsValueWasWrited(object sender, TagsEventArgs e)
        {
            this.Event_TagValueWasWrited(e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void task_TagsValueWasChanged(object sender, TagsEventArgs e)
        {
            this.Event_TagsValueWasChanged(e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void message(object sender, MessageEventArgs e)
        {
            this.Event_Message(e);
        }
        /* ======================================================================================== */
        #endregion

        #region [ PRIVATE METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Обновляет визуальное состояние узла.
        /// </summary>
        private void Refresh()
        {
            FormsExtensions.InvokeControl<TreeView>(this.TreeView, tn =>
            {
                string slot = "";
                if (this.Device.ProcessorSlot.HasValue)
                    slot = "/" + this.Device.ProcessorSlot.Value.ToString();

                this.Text = this.Device.Name + ": (" + this.Device.Address.ToString() + slot + ")";
                switch (this.Task.ServerState)
                {
                    case ServerState.Off:
                        this.ImageIndex = 0;
                        break;

                    case ServerState.TcpConnection:
                    case ServerState.Register:
                    case ServerState.ForwardOpen:
                        this.ImageIndex = 1;
                        break;

                    case ServerState.Init:
                    case ServerState.Run:
                        this.ImageIndex = 2;
                        break;

                    case ServerState.Error:
                        this.ImageIndex = 3;
                        break;
                }

                this.SelectedImageIndex = this.ImageIndex;
            });
        }
        /* ======================================================================================== */
        #endregion
    }
}
