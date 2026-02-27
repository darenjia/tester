using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;

namespace Model
{
    [Serializable]
    public class FileLinkByVehicel
    {
        public virtual string VehicelType { get; set; }
        public virtual string VehicelConfig { get; set; }
        public virtual string VehicelStage { get; set; }
        public virtual string MatchSort { get; set; }
        public virtual string Topology { get; set; }
        public virtual string CfgTemplateName { get; set; }
        public virtual string CfgTemplate { get; set; }
        public virtual string CfgTemplateJson { get; set; }
        public virtual string CfgBaudJson { get; set; }
        public virtual string EmlTemplateName { get; set; }
        public virtual string EmlTemplate { get; set; }
        public virtual  string TplyDescrible { get; set; }
        public virtual string ConTableColEdit { get; set; }
        public virtual  string EmlTableColEdit { get; set; }
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
