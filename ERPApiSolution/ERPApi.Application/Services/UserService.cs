using ERPApi.Core.DTOs;
using ERPApi.Core.Entities;
using ERPApi.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERPApi.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public UserService(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<BaseResponse<PaginatedResponse<UserDto>>> GetUsersAsync(int pageNumber, int pageSize, string search)
        {
            try
            {
                var query = _userManager.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u =>
                        u.FirstName.Contains(search) ||
                        u.LastName.Contains(search) ||
                        u.Email!.Contains(search));
                }

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var users = await query
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email!,
                        IsActive = u.IsActive,
                        CreatedDate = u.CreatedDate,
                        LastLoginDate = u.LastLoginDate,
                        Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                    })
                    .ToListAsync();

                var response = new PaginatedResponse<UserDto>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords,
                    Data = users
                };

                return new BaseResponse<PaginatedResponse<UserDto>>(response);
            }
            catch (Exception ex)
            {
                return new BaseResponse<PaginatedResponse<UserDto>>($"Error retrieving users: {ex.Message}");
            }
        }

        public async Task<BaseResponse<UserDto>> GetUserByIdAsync(string id)
        {
            try
            {
                var user = await _userManager.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return new BaseResponse<UserDto>("User not found");
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate,
                    Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
                };

                return new BaseResponse<UserDto>(userDto);
            }
            catch (Exception ex)
            {
                return new BaseResponse<UserDto>($"Error retrieving user: {ex.Message}");
            }
        }

        public async Task<BaseResponse<UserDto>> CreateUserAsync(UserDto userDto, string password)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
                if (existingUser != null)
                {
                    return new BaseResponse<UserDto>("Email already registered");
                }

                var user = new ApplicationUser
                {
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Email = userDto.Email,
                    UserName = userDto.Email,
                    IsActive = userDto.IsActive,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    return new BaseResponse<UserDto>(result.Errors.Select(e => e.Description).ToList());
                }

                // Assign roles
                if (userDto.Roles.Any())
                {
                    await _userManager.AddToRolesAsync(user, userDto.Roles);
                }

                // IMPORTANT: Save changes explicitly before audit log
                await _userManager.UpdateAsync(user); // This ensures user is saved

                // Now log audit - user should exist in database
                await _auditService.LogAsync(user.Id, "CREATE", "User", user.Id,
                    newValues: JsonSerializer.Serialize(userDto));

                var createdUserDto = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    Roles = userDto.Roles
                };

                return new BaseResponse<UserDto>(createdUserDto, "User created successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<UserDto>($"Error creating user: {ex.Message}");
            }
        }
        public async Task<BaseResponse<UserDto>> UpdateUserAsync(string id, UserDto userDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return new BaseResponse<UserDto>("User not found");
                }

                var oldValues = JsonSerializer.Serialize(new
                {
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.IsActive
                });

                user.FirstName = userDto.FirstName;
                user.LastName = userDto.LastName;
                user.IsActive = userDto.IsActive;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return new BaseResponse<UserDto>(result.Errors.Select(e => e.Description).ToList());
                }

                // Update email if changed
                if (user.Email != userDto.Email)
                {
                    var setEmailResult = await _userManager.SetEmailAsync(user, userDto.Email);
                    if (!setEmailResult.Succeeded)
                    {
                        return new BaseResponse<UserDto>(setEmailResult.Errors.Select(e => e.Description).ToList());
                    }
                }

                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (userDto.Roles.Any())
                {
                    await _userManager.AddToRolesAsync(user, userDto.Roles);
                }

                // Log audit
                var newValues = JsonSerializer.Serialize(userDto);
                await _auditService.LogAsync("SYSTEM", "UPDATE", "User", user.Id, oldValues, newValues);

                return new BaseResponse<UserDto>(userDto, "User updated successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<UserDto>($"Error updating user: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> DeleteUserAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return new BaseResponse<bool>("User not found");
                }

                // Don't allow deletion of system users
                if (user.Email!.Contains("admin"))
                {
                    return new BaseResponse<bool>("Cannot delete system user");
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return new BaseResponse<bool>(result.Errors.Select(e => e.Description).ToList());
                }

                // Log audit
                await _auditService.LogAsync("SYSTEM", "DELETE", "User", id);

                return new BaseResponse<bool>(true, "User deleted successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Error deleting user: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> AssignRolesAsync(string userId, List<string> roleIds)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new BaseResponse<bool>("User not found");
                }

                var roles = await _unitOfWork.Repository<ApplicationRole>()
                    .FindAsync(r => roleIds.Contains(r.Id));

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRolesAsync(user, roles.Select(r => r.Name));

                // Log audit
                await _auditService.LogAsync("SYSTEM", "ASSIGN_ROLES", "User", userId,
                    newValues: JsonSerializer.Serialize(roleIds));

                return new BaseResponse<bool>(true, "Roles assigned successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Error assigning roles: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> ToggleUserStatusAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new BaseResponse<bool>("User not found");
                }

                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);

                // Log audit
                await _auditService.LogAsync("SYSTEM", "TOGGLE_STATUS", "User", userId,
                    newValues: $"IsActive: {user.IsActive}");

                return new BaseResponse<bool>(true, $"User {(user.IsActive ? "activated" : "deactivated")} successfully");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>($"Error toggling user status: {ex.Message}");
            }
        }
    }
}