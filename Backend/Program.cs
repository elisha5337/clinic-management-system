using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ──────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password policy — reasonable for a clinic MVP
    options.Password.RequiredLength         = 8;
    options.Password.RequireDigit           = true;
    options.Password.RequireUppercase       = true;
    options.Password.RequireNonAlphanumeric = false;

    // Lock out after 5 failed attempts for 15 minutes
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── Auth cookie — redirect to our custom login page ───────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath        = "/Account/Login";
    options.LogoutPath       = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan   = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ── Authorization policies ────────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ReceptionistOnly", p => p.RequireRole("Receptionist"));
    options.AddPolicy("NurseOnly",        p => p.RequireRole("Nurse"));
    options.AddPolicy("DoctorOnly",       p => p.RequireRole("Doctor"));
    options.AddPolicy("ClinicalStaff",    p => p.RequireRole("Nurse", "Doctor"));
});

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── Seed roles and default users on startup ───────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Clean 404 page for any unmatched route
app.UseStatusCodePagesWithReExecute("/Home/NotFound");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // Must come before UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
