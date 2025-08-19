using AICore.Classes;
using Microsoft.EntityFrameworkCore;

namespace AICore.Models;

public partial class NewsReaderContext : DbContext
{
    public NewsReaderContext()
    {
    }

    public NewsReaderContext(DbContextOptions<NewsReaderContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Securityobject> Securityobjects { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https: //go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql(Config.Instance.ConnectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Pkconversationid).HasName("pk_conversation");

            entity.ToTable("conversation");

            entity.Property(e => e.Pkconversationid)
                .ValueGeneratedNever()
                .HasColumnName("pkconversationid");
            entity.Property(e => e.Createdat)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Fkparentid).HasColumnName("fkparentid");
            entity.Property(e => e.Fksecurityobjectowner).HasColumnName("fksecurityobjectowner");
            entity.Property(e => e.Serializedchat).HasColumnName("serializedchat");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasOne(d => d.FksecurityobjectownerNavigation).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.Fksecurityobjectowner)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_conversation_securityobjects");
        });

        modelBuilder.Entity<Securityobject>(entity =>
        {
            entity.HasKey(e => e.Activedirectoryid).HasName("pk_securityobjects");

            entity.ToTable("securityobjects");

            entity.Property(e => e.Activedirectoryid)
                .ValueGeneratedNever()
                .HasColumnName("activedirectoryid");
            entity.Property(e => e.Emailaddress).HasColumnName("emailaddress");
            entity.Property(e => e.Forename).HasColumnName("forename");
            entity.Property(e => e.Fullname).HasColumnName("fullname");
            entity.Property(e => e.Isactive).HasColumnName("isactive");
            entity.Property(e => e.Isgroup).HasColumnName("isgroup");
            entity.Property(e => e.Pass).HasColumnName("pass");
            entity.Property(e => e.Surname).HasColumnName("surname");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}