using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagementSystem.Models;

public class Doctor
{
    public int Id { get; set; }

    // Link to the login account for this doctor
    public string? ApplicationUserId { get; set; }

    [ForeignKey(nameof(ApplicationUserId))]
    public ApplicationUser? ApplicationUser { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Specialization { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(100), EmailAddress]
    public string? Email { get; set; }

    public bool IsAvailable { get; set; } = true;

    // Navigation
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
