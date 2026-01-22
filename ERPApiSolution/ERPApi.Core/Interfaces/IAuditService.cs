using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ERPApi.Core.DTOs;

namespace ERPApi.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string userId, string action, string entityName, string entityId, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null);
        Task<BaseResponse<PaginatedResponse<AuditLogDto>>> GetAuditLogsAsync(int pageNumber, int pageSize, DateTime? fromDate = null, DateTime? toDate = null, string? userId = null);
    }
}
