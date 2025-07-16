using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.Patients
{
    public class PatientAppointmentsRequestDto
    {
        [Required]
        public string ContactNumber { get; set; } // Assuming contact is the primary identifier
        [Required]
        public string Email { get; set; } // Assuming email is the secondary identifier

    }
}
