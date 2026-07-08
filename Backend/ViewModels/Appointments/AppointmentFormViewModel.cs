using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.ViewModels.Appointments;

public class AppointmentFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a patient")]
    [Display(Name = "Patient")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Please select a doctor")]
    [Display(Name = "Doctor")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Appointment date and time is required")]
    [Display(Name = "Date & Time")]
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddHours(9);

    [Required(ErrorMessage = "Reason for visit is required")]
    [MaxLength(500)]
    [Display(Name = "Reason for Visit")]
    public string ReasonForVisit { get; set; } = string.Empty;

    // Dropdowns
    public IEnumerable<SelectListItem> Patients { get; set; } = [];
    public IEnumerable<SelectListItem> Doctors  { get; set; } = [];
}
