using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace LogixTool.Controls
{
    public class DataGridViewDisableButtonColumn : DataGridViewButtonColumn
    {
        /// <summary>
        /// 
        /// </summary>
        public DataGridViewDisableButtonColumn()
        {
            this.CellTemplate = new DataGridViewDisableButtonCell();
        }
    }
}
