using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Common.Extension
{
    public static class IEnum
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xatr"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool ElementIsAlone<T>(this IEnumerable<T> ienum)
        {
            if (ienum == null)
            {
                return false;
            }
            
            if (ienum.Count() == 1)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ienum"></param>
        /// <param name="firstelem"></param>
        /// <returns></returns>
        public static bool TryGetFirstElement<T>(this IEnumerable<T> ienum, out T firstelem)
        {
            firstelem = default(T);

            if (ienum == null)
            {
                return false;
            }

            if (ienum.Count() != 1)
            {
                return false;
            }

            firstelem = ienum.First();

            return true;
        }





        /// <summary>
        /// Set Value if key exist, if not then add key and value
        /// </summary>
        /// <typeparam name="T">Type of Key</typeparam>
        /// <typeparam name="G">Type of Value</typeparam>
        /// <param name="idict">IDicionary</param>
        /// <param name="key">Key of Pair</param>
        /// <param name="value">Value of Pair</param>
        public static void Set<T, G>(this IDictionary<T, G> idict, T key, G value)
        {
            // First checking of input parameters
            if (idict == null || key == null || value == null)
            {
                return;
            }
            // Set value if key exist, if not then add
            if (idict.ContainsKey(key))
            {
                idict[key] = value;
            }
            else
            {
                idict.Add(key, value);
            }
        }
    }
}
