namespace PowerShellLog.AzureSqlDb.DbModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Cmd")]
    public partial class Cmd
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Cmd()
        {
            Logs = new HashSet<Log>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(900)]
        public string CommandText { get; set; }

        [StringLength(900)]
        public string Note { get; set; }

        public DateTime AddedAt { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Log> Logs { get; set; }
    }
}
