using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.ViewModels.Doctors;

public class DoctorFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Specialization is required")]
    [MaxLength(100)]
    public string Specialization { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [MaxLength(20)]
    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(100)]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    [Display(Name = "Available for Appointments")]
    public bool IsAvailable { get; set; } = true;

    // ── Fields for creating a linked login account (Create only) ──
    [Display(Name = "Login Email")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? AccountEmail { get; set; }

    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [DataType(DataType.Password)]
    [Display(Name = "Login Password")]
    public string? AccountPassword { get; set; }

    // Common specializations for the datalist
    public static readonly string[] Specializations =
    [
        "General Practice", "Pediatrics", "Cardiology", "Dermatology",
        "Neurology", "Orthopedics", "Gynecology", "Ophthalmology",
        "Psychiatry", "Radiology", "Surgery", "Urology"
    ];
}
