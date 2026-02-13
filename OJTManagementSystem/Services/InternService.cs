using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OJTManagementSystem.Data;
using OJTManagementSystem.Dtos;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;
using OJTManagementSystem.ViewModel;
using OJTManagementSystem.Services.Interfaces;

namespace OJTManagementSystem.Services
{
    public class InternService : IInternService
    {
        private readonly IInternRepository _internRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public InternService(
            IInternRepository internRepository,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _internRepository = internRepository;
            _userManager = userManager;
            _context = context;
        }

        public async Task<InternViewModel> CreateInternAsync(RegisterInternDto dto)
        {
            var user = new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, "TempPassword123!");
            if (!result.Succeeded)
                throw new Exception("Failed to create user account");

            await _userManager.AddToRoleAsync(user, "Intern");

            var intern = new Intern
            {
                UserId = user.Id,
                StudentId = dto.StudentId,
                School = dto.School,
                Course = dto.Course,
                Department = dto.Department,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                SupervisorId = dto.SupervisorId   // nullable — may be null at registration
            };

            await _internRepository.AddAsync(intern);

            return await GetInternByIdAsync(intern.InternId);
        }

        public async Task<InternViewModel> GetInternByIdAsync(int internId)
        {
            var intern = await _internRepository.GetByIdAsync(internId);
            if (intern == null)
                throw new Exception("Intern not found");

            return MapToViewModel(intern);
        }

        public async Task<InternViewModel> GetInternByUserIdAsync(string userId)
        {
            var intern = await _internRepository.GetByUserIdAsync(userId);
            if (intern == null)
                throw new Exception("Intern not found");

            return MapToViewModel(intern);
        }

        public async Task<List<InternViewModel>> GetAllInternsAsync()
        {
            var interns = await _internRepository.GetAllAsync();
            return interns.Select(MapToViewModel).ToList();
        }

        public async Task<List<InternViewModel>> GetInternsBySupervisorAsync(int supervisorId)
        {
            var interns = await _internRepository.GetBySupervisorIdAsync(supervisorId);
            return interns.Select(MapToViewModel).ToList();
        }

        public async Task DeleteInternAsync(int internId)
        {
            var intern = await _internRepository.GetByIdAsync(internId);
            if (intern == null)
                throw new Exception("Intern not found");

            await _internRepository.DeleteAsync(intern);
        }

        public async Task<bool> InternExistsAsync(int internId)
        {
            var intern = await _internRepository.GetByIdAsync(internId);
            return intern != null;
        }

        // Get supervisor by user ID
        public async Task<Supervisor> GetSupervisorByUserIdAsync(string userId)
        {
            return await _context.Supervisors
                .Include(s => s.User)
                .Include(s => s.Interns)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        // Get interns supervised by a specific supervisor
        public async Task<IEnumerable<InternViewModel>> GetSupervisorInternsAsync(int supervisorId)
        {
            try
            {
                // ✅ CRITICAL FIX: Include all related data to prevent null reference errors
                var interns = await _internRepository.GetBySupervisorIdAsync(supervisorId);

                if (interns == null || interns.Count == 0)
                {
                    return new List<InternViewModel>();
                }

                // ✅ FIXED: Map each intern with null checks
                var internViewModels = new List<InternViewModel>();

                foreach (var intern in interns)
                {
                    if (intern != null && intern.User != null)  // ✅ Null check
                    {
                        var viewModel = new InternViewModel
                        {
                            InternId = intern.InternId,
                            UserId = intern.UserId,
                            FirstName = intern.User.FirstName ?? "Unknown",  // ✅ Null coalescing
                            LastName = intern.User.LastName ?? "Unknown",    // ✅ Null coalescing
                            Email = intern.User.Email ?? "",                 // ✅ Null coalescing
                            PhoneNumber = intern.User.PhoneNumber ?? "",     // ✅ Null coalescing
                            StudentId = intern.StudentId ?? "N/A",           // ✅ Null coalescing
                            School = intern.School ?? "N/A",                 // ✅ Null coalescing
                            Course = intern.Course ?? "N/A",                 // ✅ Null coalescing
                            Department = intern.Department ?? "N/A",         // ✅ Null coalescing
                            StartDate = intern.StartDate,
                            EndDate = intern.EndDate,
                            SupervisorId = intern.SupervisorId ?? 0,         // ✅ Null coalescing for nullable int
                            SupervisorName = intern.Supervisor?.User?.FullName ?? "Unassigned",  // ✅ Safe navigation
                            IsActive = intern.IsActive,
                            CreatedAt = intern.CreatedAt,
                            UpdatedAt = intern.UpdatedAt
                        };

                        internViewModels.Add(viewModel);
                    }
                }

                return internViewModels;  // ✅ Returns IEnumerable<InternViewModel>
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting supervisor interns: {ex.Message}", ex);
            }
        }

        // Assign intern to supervisor
        public async Task AssignInternToSupervisorAsync(int internId, int supervisorId)
        {
            var intern = await _internRepository.GetByIdAsync(internId);
            if (intern == null)
                throw new Exception("Intern not found");

            var supervisor = await _context.Supervisors.FindAsync(supervisorId);
            if (supervisor == null)
                throw new Exception("Supervisor not found");

            intern.SupervisorId = supervisorId;
            intern.UpdatedAt = DateTime.UtcNow;

            await _internRepository.UpdateAsync(intern);
        }

        // ✅ NEW: Get all interns with no supervisor assigned (available for assignment)
        public async Task<List<InternViewModel>> GetAvailableInternsAsync()
        {
            var interns = await _context.Interns
                .Include(i => i.User)
                .Include(i => i.Supervisor)
                .Where(i => i.SupervisorId == null && i.IsActive)
                .OrderBy(i => i.User.FirstName)
                .ThenBy(i => i.User.LastName)
                .ToListAsync();

            return interns.Select(MapToViewModel).ToList();
        }

        // ✅ NEW: Find supervisor by userId, or auto-create one if not found
        public async Task<Supervisor> FindOrCreateSupervisorAsync(string userId)
        {
            // Try to find existing supervisor record
            var existing = await _context.Supervisors
                .Include(s => s.User)
                .Include(s => s.Interns)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (existing != null)
                return existing;

            // No supervisor record found — auto-create one from the ApplicationUser data
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User account not found.");

            var supervisor = new Supervisor
            {
                UserId = userId,
                Position = "Supervisor",       // default — can be updated in profile settings
                Department = "General",        // default — can be updated in profile settings
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Supervisors.Add(supervisor);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            return await _context.Supervisors
                .Include(s => s.User)
                .Include(s => s.Interns)
                .FirstAsync(s => s.SupervisorId == supervisor.SupervisorId);
        }

        private InternViewModel MapToViewModel(Intern intern)
        {
            return new InternViewModel
            {
                InternId = intern.InternId,
                UserId = intern.UserId,
                FirstName = intern.User?.FirstName,
                LastName = intern.User?.LastName,
                Email = intern.User?.Email,
                PhoneNumber = intern.User?.PhoneNumber,
                StudentId = intern.StudentId,
                School = intern.School,
                Course = intern.Course,
                Department = intern.Department,
                StartDate = intern.StartDate,
                EndDate = intern.EndDate,
                SupervisorId = intern.SupervisorId,
                SupervisorName = intern.Supervisor?.User?.FullName,
                IsActive = intern.IsActive,
                CreatedAt = intern.CreatedAt,
                UpdatedAt = intern.UpdatedAt
            };
        }
    }
}