using System;

namespace Model
{
    /// <summary>
    /// 车型授权表
    /// </summary>
    [Serializable]
    public class Authorization
    {
        public virtual string VehicelType { get; set; }
        public virtual string VehicelConfig { get; set; }
        public virtual string VehicelStage { get; set; }
        public virtual DateTime CreateTime { get; set; }
        public virtual string Creater { get; set; }
        public virtual string AuthorizeTo { get; set; }
        public virtual string AuthorizedDept { get; set; }
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
