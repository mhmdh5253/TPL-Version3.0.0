using BE.Tokening;
using Microsoft.EntityFrameworkCore;

namespace DAL.Tokening
{
    public interface ITokenRepository
    {
        Task<Token> CreateTokenAsync(string userId, string description, DateTime expiresAt);
        Task<Token> GetTokenByIdAsync(int id);
        Task<Token> GetTokenByValueAsync(string tokenValue);
        Task<IEnumerable<Token>> GetUserTokensAsync(string userId);
        Task RevokeTokenAsync(int tokenId);
        Task<bool> ValidateTokenAsync(string tokenValue);
    }

    public class DlToken : ITokenRepository
    {
        private readonly Db _context;

        public DlToken(Db context)
        {
            _context = context;
        }

        public async Task<Token> CreateTokenAsync(string userId, string description, DateTime expiresAt)
        {
            var token = new Token
            {
                UserId = userId,
                Value = GenerateTokenString(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsActive = true,
                Description = description
            };

            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<Token> GetTokenByIdAsync(int id)
        {
            return (await _context.Tokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id))!;
        }

        public async Task<Token> GetTokenByValueAsync(string tokenValue)
        {
            return (await _context.Tokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Value == tokenValue))!;
        }

        public async Task<IEnumerable<Token>> GetUserTokensAsync(string userId)
        {
            return await _context.Tokens
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task RevokeTokenAsync(int tokenId)
        {
            var token = await _context.Tokens.FindAsync(tokenId);
            if (token != null)
            {
                token.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ValidateTokenAsync(string tokenValue)
        {
            var token = await GetTokenByValueAsync(tokenValue);

            if (token == null)
                return false;

            if (!token.IsActive)
                return false;

            if (token.ExpiresAt < DateTime.UtcNow)
                return false;

            return true;
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = _context.Tokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow || !t.IsActive);

            _context.Tokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }

        private string GenerateTokenString()
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
#pragma warning disable SYSLIB0023
            var random = new System.Security.Cryptography.RNGCryptoServiceProvider();
#pragma warning restore SYSLIB0023
            var chars = new char[64];
            var bytes = new byte[64 * 4];

            random.GetBytes(bytes);

            for (int i = 0; i < 64; i++)
            {
                uint num = BitConverter.ToUInt32(bytes, i * 4);
                chars[i] = validChars[(int)(num % (uint)validChars.Length)];
            }

            return new string(chars);
        }
    }
}
