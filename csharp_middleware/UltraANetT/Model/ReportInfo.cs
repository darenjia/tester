using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    [Serializable]
    public class ReportInfo
    {
        public virtual string TaskNo { get; set; }
        public virtual string TaskRound { get; set; }
        public virtual string TaskName { get; set; }
        public virtual string CANRoad { get; set; }
        public virtual string Module { get; set; }
        public virtual DateTime TestTime { get; set; }
        public virtual string ReportTestList { get; set; }
        public virtual string ReportCoverInfo { get; set; }
        public virtual string ReportMainInfo { get; set; }
        public virtual string ReportPathInfo { get; set; }
        public virtual string TestUser { get; set; }
        public virtual string Remark { get; set; }
        public virtual string ErrorInfo { get; set; }
        // ReSharper disable once RedundantOverridenMember
        public override bool Equals(object obj)
        {
            // ReSharper disable once BaseObjectEqualsIsObjectEquals
            return base.Equals(obj);
        }
        // ReSharper disable once RedundantOverridenMember
        public override int GetHashCode()
        {
            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();
        }
    }
}
