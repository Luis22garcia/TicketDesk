using Microsoft.EntityFrameworkCore;
using TicketDesk.Api.Models;

namespace TicketDesk.Api.Data;

public class TicketDeskDbContext : DbContext
{
    public TicketDeskDbContext(DbContextOptions<TicketDeskDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<HistorialTicket> HistorialTickets { get; set; }
    public DbSet<ComentarioTicket> ComentariosTickets { get; set; }
    public DbSet<Notificacion> Notificaciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Relación Ticket - Usuario
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Usuario)
            .WithMany(u => u.Tickets)
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación Ticket - Agente
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Agente)
            .WithMany()
            .HasForeignKey(t => t.AgenteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación Ticket - Categoría
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Categoria)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación HistorialTicket - Ticket
        modelBuilder.Entity<HistorialTicket>()
            .HasOne(h => h.Ticket)
            .WithMany()
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación HistorialTicket - Usuario
        modelBuilder.Entity<HistorialTicket>()
            .HasOne(h => h.Usuario)
            .WithMany()
            .HasForeignKey(h => h.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación ComentarioTicket - Ticket
        modelBuilder.Entity<ComentarioTicket>()
            .HasOne(c => c.Ticket)
            .WithMany()
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación ComentarioTicket - Usuario
        modelBuilder.Entity<ComentarioTicket>()
            .HasOne(c => c.Usuario)
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación Notificacion - Usuario
        modelBuilder.Entity<Notificacion>()
            .HasOne(n => n.Usuario)
            .WithMany()
            .HasForeignKey(n => n.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación Notificacion - Ticket
        modelBuilder.Entity<Notificacion>()
            .HasOne(n => n.Ticket)
            .WithMany()
            .HasForeignKey(n => n.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Categorías iniciales
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria
            {
                Id = 1,
                Nombre = "Hardware",
                Descripcion = "Problemas relacionados con equipos físicos.",
                Activa = true
            },
            new Categoria
            {
                Id = 2,
                Nombre = "Software",
                Descripcion = "Problemas relacionados con aplicaciones y sistemas.",
                Activa = true
            },
            new Categoria
            {
                Id = 3,
                Nombre = "Red",
                Descripcion = "Problemas de conexión y red.",
                Activa = true
            },
            new Categoria
            {
                Id = 4,
                Nombre = "Acceso",
                Descripcion = "Problemas de acceso, cuentas y contraseñas.",
                Activa = true
            },
            new Categoria
            {
                Id = 5,
                Nombre = "Otro",
                Descripcion = "Otros problemas de soporte técnico.",
                Activa = true
            }
        );

        // Usuarios iniciales
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nombre = "Administrador",
                Email = "admin@ticketdesk.com",
                PasswordHash = "$2a$11$kcb1D1sDTsRbqwVpQg7icueQsvDYNX/xgP/5aOcRQ3dElOgUQU8Yu",
                Rol = "Administrador",
                Activo = true,
                FechaCreacion = new DateTime(2026, 9, 16)
            },
            new Usuario
            {
                Id = 2,
                Nombre = "Agente de Soporte",
                Email = "soporte@ticketdesk.com",
                PasswordHash = "$2a$11$6iWHfL3aWA.WBZGjJqd9KO.Wbwnk32kgPUBRiVFhGZQ1wKiRFOlCW",
                Rol = "Agente",
                Activo = true,
                FechaCreacion = new DateTime(2026, 9, 16)
            },
            new Usuario
            {
                Id = 3,
                Nombre = "Usuario Demo",
                Email = "usuario@ticketdesk.com",
                PasswordHash = "$2a$11$W3kN.0enOmDHwL1C1VZDb.rUfzjiz5YjTwyBYzR6dWV/uBaui33BG",
                Rol = "Usuario",
                Activo = true,
                FechaCreacion = new DateTime(2026, 9, 16)
            }
        );
    }
}