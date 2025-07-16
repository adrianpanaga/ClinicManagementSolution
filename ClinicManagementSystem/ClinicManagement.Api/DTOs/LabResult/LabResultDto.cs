// DTOs/LabResult/LabResultDto.cs
using ClinicManagement.Api.DTOs.Appointments; // For AppointmentDto
using ClinicManagement.Api.DTOs.MedicalRecords; // For MedicalRecordDto
using ClinicManagement.Api.DTOs.Patients; // For PatientDetailsDto
using ClinicManagement.Api.DTOs.StaffDetails; // For StaffDetailDto
using System;

// DTOs/LabResult/LabResultDto.cs
// ...
namespace ClinicManagement.Api.DTOs.LabResult
{
    public class LabResultDto
    {

        public int LabResultId { get; set; }
        public int PatientId { get; set; }
        public int? MedicalRecordId { get; set; }
        public int? AppointmentId { get; set; }
        // ... (existing properties) ...
        public string? TestName { get; set; } // Make nullable
        public string? ResultValue { get; set; } // Make nullable
        public string? Unit { get; set; } // Make nullable
        public string? ReferenceRange { get; set; } // Make nullable
        public string? Interpretation { get; set; } // Make nullable

        public DateOnly ResultDate { get; set; } // Will be serialized by custom converter
        public int? OrderedByStaffId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }


        // Navigation properties in DTO are usually nullable if not always eagerly loaded
        public PatientDetailsDto? Patient { get; set; } // Make nullable
        public MedicalRecordsDto? MedicalRecord { get; set; } // Make nullable
        public AppointmentDto? Appointment { get; set; } // Make nullable
        public StaffDetailDto? OrderedByStaff { get; set; } // Make nullable
    }
}