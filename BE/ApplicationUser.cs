using BE.LetterAutomation;
using BE.Ticketing.SupportTicketSystem.Models;
using BE.Calendar;
using Microsoft.AspNetCore.Identity;

namespace BE
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Ostan { get; set; }
        public string? AccountType { get; set; }
        public string? CompanyName { get; set; }
        public string? NameEdareh { get; set; }
        public string? Semat { get; set; }
        public string? Address { get; set; }
        public string? Emza { get; set; }
        public string? Avatar { get; set; }
        public bool HaqEmza { get; set; } = false;
        public int? OrganizationId { get; set; }
        public virtual Organization? Organization { get; set; }
        public virtual ICollection<ApplicationUserClaim>? Claims { get; set; }
        public virtual ICollection<ApplicationUserLogin>? Logins { get; set; }
        public virtual ICollection<ApplicationUserToken>? Tokens { get; set; }
        public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }
        public ICollection<Ticket>? CreatedTickets { get; set; }
        public ICollection<Ticket>? AssignedTickets { get; set; }
        public ICollection<TicketComment>? Comments { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
        
        // Calendar related properties
        public virtual ICollection<CalendarEvent> PersonalEvents { get; set; } = new List<CalendarEvent>();
        public virtual ICollection<CalendarEventParticipant> EventParticipations { get; set; } = new List<CalendarEventParticipant>();
        public virtual ICollection<CalendarSettings> CalendarSettings { get; set; } = new List<CalendarSettings>();
        public virtual ICollection<CalendarActivity> CalendarActivities { get; set; } = new List<CalendarActivity>();
        public virtual ICollection<CalendarReminder> CalendarReminders { get; set; } = new List<CalendarReminder>();
        
        // Chat related properties
        public virtual ICollection<BE.Chat.ChatParticipant> ChatParticipations { get; set; } = new List<BE.Chat.ChatParticipant>();
        public virtual ICollection<BE.Chat.ChatMessage> SentMessages { get; set; } = new List<BE.Chat.ChatMessage>();
        public virtual ICollection<BE.Chat.UserContact> Contacts { get; set; } = new List<BE.Chat.UserContact>();
        public virtual ICollection<BE.Chat.UserContact> ContactOf { get; set; } = new List<BE.Chat.UserContact>();

    }

}