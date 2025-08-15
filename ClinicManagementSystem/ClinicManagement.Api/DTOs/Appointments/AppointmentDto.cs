// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\Appointments\AppointmentDto.cs

using ClinicManagement.Api.DTOs.Patients;     // For PatientDetailsDto
using ClinicManagement.Api.DTOs.Services;    // For ServiceDto
using ClinicManagement.Api.DTOs.StaffDetails; // For StaffDetailDto (Doctor)
using ClinicManagement.Data.Models.Enums;
using System;

namespace ClinicManagement.Api.DTOs.Appointments
{
    // DTO for retrieving Appointment data (Read operations)
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public int? PatientId { get; set; } // Nullable as per model
        public int? DoctorId { get; set; } // Nullable to mirror model
        public int ServiceId { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        // Change from string to enum
        public AppointmentStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // --- ADDED NAVIGATION PROPERTIES TO DTO ---
        public PatientDetailsDto? Patient { get; set; } // Matches PatientDetailsDto
        public DoctorForBookingDto? Doctor { get; set; }     // For the associated Doctor (StaffDetail)
        public ServiceDto? Service { get; set; }        // For the associated Service
    }
}
