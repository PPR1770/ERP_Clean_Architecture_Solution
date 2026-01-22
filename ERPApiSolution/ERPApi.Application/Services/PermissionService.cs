using ERPApi.Core.DTOs;
using ERPApi.Core.Entities;
using ERPApi.Core.Interfaces;
using System.Text.Json;

namespace ERPApi.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public PermissionService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<BaseResponse<List<PermissionDto>>> GetAllPermissionsAsync()
        {
            try
            {
                var permissionRepo = _unitOfWork.Repository<Permission>();
                var permissions = await permissionRepo.GetAllAsync();

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
                return new BaseResponse<List<PermissionDto>>($"Error retrieving permissions: {ex.Message}");
            }
        }

        public async Task<BaseResponse<List<PermissionDto>>> GetPermissionGroupsAsync()
        {
            try
            {
                var permissionRepo = _unitOfWork.Repository<Permission>();
                var permissions = await permissionRepo.GetAllAsync();

                var groupedPermissions = permissions
                    .GroupBy(p => p.Group)
                    .Select(g => new PermissionDto
                    {
                        Group = g.Key,
                        Name = $"{g.Key} Permissions"
                    })
                    .ToList();

                return new BaseResponse<List<PermissionDto>>(groupedPermissions);
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<PermissionDto>>($"Error retrieving permission groups: {ex.Message}");
            }
        }

        public async Task<BaseResponse<PermissionDto>> CreatePermissionAsync(PermissionDto permissionDto)
        {
            try
            {
                var permissionRepo = _unitOfWork.Repository<Permission>();

                // Check if code already exists
                var existingPermission = await permissionRepo.FindAsync(p => p.Code == permissionDto.Code);
                if (existingPermission.Any())
                {
                    return new BaseResponse<PermissionDto>("Permission code already exists");
                }

                var permission = new Permission
                {
                    Name = permissionDto.Name,
                    Code = permissionDto.Code,
                    Group = permissionDto.Group,
                    Description = permissionDto.Description,
                    CreatedDate = DateTime.UtcNow
                };

                await permissionRepo.AddAsync(permission);
                await _unitOfWork.SaveChangesAsync();

                // Log audit
                await _auditService.LogAsync("SYSTEM", "CREATE", "Permission", permission.Id.ToString(),
                    newValues: JsonSerializer.Serialize(permissionDto));

                permissionDto.Id = permission.Id;
                return new BaseResponse<PermissionDto>(permissionDto, "Permission created successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<PermissionDto>($"Error creating permission: {ex.Message}");
            }
        }

        public async Task<BaseResponse<PermissionDto>> UpdatePermissionAsync(int id, PermissionDto permissionDto)
        {
            try
            {
                var permissionRepo = _unitOfWork.Repository<Permission>();
                var permission = await permissionRepo.GetByIdAsync(id);

                if (permission == null)
                {
                    return new BaseResponse<PermissionDto>("Permission not found");
                }

                var oldValues = JsonSerializer.Serialize(new
                {
                    permission.Name,
                    permission.Code,
                    permission.Group,
                    permission.Description
                });

                // Check if code already exists (excluding current permission)
                var existingPermission = await permissionRepo.FindAsync(p => p.Code == permissionDto.Code && p.Id != id);
                if (existingPermission.Any())
                {
                    return new BaseResponse<PermissionDto>("Permission code already exists");
                }

                permission.Name = permissionDto.Name;
                permission.Code = permissionDto.Code;
                permission.Group = permissionDto.Group;
                permission.Description = permissionDto.Description;

                await permissionRepo.UpdateAsync(permission);
                await _unitOfWork.SaveChangesAsync();

                // Log audit
                var newValues = JsonSerializer.Serialize(permissionDto);
                await _auditService.LogAsync("SYSTEM", "UPDATE", "Permission", id.ToString(), oldValues, newValues);

                permissionDto.Id = permission.Id;
                return new BaseResponse<PermissionDto>(permissionDto, "Permission updated successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<PermissionDto>($"Error updating permission: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> DeletePermissionAsync(int id)
        {
            try
            {
                var permissionRepo = _unitOfWork.Repository<Permission>();
                var permission = await permissionRepo.GetByIdAsync(id);

                if (permission == null)
                {
                    return new BaseResponse<bool>("Permission not found");
                }

                // Check if permission is assigned to any role
                var rolePermissionRepo = _unitOfWork.Repository<RolePermission>();
                var isAssigned = await rolePermissionRepo.ExistsAsync(rp => rp.PermissionId == id);
                if (isAssigned)
                {
                    return new BaseResponse<bool>("Cannot delete permission assigned to roles");
                }

                await permissionRepo.DeleteAsync(permission);
                await _unitOfWork.SaveChangesAsync();

                // Log audit
                await _auditService.LogAsync("SYSTEM", "DELETE", "Permission", id.ToString());

                return new BaseResponse<bool>(true, "Permission deleted successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Error deleting permission: {ex.Message}");
            }
        }
    }
}