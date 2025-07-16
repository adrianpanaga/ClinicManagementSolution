// DTOs/PatientDocument/PatientDocumentDto.cs
using ClinicManagement.Api.DTOs.Patients;
using ClinicManagement.Api.DTOs.StaffDetails; // For StaffDetailDto
using System;

namespace ClinicManagement.Api.DTOs.PatientDocuments // Adjust namespace if different
{
    public class PatientDocumentDto
    {
        public int DocumentId { get; set; }
        public int PatientId { get; set; }

        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public string FilePathOrUrl { get; set; }
        public string Notes { get; set; }
        public int? UploadedByStaffId { get; set; }
        public DateTime UploadDate { get; set; } // When it was uploaded

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Nested DTOs for related data
        public PatientDetailsDto Patient { get; set; }
        public StaffDetailDto UploadedByStaff { get; set; }
    }
}