using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class OperationLog
    {
        public virtual string OperNo { get; set; }
        public virtual DateTime OperDate { get; set; }
        public virtual string EmployeeNo { get; set; }
        public virtual string EmployeeName { get; set; }
        public virtual string OperRecord { get; set; }
        public virtual string OperTable { get; set; }
    }
}
