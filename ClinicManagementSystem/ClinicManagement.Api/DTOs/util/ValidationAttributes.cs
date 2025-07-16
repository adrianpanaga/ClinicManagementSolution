using ClinicManagement.Api.DTOs.Appointments;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.util
{
    // NEW: Custom validation attribute for conditional requiredness
    // This attribute ensures a field is required ONLY if PatientId is null
    public class RequiredIfPatientIdIsNullAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dto = (CreateAppointmentDto)validationContext.ObjectInstance;

            // If PatientId is provided, this field is not required (return success)
            if (dto.PatientId.HasValue)
            {
                return ValidationResult.Success;
            }

            // If PatientId is NULL, then this field IS required (check for null or whitespace)
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
            }

            return ValidationResult.Success;
        }
    }
}
