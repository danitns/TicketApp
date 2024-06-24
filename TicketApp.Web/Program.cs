using Hangfire;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Stripe;
using TicketApp.BusinessLogic;
using TicketApp.DataAccess;
using TicketApp.Web.Code;
using TicketApp.WebApp.Code;
using TicketApp.WebApp.Code.ExtensionMethods;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(typeof(GlobalExceptionFilterAttribute));
    options.Filters.Add(typeof(CheckUserFilter));
});

builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(BaseService).Assembly);

builder.Services.AddDbContext<TicketAppContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));
builder.Services.AddHangfireServer();

builder.Services.AddScoped<UnitOfWork>();

builder.Services.AddTicketAppCurrentUser();

builder.Services.AddPresentation();
builder.Services.AddTicketAppBusinessLogic();


builder.Services.AddAuthentication("TicketAppCookies")
       .AddCookie("TicketAppCookies", options =>
       {
           options.AccessDeniedPath = new PathString("/UserAccount/Login");
           options.LoginPath = new PathString("/UserAccount/Login");
       });

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    //options.Cookie.Name = "ThemeCookie";
    options.IdleTimeout = TimeSpan.FromSeconds(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHangfireDashboard();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

StripeConfiguration.ApiKey = "sk_test_51Ng1aJEGHVemmuZFad9QBEtTPys3sE2JIeze5cDiLUrQ2xbyKABHW5sq48VT6bRkhuai8Q72OH5bndsK4W2QxgBn00uVEEu6XE";

app.Run();
