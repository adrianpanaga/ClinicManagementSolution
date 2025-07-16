using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.TriageRecords
{
    public class UpdateTriageRecordDto
    {
        [Required]
        public int TriageRecordId { get; set; } // TriageRecord ID to update

        public int? AppointmentId { get; set; } // Optional, can be updated

        [MaxLength(100)]
        public string? ChiefComplaint { get; set; }

        [Range(35.0, 42.0)]
        public decimal? Temperature { get; set; }

        [Range(50, 250)]
        public int? BloodPressureSystolic { get; set; }

        [Range(30, 150)]
        public int? BloodPressureDiastolic { get; set; }

        [Range(40, 200)]
        public int? PulseRate { get; set; }

        [Range(8, 30)]
        public int? RespiratoryRate { get; set; }

        [Range(1.0, 500.0)]
        public decimal? Weight { get; set; }

        [Range(50.0, 300.0)]
        public decimal? Height { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsDeleted { get; set; } // Allow setting soft delete status
    }
}
