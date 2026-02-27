using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class UploadInfo
    {
        public virtual string IP { get; set; }
        public virtual string Port { get; set; }
        public virtual string User { get; set; }
        public virtual string Password { get; set; }
        public virtual string UploadPath { get; set; }
    }
}
