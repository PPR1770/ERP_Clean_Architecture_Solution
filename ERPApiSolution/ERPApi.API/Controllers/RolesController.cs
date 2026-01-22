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
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [RequirePermission("roles.view")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<RoleDto>>>> GetRoles(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = "")
        {
            var response = await _roleService.GetRolesAsync(pageNumber, pageSize, search);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [RequirePermission("roles.view")]
        public async Task<ActionResult<BaseResponse<RoleDto>>> GetRole(string id)
        {
            var response = await _roleService.GetRoleByIdAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost]
        [RequirePermission("roles.create")]
        public async Task<ActionResult<BaseResponse<RoleDto>>> CreateRole([FromBody] RoleDto roleDto)
        {
            var response = await _roleService.CreateRoleAsync(roleDto);
            return CreatedAtAction(nameof(GetRole), new { id = response.Data?.Id }, response);
        }

        [HttpPut("{id}")]
        [RequirePermission("roles.edit")]
        public async Task<ActionResult<BaseResponse<RoleDto>>> UpdateRole(string id, [FromBody] RoleDto roleDto)
        {
            var response = await _roleService.UpdateRoleAsync(id, roleDto);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [RequirePermission("roles.delete")]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteRole(string id)
        {
            var response = await _roleService.DeleteRoleAsync(id);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("{id}/assign-permissions")]
        [RequirePermission("roles.edit")]
        public async Task<ActionResult<BaseResponse<bool>>> AssignPermissions(string id, [FromBody] List<string> permissionCodes)
        {
            var response = await _roleService.AssignPermissionsAsync(id, permissionCodes);
            return Ok(response);
        }

        [HttpGet("{id}/permissions")]
        [RequirePermission("roles.view")]
        public async Task<ActionResult<BaseResponse<List<PermissionDto>>>> GetRolePermissions(string id)
        {
            var response = await _roleService.GetRolePermissionsAsync(id);
            return Ok(response);
        }
    }
}