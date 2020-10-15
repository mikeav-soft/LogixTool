﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.EthernetIP.AllenBradley.Models.Events
{
    /// <summary>
    /// Аргументы события изменения привязки к устройству с текущим тэгом.
    /// </summary>
    public class TagTaskEndEditEventArgs : EventArgs
    {
        /// <summary>
        /// Текущий тэг.
        /// </summary>
        public LogixTag Tag { get; private set; }
        /// <summary>
        /// Текущее устройство которое привязано к тэгу.
        /// </summary>
        public TagTask OldDevice { get; private set; }
        /// <summary>
        /// Новое устройство к которому стоит привязать тэг.
        /// </summary>
        public TagTask NewDevice { get; private set; }

        /// <summary>
        /// Создает новый аргумент события.
        /// </summary>
        /// <param name="tag">Текущий тэг.</param>
        /// <param name="oldDevice">Старое (текущая) задача устройства которое привязано к тэгу.</param>
        /// <param name="newDevice">Новое задача устройства к которому стоит привязать тэг.</param>
        public TagTaskEndEditEventArgs(LogixTag tag, TagTask oldDevice, TagTask newDevice)
        {
            this.Tag = tag;
            this.OldDevice = oldDevice;
            this.NewDevice = newDevice;
        }
    }
}
