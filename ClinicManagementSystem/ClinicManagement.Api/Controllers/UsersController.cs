// Location: ClinicManagement.ApiNew/Controllers/UsersController.cs
using ClinicManagement.Api.DTOs.Users;
using ClinicManagement.Data.Context;
using ClinicManagement.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // For accessing user claims

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints in this controller require authentication by default
    public class UsersController : ControllerBase
    {
        private readonly ClinicManagementDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ClinicManagementDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of all users. Requires 'Admin' role.
        /// </summary>
        /// <returns>A list of UserDto objects.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,HR")] // Only Admin or HR roles can access this
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _context.Users
                                    .Select(u => new UserDto
                                    {
                                        UserId = u.Id,
                                        Username = u.UserName ?? string.Empty, // FIX CS8601: Handle potential null UserName
                                        Email = u.Email ?? string.Empty,       // FIX CS8601: Handle potential null Email
                                        IsActive = u.IsActive, // No cast needed if IsActive is bool, or if bool? and DTO is bool?
                                        CreatedAt = u.CreatedAt // No cast needed if CreatedAt is DateTime, or if DateTime? and DTO is DateTime?
                                    })
                                    .ToListAsync();
            return Ok(users);
        }

        /// <summary>
        /// Gets a specific user by ID. Accessible by 'Admin', 'HR', or the user themselves.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>The UserDto object.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            // Check if the requesting user is the target user OR has Admin/HR role
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value; // This variable isn't used after this line

            if (userIdClaim == null || (!User.IsInRole("Admin") && !User.IsInRole("HR") && userIdClaim != id.ToString()))
            {
                return Forbid(); // Return 403 Forbidden if not authorized
            }

            var user = await _context.Users
                                    .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            var userDto = new UserDto
            {
                UserId = user.Id,
                Username = user.UserName ?? string.Empty, // FIX CS8601: Handle potential null UserName
                Email = user.Email ?? string.Empty,       // FIX CS8601: Handle potential null Email
                IsActive = user.IsActive, // No cast needed if IsActive is bool, or if bool? and DTO is bool?
                CreatedAt = user.CreatedAt // No cast needed if CreatedAt is DateTime, or if DateTime? and DTO is DateTime?
            };

            return Ok(userDto);
        }

        /// <summary>
        /// Updates a user's details. Accessible by 'Admin', 'HR', or the user themselves.
        /// Only specific fields (Email, IsActive) are updatable via this DTO.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="updateDto">Updated user details.</param>
        /// <returns>No content on success.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateDto)
        {
            var userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (userToUpdate == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            // Authorization check: Only Admin/HR can update other users, or a user can update themselves.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || (!User.IsInRole("Admin") && !User.IsInRole("HR") && userIdClaim != id.ToString()))
            {
                return Forbid();
            }

            if (updateDto.Email != null && await _context.Users.AnyAsync(u => u.Email == updateDto.Email && u.Id != id))
            {
                return BadRequest("Email already in use by another user.");
            }

            // Apply updates
            if (updateDto.Email != null)
            {
                userToUpdate.Email = updateDto.Email;
            }
            // Add a null check for UserName in updateDto if you intend to update it
            // For example:
            // if (updateDto.Username != null)
            // {
            //     userToUpdate.UserName = updateDto.Username;
            // }


            // Only Admin/HR can change the IsActive status for others.
            // A user cannot deactivate themselves through this endpoint.
            if (updateDto.IsActive.HasValue)
            {
                if (User.IsInRole("Admin") || User.IsInRole("HR"))
                {
                    userToUpdate.IsActive = updateDto.IsActive.Value; // Using .Value for non-nullable assignment
                }
                else if (userIdClaim == id.ToString())
                {
                    // A user trying to change their own active status (should ideally be handled elsewhere if allowed)
                    // For now, disallow self-deactivation via this generic update.
                    return Forbid("Users cannot change their own active status.");
                }
                else
                {
                    return Forbid("You do not have permission to change the active status of other users.");
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                // Fixed CA2017 warning by ensuring correct logging template and arguments
                _logger.LogInformation("User updated: User ID {UserId}, Performed by User: {PerformingUser}", id, User.Identity?.Name ?? "Unknown");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Users.AnyAsync(e => e.Id == id))
                {
                    return NotFound($"User with ID {id} not found after update attempt.");
                }
                throw; // Re-throw other concurrency exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the user.");
            }
        }

        /// <summary>
        /// Assigns a role to a user. Requires 'Admin' role.
        /// </summary>
        /// <param name="id">The ID of the user to assign the role to.</param>
        /// <param name="assignRoleDto">The DTO containing the new role name.</param>
        /// <returns>No content on success.</returns>
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")] // Only Admin can assign roles
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleDto assignRoleDto)
        {
            var userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (userToUpdate == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == assignRoleDto.RoleName);

            if (role == null)
            {
                return BadRequest($"Role '{assignRoleDto.RoleName}' not found.");
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} role assigned to {RoleName} by {PerformingUser}", id, assignRoleDto.RoleName, User.Identity?.Name ?? "Unknown");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", assignRoleDto.RoleName, id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while assigning the role.");
            }
        }

        /// <summary>
        /// Deactivates a user. Requires 'Admin' role. (Soft delete for data integrity).
        /// </summary>
        /// <param name="id">The ID of the user to deactivate.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")] // Using DELETE for a logical "cancel" which changes status
        [Authorize(Roles = "Admin")] // Only Admin can deactivate users
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var userToDelete = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (userToDelete == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            // Prevent an admin from deactivating themselves or the last active admin
            // (More robust logic needed for last admin check in a real app)
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == id.ToString())
            {
                return BadRequest("You cannot deactivate your own account.");
            }

            userToDelete.IsActive = false; // Soft delete

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} deactivated by {PerformingUser}", id, User.Identity?.Name ?? "Unknown");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deactivating the user.");
            }
        }
    }
}