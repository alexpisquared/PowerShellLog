namespace PowerShellLog.AzureSqlDb.DbModel
{
  using System;
  using System.Data.Entity;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;

  public interface IA0DbContext
  {
    DbSet<Cmd> Cmds { get; set; }
    DbSet<Log> Logs { get; set; }
    DbSet<UsageVw> UsageVws { get; set; }
  }


  public partial class A0DbContext_NotUsed : DbContext//, IA0DbContext
  {
    A0DbContext_NotUsed() : base("data source=sqs.database.windows.net;initial catalog=PowerShellLog;persist security info=True;user id=azuresqluser;password=\";lkj;lkj99\";MultipleActiveResultSets=True;App=EntityFramework")        // : base("name=A0DbContext")
    {
    }

    public virtual DbSet<Cmd> Cmds { get; set; }
    public virtual DbSet<Log> Logs { get; set; }
    public virtual DbSet<UsageVw> UsageVws { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Cmd>()
          .Property(e => e.CommandText)
          .IsUnicode(false);

      modelBuilder.Entity<Cmd>()
          .HasMany(e => e.Logs)
          .WithRequired(e => e.Cmd)
          .HasForeignKey(e => e.CommandId)
          .WillCascadeOnDelete(false);

      modelBuilder.Entity<UsageVw>()
          .Property(e => e.CommandText)
          .IsUnicode(false);
    }
  }
}
