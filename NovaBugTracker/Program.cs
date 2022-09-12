using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NovaBugTracker.Data;
using NovaBugTracker.Extensions;
using NovaBugTracker.Models;
using NovaBugTracker.Services;
using NovaBugTracker.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = DataUtility.GetConnectionString(builder.Configuration);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString,
    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
    ));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<BTUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddClaimsPrincipalFactory<BTUserClaimsPrincipalFactory>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

//custom services
builder.Services.AddScoped<IBTFileService, BTFileService>();
builder.Services.AddScoped<IBTRolesService, BTRolesService>();
builder.Services.AddScoped<IBTProjectService, BTProjectService>();
builder.Services.AddScoped<IBTTicketService, BTTicketService> ();
builder.Services.AddScoped<IBTCompanyService, BTCompanyService>();
builder.Services.AddScoped<IEmailSender, BTEmailService>();
builder.Services.AddScoped<IBTInviteService, BTInviteService>();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));


builder.Services.AddMvc();

var app = builder.Build();

var scope = app.Services.CreateScope();
await DataUtility.ManageDataAsync(scope.ServiceProvider);


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
