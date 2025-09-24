// ====================================================================================
// فایل اصلی برنامه - تنظیمات و پیکربندی سرویس‌ها
// ====================================================================================

using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using BE;
using BLL.Calendar;
using BLL.Chat;
using BLL.LetterAutomation;
using BLL.Ticketing;
using BLL.Tokening;
using BLL.Wallet;
using DAL;
using DAL.Calendar;
using DAL.LetterAutomation;
using DAL.Ticketing;
using DAL.Tokening;
using DAL.Wallet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using TPLWeb.Hubs;
using TPLWeb.Services.Sms;
using TPLWeb.Tools;
using WebMarkupMin.AspNetCore3;
using static TPLWeb.Tools.RenderViewToString;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace TPLWeb
{
    /// <summary>
    /// کلاس اصلی برنامه - نقطه شروع اپلیکیشن
    /// </summary>
    public class Program
    {
        /// <summary>
        /// نقطه شروع برنامه - متد اصلی
        /// </summary>
        /// <param name="args">پارامترهای ورودی برنامه</param>
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Apply EF Core migrations on startup (code-first create/update)
            using (var scope = host.Services.CreateScope())
            {
                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<Db>();
                    db.Database.Migrate();
                }
                catch (Exception ex)
                {
                    // Log and rethrow or continue based on policy; here we log to console
                    Console.WriteLine($"Database migration failed: {ex.Message}");
                    throw;
                }
            }

            host.Run();
        }

        /// <summary>
        /// ایجاد و پیکربندی میزبان برنامه
        /// </summary>
        /// <param name="args">پارامترهای ورودی</param>
        /// <returns>سازنده میزبان برنامه</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    /// <summary>
    /// کلاس راه‌اندازی و پیکربندی سرویس‌ها
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// پیکربندی برنامه
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// سازنده کلاس Startup
        /// </summary>
        /// <param name="configuration">پیکربندی برنامه</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// پیکربندی سرویس‌های برنامه
        /// </summary>
        /// <param name="services">کالکشن سرویس‌ها</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // ====================================================================================
            // ثبت سرویس‌های پایه و اصلی
            // ====================================================================================

            // ثبت سرویس‌های DAL (Data Access Layer)
            services.AddScoped<DlLetter>();

            // ثبت سرویس‌های BLL (Business Logic Layer)
            services.AddScoped<ILetterService, BlLetter>();

            // پیکربندی کنترلرها و ویوها با قابلیت کامپایل در زمان اجرا
            services.AddControllersWithViews(options =>
                {
                    // Persian date binder for DateTime/DateOnly
                    options.ModelBinderProviders.Insert(0, new PersianDateModelBinderProvider());
                })
                .AddRazorRuntimeCompilation()
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

            // ====================================================================================
            // پیکربندی سرویس‌های اصلی
            // ====================================================================================

            // پیکربندی سرویس Identity برای احراز هویت
            ConfigureIdentity(services);

            // پیکربندی سرویس Logger برای ثبت رویدادها
            ConfigureLogger();

            // پیکربندی سرویس Minify کردن HTML
            ConfigureHtmlMinification(services);

            // ====================================================================================
            // پیکربندی سرویس‌های اضافی
            // ====================================================================================

            // فعال‌سازی کش پاسخ‌ها
            services.AddResponseCaching();

            // پیکربندی انکودر HTML برای پشتیبانی از زبان فارسی
            services.AddSingleton(HtmlEncoder.Create(new[] { UnicodeRanges.BasicLatin, UnicodeRanges.Arabic }));

            // اضافه کردن Serilog برای لاگینگ
            services.AddSerilog();

            // پیکربندی MVC: از AddControllersWithViews بالا استفاده شده است

            // اضافه کردن سرویس HTTP Client
            services.AddHttpClient();

            // اضافه کردن قابلیت Progressive Web App
            services.AddProgressiveWebApp();

            // پیکربندی Session با تنظیمات امنیتی
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20); // زمان انقضای session
                options.Cookie.HttpOnly = true; // امنیت کوکی
                options.Cookie.IsEssential = true; // کوکی ضروری
            });

            // پیکربندی سایر سرویس‌ها
            ConfigureAdditionalServices(services);

            // فرهنگ پیش‌فرض برنامه: fa-IR
            var supportedCultures = new[] { new CultureInfo("fa-IR") };
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("fa-IR");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
        }

        /// <summary>
        /// پیکربندی سرویس Identity برای احراز هویت و مدیریت کاربران
        /// </summary>
        /// <param name="services">کالکشن سرویس‌ها</param>
        private void ConfigureIdentity(IServiceCollection services)
        {
            // پیکربندی دیتابیس با Entity Framework
            services.AddDbContext<Db>(options => options.UseSqlServer(Configuration.GetConnectionString("CON1")));

            // پیکربندی Identity با تنظیمات امنیتی
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // تنظیمات کاربر
                options.User.RequireUniqueEmail = false; // ایمیل یکتا الزامی نیست
                
                // تنظیمات رمز عبور
                options.Password.RequireUppercase = true; // حروف بزرگ الزامی
                options.Password.RequiredUniqueChars = 0; // کاراکترهای یکتا
                options.Password.RequireLowercase = true; // حروف کوچک الزامی
                options.Password.RequireDigit = true; // اعداد الزامی
                options.Password.RequireNonAlphanumeric = true; // کاراکترهای خاص الزامی
                options.Password.RequiredLength = 8; // حداقل طول رمز عبور
                
                // تنظیمات ورود
                options.SignIn.RequireConfirmedEmail = false; // تایید ایمیل الزامی نیست
                options.SignIn.RequireConfirmedPhoneNumber = false; // تایید شماره تلفن الزامی نیست
                options.SignIn.RequireConfirmedAccount = false; // تایید حساب الزامی نیست
                
                // تنظیمات قفل حساب
                options.Lockout.AllowedForNewUsers = false; // قفل برای کاربران جدید غیرفعال
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(20); // زمان قفل
                options.Lockout.MaxFailedAccessAttempts = 10; // حداکثر تلاش‌های ناموفق
                
                // تنظیمات محافظت از داده‌ها
                options.Stores.ProtectPersonalData = false; // محافظت از داده‌های شخصی غیرفعال
            })
            .AddEntityFrameworkStores<Db>() // استفاده از Entity Framework
            .AddDefaultTokenProviders() // ارائه‌دهندگان توکن پیش‌فرض
            .AddErrorDescriber<PersianIdentityErrors>(); // پیام‌های خطا به فارسی

            // پیکربندی کوکی احراز هویت
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Account/AccessDenied"; // مسیر دسترسی رد شده
                options.LoginPath = "/Account/Login"; // مسیر ورود
                options.LogoutPath = "/Account/LogOut"; // مسیر خروج
                options.Cookie.HttpOnly = false; // دسترسی JavaScript به کوکی
                options.ExpireTimeSpan = TimeSpan.FromDays(10); // زمان انقضای کوکی
            });

            // پیکربندی مجوزها و نقش‌ها
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("SuperAdmin")); // سیاست ادمین
            });
        }

        /// <summary>
        /// پیکربندی سرویس Logger با Serilog
        /// </summary>
        private void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Seq(Configuration.GetConnectionString("SeqLoging")!) // ارسال لاگ به Seq
                .CreateLogger();
        }

        /// <summary>
        /// پیکربندی سرویس Minify کردن HTML
        /// </summary>
        /// <param name="services">کالکشن سرویس‌ها</param>
        private void ConfigureHtmlMinification(IServiceCollection services)
        {
            services.AddWebMarkupMin(options =>
            {
                options.AllowCompressionInDevelopmentEnvironment = true; // فشرده‌سازی در محیط توسعه
                options.AllowMinificationInDevelopmentEnvironment = true; // مینیفای در محیط توسعه
            })
                .AddHtmlMinification() // مینیفای HTML
                .AddHttpCompression() // فشرده‌سازی HTTP
                .AddXhtmlMinification() // مینیفای XHTML
                .AddXmlMinification(); // مینیفای XML
        }

        /// <summary>
        /// پیکربندی سایر سرویس‌های برنامه
        /// </summary>
        /// <param name="services">کالکشن سرویس‌ها</param>
        private void ConfigureAdditionalServices(IServiceCollection services)
        {
            // ====================================================================================
            // سرویس‌های عمومی و ابزاری
            // ====================================================================================

            // سرویس‌های ابزاری
            services.AddSingleton<ControllerActionService>(); // سرویس عملیات کنترلر
            services.AddTransient<ISmsSender, SmsSender>(); // سرویس ارسال پیامک
            services.AddTransient<IEmailSender, EmailSender>(); // سرویس ارسال ایمیل
            services.AddTransient<IViewRenderService, ViewRenderService>(); // سرویس رندر ویو

            // سرویس محافظت از داده‌ها
            services.AddDataProtection();

            // ====================================================================================
            // سرویس‌های کیف پول (Wallet)
            // ====================================================================================

            services.AddScoped<IWalletRepository, WalletRepository>(); // ریپوزیتوری کیف پول
            services.AddScoped<BlWallet>(); // سرویس منطق کسب و کار کیف پول

            // ====================================================================================
            // سرویس‌های تیکتینگ (Ticketing)
            // ====================================================================================

            // سرویس‌های تیکت
            services.AddScoped<ITicketService, DlTicket>(); // سرویس تیکت
            services.AddScoped<BlTicket>(); // منطق کسب و کار تیکت
            services.AddScoped<DlTicket>(); // دسترسی به داده تیکت

            // سرویس‌های کامنت
            services.AddScoped<ICommentService, DlComment>(); // سرویس کامنت
            services.AddScoped<BlComment>(); // منطق کسب و کار کامنت
            services.AddScoped<DlComment>(); // دسترسی به داده کامنت

            // سرویس‌های پیوست
            services.AddScoped<IAttachmentService, DlAttachment>(); // سرویس پیوست
            services.AddScoped<BlAttachment>(); // منطق کسب و کار پیوست
            services.AddScoped<DlAttachment>(); // دسترسی به داده پیوست

            // سرویس‌های دسته‌بندی
            services.AddScoped<ICategoryService, DlCategory>(); // سرویس دسته‌بندی
            services.AddScoped<BlCategory>(); // منطق کسب و کار دسته‌بندی
            services.AddScoped<DlCategory>(); // دسترسی به داده دسته‌بندی

            // سرویس‌های گزارش تیکت
            services.AddScoped<ITicketReport, DlTicketReport>(); // سرویس گزارش تیکت
            services.AddScoped<BlTicketReport>(); // منطق کسب و کار گزارش تیکت
            services.AddScoped<DlTicketReport>(); // دسترسی به داده گزارش تیکت

            // سرویس‌های اعلان
            services.AddScoped<INotificationService, DlNotification>(); // سرویس اعلان
            services.AddScoped<BlNotification>(); // منطق کسب و کار اعلان
            services.AddScoped<DlNotification>(); // دسترسی به داده اعلان

            // ====================================================================================
            // سرویس‌های توکن (Tokening)
            // ====================================================================================

            services.AddScoped<ITokenRepository, DlToken>(); // ریپوزیتوری توکن
            services.AddScoped<BlToken>(); // منطق کسب و کار توکن
            services.AddScoped<DlToken>(); // دسترسی به داده توکن

            // ====================================================================================
            // سرویس‌های اتوماسیون نامه (Letter Automation)
            // ====================================================================================

            // سرویس‌های کلاسه‌نامه
            services.AddScoped<IKelasehnamehRepository, DlKelasehnameh>(); // ریپوزیتوری کلاسه‌نامه
            services.AddScoped<DlKelasehnameh>(); // دسترسی به داده کلاسه‌نامه
            services.AddScoped<Blkelaseh>(); // منطق کسب و کار کلاسه‌نامه

            // سرویس‌های سازمان
            services.AddScoped<IOrganizationRepository, DlOrganization>(); // ریپوزیتوری سازمان
            services.AddScoped<DlOrganization>(); // دسترسی به داده سازمان
            services.AddScoped<BlRecivers>(); // منطق کسب و کار گیرندگان

            // سرویس‌های تایید نامه
            services.AddScoped<ILetterApprovalService, LetterApprovalService>(); // سرویس تایید نامه
            services.AddScoped<LetterApprovalService>(); // سرویس تایید نامه
            services.AddScoped<BlLetterApprovalService>(); // منطق کسب و کار تایید نامه

            // سرویس‌های تایید عمومی
            services.AddScoped<IApprovalService, ApprovalService>(); // سرویس تایید عمومی
            services.AddScoped<IOrganizationService, OrganizationService>(); // سرویس سازمان
            services.AddScoped<OrganizationService>(); // سرویس سازمان
            services.AddScoped<ApprovalService>(); // سرویس تایید

            // سرویس نامه
            services.AddScoped<BlLetter>(); // منطق کسب و کار نامه

            // ====================================================================================
            // سرویس‌های تقویم (Calendar)
            // ====================================================================================

            services.AddScoped<ICalendarEventRepository, CalendarEventRepository>(); // ریپوزیتوری رویدادهای تقویم
            services.AddScoped<ICalendarCategoryRepository, CalendarCategoryRepository>(); // ریپوزیتوری دسته‌بندی‌های تقویم
            services.AddScoped<ICalendarService, CalendarService>(); // سرویس تقویم

            // ====================================================================================
            // سرویس‌های چت (Chat)
            // ====================================================================================

            services.AddScoped<BLL.Chat.IChatService, BLL.Chat.ChatService>(); // سرویس چت
            services.AddScoped<IChatNotificationService, ChatNotificationService>(); // سرویس اعلان چت

            // ====================================================================================
            // سرویس‌های پیامک (SMS)
            // ====================================================================================

            services.AddScoped<ISmsSender, SmsSender>(); // سرویس ارسال پیامک

            // ====================================================================================
            // سرویس‌های پس‌زمینه (Background Services)
            // ====================================================================================

            services.AddHostedService<TPLWeb.Services.ReminderBackgroundService>(); // سرویس ارسال یادآوری‌ها

            // ====================================================================================
            // سرویس‌های ارتباطی و سیگنال
            // ====================================================================================

            // SignalR برای ارتباطات real-time
            services.AddSignalR();

            // دسترسی به HTTP Context برای سرویس تقویم
            services.AddHttpContextAccessor();
        }

        /// <summary>
        /// پیکربندی میدلورها و خط لوله درخواست‌ها
        /// </summary>
        /// <param name="app">سازنده اپلیکیشن</param>
        /// <param name="env">محیط اجرا</param>
        /// <param name="dbContext">کانتکست دیتابیس</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Db dbContext)
        {
            // ====================================================================================
            // پیکربندی محیط اجرا
            // ====================================================================================

            if (env.IsDevelopment())
            {
                // صفحه خطا برای محیط توسعه
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // صفحه خطا برای محیط تولید
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts(); // HTTP Strict Transport Security
            }

            // اجرای میگریشن‌های دیتابیس برای ثبت تغییرات (اضافی؛ Main نیز انجام می‌دهد)
            dbContext.Database.Migrate();

            // ====================================================================================
            // پیکربندی میدلورهای امنیتی و عملکرد
            // ====================================================================================

            // Localization (fa-IR)
            var locOptions = app.ApplicationServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            // ریدایرکت HTTPS
            app.UseHttpsRedirection();

            // کش پاسخ‌ها
            app.UseResponseCaching();

            // مینیفای کردن محتوا
            app.UseWebMarkupMin();

            // ====================================================================================
            // پیکربندی فایل‌های استاتیک با کش پیشرفته
            // ====================================================================================

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    var path = ctx.File.PhysicalPath;
                    if (path != null)
                    {
                        var extension = Path.GetExtension(path).ToLowerInvariant();
                        var fileName = Path.GetFileName(path);

                        // Cache busting برای فایل‌های bundle
                        if (fileName.Contains("bundle.min") || fileName.Contains("vendor.min"))
                        {
                            // اضافه کردن پارامتر نسخه برای cache busting
                            var fileInfo = new FileInfo(path);
                            var version = fileInfo.LastWriteTimeUtc.Ticks.ToString();

                            // تنظیم کش کوتاه برای فایل‌های bundle در محیط توسعه
                            if (env.IsDevelopment())
                            {
                                ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                                ctx.Context.Response.Headers.Append("Pragma", "no-cache");
                                ctx.Context.Response.Headers.Append("Expires", "0");
                            }
                            else
                            {
                                // تنظیم کش طولانی برای تولید با نسخه
                                ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age=31536000, immutable");
                                ctx.Context.Response.Headers.Append("ETag", $"\"{version}\"");
                            }
                        }
                        else
                        {
                            // کش عادی برای سایر فایل‌ها
                            var cacheTime = extension switch
                            {
                                ".css" or ".js" => "31536000", // 1 سال برای CSS/JS
                                ".png" or ".jpg" or ".jpeg" or ".gif" or ".svg" or ".ico" => "2592000", // 30 روز برای تصاویر
                                ".woff" or ".woff2" or ".ttf" or ".eot" => "31536000", // 1 سال برای فونت‌ها
                                _ => "86400" // 1 روز برای سایر فایل‌ها
                            };

                            ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cacheTime}");
                            ctx.Context.Response.Headers.Append("Expires", new DateTimeOffset(DateTime.UtcNow.AddYears(1)).ToString("R"));

                            // اضافه کردن هدرهای فشرده‌سازی
                            if (extension == ".css" || extension == ".js" || extension == ".html")
                            {
                                ctx.Context.Response.Headers.Append("Vary", "Accept-Encoding");
                            }
                        }
                    }
                }
            });

            // ====================================================================================
            // پیکربندی میدلورهای احراز هویت و مجوز
            // ====================================================================================

            // Session
            app.UseSession();

            // Routing
            app.UseRouting();

            // احراز هویت
            app.UseAuthentication();

            // مجوزدهی
            app.UseAuthorization();

            // میدلور مجوزهای سفارشی
            app.UseMiddleware<PermissionMiddleware>();

            // ریدایرکت HTTPS مجدد
            app.UseHttpsRedirection();

            // ====================================================================================
            // پیکربندی نقاط پایانی (Endpoints)
            // ====================================================================================

            app.UseEndpoints(endpoints =>
            {
                // مسیر پیش‌فرض کنترلر
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Hub چت برای ارتباطات real-time
                endpoints.MapHub<ChatHub>("/chatHub");
            });
        }
    }
}
