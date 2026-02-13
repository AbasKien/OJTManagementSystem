using OJTManagementSystem.Dtos;
using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;
using OJTManagementSystem.ViewModel;
using OJTManagementSystem.Services.Interfaces;

namespace OJTManagementSystem.Services
{
    public class EvaluationService : IEvaluationService
    {
        private readonly IEvaluationRepository _evaluationRepository;
        private readonly IInternRepository _internRepository;

        public EvaluationService(
            IEvaluationRepository evaluationRepository,
            IInternRepository internRepository)
        {
            _evaluationRepository = evaluationRepository;
            _internRepository = internRepository;
        }

        public async Task<EvaluationViewModel> SubmitEvaluationAsync(int supervisorId, SubmitEvaluationDto dto)
        {
            var intern = await _internRepository.GetByIdAsync(dto.InternId);
            if (intern == null)
                throw new Exception("Intern not found");

            var finalRating = CalculateFinalRating(
                dto.TechnicalCompetence,
                dto.Punctuality,
                dto.Cooperation,
                dto.Communication,
                dto.QualityOfWork,
                dto.Reliability
            );

            var evaluation = new Evaluation
            {
                InternId = dto.InternId,
                SupervisorId = supervisorId,
                TechnicalCompetence = dto.TechnicalCompetence,
                Punctuality = dto.Punctuality,
                Cooperation = dto.Cooperation,
                Communication = dto.Communication,
                QualityOfWork = dto.QualityOfWork,
                Reliability = dto.Reliability,
                FinalRating = finalRating,
                Comments = dto.Comments
            };

            await _evaluationRepository.AddAsync(evaluation);
            return await GetEvaluationByIdAsync(evaluation.EvaluationId);
        }

        public async Task<EvaluationViewModel> GetEvaluationByIdAsync(int evaluationId)
        {
            var evaluation = await _evaluationRepository.GetByIdAsync(evaluationId);
            if (evaluation == null)
                throw new Exception("Evaluation not found");

            return MapToViewModel(evaluation);
        }

        public async Task<List<EvaluationViewModel>> GetInternEvaluationsAsync(int internId)
        {
            var evaluations = await _evaluationRepository.GetByInternIdAsync(internId);
            return evaluations.Select(MapToViewModel).ToList();
        }

        public async Task<List<EvaluationViewModel>> GetSupervisorEvaluationsAsync(int supervisorId)
        {
            var evaluations = await _evaluationRepository.GetBySupervisorIdAsync(supervisorId);
            return evaluations.Select(MapToViewModel).ToList();
        }

        public async Task<EvaluationViewModel> GetLatestEvaluationAsync(int internId)
        {
            var evaluation = await _evaluationRepository.GetLatestByInternAsync(internId);
            if (evaluation == null)
                return null;

            return MapToViewModel(evaluation);
        }

        public async Task DeleteEvaluationAsync(int evaluationId)
        {
            var evaluation = await _evaluationRepository.GetByIdAsync(evaluationId);
            if (evaluation == null)
                throw new Exception("Evaluation not found");

            await _evaluationRepository.DeleteAsync(evaluation);
        }

        private decimal CalculateFinalRating(
            decimal technical, decimal punctuality, decimal cooperation,
            decimal communication, decimal quality, decimal reliability)
        {
            return (technical + punctuality + cooperation + communication + quality + reliability) / 6m;
        }

        private EvaluationViewModel MapToViewModel(Evaluation evaluation)
        {
            return new EvaluationViewModel
            {
                EvaluationId = evaluation.EvaluationId,
                InternId = evaluation.InternId,
                InternName = evaluation.Intern?.User?.FullName,
                SupervisorId = evaluation.SupervisorId,
                SupervisorName = evaluation.Supervisor?.User?.FullName,
                TechnicalCompetence = evaluation.TechnicalCompetence,
                Punctuality = evaluation.Punctuality,
                Cooperation = evaluation.Cooperation,
                Communication = evaluation.Communication,
                QualityOfWork = evaluation.QualityOfWork,
                Reliability = evaluation.Reliability,
                FinalRating = evaluation.FinalRating,
                Comments = evaluation.Comments,
                CreatedAt = evaluation.CreatedAt,
                UpdatedAt = evaluation.UpdatedAt
            };
        }
    }
}