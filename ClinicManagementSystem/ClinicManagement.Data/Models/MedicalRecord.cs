// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.Data\Models\MedicalRecord.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement.Data.Models
{
    public partial class MedicalRecord
    {
        [Key]
        public int RecordId { get; set; }

        public int PatientId { get; set; }
        public int? AppointmentId { get; set; } // Make nullable here too for consistency if DB allows NULL
        public int StaffId { get; set; }
        public int? ServiceId { get; set; } // Make nullable here too for consistency if DB allows NULL

        [StringLength(500)]
        public string? Diagnosis { get; set; }

        [StringLength(1000)]
        public string? Treatment { get; set; }

        [StringLength(1000)]
        public string? Prescription { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // --- NEW PROPERTY FOR SOFT DELETE ---
        public bool IsDeleted { get; set; } = false; // Default to false (not deleted)

        // Navigation Properties
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment? Appointment { get; set; }

        [ForeignKey("StaffId")]
        public virtual StaffDetail? Staff { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }

        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    }
}
