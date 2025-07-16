using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagement.Data.Models
{
    public class LabResult
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LabResultId { get; set; } // Primary Key

        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!; // Initialize navigation property

        public int? MedicalRecordId { get; set; }
        public MedicalRecord? MedicalRecord { get; set; } // Make nullable

        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; } // Make nullable



        [Required]
        [MaxLength(100)]
        public string TestName { get; set; } = null!; // e.g., "Complete Blood Count", "Urinalysis"

        [MaxLength(255)]
        public string ResultValue { get; set; } = null!; // The actual result, e.g., "Positive", "12.5 g/dL", "Normal"

        [MaxLength(50)]
        public string? Unit { get; set; } // e.g., "mg/dL", "g/dL", "cells/mm3" (if applicable)

        [MaxLength(100)]
        public string? ReferenceRange { get; set; } // Normal range, e.g., "4.0-5.5"

        [MaxLength(500)]
        public string? Interpretation { get; set; } // Doctor's notes on the result

        public DateTime ResultDate { get; set; } // Date the result was obtained/reported

        // Who ordered/reviewed the lab (staff member)
        public int? OrderedByStaffId { get; set; }
        public StaffDetail? OrderedByStaff { get; set; } // Navigation property

        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false; // Soft delete
    }
}
