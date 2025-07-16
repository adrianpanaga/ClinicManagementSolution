using ClinicManagement.Api.DTOs.ClinicSettings;
using ClinicManagement.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")] // Route: /api/ClinicSettings
    [ApiController]
    [Authorize(Roles = "Admin,HR,Doctor")] // AUTHORIZATION: Only these roles can access/update
    public class ClinicSettingsController : ControllerBase
    {
        private readonly ClinicManagementDbContext _context;

        public ClinicSettingsController(ClinicManagementDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the current clinic operating hours and lunch break settings.
        /// </summary>
        /// <param name="id">The ID of the clinic settings record (usually 1, since it's a single record).</param>
        /// <returns>The clinic settings data.</returns>
        [HttpGet("{id}")] // Route: GET /api/ClinicSettings/{id}
        public async Task<ActionResult<GetClinicSettingsDto>> GetClinicSettings(int id)
        {
            var settings = await _context.ClinicSettings.FindAsync(id);

            if (settings == null)
            {
                return NotFound("Clinic settings not found. Please ensure default settings are seeded.");
            }

            // Map to DTO
            var dto = new GetClinicSettingsDto
            {
                Id = settings.Id,
                OpenTime = settings.OpenTime.ToString("HH:mm"),
                CloseTime = settings.CloseTime.ToString("HH:mm"),
                LunchStartTime = settings.LunchStartTime.ToString("HH:mm"),
                LunchEndTime = settings.LunchEndTime.ToString("HH:mm")
            };

            return Ok(dto);
        }

        /// <summary>
        /// Updates the clinic operating hours and lunch break settings.
        /// </summary>
        /// <param name="id">The ID of the clinic settings record (usually 1).</param>
        /// <param name="updateDto">The updated settings data.</param>
        /// <returns>No Content if successful.</returns>
        [HttpPut("{id}")] // Route: PUT /api/ClinicSettings/{id}
        public async Task<IActionResult> UpdateClinicSettings(int id, UpdateClinicSettingsDto updateDto)
        {
            var settings = await _context.ClinicSettings.FindAsync(id);

            if (settings == null)
            {
                return NotFound("Clinic settings not found. Cannot update.");
            }

            // Parse TimeOnly from string DTO
            if (!TimeOnly.TryParse(updateDto.OpenTime, out var openTime) ||
                !TimeOnly.TryParse(updateDto.CloseTime, out var closeTime) ||
                !TimeOnly.TryParse(updateDto.LunchStartTime, out var lunchStartTime) ||
                !TimeOnly.TryParse(updateDto.LunchEndTime, out var lunchEndTime))
            {
                return BadRequest("Invalid time format. Please use HH:mm.");
            }

            settings.OpenTime = openTime;
            settings.CloseTime = closeTime;
            settings.LunchStartTime = lunchStartTime;
            settings.LunchEndTime = lunchEndTime;
            settings.UpdatedAt = DateTime.UtcNow; // Update timestamp

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ClinicSettings.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw; // Re-throw if it's a different concurrency issue
                }
            }

            return NoContent(); // 204 No Content
        }
    }
}
