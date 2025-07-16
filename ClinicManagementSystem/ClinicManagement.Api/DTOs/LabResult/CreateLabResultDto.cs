using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.LabResult
{
    public class CreateLabResultDto
    {
        [Required]
        public int PatientId { get; set; }

        public int? MedicalRecordId { get; set; }
        public int? AppointmentId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string TestName { get; set; } // Use 'required' (C# 11+) or set default

        [Required]
        [MaxLength(255)]
        public required string ResultValue { get; set; } // Use 'required'

        [MaxLength(50)]
        public string? Unit { get; set; } // Make nullable
        [MaxLength(100)]
        public string? ReferenceRange { get; set; } // Make nullable
        [MaxLength(500)]
        public string? Interpretation { get; set; } // Make nullable

        [Required]
        public DateOnly ResultDate { get; set; }

        public int? OrderedByStaffId { get; set; }
    }
}
