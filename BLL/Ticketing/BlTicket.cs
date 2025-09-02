using BE.Ticketing.SupportTicketSystem.Models;
using DAL.Ticketing;

namespace BLL.Ticketing
{
    public class BlTicket : ITicketService
    {
        private readonly DlTicket _ticket;
        public BlTicket(DlTicket ticket)
        {
            _ticket = ticket;
        }
        public async Task<bool> Create(Ticket ticket)
        {
            return await _ticket.Create(ticket);
        }

        public async Task<bool> GetTicketById(int ticketId)
        {
            return await _ticket.GetTicketById(ticketId);
        }

        public async Task<List<Ticket>> GetAllUserTickets(string userId)
        {
            return await _ticket.GetAllUserTickets(userId);
        }

        public async Task<List<Ticket>> GetAllTickets()
        {
            return await _ticket.GetAllTickets();
        }

        public async Task<bool> DeleteTicketById(string ticketId)
        {
            return await _ticket.DeleteTicketById(ticketId);
        }

        public async Task<bool> Update(Ticket ticket)
        {
            return await _ticket.Update(ticket);
        }

        public async Task<bool> ChangeStatus(string ticketId, TicketStatus newStatus)
        {
            return await _ticket.ChangeStatus(ticketId, newStatus);
        }

        public async Task<bool> UpdateTicketCategory(int ticketId, int categoryId)
        {
            return await _ticket.UpdateTicketCategory(ticketId, categoryId);
        }
    }



}
