using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Models;

public partial class MecaFlowContext : DbContext
{
    public MecaFlowContext()
    {
    }

    public MecaFlowContext(DbContextOptions<MecaFlowContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asistencia> Asistencias { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<CuentasCliente> CuentasClientes { get; set; }

    public virtual DbSet<Diagnostico> Diagnosticos { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<IngresosVehiculo> IngresosVehiculos { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<ReportesFinanciero> ReportesFinancieros { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TareasVehiculo> TareasVehiculos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<Marca> Marcas { get; set; }
    public DbSet<Modelo> Modelos { get; internal set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-5LOR99L\\SQLEXPRESS01;Database=MecaFlowDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asistencia>(entity =>
        {
            entity.HasKey(e => e.AsistenciaId).HasName("PK__Asistenc__72710FA52933811A");

            entity.HasOne(d => d.Empleado).WithMany(p => p.Asistencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Asistenci__Emple__440B1D61");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.ClienteId).HasName("PK__Clientes__71ABD087981607D1");

            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<CuentasCliente>(entity =>
        {
            entity.HasKey(e => e.CuentaClienteId).HasName("PK__CuentasC__71D3BEBA4D179F54");

            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Cliente).WithMany(p => p.CuentasClientes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CuentasCl__Clien__5BE2A6F2");
        });

        modelBuilder.Entity<Diagnostico>(entity =>
        {
            entity.HasKey(e => e.DiagnosticoId).HasName("PK__Diagnost__9A0D5D5A3F72F854");

            entity.HasOne(d => d.Empleado).WithMany(p => p.Diagnosticos).HasConstraintName("FK__Diagnosti__Emple__47DBAE45");

            entity.HasOne(d => d.Vehiculo).WithMany(p => p.Diagnosticos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Diagnosti__Vehic__46E78A0C");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.EmpleadoId).HasName("PK__Empleado__958BE910A5C982F3");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.FacturaId).HasName("PK__Facturas__5C02486591CABA0D");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Facturas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Facturas__Client__52593CB8");

            entity.HasOne(d => d.Vehiculo).WithMany(p => p.Facturas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Facturas__Vehicu__534D60F1");
        });

        modelBuilder.Entity<IngresosVehiculo>(entity =>
        {
            entity.HasKey(e => e.IngresoId).HasName("PK__Ingresos__DBF0909A29742D0C");

            entity.HasOne(d => d.Vehiculo).WithMany(p => p.IngresosVehiculos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__IngresosV__Vehic__4F7CD00D");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.PagoId).HasName("PK__Pagos__F00B613859F798AA");

            entity.HasOne(d => d.Factura).WithMany(p => p.Pagos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pagos__FacturaId__571DF1D5");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.PermisoId).HasName("PK__Permisos__96E0C7236BECEC39");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__Password__E3A57E4A__________");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Usado).HasDefaultValue(false);

            entity.HasOne(d => d.Usuario).WithMany()
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__PasswordR__Usuar__________");

            entity.HasIndex(e => e.Token).HasDatabaseName("IX_PasswordResetTokens_Token");
            entity.HasIndex(e => e.UsuarioId).HasDatabaseName("IX_PasswordResetTokens_UsuarioId");
        });

        modelBuilder.Entity<ReportesFinanciero>(entity =>
        {
            entity.HasKey(e => e.ReporteId).HasName("PK__Reportes__0B29EA6EC5885A5A");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PK__Roles__F92302F1B4E8E37A");

            entity.HasMany(d => d.Permisos).WithMany(p => p.Rols)
                .UsingEntity<Dictionary<string, object>>(
                    "RolPermiso",
                    r => r.HasOne<Permiso>().WithMany()
                        .HasForeignKey("PermisoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__RolPermis__Permi__6B24EA82"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RolId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__RolPermis__RolId__6A30C649"),
                    j =>
                    {
                        j.HasKey("RolId", "PermisoId").HasName("PK__RolPermi__D04D0E834F824912");
                        j.ToTable("RolPermisos");
                    });
        });

        modelBuilder.Entity<TareasVehiculo>(entity =>
        {
            entity.HasKey(e => e.TareaId).HasName("PK__TareasVe__5CD8399135430AA2");

            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Realizada).HasDefaultValue(false);

            entity.HasOne(d => d.Vehiculo).WithMany(p => p.TareasVehiculos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TareasVeh__Vehic__4CA06362");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__Usuarios__2B3DE7B8C4FF2483");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");

            entity.HasMany(d => d.Rols).WithMany(p => p.Usuarios)
                .UsingEntity<Dictionary<string, object>>(
                    "UsuarioRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RolId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UsuarioRo__RolId__6754599E"),
                    l => l.HasOne<Usuario>().WithMany()
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UsuarioRo__Usuar__66603565"),
                    j =>
                    {
                        j.HasKey("UsuarioId", "RolId").HasName("PK__UsuarioR__24AFD7975F9DBC41");
                        j.ToTable("UsuarioRoles");
                    });
        });

        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasKey(e => e.VehiculoId).HasName("PK__Vehiculo__AA088600B7BE4ACB");

            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Vehiculos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Vehiculos__Clien__3C69FB99");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
