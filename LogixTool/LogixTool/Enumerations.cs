using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool
{
    /// <summary>
    /// 
    /// </summary>
    public enum SeparationFilePeriodBy 
    { 
        None,
        Minute,
        Hour,
        Day,
        Week,
        Month 
    };
    /// <summary>
    /// 
    /// </summary>
    public enum RecordingEventType { All, ByPeriod, BySelectedTags };
    /// <summary>
    /// 
    /// </summary>
    public enum RecordingPeriodUnits 
    {
        mSec,
        Sec,
        Min,
        Hour 
    };
}
