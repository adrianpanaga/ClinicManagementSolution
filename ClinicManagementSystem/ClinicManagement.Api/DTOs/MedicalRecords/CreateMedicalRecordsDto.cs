// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\MedicalRecords\CreateMedicalRecordsDto.cs

using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.MedicalRecords
{
    // DTO for creating a new Medical Record (Write operations - POST)
    public class CreateMedicalRecordsDto
    {
        [Required(ErrorMessage = "Patient ID is required.")]
        public int PatientId { get; set; }
        public int? AppointmentId { get; set; }
        [Required(ErrorMessage = "Staff ID (Doctor) is required.")]
        public int StaffId { get; set; }
        public int? ServiceId { get; set; }
        [MaxLength(500, ErrorMessage = "Diagnosis cannot exceed 500 characters.")]
        public string? Diagnosis { get; set; }
        [MaxLength(1000, ErrorMessage = "Treatment cannot exceed 1000 characters.")]
        public string? Treatment { get; set; }
        [MaxLength(1000, ErrorMessage = "Prescription cannot exceed 1000 characters.")]
        public string? Prescription { get; set; }
    }
}
