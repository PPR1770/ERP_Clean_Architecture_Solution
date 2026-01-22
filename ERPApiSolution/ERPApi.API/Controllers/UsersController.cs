using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERPApi.Core.DTOs;
using ERPApi.Core.Interfaces;
using ERPApi.API.Attributes;

namespace ERPApi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "RequireAdminRole")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [RequirePermission("users.view")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<UserDto>>>> GetUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = "")
        {
            var response = await _userService.GetUsersAsync(pageNumber, pageSize, search);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [RequirePermission("users.view")]
        public async Task<ActionResult<BaseResponse<UserDto>>> GetUser(string id)
        {
            var response = await _userService.GetUserByIdAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost]
        [RequirePermission("users.create")]
        public async Task<ActionResult<BaseResponse<UserDto>>> CreateUser([FromBody] CreateUserRequest request)
        {
            var userDto = new UserDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                IsActive = request.IsActive,
                Roles = request.Roles
            };

            var response = await _userService.CreateUserAsync(userDto, request.Password);

            // Check if response is successful and has data
            if (!response.Success || response.Data == null)
            {
                return BadRequest(response);
            }

            // Ensure ID is not null
            if (string.IsNullOrEmpty(response.Data.Id))
            {
                return StatusCode(500, new BaseResponse<UserDto>("User created but ID is null"));
            }

            // Return CreatedAtAction with proper route values
            return CreatedAtAction(
                nameof(GetUser),
                new { id = response.Data.Id },  // Remove null-conditional, we checked it above
                response
            );
        }

        [HttpPut("{id}")]
        [RequirePermission("users.edit")]
        public async Task<ActionResult<BaseResponse<UserDto>>> UpdateUser(string id, [FromBody] UserDto userDto)
        {
            var response = await _userService.UpdateUserAsync(id, userDto);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [RequirePermission("users.delete")]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteUser(string id)
        {
            var response = await _userService.DeleteUserAsync(id);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("{id}/assign-roles")]
        [RequirePermission("users.edit")]
        public async Task<ActionResult<BaseResponse<bool>>> AssignRoles(string id, [FromBody] List<string> roleIds)
        {
            var response = await _userService.AssignRolesAsync(id, roleIds);
            return Ok(response);
        }

        [HttpPost("{id}/toggle-status")]
        [RequirePermission("users.edit")]
        public async Task<ActionResult<BaseResponse<bool>>> ToggleUserStatus(string id)
        {
            var response = await _userService.ToggleUserStatusAsync(id);
            return Ok(response);
        }
    }

    public class CreateUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}