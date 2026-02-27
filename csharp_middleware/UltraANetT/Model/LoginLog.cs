using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class LoginLog
    {
        public virtual string LoginNo { get; set; }
        public virtual string EmployeeNo { get; set; }
        public virtual string EmployeeName { get; set; }
        public virtual string Department { get; set; }
        public virtual DateTime LoginDate { get; set; }
        public virtual DateTime LoginOffDate { get; set; }
    }
}
