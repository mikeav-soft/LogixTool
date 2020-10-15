using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogixTool.Common;

namespace LogixTool.Logix.Models
{
    public class MemoryObject
    {
        /// <summary>
        /// Стартовая координата в пространствае памяти.
        /// </summary>
        public MemoryCoord StartCoord { get;set; }
        /// <summary>
        /// Длина в элемнтарных условных единицах памяти.
        /// </summary>
        public uint Length { get; set; }
        /// <summary>
        /// Ссылка на объект.
        /// </summary>
        public Object Obj { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="startCoord"></param>
        /// <param name="length"></param>
        public MemoryObject(Object obj, MemoryCoord startCoord, uint length)
        {
            this.Obj = obj;
            this.StartCoord = startCoord;
            this.Length = length;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string objString = "NULL";
            if (this.Obj!=null)
            {
                objString = this.Obj.ToString();
            }
            return StartCoord.ToString() + " :: L=" + this.Length.ToString("000") + "______" + objString; 
        }
    }
}
