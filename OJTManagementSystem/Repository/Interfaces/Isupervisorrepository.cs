using OJTManagementSystem.Models;

namespace OJTManagementSystem.Repository.Interfaces
{
    /// <summary>
    /// Repository interface for Supervisor data access operations
    /// </summary>
    public interface ISupervisorRepository
    {
        /// <summary>
        /// Get a supervisor by their unique identifier
        /// </summary>
        /// <param name="supervisorId">The supervisor's unique ID</param>
        /// <returns>Supervisor object or null if not found</returns>
        Task<Supervisor> GetByIdAsync(int supervisorId);

        /// <summary>
        /// Get a supervisor by their associated user ID
        /// </summary>
        /// <param name="userId">The ApplicationUser ID</param>
        /// <returns>Supervisor object or null if not found</returns>
        Task<Supervisor> GetByUserIdAsync(string userId);

        /// <summary>
        /// Get all active supervisors in the system
        /// </summary>
        /// <returns>Collection of active supervisors ordered by full name</returns>
        Task<IEnumerable<Supervisor>> GetAllActiveSupervisorsAsync();

        /// <summary>
        /// Get all supervisors (active and inactive)
        /// </summary>
        /// <returns>Collection of all supervisors</returns>
        Task<IEnumerable<Supervisor>> GetAllAsync();

        /// <summary>
        /// Add a new supervisor to the database
        /// </summary>
        /// <param name="supervisor">The supervisor object to add</param>
        /// <returns>The added supervisor with generated ID</returns>
        Task<Supervisor> AddAsync(Supervisor supervisor);

        /// <summary>
        /// Update an existing supervisor
        /// </summary>
        /// <param name="supervisor">The supervisor object with updated values</param>
        /// <returns>The updated supervisor</returns>
        Task<Supervisor> UpdateAsync(Supervisor supervisor);

        /// <summary>
        /// Delete a supervisor by ID
        /// </summary>
        /// <param name="supervisorId">The supervisor's unique ID</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        Task<bool> DeleteAsync(int supervisorId);

        /// <summary>
        /// Check if a supervisor exists by ID
        /// </summary>
        /// <param name="supervisorId">The supervisor's unique ID</param>
        /// <returns>True if supervisor exists, false otherwise</returns>
        Task<bool> ExistsAsync(int supervisorId);
    }
}