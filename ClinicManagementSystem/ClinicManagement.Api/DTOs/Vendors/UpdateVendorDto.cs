// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\Vendors\UpdateVendorDto.cs

using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.Vendors
{
    // DTO for updating an existing Vendor (Write operations - PUT)
    public class UpdateVendorDto
    {
        [Required(ErrorMessage = "Vendor ID is required for update.")]
        public int VendorId { get; set; }

        [MaxLength(100, ErrorMessage = "Vendor Name cannot exceed 100 characters.")]
        public string? VendorName { get; set; }

        // --- ADDED NEW PROPERTIES (Nullable for optional update) ---
        [MaxLength(100, ErrorMessage = "Contact Person cannot exceed 100 characters.")]
        public string? ContactPerson { get; set; }

        [MaxLength(20, ErrorMessage = "Contact Number cannot exceed 20 characters.")]
        public string? ContactNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string? Email { get; set; }

        [MaxLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
        public string? Address { get; set; }
        // --- END NEW PROPERTIES ---

        [MaxLength(255, ErrorMessage = "Notes cannot exceed 255 characters.")]
        public string? Notes { get; set; }

    }
}
