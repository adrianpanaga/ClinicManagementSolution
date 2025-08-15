// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\Controllers\AppointmentsController.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // For authorization attributes
using System.Security.Claims; // For accessing user claims (e.g., UserId)
using ClinicManagement.Data;
using ClinicManagement.Data.Context; // Your DbContext
using ClinicManagement.Data.Models;   // Your EF Core models
using ClinicManagement.Api.DTOs.Appointments; // Your Appointment DTOs
using ClinicManagement.Api.DTOs.Patients;     // For PatientDetailsDto
using ClinicManagement.Api.DTOs.StaffDetails; // For StaffDetailDto (Doctor)
using ClinicManagement.Api.DTOs.Services;
using ClinicManagement.Data.Models.Enums;    // For ServiceDto

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All actions in this controller require an authenticated user by default
    public class AppointmentsController : ControllerBase
    {
        private readonly ClinicManagementDbContext _context;

        public AppointmentsController(ClinicManagementDbContext context)
        {
            _context = context;
        }

        // Helper method to map Appointment model to AppointmentDto
        private static AppointmentDto MapToAppointmentDto(Appointment appointment)
        {
            if (appointment == null) return null; // CS8603: Expected null return.

            return new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                ServiceId = appointment.ServiceId,
                AppointmentDateTime = appointment.AppointmentDateTime,
                Status = appointment.Status,
                Notes = appointment.Notes,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt,
                // Manually map related entities to their respective DTOs
                Patient = appointment.Patient != null ? new PatientDetailsDto
                {
                    PatientId = appointment.Patient.PatientId,
                    FirstName = appointment.Patient.FirstName,
                    MiddleName = appointment.Patient.MiddleName,
                    LastName = appointment.Patient.LastName,
                    Gender = appointment.Patient.Gender,
                    DateOfBirth = appointment.Patient.DateOfBirth.HasValue ? DateOnly.FromDateTime(appointment.Patient.DateOfBirth.Value) : (DateOnly?)null,
                    Address = appointment.Patient.Address,
                    ContactNumber = appointment.Patient.ContactNumber,
                    Email = appointment.Patient.Email,
                    BloodType = appointment.Patient.BloodType,
                    EmergencyContactName = appointment.Patient.EmergencyContactName,
                    EmergencyContactNumber = appointment.Patient.EmergencyContactNumber,
                    PhotoUrl = appointment.Patient.PhotoUrl,
                    CreatedAt = appointment.Patient.CreatedAt,
                    UpdatedAt = appointment.Patient.UpdatedAt,
                    UserId = appointment.Patient.UserId,
                    IsDeleted = appointment.Patient.IsDeleted
                } : null,
                Doctor = appointment.Doctor != null ? new DoctorForBookingDto
                {
                    StaffId = appointment.Doctor.StaffId,
                    FirstName = appointment.Doctor.FirstName,
                    LastName = appointment.Doctor.LastName,
                    JobTitle = appointment.Doctor.JobTitle,
                    Specialization = appointment.Doctor.Specialization,
                } : null,
                Service = appointment.Service != null ? new ServiceDto
                {
                    ServiceId = appointment.Service.ServiceId,
                    ServiceName = appointment.Service.ServiceName,
                    Description = appointment.Service.Description,
                    Price = appointment.Service.Price,
                    CreatedAt = appointment.Service.CreatedAt,
                    UpdatedAt = appointment.Service.UpdatedAt
                } : null
            };
        }

        // GET: api/Appointments
        /// <summary>
        /// Retrieves all appointments. Accessible by Admins, Receptionists. Doctors/Nurses/Patients see filtered results.
        /// </summary>
        /// <returns>A list of AppointmentDto.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Receptionist,Doctor,Nurse,Patient")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments()
        {
            if (_context.Appointments == null)
            {
                return NotFound();
            }

            IQueryable<Appointment> query = _context.Appointments
                                                .Include(a => a.Patient)
                                                .Include(a => a.Doctor)
                                                .Include(a => a.Service);

            var appointments = await query.ToListAsync();

            // Apply specific authorization filters based on role if not Admin/HR/Receptionist
            if (!(User.IsInRole("Admin") || User.IsInRole("Receptionist")))
            {
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized("Could not identify current user.");
                }

                if (User.IsInRole("Patient"))
                {
                    var patientIdForCurrentUser = await _context.Patients
                                                                  .Where(p => p.UserId == currentUserId)
                                                                  .Select(p => p.PatientId)
                                                                  .FirstOrDefaultAsync();
                    appointments = appointments.Where(a => a.PatientId == patientIdForCurrentUser).ToList();
                }
                else if (User.IsInRole("Doctor") || User.IsInRole("Nurse"))
                {
                    var staffIdForCurrentUser = await _context.StaffDetails
                                                                .Where(sd => sd.UserId == currentUserId)
                                                                .Select(sd => sd.StaffId)
                                                                .FirstOrDefaultAsync();
                    appointments = appointments.Where(a => a.DoctorId == staffIdForCurrentUser).ToList();
                }
                else
                {
                    return Forbid("You do not have sufficient permissions to view appointments.");
                }
            }

            return appointments.Select(a => MapToAppointmentDto(a)).ToList();
        }

        // GET: api/Appointments/5
        /// <summary>
        /// Retrieves a specific appointment by ID.
        /// Requires Admin, HR, Receptionist, or if patient/doctor/nurse, then it must be their own.
        /// </summary>
        /// <param name="id">The ID of the appointment.</param>
        /// <returns>The AppointmentDto if found, otherwise NotFound.</returns>
        // Helper method for GetAppointments might be needed for CreatedAtAction
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<AppointmentDto>> GetAppointments(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .Include(a => a.Patient)
                .Select(a => new AppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    PatientId = a.PatientId,
                    DoctorId = a.DoctorId,
                    ServiceId = a.ServiceId,
                    AppointmentDateTime = a.AppointmentDateTime,
                    Status = a.Status,
                    Notes = a.Notes,
                    Doctor = new DoctorForBookingDto
                    {
                        StaffId = a.Doctor.StaffId,
                        FirstName = a.Doctor.FirstName,
                        LastName = a.Doctor.LastName,
                        Specialization = a.Doctor.Specialization,
                        JobTitle = a.Doctor.JobTitle
                    },
                    Service = new ServiceDto
                    {
                        ServiceId = a.Service.ServiceId,
                        ServiceName = a.Service.ServiceName,
                        Description = a.Service.Description,
                        Price = a.Service.Price
                    },
                    Patient = a.Patient != null ? new PatientDetailsDto // FIX: Null check before mapping Patient
                    {
                        PatientId = a.Patient.PatientId,
                        FirstName = a.Patient.FirstName, // FIX: Use FirstName
                        LastName = a.Patient.LastName,   // FIX: Use LastName
                        Email = a.Patient.Email,
                        ContactNumber = a.Patient.ContactNumber
                    } : null
                })
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return appointment;
        }

        /// <summary>
        /// Gets available time slots for booking an appointment.
        /// This endpoint does NOT require authentication for the public patient portal.
        /// </summary>
        /// <param name="serviceId">The ID of the service.</param>
        /// <param name="date">The date for the appointment (format:YYYY-MM-DD).</param>
        /// <param name="doctorId">Optional: The ID of a specific doctor (StaffId).</param>
        /// <returns>A list of available time slots (e.g., "09:00", "09:30").</returns>
        /// <summary>
        /// Gets available time slots for booking an appointment.
        /// This endpoint does NOT require authentication for the public patient portal.
        /// </summary>
        // FIX: Re-insert GetAvailableSlots method from previous iteration
        [HttpGet("AvailableSlots")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetAvailableSlots(
            [FromQuery] int serviceId,
            [FromQuery] DateOnly date,
            [FromQuery] int? doctorId = null)
        {
            int? nullableDoctorId = doctorId;

            var clinicSettings = await _context.ClinicSettings.FirstOrDefaultAsync();
            if (clinicSettings == null)
            {
                return StatusCode(500, "Clinic operating hours not configured in the database.");
            }

            TimeOnly clinicOpenTime = clinicSettings.OpenTime;
            TimeOnly clinicCloseTime = clinicSettings.CloseTime;
            TimeOnly lunchStart = clinicSettings.LunchStartTime;
            TimeOnly lunchEnd = clinicSettings.LunchEndTime;

            TimeSpan appointmentDuration = TimeSpan.FromMinutes(30);

            DateTime currentDateTime = DateTime.Now;
            DateOnly todayDate = DateOnly.FromDateTime(currentDateTime);
            TimeOnly currentTime = TimeOnly.FromDateTime(currentDateTime);

            var existingAppointmentsQuery = _context.Appointments
                .Where(a => DateOnly.FromDateTime(a.AppointmentDateTime) == date &&
                            !a.Status.Equals("Cancelled"));

            if (doctorId.HasValue)
            {
                existingAppointmentsQuery = existingAppointmentsQuery.Where(a => a.DoctorId == doctorId.Value);

                var doctor = await _context.StaffDetails.FirstOrDefaultAsync(s => s.StaffId == doctorId.Value);
                if (doctor == null || string.IsNullOrEmpty(doctor.Specialization))
                {
                    return Ok(new List<string>());
                }
            }

            var existingAppointments = await existingAppointmentsQuery.ToListAsync();

            var occupiedTimeRanges = new List<(DateTime Start, DateTime End)>();
            foreach (var appt in existingAppointments)
            {
                DateTime apptStart = appt.AppointmentDateTime;
                DateTime apptEnd = apptStart.Add(appointmentDuration);
                occupiedTimeRanges.Add((apptStart, apptEnd));
            }

            List<string> availableSlots = new List<string>();
            DateTime currentSlotCandidateStart = date.ToDateTime(clinicOpenTime);

            while (currentSlotCandidateStart.TimeOfDay.Add(appointmentDuration) <= clinicCloseTime.ToTimeSpan())
            {
                DateTime currentSlotCandidateEnd = currentSlotCandidateStart.Add(appointmentDuration);
                bool isOccupied = false;

                if (date == todayDate && currentSlotCandidateStart < currentDateTime)
                {
                    isOccupied = true;
                }

                if (!isOccupied)
                {
                    TimeOnly slotStartTime = TimeOnly.FromDateTime(currentSlotCandidateStart);
                    TimeOnly slotEndTime = TimeOnly.FromDateTime(currentSlotCandidateEnd);

                    if (slotStartTime < lunchEnd && slotEndTime > lunchStart)
                    {
                        isOccupied = true;
                    }
                }

                if (!isOccupied)
                {
                    foreach (var occupiedRange in occupiedTimeRanges)
                    {
                        if (currentSlotCandidateStart < occupiedRange.Item2 && currentSlotCandidateEnd > occupiedRange.Item1)
                        {
                            isOccupied = true;
                            break;
                        }
                    }
                }

                if (!isOccupied)
                {
                    availableSlots.Add(currentSlotCandidateStart.ToString("HH:mm"));
                }

                currentSlotCandidateStart = currentSlotCandidateStart.Add(appointmentDuration);
            }

            return Ok(availableSlots);
        }

        // NEW: Get Appointments by Patient ID
        /// <summary>
        /// Retrieves all appointments for a specific patient.
        /// Publicly accessible for the patient portal's 'My Appointments' view.
        /// </summary>
        /// <param name="patientId">The ID of the patient.</param>
        /// <returns>A list of AppointmentDto for the specified patient.</returns>
        [HttpGet("ByPatientId/{patientId}")] // Route: GET /api/Appointments/ByPatientId/{patientId}
        [AllowAnonymous] // Allow public access
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointmentsByPatientId(int patientId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDateTime) // Order by full DateTime
                .Select(a => new AppointmentDto
                {
                    AppointmentId = a.AppointmentId, // Use correct ID property
                    AppointmentDateTime = a.AppointmentDateTime.Date,
                    Status = a.Status,
                    Notes = a.Notes,
                    Patient = new PatientDetailsDto
                    {
                        PatientId = a.Patient.PatientId,
                        FirstName = a.Patient.FirstName,
                        LastName = a.Patient.LastName,
                        ContactNumber = a.Patient.ContactNumber,
                        Email = a.Patient.Email
                    },
                    Service = new ServiceDto
                    {
                        ServiceId = a.Service.ServiceId,
                        ServiceName = a.Service.ServiceName
                    },
                    Doctor = a.Doctor != null ? new DoctorForBookingDto
                    {
                        StaffId = a.Doctor.StaffId,
                        FirstName = a.Doctor.FirstName,
                        LastName = a.Doctor.LastName,
                        Specialization = a.Doctor.Specialization
                    } : null
                })
                .ToListAsync();

            if (!appointments.Any())
            {
                return NotFound($"No appointments found for patient ID: {patientId}");
            }

            return Ok(appointments);
        }

        // NEW: Update Appointment Status (e.g., mark as completed, cancelled)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Receptionist,Nurse,Admin")] // Only certain roles can update status
        public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromQuery] string status)
        {
            var appointment = await _context.Appointments.FindAsync(id); // Use FindAsync with int ID
            if (appointment == null)
            {
                return NotFound();
            }

            if (!Enum.TryParse(status, true, out AppointmentStatus newStatus))
            {
                return BadRequest("Invalid appointment status.");
            }

            appointment.Status = newStatus;
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }

        // PUT: api/Appointments/5
        /// <summary>
        /// Updates an existing appointment.
        /// Requires Admin, HR, or Receptionist role.
        /// </summary>
        /// <param name="id">The ID of the appointment to update.</param>
        /// <param name="updateAppointmentDto">The DTO object with updated data.</param>
        /// <returns>NoContent if successful, BadRequest if ID mismatch, NotFound if appointment not found, or throws exception for concurrency.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,HR,Receptionist")] // Roles allowed to update appointments
        public async Task<IActionResult> PutAppointment(int id, UpdateAppointmentDto updateAppointmentDto)
        {
            if (id != updateAppointmentDto.AppointmentId)
            {
                return BadRequest("Mismatched Appointment ID in route and body.");
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Validate foreign keys exist and are not soft-deleted if updated
            if (updateAppointmentDto.PatientId.HasValue && updateAppointmentDto.PatientId != appointment.PatientId)
            {
                if (!await _context.Patients.AnyAsync(p => p.PatientId == updateAppointmentDto.PatientId.Value && !p.IsDeleted))
                {
                    return BadRequest($"Patient with ID {updateAppointmentDto.PatientId.Value} does not exist or is deleted.");
                }
            }
            if (updateAppointmentDto.DoctorId.HasValue && updateAppointmentDto.DoctorId != appointment.DoctorId)
            {
                if (!await _context.StaffDetails.AnyAsync(s => s.StaffId == updateAppointmentDto.DoctorId.Value && !s.IsDeleted))
                {
                    return BadRequest($"Doctor (Staff) with ID {updateAppointmentDto.DoctorId.Value} does not exist or is deleted.");
                }
            }
            if (updateAppointmentDto.ServiceId.HasValue && updateAppointmentDto.ServiceId != appointment.ServiceId)
            {
                if (!await _context.Services.AnyAsync(s => s.ServiceId == updateAppointmentDto.ServiceId.Value))
                {
                    return BadRequest($"Service with ID {updateAppointmentDto.ServiceId.Value} does not exist.");
                }
            }

            // Manually map properties from DTO to the existing EF Core entity
            appointment.PatientId = updateAppointmentDto.PatientId;
            appointment.DoctorId = updateAppointmentDto.DoctorId ?? appointment.DoctorId; // DoctorId is non-nullable in model, needs coalescing
            appointment.ServiceId = updateAppointmentDto.ServiceId ?? appointment.ServiceId; // ServiceId is non-nullable in model, needs coalescing
            appointment.AppointmentDateTime = updateAppointmentDto.AppointmentDateTime ?? appointment.AppointmentDateTime; // DateTime is non-nullable, needs coalescing
            appointment.Status = updateAppointmentDto.Status;
            appointment.Notes = updateAppointmentDto.Notes ?? appointment.Notes;
            appointment.UpdatedAt = DateTime.UtcNow; // Set update timestamp

            _context.Entry(appointment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw; // Re-throw if it's a genuine concurrency issue
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Creates a new appointment. Handles doctor assignment if no specific doctor is chosen.
        /// </summary>
        /// <param name="createAppointmentDto">The appointment details.</param>
        /// <returns>The created AppointmentDto.</returns>
        [HttpPost]
        [AllowAnonymous] // Keep AllowAnonymous for public booking
        public async Task<ActionResult<AppointmentDto>> PostAppointment(CreateAppointmentDto createAppointmentDto)
        {
            // --- 1. Determine Patient Entity (Lookup or Create) ---
            Patient patientEntity; // This will hold the definitive Patient entity for the appointment

            if (createAppointmentDto.PatientId.HasValue)
            {
                // PatientId provided from frontend (e.g., from prior verification)
                var existingPatientById = await _context.Patients
                    .Where(p => p.PatientId == createAppointmentDto.PatientId.Value && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingPatientById == null)
                {
                    return BadRequest("Provided Patient ID does not exist or is deleted.");
                }
                patientEntity = existingPatientById; // Use the found existing entity
            }
            else
            {
                // PatientId NOT provided, attempt to find by contact/email or create new
                // Model validation (RequiredIfPatientIdIsNull) should have already checked these for null/empty
                var existingPatientByContact = await _context.Patients
                    .FirstOrDefaultAsync(p => (p.Email == createAppointmentDto.Email && p.Email != null) ||
                                              (p.ContactNumber == createAppointmentDto.ContactNumber && p.ContactNumber != null));

                if (existingPatientByContact != null)
                {
                    patientEntity = existingPatientByContact; // Found existing patient by contact, use their entity
                    // Optional: Update existing patient's details from DTO if desired (e.g., name updates)
                    // _context.Entry(patientEntity).State = EntityState.Modified; // Uncomment if updating existing
                }
                else
                {
                    // No existing patient found, create a new Patient record
                    var newPatient = new Patient
                    {
                        FirstName = createAppointmentDto.FirstName,
                        LastName = createAppointmentDto.LastName,
                        Email = createAppointmentDto.Email,
                        ContactNumber = createAppointmentDto.ContactNumber,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false,
                        // DateOfBirth = createAppointmentDto.DateOfBirth; // Uncomment if DTO has it
                        // ... other patient fields from DTO if you add them
                    };
                    _context.Patients.Add(newPatient);
                    patientEntity = newPatient; // Assign the newly added entity
                }

                // CRITICAL FIX: SaveChangesAsync must happen here to get the auto-generated PatientId for new patients
                await _context.SaveChangesAsync();
            }

            // At this point, patientEntity should be guaranteed to be non-null
            // and patientEntity.PatientId will have the correct ID (either existing or newly generated).
            // This also means the 'Patient' navigation property can be correctly linked.
            if (patientEntity == null || patientEntity.PatientId == 0) // Defensive check, should ideally not be hit
            {
                return StatusCode(500, "Internal error: Patient ID could not be determined or assigned.");
            }

            // --- 2. Doctor Assignment Logic (from previous steps) ---
            if (!createAppointmentDto.DoctorId.HasValue)
            {
                var allAvailableDoctors = await _context.StaffDetails
                    .Where(s => !string.IsNullOrEmpty(s.Specialization) && !s.IsDeleted)
                    .ToListAsync();
                if (!allAvailableDoctors.Any())
                {
                    return BadRequest("No doctors are available to be assigned.");
                }
                var appointmentDateTime = createAppointmentDto.AppointmentDateTime;
                var appointmentDuration = TimeSpan.FromMinutes(30);

                DateTime newAppointmentStart = appointmentDateTime;
                DateTime newAppointmentEnd = newAppointmentStart.Add(appointmentDuration);

                var overlappingAppointments = await _context.Appointments
                    .Where(a => !a.Status.Equals("Cancelled") &&
                                a.AppointmentDateTime < newAppointmentEnd &&
                                a.AppointmentDateTime > newAppointmentStart.Subtract(appointmentDuration))
                    .Select(a => a.DoctorId)
                    .Distinct()
                    .ToListAsync();

                var assignedDoctor = allAvailableDoctors
                    .FirstOrDefault(d => !overlappingAppointments.Contains(d.StaffId));

                if (assignedDoctor == null)
                {
                    return BadRequest("No doctor available for the selected time and service. Please choose another slot.");
                }
                createAppointmentDto.DoctorId = assignedDoctor.StaffId; // Assign the found doctor's ID
            }

            // --- 3. Create and Save Appointment Entity ---
            var appointment = new Appointment
            {
                PatientId = patientEntity.PatientId, // Assign the confirmed PatientId from the entity
                DoctorId = createAppointmentDto.DoctorId.Value,
                ServiceId = createAppointmentDto.ServiceId,
                AppointmentDateTime = createAppointmentDto.AppointmentDateTime,
                Status = createAppointmentDto.Status,
                Notes = createAppointmentDto.Notes,
                CreatedAt = DateTime.UtcNow
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(); // Save the new appointment

            // --- 4. Eager Load Navigation Properties for DTO Response ---
            // These are crucial for the frontend to display details on the confirmation page.
            // Since patientEntity is already loaded/attached to context, its reference is good.
            await _context.Entry(appointment).Reference(a => a.Doctor).LoadAsync();   // Load Doctor entity
            await _context.Entry(appointment).Reference(a => a.Service).LoadAsync(); // Load Service entity
            // Patient entity is already loaded through patientEntity, no need to LoadAsync again.
            // EF Core should be able to track it if patientEntity was from context or added to context.
            // To ensure appointment.Patient navigation property is set:
            appointment.Patient = patientEntity; // Explicitly set the navigation property on the saved appointment

            // --- 5. Map to AppointmentDto for Response ---
            var createdAppointmentDto = new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                ServiceId = appointment.ServiceId,
                AppointmentDateTime = appointment.AppointmentDateTime,
                Status = appointment.Status,
                Notes = appointment.Notes,
                // Patient, Doctor, Service will now be correctly populated because navigation properties are loaded
                Patient = (appointment.Patient != null) ? new PatientDetailsDto
                {
                    PatientId = appointment.Patient.PatientId,
                    // Ensure these properties exist on your Patient model and are populated
                    FirstName = appointment.Patient.FirstName,
                    LastName = appointment.Patient.LastName,
                    Email = appointment.Patient.Email,
                    ContactNumber = appointment.Patient.ContactNumber
                } : null, // Should not be null here if logic is followed
                Doctor = (appointment.Doctor != null) ? new DoctorForBookingDto
                {
                    StaffId = appointment.Doctor.StaffId,
                    FirstName = appointment.Doctor.FirstName,
                    LastName = appointment.Doctor.LastName,
                    Specialization = appointment.Doctor.Specialization,
                    JobTitle = appointment.Doctor.JobTitle
                } : null, // Should not be null here
                Service = (appointment.Service != null) ? new ServiceDto
                {
                    ServiceId = appointment.Service.ServiceId,
                    ServiceName = appointment.Service.ServiceName,
                    Description = appointment.Service.Description,
                    Price = appointment.Service.Price
                } : null // Should not be null here
            };

            return CreatedAtAction(nameof(GetAppointments), new { id = appointment.AppointmentId }, createdAppointmentDto);
        }

        // DELETE: api/Appointments/5 (Hard Delete)
        /// <summary>
        /// Deletes an appointment by its ID. Note: This is a hard delete.
        /// Requires Admin or HR role.
        /// </summary>
        /// <param name="id">The ID of the appointment to delete.</param>
        /// <returns>NoContent if successful, or NotFound if the appointment does not exist.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,HR")] // Only Admin or HR can delete appointments
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            if (_context.Appointments == null)
            {
                return NotFound();
            }
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return NoContent(); // Indicate successful deletion
        }

        private bool AppointmentExists(int id)
        {
            return (_context.Appointments?.Any(e => e.AppointmentId == id)).GetValueOrDefault();
        }
    }
}
