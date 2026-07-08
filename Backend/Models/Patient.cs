using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagementSystem.Models;

public enum Gender { Male, Female, Other }

public class Patient
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public Gender Gender { get; set; }

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(100), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(5)]
    public string? BloodType { get; set; }

    [MaxLength(100)]
    public string? EmergencyContactName { get; set; }

    [MaxLength(20)]
    public string? EmergencyContactPhone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // FK to the user who registered this patient
    public string? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public ApplicationUser? CreatedBy { get; set; }

    // Navigation
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    // Computed helper
    [NotMapped]
    public int Age => DateTime.Today.Year - DateOfBirth.Year -
                      (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
}
