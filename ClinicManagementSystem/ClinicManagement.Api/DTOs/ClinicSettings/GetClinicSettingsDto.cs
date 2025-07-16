namespace ClinicManagement.Api.DTOs.ClinicSettings
{
    public class GetClinicSettingsDto
    {
        public int Id { get; set; }
        public string OpenTime { get; set; } // Represent TimeOnly as string for API contract
        public string CloseTime { get; set; }
        public string LunchStartTime { get; set; }
        public string LunchEndTime { get; set; }
    }
}
