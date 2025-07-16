using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.DTOs.Patients;

namespace ClinicManagement.Api.DTOs.TriageRecords
{
    public class TriageRecordDto
    {
        public int TriageRecordId { get; set; }
        public int PatientId { get; set; }
        public int? AppointmentId { get; set; }

        public string? ChiefComplaint { get; set; }
        public decimal? Temperature { get; set; }
        public int? BloodPressureSystolic { get; set; }
        public int? BloodPressureDiastolic { get; set; }
        public int? PulseRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Nested DTOs for related data
        public PatientDetailsDto? Patient { get; set; } // Full patient details (optional, can be simplified)
        public AppointmentDto? Appointment { get; set; } // Full appointment details (optional)
    }
}
