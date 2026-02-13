using OJTManagementSystem.Models;
using OJTManagementSystem.Repository.Interfaces;
using OJTManagementSystem.ViewModel;
using OJTManagementSystem.Helpers;
using OJTManagementSystem.Services.Interfaces;

namespace OJTManagementSystem.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IInternRepository _internRepository;
        private readonly IDailyTimeRecordRepository _dtrRepository;
        private readonly PdfGeneratorHelper _pdfGenerator;

        public CertificateService(
            ICertificateRepository certificateRepository,
            IInternRepository internRepository,
            IDailyTimeRecordRepository dtrRepository,
            PdfGeneratorHelper pdfGenerator)
        {
            _certificateRepository = certificateRepository;
            _internRepository = internRepository;
            _dtrRepository = dtrRepository;
            _pdfGenerator = pdfGenerator;
        }

        // ✅ FIXED: Added issuedBy parameter
        public async Task<CertificateViewModel> GenerateCertificateAsync(int internId, string issuedBy)
        {
            // ✅ Use GetWithAllDataAsync so intern.User is loaded (prevents NullReferenceException)
            var intern = await _internRepository.GetWithAllDataAsync(internId);
            if (intern == null)
                throw new Exception("Intern not found");

            if (intern.User == null)
                throw new Exception("Intern user profile not found. Cannot generate certificate.");

            var existingCertificate = await _certificateRepository.GetByInternIdAsync(internId);
            if (existingCertificate != null)
                throw new Exception("Certificate already exists for this intern");

            var totalHours = await _dtrRepository.GetTotalHoursByInternAsync(internId);

            var certificateNumber = GenerateCertificateNumber();
            var pdfContent = await _pdfGenerator.GenerateCertificatePdfAsync(intern, totalHours);

            var certificate = new Certificate
            {
                InternId = internId,
                CertificateNumber = certificateNumber,
                IssuedDate = DateTime.UtcNow,
                TotalHoursRendered = totalHours,
                StartDate = intern.StartDate,
                EndDate = intern.EndDate,
                PdfContent = pdfContent,
                PdfFileName = $"Certificate_{intern.StudentId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf",
                // ✅ FIXED: IssuedBy is now set — no more NULL error
                IssuedBy = issuedBy ?? "OJT Management System"
            };

            await _certificateRepository.AddAsync(certificate);
            return await GetCertificateByIdAsync(certificate.CertificateId);
        }

        public async Task<CertificateViewModel> GetCertificateByIdAsync(int certificateId)
        {
            var certificate = await _certificateRepository.GetByIdAsync(certificateId);
            if (certificate == null)
                throw new Exception("Certificate not found");

            return MapToViewModel(certificate);
        }

        public async Task<CertificateViewModel> GetCertificateByInternIdAsync(int internId)
        {
            var certificate = await _certificateRepository.GetByInternIdAsync(internId);
            if (certificate == null)
                return null;

            return MapToViewModel(certificate);
        }

        public async Task<List<CertificateViewModel>> GetAllCertificatesAsync()
        {
            var certificates = await _certificateRepository.GetAllWithInternDataAsync();
            return certificates.Select(MapToViewModel).ToList();
        }

        public async Task<byte[]> GetCertificatePdfAsync(int certificateId)
        {
            // ✅ Try by certificateId first
            var certificate = await _certificateRepository.GetByIdAsync(certificateId);

            // ✅ FALLBACK: If not found by certificateId, try treating it as internId
            if (certificate == null)
                certificate = await _certificateRepository.GetByInternIdAsync(certificateId);

            if (certificate == null)
                throw new Exception("Certificate not found");

            if (certificate.PdfContent == null || certificate.PdfContent.Length == 0)
                throw new Exception("PDF content is missing for this certificate.");

            return certificate.PdfContent;
        }

        public async Task DeleteCertificateAsync(int certificateId)
        {
            var certificate = await _certificateRepository.GetByIdAsync(certificateId);
            if (certificate == null)
                throw new Exception("Certificate not found");

            await _certificateRepository.DeleteAsync(certificate);
        }

        private string GenerateCertificateNumber()
        {
            return $"CERT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private CertificateViewModel MapToViewModel(Certificate certificate)
        {
            return new CertificateViewModel
            {
                CertificateId = certificate.CertificateId,
                InternId = certificate.InternId,
                InternName = certificate.Intern?.User?.FullName,
                School = certificate.Intern?.School,
                Course = certificate.Intern?.Course,
                CertificateNumber = certificate.CertificateNumber,
                IssuedDate = certificate.IssuedDate,
                TotalHoursRendered = certificate.TotalHoursRendered,
                StartDate = certificate.StartDate,
                EndDate = certificate.EndDate,
                IssuedBy = certificate.IssuedBy,
                PdfFileName = certificate.PdfFileName,
                CreatedAt = certificate.CreatedAt
            };
        }
    }
}