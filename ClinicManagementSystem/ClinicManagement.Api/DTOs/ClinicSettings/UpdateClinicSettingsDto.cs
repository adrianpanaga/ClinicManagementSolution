using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.ClinicSettings
{
    public class UpdateClinicSettingsDto
    {
        [Required]
        public string OpenTime { get; set; }
        [Required]
        public string CloseTime { get; set; }
        [Required]
        public string LunchStartTime { get; set; }
        [Required]
        public string LunchEndTime { get; set; }
    }
}
