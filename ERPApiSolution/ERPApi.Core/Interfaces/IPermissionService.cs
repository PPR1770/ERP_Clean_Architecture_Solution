using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ERPApi.Core.DTOs;

namespace ERPApi.Core.Interfaces
{
    public interface IPermissionService
    {
        Task<BaseResponse<List<PermissionDto>>> GetAllPermissionsAsync();
        Task<BaseResponse<List<PermissionDto>>> GetPermissionGroupsAsync();
        Task<BaseResponse<PermissionDto>> CreatePermissionAsync(PermissionDto permissionDto);
        Task<BaseResponse<PermissionDto>> UpdatePermissionAsync(int id, PermissionDto permissionDto);
        Task<BaseResponse<bool>> DeletePermissionAsync(int id);
    }
}