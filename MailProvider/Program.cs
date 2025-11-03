using MailProvider.Interfaces;
using MailProvider.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Session timeout
    options.Cookie.HttpOnly = true; // Security: prevent client-side script access
    options.Cookie.IsEssential = true; // Required for GDPR compliance
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Use HTTPS only
    options.Cookie.SameSite = SameSiteMode.Lax; // CSRF protection
    options.Cookie.Name = ".MyApp.Session"; // Custom session cookie name
});
builder.Services.AddScoped<GoogleService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
