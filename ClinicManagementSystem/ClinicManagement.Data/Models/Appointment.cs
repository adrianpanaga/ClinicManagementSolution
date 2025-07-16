// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.Data\Models\Appointment.cs

using ClinicManagement.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement.Data.Models
{
    public partial class Appointment
    {
        public Appointment()
        {
            MedicalRecords = new HashSet<MedicalRecord>();
        }

        [Key]
        public int AppointmentId { get; set; }

        public int? PatientId { get; set; } // Already made nullable for Patient soft delete


        // --- IMPORTANT CHANGE: Make DoctorId nullable ---
        public int? DoctorId { get; set; }

        public int ServiceId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime AppointmentDateTime { get; set; }

        // Change from string to enum
        [Required]
        [Column(TypeName = "nvarchar(50)")] // Store as string in DB for readability/backward compatibility
        public AppointmentStatus Status { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        // --- IMPORTANT CHANGE: Make Doctor navigation property nullable ---
        [ForeignKey("DoctorId")]
        public virtual StaffDetail? Doctor { get; set; } // Now nullable

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }

        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; }
    }
}
