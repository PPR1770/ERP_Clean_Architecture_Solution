using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ERPApi.Core.DTOs;

namespace ERPApi.Core.Interfaces
{
    public interface IRoleService
    {
        Task<BaseResponse<PaginatedResponse<RoleDto>>> GetRolesAsync(int pageNumber, int pageSize, string search);
        Task<BaseResponse<RoleDto>> GetRoleByIdAsync(string id);
        Task<BaseResponse<RoleDto>> CreateRoleAsync(RoleDto roleDto);
        Task<BaseResponse<RoleDto>> UpdateRoleAsync(string id, RoleDto roleDto);
        Task<BaseResponse<bool>> DeleteRoleAsync(string id);
        Task<BaseResponse<bool>> AssignPermissionsAsync(string roleId, List<int> permissionIds);
        Task<BaseResponse<List<PermissionDto>>> GetRolePermissionsAsync(string roleId);
        Task<BaseResponse<bool>> AssignPermissionsAsync(string id, List<string> permissionCodes);
    }
}
