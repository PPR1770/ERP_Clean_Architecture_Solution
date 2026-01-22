using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ERPApi.Core.DTOs;

namespace ERPApi.Core.Interfaces
{
    public interface IMenuService
    {
        Task<BaseResponse<List<MenuDto>>> GetMenusAsync();
        Task<BaseResponse<List<MenuDto>>> GetUserMenusAsync(string userId);
        Task<BaseResponse<MenuDto>> GetMenuByIdAsync(int id);
        Task<BaseResponse<MenuDto>> CreateMenuAsync(MenuDto menuDto);
        Task<BaseResponse<MenuDto>> UpdateMenuAsync(int id, MenuDto menuDto);
        Task<BaseResponse<bool>> DeleteMenuAsync(int id);
        Task<BaseResponse<bool>> AssignRoleMenusAsync(string roleId, List<int> menuIds);
    }
}
