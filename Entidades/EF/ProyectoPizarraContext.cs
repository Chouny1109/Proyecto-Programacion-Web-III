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

    public virtual DbSet<InvitacionPizarra> InvitacionPizarras { get; set; }

    public virtual DbSet<Mensaje> Mensajes { get; set; }

    public virtual DbSet<MensajeVisto> MensajeVistos { get; set; }

    public virtual DbSet<Pizarra> Pizarras { get; set; }

    public virtual DbSet<PizarraUsuario> PizarraUsuarios { get; set; }

    public virtual DbSet<Texto> Textos { get; set; }

    public virtual DbSet<Trazo> Trazos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=ProyectoPizarra;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InvitacionPizarra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Invitaci__3214EC0732C0BD28");

            entity.ToTable("InvitacionPizarra");

            entity.HasIndex(e => e.CodigoInvitacion, "UQ__Invitaci__14EDD62D6DA2A155").IsUnique();

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
                .HasConstraintName("FK__Invitacio__Pizar__6B24EA82");
        });

        modelBuilder.Entity<Mensaje>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Mensaje__3214EC072898E48B");

            entity.ToTable("Mensaje");

            entity.HasIndex(e => new { e.PizarraId, e.UsuarioId, e.FechaPublicacion }, "IX_Mensaje_Pizarra_Usuario_Fecha");

            entity.Property(e => e.FechaPublicacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreUsuario).HasMaxLength(100);

            entity.HasOne(d => d.Pizarra).WithMany(p => p.Mensajes)
                .HasForeignKey(d => d.PizarraId)
                .HasConstraintName("FK_Mensaje_Pizarra");
        });

        modelBuilder.Entity<MensajeVisto>(entity =>
        {
            entity.HasKey(e => new { e.MensajeId, e.UsuarioId }).IsClustered(false);

            entity.ToTable("MensajeVisto");

            entity.Property(e => e.FechaVisto)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Mensaje).WithMany(p => p.MensajeVistos)
                .HasForeignKey(d => d.MensajeId)
                .HasConstraintName("FK_MensajeVisto_Mensaje");
        });

        modelBuilder.Entity<Pizarra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pizarra__3214EC077B42F457");

            entity.ToTable("Pizarra");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ColorFondo).HasMaxLength(100);
            entity.Property(e => e.CreadorId).HasMaxLength(100);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombrePizarra).HasMaxLength(200);
        });

        modelBuilder.Entity<PizarraUsuario>(entity =>
        {
            entity.HasKey(e => new { e.PizarraId, e.UsuarioId }).HasName("PK__PizarraU__5195DC57DE0D30AF");

            entity.Property(e => e.Rol).HasMaxLength(50);

            entity.HasOne(d => d.Pizarra).WithMany(p => p.PizarraUsuarios)
                .HasForeignKey(d => d.PizarraId)
                .HasConstraintName("FK__PizarraUs__Pizar__6D0D32F4");
        });

        modelBuilder.Entity<Texto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Textos__3214EC07CF16E22C");

            entity.Property(e => e.Id).HasMaxLength(50);
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
