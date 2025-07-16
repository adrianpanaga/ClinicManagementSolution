namespace ClinicManagement.Api.DTOs.StaffDetails
{
    public class DoctorForBookingDto
    {
        public int StaffId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? JobTitle { get; set; } // Could be "Doctor", "Physician", "Specialist"
        public string? Specialization { get; set; } // e.g., "Cardiology", "Dermatology"

        // Optional: Combine name for easier display on frontend
        public string? FullName => $"{FirstName} {LastName}";
    }
}
