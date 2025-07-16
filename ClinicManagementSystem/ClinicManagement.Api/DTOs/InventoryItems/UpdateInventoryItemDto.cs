// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\InventoryItems\UpdateInventoryItemDto.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // For decimal type attribute if needed

namespace ClinicManagement.Api.DTOs.InventoryItems
{
    public class UpdateInventoryItemDto
    {
        [Required(ErrorMessage = "Item ID is required for update.")]
        public int ItemId { get; set; }

        [StringLength(255, ErrorMessage = "Item name cannot exceed 255 characters.")]
        public string? ItemName { get; set; } = null!; // Nullable as it might not always be updated

        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
        public string? Category { get; set; }

        [StringLength(50, ErrorMessage = "Unit of measure cannot exceed 50 characters.")]
        public string? UnitOfMeasure { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? PurchasePrice { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? SellingPrice { get; set; }

        public int? ReorderLevel { get; set; }

        public int? LeadTimeDays { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vendor ID is required for update.")]
        public int VendorId { get; set; }
    }
}
