using Microsoft.EntityFrameworkCore;
using OJTManagementSystem.Data;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;

namespace OJTManagementSystem.Repository
{
    public class CertificateRepository : GenericRepository<Certificate>, ICertificateRepository
    {
        private readonly ApplicationDbContext _context;

        public CertificateRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Certificate> GetByInternIdAsync(int internId)
        {
            return await _context.Certificates
                .Include(c => c.Intern)
                .FirstOrDefaultAsync(c => c.InternId == internId);
        }

        public async Task<Certificate> GetByCertificateNumberAsync(string certificateNumber)
        {
            return await _context.Certificates
                .Include(c => c.Intern)
                .FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber);
        }

        public async Task<List<Certificate>> GetAllWithInternDataAsync()
        {
            return await _context.Certificates
                .Include(c => c.Intern)
                .OrderByDescending(c => c.IssuedDate)
                .ToListAsync();
        }
    }
}