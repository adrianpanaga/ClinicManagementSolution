// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\Appointments\CreateAppointmentDto.cs

using ClinicManagement.Api.DTOs.util;
using ClinicManagement.Data.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.Appointments
{
    // DTO for creating a new Appointment (Write operations - POST)
    public class CreateAppointmentDto
    {
        public int? PatientId { get; set; } // Can be nullable if appointment doesn't require patient initially

        public int? DoctorId { get; set; } // Nullable, backend will handle assigning an available doctor.

        [Required(ErrorMessage = "Service ID is required.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Appointment Date/Time is required.")]
        public DateTime AppointmentDateTime { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public AppointmentStatus Status { get; set; } // Made nullable to match model, but [Required] ensures value on creation

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }

        // NEW: Patient details, required if PatientId is not provided
        [RequiredIfPatientIdIsNull(ErrorMessage = "Patient's first name is required if Patient ID is not provided.")]
        [StringLength(50)]
        public string? FirstName { get; set; } // Nullable, but required by custom attribute

        [RequiredIfPatientIdIsNull(ErrorMessage = "Patient's last name is required if Patient ID is not provided.")]
        [StringLength(50)]
        public string? LastName { get; set; } // Nullable, but required by custom attribute

        [RequiredIfPatientIdIsNull(ErrorMessage = "Patient's email is required if Patient ID is not provided.")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; } // Nullable, but required by custom attribute

        [RequiredIfPatientIdIsNull(ErrorMessage = "Patient's contact number is required if Patient ID is not provided.")]
        [StringLength(20)]
        public string? ContactNumber { get; set; } // Nullable, but required by custom attribute

    }
}
