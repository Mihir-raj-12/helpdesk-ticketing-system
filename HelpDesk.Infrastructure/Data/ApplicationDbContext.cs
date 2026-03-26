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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Explicitly define primary keys for BaseEntity children
            builder.Entity<AuditLog>().HasKey(a => a.Id);
            builder.Entity<AuditDetail>().HasKey(a => a.Id);
            builder.Entity<Ticket>().HasKey(t => t.Id);
            builder.Entity<Category>().HasKey(c => c.Id);
            builder.Entity<Comment>().HasKey(c => c.Id);

            // Ticket - raisedByUser relationship
            builder.Entity<Ticket>()
               .HasOne(t => t.RaisedByUser)
               .WithMany(u => u.RaisedTickets)
                .HasForeignKey(t => t.RaisedByUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // Ticket - AssignedToUser relationship
            builder.Entity<Ticket>()
                .HasOne(t => t.AssignedToUser)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedToUserId )
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket - Category relationship
            builder.Entity<Ticket>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tickets)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comment - Ticket relationship

            builder.Entity<Comment>()
                .HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment - User relationship

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //AuditLog - user relationship
            builder.Entity<AuditLog>()
                .HasOne(a => a.PerformedByUser)
                .WithMany()
                .HasForeignKey(a => a.PerformedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            //AuditDetail - AuditLog relationship

            builder.Entity<AuditDetail>()
                .HasOne(ad => ad.AuditLog)
                .WithMany(al => al.AuditDetails)
                .HasForeignKey(ad => ad.AuditLogId)
                .OnDelete(DeleteBehavior.Cascade);



        }


    }
}
