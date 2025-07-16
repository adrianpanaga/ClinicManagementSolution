// DTOs/PatientDocument/UpdatePatientDocumentDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.PatientDocuments // Adjust namespace if different
{
    public class UpdatePatientDocumentDto
    {
        [Required]
        public int DocumentId { get; set; } // ID of the document to update

        [MaxLength(255)]
        public string? DocumentName { get; set; }

        [MaxLength(50)]
        public string? DocumentType { get; set; }

        [MaxLength(500)]
        public string? FilePathOrUrl { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int? UploadedByStaffId { get; set; }

        public bool IsDeleted { get; set; }
    }
}