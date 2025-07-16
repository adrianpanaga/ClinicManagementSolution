namespace ClinicManagement.Api.DTOs.Users
{
    public class UpdateUserDto
    {
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
        // Password update will be a separate endpoint for security best practices.
    }
}
