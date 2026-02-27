using System;

namespace Model
{
    /// <summary>
    /// 拓扑图表
    /// </summary>
    [Serializable]
    public class Topology
    {
        public virtual string VehicelType { get; set; }
        public virtual string VehicelConfig { get; set; }
        public virtual string VehicelStage { get; set; }
        public virtual string Tply { get; set; }
        public virtual string TplyDescrible { get; set; }
        public virtual string ImportUser { get; set; }
        public virtual string ImportTime { get; set; }

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
