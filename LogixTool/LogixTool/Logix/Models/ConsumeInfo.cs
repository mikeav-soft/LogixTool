using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LogixTool.Common;

namespace LogixTool.Logix.Models
{
    public class ConsumeInfo
    {
        /// <summary>
        /// 
        /// </summary>
        private string MESSAGE_HEADER
        {
            get
            {
                return "Consume Info";
            }
        }

        public string Producer { get; set; }
        public string RemoteTag { get; set; }
        public string RemoteInstance { get; set; }
        public string RPI { get; set; }
        public string Unicast { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xelem"></param>
        public ConsumeInfo(XElement xelem)
        {
            if (!xelem.ExistAs("ConsumeInfo"))
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "Source XML Element is not a \"ConsumeInfo\" or is Null."));
                return;
            }

            this.Producer = xelem.Attribute("Producer").GetXValue("");
            this.RemoteTag = xelem.Attribute("RemoteTag").GetXValue("");
            this.RemoteInstance = xelem.Attribute("RemoteInstance").GetXValue("");
            this.RPI = xelem.Attribute("RPI").GetXValue("");
            this.Unicast = xelem.Attribute("Unicast").GetXValue("");
        }
    }
}
