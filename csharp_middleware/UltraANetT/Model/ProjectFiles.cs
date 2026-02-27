using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class ProjectFiles
    {
        public virtual string ProName { get; set; }
        public virtual byte[] Content { get; set; }
        public virtual string UploadUser { get; set; }
        public virtual DateTime UploadDate { get; set; }
    }
}