using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogixTool.Common
{
    /// <summary>
    /// Расширения для ComboBox.
    /// </summary>
    public static class ComboBoxExtensions
    {
        /// <summary>
        /// Устанавливает имена из перечисляемого типа как коллекцию ComboBox.
        /// </summary>
        /// <typeparam name="T">Перечисляемый тип.</typeparam>
        /// <param name="comboBox">Текущий элеент управления ComboBox.</param>
        public static bool SetCollectionFromEnumeration<T>(this ComboBox comboBox)
        {
            comboBox.Items.Clear();

            if (!typeof(T).IsEnum)
            {
                return false;
            }

            foreach (Enum enumItem in Enum.GetValues(typeof(T)))
            {
                comboBox.Items.Add(enumItem.ToString());
            }

            return true;
        }
        /// <summary>
        /// Устанавливает выделенный объект элемента управления ComboBox по текстовому значению.
        /// </summary>
        /// <param name="comboBox">Текущий элеент управления ComboBox.</param>
        /// <param name="objectName">Текстовое значение устанавливаемого объекта.</param>
        public static bool SetItemFromText(this ComboBox comboBox, string objectName)
        {
            if (comboBox == null || objectName == null)
            {
                return false;
            }

            foreach (object item in comboBox.Items)
            {
                if (item != null && item.ToString() == objectName)
                {
                    comboBox.SelectedItem = item;
                    return true;
                }
            }
            return false;
        }
    }
}
