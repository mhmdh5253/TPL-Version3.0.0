using BE.LetterAutomation;
using DAL.LetterAutomation;

namespace BLL.LetterAutomation
{
    public class Blkelaseh : IKelasehnamehRepository
    {
        private readonly DlKelasehnameh _dlkelasehnameh;

        public Blkelaseh(DlKelasehnameh dlkelasehnameh)
        {
            _dlkelasehnameh = dlkelasehnameh;
        }

        public async Task<Kelasehnameh> CreateKelasehnamehAsync(Kelasehnameh kelasehnameh)
        {
            return await _dlkelasehnameh.CreateKelasehnamehAsync(kelasehnameh);
        }

        public async Task<Kelasehnameh> GetKelasehnamehByIdAsync(int id, string userId)
        {
            return await _dlkelasehnameh.GetKelasehnamehByIdAsync(id, userId);
        }

        public async Task<IEnumerable<Kelasehnameh>> GetAllKelasehnamehsAsync(string userId)
        {
            return await _dlkelasehnameh.GetAllKelasehnamehsAsync(userId);
        }

        public async Task<Kelasehnameh> UpdateKelasehnamehAsync(Kelasehnameh kelasehnameh)
        {
            return await _dlkelasehnameh.UpdateKelasehnamehAsync(kelasehnameh);
        }

        public async Task<bool> DeleteKelasehnamehAsync(int id, string userId)
        {
            return await _dlkelasehnameh.DeleteKelasehnamehAsync(id, userId);
        }
    }
}
