using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.ViewModels.Dashboard;

namespace ClinicManagementSystem.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context     = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var user  = await _userManager.GetUserAsync(User);

        // Base appointment query — Doctors only see their own appointments
        IQueryable<Appointment> appointmentsQuery = _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor);

        if (User.IsInRole("Doctor") && user != null)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.ApplicationUserId == user.Id);

            if (doctor != null)
                appointmentsQuery = appointmentsQuery.Where(a => a.DoctorId == doctor.Id);
        }

        var allAppointments = await appointmentsQuery.ToListAsync();

        // ── Week labels (Mon–Sun of current week) ─────────────────
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        var weekLabels  = Enumerable.Range(0, 7)
            .Select(i => startOfWeek.AddDays(i).ToString("ddd dd"))
            .ToList();

        var weekCounts = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var day = startOfWeek.AddDays(i).Date;
                return allAppointments.Count(a => a.AppointmentDate.Date == day);
            })
            .ToList();

        var vm = new DashboardViewModel
        {
            TotalPatients       = await _context.Patients.CountAsync(p => p.IsActive),
            TotalDoctors        = await _context.Doctors.CountAsync(d => d.IsAvailable),
            TodayAppointments   = allAppointments.Count(a => a.AppointmentDate.Date == today),
            PendingAppointments = allAppointments.Count(a => a.Status == AppointmentStatus.Scheduled),

            ScheduledCount  = allAppointments.Count(a => a.Status == AppointmentStatus.Scheduled),
            InProgressCount = allAppointments.Count(a => a.Status == AppointmentStatus.InProgress),
            CompletedCount  = allAppointments.Count(a => a.Status == AppointmentStatus.Completed),
            CancelledCount  = allAppointments.Count(a => a.Status == AppointmentStatus.Cancelled),

            WeekDayLabels = weekLabels,
            WeekDayCounts = weekCounts,

            RecentAppointments = allAppointments
                .OrderByDescending(a => a.AppointmentDate)
                .Take(8)
                .ToList()
        };

        return View(vm);
    }
}
