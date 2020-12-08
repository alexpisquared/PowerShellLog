using System;
using System.Collections.Generic;

namespace PowerShellLog.Db.DbModel
{
    public partial class Cmd
    {
        public Cmd()
        {
            Log = new HashSet<Log>();
        }

        public int Id { get; set; }
        public string CommandText { get; set; }
        public string Note { get; set; }
        public DateTime AddedAt { get; set; }

        public virtual ICollection<Log> Log { get; set; }
    }
}
