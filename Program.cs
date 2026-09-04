using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Bravo.Data;
using Bravo.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. دعم الـ Controllers مع شاشات العرض (Views) بدلاً من الـ API
builder.Services.AddControllersWithViews();

// 2. ربط قاعدة البيانات SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

// 3. إعداد نظام Identity لإدارة المستخدمين
builder.Services.AddIdentity<Bravo.Models.AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 4. إعداد نظام المصادقة عبر ملفات تعريف الارتباط (Cookies)
// هذا الإعداد يخبر النظام أين يوجه المستخدم إذا حاول الدخول لصفحة محمية
builder.Services.ConfigureApplicationCookie(options =>
{
    // إخبار النظام بالمسار الجديد لصفحة تسجيل الدخول
    options.LoginPath = "/Auth/Account/Login";

    // إخبار النظام بالمسار الجديد لصفحة رفض الصلاحيات
    options.AccessDeniedPath = "/Auth/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});
builder.Services.AddRazorPages();

var app = builder.Build();

// 5. إعداد بيئة التطوير والإنتاج لمعالجة الأخطاء
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// 6. تفعيل قراءة الملفات الثابتة (مهم جداً لتشغيل ملفات CSS و JavaScript والصور)
app.UseStaticFiles();

app.UseRouting();

// 7. الترتيب الحساس: التحقق من الهوية قبل الصلاحيات
app.UseAuthentication();
app.UseAuthorization();

// 8. إعداد نظام التوجيه (Routing) الافتراضي الخاص بـ MVC
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // ضروري إذا كنت تستخدم Identity

app.Run();