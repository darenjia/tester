using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Segment
    {
        public virtual string SegmentName { get; set; }
        public virtual string Baud { get; set; }
        public virtual string Correspond { get; set; }
    }
}
