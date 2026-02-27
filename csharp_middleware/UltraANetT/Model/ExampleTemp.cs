using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class ExampleTemp
    {
        public virtual string Name { get; set; }
        public virtual string Version { get; set; }
        public virtual string Content { get; set; }
        public virtual string MatchSort { get; set; }
        public virtual DateTime ImportDate { get; set; }
        public virtual DateTime EmlTemplate { get; set; }
        public virtual string BusType { get; set; }
    }
}
