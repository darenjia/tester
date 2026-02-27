using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [Serializable]
    public class Employee
    {
        public virtual string ElyNo { get; set; }
        public virtual string ElyName { get; set; }
        public virtual string ElyRole { get; set; }
        public virtual string Department { get; set; }
        public virtual string Sex { get; set; }
        public virtual string Contact { get; set; }
        public virtual string Mail { get; set; }
        public virtual string Password { get; set; }
        //public virtual object Portrait { get; set; }
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
