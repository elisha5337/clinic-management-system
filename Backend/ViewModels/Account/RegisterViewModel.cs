using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.ViewModels.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a role")]
    public string Role { get; set; } = string.Empty;

    // Populated in the view from the available roles
    public List<string> AvailableRoles { get; set; } = ["Receptionist", "Nurse", "Doctor"];
}
