using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.Services;

public interface IAuditService
{
    Task LogAsync(string userId, string userName, string action,
                  string entityName, string? entityId = null, string? details = null);
}

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string userId, string userName, string action,
                               string entityName, string? entityId = null, string? details = null)
    {
        var log = new AuditLog
        {
            UserId     = userId,
            UserName   = userName,
            Action     = action,
            EntityName = entityName,
            EntityId   = entityId,
            Details    = details,
            Timestamp  = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
