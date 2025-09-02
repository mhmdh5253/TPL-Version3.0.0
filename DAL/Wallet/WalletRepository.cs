using BE;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL.Wallet
{

    // اینترفیس ریپازیتوری کیف پول
    public interface IWalletRepository
    {
        Task<BE.Wallet> GetWalletByUserIdAsync(string userId);
        Task<BE.Wallet> CreateWalletAsync(string userId);
        Task<decimal> GetBalanceAsync(string userId);
        Task<decimal> DepositAsync(string userId, decimal amount, string description = null);
        Task<decimal> WithdrawAsync(string userId, decimal amount, string description = null);
        Task<bool> HasSufficientBalanceAsync(string userId, decimal amount);
        Task<IEnumerable<WalletTransaction>> GetTransactionHistoryAsync(string userId, int count = 10);
        Task<WalletTransaction> GetTransactionByIdAsync(int transactionId);
    }

    public class WalletRepository : IWalletRepository
    {
        private readonly Db _context;
        private readonly ILogger<WalletRepository> _logger;

        public WalletRepository(Db context, ILogger<WalletRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BE.Wallet> GetWalletByUserIdAsync(string userId)
        {
            return (await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId))!;
        }

        public async Task<BE.Wallet> CreateWalletAsync(string userId)
        {
            var wallet = new BE.Wallet
            {
                UserId = userId,
                Balance = 0,
                LastUpdate = DateTime.Now
            };

            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();

            return wallet;
        }

        public async Task<decimal> GetBalanceAsync(string userId)
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            return wallet?.Balance ?? 0;
        }

        public async Task<decimal> DepositAsync(string userId, decimal amount, string description = null)
        {
            if (amount <= 0)
                throw new ArgumentException("مبلغ واریز باید بیشتر از صفر باشد.");

            var wallet = await GetWalletByUserIdAsync(userId) ?? await CreateWalletAsync(userId);

            wallet.Balance += amount;
            wallet.LastUpdate = DateTime.Now;

            var transaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = amount,
                Type = TransactionType.Deposit,
                Description = description ?? "واریز به کیف پول",
                TransactionDate = DateTime.Now,
                ReferenceId = Guid.NewGuid().ToString()
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Deposit of {amount} Rials to wallet {wallet.Id} for user {userId}");

            return wallet.Balance;
        }

        public async Task<decimal> WithdrawAsync(string userId, decimal amount, string description = null)
        {
            if (amount <= 0)
                throw new ArgumentException("مبلغ برداشت باید بیشتر از صفر باشد.");

            var wallet = await GetWalletByUserIdAsync(userId);
            if (wallet == null)
                throw new InvalidOperationException("کیف پول پیدا نشد.");

            if (wallet.Balance < amount)
                throw new InvalidOperationException("موجودی کافی نیست.");

            wallet.Balance -= amount;
            wallet.LastUpdate = DateTime.Now;

            var transaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = amount,
                Type = TransactionType.Withdrawal,
                Description = description ?? "برداشت از کیف پول",
                TransactionDate = DateTime.Now,
                ReferenceId = Guid.NewGuid().ToString()
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Withdrawal of {amount} Rials from wallet {wallet.Id} for user {userId}");

            return wallet.Balance;
        }

        public async Task<bool> HasSufficientBalanceAsync(string userId, decimal amount)
        {
            var balance = await GetBalanceAsync(userId);
            return balance >= amount;
        }

        public async Task<IEnumerable<WalletTransaction>> GetTransactionHistoryAsync(string userId, int count = 10)
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            if (wallet == null)
                return Enumerable.Empty<WalletTransaction>();

            return await _context.WalletTransactions
                .Where(t => t.WalletId == wallet.Id)
                .OrderByDescending(t => t.TransactionDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<WalletTransaction> GetTransactionByIdAsync(int transactionId)
        {
            return (await _context.WalletTransactions
                .FirstOrDefaultAsync(t => t.Id == transactionId))!;
        }
    }
}
