using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OJTManagementSystem.Models;

namespace OJTManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // ═══════════════════════════════════════════════════════════
        // EXISTING DBSETS
        // ═══════════════════════════════════════════════════════════
        public DbSet<Intern> Interns { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
        public DbSet<DailyTimeRecord> DailyTimeRecords { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }

        // ═══════════════════════════════════════════════════════════
        // ✅ NEW DBSETS FOR GROUP CHAT
        // ═══════════════════════════════════════════════════════════
        public DbSet<GroupChat> GroupChats { get; set; }
        public DbSet<GroupChatMember> GroupChatMembers { get; set; }
        public DbSet<GroupChatMessage> GroupChatMessages { get; set; }
        public DbSet<GroupChatMessageReadReceipt> GroupChatMessageReadReceipts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // ═══════════════════════════════════════════════════════════
            // EXISTING CONFIGURATIONS
            // ═══════════════════════════════════════════════════════════
            modelBuilder.Entity<ApplicationUser>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Intern>()
                .HasOne(i => i.User)
                .WithMany(u => u.InternProfiles)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Intern>()
                .HasOne(i => i.Supervisor)
                .WithMany(s => s.Interns)
                .HasForeignKey(i => i.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Supervisor>()
                .HasOne(s => s.User)
                .WithMany(u => u.SupervisorProfiles)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailyTimeRecord>()
                .HasOne(d => d.Intern)
                .WithMany(i => i.DailyTimeRecords)
                .HasForeignKey(d => d.InternId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Intern)
                .WithMany(i => i.Evaluations)
                .HasForeignKey(e => e.InternId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Supervisor)
                .WithMany(s => s.Evaluations)
                .HasForeignKey(e => e.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.Intern)
                .WithMany(i => i.Certificates)
                .HasForeignKey(c => c.InternId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(l => l.Intern)
                .WithMany(i => i.LeaveRequests)
                .HasForeignKey(l => l.InternId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(c => c.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(c => c.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ═══════════════════════════════════════════════════════════
            // ✅ GROUP CHAT CONFIGURATIONS
            // ═══════════════════════════════════════════════════════════

            // GroupChat → Creator relationship
            modelBuilder.Entity<GroupChat>()
                .HasOne(g => g.Creator)
                .WithMany()
                .HasForeignKey(g => g.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // GroupChat → Members relationship
            modelBuilder.Entity<GroupChat>()
                .HasMany(g => g.Members)
                .WithOne(m => m.GroupChat)
                .HasForeignKey(m => m.GroupChatId)
                .OnDelete(DeleteBehavior.Cascade);

            // GroupChat → Messages relationship
            modelBuilder.Entity<GroupChat>()
                .HasMany(g => g.Messages)
                .WithOne(msg => msg.GroupChat)
                .HasForeignKey(msg => msg.GroupChatId)
                .OnDelete(DeleteBehavior.Cascade);

            // GroupChatMember → User relationship
            modelBuilder.Entity<GroupChatMember>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // GroupChatMember unique constraint
            modelBuilder.Entity<GroupChatMember>()
                .HasIndex(m => new { m.GroupChatId, m.UserId })
                .IsUnique();

            // GroupChatMessage → Sender relationship
            modelBuilder.Entity<GroupChatMessage>()
                .HasOne(msg => msg.Sender)
                .WithMany()
                .HasForeignKey(msg => msg.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // GroupChatMessage → ReadReceipts relationship
            modelBuilder.Entity<GroupChatMessage>()
                .HasMany(msg => msg.ReadReceipts)
                .WithOne(r => r.Message)
                .HasForeignKey(r => r.GroupChatMessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // GroupChatMessageReadReceipt → User relationship
            modelBuilder.Entity<GroupChatMessageReadReceipt>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // GroupChatMessageReadReceipt unique constraint
            modelBuilder.Entity<GroupChatMessageReadReceipt>()
                .HasIndex(r => new { r.GroupChatMessageId, r.UserId })
                .IsUnique();
        }
    }
}