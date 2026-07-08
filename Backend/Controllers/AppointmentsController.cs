using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Services;
using ClinicManagementSystem.ViewModels.Appointments;

namespace ClinicManagementSystem.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;

    public AppointmentsController(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        IAuditService auditService)
    {
        _context      = context;
        _userManager  = userManager;
        _auditService = auditService;
    }

    // ── Index — filterable list ───────────────────────────────────
    public async Task<IActionResult> Index(string? status, string? date, int? doctorId)
    {
        var user  = await _userManager.GetUserAsync(User);
        var query = _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .AsQueryable();

        // Doctors only see their own appointments
        if (User.IsInRole("Doctor") && user != null)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.ApplicationUserId == user.Id);
            if (doctor != null)
                query = query.Where(a => a.DoctorId == doctor.Id);
        }

        // Filters
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<AppointmentStatus>(status, out var parsedStatus))
            query = query.Where(a => a.Status == parsedStatus);

        if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDate))
            query = query.Where(a => a.AppointmentDate.Date == parsedDate.Date);

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId.Value);

        var appointments = await query
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        // Pass filter options to view
        ViewData["StatusFilter"]   = status;
        ViewData["DateFilter"]     = date;
        ViewData["DoctorFilter"]   = doctorId;
        ViewData["DoctorList"]     = new SelectList(
            await _context.Doctors.OrderBy(d => d.FullName).ToListAsync(),
            "Id", "FullName", doctorId);

        return View(appointments);
    }

    // ── Details ───────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Vitals)
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return NotFound();

        // Doctors can only view their own appointments
        if (User.IsInRole("Doctor"))
        {
            var user   = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.ApplicationUserId == user!.Id);

            if (doctor == null || appointment.DoctorId != doctor.Id)
                return Forbid();
        }

        return View(appointment);
    }

    // ── Create ────────────────────────────────────────────────────
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> Create(int? patientId)
    {
        var vm = new AppointmentFormViewModel
        {
            PatientId = patientId ?? 0,
            Patients  = await GetPatientSelectList(),
            Doctors   = await GetDoctorSelectList()
        };
        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Receptionist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Patients = await GetPatientSelectList();
            vm.Doctors  = await GetDoctorSelectList();
            return View(vm);
        }

        var user = await _userManager.GetUserAsync(User);
        var appointment = new Appointment
        {
            PatientId       = vm.PatientId,
            DoctorId        = vm.DoctorId,
            AppointmentDate = vm.AppointmentDate,
            ReasonForVisit  = vm.ReasonForVisit,
            Status          = AppointmentStatus.Scheduled,
            CreatedById     = user?.Id
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            user!.Id, user.UserName ?? user.Email!,
            "Created", "Appointment", appointment.Id.ToString(),
            $"Booked appointment for patient ID {vm.PatientId} with doctor ID {vm.DoctorId}");

        TempData["Success"] = "Appointment booked successfully.";
        return RedirectToAction(nameof(Details), new { id = appointment.Id });
    }

    // ── Edit ──────────────────────────────────────────────────────
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> Edit(int id)
    {
        var appt = await _context.Appointments.FindAsync(id);
        if (appt == null) return NotFound();

        if (appt.Status == AppointmentStatus.Completed ||
            appt.Status == AppointmentStatus.Cancelled)
        {
            TempData["Error"] = "Completed or cancelled appointments cannot be edited.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var vm = new AppointmentFormViewModel
        {
            Id              = appt.Id,
            PatientId       = appt.PatientId,
            DoctorId        = appt.DoctorId,
            AppointmentDate = appt.AppointmentDate,
            ReasonForVisit  = appt.ReasonForVisit,
            Patients        = await GetPatientSelectList(),
            Doctors         = await GetDoctorSelectList()
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Receptionist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppointmentFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            vm.Patients = await GetPatientSelectList();
            vm.Doctors  = await GetDoctorSelectList();
            return View(vm);
        }

        var appt = await _context.Appointments.FindAsync(id);
        if (appt == null) return NotFound();

        appt.PatientId       = vm.PatientId;
        appt.DoctorId        = vm.DoctorId;
        appt.AppointmentDate = vm.AppointmentDate;
        appt.ReasonForVisit  = vm.ReasonForVisit;

        await _context.SaveChangesAsync();

        var user = await _userManager.GetUserAsync(User);
        await _auditService.LogAsync(
            user!.Id, user.UserName ?? user.Email!,
            "Updated", "Appointment", appt.Id.ToString(),
            $"Rescheduled appointment ID {appt.Id}");

        TempData["Success"] = "Appointment updated successfully.";
        return RedirectToAction(nameof(Details), new { id = appt.Id });
    }

    // ── Update Status (Doctor only) ───────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Doctor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status,
                                                   string? diagnosis, string? prescription)
    {
        var appt = await _context.Appointments.FindAsync(id);
        if (appt == null) return NotFound();

        appt.Status = status;

        if (!string.IsNullOrWhiteSpace(diagnosis))
            appt.Diagnosis = diagnosis;

        if (!string.IsNullOrWhiteSpace(prescription))
            appt.Prescription = prescription;

        await _context.SaveChangesAsync();

        var user = await _userManager.GetUserAsync(User);
        await _auditService.LogAsync(
            user!.Id, user.UserName ?? user.Email!,
            "StatusUpdated", "Appointment", appt.Id.ToString(),
            $"Status changed to {status}" +
            (string.IsNullOrEmpty(diagnosis) ? "" : " with diagnosis"));

        TempData["Success"] = $"Appointment status updated to {status}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Cancel (Receptionist only) ────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Receptionist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var appt = await _context.Appointments.FindAsync(id);
        if (appt == null) return NotFound();

        appt.Status = AppointmentStatus.Cancelled;
        await _context.SaveChangesAsync();

        var user = await _userManager.GetUserAsync(User);
        await _auditService.LogAsync(
            user!.Id, user.UserName ?? user.Email!,
            "Cancelled", "Appointment", appt.Id.ToString(),
            $"Cancelled appointment ID {appt.Id}");

        TempData["Success"] = "Appointment cancelled.";
        return RedirectToAction(nameof(Index));
    }

    // ── Private helpers ───────────────────────────────────────────
    private async Task<IEnumerable<SelectListItem>> GetPatientSelectList() =>
        (await _context.Patients
            .Where(p => p.IsActive)
            .OrderBy(p => p.FullName)
            .ToListAsync())
        .Select(p => new SelectListItem(
            $"{p.FullName} ({p.Phone})", p.Id.ToString()));

    private async Task<IEnumerable<SelectListItem>> GetDoctorSelectList() =>
        (await _context.Doctors
            .Where(d => d.IsAvailable)
            .OrderBy(d => d.FullName)
            .ToListAsync())
        .Select(d => new SelectListItem(
            $"{d.FullName} — {d.Specialization}", d.Id.ToString()));
}
