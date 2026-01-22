using ERPApi.Core.DTOs;
using ERPApi.Core.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPApi.Core.Interfaces
{
    public interface IAuthService
    {
        Task<BaseResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<BaseResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<BaseResponse<bool>> RegisterAsync(RegisterRequest request);
        Task<BaseResponse<bool>> LogoutAsync(string userId);
        Task<BaseResponse<bool>> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
        Task<BaseResponse<bool>> ForgotPasswordAsync(string email);
        Task<BaseResponse<bool>> ResetPasswordAsync(string email, string token, string newPassword);
    }
}
