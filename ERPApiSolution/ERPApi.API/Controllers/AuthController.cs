using ERPApi.Core.DTOs;
using ERPApi.Core.DTOs.Auth;
using ERPApi.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERPApi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAuditService _auditService;

        public AuthController(IAuthService authService, IAuditService auditService)
        {
            _authService = authService;
            _auditService = auditService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<BaseResponse<LoginResponse>>> Login(Core.DTOs.Auth.LoginRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            // Add IP and UserAgent to request if needed in service
            var response = await _authService.LoginAsync(request);

            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<BaseResponse<LoginResponse>>> RefreshToken(RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request);
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<ActionResult<BaseResponse<bool>>> Register(Core.DTOs.Auth.RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<bool>>> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _authService.LogoutAsync(userId!);
            return Ok(response);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _authService.ChangePasswordAsync(userId!, request.OldPassword, request.NewPassword);
            return Ok(response);
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<BaseResponse<bool>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var response = await _authService.ForgotPasswordAsync(request.Email);
            return Ok(response);
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<BaseResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var response = await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
            return Ok(response);
        }
    }
}