namespace PowerShellLog.AzureSqlDb.DbModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UsageVw")]
    public partial class UsageVw
    {
        [Key]
        [StringLength(900)]
        public string CommandText { get; set; }

        public int? TimesUsed { get; set; }

        public DateTime? FirstTime { get; set; }

        public DateTime? LastTime { get; set; }
    }
}
