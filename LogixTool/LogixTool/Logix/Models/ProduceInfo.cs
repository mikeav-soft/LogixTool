using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LogixTool.Common;

namespace LogixTool.Logix.Models
{
    public class ProduceInfo
    {
        /// <summary>
        /// 
        /// </summary>
        private string MESSAGE_HEADER
        {
            get
            {
                return "Produce Info";
            }
        }

        public string ProduceCount { get; set; }
        public string ProgrammaticallySendEventTrigger { get; set; }
        public string UnicastPermitted { get; set; }
        public string MinimumRPI { get; set; }
        public string MaximumRPI { get; set; }
        public string DefaultRPI { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xelem"></param>
        public ProduceInfo (XElement xelem)
        {
            if (!xelem.ExistAs("ProduceInfo"))
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "Source XML Element is not a \"ProduceInfo\" or is Null."));
                return;
            }

            this.ProduceCount = xelem.Attribute("ProduceCount").GetXValue("");
            this.ProgrammaticallySendEventTrigger = xelem.Attribute("ProgrammaticallySendEventTrigger").GetXValue("");
            this.UnicastPermitted = xelem.Attribute("UnicastPermitted").GetXValue("");
            this.MinimumRPI = xelem.Attribute("MinimumRPI").GetXValue("");
            this.MaximumRPI = xelem.Attribute("MaximumRPI").GetXValue("");
            this.DefaultRPI = xelem.Attribute("DefaultRPI").GetXValue("");
        }
    }
}
