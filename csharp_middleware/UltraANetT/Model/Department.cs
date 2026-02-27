using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Department
    {
        public virtual string Name { get; set; }
        public virtual string Master { get; set; }
        public virtual int NumForDept { get; set; }
        public virtual string Remark { get; set; }
    }
}
