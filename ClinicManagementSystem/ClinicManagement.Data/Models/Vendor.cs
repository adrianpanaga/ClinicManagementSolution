// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.Data\Models\Vendor.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement.Data.Models
{
    public partial class Vendor
    {
        public Vendor()
        {
            InventoryItems = new HashSet<InventoryItem>();
        }

        [Key]
        [Column("VendorID")]
        public int VendorId { get; set; }

        [Required]
        [StringLength(100)]
        public string VendorName { get; set; } = null!;

        // --- ADDED NEW PROPERTIES ---
        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        public string? ContactNumber { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }
        // --- END NEW PROPERTIES ---

        [StringLength(255)]
        public string? Notes { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        // Soft delete property
        public bool IsDeleted { get; set; } = false;

        [InverseProperty("Vendor")]
        public virtual ICollection<InventoryItem> InventoryItems { get; set; }
    }
}
