using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Services;
using ClinicManagementSystem.ViewModels.Vitals;

namespace ClinicManagementSystem.Controllers;

[Authorize(Roles = "Nurse")]
public class VitalsController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;

    public VitalsController(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        IAuditService auditService)
    {
        _context      = context;
        _userManager  = userManager;
        _auditService = auditService;
    }

    // ── Create GET ────────────────────────────────────────────────
    public async Task<IActionResult> Create(int appointmentId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null) return NotFound();

        // Guard: vitals already recorded
        if (await _context.Vitals.AnyAsync(v => v.AppointmentId == appointmentId))
        {
            TempData["Error"] = "Vitals have already been recorded for this appointment.";
            return RedirectToAction("Details", "Appointments", new { id = appointmentId });
        }

        // Guard: cancelled appointments cannot have vitals
        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            TempData["Error"] = "Cannot record vitals for a cancelled appointment.";
            return RedirectToAction("Details", "Appointments", new { id = appointmentId });
        }

        var vm = new VitalsFormViewModel
        {
            AppointmentId   = appointmentId,
            PatientName     = appointment.Patient.FullName,
            DoctorName      = appointment.Doctor.FullName,
            AppointmentDate = appointment.AppointmentDate
        };

        return View(vm);
    }

    // ── Create POST ───────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VitalsFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.GetUserAsync(User);

        var vitals = new Vitals
        {
            AppointmentId = vm.AppointmentId,
            BloodPressure = vm.BloodPressure,
            HeartRate     = vm.HeartRate,
            Temperature   = vm.Temperature,
            Weight        = vm.Weight,
            Height        = vm.Height,
            Notes         = vm.Notes,
            RecordedById  = user?.Id,
            RecordedAt    = DateTime.UtcNow
        };

        // Update appointment status to InProgress and save nurse notes
        var appointment = await _context.Appointments.FindAsync(vm.AppointmentId);
        if (appointment != null)
        {
            if (appointment.Status == AppointmentStatus.Scheduled)
                appointment.Status = AppointmentStatus.InProgress;

            if (!string.IsNullOrWhiteSpace(vm.PreExamNotes))
                appointment.NurseNotes = vm.PreExamNotes;
        }

        _context.Vitals.Add(vitals);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            user!.Id, user.UserName ?? user.Email!,
            "Created", "Vitals", vitals.Id.ToString(),
            $"Recorded vitals for appointment ID {vm.AppointmentId}");

        TempData["Success"] = "Vitals recorded successfully.";
        return RedirectToAction("Details", "Appointments", new { id = vm.AppointmentId });
    }
}
