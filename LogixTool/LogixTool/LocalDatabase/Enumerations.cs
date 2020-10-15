using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.LocalDatabase
{
    /// <summary>
    /// Перечисление владельцев данных.
    /// </summary>
    public enum StoreOwner
    {
        [OwnerAttribute("common", "Global data of application.")]
        Common,

        [OwnerAttribute("app.diag.generator", "Diag Generator Tool.")]
        AppDiagGenerator,

        [OwnerAttribute("app.export.io", "IO Tags Export Tool.")]
        AppExportIO,

        [OwnerAttribute("app.plc.inspector", "PLC Inspector Tool.")]
        AppPLCInspector,

        [OwnerAttribute("app.registrator", "Registrator Tool.")]
        AppRegistrator,
    }
    /// <summary>
    /// Перечисление типов хранимых данных.
    /// </summary>
    public enum StoreType
    {
        [TypeAttribute("project")]
        Project,

        [TypeAttribute("task")]
        Task,

        [TypeAttribute("program")]
        Programs,

        [TypeAttribute("routine")]
        Routine,

        [TypeAttribute("rung")]
        Rung,

        [TypeAttribute("addon")]
        Addon,

        [TypeAttribute("udt")]
        Udt,

        [TypeAttribute("eip.devices")]
        EipDevices,

        [TypeAttribute("settings")]
        Settings
    }
}
