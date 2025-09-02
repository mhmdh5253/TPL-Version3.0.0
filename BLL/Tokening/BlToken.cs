using BE.Tokening;
using DAL.Tokening;

namespace BLL.Tokening
{
    public class BlToken : ITokenRepository
    {
        private readonly DlToken _token;
        public BlToken(DlToken token)
        {
            _token = token;
        }
        public async Task<Token> CreateTokenAsync(string userId, string description, DateTime expiresAt)
        {
            return await _token.CreateTokenAsync(userId, description, expiresAt);
        }

        public async Task<Token> GetTokenByIdAsync(int id)
        {
            return await _token.GetTokenByIdAsync(id);
        }

        public async Task<Token> GetTokenByValueAsync(string tokenValue)
        {
            return await _token.GetTokenByValueAsync(tokenValue);
        }

        public async Task<IEnumerable<Token>> GetUserTokensAsync(string userId)
        {
            return await _token.GetUserTokensAsync(userId);
        }

        public async Task RevokeTokenAsync(int tokenId)
        {
            await _token.RevokeTokenAsync(tokenId);
        }

        public async Task<bool> ValidateTokenAsync(string tokenValue)
        {
            return await _token.ValidateTokenAsync(tokenValue);
        }
    }
}
