using BE.Ticketing.SupportTicketSystem.Models;
using DAL.Ticketing;
using Microsoft.AspNetCore.Http;

namespace BLL.Ticketing
{
    public class BlAttachment : IAttachmentService
    {
        private readonly DlAttachment _attachment;
        public BlAttachment(DlAttachment attachment)
        {
            _attachment = attachment;
        }
        public async Task<bool> UploadAttachment(IFormFile file, string ticketId, string userId)
        {
            return await _attachment.UploadAttachment(file, ticketId, userId);
        }

        public async Task<List<Attachment>> GetAttachmentsByTicket(string ticketId)
        {
            return await _attachment.GetAttachmentsByTicket(ticketId);
        }

        public async Task<bool> DeleteAttachment(string attachmentId)
        {
            return await _attachment.DeleteAttachment(attachmentId);
        }

        public async Task<Stream> DownloadAttachment(string attachmentId)
        {
            return await _attachment.DownloadAttachment(attachmentId);
        }
    }
}
