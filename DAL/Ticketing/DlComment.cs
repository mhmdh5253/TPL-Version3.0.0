using BE.Ticketing.SupportTicketSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Ticketing
{
    public interface ICommentService
    {
        /// <summary>
        /// افزودن کامنت جدید به تیکت
        /// </summary>
        /// <param name="comment">مدل کامنت</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> AddComment(TicketComment comment);

        /// <summary>
        /// دریافت تمام کامنت‌های یک تیکت
        /// </summary>
        /// <param name="ticketId">شناسه تیکت</param>
        /// <returns>لیست کامنت‌ها</returns>
        Task<List<TicketComment>> GetCommentsByTicket(string ticketId);

        /// <summary>
        /// حذف یک کامنت
        /// </summary>
        /// <param name="commentId">شناسه کامنت</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> DeleteComment(string commentId);

        /// <summary>
        /// به‌روزرسانی کامنت
        /// </summary>
        /// <param name="comment">مدل کامنت با اطلاعات جدید</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> UpdateComment(TicketComment comment);
    }

    public class DlComment : ICommentService
    {
        private readonly Db _context;

        public DlComment(Db context)
        {
            _context = context;
        }

        public async Task<bool> AddComment(TicketComment comment)
        {
            try
            {
                await _context.TicketComments.AddAsync(comment);

                // Update ticket's updated date
                var ticket = await _context.Tickets.FindAsync(comment.TicketId);
                if (ticket != null)
                {
                    ticket.UpdatedDate = DateTime.Now;
                    _context.Tickets.Update(ticket);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<TicketComment>> GetCommentsByTicket(string ticketId)
        {
            return await _context.TicketComments
                .Where(c => c.TicketId == int.Parse(ticketId))
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> DeleteComment(string commentId)
        {
            try
            {
                var comment = await _context.TicketComments.FindAsync(int.Parse(commentId));
                if (comment == null) return false;

                _context.TicketComments.Remove(comment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateComment(TicketComment comment)
        {
            try
            {
                _context.TicketComments.Update(comment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
