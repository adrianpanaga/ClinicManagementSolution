// DTOs/PatientDocument/CreatePatientDocumentDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.PatientDocuments // Adjust namespace if different
{
    public class CreatePatientDocumentDto
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        [MaxLength(255)]
        public required string DocumentName { get; set; }

        [MaxLength(50)]
        public string? DocumentType { get; set; } // e.g., "Consent Form", "Referral Letter"

        [Required]
        [MaxLength(500)]
        public required string FilePathOrUrl { get; set; } // URL or path to the actual file

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int? UploadedByStaffId { get; set; } // Optional: can be set in DTO or derived from JWT
    }
}