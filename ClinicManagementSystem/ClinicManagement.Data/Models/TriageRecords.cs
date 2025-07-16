using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagement.Data.Models
{
    public class TriageRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TriageRecordId { get; set; } // Primary Key for TriageRecord, using TriageRecordId for clarity

        // Foreign Key to Patient
        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!; // Navigation property

        // Optional: Link to an Appointment if Triage is done during an appointment
        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; } // Navigation property

        // Triage Data Fields
        [MaxLength(100)]
        public string ChiefComplaint { get; set; } = null!; // Main reason for visit

        public decimal? Temperature { get; set; } // Storing in Celsius, specify unit in UI (e.g., 37.0)
        public int? BloodPressureSystolic { get; set; } // Systolic BP (e.g., 120)
        public int? BloodPressureDiastolic { get; set; } // Diastolic BP (e.g., 80)
        public int? PulseRate { get; set; } // beats per minute (e.g., 72)
        public int? RespiratoryRate { get; set; } // breaths per minute (e.g., 16)
        public decimal? Weight { get; set; } // Storing in kilograms, specify unit in UI (e.g., 70.5)
        public decimal? Height { get; set; } // Storing in centimeters, specify unit in UI (e.g., 175.2)

        [MaxLength(500)]
        public string Notes { get; set; } = null!; // Additional notes from triage

        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false; // Soft delete
    }
}
