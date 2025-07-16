// Models/PatientDocument.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement.Data.Models // Adjust namespace if different
{
    public class PatientDocument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DocumentId { get; set; } // Primary Key

        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!; // Navigation property

        [Required]
        [MaxLength(255)]
        public required string DocumentName { get; set; } // Display name of the document, e.g., "Consent Form - 2024"

        [MaxLength(50)]
        public string? DocumentType { get; set; } // e.g., "Consent Form", "Referral Letter", "Insurance Policy"

        [Required]
        [MaxLength(500)] // Increased length to accommodate URLs or file paths
        public required string FilePathOrUrl { get; set; } // Path/URL to the actual stored file

        [MaxLength(500)]
        public string? Notes { get; set; } // Any additional notes about the document

        // Who uploaded/added this document (staff member)
        public int? UploadedByStaffId { get; set; }
        public StaffDetail UploadedByStaff { get; set; } = null!; // Navigation property

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false; // Soft delete
    }
}