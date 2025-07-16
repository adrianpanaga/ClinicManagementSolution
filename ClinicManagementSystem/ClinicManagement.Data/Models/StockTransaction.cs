// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.Data\Models\StockTransaction.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement.Data.Models
{
    public partial class StockTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int BatchId { get; set; } // Foreign Key to ItemBatch

        [Required]
        public int Quantity { get; set; } // Quantity of items transacted (positive for IN, negative for OUT)

        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; } = null!; // e.g., "IN", "OUT", "ADJUSTMENT"

        [StringLength(500)]
        public string? Notes { get; set; } // Additional details about the transaction

        [Column(TypeName = "datetime")]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        // Optional: Link to a StaffDetail if a staff member initiated the transaction
        public int? StaffId { get; set; } // Foreign Key to StaffDetail

        // Optional: Link to a Patient if items are dispensed to a patient (e.g., medication)
        public int? PatientId { get; set; } // Foreign Key to Patient

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("BatchId")]
        public virtual ItemBatch Batch { get; set; } = null!;

        [ForeignKey("StaffId")]
        public virtual StaffDetail? Staff { get; set; } // Nullable as not all transactions require a specific staff

        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; } // Nullable as not all transactions are patient-related
    }
}
