using ERPApi.Core.DTOs;
using ERPApi.Core.Entities;
using ERPApi.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERPApi.Application.Services
{
    public class MenuService : IMenuService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public MenuService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<BaseResponse<List<MenuDto>>> GetMenusAsync()
        {
            try
            {
                var menuRepo = _unitOfWork.Repository<Menu>();
                var menus = await menuRepo.FindAsync(m => m.ParentId == null);

                var menuDtos = await BuildMenuTree(menus);
                return new BaseResponse<List<MenuDto>>(menuDtos);
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<MenuDto>>($"Error retrieving menus: {ex.Message}");
            }
        }

        public async Task<BaseResponse<List<MenuDto>>> GetUserMenusAsync(string userId)
        {
            try
            {
                var userRoleRepo = _unitOfWork.Repository<UserRole>();
                var userRoles = await userRoleRepo.FindAsync(ur => ur.UserId == userId);
                var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

                var roleMenuRepo = _unitOfWork.Repository<RoleMenu>();
                var roleMenus = await roleMenuRepo.FindAsync(rm => roleIds.Contains(rm.RoleId));
                var menuIds = roleMenus.Select(rm => rm.MenuId).Distinct().ToList();

                var menuRepo = _unitOfWork.Repository<Menu>();
                var menus = await menuRepo.FindAsync(m => menuIds.Contains(m.Id) && m.IsActive && m.ParentId == null);

                var menuDtos = await BuildMenuTree(menus, menuIds);
                return new BaseResponse<List<MenuDto>>(menuDtos);
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<MenuDto>>($"Error retrieving user menus: {ex.Message}");
            }
        }

        public async Task<BaseResponse<MenuDto>> GetMenuByIdAsync(int id)
        {
            try
            {
                var menuRepo = _unitOfWork.Repository<Menu>();
                var menu = await menuRepo.GetByIdAsync(id);

                if (menu == null)
                {
                    return new BaseResponse<MenuDto>("Menu not found");
                }

                var menuDto = new MenuDto
                {
                    Id = menu.Id,
                    Title = menu.Title,
                    Icon = menu.Icon,
                    Url = menu.Url,
                    ParentId = menu.ParentId,
                    Order = menu.Order,
                    IsActive = menu.IsActive
                };

                return new BaseResponse<MenuDto>(menuDto);
            }
            catch (Exception ex)
            {
                return new BaseResponse<MenuDto>($"Error retrieving menu: {ex.Message}");
            }
        }

        public async Task<BaseResponse<MenuDto>> CreateMenuAsync(MenuDto menuDto)
        {
            try
            {
                var menuRepo = _unitOfWork.Repository<Menu>();

                // Check if parent exists
                if (menuDto.ParentId.HasValue)
                {
                    var parent = await menuRepo.GetByIdAsync(menuDto.ParentId.Value);
                    if (parent == null)
                    {
                        return new BaseResponse<MenuDto>("Parent menu not found");
                    }
                }

                var menu = new Menu
                {
                    Title = menuDto.Title,
                    Icon = menuDto.Icon,
                    Url = menuDto.Url,
                    ParentId = menuDto.ParentId,
                    Order = menuDto.Order,
                    IsActive = menuDto.IsActive,
                    CreatedDate = DateTime.UtcNow
                };

                await menuRepo.AddAsync(menu);
                await _unitOfWork.SaveChangesAsync();

                // Log audit
                await _auditService.LogAsync("SYSTEM", "CREATE", "Menu", menu.Id.ToString(),
                    newValues: JsonSerializer.Serialize(menuDto));

                menuDto.Id = menu.Id;
                return new BaseResponse<MenuDto>(menuDto, "Menu created successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<MenuDto>($"Error creating menu: {ex.Message}");
            }
        }

        public async Task<BaseResponse<MenuDto>> UpdateMenuAsync(int id, MenuDto menuDto)
        {
            try
            {
                var menuRepo = _unitOfWork.Repository<Menu>();
                var menu = await menuRepo.GetByIdAsync(id);

                if (menu == null)
                {
                    return new BaseResponse<MenuDto>("Menu not found");
                }

                var oldValues = JsonSerializer.Serialize(new
                {
                    menu.Title,
                    menu.Icon,
                    menu.Url,
                    menu.ParentId,
                    menu.Order,
                    menu.IsActive
                });

                // Check if parent exists
                if (menuDto.ParentId.HasValue)
                {
                    var parent = await menuRepo.GetByIdAsync(menuDto.ParentId.Value);
                    if (parent == null)
                    {
                        return new BaseResponse<MenuDto>("Parent menu not found");
                    }
                }

                menu.Title = menuDto.Title;
                menu.Icon = menuDto.Icon;
                menu.Url = menuDto.Url;
                menu.ParentId = menuDto.ParentId;
                menu.Order = menuDto.Order;
                menu.IsActive = menuDto.IsActive;

                await menuRepo.UpdateAsync(menu);
                await _unitOfWork.SaveChangesAsync();

                // Log audit
                var newValues = JsonSerializer.Serialize(menuDto);
                await _auditService.LogAsync("SYSTEM", "UPDATE", "Menu", id.ToString(), oldValues, newValues);

                menuDto.Id = menu.Id;
                return new BaseResponse<MenuDto>(menuDto, "Menu updated successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<MenuDto>($"Error updating menu: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> DeleteMenuAsync(int id)
        {
            try
            {
                var menuRepo = _unitOfWork.Repository<Menu>();
                var menu = await menuRepo.GetByIdAsync(id);

                if (menu == null)
                {
                    return new BaseResponse<bool>("Menu not found");
                }

                // Check if menu has children
                var hasChildren = await menuRepo.ExistsAsync(m => m.ParentId == id);
                if (hasChildren)
                {
                    return new BaseResponse<bool>("Cannot delete menu with children");
                }

                // Check if menu is assigned to any role
                var roleMenuRepo = _unitOfWork.Repository<RoleMenu>();
                var isAssigned = await roleMenuRepo.ExistsAsync(rm => rm.MenuId == id);
                if (isAssigned)
                {
                    return new BaseResponse<bool>("Cannot delete menu assigned to roles");
                }

                await menuRepo.DeleteAsync(menu);
                await _unitOfWork.SaveChangesAsync();

                // Log audit
                await _auditService.LogAsync("SYSTEM", "DELETE", "Menu", id.ToString());

                return new BaseResponse<bool>(true, "Menu deleted successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Error deleting menu: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> AssignRoleMenusAsync(string roleId, List<int> menuIds)
        {
            try
            {
                var roleMenuRepo = _unitOfWork.Repository<RoleMenu>();

                // Remove existing menus
                var existingMenus = await roleMenuRepo.FindAsync(rm => rm.RoleId == roleId);
                foreach (var menu in existingMenus)
                {
                    await roleMenuRepo.DeleteAsync(menu);
                }

                // Add new menus
                foreach (var menuId in menuIds)
                {
                    var roleMenu = new RoleMenu
                    {
                        RoleId = roleId,
                        MenuId = menuId
                    };
                    await roleMenuRepo.AddAsync(roleMenu);
                }

                await _unitOfWork.SaveChangesAsync();

                // Log audit
                await _auditService.LogAsync("SYSTEM", "ASSIGN_MENUS", "Role", roleId,
                    newValues: JsonSerializer.Serialize(menuIds));

                return new BaseResponse<bool>(true, "Menus assigned successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Error assigning menus: {ex.Message}");
            }
        }

        private async Task<List<MenuDto>> BuildMenuTree(IEnumerable<Menu> menus, List<int>? allowedMenuIds = null)
        {
            var result = new List<MenuDto>();

            foreach (var menu in menus.OrderBy(m => m.Order))
            {
                var menuDto = new MenuDto
                {
                    Id = menu.Id,
                    Title = menu.Title,
                    Icon = menu.Icon,
                    Url = menu.Url,
                    ParentId = menu.ParentId,
                    Order = menu.Order,
                    IsActive = menu.IsActive
                };

                // Get children
                var menuRepo = _unitOfWork.Repository<Menu>();
                var children = await menuRepo.FindAsync(m => m.ParentId == menu.Id);

                if (children.Any())
                {
                    var childDtos = await BuildMenuTree(children, allowedMenuIds);
                    if (allowedMenuIds == null || childDtos.Any())
                    {
                        menuDto.Children = childDtos;
                    }
                }

                // Only add menu if it has children or is allowed
                if (allowedMenuIds == null || allowedMenuIds.Contains(menu.Id) || menuDto.Children.Any())
                {
                    result.Add(menuDto);
                }
            }

            return result.OrderBy(m => m.Order).ToList();
        }
    }
}