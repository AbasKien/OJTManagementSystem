using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OJTManagementSystem.Data;
using OJTManagementSystem.Helpers;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository;
using OJTManagementSystem.Repository.Interfaces;
using OJTManagementSystem.Services;
using OJTManagementSystem.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

// ═══════════════════════════════════════════════════════════
// REPOSITORIES
// ═══════════════════════════════════════════════════════════
builder.Services.AddScoped<IGenericRepository<ApplicationUser>, GenericRepository<ApplicationUser>>();
builder.Services.AddScoped<IInternRepository, InternRepository>();
builder.Services.AddScoped<ISupervisorRepository, SupervisorRepository>();
builder.Services.AddScoped<IDailyTimeRecordRepository, DailyTimeRecordRepository>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ICertificateRepository, CertificateRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

// ✅ ADD THESE NEW REPOSITORIES FOR GROUP CHAT
builder.Services.AddScoped<IGroupChatRepository, GroupChatRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// ═══════════════════════════════════════════════════════════
// SERVICES
// ═══════════════════════════════════════════════════════════
builder.Services.AddScoped<IInternService, InternService>();
builder.Services.AddScoped<ISupervisorService, SupervisorService>();
builder.Services.AddScoped<IDtrService, DtrService>();
builder.Services.AddScoped<IEvaluationService, EvaluationService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// ✅ ADD THESE NEW SERVICES FOR GROUP CHAT
builder.Services.AddScoped<IGroupChatService, GroupChatService>();

// ═══════════════════════════════════════════════════════════
// HELPERS
// ═══════════════════════════════════════════════════════════
builder.Services.AddScoped<PdfGeneratorHelper>();

// ═══════════════════════════════════════════════════════════
// EMAIL CONFIGURATION
// ═══════════════════════════════════════════════════════════
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ✅ Only use HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ✅ SEEDING DATA
Console.WriteLine("\n");
Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
Console.WriteLine("║          STARTING DATABASE SEEDING PROCESS                 ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
Console.WriteLine("\n");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    try
    {
        Console.WriteLine("📦 Running database migrations...");
        context.Database.Migrate();
        Console.WriteLine("✅ Migrations completed!");
        Console.WriteLine("");

        // ✅ SEED ROLES
        Console.WriteLine("🔐 Seeding roles...");
        var roles = new[] { "Supervisor", "Intern" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"   ✅ Role '{role}' created");
            }
            else
            {
                Console.WriteLine($"   ℹ️  Role '{role}' already exists");
            }
        }
        Console.WriteLine("");

        // ✅ SEED SUPERVISOR USER
        var supervisorEmail = "supervisor@ojtsystem.com";
        Console.WriteLine($"👤 Checking supervisor account: {supervisorEmail}");

        var supervisorUser = await userManager.FindByEmailAsync(supervisorEmail);

        if (supervisorUser == null)
        {
            Console.WriteLine("   ℹ️  Supervisor user not found, creating...");

            var supervisor = new ApplicationUser
            {
                FirstName = "System",
                LastName = "Supervisor",
                Email = supervisorEmail,
                UserName = supervisorEmail,
                PhoneNumber = "1234567890",
                IsActive = true
            };

            var result = await userManager.CreateAsync(supervisor, "SupervisorPassword123!");

            if (result.Succeeded)
            {
                Console.WriteLine("   ✅ Supervisor user created");

                // Add role
                var roleResult = await userManager.AddToRoleAsync(supervisor, "Supervisor");
                if (roleResult.Succeeded)
                {
                    Console.WriteLine("   ✅ Supervisor role assigned");
                }
                else
                {
                    Console.WriteLine("   ❌ Failed to assign role");
                    foreach (var error in roleResult.Errors)
                    {
                        Console.WriteLine($"      Error: {error.Description}");
                    }
                }

                // ✅ CREATE SUPERVISOR RECORD WITH POSITION
                Console.WriteLine("   📝 Creating supervisor database record...");
                var supervisorRecord = new Supervisor
                {
                    UserId = supervisor.Id,
                    Position = "Supervisor",  // ✅ FIXED: Added Position
                    Department = "Administration",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Supervisors.Add(supervisorRecord);
                await context.SaveChangesAsync();
                Console.WriteLine("   ✅ Supervisor record created in database");

                supervisorUser = supervisor;
            }
            else
            {
                Console.WriteLine("   ❌ Failed to create supervisor user!");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"      Error: {error.Description}");
                }
            }
        }
        else
        {
            Console.WriteLine("   ℹ️  Supervisor user already exists");
        }

        // ✅ Verify Supervisor Record Exists
        if (supervisorUser != null)
        {
            Console.WriteLine("\n🔍 Verifying supervisor record in database...");
            var supervisorRecord = await context.Supervisors
                .FirstOrDefaultAsync(s => s.UserId == supervisorUser.Id);

            if (supervisorRecord == null)
            {
                Console.WriteLine("   ⚠️  Supervisor record NOT found, creating now...");
                supervisorRecord = new Supervisor
                {
                    UserId = supervisorUser.Id,
                    Position = "Supervisor",  // ✅ FIXED: Added Position
                    Department = "Administration",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Supervisors.Add(supervisorRecord);
                await context.SaveChangesAsync();
                Console.WriteLine("   ✅ Supervisor record created!");
            }
            else
            {
                Console.WriteLine("   ✅ Supervisor record exists in database");
            }
        }

        Console.WriteLine("\n");
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║        ✅ SEEDING COMPLETED SUCCESSFULLY!                  ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║                                                            ║");
        Console.WriteLine("║  📧 Supervisor Email:    supervisor@ojtsystem.com          ║");
        Console.WriteLine("║  🔐 Supervisor Password: SupervisorPassword123!            ║");
        Console.WriteLine("║  👔 Position:            Supervisor                        ║");
        Console.WriteLine("║                                                            ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine("\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine("\n");
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║             ❌ ERROR DURING SEEDING                         ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"\n❌ Error: {ex.Message}");
        Console.WriteLine($"\n📌 Details: {ex.InnerException?.Message}");
        Console.WriteLine($"\n🔍 Stack Trace:\n{ex.StackTrace}");
        Console.WriteLine("\n");
    }
}

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Application error: {ex.Message}");
}