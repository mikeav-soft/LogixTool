using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Logix.PSA
{
    class AtributePsaDiagTriggerType : Attribute
    {
        /// <summary>
        /// Prefixes of Message.
        /// </summary>
        public string[] Prefixes { get; set; }

        /// <summary>
        /// Create new Attribule for Trigger Type.
        /// </summary>
        /// <param name="prefixes"></param>
        public AtributePsaDiagTriggerType(string[] prefixes)
        {
            if (prefixes != null || prefixes.All(t => t != null))
            {
                this.Prefixes = prefixes;
            }
            else
            {
                this.Prefixes = new string[] { "" };
            }
        }

        /// <summary>
        /// Check tag name.
        /// </summary>
        /// <param name="s">Input name.</param>
        /// <returns></returns>
        public bool CheckName(string s, out string prefix)
        {
            prefix = "";

            if (s == null || s.Trim() == "" || this.Prefixes.Any(t => t == null || t.Trim() == ""))
            {
                return false;
            }

            foreach (string p in this.Prefixes)
            {
                if (s.StartsWith(p))
                {
                    prefix = p;
                    return true;
                }
            }

            return false;
        }
    }
}
