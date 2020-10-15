using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LogixTool.Common;

namespace LogixTool.Logix.Models
{
    public class MessageParameters
    {
        /// <summary>
        /// 
        /// </summary>
        private string MESSAGE_HEADER
        {
            get
            {
                return "Message Parameters";
            }
        }

        public string MessageType { get; set; }
        public string RequestedLength { get; set; }
        public string ConnectedFlag { get; set; }
        public string ConnectionPath { get; set; }
        public string CommTypeCode { get; set; }
        public string ServiceCode { get; set; }
        public string ObjectType { get; set; }
        public string TargetObject { get; set; }
        public string AttributeNumber { get; set; }
        public string LocalIndex { get; set; }
        public string LocalElement { get; set; }
        public string DestinationTag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xelem"></param>
        public MessageParameters(XElement xelem)
        {
            if (!xelem.ExistAs("MessageParameters"))
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "Source XML Element is not a \"MessageParameters\" or is Null."));
                return;
            }

            this.MessageType = xelem.Attribute("MessageType").GetXValue("");
            this.RequestedLength = xelem.Attribute("RequestedLength").GetXValue("");
            this.ConnectedFlag = xelem.Attribute("ConnectedFlag").GetXValue("");
            this.ConnectionPath = xelem.Attribute("ConnectionPath").GetXValue("");
            this.CommTypeCode = xelem.Attribute("CommTypeCode").GetXValue("");
            this.ServiceCode = xelem.Attribute("ServiceCode").GetXValue("");
            this.ObjectType = xelem.Attribute("ObjectType").GetXValue("");
            this.TargetObject = xelem.Attribute("TargetObject").GetXValue("");
            this.AttributeNumber = xelem.Attribute("AttributeNumber").GetXValue("");
            this.LocalIndex = xelem.Attribute("LocalIndex").GetXValue("");
            this.LocalElement = xelem.Attribute("LocalElement").GetXValue("");
            this.DestinationTag = xelem.Attribute("DestinationTag").GetXValue("");
        }
    }
}
