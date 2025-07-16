// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\StaffDetails\StaffDetailDto.cs

using System;
using System.ComponentModel.DataAnnotations;
// Assuming you have a DTO for User if you want to expose User details within StaffDetailDto
// using ClinicManagement.Api.DTOs.Users;

namespace ClinicManagement.Api.DTOs.StaffDetails
{
    // DTO for retrieving StaffDetail data (Read operations)
    public class StaffDetailDto
    {
        public int StaffId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? JobTitle { get; set; }
        public string? Specialization { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UserId { get; set; } // Link to the associated User ID if exposed

        // --- NEW PROPERTY FOR SOFT DELETE STATUS ---
        public bool IsDeleted { get; set; }

        public static implicit operator StaffDetailDto(DoctorForBookingDto v)
        {
            throw new NotImplementedException();
        }

        // You might consider adding a UserDto here if you want to expose full user details,
        // but for now, we'll keep it simple with just UserId.
        // public UserDto? User { get; set; }
    }
}
