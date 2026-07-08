using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.ViewModels.Patients;

public class PatientFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-30);

    [Required(ErrorMessage = "Gender is required")]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [MaxLength(20)]
    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(100)]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(5)]
    [Display(Name = "Blood Type")]
    public string? BloodType { get; set; }

    [MaxLength(100)]
    [Display(Name = "Emergency Contact Name")]
    public string? EmergencyContactName { get; set; }

    [MaxLength(20)]
    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Emergency Contact Phone")]
    public string? EmergencyContactPhone { get; set; }

    // Dropdown options
    public IEnumerable<SelectListItem> GenderOptions { get; set; } = Enum
        .GetValues<Gender>()
        .Select(g => new SelectListItem(g.ToString(), g.ToString()));

    public IEnumerable<SelectListItem> BloodTypeOptions { get; set; } =
        new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" }
        .Select(b => new SelectListItem(b, b));
}
