using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    [Serializable]
    public class DBC
    {
        public virtual string VehicelType { get; set; }
        public virtual string VehicelConfig { get; set; }
        public virtual string VehicelStage { get; set; }
        public virtual string DBCName { get; set; }
        public virtual string BelongCAN { get; set; }
        public virtual string DBCContent { get; set; }
        public virtual string ImportUser { get; set; }
        public virtual DateTime ImportTime { get; set; }
        public virtual string FormerDBCName { get; set; }
        public virtual string CANType { get; set; }

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
