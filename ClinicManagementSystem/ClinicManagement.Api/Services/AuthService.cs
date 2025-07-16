// Location: ClinicManagement.ApiNew/Services/AuthService.cs
using ClinicManagement.Api.DTOs.Auth;
using ClinicManagement.Data.Context;
using ClinicManagement.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Api.Services
{
    // IDE0290: Use primary constructor - Not changing to primary constructor for broader compatibility
    public class AuthService
    {
        private readonly ClinicManagementDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<AuthService> _logger; // CS8618: _logger is now correctly assigned in constructor

        public AuthService(
            ClinicManagementDbContext context,
            IConfiguration configuration,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<Role> roleManager,
            ILogger<AuthService> logger
        )
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger; // Correctly assigned here
        }

        public async Task<IdentityResult> Register(RegisterUserDto registerDto)
        {
            registerDto.Username = registerDto.Username.Trim();
            registerDto.Email = registerDto.Email.Trim();
            registerDto.RoleName = registerDto.RoleName?.Trim();

            if (await _userManager.FindByNameAsync(registerDto.Username) != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Username already exists." });
            }
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Email already exists." });
            }

            string targetRoleName = string.IsNullOrEmpty(registerDto.RoleName) ? "Patient" : registerDto.RoleName;

            var roleExists = await _roleManager.RoleExistsAsync(targetRoleName);
            if (!roleExists)
            {
                // IDE0090: 'new' expression can be simplified (e.g., `new()`)
                var newRole = new Role { Name = targetRoleName, NormalizedName = targetRoleName.ToUpper(), CreatedAt = DateTime.UtcNow };
                var roleCreateResult = await _roleManager.CreateAsync(newRole);
                if (!roleCreateResult.Succeeded)
                {
                    _logger.LogError("Failed to create target role {RoleName} during registration: {Errors}",
                        targetRoleName, string.Join(", ", roleCreateResult.Errors.Select(e => e.Description)));
                    return IdentityResult.Failed(new IdentityError { Description = "Failed to create target role." });
                }
            }

            var newUser = new User
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false
            };

            var createResult = await _userManager.CreateAsync(newUser, registerDto.Password);

            if (createResult.Succeeded)
            {
                // IDE0305: Collection initialization can be simplified - This message is for the DTO not this part of code
                var addToRoleResult = await _userManager.AddToRoleAsync(newUser, targetRoleName);
                if (!addToRoleResult.Succeeded)
                {
                    _logger.LogError("User {Username} created, but failed to assign role {RoleName}: {Errors}",
                        newUser.UserName, targetRoleName, string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                    return IdentityResult.Failed(new IdentityError { Description = "User created but failed to assign role." });
                }
            }

            return createResult;
        }

        public async Task<string?> Login(LoginDto loginDto)
        {
            loginDto.Username = loginDto.Username.Trim();

            var user = await _userManager.FindByNameAsync(loginDto.Username);

            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Login attempt for non-existent or inactive user: {Username}", loginDto.Username);
                return null;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User {Username} account locked out.", loginDto.Username);
                    return null;
                }
                if (result.IsNotAllowed)
                {
                    _logger.LogWarning("User {Username} not allowed to sign in (e.g., email not confirmed).", loginDto.Username);
                    return null;
                }
                if (result.RequiresTwoFactor)
                {
                    _logger.LogInformation("User {Username} requires two-factor authentication.", loginDto.Username);
                    return null;
                }

                _logger.LogWarning("Login failed for user {Username}. Invalid credentials.", loginDto.Username);
                return null;
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            return GenerateJwtToken(user, userRoles.ToList());
        }

        private string GenerateJwtToken(User user, List<string> roles)
        {
            var jwtSecret = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expirationDays = Convert.ToDouble(_configuration["Jwt:DurationInDays"]);

            if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                throw new InvalidOperationException("JWT configuration is missing in appsettings.json.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // IDE0090: 'new' expression can be simplified
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // IDE0090: 'new' expression can be simplified
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expirationDays),
                signingCredentials: credentials);

            // IDE0090: 'new' expression can be simplified
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}