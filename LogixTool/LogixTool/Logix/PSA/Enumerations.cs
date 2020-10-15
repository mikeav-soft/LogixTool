using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Logix.PSA
{
    /// <summary>
    /// Начальное текстовое значение элементов Program и  Routine.
    /// </summary>
    public enum LogicNamePrefix { IM_Init, IM_Diag, IM_App, PM_App, OM_App }
    /// <summary>
    /// 
    /// </summary>
    public enum UdtType { Unknown, C, D, I, M, O, P, U }

    /// <summary>
    /// Вид триггера события или ошибок. Стандарт PSA.
    /// </summary>
    public enum DiagTriggerType
    {
        /// <summary>
        /// Является неопределенным типом.
        /// </summary>
        Null,
        /// <summary>
        /// Является "Желтым" типом триггеров.
        /// </summary>
        [AtributePsaDiagTriggerType(new string[] { "AE", "AL" })]
        Yellow,
        /// <summary>
        /// Является "Оранжевым" типом триггеров.
        /// </summary>
        [AtributePsaDiagTriggerType(new string[] { "WA", "WO" })]
        Orange,
        /// <summary>
        /// Является "Красным" типом триггеров.
        /// </summary>
        [AtributePsaDiagTriggerType(new string[] { "IF", "PS", "DF" })]
        Red
    }
}

