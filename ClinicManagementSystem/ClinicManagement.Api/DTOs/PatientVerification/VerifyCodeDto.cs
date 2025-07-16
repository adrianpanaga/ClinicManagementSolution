using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.PatientVerification
{
    public class VerifyCodeDto
    {
        [Required(ErrorMessage = "Contact identifier is required.")]
        [StringLength(100, ErrorMessage = "Contact identifier cannot exceed 100 characters.")]
        public required string ContactIdentifier { get; set; } // <--- Added 'required'

        [Required(ErrorMessage = "Verification code is required.")]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "Verification code must be between 4 and 10 characters.")]
        public required string Code { get; set; } // <--- Added 'required'
    }
}
