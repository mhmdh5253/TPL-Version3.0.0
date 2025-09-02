using System.ComponentModel.DataAnnotations;

namespace BE
{
    // مدل کیف پول
    public class Wallet
    {
        public int Id { get; set; }
        public string? UserId { get; set; } // شناسه کاربر
        public decimal Balance { get; set; } // موجودی به ریال
        public DateTime LastUpdate { get; set; }
    }

    // مدل تراکنش
    public class WalletTransaction
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public decimal Amount { get; set; } // مبلغ به ریال
        public TransactionType Type { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? ReferenceId { get; set; } // شناسه مرجع برای پیگیری
    }

    public enum TransactionType
    {
        Deposit,    // واریز
        Withdrawal, // برداشت
        Purchase,   // خرید
        Refund      // بازپرداخت
    }


    public class DepositRequest
    {
        [Display(Name = "مبلغ")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1000, 100000000, ErrorMessage = "مبلغ باید بین 1,000 تا 100,000,000 ریال باشد")]
        public decimal Amount { get; set; } // مبلغ به ریال

        public string? Description { get; set; }
    }

    public class WithdrawRequest
    {
        [Display(Name = "مبلغ")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1000, 100000000, ErrorMessage = "مبلغ باید بین 1,000 تا 100,000,000 ریال باشد")]
        public decimal Amount { get; set; } // مبلغ به ریال

        public string? Description { get; set; }
    }
}
