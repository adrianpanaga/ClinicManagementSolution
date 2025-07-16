// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.Data\Models\InventoryItem.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement.Data.Models
{
    public partial class InventoryItem
    {
        public InventoryItem()
        {
            ItemBatches = new HashSet<ItemBatch>();
            //MedicalRecords = new HashSet<MedicalRecord>();
        }

        [Key]
        public int ItemId { get; set; }

        [Required]
        [StringLength(255)]
        public string ItemName { get; set; } = null!; // Name of the inventory item (e.g., "Paracetamol 500mg")

        [StringLength(100)]
        public string? Category { get; set; } // e.g., "Medication", "Surgical Supply", "Vaccine"

        [StringLength(50)]
        public string? UnitOfMeasure { get; set; } // e.g., "Tablet", "Bottle", "Box", "mL"

        [Column(TypeName = "decimal(18, 2)")] // Example: Cost per unit
        public decimal? PurchasePrice { get; set; }

        [Column(TypeName = "decimal(18, 2)")] // Example: Selling price per unit
        public decimal? SellingPrice { get; set; }

        public int VendorId { get; set; } // Assuming VendorId is an integer foreign key

        [Required]
        public Vendor Vendor { get; set; } // Assuming Vendor is a navigation property to a Vendor class


        public int? ReorderLevel { get; set; } // Minimum stock level before reordering

        public int? LeadTimeDays { get; set; } // Days it takes to receive a new order

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        // --- Soft Delete Property ---
        public bool IsDeleted { get; set; } = false; // Default to false (not deleted)

        // Navigation properties
        public virtual ICollection<ItemBatch> ItemBatches { get; set; } = new List<ItemBatch>();
        // public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } // Uncomment if linking MedicalRecords
    }
}
