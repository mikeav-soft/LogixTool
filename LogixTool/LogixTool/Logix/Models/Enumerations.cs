using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Logix.Models
{
    /// <summary>
    /// Тип задачи.
    /// </summary>
    public enum TaskType { UNKNOWN, PERIODIC, CONTINUOUS, CONTROLLER_FAULT_HANDLER, POWER_UP_HANDLER };
    /// <summary>
    /// Класс задачи.
    /// </summary>
    public enum TaskClass { UNKNOWN, STANDARD, SAFETY };
    /// <summary>
    /// Тип рутины.
    /// </summary>
    public enum RoutineType { NULL, RLL, ST };
    /// <summary>
    /// Класс задачи.
    /// </summary>
    public enum LogicClass { NULL, STANDARD, SAFETY };
    /// <summary>
    /// Разовидность тэга.
    /// </summary>
    public enum TagType { NULL, BASE, ALIAS, PRODUCED, CONSUMED }
    /// <summary>
    /// Тип внешнего доступа тэга.
    /// </summary>
    public enum TagExternalAccess { NULL = 0, READ_WRITE = 1, READ_ONLY = 2, NONE = 3 };
    /// <summary>
    /// Способ отображения значения.
    /// </summary>
    public enum TagRadix { NULL, BINARY, DECIMAL, OCTAL, HEX, FLOAT, EXPONENTIAL, DATE_TIME, ASCII };
    /// <summary>
    /// Семейство типа данных.
    /// </summary>
    public enum DataTypeFamily { NULL, ATOMIC, PREDEFINED, USER_DEFINED, STRING, MODULE_DEFINED, ADDON_INSTANCE }
    /// <summary>
    /// Поведение параметра в инструкции.
    /// </summary>
    public enum ParameterUsage { Null, In, Out, InOut, Const, Immediate, Local }
    /// <summary>
    /// 
    /// </summary>
    public enum CrossReferenceType
    {
        None,
        ToInstructionParameter,
        ToTag,
        ToTagAlias,
        ToTagArrayIndex,
        ToDataType,
        ToDataTypeMember,
        ToInstruction,
        ToSafetyMap
    }
}
