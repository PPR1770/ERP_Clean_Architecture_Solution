using ERPApi.Core.DTOs;
using ERPApi.Core.Entities;
using ERPApi.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERPApi.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public RoleService(
            RoleManager<ApplicationRole> roleManager,
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<BaseResponse<PaginatedResponse<RoleDto>>> GetRolesAsync(int pageNumber, int pageSize, string search)
        {
            try
            {
                var query = _roleManager.Roles.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(r => r.Name.Contains(search) || r.Description.Contains(search));
                }

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var roles = await query
                    .OrderBy(r => r.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RoleDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description,
                        IsSystemRole = r.IsSystemRole,
                        CreatedDate = r.CreatedDate
                    })
                    .ToListAsync();

                // Get permissions for each role
                foreach (var role in roles)
                {
                    var permissions = await GetRolePermissionsAsync(role.Id);
                    role.Permissions = permissions.Data?.Select(p => p.Code).ToList() ?? new List<string>();
                }

                var response = new PaginatedResponse<RoleDto>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords,
                    Data = roles
                };

                return new BaseResponse<PaginatedResponse<RoleDto>>(response);
            }
            catch (Exception ex)
            {
                return new BaseResponse<PaginatedResponse<RoleDto>>($"Error retrieving roles: {ex.Message}");
            }
        }

        public async Task<BaseResponse<RoleDto>> GetRoleByIdAsync(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return new BaseResponse<RoleDto>("Role not found");
                }

                var roleDto = new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    IsSystemRole = role.IsSystemRole,
                    CreatedDate = role.CreatedDate
                };

                var permissions = await GetRolePermissionsAsync(id);
                roleDto.Permissions = permissions.Data?.Select(p => p.Code).ToList() ?? new List<string>();

                return new BaseResponse<RoleDto>(roleDto);
            }
            catch (Exception ex)
            {
                return new BaseResponse<RoleDto>($"Error retrieving role: {ex.Message}");
            }
        }

        public async Task<BaseResponse<RoleDto>> CreateRoleAsync(RoleDto roleDto)
        {
            try
            {
                var existingRole = await _roleManager.FindByNameAsync(roleDto.Name);
                if (existingRole != null)
                {
                    return new BaseResponse<RoleDto>("Role already exists");
                }

                var role = new ApplicationRole
                {
                    Name = roleDto.Name,
                    Description = roleDto.Description,
                    IsSystemRole = false,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    return new BaseResponse<RoleDto>(result.Errors.Select(e => e.Description).ToList());
                }

                // Assign permissions using permission IDs
                if (roleDto.Permissions.Any())
                {
                    // First, get permission IDs from codes
                    var permissionRepo = _unitOfWork.Repository<Permission>();
                    var permissions = await permissionRepo.FindAsync(p => roleDto.Permissions.Contains(p.Code));
                    var permissionIds = permissions.Select(p => p.Id).ToList();

                    if (permissionIds.Any())
                    {
                        await AssignPermissionsByIdAsync(role.Id, permissionIds);
                    }
                }

                // Log audit
                await _auditService.LogAsync("SYSTEM", "CREATE", "Role", role.Id,
                    newValues: JsonSerializer.Serialize(roleDto));

                var createdRoleDto = new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    IsSystemRole = role.IsSystemRole,
                    CreatedDate = role.CreatedDate,
                    Permissions = roleDto.Permissions
                };

                return new BaseResponse<RoleDto>(createdRoleDto, "Role created successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<RoleDto>($"Error creating role: {ex.Message}");
            }
        }

        public async Task<BaseResponse<RoleDto>> UpdateRoleAsync(string id, RoleDto roleDto)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return new BaseResponse<RoleDto>("Role not found");
                }

                if (role.IsSystemRole)
                {
                    return new BaseResponse<RoleDto>("Cannot modify system role");
                }

                var oldValues = JsonSerializer.Serialize(new
                {
                    role.Name,
                    role.Description
                });

                role.Name = roleDto.Name;
                role.Description = roleDto.Description;

                var result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    return new BaseResponse<RoleDto>(result.Errors.Select(e => e.Description).ToList());
                }

                // Update permissions using permission IDs
                var permissionRepo = _unitOfWork.Repository<Permission>();
                var permissions = await permissionRepo.FindAsync(p => roleDto.Permissions.Contains(p.Code));
                var permissionIds = permissions.Select(p => p.Id).ToList();

                if (permissionIds.Any())
                {
                    await AssignPermissionsByIdAsync(role.Id, permissionIds);
                }

                // Log audit
                var newValues = JsonSerializer.Serialize(roleDto);
                await _auditService.LogAsync("SYSTEM", "UPDATE", "Role", role.Id, oldValues, newValues);

                return new BaseResponse<RoleDto>(roleDto, "Role updated successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<RoleDto>($"Error updating role: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> DeleteRoleAsync(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return new BaseResponse<bool>("Role not found");
                }

                if (role.IsSystemRole)
                {
                    return new BaseResponse<bool>("Cannot delete system role");
                }

                // Check if role has users
                var userRoleRepo = _unitOfWork.Repository<UserRole>();
                var hasUsers = await userRoleRepo.ExistsAsync(ur => ur.RoleId == id);
                if (hasUsers)
                {
                    return new BaseResponse<bool>("Cannot delete role with assigned users");
                }

                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                {
                    return new BaseResponse<bool>(result.Errors.Select(e => e.Description).ToList());
                }

                // Log audit
                await _auditService.LogAsync("SYSTEM", "DELETE", "Role", id);

                return new BaseResponse<bool>(true, "Role deleted successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Error deleting role: {ex.Message}");
            }
        }

        public async Task<BaseResponse<List<PermissionDto>>> GetRolePermissionsAsync(string roleId)
        {
            try
            {
                var rolePermissionRepo = _unitOfWork.Repository<RolePermission>();
                var rolePermissions = await rolePermissionRepo.FindAsync(rp => rp.RoleId == roleId);
                var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();

                var permissionRepo = _unitOfWork.Repository<Permission>();
                var permissions = await permissionRepo.FindAsync(p => permissionIds.Contains(p.Id));

                var permissionDtos = permissions.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Group = p.Group,
                    Description = p.Description
                }).ToList();

                return new BaseResponse<List<PermissionDto>>(permissionDtos);
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<PermissionDto>>($"Error retrieving role permissions: {ex.Message}");
            }
        }

        // Method for assigning permissions by permission IDs (renamed to avoid conflict)
        public async Task<BaseResponse<bool>> AssignPermissionsByIdAsync(string roleId, List<int> permissionIds)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return new BaseResponse<bool>("Role not found");
                }

                var rolePermissionRepo = _unitOfWork.Repository<RolePermission>();

                // Remove existing permissions
                var existingPermissions = await rolePermissionRepo.FindAsync(rp => rp.RoleId == roleId);
                foreach (var perm in existingPermissions)
                {
                    await rolePermissionRepo.DeleteAsync(perm);
                }

                // Add new permissions
                foreach (var permissionId in permissionIds)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permissionId
                    };
                    await rolePermissionRepo.AddAsync(rolePermission);
                }

                await _unitOfWork.SaveChangesAsync();

                // Log audit
                await _auditService.LogAsync("SYSTEM", "ASSIGN_PERMISSIONS", "Role", roleId,
                    newValues: JsonSerializer.Serialize(permissionIds));

                return new BaseResponse<bool>(true, "Permissions assigned successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Error assigning permissions: {ex.Message}");
            }
        }

        // Method for assigning permissions by permission codes (if needed)
        public async Task<BaseResponse<bool>> AssignPermissionsByCodeAsync(string roleId, List<string> permissionCodes)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return new BaseResponse<bool>("Role not found");
                }

                var permissionRepo = _unitOfWork.Repository<Permission>();
                var permissions = await permissionRepo.FindAsync(p => permissionCodes.Contains(p.Code));
                var permissionIds = permissions.Select(p => p.Id).ToList();

                if (!permissionIds.Any())
                {
                    return new BaseResponse<bool>("No valid permissions found");
                }

                // Use the ID-based method
                return await AssignPermissionsByIdAsync(roleId, permissionIds);
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Error assigning permissions: {ex.Message}");
            }
        }

        //public async Task<BaseResponse<bool>> AssignPermissionsAsync(string roleId, List<string> permissionCodes)
        //{
        //    try
        //    {
        //        var role = await _roleManager.FindByIdAsync(roleId);
        //        if (role == null)
        //        {
        //            return new BaseResponse<bool>("Role not found");
        //        }

        //        var permissionRepo = _unitOfWork.Repository<Permission>();
        //        var permissions = await permissionRepo.FindAsync(p => permissionCodes.Contains(p.Code));
        //        var permissionIds = permissions.Select(p => p.Id).ToList();

        //        // Remove existing permissions
        //        var rolePermissionRepo = _unitOfWork.Repository<RolePermission>();
        //        var existingPermissions = await rolePermissionRepo.FindAsync(rp => rp.RoleId == roleId);
        //        foreach (var perm in existingPermissions)
        //        {
        //            await rolePermissionRepo.DeleteAsync(perm);
        //        }

        //        // Add new permissions
        //        foreach (var permissionId in permissionIds)
        //        {
        //            var rolePermission = new RolePermission
        //            {
        //                RoleId = roleId,
        //                PermissionId = permissionId
        //            };
        //            await rolePermissionRepo.AddAsync(rolePermission);
        //        }

        //        await _unitOfWork.SaveChangesAsync();

        //        // Log audit
        //        await _auditService.LogAsync("SYSTEM", "ASSIGN_PERMISSIONS", "Role", roleId,
        //            newValues: JsonSerializer.Serialize(permissionCodes));

        //        return new BaseResponse<bool>(true, "Permissions assigned successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        return new BaseResponse<bool>($"Error assigning permissions: {ex.Message}");
        //    }
        //}
        public async Task<BaseResponse<bool>> AssignPermissionsAsync(string roleId, List<int> permissionIds)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return new BaseResponse<bool>("Role not found");
                }

                var rolePermissionRepo = _unitOfWork.Repository<RolePermission>();

                // Remove existing permissions
                var existingPermissions = await rolePermissionRepo.FindAsync(rp => rp.RoleId == roleId);
                foreach (var perm in existingPermissions)
                {
                    await rolePermissionRepo.DeleteAsync(perm);
                }

                // Add new permissions
                foreach (var permissionId in permissionIds)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permissionId
                    };
                    await rolePermissionRepo.AddAsync(rolePermission);
                }

                await _unitOfWork.SaveChangesAsync();

                // Log audit
                await _auditService.LogAsync("SYSTEM", "ASSIGN_PERMISSIONS", "Role", roleId,
                    newValues: JsonSerializer.Serialize(permissionIds));

                return new BaseResponse<bool>(true, "Permissions assigned successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Error assigning permissions: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> AssignPermissionsAsync(string roleId, List<string> permissionCodes)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return new BaseResponse<bool>("Role not found");
                }

                var permissionRepo = _unitOfWork.Repository<Permission>();
                var permissions = await permissionRepo.FindAsync(p => permissionCodes.Contains(p.Code));
                var permissionIds = permissions.Select(p => p.Id).ToList();

                if (!permissionIds.Any())
                {
                    return new BaseResponse<bool>("No valid permissions found");
                }

                // Use the ID-based method
                return await AssignPermissionsByIdAsync(roleId, permissionIds);
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Error assigning permissions: {ex.Message}");
            }
        }

    }
}