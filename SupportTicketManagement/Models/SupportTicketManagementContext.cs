using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SupportTicketManagement.Models;

public partial class SupportTicketManagementContext : DbContext
{
    public SupportTicketManagementContext()
    {
    }

    public SupportTicketManagementContext(DbContextOptions<SupportTicketManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketComment> TicketComments { get; set; }

    public virtual DbSet<TicketStatusLog> TicketStatusLogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC074F156A93");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F6628D2188").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(20);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tickets__3214EC07803B7F4A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Priority)
                .HasMaxLength(10)
                .HasDefaultValue("MEDIUM");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("OPEN");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.TicketAssignedToNavigations)
                .HasForeignKey(d => d.AssignedTo)
                .HasConstraintName("FK__Tickets__Assigne__4AB81AF0");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TicketCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tickets__Created__49C3F6B7");
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketCo__3214EC07A07F35E7");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketComments)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK__TicketCom__Ticke__4E88ABD4");

            entity.HasOne(d => d.User).WithMany(p => p.TicketComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketCom__UserI__4F7CD00D");
        });

        modelBuilder.Entity<TicketStatusLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketSt__3214EC07345938B4");

            entity.Property(e => e.ChangedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NewStatus).HasMaxLength(20);
            entity.Property(e => e.OldStatus).HasMaxLength(20);

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.TicketStatusLogs)
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TicketSta__Chang__5629CD9C");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketStatusLogs)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK__TicketSta__Ticke__5535A963");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07EC42DE0D");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534C48A3D9D").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleId__4222D4EF");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
