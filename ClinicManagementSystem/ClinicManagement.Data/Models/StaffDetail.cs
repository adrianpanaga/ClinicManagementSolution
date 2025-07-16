// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.Data\Models\StaffDetail.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement.Data.Models
{
    public partial class StaffDetail
    {
        public StaffDetail()
        {
            Appointments = new HashSet<Appointment>();
            MedicalRecords = new HashSet<MedicalRecord>();
        }
         
        [Key]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [MaxLength(50)]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Job Title is required.")]
        [MaxLength(50)]
        public string? JobTitle { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }

        [Required(ErrorMessage = "Contact Number is required.")]
        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default to current UTC time

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        public int? UserId { get; set; } // Foreign Key to User (IdentityUser)

        // --- NEW PROPERTY FOR SOFT DELETE ---
        public bool IsDeleted { get; set; } = false; // Default to false (not deleted)

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; } // Mark as nullable

        [InverseProperty("Doctor")]
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; }
    }
}
