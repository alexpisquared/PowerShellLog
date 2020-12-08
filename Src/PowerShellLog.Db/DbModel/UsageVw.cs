using System;
using System.Collections.Generic;

namespace PowerShellLog.Db.DbModel
{
    public partial class UsageVw
    {
        public string CommandText { get; set; }
        public int? TimesUsed { get; set; }
        public DateTime? FirstTime { get; set; }
        public DateTime? LastTime { get; set; }
    }
}
