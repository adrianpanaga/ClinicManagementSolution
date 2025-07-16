using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.PatientVerification // Adjust namespace to your project's DTOs
{
    public class RequestVerificationCodeDto
    {
        [Required(ErrorMessage = "Contact identifier is required.")]
        [StringLength(100, ErrorMessage = "Contact identifier cannot exceed 100 characters.")]
        public required string ContactIdentifier { get; set; } // <--- Added 'required'

        [Required(ErrorMessage = "Method (email or sms) is required.")]
        [StringLength(10, ErrorMessage = "Method cannot exceed 10 characters.")]
        [AllowedValues(new string[] { "email", "sms" }, ErrorMessage = "Method must be 'email' or 'sms'.")]
        public required string Method { get; set; } // <--- Added 'required'

        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        public string? LastName { get; set; } // Can be nullable
    }

    // Custom Validation Attribute for AllowedValues
    // You might put this in a 'ValidationAttributes' folder or similar
    public class AllowedValuesAttribute : ValidationAttribute
    {
        private readonly string[] _allowedValues;

        public AllowedValuesAttribute(string[] allowedValues)
        {
            _allowedValues = allowedValues;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // [Required] will handle null check
            }

            var stringValue = value.ToString();
            if (!_allowedValues.Contains(stringValue, StringComparer.OrdinalIgnoreCase))
            {
                return new ValidationResult(ErrorMessage ?? $"The field {validationContext.DisplayName} must be one of the following values: {string.Join(", ", _allowedValues)}.");
            }

            return ValidationResult.Success;
        }
    }
}