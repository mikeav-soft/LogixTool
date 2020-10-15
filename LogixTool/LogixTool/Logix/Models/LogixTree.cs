using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogixTool.Common;

namespace LogixTool.Logix.Models
{
    public class LogixTree : Tree
    {
        /// <summary>
        /// Перекрестные ссылки на владельцев использующих данный объект.
        /// </summary>
        public List<CrossReferenceItem> CrossRefference { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public LogixTree(string name)
            : base(name)
        {
            this.CrossRefference = new List<CrossReferenceItem>();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public new LogixTree Clone()
        {
            LogixTree tree = (LogixTree)this.MemberwiseClone();
            tree.Childrens = new Dictionary<string, Tree>();
            tree.Parrent = null;
            tree.CrossRefference = new List<CrossReferenceItem>();
            return tree;
        }
    }
}
