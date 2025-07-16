// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\Patients\UpdatePatientDto.cs

using ClinicManagement.Api.DTOs.util;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // Added for JsonConverter

namespace ClinicManagement.Api.DTOs.Patients
{
    public class UpdatePatientDto
    {
        [JsonIgnore]
        public int PatientId { get; set; }

        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? MiddleName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))] // Ensure DateOnly is serialized correctly
        public DateOnly? DateOfBirth { get; set; } // Changed to DateOnly?

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(5)]
        public string? BloodType { get; set; }

        [MaxLength(100)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(20)]
        public string? EmergencyContactNumber { get; set; }

        [MaxLength(255)]
        public string? PhotoUrl { get; set; }
    }
}
