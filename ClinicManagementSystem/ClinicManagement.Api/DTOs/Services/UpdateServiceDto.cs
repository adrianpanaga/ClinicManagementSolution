// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\Services\UpdateServiceDto.cs

using System.ComponentModel.DataAnnotations;
using System;

namespace ClinicManagement.Api.DTOs.Services
{
    // DTO for updating an existing Service (Write operations - PUT)
    public class UpdateServiceDto
    {
        [Required(ErrorMessage = "Service ID is required for update.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Service Name is required.")]
        [MaxLength(100, ErrorMessage = "Service Name cannot exceed 100 characters.")]
        public string? ServiceName { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 9999999.99, ErrorMessage = "Price must be a positive value.")]
        public decimal Price { get; set; }
    }
}
