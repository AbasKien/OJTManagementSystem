using OJTManagementSystem.ViewModel;

namespace OJTManagementSystem.Services.Interfaces
{
    public interface ICertificateService
    {
        // ✅ FIXED: Added issuedBy parameter so supervisor name is saved
        Task<CertificateViewModel> GenerateCertificateAsync(int internId, string issuedBy);
        Task<CertificateViewModel> GetCertificateByIdAsync(int certificateId);
        Task<CertificateViewModel> GetCertificateByInternIdAsync(int internId);
        Task<List<CertificateViewModel>> GetAllCertificatesAsync();
        Task<byte[]> GetCertificatePdfAsync(int certificateId);
        Task DeleteCertificateAsync(int certificateId);
    }
}