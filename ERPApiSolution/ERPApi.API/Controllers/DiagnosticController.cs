using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ERPApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DiagnosticController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("check-config")]
        public IActionResult CheckConfig()
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            return Ok(new
            {
                JwtKey = string.IsNullOrEmpty(jwtKey) ? "MISSING" : $"Present ({jwtKey.Length} chars)",
                JwtKeyFirst10 = jwtKey?.Substring(0, Math.Min(10, jwtKey.Length)),
                JwtIssuer = jwtIssuer ?? "MISSING",
                JwtAudience = jwtAudience ?? "MISSING",
                TokenExpiration = _configuration["Jwt:TokenExpirationMinutes"]
            });
        }

        [HttpGet("generate-test-jwt")]
        public IActionResult GenerateTestJwt()
        {
            var key = System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "test-user-id"),
                new Claim(JwtRegisteredClaimNames.Email, "test@example.com"),
                new Claim(JwtRegisteredClaimNames.Name, "Test User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("permissions", "ViewDashboard"),
                new Claim("permissions", "ManageUsers")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Decode it to show what's inside
            var handler = new JwtSecurityTokenHandler();
            var decoded = handler.ReadJwtToken(tokenString);

            return Ok(new
            {
                Token = tokenString,
                DecodedHeader = decoded.Header,
                DecodedPayload = decoded.Payload,
                AllClaims = decoded.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }

        [HttpGet("check-auth-header")]
        public IActionResult CheckAuthHeader()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            return Ok(new
            {
                HasAuthHeader = !string.IsNullOrEmpty(authHeader),
                AuthHeader = authHeader,
                AuthHeaderStartsWithBearer = authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ?? false,
                Headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            });
        }

        [Authorize]
        [HttpGet("test-simple-auth")]
        public IActionResult TestSimpleAuth()
        {
            return Ok(new
            {
                Message = "Authentication successful!",
                UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                UserName = User.Identity?.Name,
                IsAuthenticated = User.Identity?.IsAuthenticated,
                AuthenticationType = User.Identity?.AuthenticationType,
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }

        [HttpPost("validate-this-token")]
        public IActionResult ValidateThisToken()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return BadRequest(new { Error = "No Bearer token found" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var handler = new JwtSecurityTokenHandler();

                // Use EXACT same validation as your app
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };

                var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
                var jwtToken = handler.ReadJwtToken(token);

                return Ok(new
                {
                    Success = true,
                    Message = "Token is VALID",
                    TokenExpiresAt = validatedToken.ValidTo,
                    CurrentTime = DateTime.UtcNow,
                    IsExpired = validatedToken.ValidTo < DateTime.UtcNow,
                    Claims = principal.Claims.Select(c => new { c.Type, c.Value }),
                    TokenDetails = new
                    {
                        Algorithm = jwtToken.Header.Alg,
                        Type = jwtToken.Header.Typ,
                        Issuer = jwtToken.Issuer,
                        Audience = jwtToken.Audiences,
                        Expiration = jwtToken.ValidTo
                    }
                });
            }
            catch (SecurityTokenExpiredException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Error = "Token EXPIRED",
                    Message = ex.Message,
                    CurrentTime = DateTime.UtcNow
                });
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Error = "Invalid SIGNATURE",
                    Message = ex.Message,
                    KeyLength = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!).Length * 8 + " bits"
                });
            }
            catch (SecurityTokenInvalidIssuerException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Error = "Invalid ISSUER",
                    Message = ex.Message,
                    ExpectedIssuer = _configuration["Jwt:Issuer"]
                });
            }
            catch (SecurityTokenInvalidAudienceException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Error = "Invalid AUDIENCE",
                    Message = ex.Message,
                    ExpectedAudience = _configuration["Jwt:Audience"]
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Error = "Validation FAILED",
                    Message = ex.Message,
                    ExceptionType = ex.GetType().Name
                });
            }
        }
    }
}