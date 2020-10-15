using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Logix.Models
{
    public class SafetyTagMap
    {
        /// <summary>
        /// 
        /// </summary>
        public LogicInstructionParameter StandardTag {get;set;}
        /// <summary>
        /// 
        /// </summary>
        public LogicInstructionParameter SafetyTag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="standardTag"></param>
        /// <param name="safetyTag"></param>
        public SafetyTagMap(object standardTag, object safetyTag)
        {
            this.StandardTag = new LogicInstructionParameter("0","",ParameterUsage.In);
            this.StandardTag.Value = standardTag;

            this.SafetyTag = new LogicInstructionParameter("0","",ParameterUsage.Out);
            this.SafetyTag.Value = safetyTag;
        }
    }
}
