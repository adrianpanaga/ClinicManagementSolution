// DTOs/LabResult/UpdateLabResultDto.cs
using System;
using System.ComponentModel.DataAnnotations;

// DTOs/LabResult/UpdateLabResultDto.cs
// ...
namespace ClinicManagement.Api.DTOs.LabResult
{
    public class UpdateLabResultDto
    {
        [Required]
        public int LabResultId { get; set; }

        public int? MedicalRecordId { get; set; }
        public int? AppointmentId { get; set; }

        [MaxLength(100)]
        public string? TestName { get; set; } // Make nullable as it's an update DTO
        [MaxLength(255)]
        public string? ResultValue { get; set; } // Make nullable
        [MaxLength(50)]
        public string? Unit { get; set; } // Make nullable
        [MaxLength(100)]
        public string? ReferenceRange { get; set; } // Make nullable
        [MaxLength(500)]
        public string? Interpretation { get; set; } // Make nullable

        public DateOnly? ResultDate { get; set; }

        public int? OrderedByStaffId { get; set; }

        public bool IsDeleted { get; set; }
    }
}