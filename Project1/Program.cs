using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Project1.Models;
using Project1.Repository_Menu;
using Project1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Login/Index";
    options.LogoutPath = "/Login/Logout";
    options.Cookie.Name = "PizzaOnlineCookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ReturnUrlParameter = "returnUrl";
})
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

var connectionString = builder.Configuration.GetConnectionString("PizzaOnlineContext");
builder.Services.AddDbContext<PizzaOnlineContext>(x=>x.UseSqlServer(connectionString));
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();

//Thêm Session
//Begin
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian tồn tại của session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
//End

// đăng ký VnPayClient dạng Singleton() - chỉ có 1 instance duy nhất trong toàn ứng dụng
builder.Services.AddSingleton<IVnPayService, VnPayService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//kích hoạt session
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Welcome}/{action=WelcomePage}/{id?}");

app.Run();
