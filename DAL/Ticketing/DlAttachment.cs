using BE.Ticketing.SupportTicketSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DAL.Ticketing
{
    public interface IAttachmentService
    {
        /// <summary>
        /// آپلود فایل پیوست
        /// </summary>
        /// <param name="file">فایل آپلود شده</param>
        /// <param name="ticketId">شناسه تیکت</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> UploadAttachment(IFormFile file, string ticketId, string userId);

        /// <summary>
        /// دریافت لیست پیوست‌های یک تیکت
        /// </summary>
        /// <param name="ticketId">شناسه تیکت</param>
        /// <returns>لیست پیوست‌ها</returns>
        Task<List<Attachment>> GetAttachmentsByTicket(string ticketId);

        /// <summary>
        /// حذف یک پیوست
        /// </summary>
        /// <param name="attachmentId">شناسه پیوست</param>
        /// <returns>نتیجه عملیات (true/false)</returns>
        Task<bool> DeleteAttachment(string attachmentId);

        /// <summary>
        /// دانلود فایل پیوست
        /// </summary>
        /// <param name="attachmentId">شناسه پیوست</param>
        /// <returns>فایل به صورت Stream</returns>
        Task<Stream> DownloadAttachment(string attachmentId);
    }

    public class DlAttachment : IAttachmentService
    {
        private readonly Db _context;
        private readonly IWebHostEnvironment _environment;

        public DlAttachment(Db context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<bool> UploadAttachment(IFormFile file, string ticketId, string userId)
        {
            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var attachment = new Attachment
                {
                    FileName = file.FileName,
                    FilePath = $"/uploads/{uniqueFileName}",
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    TicketId = int.Parse(ticketId),
                    UploadedByUserId = userId,
                    UploadDate = DateTime.Now
                };

                await _context.Attachments.AddAsync(attachment);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Attachment>> GetAttachmentsByTicket(string ticketId)
        {
            return await _context.Attachments
                .Where(a => a.TicketId == int.Parse(ticketId))
                .ToListAsync();
        }

        public async Task<bool> DeleteAttachment(string attachmentId)
        {
            try
            {
                var attachment = await _context.Attachments.FindAsync(int.Parse(attachmentId));
                if (attachment == null) return false;

                var filePath = Path.Combine(_environment.WebRootPath, attachment.FilePath!.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Stream> DownloadAttachment(string attachmentId)
        {
            var attachment = await _context.Attachments.FindAsync(int.Parse(attachmentId));
            if (attachment == null) return null!;

            var filePath = Path.Combine(_environment.WebRootPath, attachment.FilePath!.TrimStart('/'));
            if (!File.Exists(filePath)) return null!;

            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
    }
}
