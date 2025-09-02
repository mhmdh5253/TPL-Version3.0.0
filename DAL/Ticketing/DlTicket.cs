using BE.Ticketing.SupportTicketSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Ticketing
{

    public interface ITicketService
    {
        /// <summary>
        /// ایجاد یک تیکت جدید
        /// </summary>
        /// <param name="ticket">مدل تیکت</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> Create(Ticket ticket);

        /// <summary>
        /// دریافت تیکت بر اساس شناسه
        /// </summary>
        /// <param name="ticketId">شناسه تیکت</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> GetTicketById(int ticketId);

        /// <summary>
        /// دریافت تمام تیکت‌های یک کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست تیکت‌های کاربر</returns>
        Task<List<Ticket>> GetAllUserTickets(string userId);

        /// <summary>
        /// دریافت تمام تیکت‌های سیستم
        /// </summary>
        /// <returns>لیست تمام تیکت‌ها</returns>
        Task<List<Ticket>> GetAllTickets();

        /// <summary>
        /// حذف تیکت بر اساس شناسه
        /// </summary>
        /// <param name="ticketId">شناسه تیکت</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> DeleteTicketById(string ticketId);

        /// <summary>
        /// به‌روزرسانی اطلاعات تیکت
        /// </summary>
        /// <param name="ticket">مدل تیکت با اطلاعات جدید</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> Update(Ticket ticket);

        /// <summary>
        /// تغییر وضعیت تیکت
        /// </summary>
        /// <param name="ticketId">شناسه تیکت</param>
        /// <param name="newStatus">وضعیت جدید</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> ChangeStatus(string ticketId, TicketStatus newStatus);
        /// <summary>
        /// ارجاع تیکت به دپارتمان
        /// </summary>
        /// <param name="ticketId">شناسه تیکت</param>
        /// <param name="categoryId">دسته بندی جدید</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> UpdateTicketCategory(int ticketId, int categoryId);
    }
    public class DlTicket : ITicketService
    {
        private readonly Db _context;

        public DlTicket(Db context)
        {
            _context = context;
        }

        public async Task<bool> Create(Ticket ticket)
        {
            try
            {
                await _context.Tickets.AddAsync(ticket);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> GetTicketById(int ticketId)
        {
            return await _context.Tickets.AnyAsync(t => t.Id == ticketId);
        }

        public async Task<List<Ticket>> GetAllUserTickets(string userId)
        {
            return await _context.Tickets
                .Where(t => t.CreatedByUserId == userId)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Ticket>> GetAllTickets()
        {
            return await _context.Tickets
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .Include(t => t.Category)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> DeleteTicketById(string ticketId)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(int.Parse(ticketId));
                if (ticket == null) return false;

                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Update(Ticket ticket)
        {
            try
            {
                ticket.UpdatedDate = DateTime.Now;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangeStatus(string ticketId, TicketStatus newStatus)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(int.Parse(ticketId));
                if (ticket == null) return false;

                ticket.Status = newStatus;
                ticket.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateTicketCategory(int ticketId, int categoryId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null) return false;

            ticket.CategoryId = categoryId;
            ticket.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
