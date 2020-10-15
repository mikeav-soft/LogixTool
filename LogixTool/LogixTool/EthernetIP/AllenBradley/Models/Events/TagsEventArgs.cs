using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogixTool.EthernetIP.AllenBradley.Models.Events
{
    public class TagsEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public List<TagHandler> Tags { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tags"></param>
        public TagsEventArgs(List<TagHandler> tags)
        {
            this.Tags = tags;
        }
    }
}
