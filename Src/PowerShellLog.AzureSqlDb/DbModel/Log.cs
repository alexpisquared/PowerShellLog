namespace PowerShellLog.AzureSqlDb.DbModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Log")]
    public partial class Log
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Machine { get; set; }

        public int LineNum { get; set; }

        public int CommandId { get; set; }

        public DateTime AddedAt { get; set; }

        public virtual Cmd Cmd { get; set; }
    }
}
