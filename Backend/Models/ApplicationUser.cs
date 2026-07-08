using Microsoft.AspNetCore.Identity;

namespace ClinicManagementSystem.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
