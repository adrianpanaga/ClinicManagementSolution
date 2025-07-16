using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagement.Data.Models
{
    public partial class VerificationCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; } // Navigation property

        [Required]
        [StringLength(10)]
        public string Code { get; set; }

        [Required]
        [StringLength(100)]
        public string ContactMethod { get; set; } // Email or Phone Number

        public DateTime SentAt { get; set; } = DateTime.Now; // Default value set in SQL also
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false; // Default value set in SQL also

    }
}
