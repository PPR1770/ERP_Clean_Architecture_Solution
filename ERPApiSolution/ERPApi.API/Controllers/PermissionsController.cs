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
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        [RequirePermission("permissions.view")]
        public async Task<ActionResult<BaseResponse<List<PermissionDto>>>> GetPermissions()
        {
            var response = await _permissionService.GetAllPermissionsAsync();
            return Ok(response);
        }

        [HttpGet("groups")]
        [RequirePermission("permissions.view")]
        public async Task<ActionResult<BaseResponse<List<PermissionDto>>>> GetPermissionGroups()
        {
            var response = await _permissionService.GetPermissionGroupsAsync();
            return Ok(response);
        }

        [HttpPost]
        [RequirePermission("permissions.manage")]
        public async Task<ActionResult<BaseResponse<PermissionDto>>> CreatePermission([FromBody] PermissionDto permissionDto)
        {
            var response = await _permissionService.CreatePermissionAsync(permissionDto);
            return CreatedAtAction(nameof(CreatePermission), new { id = response.Data?.Id }, response);
        }

        [HttpPut("{id}")]
        [RequirePermission("permissions.manage")]
        public async Task<ActionResult<BaseResponse<PermissionDto>>> UpdatePermission(int id, [FromBody] PermissionDto permissionDto)
        {
            var response = await _permissionService.UpdatePermissionAsync(id, permissionDto);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [RequirePermission("permissions.manage")]
        public async Task<ActionResult<BaseResponse<bool>>> DeletePermission(int id)
        {
            var response = await _permissionService.DeletePermissionAsync(id);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}