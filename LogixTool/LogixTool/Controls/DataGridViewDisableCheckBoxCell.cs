using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace LogixTool.Controls
{
    public class DataGridViewDisableCheckBoxCell : DataGridViewCheckBoxCell
    {
        private bool _Hide;
        /// <summary>
        /// 
        /// </summary>
        public bool Hide
        {
            get
            {
                return _Hide;
            }
            set
            {
                _Hide = value;
            }
        }

        /// <summary>
        /// By default, display the button cell.
        /// </summary>
        public DataGridViewDisableCheckBoxCell()
        {
            this._Hide = true;
        }

        /// <summary>
        /// Override the Clone method so that the Hide property is copied.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            DataGridViewDisableCheckBoxCell cell = (DataGridViewDisableCheckBoxCell)base.Clone();
            cell.Hide = this.Hide;

            return cell;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="clipBounds"></param>
        /// <param name="cellBounds"></param>
        /// <param name="rowIndex"></param>
        /// <param name="elementState"></param>
        /// <param name="value"></param>
        /// <param name="formattedValue"></param>
        /// <param name="errorText"></param>
        /// <param name="cellStyle"></param>
        /// <param name="advancedBorderStyle"></param>
        /// <param name="paintParts"></param>
        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value,
            object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            // The button cell is hide, so paint the border,  
            // background, and hide button for the cell.
            if (this._Hide)
            {
                // Draw the cell background, if specified.
                if ((paintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
                {
                    SolidBrush cellBackground = new SolidBrush(cellStyle.BackColor);
                    graphics.FillRectangle(cellBackground, cellBounds);
                    cellBackground.Dispose();
                }

                // Draw the cell borders, if specified.
                if ((paintParts & DataGridViewPaintParts.Border) ==  DataGridViewPaintParts.Border)
                {
                    PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
                }
            }
            else
            {
                base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText,
                  cellStyle, advancedBorderStyle, paintParts);
            }
        }
    }
}
