// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\StaffDetails\UpdateStaffDetailDto.cs

using System.ComponentModel.DataAnnotations;
using System;

namespace ClinicManagement.Api.DTOs.StaffDetails
{
    // DTO for updating an existing StaffDetail (Write operations - PUT)
    public class UpdateStaffDetailDto
    {
        [Required(ErrorMessage = "Staff ID is required for update.")]
        public int StaffId { get; set; }

        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? MiddleName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [MaxLength(50)]
        public string? JobTitle { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [MaxLength(100)]
        public string? Email { get; set; }

        // UserId should typically not be updated directly via this DTO
        // public int? UserId { get; set; }
    }
}
