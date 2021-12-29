using AAV.Sys.Helpers;
using Microsoft.EntityFrameworkCore;

namespace PowerShellLog.Db.DbModel;

public partial class A0DbContext : DbContext
{
  //const string _dbSubP = @"Public\AppData\PowerShellLog\";
  //static string _un, _pw; // GitGuradian only
  //static readonly string
  //  lclFl = $@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename={OneDrive.Folder($@"{_dbSubP}PowerShellLog.mdf")};Integrated Security=True;Connect Timeout=17;",
  //  exprs = $@"data source=.\sqlexpress;initial catalog=PowerShellLog;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework",
  //  azure = $"data source=sqs.database.windows.net;initial catalog=PowerShellLog;persist security info=True;user id={_un};password=\"{_pw}\";MultipleActiveResultSets=True;App=EntityFramework",
  //  confg = "name=A0DbContext";
  //readonly string _constr;

  //public static A0DbContext GetLclFl => _LclFl;
  //public static string Constr_LclFl => lclFl;

  //static A0DbContext _Exprs => new A0DbContext(exprs);
  //static A0DbContext _LclFl => new A0DbContext(lclFl);
  //static A0DbContext _Azure => new A0DbContext(azure);
  //static A0DbContext _Confg => new A0DbContext(confg);

  //public A0DbContext() : this(constr: lclFl) { } // for EF-based controllers: parameterless ctor is needed!!!
  //A0DbContext(string constr) : base() => _constr = constr;

  public A0DbContext(DbContextOptions<A0DbContext> options)
      : base(options)
  {
  }

  public virtual DbSet<Cmd> Cmd { get; set; }
  public virtual DbSet<Log> Log { get; set; }
  public virtual DbSet<MigrationHistory> MigrationHistory { get; set; }
  public virtual DbSet<UsageVw> UsageVw { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    //if (!optionsBuilder.IsConfigured)
    //{
    //  //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
    //  optionsBuilder.UseSqlServer(_constr); // ("Server=.\\SQLExpress;Database=PowerShellLog;Trusted_Connection=True;");
    //}
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Cmd>(entity =>
    {
      entity.Property(e => e.AddedAt).HasColumnType("datetime");

      entity.Property(e => e.CommandText)
                .IsRequired()
                .HasMaxLength(900)
                .IsUnicode(false);

      entity.Property(e => e.Note).HasMaxLength(900);
    });

    modelBuilder.Entity<Log>(entity =>
    {
      entity.HasIndex(e => e.CommandId)
                .HasName("IX_CommandId");

      entity.Property(e => e.AddedAt).HasColumnType("datetime");

      entity.Property(e => e.Machine)
                .IsRequired()
                .HasMaxLength(50);

      entity.HasOne(d => d.Command)
                .WithMany(p => p.Log)
                .HasForeignKey(d => d.CommandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dbo.Log_dbo.Cmd_CommandId");
    });

    modelBuilder.Entity<MigrationHistory>(entity =>
    {
      entity.HasKey(e => new { e.MigrationId, e.ContextKey })
                .HasName("PK_dbo.__MigrationHistory");

      entity.ToTable("__MigrationHistory");

      entity.Property(e => e.MigrationId).HasMaxLength(150);

      entity.Property(e => e.ContextKey).HasMaxLength(300);

      entity.Property(e => e.Model).IsRequired();

      entity.Property(e => e.ProductVersion)
                .IsRequired()
                .HasMaxLength(32);
    });

    modelBuilder.Entity<UsageVw>(entity =>
    {
      entity.HasNoKey();

      entity.ToView("UsageVw");

      entity.Property(e => e.CommandText)
                .IsRequired()
                .HasMaxLength(900)
                .IsUnicode(false);

      entity.Property(e => e.FirstTime).HasColumnType("datetime");

      entity.Property(e => e.LastTime).HasColumnType("datetime");
    });

    OnModelCreatingPartial(modelBuilder);
  }

  partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
//tu: Scaffold-DbContext "Server=.\SQLExpress;Database=PowerShellLog;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir DbModel