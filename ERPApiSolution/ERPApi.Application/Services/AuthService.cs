using ERPApi.Core.DTOs;
using ERPApi.Core.DTOs.Auth;
using ERPApi.Core.Entities;
using ERPApi.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ERPApi.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IEmailService emailService,
            IAuditService auditService,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _auditService = auditService;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return new BaseResponse<LoginResponse>("Invalid credentials");
                }

                if (!user.IsActive)
                {
                    return new BaseResponse<LoginResponse>("Account is disabled");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    return new BaseResponse<LoginResponse>("Invalid credentials");
                }

                // Update last login
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Get user roles and permissions
                var roles = await _userManager.GetRolesAsync(user);
                var permissions = await GetUserPermissionsAsync(user.Id);

                // Generate tokens
                var token = _tokenService.GenerateToken(user, roles.ToList(), permissions);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Save refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiryTime();
                await _userManager.UpdateAsync(user);

                // Log audit
                await _auditService.LogAsync(user.Id, "LOGIN", "User", user.Id, ipAddress: request.GetType().GetProperty("IpAddress")?.GetValue(request) as string);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate,
                    Roles = roles.ToList(),
                    Permissions = permissions
                };

                var response = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:TokenExpirationMinutes"])),
                    User = userDto
                };

                return new BaseResponse<LoginResponse>(response);
            }
            catch (Exception ex)
            {
                return new BaseResponse<LoginResponse>($"Login failed: {ex.Message}");
            }
        }

        public async Task<BaseResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(request.Token);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return new BaseResponse<LoginResponse>("Invalid token");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return new BaseResponse<LoginResponse>("Invalid refresh token");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var permissions = await GetUserPermissionsAsync(user.Id);

                var newToken = _tokenService.GenerateToken(user, roles.ToList(), permissions);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiryTime();
                await _userManager.UpdateAsync(user);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate,
                    Roles = roles.ToList(),
                    Permissions = permissions
                };

                var response = new LoginResponse
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:TokenExpirationMinutes"])),
                    User = userDto
                };

                return new BaseResponse<LoginResponse>(response);
            }
            catch (Exception ex)
            {
                return new BaseResponse<LoginResponse>($"Refresh token failed: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return new BaseResponse<bool>("Email already registered");
                }

                var user = new ApplicationUser
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    UserName = request.Email,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return new BaseResponse<bool>(result.Errors.Select(e => e.Description).ToList());
                }

                // Assign default role
                await _userManager.AddToRoleAsync(user, "User");

                // Send welcome email
                //await _emailService.SendWelcomeEmailAsync(user.Email!, $"{user.FirstName} {user.LastName}");

                // Log audit
                await _auditService.LogAsync(user.Id, "REGISTER", "User", user.Id);

                return new BaseResponse<bool>(true, "Registration successful");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Registration failed: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> LogoutAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryTime = null;
                    await _userManager.UpdateAsync(user);
                }

                // Log audit
                await _auditService.LogAsync(userId, "LOGOUT", "User", userId);

                return new BaseResponse<bool>(true, "Logout successful");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Logout failed: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new BaseResponse<bool>("User not found");
                }

                var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
                if (!result.Succeeded)
                {
                    return new BaseResponse<bool>(result.Errors.Select(e => e.Description).ToList());
                }

                // Log audit
                await _auditService.LogAsync(userId, "CHANGE_PASSWORD", "User", userId);

                return new BaseResponse<bool>(true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Change password failed: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    // Return success even if user not found for security
                    return new BaseResponse<bool>(true, "If your email exists, you will receive a password reset link");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var resetLink = $"{_configuration["AppUrl"]}/reset-password?email={email}&token={encodedToken}";

                await _emailService.SendPasswordResetEmailAsync(email, resetLink);

                // Log audit
                await _auditService.LogAsync(user.Id, "FORGOT_PASSWORD", "User", user.Id);

                return new BaseResponse<bool>(true, "Password reset email sent");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Forgot password failed: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    return new BaseResponse<bool>("Invalid request");
                }

                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
                if (!result.Succeeded)
                {
                    return new BaseResponse<bool>(result.Errors.Select(e => e.Description).ToList());
                }

                // Log audit
                await _auditService.LogAsync(user.Id, "RESET_PASSWORD", "User", user.Id);

                return new BaseResponse<bool>(true, "Password reset successful");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Reset password failed: {ex.Message}");
            }
        }

        private async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            var user = await _userManager.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return new List<string>();

            var permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToList();

            return permissions;
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}