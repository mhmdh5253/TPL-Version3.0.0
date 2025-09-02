using BE.LetterAutomation;
using DAL.LetterAutomation;

namespace BLL.LetterAutomation
{
    public interface ILetterService
    {
        Task<ApiResponse> CreateLetterAsync(Letter letter);
        Task<ApiResponse> GetLetterByIdAsync(int id);
        Task<ApiResponse> GetAllLettersAsync();
        Task<ApiResponse> SearchLettersAsync(LetterSearchParams searchParams);
        Task<ApiResponse> UpdateLetterAsync(Letter letter);
        Task<ApiResponse> DeleteLetterAsync(int id);
        Task<ApiResponse> AdvancedSearchLettersAsync(AdvancedLetterSearchParams searchParams);
    }

    public class BlLetter : ILetterService
    {
        private readonly DlLetter _letterRepository;

        public BlLetter(DlLetter letterRepository)
        {
            _letterRepository = letterRepository;
        }

        public string GenerateLetterNumber()
        {
            return _letterRepository.GenerateLetterNumber();
        }
        public async Task<ApiResponse> CreateLetterAsync(Letter letter)
        {
            try
            {
                var createdLetter = await _letterRepository.CreateLetterAsync(letter);
                return new ApiResponse
                {
                    Success = true,
                    Message = "نامه با موفقیت ایجاد شد",
                    Data = createdLetter
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"خطا در ایجاد نامه: {ex.Message}",
                    Data = null!
                };
            }
        }

        public async Task<ApiResponse> DeleteLetterAsync(int id)
        {
            try
            {
                var result = await _letterRepository.DeleteLetterAsync(id);
                return new ApiResponse
                {
                    Success = result,
                    Message = result ? "نامه با موفقیت حذف شد" : "نامه مورد نظر یافت نشد",
                    Data = null!
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"خطا در حذف نامه: {ex.Message}",
                    Data = null!
                };
            }
        }

        public async Task<ApiResponse> GetAllLettersAsync()
        {
            try
            {
                var letters = await _letterRepository.GetAllLettersAsync();
                return new ApiResponse
                {
                    Success = true,
                    Message = "لیست نامه‌ها با موفقیت دریافت شد",
                    Data = letters
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"خطا در دریافت لیست نامه‌ها: {ex.Message}",
                    Data = null!
                };
            }
        }

        public async Task<ApiResponse> GetLetterByIdAsync(int id)
        {
            try
            {
                var letter = await _letterRepository.GetLetterByIdAsync(id);
                return new ApiResponse
                {
                    Success = letter != null!,
                    Message = letter != null ? "نامه با موفقیت دریافت شد" : "نامه مورد نظر یافت نشد",
                    Data = letter!
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"خطا در دریافت نامه: {ex.Message}",
                    Data = null!
                };
            }
        }

        public async Task<ApiResponse> SearchLettersAsync(LetterSearchParams searchParams)
        {
            try
            {
                var letters = await _letterRepository.SearchLettersAsync(searchParams);
                return new ApiResponse
                {
                    Success = true,
                    Message = "جستجوی نامه‌ها با موفقیت انجام شد",
                    Data = letters
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"خطا در جستجوی نامه‌ها: {ex.Message}",
                    Data = null!
                };
            }
        }

        public async Task<ApiResponse> UpdateLetterAsync(Letter letter)
        {
            try
            {
                var updatedLetter = await _letterRepository.UpdateLetterAsync(letter);
                return new ApiResponse
                {
                    Success = true,
                    Message = "نامه با موفقیت بروزرسانی شد",
                    Data = updatedLetter
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"خطا در بروزرسانی نامه: {ex.Message}",
                    Data = null!
                };
            }
        }

        public async Task<ApiResponse> AdvancedSearchLettersAsync(AdvancedLetterSearchParams searchParams)
        {
            try
            {
                var results = await _letterRepository.AdvancedSearchLettersAsync(searchParams);

                return new ApiResponse
                {
                    Success = true,
                    Message = results.Any() ? "نتایج جستجو یافت شد" : "نتیجه‌ای یافت نشد",
                    Data = results
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"خطا در جستجو: {ex.Message}",
                    Data = null
                };
            }
        }



    }

}