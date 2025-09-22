using BE;
using BE.LetterAutomation;
using BE.Ticketing.SupportTicketSystem.Models;
using BE.Tokening;
using BE.Calendar;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DAL
{
    public class Db : IdentityDbContext<ApplicationUser, ApplicationRole, string, ApplicationUserClaim,
        ApplicationUserRole, ApplicationUserLogin, ApplicationRoleClaim, ApplicationUserToken>
    {
        private readonly IConfiguration? _configuration;

        public Db()
        {
            // Parameterless constructor for migrations
        }

        public Db(DbContextOptions<Db> options) : base(options)
        {
        }

        public Db(DbContextOptions<Db> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && _configuration != null)
            {
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString("CON1"));
            }
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<BE.Wallet> Wallets { get; set; } = null!;
        public DbSet<WalletTransaction> WalletTransactions { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<TicketComment> TicketComments { get; set; } = null!;
        public DbSet<Attachment> Attachments { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Report> Reports { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Token> Tokens { get; set; } = null!;
        public DbSet<Letter> Letters { get; set; } = null!;
        public DbSet<Kelasehnameh> Kelasehnamehha { get; set; } = null!;
        public DbSet<Organization> Organizations { get; set; } = null!;
        public DbSet<LetterApproval> LetterApprovals { get; set; } = null!;
        public DbSet<UserOrganization> UserOrganizations { get; set; } = null!;
        public DbSet<LetterReferral> LetterReferrals { get; set; } = null!;
        public DbSet<LetterAction> LetterActions { get; set; } = null!;
        
        // Calendar DbSets
        public DbSet<BE.Calendar.CalendarEvent> CalendarEvents { get; set; } = null!;
        public DbSet<BE.Calendar.CalendarEventCategory> CalendarEventCategories { get; set; } = null!;
        public DbSet<BE.Calendar.CalendarEventParticipant> CalendarEventParticipants { get; set; } = null!;
        public DbSet<BE.Calendar.CalendarReminder> CalendarReminders { get; set; } = null!;
        
        // Chat DbSets
        public DbSet<BE.Chat.ChatRoom> ChatRooms { get; set; } = null!;
        public DbSet<BE.Chat.ChatParticipant> ChatParticipants { get; set; } = null!;
        public DbSet<BE.Chat.ChatMessage> ChatMessages { get; set; } = null!;
        public DbSet<BE.Chat.UserContact> UserContacts { get; set; } = null!;
        public DbSet<Archive> Archives { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);
            builder.Entity<Ticket>()
                .HasMany(t => t.Comments)
                .WithOne(c => c.Ticket)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>(b =>
                {
                    b.ToTable("Users");
                    b.Property(user => user.Email).HasMaxLength(260);

                    // افزودن ویژگی‌های جدید به ApplicationUser در صورت نیاز
                    b.Property(user => user.FirstName).HasMaxLength(250);
                    b.Property(user => user.LastName).HasMaxLength(250);
                    b.Property(user => user.Ostan).HasMaxLength(250);
                    b.Property(user => user.AccountType).HasMaxLength(20);
                    b.Property(user => user.CompanyName).HasMaxLength(300);
                    b.Property(user => user.Address).HasMaxLength(450);
                    b.Property(user => user.Avatar).HasMaxLength(260);

                    b.HasMany(e => e.Claims)
                        .WithOne(e => e.User)
                        .HasForeignKey(uc => uc.UserId)
                        .IsRequired();

                    b.HasMany(e => e.Logins)
                        .WithOne(e => e.User)
                        .HasForeignKey(ul => ul.UserId)
                        .IsRequired();

                    b.HasMany(e => e.Tokens)
                        .WithOne(e => e.User)
                        .HasForeignKey(ut => ut.UserId)
                        .IsRequired();


                });









            // Calendar entity configurations to fix cascade paths
            builder.Entity<BE.Calendar.CalendarEventParticipant>(b =>
            {
                b.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(p => p.CalendarEvent)
                    .WithMany(e => e.Participants)
                    .HasForeignKey(p => p.CalendarEventId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<BE.Calendar.CalendarReminder>(b =>
            {
                b.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(r => r.CalendarEvent)
                    .WithMany(e => e.Reminders)
                    .HasForeignKey(r => r.CalendarEventId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Chat entity configurations
            builder.Entity<BE.Chat.ChatParticipant>(b =>
            {
                b.HasOne(p => p.User)
                    .WithMany(u => u.ChatParticipations)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(p => p.ChatRoom)
                    .WithMany(r => r.Participants)
                    .HasForeignKey(p => p.ChatRoomId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<BE.Chat.ChatMessage>(b =>
            {
                b.HasOne(m => m.Sender)
                    .WithMany(u => u.SentMessages)
                    .HasForeignKey(m => m.SenderId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(m => m.ChatRoom)
                    .WithMany(r => r.Messages)
                    .HasForeignKey(m => m.ChatRoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(m => m.ReplyToMessage)
                    .WithMany()
                    .HasForeignKey(m => m.ReplyToMessageId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<BE.Chat.UserContact>(b =>
            {
                b.HasOne(c => c.User)
                    .WithMany(u => u.Contacts)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(c => c.ContactUser)
                    .WithMany(u => u.ContactOf)
                    .HasForeignKey(c => c.ContactUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });


        }
    }
}