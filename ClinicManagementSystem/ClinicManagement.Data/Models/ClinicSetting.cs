// ClinicManagement.API/Models/ClinicSetting.cs
using System;
using System.ComponentModel.DataAnnotations; // For [Key]

namespace ClinicManagement.Data.Models
{
    public class ClinicSetting
    {
        [Key] // Primary key
        public int Id { get; set; } // Or use a Guid if you prefer GUIDs for PKs

        // Assuming you'll have only ONE row for general clinic hours.
        // You could also store these as key-value pairs (SettingName, SettingValue)
        // but explicit columns are clearer for fixed types of settings.

        [Required]
        public TimeOnly OpenTime { get; set; } // Clinic opening time

        [Required]
        public TimeOnly CloseTime { get; set; } // Clinic closing time

        [Required]
        public TimeOnly LunchStartTime { get; set; } // Lunch break start

        [Required]
        public TimeOnly LunchEndTime { get; set; } // Lunch break end

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}