using OJTManagementSystem.Dtos;
using OJTManagementSystem.ViewModel;

namespace OJTManagementSystem.Services.Interfaces
{
    public interface IEvaluationService
    {
        Task<EvaluationViewModel> SubmitEvaluationAsync(int supervisorId, SubmitEvaluationDto dto);
        Task<EvaluationViewModel> GetEvaluationByIdAsync(int evaluationId);
        Task<List<EvaluationViewModel>> GetInternEvaluationsAsync(int internId);
        Task<List<EvaluationViewModel>> GetSupervisorEvaluationsAsync(int supervisorId);
        Task<EvaluationViewModel> GetLatestEvaluationAsync(int internId);
        Task DeleteEvaluationAsync(int evaluationId);
    }
}