using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class FaultType
    {
        public virtual string ErrorType { get; set; }
        public virtual string IsMessage { get; set; }
        public virtual string MessageCount { get; set; }
        public virtual string MsgInformation { get; set; }
        public virtual string CheckInfor { get; set; }

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
