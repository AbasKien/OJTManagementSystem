using Microsoft.EntityFrameworkCore;
using OJTManagementSystem.Data;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;

namespace OJTManagementSystem.Repository
{
    /// <summary>
    /// Repository implementation for Supervisor data access operations
    /// </summary>
    public class SupervisorRepository : ISupervisorRepository
    {
        private readonly ApplicationDbContext _context;

        public SupervisorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Supervisor> GetByIdAsync(int supervisorId)
        {
            return await _context.Supervisors
                .Include(s => s.User)
                .Include(s => s.Interns)
                .FirstOrDefaultAsync(s => s.SupervisorId == supervisorId);
        }

        public async Task<Supervisor> GetByUserIdAsync(string userId)
        {
            return await _context.Supervisors
                .Include(s => s.User)
                .Include(s => s.Interns)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<IEnumerable<Supervisor>> GetAllActiveSupervisorsAsync()
        {
            return await _context.Supervisors
                .Where(s => s.IsActive)
                .Include(s => s.User)
                .OrderBy(s => s.User.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Supervisor>> GetAllAsync()
        {
            return await _context.Supervisors
                .Include(s => s.User)
                .Include(s => s.Interns)
                .OrderBy(s => s.User.FullName)
                .ToListAsync();
        }

        public async Task<Supervisor> AddAsync(Supervisor supervisor)
        {
            _context.Supervisors.Add(supervisor);
            await _context.SaveChangesAsync();
            return supervisor;
        }

        public async Task<Supervisor> UpdateAsync(Supervisor supervisor)
        {
            _context.Supervisors.Update(supervisor);
            await _context.SaveChangesAsync();
            return supervisor;
        }

        public async Task<bool> DeleteAsync(int supervisorId)
        {
            var supervisor = await GetByIdAsync(supervisorId);
            if (supervisor == null) return false;

            _context.Supervisors.Remove(supervisor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int supervisorId)
        {
            return await _context.Supervisors
                .AnyAsync(s => s.SupervisorId == supervisorId);
        }
    }
}