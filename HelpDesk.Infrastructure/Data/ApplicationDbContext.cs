using HelpDesk.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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

        public DbSet<SystemSetting> SystemSettings { get; set; }

        public DbSet<SlaConfig> SlaConfigs { get; set; }
        public DbSet<PublicHoliday> PublicHolidays { get; set; }

        public DbSet<KbArticle> KbArticles { get; set; }
        public DbSet<KbArticleVersion> KbArticleVersions { get; set; }

        public DbSet<RecurringTicket> RecurringTickets { get; set; }
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
            // PRD Rule: The 'General' department must always exist as a fallback
            builder.Entity<Department>().HasData(
                new Department
                {
                    Id = 1,
                    Name = "General",
                    IsActive = true,
                    DepartmentHeadId = null,
                    // NEW: Required by BaseEntity
                    CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LastUpdatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );


            // --- SYSTEM SETTINGS SEED ---
            // --- SYSTEM SETTINGS SEED ---
            builder.Entity<SystemSetting>().HasData(
                new SystemSetting
                {
                    Id = 1,
                    SystemName = "Help Desk Ticket Management System",
                    SupportEmailAddress = "helpdesk@company.com",
                    BusinessHourStart = new TimeSpan(11, 30, 0), 
                    BusinessHourEnd = new TimeSpan(20, 30, 0),
                    WorkingDays = "Monday,Tuesday,Wednesday,Thursday,Friday",
                    IsActive = true,
                    // NEW: Required by BaseEntity
                    CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LastUpdatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // PRD 6.2 Default SLA Targets Seed Data
            // Note: Assuming an 8-hour business day. 3 business days = 24 hrs. 5 business days = 40 hrs.
            builder.Entity<SlaConfig>().HasData(
                new SlaConfig
                {
                    Id = 1,
                    Priority = HelpDesk.Core.Enums.TicketPriority.Low,
                    FirstResponseHours = 8,  // 1 business day
                    ResolutionHours = 40,    // 5 business days
                    WarningThresholdPercent = 75,
                    CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                },
                new SlaConfig
                {
                    Id = 2,
                    Priority = HelpDesk.Core.Enums.TicketPriority.Medium,
                    FirstResponseHours = 8,
                    ResolutionHours = 24,    // 3 business days
                    WarningThresholdPercent = 75,
                    CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                },
                new SlaConfig
                {
                    Id = 3,
                    Priority = HelpDesk.Core.Enums.TicketPriority.High,
                    FirstResponseHours = 4,
                    ResolutionHours = 8,
                    WarningThresholdPercent = 75,
                    CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                },
                new SlaConfig
                {
                    Id = 4,
                    Priority = HelpDesk.Core.Enums.TicketPriority.Critical,
                    FirstResponseHours = 1,
                    ResolutionHours = 4,
                    WarningThresholdPercent = 75,
                    CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                }
            );

            // --- KNOWLEDGE BASE RELATIONSHIPS ---

            // 1. Primary Keys
            builder.Entity<KbArticle>().HasKey(k => k.Id);
            builder.Entity<KbArticleVersion>().HasKey(kv => kv.Id);

            // 2. An Article belongs to ONE Category
            builder.Entity<KbArticle>()
                .HasOne(k => k.Category)
                .WithMany() // We don't necessarily need a list of articles on the Category model
                .HasForeignKey(k => k.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. A Version belongs to ONE Article
            builder.Entity<KbArticleVersion>()
                .HasOne(kv => kv.KbArticle)
                .WithMany()
                .HasForeignKey(kv => kv.KbArticleId)
                .OnDelete(DeleteBehavior.Cascade); // If the Admin deletes the main article, delete its history too

            // 4. A Version is saved by ONE User (Author/Editor)
            builder.Entity<KbArticleVersion>()
                .HasOne(kv => kv.SavedByUser)
                .WithMany()
                .HasForeignKey(kv => kv.SavedByUserId)
                .OnDelete(DeleteBehavior.Restrict); // NEVER delete a user just because an article version is deleted!
        }
    }
}