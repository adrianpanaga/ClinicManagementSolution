// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\MedicalRecords\MedicalRecordsDto.cs

using System;
using ClinicManagement.Api.DTOs.Appointments; // Using existing Appointment DTO
using ClinicManagement.Api.DTOs.Patients;    // Using existing Patient DTO
using ClinicManagement.Api.DTOs.StaffDetails; // Using existing StaffDetail DTO
using ClinicManagement.Api.DTOs.Services;    // Using existing Service DTO

namespace ClinicManagement.Api.DTOs.MedicalRecords
{
    // DTO for retrieving Medical Record data (Read operations)
    public class MedicalRecordsDto
    {
        public int RecordId { get; set; }
        public int PatientId { get; set; }
        public int? AppointmentId { get; set; }
        public int StaffId { get; set; } // Doctor who created the record
        public int? ServiceId { get; set; } // Service associated with this record
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public string? Prescription { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } // Soft delete property for medical records

        // Include DTOs for related entities for a complete response
        public PatientDetailsDto? Patient { get; set; } // Assuming your Patient DTO is PatientDetailsDto
        public StaffDetailDto? Staff { get; set; }
        public AppointmentDto? Appointment { get; set; }
        public ServiceDto? Service { get; set; }
    }
}
