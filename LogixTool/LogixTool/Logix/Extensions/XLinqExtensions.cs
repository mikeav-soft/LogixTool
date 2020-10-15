using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LogixTool
{
    /// <summary>
    /// 
    /// </summary>
    public static class XLinqExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xatr"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetXValue(this XAttribute xatr, string defaultValue)
        {
            if (xatr != null)
            {
                return xatr.Value;
            }
            else
            {
                return defaultValue;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xatr"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetXValue(this XAttribute xatr, out string value)
        {
            value = "";
            if (xatr != null)
            {
                value = xatr.Value;
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xatr"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ExisitWithXValue(this XAttribute xatr, string value)
        {
            return (xatr != null && xatr.Value == value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xelem"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool ExistAs (this XElement xelem, string name)
        {
            return (xelem != null && xelem.Name == name);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xatr"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetXValue(this XElement xelem, string defaultValue)
        {
            if (xelem != null)
            {
                return xelem.Value;
            }
            else
            {
                return defaultValue;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xelem"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XElement GetNearestXParent (this XElement xelem, string name)
        {
            XElement xresult = null;

            if (xelem == null)
            {
                return null;
            }

            IEnumerable<XElement> xancestors = xelem.Ancestors(name);
            if (xancestors != null && xancestors.Count() == 1)
            {
                xresult = xancestors.First();
            }

            return xresult;
        }

    }
}
