using Microsoft.AspNetCore.Identity;

namespace TPLWeb.Tools
{
    public class PersianIdentityErrors : IdentityErrorDescriber
    {
        public override IdentityError DuplicateEmail(string? email)
        {
            return new IdentityError()
            {
                Code = nameof(DuplicateEmail),
                Description = $"توسط شخص دیگری انتخاب شده است '{email}' ایمیل"
            };
        }

        public override IdentityError DuplicateUserName(string? userName)
        {
            return new IdentityError()
            {
                Code = nameof(DuplicateUserName),
                Description = $"توسط شخص دیگری انتخاب شده است '{userName}' نام کاربری"
            };
        }

        public override IdentityError InvalidEmail(string? email)
        {
            return new IdentityError()
            {
                Code = nameof(InvalidEmail),
                Description = $"یک ایمیل معتبر نیست '{email}' ایمیل"
            };
        }

        public override IdentityError DuplicateRoleName(string? role)
        {
            return new IdentityError()
            {
                Code = nameof(DuplicateRoleName),
                Description = $"قبلا ثبت شده است '{role}' مقام"
            };
        }

        public override IdentityError InvalidRoleName(string? role)
        {
            return new IdentityError()
            {
                Code = nameof(InvalidRoleName),
                Description = $"معتبر نیست '{role}' نام"
            };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError()
            {
                Code = nameof(PasswordRequiresDigit),
                Description = "رمز عبور باید حداقل دارای یک عدد باشد"
            };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError()
            {
                Code = nameof(PasswordRequiresLower),
                Description = "رمز عبور باید حداقل دارای یک کاراکتر انگلیسی کوچک باشد ('a'-'z')"
            };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError()
            {
                Code = nameof(PasswordRequiresUpper),
                Description = "رمز عبور باید حداقل دارای یک کاراکتر انگلیسی بزرگ باشد ('A'-'Z')"
            };
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError()
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = "رمز عبور باید حداقل دارای یک کاراکتر ویژه باشد مثل '@#%^&'"
            };
        }

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
        {
            return new IdentityError()
            {
                Code = nameof(PasswordRequiresUniqueChars),
                Description = $"کاراکتر منحصر به فرد باشد {uniqueChars} رمز عبور باید حداقل دارای"
            };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError()
            {
                Code = nameof(PasswordTooShort),
                Description = $"رمز عبور نباید کمتر از {length} کاراکتر باشد"
            };
        }

        public override IdentityError InvalidUserName(string? UserName)
        {
            return new IdentityError()
            {
                Code = nameof(InvalidUserName),
                Description = $"معتبر نیست '{UserName}' نام کاربری"
            };
        }

        public override IdentityError UserNotInRole(string? role)
        {
            return new IdentityError()
            {
                Code = nameof(UserNotInRole),
                Description = $"نیست '{role}' کاربر مورد نظر در مقام"
            };
        }

        public override IdentityError UserAlreadyInRole(string? role)
        {
            return new IdentityError()
            {
                Code = nameof(UserAlreadyInRole),
                Description = $"است '{role}' کاربر مورد نظر در مقام"
            };
        }

        public override IdentityError DefaultError()
        {
            return new IdentityError()
            {
                Code = nameof(DefaultError),
                Description = "خطای پیشبینی نشده رخ داد"
            };
        }

        public override IdentityError ConcurrencyFailure()
        {
            return new IdentityError()
            {
                Code = nameof(ConcurrencyFailure),
                Description = "خطای همزمانی رخ داد"
            };
        }

        public override IdentityError InvalidToken()
        {
            return new IdentityError()
            {
                Code = nameof(InvalidToken),
                Description = "توکن معتبر نیست"
            };
        }

        public override IdentityError RecoveryCodeRedemptionFailed()
        {
            return new IdentityError()
            {
                Code = nameof(RecoveryCodeRedemptionFailed),
                Description = "کد بازیابی معتبر نیست"
            };
        }

        public override IdentityError UserLockoutNotEnabled()
        {
            return new IdentityError()
            {
                Code = nameof(UserLockoutNotEnabled),
                Description = "قابلیت قفل اکانت کاربر فعال نیست"
            };
        }

        public override IdentityError UserAlreadyHasPassword()
        {
            return new IdentityError()
            {
                Code = nameof(UserAlreadyHasPassword),
                Description = "کاربر از قبل رمزعبور دارد"
            };
        }

        public override IdentityError PasswordMismatch()
        {
            return new IdentityError()
            {
                Code = nameof(PasswordMismatch),
                Description = "عدم تطابق رمزعبور"
            };
        }

        public override IdentityError LoginAlreadyAssociated()
        {
            return new IdentityError()
            {
                Code = nameof(LoginAlreadyAssociated),
                Description = "از قبل اکانت خارجی به حساب این کاربر متصل است"
            };
        }
    }
}