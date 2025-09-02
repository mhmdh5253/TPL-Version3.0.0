using BE.LetterAutomation;
using Microsoft.EntityFrameworkCore;

namespace DAL.LetterAutomation
{
    public interface IKelasehnamehRepository
    {
        Task<Kelasehnameh> CreateKelasehnamehAsync(Kelasehnameh Kelasehnameh);
        Task<Kelasehnameh> GetKelasehnamehByIdAsync(int id, string userId);
        Task<IEnumerable<Kelasehnameh>> GetAllKelasehnamehsAsync(string userId);
        Task<Kelasehnameh> UpdateKelasehnamehAsync(Kelasehnameh Kelasehnameh);
        Task<bool> DeleteKelasehnamehAsync(int id, string userId);
    }

    public class DlKelasehnameh : IKelasehnamehRepository
    {
        private readonly Db _context;

        public DlKelasehnameh(Db context)
        {
            _context = context;
        }

        public async Task<Kelasehnameh> CreateKelasehnamehAsync(Kelasehnameh Kelasehnameh)
        {
            _context.Kelasehnamehha.Add(Kelasehnameh);
            await _context.SaveChangesAsync();
            return Kelasehnameh;
        }

        public async Task<Kelasehnameh> GetKelasehnamehByIdAsync(int id, string userId)
        {
            return (await _context.Kelasehnamehha
                .FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId))!;
        }

        public async Task<IEnumerable<Kelasehnameh>> GetAllKelasehnamehsAsync(string userId)
        {
            return await _context.Kelasehnamehha
                .Where(k => k.UserId == userId)
                .ToListAsync();
        }

        public async Task<Kelasehnameh> UpdateKelasehnamehAsync(Kelasehnameh Kelasehnameh)
        {
            _context.Kelasehnamehha.Update(Kelasehnameh);
            await _context.SaveChangesAsync();
            return Kelasehnameh;
        }

        public async Task<bool> DeleteKelasehnamehAsync(int id, string userId)
        {
            var kelasehnameh = await _context.Kelasehnamehha
                .FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId);

            if (kelasehnameh == null) return false;

            _context.Kelasehnamehha.Remove(kelasehnameh);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}