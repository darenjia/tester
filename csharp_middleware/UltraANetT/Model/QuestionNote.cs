using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    [Serializable]
    public class QuestionNote
    {
        public virtual string VehicelType { get; set; }
        public virtual string VehicelConfig { get; set; }
        public virtual string VehicelStage { get; set; }
        public virtual string TaskRound { get; set; }
        public virtual string TestType { get; set; }
        public virtual string Module { get; set; }
        public virtual string FailItemInfo { get; set; }
        //public virtual string ExapID { get; set; }
        //public virtual string ExapName { get; set; }
        //public virtual string AssessItem { get; set; }
        //public virtual string DescriptionOfValue { get; set; }
        //public virtual string MinValue { get; set; }
        //public virtual string MaxValue { get; set; }
        //public virtual string NormalValue { get; set; }
        //public virtual string TestValue { get; set; }
        //public virtual string Result { get; set; }
        public virtual DateTime TestTime { get; set; }
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
