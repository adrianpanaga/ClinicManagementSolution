using ClinicManagement.Api.DTOs.PatientVerification;
using ClinicManagement.Data.Context; // Corrected namespace: ClinicManagement.Data.Context
using ClinicManagement.Data.Models; // Corrected namespace: ClinicManagement.Data.Models
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic; // Added for ICollection<Appointment> in Patient model if not already there
using System.Linq;
using System.Threading.Tasks;

// Corrected namespace to match folder structure
namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // IDE0290: Use primary constructor - Not changing for broader compatibility
    public class PatientVerificationController : ControllerBase
    {
        private readonly ClinicManagementDbContext _context;
        private readonly ILogger<PatientVerificationController> _logger;

        public PatientVerificationController(ClinicManagementDbContext context, ILogger<PatientVerificationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("RequestCode")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RequestCode([FromBody] RequestVerificationCodeDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // CS8601: Possible null reference assignment - Add null-forgiving operator or null check
            // Fixed by adding null-forgiving operator '!' as we expect these to be non-null based on [Required]
            // and trim/tolower are safe for null.
            model.ContactIdentifier = model.ContactIdentifier?.Trim().ToLower()!;
            model.LastName = model.LastName?.Trim(); // LastName can be null, so no '!' needed

            IQueryable<Patient> patientQuery = _context.Patients
                .Where(p => !p.IsDeleted);

            if (model.Method.Equals("email", StringComparison.OrdinalIgnoreCase))
            {
                patientQuery = patientQuery.Where(p => p.Email == model.ContactIdentifier);
            }
            else if (model.Method.Equals("sms", StringComparison.OrdinalIgnoreCase))
            {
                patientQuery = patientQuery.Where(p => p.ContactNumber == model.ContactIdentifier);
            }
            else
            {
                return BadRequest("Invalid verification method specified. Must be 'email' or 'sms'.");
            }

            if (!string.IsNullOrEmpty(model.LastName))
            {
                patientQuery = patientQuery.Where(p => p.LastName == model.LastName);
            }

            var patients = await patientQuery.ToListAsync();

            // CA1860: Prefer using 'Count' or 'Length' properties rather than calling 'Enumerable.Any()'.
            if (patients is not { Count: > 0 }) // Check for null or empty list more efficiently and readably
            {
                // CA2254: The logging message template should not vary between calls.
                _logger.LogWarning("Verification code requested for non-existent or unmatching contact. ContactIdentifier: {ContactIdentifier}", model.ContactIdentifier);
                return Ok(new { message = "If a matching record exists, a verification code has been sent." });
            }

            if (patients.Count > 1)
            {
                _logger.LogWarning("Multiple patients found for contact: {ContactIdentifier}. Using the first match.", model.ContactIdentifier);
            }

            var patient = patients.First();

            string verificationCode = GenerateOtp();
            var expiresAt = DateTime.UtcNow.AddMinutes(5);

            var newVerificationCode = new VerificationCode // IDE0090: 'new' expression can be simplified
            {
                PatientId = patient.PatientId,
                Code = verificationCode,
                ContactMethod = model.ContactIdentifier,
                SentAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsUsed = false
            };
            _context.VerificationCodes.Add(newVerificationCode);
            await _context.SaveChangesAsync();

            // CA2254: The logging message template should not vary between calls.
            _logger.LogInformation("--- SIMULATED SEND ---");
            if (model.Method.Equals("email", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Simulating email send. To: {Email}, Subject: {Subject}, Body: {Body}",
                                        model.ContactIdentifier, "Your Clinic Verification Code", $"Your verification code is: {verificationCode}. It expires in 5 minutes.");
            }
            else
            {
                _logger.LogInformation("Simulating SMS send. To: {Phone}, Message: {Message}",
                                        model.ContactIdentifier, $"Your Clinic verification code: {verificationCode}. Expires in 5 min.");
            }
            _logger.LogInformation("--- END SIMULATED SEND ---");

            return Ok(new { message = "Verification code has been sent." });
        }

        /// <summary>
        /// Verifies a code sent to a patient and returns associated appointments.
        /// Publicly accessible for patient portal.
        /// </summary>
        /// <param name="verifyCodeDto">Contains contact identifier and the code to verify.</param>
        /// <returns>A confirmation message and patient details if verification is successful.</returns>
        [HttpPost("VerifyCode")] // Route: POST /api/PatientVerification/VerifyCode
        [AllowAnonymous] // Publicly accessible
        public async Task<ActionResult<VerifyCodeResultDto>> VerifyCode(VerifyCodeDto verifyCodeDto)
        {
            if (verifyCodeDto == null || string.IsNullOrWhiteSpace(verifyCodeDto.ContactIdentifier) || string.IsNullOrWhiteSpace(verifyCodeDto.Code))
            {
                return BadRequest("Contact identifier and code are required.");
            }

            // Find the patient associated with the contact identifier
            var patient = await _context.Patients
                .Where(p => (p.Email == verifyCodeDto.ContactIdentifier || p.ContactNumber == verifyCodeDto.ContactIdentifier) && !p.IsDeleted)
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                _logger.LogWarning("Verification attempt for non-existent or deleted patient. ContactIdentifier: {ContactIdentifier}", verifyCodeDto.ContactIdentifier);
                return BadRequest(new VerifyCodeResultDto { IsSuccess = false, Message = "Verification failed: Invalid contact or code." });
            }

            // Find the most recent, valid verification code for this patient
            var verificationCodeRecord = await _context.VerificationCodes
                .Where(vc => vc.PatientId == patient.PatientId &&
                             vc.Code == verifyCodeDto.Code &&
                             vc.ExpiresAt > DateTime.UtcNow &&
                             !vc.IsUsed)
                .OrderByDescending(vc => vc.ExpiresAt)
                .FirstOrDefaultAsync();

            if (verificationCodeRecord == null)
            {
                _logger.LogWarning("Verification failed for patient {PatientId}: Invalid or expired code. ContactIdentifier: {ContactIdentifier}", patient.PatientId, verifyCodeDto.ContactIdentifier);
                return BadRequest(new VerifyCodeResultDto { IsSuccess = false, Message = "Verification failed: Invalid contact or code." });
            }

            // Mark code as used (optional, but good for security)
            verificationCodeRecord.IsUsed = true;
            _context.VerificationCodes.Update(verificationCodeRecord);
            await _context.SaveChangesAsync();

            // Verification successful, return patient details
            return Ok(new VerifyCodeResultDto
            {
                IsSuccess = true,
                Message = "Verification successful!",
                PatientId = patient.PatientId, // CRITICAL: Return PatientId
                FullName = $"{patient.FirstName} {patient.LastName}", // Return full name
                Email = patient.Email,
                ContactNumber = patient.ContactNumber
            });
        }


        // CA1822: Mark as static as it does not access instance data
        private static string GenerateOtp(int length = 6)
        {
            Random random = new(); // IDE0090: 'new' expression can be simplified
            return new string(Enumerable.Repeat("0123456789", length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}