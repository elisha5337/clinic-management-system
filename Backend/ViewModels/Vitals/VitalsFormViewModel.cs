using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.ViewModels.Vitals;

public class VitalsFormViewModel
{
    public int AppointmentId { get; set; }

    // Patient and appointment info for display
    public string PatientName      { get; set; } = string.Empty;
    public string DoctorName       { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }

    [MaxLength(20)]
    [Display(Name = "Blood Pressure")]
    public string? BloodPressure { get; set; }

    [Range(30, 250, ErrorMessage = "Heart rate must be between 30 and 250 bpm")]
    [Display(Name = "Heart Rate (bpm)")]
    public int? HeartRate { get; set; }

    [Range(30.0, 45.0, ErrorMessage = "Temperature must be between 30°C and 45°C")]
    [Display(Name = "Temperature (°C)")]
    public decimal? Temperature { get; set; }

    [Range(1.0, 500.0, ErrorMessage = "Please enter a valid weight")]
    [Display(Name = "Weight (kg)")]
    public decimal? Weight { get; set; }

    [Range(30.0, 250.0, ErrorMessage = "Please enter a valid height")]
    [Display(Name = "Height (cm)")]
    public decimal? Height { get; set; }

    [MaxLength(1000)]
    [Display(Name = "Nurse Notes")]
    public string? Notes { get; set; }

    // Pre-examination notes saved directly on the Appointment record
    [MaxLength(1000)]
    [Display(Name = "Pre-Examination Notes")]
    public string? PreExamNotes { get; set; }
}
