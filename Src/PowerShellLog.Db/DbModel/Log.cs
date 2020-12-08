using System;
using System.Collections.Generic;

namespace PowerShellLog.Db.DbModel
{
    public partial class Log
    {
        public int Id { get; set; }
        public string Machine { get; set; }
        public int LineNum { get; set; }
        public int CommandId { get; set; }
        public DateTime AddedAt { get; set; }

        public virtual Cmd Command { get; set; }
    }
}
