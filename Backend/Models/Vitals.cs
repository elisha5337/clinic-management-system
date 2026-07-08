using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagementSystem.Models;

public class Vitals
{
    public int Id { get; set; }

    [Required]
    public int AppointmentId { get; set; }

    [ForeignKey(nameof(AppointmentId))]
    public Appointment Appointment { get; set; } = null!;

    // e.g. "120/80"
    [MaxLength(20)]
    public string? BloodPressure { get; set; }

    // beats per minute
    public int? HeartRate { get; set; }

    // Celsius
    [Column(TypeName = "decimal(4,1)")]
    public decimal? Temperature { get; set; }

    // kg
    [Column(TypeName = "decimal(5,1)")]
    public decimal? Weight { get; set; }

    // cm
    [Column(TypeName = "decimal(5,1)")]
    public decimal? Height { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public string? RecordedById { get; set; }

    [ForeignKey(nameof(RecordedById))]
    public ApplicationUser? RecordedBy { get; set; }
}
