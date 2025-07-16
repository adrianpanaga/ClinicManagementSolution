// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\Services\ServiceDto.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.Services
{
    // DTO for retrieving Service data (Read operations)
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
