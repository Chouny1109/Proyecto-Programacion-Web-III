using System;
using System.Collections.Generic;
using DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Entidades.EF;

public partial class ProyectoPizarraContext : IdentityDbContext<IdentityUser>
{
    public ProyectoPizarraContext()
    {
    }

    public ProyectoPizarraContext(DbContextOptions<ProyectoPizarraContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Pizarra> Pizarras { get; set; }

    public virtual DbSet<PizarraUsuario> PizarraUsuarios { get; set; }

    public virtual DbSet<Texto> Textos { get; set; }

    public virtual DbSet<Trazo> Trazos { get; set; }

    public virtual DbSet<InvitacionPizarra> InvitacionPizarras { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=ProyectoPizarra;Trusted_Connection=True;;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pizarra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pizarra__3214EC077B42F457");

            entity.ToTable("Pizarra");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreadorId).HasMaxLength(100);
            entity.Property(e => e.ColorFondo);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombrePizarra).HasMaxLength(200);
        });

        modelBuilder.Entity<PizarraUsuario>(entity =>
        {
            entity.HasKey(e => new { e.PizarraId, e.UsuarioId }).HasName("PK__PizarraU__5195DC57C5B7103F");

            entity.Property(e => e.Rol).HasMaxLength(50);

            entity.HasOne(d => d.Pizarra).WithMany(p => p.PizarraUsuarios)
                .HasForeignKey(d => d.PizarraId)
                .HasConstraintName("FK__PizarraUs__Pizar__534D60F1");
        });

        modelBuilder.Entity<Texto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Textos__3214EC07CF16E22C");

            entity.Property(e => e.Color).HasMaxLength(50);

            entity.HasOne(d => d.Pizarra).WithMany(p => p.Textos)
                .HasForeignKey(d => d.PizarraId)
                .HasConstraintName("FK__Textos__PizarraI__5070F446");
        });

        modelBuilder.Entity<Trazo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Trazos__3214EC07308BD2DF");

            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Xfin).HasColumnName("XFin");
            entity.Property(e => e.Xinicio).HasColumnName("XInicio");
            entity.Property(e => e.Yfin).HasColumnName("YFin");
            entity.Property(e => e.Yinicio).HasColumnName("YInicio");
            entity.Property(e => e.GrupoTrazoId);

            entity.HasOne(d => d.Pizarra).WithMany(p => p.Trazos)
                .HasForeignKey(d => d.PizarraId)
                .HasConstraintName("FK__Trazos__PizarraI__4D94879B");
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
            entity.Property(e => e.Rol).HasDefaultValue(RolEnPizarra.Escritura);
            entity.Property(e => e.UsuarioRemitenteId).HasMaxLength(450);

            entity.HasOne(d => d.Pizarra).WithMany(p => p.InvitacionPizarras)
                .HasForeignKey(d => d.PizarraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invitacio__Pizar__0D7A0286");

        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}