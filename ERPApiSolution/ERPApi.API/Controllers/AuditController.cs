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
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        [RequirePermission("audit.view")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<AuditLogDto>>>> GetAuditLogs(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? userId = null)
        {
            var response = await _auditService.GetAuditLogsAsync(pageNumber, pageSize, fromDate, toDate, userId);
            return Ok(response);
        }
    }
}