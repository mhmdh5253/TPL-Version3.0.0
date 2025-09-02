using BE;
using DAL.Wallet;

namespace BLL.Wallet
{
    public class BlWallet : IWalletRepository
    {
        private readonly DAL.Wallet.IWalletRepository _repository;
        public BlWallet(IWalletRepository repository)
        {
            _repository = repository;
        }
        public async Task<BE.Wallet> GetWalletByUserIdAsync(string userId)
        {
            return await _repository.GetWalletByUserIdAsync(userId);
        }

        public async Task<BE.Wallet> CreateWalletAsync(string userId)
        {
            return await _repository.CreateWalletAsync(userId);
        }

        public async Task<decimal> GetBalanceAsync(string userId)
        {
            return await _repository.GetBalanceAsync(userId);
        }

        public async Task<decimal> DepositAsync(string userId, decimal amount, string description = null!)
        {
            return await _repository.DepositAsync(userId, amount, description);
        }

        public async Task<decimal> WithdrawAsync(string userId, decimal amount, string description = null!)
        {
            return await _repository.WithdrawAsync(userId, amount, description);
        }

        public async Task<bool> HasSufficientBalanceAsync(string userId, decimal amount)
        {
            return await _repository.HasSufficientBalanceAsync(userId, amount);
        }

        public async Task<IEnumerable<WalletTransaction>> GetTransactionHistoryAsync(string userId, int count = 10)
        {
            return await _repository.GetTransactionHistoryAsync(userId, count);
        }

        public async Task<WalletTransaction> GetTransactionByIdAsync(int transactionId)
        {
            return await _repository.GetTransactionByIdAsync(transactionId);
        }
    }
}
