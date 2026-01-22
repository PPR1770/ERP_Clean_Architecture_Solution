using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ERPApi.Core.DTOs;

namespace ERPApi.Core.Interfaces
{
    public interface IUserService
    {
        Task<BaseResponse<PaginatedResponse<UserDto>>> GetUsersAsync(int pageNumber, int pageSize, string search);
        Task<BaseResponse<UserDto>> GetUserByIdAsync(string id);
        Task<BaseResponse<UserDto>> CreateUserAsync(UserDto userDto, string password);
        Task<BaseResponse<UserDto>> UpdateUserAsync(string id, UserDto userDto);
        Task<BaseResponse<bool>> DeleteUserAsync(string id);
        Task<BaseResponse<bool>> AssignRolesAsync(string userId, List<string> roleIds);
        Task<BaseResponse<bool>> ToggleUserStatusAsync(string userId);
    }
}
