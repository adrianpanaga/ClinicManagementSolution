// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\StaffDetails\CreateStaffDetailDto.cs

using System.ComponentModel.DataAnnotations;
using System;

namespace ClinicManagement.Api.DTOs.StaffDetails
{
    // DTO for creating a new StaffDetail (Write operations - POST)
    public class CreateStaffDetailDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50)]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Job Title is required.")]
        [MaxLength(50)]
        public string? JobTitle { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }

        [Required(ErrorMessage = "Contact number is required.")]
        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [MaxLength(100)]
        public string? Email { get; set; }

        // UserId should typically be linked during user registration or by an admin, not directly in creation DTO
        // public int? UserId { get; set; }
    }
}
