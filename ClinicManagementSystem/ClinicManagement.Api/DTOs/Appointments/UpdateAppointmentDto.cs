// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\Appointments\UpdateAppointmentDto.cs

using ClinicManagement.Data.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.Appointments
{
    // DTO for updating an existing Appointment (Write operations - PUT)
    public class UpdateAppointmentDto
    {
        [Required(ErrorMessage = "Appointment ID is required for update.")]
        public int AppointmentId { get; set; }

        public int? PatientId { get; set; } // Nullable, as updates might change patient or clear it

        public int? DoctorId { get; set; } // Nullable for partial updates

        public int? ServiceId { get; set; } // Nullable for partial updates

        public DateTime? AppointmentDateTime { get; set; } // Nullable for partial updates

        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public AppointmentStatus Status { get; set; } // Nullable for partial updates

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }
    }
}
