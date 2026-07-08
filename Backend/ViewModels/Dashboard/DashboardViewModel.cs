using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.ViewModels.Dashboard;

public class DashboardViewModel
{
    // ── Stat cards ────────────────────────────────────────────────
    public int TotalPatients          { get; set; }
    public int TotalDoctors           { get; set; }
    public int TodayAppointments      { get; set; }
    public int PendingAppointments    { get; set; }

    // ── Chart data — appointments by status (pie chart) ───────────
    public int ScheduledCount         { get; set; }
    public int InProgressCount        { get; set; }
    public int CompletedCount         { get; set; }
    public int CancelledCount         { get; set; }

    // ── Chart data — appointments per day this week (bar chart) ───
    public List<string> WeekDayLabels { get; set; } = [];
    public List<int>    WeekDayCounts { get; set; } = [];

    // ── Recent appointments table ─────────────────────────────────
    public List<Appointment> RecentAppointments { get; set; } = [];
}
