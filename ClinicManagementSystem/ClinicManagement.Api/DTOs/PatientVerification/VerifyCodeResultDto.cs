namespace ClinicManagement.Api.DTOs.PatientVerification
{
    public class VerifyCodeResultDto
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }

        // NEW: Patient details of the verified user
        public int? PatientId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? ContactNumber { get; set; }

    }
}
