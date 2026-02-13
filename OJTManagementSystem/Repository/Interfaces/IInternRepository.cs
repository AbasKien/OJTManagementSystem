using OJTManagementSystem.Models;

namespace OJTManagementSystem.Repository.Interfaces
{
    public interface IInternRepository : IGenericRepository<Intern>
    {
        Task<Intern> GetByUserIdAsync(string userId);
        Task<List<Intern>> GetBySupervisorIdAsync(int supervisorId);
        Task<Intern> GetWithAllDataAsync(int internId);
    }
}