using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OnlineCourses.Application.DTOs; // Assuming DTOs are here or I need to create them
using OnlineCourses.Infrastructure.Identity;

namespace OnlineCourses.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign default role
                await _userManager.AddToRoleAsync(user, "User");
                return Ok(new { message = "User registered successfully" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token and a refresh token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var token = GenerateJwtToken(user, roles);
                    var refreshToken = GenerateRefreshToken();

                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                    var updateResult = await _userManager.UpdateAsync(user);
                    
                    if (!updateResult.Succeeded)
                    {
                        return StatusCode(500, new { message = "Failed to update user", errors = updateResult.Errors });
                    }

                    return Ok(new AuthResponseDto
                    {
                        Token = token,
                        RefreshToken = refreshToken,
                        Email = user.Email!,
                        Roles = roles.ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
            }

            return Unauthorized(new { message = "Invalid login attempt" });
        }

        /// <summary>
        /// Refreshes the JWT token using a valid refresh token.
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenRequestDto model)
        {
            var principal = GetPrincipalFromExpiredToken(model.Token);
            if (principal == null) return BadRequest("Invalid token");

            var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var user = await _userManager.FindByEmailAsync(email!);

            if (user == null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return BadRequest("Invalid refresh token");

            var roles = await _userManager.GetRolesAsync(user);
            var newToken = GenerateJwtToken(user, roles);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return Ok(new AuthResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                Email = user.Email!,
                Roles = roles.ToList()
            });
        }

        private string GenerateJwtToken(AppUser user, IList<string> roles)
        {
            var jwtKey = _configuration["JWT_KEY"] ?? "SecretKeyForTestingOnly_PleaseChangeInEnvFile_AtLeast32CharsLong";
            var jwtIssuer = _configuration["JWT_ISSUER"] ?? "OnlineCourses";
            var jwtAudience = _configuration["JWT_AUDIENCE"] ?? "OnlineCourses";

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtKey = _configuration["JWT_KEY"] ?? "SecretKeyForTestingOnly_PleaseChangeInEnvFile_AtLeast32CharsLong";
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
