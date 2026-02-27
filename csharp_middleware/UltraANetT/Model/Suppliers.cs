using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    [Serializable]
    public class Suppliers
    {
        public virtual string Module { get; set; }
        public virtual string Type { get; set; }
        public virtual string Supplier { get; set; }
        public virtual string Contact { get; set; }
        public virtual string Tel { get; set; }

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
