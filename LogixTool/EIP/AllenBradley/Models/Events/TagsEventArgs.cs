using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EIP.AllenBradley.Models.Events
{
    public class TagsEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public List<LogixTagHandler> Tags { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tags"></param>
        public TagsEventArgs(List<LogixTagHandler> tags)
        {
            this.Tags = tags;
        }
    }
}
