// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\Patients\PatientDetailsDto.cs

using ClinicManagement.Api.DTOs.util;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // Added for JsonConverter

namespace ClinicManagement.Api.DTOs.Patients
{
    // DTO for retrieving detailed Patient data (Read operations)
    public class PatientDetailsDto
    {
        public int PatientId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))] // Ensure DateOnly is serialized correctly
        public DateOnly? DateOfBirth { get; set; } // Changed to DateOnly? for nullable consistency

        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? BloodType { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactNumber { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UserId { get; set; } // Link to the associated User ID if exposed

        // --- NEW PROPERTY FOR SOFT DELETE STATUS ---
        public bool IsDeleted { get; set; }
    }
}
