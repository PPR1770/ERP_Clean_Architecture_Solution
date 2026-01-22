using ERPApi.Core.Entities;
using ERPApi.Core.DTOs;
using ERPApi.Core.Interfaces;
using ERPApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPApi.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string userId, string action, string entityName, string entityId,
            string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValues = oldValues ?? string.Empty,
                NewValues = newValues ?? string.Empty,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<BaseResponse<PaginatedResponse<AuditLogDto>>> GetAuditLogsAsync(int pageNumber,
            int pageSize, DateTime? fromDate = null, DateTime? toDate = null, string? userId = null)
        {
            try
            {
                var query = _context.AuditLogs
                    .Include(a => a.User)
                    .AsQueryable();

                if (fromDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp <= toDate.Value);
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(a => a.UserId == userId);
                }

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var logs = await query
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new AuditLogDto
                    {
                        Id = a.Id,
                        UserName = a.User.Email!,
                        Action = a.Action,
                        EntityName = a.EntityName,
                        EntityId = a.EntityId,
                        OldValues = a.OldValues,
                        NewValues = a.NewValues,
                        IpAddress = a.IpAddress,
                        UserAgent = a.UserAgent,
                        Timestamp = a.Timestamp
                    })
                    .ToListAsync();

                var response = new PaginatedResponse<AuditLogDto>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords,
                    Data = logs
                };

                return new BaseResponse<PaginatedResponse<AuditLogDto>>(response);
            }
            catch (Exception ex)
            {
                return new BaseResponse<PaginatedResponse<AuditLogDto>>($"Error retrieving audit logs: {ex.Message}");
            }
        }
    }
}