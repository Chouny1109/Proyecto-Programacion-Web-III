using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Entidades.EF;

public partial class ProyectoPizarraContext : DbContext
{
    public ProyectoPizarraContext()
    {
    }

    public ProyectoPizarraContext(DbContextOptions<ProyectoPizarraContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<InvitacionPizarra> InvitacionPizarras { get; set; }

    public virtual DbSet<Pizarra> Pizarras { get; set; }

    public virtual DbSet<PizarraUsuario> PizarraUsuarios { get; set; }

    public virtual DbSet<Texto> Textos { get; set; }

    public virtual DbSet<Trazo> Trazos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=ProyectoPizarra;Trusted_Connection=True;;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<InvitacionPizarra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Invitaci__3214EC07859B1592");

            entity.ToTable("InvitacionPizarra");

            entity.HasIndex(e => e.CodigoInvitacion, "UQ__Invitaci__14EDD62D089BCCE6").IsUnique();

            entity.Property(e => e.CodigoInvitacion).HasMaxLength(100);
            entity.Property(e => e.FechaExpiracion).HasColumnType("datetime");
            entity.Property(e => e.FechaInvitacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Rol).HasDefaultValue(1);
            entity.Property(e => e.UsuarioRemitenteId).HasMaxLength(450);

            entity.HasOne(d => d.Pizarra).WithMany(p => p.InvitacionPizarras)
                .HasForeignKey(d => d.PizarraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invitacio__Pizar__0D7A0286");

            entity.HasOne(d => d.UsuarioRemitente).WithMany(p => p.InvitacionPizarras)
                .HasForeignKey(d => d.UsuarioRemitenteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invitacio__Usuar__0E6E26BF");
        });

        modelBuilder.Entity<Pizarra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pizarra__3214EC077B42F457");

            entity.ToTable("Pizarra");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreadorId).HasMaxLength(100);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombrePizarra).HasMaxLength(200);
        });

        modelBuilder.Entity<PizarraUsuario>(entity =>
        {
            entity.HasKey(e => new { e.PizarraId, e.UsuarioId }).HasName("PK__PizarraU__5195DC5723860F1B");

            entity.Property(e => e.Rol).HasMaxLength(50);

            entity.HasOne(d => d.Pizarra).WithMany(p => p.PizarraUsuarios)
                .HasForeignKey(d => d.PizarraId)
                .HasConstraintName("FK__PizarraUs__Pizar__07C12930");

            entity.HasOne(d => d.Usuario).WithMany(p => p.PizarraUsuarios)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PizarraUs__Usuar__08B54D69");
        });

        modelBuilder.Entity<Texto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Textos__3214EC07CF16E22C");

            entity.HasIndex(e => e.PizarraId, "IX_Textos_PizarraId");

            entity.Property(e => e.Color).HasMaxLength(50);

            entity.HasOne(d => d.Pizarra).WithMany(p => p.Textos)
                .HasForeignKey(d => d.PizarraId)
                .HasConstraintName("FK__Textos__PizarraI__5070F446");
        });

        modelBuilder.Entity<Trazo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Trazos__3214EC07308BD2DF");

            entity.HasIndex(e => e.PizarraId, "IX_Trazos_PizarraId");

            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Xfin).HasColumnName("XFin");
            entity.Property(e => e.Xinicio).HasColumnName("XInicio");
            entity.Property(e => e.Yfin).HasColumnName("YFin");
            entity.Property(e => e.Yinicio).HasColumnName("YInicio");

            entity.HasOne(d => d.Pizarra).WithMany(p => p.Trazos)
                .HasForeignKey(d => d.PizarraId)
                .HasConstraintName("FK__Trazos__PizarraI__4D94879B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
