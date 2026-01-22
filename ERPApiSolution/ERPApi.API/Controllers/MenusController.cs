using ERPApi.API.Attributes;
using ERPApi.Core.DTOs;
using ERPApi.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERPApi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Policy = "RequireAdminRole")]
    [Authorize]
    public class MenusController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenusController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet]
        [RequirePermission("menus.view")]
        public async Task<ActionResult<BaseResponse<List<MenuDto>>>> GetMenus()
        {
            var response = await _menuService.GetMenusAsync();
            return Ok(response);
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<List<MenuDto>>>> GetUserMenus()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _menuService.GetUserMenusAsync(userId!);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [RequirePermission("menus.view")]
        public async Task<ActionResult<BaseResponse<MenuDto>>> GetMenu(int id)
        {
            var response = await _menuService.GetMenuByIdAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost]
        [RequirePermission("menus.manage")]
        public async Task<ActionResult<BaseResponse<MenuDto>>> CreateMenu([FromBody] MenuDto menuDto)
        {
            var response = await _menuService.CreateMenuAsync(menuDto);
            return Ok(response);
            //return CreatedAtAction(nameof(GetMenu), new { id = response.Data?.Id }, response);
        }

        [HttpPut("{id}")]
        [RequirePermission("menus.manage")]
        public async Task<ActionResult<BaseResponse<MenuDto>>> UpdateMenu(int id, [FromBody] MenuDto menuDto)
        {
            var response = await _menuService.UpdateMenuAsync(id, menuDto);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [RequirePermission("menus.manage")]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteMenu(int id)
        {
            var response = await _menuService.DeleteMenuAsync(id);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("role/{roleId}/assign")]
        [RequirePermission("menus.manage")]
        public async Task<ActionResult<BaseResponse<bool>>> AssignRoleMenus(string roleId, [FromBody] List<int> menuIds)
        {
            var response = await _menuService.AssignRoleMenusAsync(roleId, menuIds);
            return Ok(response);
        }
    }
}