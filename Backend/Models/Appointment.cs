using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagementSystem.Models;

public enum AppointmentStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled
}

public class Appointment
{
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = null!;

    [Required]
    public int DoctorId { get; set; }

    [ForeignKey(nameof(DoctorId))]
    public Doctor Doctor { get; set; } = null!;

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required, MaxLength(500)]
    public string ReasonForVisit { get; set; } = string.Empty;

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    // Added by Nurse during pre-examination
    [MaxLength(1000)]
    public string? NurseNotes { get; set; }

    // Added by Doctor after examination
    [MaxLength(2000)]
    public string? Diagnosis { get; set; }

    [MaxLength(2000)]
    public string? Prescription { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public ApplicationUser? CreatedBy { get; set; }

    // Navigation — one appointment can have one vitals record
    public Vitals? Vitals { get; set; }
}
