using OJTManagementSystem.Models;

namespace OJTManagementSystem.Repository.Interfaces
{
    public interface IEvaluationRepository : IGenericRepository<Evaluation>
    {
        Task<List<Evaluation>> GetByInternIdAsync(int internId);
        Task<List<Evaluation>> GetBySupervisorIdAsync(int supervisorId);
        Task<Evaluation> GetLatestByInternAsync(int internId);
        Task<Evaluation> GetByInternAndSupervisorAsync(int internId, int supervisorId);
    }
}