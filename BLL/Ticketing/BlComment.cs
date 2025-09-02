using BE.Ticketing.SupportTicketSystem.Models;
using DAL.Ticketing;

namespace BLL.Ticketing
{
    public class BlComment : ICommentService
    {
        private readonly DlComment _comment;
        public BlComment(DlComment comment)
        {
            _comment = comment;
        }
        public async Task<bool> AddComment(TicketComment comment)
        {
            return await _comment.AddComment(comment);
        }

        public async Task<List<TicketComment>> GetCommentsByTicket(string ticketId)
        {
            return await _comment.GetCommentsByTicket(ticketId);
        }

        public async Task<bool> DeleteComment(string commentId)
        {
            return await _comment.DeleteComment(commentId);
        }

        public async Task<bool> UpdateComment(TicketComment comment)
        {
            return await _comment.UpdateComment(comment);
        }
    }
}
