using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.EthernetIP.AllenBradley
{
    /// <summary>
    /// 
    /// </summary>
    public enum ServerState
    {
        Off = 0,
        TcpConnection = 1,
        Register = 2,
        ForwardOpen = 3,
        Init = 4,
        Run = 5,
        Error = -1
    }
    /// <summary>
    /// Вид системы исчисления.
    /// </summary>
    public enum TagValueRadix
    {
        Decimal,
        Hex,
        Binary,
        ASCII
    }
    /// <summary>
    /// Семейство типа данных тэга.
    /// </summary>
    public enum TagDataTypeFamily
    {
        Null,
        AtomicBool,
        AtomicFloat,
        AtomicDecimal,
        AtomicLong,
        Structure
    }
    /// <summary>
    /// Метод применяемый для чтения значения тэга.
    /// </summary>
    public enum TagReadMethod
    {
        Simple,
        Fragmet,
        Table
    }
    /// <summary>
    /// Метод применяемый для записи значения тэга.
    /// </summary>
    public enum TagWriteMethod
    {
        Simple,
        Fragmet
    }

    /// <summary>
    /// 
    /// </summary>
    public enum TaskProcessState
    {
        Running,
        Run,
        Stoping,
        Stop
    }
}
