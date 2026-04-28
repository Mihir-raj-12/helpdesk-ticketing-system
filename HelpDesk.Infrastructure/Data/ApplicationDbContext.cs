using HelpDesk.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AuditDetail> AuditDetails { get; set; }

        // NEW: Add the Departments table
        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AuditLog>().HasKey(a => a.Id);
            builder.Entity<AuditDetail>().HasKey(a => a.Id);
            builder.Entity<Ticket>().HasKey(t => t.Id);
            builder.Entity<Category>().HasKey(c => c.Id);
            builder.Entity<Comment>().HasKey(c => c.Id);

            // NEW: Department Primary Key
            builder.Entity<Department>().HasKey(d => d.Id);

            // --- TICKET RELATIONSHIPS ---
            builder.Entity<Ticket>()
               .HasOne(t => t.RaisedByUser)
               .WithMany(u => u.RaisedTickets)
                .HasForeignKey(t => t.RaisedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Ticket>()
                .HasOne(t => t.AssignedToUser)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Ticket>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tickets)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- COMMENT RELATIONSHIPS ---
            builder.Entity<Comment>()
                .HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- AUDIT RELATIONSHIPS ---
            builder.Entity<AuditLog>()
                .HasOne(a => a.PerformedByUser)
                .WithMany()
                .HasForeignKey(a => a.PerformedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AuditDetail>()
                .HasOne(ad => ad.AuditLog)
                .WithMany(al => al.AuditDetails)
                .HasForeignKey(ad => ad.AuditLogId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- NEW: DEPARTMENT RELATIONSHIPS ---

            // 1. A User belongs to ONE Department
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete loop

            // 2. A Department has ONE Department Head (User)
            builder.Entity<Department>()
                .HasOne(d => d.DepartmentHead)
                .WithMany() // We don't need a list of 'HeadedDepartments' on the User model
                .HasForeignKey(d => d.DepartmentHeadId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete loop

            // --- SEED DATA ---

            // PRD Rule: The 'General' department must always exist as a fallback
            builder.Entity<Department>().HasData(
                new Department
                {
                    Id = 1,
                    Name = "General",
                    IsActive = true,
                    DepartmentHeadId = null
                }
            );
        }
    }
}