using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagementSystem.Models;

public class AuditLog
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    // Snapshot of username at time of action (in case user is deleted later)
    [MaxLength(256)]
    public string UserName { get; set; } = string.Empty;

    // e.g. "Created", "Updated", "Deleted", "Login", "Logout"
    [Required, MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    // e.g. "Patient", "Appointment", "Doctor"
    [Required, MaxLength(50)]
    public string EntityName { get; set; } = string.Empty;

    // The PK of the affected record
    [MaxLength(50)]
    public string? EntityId { get; set; }

    // Human-readable summary of what changed
    [MaxLength(2000)]
    public string? Details { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
