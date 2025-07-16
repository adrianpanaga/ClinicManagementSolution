using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.TriageRecords
{
    public class CreateTriageRecordDto
    {
        [Required]
        public int PatientId { get; set; }

        public int? AppointmentId { get; set; } // Optional link to an appointment

        [MaxLength(100)]
        public required string ChiefComplaint { get; set; }

        [Range(35.0, 42.0, ErrorMessage = "Temperature must be between 35.0 and 42.0 Celsius.")]
        public decimal? Temperature { get; set; }

        [Range(50, 250, ErrorMessage = "Systolic Blood Pressure must be between 50 and 250.")]
        public int? BloodPressureSystolic { get; set; }

        [Range(30, 150, ErrorMessage = "Diastolic Blood Pressure must be between 30 and 150.")]
        public int? BloodPressureDiastolic { get; set; }

        [Range(40, 200, ErrorMessage = "Pulse Rate must be between 40 and 200 beats per minute.")]
        public int? PulseRate { get; set; }

        [Range(8, 30, ErrorMessage = "Respiratory Rate must be between 8 and 30 breaths per minute.")]
        public int? RespiratoryRate { get; set; }

        [Range(1.0, 500.0, ErrorMessage = "Weight must be between 1.0 and 500.0 kg.")]
        public decimal? Weight { get; set; }

        [Range(50.0, 300.0, ErrorMessage = "Height must be between 50.0 and 300.0 cm.")]
        public decimal? Height { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
