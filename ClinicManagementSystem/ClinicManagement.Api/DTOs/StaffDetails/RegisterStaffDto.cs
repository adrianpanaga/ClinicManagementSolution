using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.StaffDetails
{
    /// <summary>
    /// DTO for registering a new StaffDetail and their associated User account in a single request.
    /// </summary>
    public class RegisterStaffDto
    {
        // --- User Account Details ---
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;

        // Role for the user account will be derived from JobTitle for simplicity,
        // unless you want a separate 'Role' field here.
        // public string Role { get; set; } // <--- See discussion below

        // --- Staff Detail Information ---
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Middle name cannot exceed 50 characters.")]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Job title is required.")]
        [StringLength(50, ErrorMessage = "Job title cannot exceed 50 characters.")]
        public string JobTitle { get; set; } = string.Empty; // e.g., "Doctor", "Receptionist", "Nurse"

        [StringLength(100, ErrorMessage = "Specialization cannot exceed 100 characters.")]
        public string? Specialization { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required.")]
        [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters.")]
        public string ContactNumber { get; set; } = string.Empty;
    }
}