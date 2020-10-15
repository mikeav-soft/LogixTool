using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogixTool.Common
{
    public class FormsExtensions
    {
        /// <summary>
        /// Delegate for usual Method Procedure
        /// </summary>
        private delegate void DelegateProcedure();

        /// <summary>
        /// Производит действие с потомками элемента управления Control для реализации в другом потоке.
        /// </summary>
        /// <typeparam name="T">Тип, являющийся потомком от Control.</typeparam>
        /// <param name="control">Элемент управления с которым производятся действие.</param>
        /// <param name="action">Действие.</param>
        public static void InvokeControl<T>(T control, Action<T> action) where T : Control
        {
            // Anonym Delegate
            DelegateProcedure act = delegate
            {
                action(control);
            };

            // Do Invoke is required
            if (control != null && control.InvokeRequired)
            {
                control.Invoke(act);
            }
            else
            {
                act();
            }
        }
    }
}
