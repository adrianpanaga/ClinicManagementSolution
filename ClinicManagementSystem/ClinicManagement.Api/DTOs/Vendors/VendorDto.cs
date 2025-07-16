// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\Vendors\VendorDto.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.Vendors
{
    // DTO for retrieving Vendor data (Read operations)
    public class VendorDto
    {
        public int VendorId { get; set; }
        public string? VendorName { get; set; }

        // --- ADDED NEW PROPERTIES ---
        public string? ContactPerson { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        // --- END NEW PROPERTIES ---

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
