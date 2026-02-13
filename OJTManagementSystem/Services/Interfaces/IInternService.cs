using OJTManagementSystem.Dtos;
using OJTManagementSystem.Models;
using OJTManagementSystem.ViewModel;

namespace OJTManagementSystem.Services.Interfaces
{
    public interface IInternService
    {
        // EXISTING METHODS
        Task<InternViewModel> CreateInternAsync(RegisterInternDto dto);
        Task<InternViewModel> GetInternByIdAsync(int internId);
        Task<InternViewModel> GetInternByUserIdAsync(string userId);
        Task<List<InternViewModel>> GetAllInternsAsync();
        Task<List<InternViewModel>> GetInternsBySupervisorAsync(int supervisorId);
        Task DeleteInternAsync(int internId);
        Task<bool> InternExistsAsync(int internId);

        // SUPERVISOR MANAGEMENT METHODS
        /// <summary>
        /// Get supervisor by user ID
        /// </summary>
        Task<Supervisor> GetSupervisorByUserIdAsync(string userId);

        /// <summary>
        /// Get all interns supervised by a specific supervisor
        /// </summary>
        Task<IEnumerable<InternViewModel>> GetSupervisorInternsAsync(int supervisorId);

        /// <summary>
        /// Assign an intern to a supervisor
        /// </summary>
        Task AssignInternToSupervisorAsync(int internId, int supervisorId);

        /// <summary>
        /// Get all interns that have no supervisor assigned yet
        /// </summary>
        Task<List<InternViewModel>> GetAvailableInternsAsync();

        /// <summary>
        /// Gets the supervisor record for a user ID.
        /// If none exists, auto-creates one so the supervisor can start assigning interns.
        /// </summary>
        Task<Supervisor> FindOrCreateSupervisorAsync(string userId);
    }
}