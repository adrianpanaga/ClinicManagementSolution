// Location: ClinicManagement.Data/Models/Service.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement.Data.Models
{
    public partial class Service
    {
        public Service()
        {
            Appointments = new HashSet<Appointment>();
            MedicalRecords = new HashSet<MedicalRecord>(); // Added MedicalRecords collection
        }

        [Key]
        [Column("ServiceID")]
        public int ServiceId { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceName { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        [InverseProperty("Service")]
        public virtual ICollection<Appointment> Appointments { get; set; }

        [InverseProperty("Service")] // New: InverseProperty for MedicalRecords if a Service can have multiple MedicalRecords
        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; }
    }
}
