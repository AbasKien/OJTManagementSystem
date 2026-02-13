using Microsoft.EntityFrameworkCore;
using OJTManagementSystem.Data;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;

namespace OJTManagementSystem.Repository
{
    public class EvaluationRepository : GenericRepository<Evaluation>, IEvaluationRepository
    {
        private readonly ApplicationDbContext _context;

        public EvaluationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Evaluation>> GetByInternIdAsync(int internId)
        {
            return await _context.Evaluations
                .Where(e => e.InternId == internId)
                .Include(e => e.Intern)
                .Include(e => e.Supervisor)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Evaluation>> GetBySupervisorIdAsync(int supervisorId)
        {
            return await _context.Evaluations
                .Where(e => e.SupervisorId == supervisorId)
                .Include(e => e.Intern)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<Evaluation> GetLatestByInternAsync(int internId)
        {
            return await _context.Evaluations
                .Where(e => e.InternId == internId)
                .Include(e => e.Supervisor)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<Evaluation> GetByInternAndSupervisorAsync(int internId, int supervisorId)
        {
            return await _context.Evaluations
                .FirstOrDefaultAsync(e => e.InternId == internId && e.SupervisorId == supervisorId);
        }
    }
}