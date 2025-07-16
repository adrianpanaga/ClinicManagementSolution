// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.Data\Models\ItemBatch.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Keep this for other attributes
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement.Data.Models
{
    public partial class ItemBatch
    {
        public ItemBatch()
        {
            StockTransactions = new HashSet<StockTransaction>();
        }

        [Key]
        public int BatchId { get; set; }

        // --- IMPORTANT CHANGE: [Required] attribute removed from ItemId ---
        public int? ItemId { get; set; } // Foreign Key to InventoryItem - MUST BE NULLABLE

        [Required]
        [StringLength(100)]
        public string BatchNumber { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ExpirationDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow.Date;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CostPerUnit { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SalePrice { get; set; }

        public int? VendorId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("ItemId")]
        public virtual InventoryItem? Item { get; set; } // MUST BE NULLABLE

        [ForeignKey("VendorId")]
        public virtual Vendor? Vendor { get; set; }

        public virtual ICollection<StockTransaction> StockTransactions { get; set; }
    }
}
