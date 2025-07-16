namespace ClinicManagement.Api.DTOs.Auth
{
    public class RegisterUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // Optionally, include role name for registration, though it's safer
        // for an admin to assign roles post-registration for staff,
        // and default to 'Patient' for public registrations.
        public string? RoleName { get; set; }
    }
}
