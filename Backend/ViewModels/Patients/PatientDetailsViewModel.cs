using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.ViewModels.Patients;

public class PatientDetailsViewModel
{
    public Patient Patient { get; set; } = null!;
    public List<Appointment> Appointments { get; set; } = [];
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public Appointment? LastAppointment { get; set; }
}
