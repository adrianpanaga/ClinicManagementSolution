// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\Patients\CreatePatientDto.cs

using ClinicManagement.Api.DTOs.util;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // Added for JsonConverter

namespace ClinicManagement.Api.DTOs.Patients
{
    public class CreatePatientDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50)]
        public string? LastName { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))] // Ensure DateOnly is serialized correctly
        public DateOnly? DateOfBirth { get; set; } // Changed to DateOnly?

        [MaxLength(255)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Contact number is required.")]
        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
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
