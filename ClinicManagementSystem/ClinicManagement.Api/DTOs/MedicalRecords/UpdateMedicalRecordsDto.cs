// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\MedicalRecords\UpdateMedicalRecordsDto.cs

using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.MedicalRecords
{
    // DTO for updating an existing Medical Record (Write operations - PUT)
    public class UpdateMedicalRecordsDto
    {
        [Required(ErrorMessage = "Record ID is required for update.")]
        public int RecordId { get; set; }
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
