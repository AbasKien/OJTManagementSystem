using OJTManagementSystem.Models;

namespace OJTManagementSystem.Repository.Interfaces
{
    public interface ICertificateRepository : IGenericRepository<Certificate>
    {
        Task<Certificate> GetByInternIdAsync(int internId);
        Task<Certificate> GetByCertificateNumberAsync(string certificateNumber);
        Task<List<Certificate>> GetAllWithInternDataAsync();
    }
}