using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OJTManagementSystem.Data;
using OJTManagementSystem.Enums;
using OJTManagementSystem.Helpers;
using OJTManagementSystem.Hubs;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository;
using OJTManagementSystem.Repository.Interfaces;
using OJTManagementSystem.Services;
using OJTManagementSystem.Services.Interfaces;
using OJTManagementSystem.Hubs;


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

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddControllersWithViews();

// ✅ FIX: Add Session support so notifications survive across redirects within a login session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8); // session lives as long as a workday
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//signal R
builder.Services.AddSignalR();


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
builder.Services.AddScoped<IGroupChatService, GroupChatService>();

// ═══════════════════════════════════════════════════════════
// HELPERS
// ═══════════════════════════════════════════════════════════
builder.Services.AddScoped<PdfGeneratorHelper>();

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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();

// ✅ FIX: UseSession must be AFTER UseRouting and BEFORE UseAuthorization
app.UseSession();

app.UseAuthorization();

// ✅ ADD THIS - Map SignalR Hub
app.MapHub<ChatHub>("/chatHub");  
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ════════════════════════════════════════════════════════════════════
// SEEDING DATA
// ════════════════════════════════════════════════════════════════════
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
        // ── MIGRATIONS ──────────────────────────────────────────────
        Console.WriteLine("📦 Running database migrations...");
        context.Database.Migrate();
        Console.WriteLine("✅ Migrations completed!\n");

        // ── ROLES ────────────────────────────────────────────────────
        Console.WriteLine("🔐 Seeding roles...");
        foreach (var role in new[] { "Supervisor", "Intern" })
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

        // ── SUPERVISOR ACCOUNT ───────────────────────────────────────
        Console.WriteLine("👔 Seeding supervisor account...");
        var supervisorEmail = "supervisor@ojtsystem.com";
        var supervisorUser = await userManager.FindByEmailAsync(supervisorEmail);

        if (supervisorUser == null)
        {
            supervisorUser = new ApplicationUser
            {
                FirstName = "System",
                LastName = "Supervisor",
                Email = supervisorEmail,
                UserName = supervisorEmail,
                PhoneNumber = "1234567890",
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(supervisorUser, "SupervisorPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(supervisorUser, "Supervisor");
                Console.WriteLine("   ✅ Supervisor user created & email confirmed");
            }
            else
            {
                foreach (var e in result.Errors)
                    Console.WriteLine($"   ❌ {e.Description}");
            }
        }
        else
        {
            if (!supervisorUser.EmailConfirmed)
            {
                supervisorUser.EmailConfirmed = true;
                await userManager.UpdateAsync(supervisorUser);
                Console.WriteLine("   ✅ Existing supervisor email confirmed");
            }
            else
            {
                Console.WriteLine("   ℹ️  Supervisor user already exists");
            }
        }

        // ── SUPERVISOR DB RECORD ─────────────────────────────────────
        Console.WriteLine("   🔍 Verifying supervisor DB record...");
        var supervisorRecord = await context.Supervisors
            .FirstOrDefaultAsync(s => s.UserId == supervisorUser.Id);

        if (supervisorRecord == null)
        {
            supervisorRecord = new Supervisor
            {
                UserId = supervisorUser.Id,
                Position = "OJT Supervisor",
                Department = "Information Technology",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Supervisors.Add(supervisorRecord);
            await context.SaveChangesAsync();
            Console.WriteLine("   ✅ Supervisor DB record created");
        }
        else
        {
            Console.WriteLine("   ℹ️  Supervisor DB record already exists");
        }

        // ── INTERN ACCOUNT ───────────────────────────────────────────
        Console.WriteLine("\n🎓 Seeding intern account...");
        var internEmail = "intern@ojtsystem.com";
        var internUser = await userManager.FindByEmailAsync(internEmail);

        if (internUser == null)
        {
            internUser = new ApplicationUser
            {
                FirstName = "Juan",
                LastName = "dela Cruz",
                Email = internEmail,
                UserName = internEmail,
                PhoneNumber = "09123456789",
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(internUser, "InternPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(internUser, "Intern");
                Console.WriteLine("   ✅ Intern user created & email confirmed");
            }
            else
            {
                foreach (var e in result.Errors)
                    Console.WriteLine($"   ❌ {e.Description}");
            }
        }
        else
        {
            if (!internUser.EmailConfirmed)
            {
                internUser.EmailConfirmed = true;
                await userManager.UpdateAsync(internUser);
                Console.WriteLine("   ✅ Existing intern email confirmed");
            }
            else
            {
                Console.WriteLine("   ℹ️  Intern user already exists");
            }
        }

        // ── INTERN DB RECORD ─────────────────────────────────────────
        Console.WriteLine("   🔍 Verifying intern DB record...");

        var internStartDate = DateTime.Today.AddMonths(-6);
        var internEndDate = DateTime.Today.AddDays(-1);
        var totalOjtHours = 486m;

        var internRecord = await context.Interns
            .FirstOrDefaultAsync(i => i.UserId == internUser.Id);

        if (internRecord == null)
        {
            internRecord = new Intern
            {
                UserId = internUser.Id,
                StudentId = "2021-00001",
                School = "Batangas State University",
                Course = "Bachelor of Science in Information Technology",
                Department = "Information Technology",
                StartDate = internStartDate,
                EndDate = internEndDate,
                SupervisorId = supervisorRecord.SupervisorId,
                CompletedHours = totalOjtHours,
                IsActive = true,
                CreatedAt = internStartDate
            };
            context.Interns.Add(internRecord);
            await context.SaveChangesAsync();
            Console.WriteLine("   ✅ Intern DB record created & assigned to supervisor");
        }
        else
        {
            internRecord.SupervisorId = supervisorRecord.SupervisorId;
            internRecord.CompletedHours = totalOjtHours;
            internRecord.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            Console.WriteLine("   ℹ️  Intern record updated — supervisor assigned & hours set");
        }

        // ── DAILY TIME RECORDS (DTRs) ─────────────────────────────────
        Console.WriteLine("\n📋 Seeding Daily Time Records...");
        var dtrExists = await context.DailyTimeRecords
            .AnyAsync(d => d.InternId == internRecord.InternId);

        if (!dtrExists)
        {
            var dtrs = new List<DailyTimeRecord>();
            var current = internStartDate;

            while (current <= internEndDate)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday &&
                    current.DayOfWeek != DayOfWeek.Sunday)
                {
                    dtrs.Add(new DailyTimeRecord
                    {
                        InternId = internRecord.InternId,
                        RecordDate = current,
                        TimeIn = new TimeSpan(8, 0, 0),
                        TimeOut = new TimeSpan(17, 0, 0),
                        TotalHours = 9m,
                        ActivityDescription = $"Completed assigned tasks and projects for {current:MMMM dd, yyyy}.",
                        Status = DtrStatus.Approved,
                        ApprovedAt = current.AddDays(1),
                        CreatedAt = current,
                        UpdatedAt = current.AddDays(1)
                    });
                }
                current = current.AddDays(1);
            }

            context.DailyTimeRecords.AddRange(dtrs);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✅ {dtrs.Count} DTR records created (all Approved)");
        }
        else
        {
            Console.WriteLine("   ℹ️  DTR records already exist");
        }

        // ── EVALUATION ────────────────────────────────────────────────
        Console.WriteLine("\n⭐ Seeding Evaluation...");
        var evalExists = await context.Evaluations
            .AnyAsync(e => e.InternId == internRecord.InternId);

        if (!evalExists)
        {
            var evaluation = new Evaluation
            {
                InternId = internRecord.InternId,
                SupervisorId = supervisorRecord.SupervisorId,
                TechnicalCompetence = 4.8m,
                Punctuality = 5.0m,
                Cooperation = 4.9m,
                Communication = 4.7m,
                QualityOfWork = 4.8m,
                Reliability = 4.9m,
                FinalRating = 4.85m,
                Comments = "Excellent intern. Completed all tasks on time, demonstrated strong technical skills, " +
                           "and worked collaboratively with the team throughout the OJT period. " +
                           "Highly recommended for future employment.",
                CreatedAt = internEndDate,
                UpdatedAt = internEndDate
            };
            context.Evaluations.Add(evaluation);
            await context.SaveChangesAsync();
            Console.WriteLine("   ✅ Evaluation created (Final Rating: 4.85 / 5.0)");
        }
        else
        {
            Console.WriteLine("   ℹ️  Evaluation already exists");
        }

        // ── CERTIFICATE ───────────────────────────────────────────────
        Console.WriteLine("\n🏆 Seeding Certificate...");
        var certExists = await context.Certificates
            .AnyAsync(c => c.InternId == internRecord.InternId);

        if (!certExists)
        {
            var certNumber = $"OJT-{internEndDate:yyyy}-{internRecord.InternId:D5}";

            var certificate = new Certificate
            {
                InternId = internRecord.InternId,
                CertificateNumber = certNumber,
                IssuedDate = internEndDate,
                TotalHoursRendered = totalOjtHours,
                StartDate = internStartDate,
                EndDate = internEndDate,
                IssuedBy = supervisorUser.FullName,
                PdfFileName = $"Certificate_{certNumber}.pdf",
                CreatedAt = internEndDate
            };
            context.Certificates.Add(certificate);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✅ Certificate created — #{certNumber}");
        }
        else
        {
            Console.WriteLine("   ℹ️  Certificate already exists");
        }

        // ── SUMMARY ───────────────────────────────────────────────────
        Console.WriteLine("\n");
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           ✅ SEEDING COMPLETED SUCCESSFULLY!                 ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║                                                              ║");
        Console.WriteLine("║  👔 SUPERVISOR                                               ║");
        Console.WriteLine("║     📧 Email:     supervisor@ojtsystem.com                   ║");
        Console.WriteLine("║     🔐 Password:  SupervisorPassword123!                     ║");
        Console.WriteLine("║     ✉️  Confirmed: Yes                                        ║");
        Console.WriteLine("║                                                              ║");
        Console.WriteLine("║  🎓 INTERN (fully completed OJT — certificate ready)         ║");
        Console.WriteLine("║     📧 Email:     intern@ojtsystem.com                       ║");
        Console.WriteLine("║     🔐 Password:  InternPassword123!                         ║");
        Console.WriteLine("║     ✉️  Confirmed: Yes                                        ║");
        Console.WriteLine("║     👔 Supervisor: System Supervisor (assigned)               ║");
        Console.WriteLine("║     ⏱️  Hours:      486 hrs completed                         ║");
        Console.WriteLine("║     ⭐ Rating:     4.85 / 5.0                                 ║");
        Console.WriteLine("║     🏆 Certificate: Generated & ready to view                 ║");
        Console.WriteLine("║                                                              ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
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