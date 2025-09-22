// DAL/LetterAutomation/ILetterRepository.cs
using BE.LetterAutomation;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DAL.LetterAutomation
{
    public interface ILetterRepository
    {
        Task<Letter> CreateLetterAsync(Letter letter);
        Task<Letter> GetLetterByIdAsync(int id);
        Task<IEnumerable<Letter>> GetAllLettersAsync();
        Task<List<LetterSearchResult>> SearchLettersAsync(LetterSearchParams searchParams);
        Task<Letter> UpdateLetterAsync(Letter letter);
        Task<bool> DeleteLetterAsync(int id);
        string GenerateLetterNumber();
        Task<List<AdvancedLetterSearchResult>> AdvancedSearchLettersAsync(AdvancedLetterSearchParams searchParams);
    }

    public class DlLetter : ILetterRepository
    {
        private readonly Db _context;

        public DlLetter(Db context)
        {
            _context = context;
        }
        public string GenerateLetterNumber()
        {
            Random _random = new Random();
            PersianCalendar pc = new PersianCalendar();
            string year = pc.GetYear(DateTime.Now).ToString();
            string fullNumber;
            bool exist;
            int maxAttempts = 10; // حداکثر تعداد تلاش برای جلوگیری از حلقه بی‌نهایت
            int attempts = 0;

            do
            {
                string randomNumber = _random.Next(100000, 999999).ToString();
                fullNumber = $"TPL-{year}-{randomNumber}";
                exist = _context.Letters.Any(x => x.LetterNumber == fullNumber);
                attempts++;

                if (attempts >= maxAttempts)
                {
                    throw new InvalidOperationException("امکان تولید شماره نامه منحصربه‌فرد پس از چندین تلاش فراهم نشد.");
                }
            } while (exist);

            return fullNumber;
        }
        public async Task<Letter> CreateLetterAsync(Letter letter)
        {
            _context.Letters.Add(letter);
            await _context.SaveChangesAsync();
            return letter;
        }

        public async Task<bool> DeleteLetterAsync(int id)
        {
            try
            {
                var letter = await _context.Letters
                    .Include(l => l.Referrals)
                    .FirstOrDefaultAsync(l => l.Id == id);
                if (letter == null) return false;
                

                // حذف سایر وابستگی‌ها
                var actions = _context.LetterActions.Where(a => a.LetterId == id);
                _context.LetterActions.RemoveRange(actions);

                var approvals = _context.LetterApprovals.Where(a => a.LetterId == id);
                _context.LetterApprovals.RemoveRange(approvals);

                var referrals = _context.LetterReferrals.Where(r => r.LetterId == id);
                _context.LetterReferrals.RemoveRange(referrals);

                // اگر Archive دارید و در DbSet تعریف شده:
                // var archives = _context.Archives.Where(a => a.LetterId == id);
                // _context.Archives.RemoveRange(archives);

                _context.Letters.Remove(letter);
                        await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                            Console.WriteLine(e);
                            throw;
            }
        }

        public async Task<IEnumerable<Letter>> GetAllLettersAsync()
        {
            return await _context.Letters
                .Include(l => l.RelatedLetter)
                .OrderByDescending(l => l.RegistrationDate)
                .ToListAsync();
        }

        public async Task<Letter> GetLetterByIdAsync(int id)
        {
            return (await _context.Letters
                .Include(l => l.RelatedLetter)
                .FirstOrDefaultAsync(l => l.Id == id))!;
        }

        public async Task<List<LetterSearchResult>> SearchLettersAsync(LetterSearchParams searchParams)
        {
            var query = _context.Letters
                .Include(l => l.CopyReceiversList)
                .AsQueryable();

            // فیلتر بر اساس موضوع
            if (!string.IsNullOrEmpty(searchParams.Subject))
            {
                query = query.Where(l => l.Subject != null && l.Subject.Contains(searchParams.Subject));
            }

            // فیلتر بر اساس شماره نامه
            if (!string.IsNullOrEmpty(searchParams.LetterNumber))
            {
                query = query.Where(l => l.LetterNumber != null && l.LetterNumber.Contains(searchParams.LetterNumber));
            }

            // فیلتر بر اساس تاریخ
            if (!string.IsNullOrEmpty(searchParams.FromDate))
            {
                if (DateTime.TryParse(searchParams.FromDate, out var fromDate))
                {
                    query = query.Where(l => l.RegistrationDate >= fromDate);
                }
            }

            if (!string.IsNullOrEmpty(searchParams.ToDate))
            {
                if (DateTime.TryParse(searchParams.ToDate, out var toDate))
                {
                    query = query.Where(l => l.RegistrationDate <= toDate);
                }
            }

            // محدودسازی به دسترسی‌های کاربر جاری
            query = query.Where(l => !l.IsDeleted && l.Status != LetterStatus.Deleted);

            if (!string.IsNullOrEmpty(searchParams.CurrentUsername) || searchParams.UserOrganizationId.HasValue)
            {
                string? orgIdStr = searchParams.UserOrganizationId?.ToString();
                query = query.Where(l =>
                    (searchParams.CurrentUsername != null && l.Username == searchParams.CurrentUsername)
                    ||
                    (orgIdStr != null && (
                        l.Receiver == orgIdStr ||
                        l.CopyReceiversList.Any(o => o.Id == searchParams.UserOrganizationId!.Value)
                    ))
                );
            }

            return await query
                .OrderByDescending(l => l.RegistrationDate)
                .Select(l => new LetterSearchResult
                {
                    Id = l.Id,
                    LetterNumber = l.LetterNumber ?? string.Empty,
                    Subject = l.Subject ?? string.Empty,
                    RegistrationDate = l.RegistrationDate.ToString("yyyy/MM/dd")
                })
                .Take(100)
                .ToListAsync();
        }

        public async Task<Letter> UpdateLetterAsync(Letter letter)
        {
            _context.Letters.Update(letter);
            await _context.SaveChangesAsync();
            return letter;
        }

        public async Task<List<AdvancedLetterSearchResult>> AdvancedSearchLettersAsync(AdvancedLetterSearchParams searchParams)
        {
            var query = _context.Letters
                .Include(l => l.CopyReceiversList)
                .AsQueryable();

            // اعمال فیلترها
            if (!string.IsNullOrEmpty(searchParams.Subject))
                query = query.Where(l => l.Subject!.Contains(searchParams.Subject));

            if (!string.IsNullOrEmpty(searchParams.LetterNumber))
                query = query.Where(l => l.LetterNumber!.Contains(searchParams.LetterNumber));

            if (!string.IsNullOrEmpty(searchParams.Sender))
                query = query.Where(l => l.Sender!.Contains(searchParams.Sender));

            if (!string.IsNullOrEmpty(searchParams.Receiver))
                query = query.Where(l => l.Receiver!.Contains(searchParams.Receiver));

            if (!string.IsNullOrEmpty(searchParams.SignerName))
                query = query.Where(l => l.SignerName!.Contains(searchParams.SignerName));
            
            if (!string.IsNullOrEmpty(searchParams.FileCode))
                query = query.Where(l => l.FileCode!.Contains(searchParams.FileCode));

            if (searchParams.Priority.HasValue)
                query = query.Where(l => l.Priority == searchParams.Priority.Value);

            if (searchParams.Classification.HasValue)
                query = query.Where(l => l.Classification == searchParams.Classification.Value);

            if (searchParams.Status.HasValue)
                query = query.Where(l => l.Status == searchParams.Status.Value);

            // فیلتر تاریخ
            if (!string.IsNullOrEmpty(searchParams.FromDate) && DateTime.TryParse(searchParams.FromDate, out var fromDate))
                query = query.Where(l => l.RegistrationDate >= fromDate);

            if (!string.IsNullOrEmpty(searchParams.ToDate) && DateTime.TryParse(searchParams.ToDate, out var toDate))
                query = query.Where(l => l.RegistrationDate <= toDate);

            // فیلتر کلیدواژه‌ها
            if (!string.IsNullOrEmpty(searchParams.Keywords))
            {
                var keywords = searchParams.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim()).ToList();

                // برای EF Core
                query = query.Where(l => l.Keywords != null && keywords.Any(k => l.Keywords.Contains(k)));

                // یا اگر لیست است
                // query = query.Where(l => l.KeywordsList != null && keywords.Any(k => l.KeywordsList.Contains(k)));
            }

            // محدودسازی به دسترسی‌های کاربر جاری
            query = query.Where(l => !l.IsDeleted && l.Status != LetterStatus.Deleted);

            if (!string.IsNullOrEmpty(searchParams.CurrentUsername) || searchParams.UserOrganizationId.HasValue)
            {
                string? orgIdStr = searchParams.UserOrganizationId?.ToString();
                query = query.Where(l =>
                    (searchParams.CurrentUsername != null && l.Username == searchParams.CurrentUsername)
                    ||
                    (orgIdStr != null && (
                        l.Receiver == orgIdStr ||
                        l.CopyReceiversList.Any(o => o.Id == searchParams.UserOrganizationId!.Value)
                    ))
                );
            }

            // اجرای کوئری و تبدیل به مدل نتیجه
            return await query
                .OrderByDescending(l => l.RegistrationDate)
                .Select(l => new AdvancedLetterSearchResult
                {
                    Id = l.Id,
                    LetterNumber = l.LetterNumber ?? "---",
                    Subject = l.Subject,
                    Sender = l.Sender,
                    Receiver = l.Receiver,
                    RegistrationDate = l.RegistrationDate.ToString("yyyy/MM/dd"),
                    Priority = l.Priority.ToString(),
                    Classification = l.Classification.ToString(),
                    Status = l.Status.ToString(),
                    SignerName = l.SignerName,
                    FileCode = l.FileCode
                })
                .Take(200) // محدودیت برای جلوگیری از بار زیاد
                .ToListAsync();
        }
    }

}