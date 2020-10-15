using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogixTool.Common
{
    public class CustomListView : ListView
    {
        /// <summary>
        /// 
        /// </summary>
        public CustomListView()
        {
            // Activate double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            // Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        protected override void OnNotifyMessage(Message m)
        {
            //Filter out the WM_ERASEBKGND message
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }

    }
}
