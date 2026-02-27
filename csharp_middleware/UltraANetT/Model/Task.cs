using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    [Serializable]
    public class Task
    {
        public virtual string TaskNo { get; set; }
        public virtual string TaskRound { get; set; }
        public virtual string TaskName { get; set; }
        public virtual string CANRoad { get; set; }
        public virtual string Module { get; set; }
        public virtual string TestType { get; set; }
        public virtual DateTime CreateTime { get; set; }
        public virtual string Creater { get; set; }
        public virtual string AuthTester { get; set; }
        public virtual string AuthorizedFromDept { get; set; }
        public virtual string Supplier { get; set; }
        
        public virtual string ContainExmp { get; set; }
        public virtual DateTime AuthorizationTime { get; set; }
        public virtual DateTime InvalidTime { get; set; }
        public virtual string Remark { get; set; }

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
