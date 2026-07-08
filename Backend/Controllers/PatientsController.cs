using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Services;
using ClinicManagementSystem.ViewModels.Patients;

namespace ClinicManagementSystem.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;

    public PatientsController(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        IAuditService auditService)
    {
        _context      = context;
        _userManager  = userManager;
        _auditService = auditService;
    }

    // ── Index — searchable patient list ──────────────────────────
    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Patients
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query  = query.Where(p =>
                p.FullName.ToLower().Contains(search) ||
                p.Phone.Contains(search) ||
                (p.Email != null && p.Email.ToLower().Contains(search)));
        }

        var patients = await query
            .OrderBy(p => p.FullName)
            .ToListAsync();

        ViewData["Search"] = search;
        return View(patients);
    }

    // ── Details ───────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (patient == null) return NotFound();

        var appointments = await _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Vitals)
            .Where(a => a.PatientId == id)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        var vm = new PatientDetailsViewModel
        {
            Patient              = patient,
            Appointments         = appointments,
            TotalAppointments    = appointments.Count,
            CompletedAppointments= appointments.Count(a => a.Status == AppointmentStatus.Completed),
            LastAppointment      = appointments.FirstOrDefault()
        };

        return View(vm);
    }

    // ── Create ────────────────────────────────────────────────────
    [Authorize(Roles = "Receptionist")]
    public IActionResult Create()
    {
        return View(new PatientFormViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Receptionist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PatientFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user    = await _userManager.GetUserAsync(User);
        var patient = new Patient
        {
            FullName              = vm.FullName,
            DateOfBirth           = vm.DateOfBirth,
            Gender                = vm.Gender,
            Phone                 = vm.Phone,
            Email                 = vm.Email,
            Address               = vm.Address,
            BloodType             = vm.BloodType,
            EmergencyContactName  = vm.EmergencyContactName,
            EmergencyContactPhone = vm.EmergencyContactPhone,
            CreatedById           = user?.Id
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            user!.Id, user.UserName ?? user.Email!,
            "Created", "Patient", patient.Id.ToString(),
            $"Registered patient: {patient.FullName}");

        TempData["Success"] = $"Patient '{patient.FullName}' registered successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── Edit ──────────────────────────────────────────────────────
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> Edit(int id)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (patient == null) return NotFound();

        var vm = new PatientFormViewModel
        {
            Id                    = patient.Id,
            FullName              = patient.FullName,
            DateOfBirth           = patient.DateOfBirth,
            Gender                = patient.Gender,
            Phone                 = patient.Phone,
            Email                 = patient.Email,
            Address               = patient.Address,
            BloodType             = patient.BloodType,
            EmergencyContactName  = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Receptionist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PatientFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (patient == null) return NotFound();

        patient.FullName              = vm.FullName;
        patient.DateOfBirth           = vm.DateOfBirth;
        patient.Gender                = vm.Gender;
        patient.Phone                 = vm.Phone;
        patient.Email                 = vm.Email;
        patient.Address               = vm.Address;
        patient.BloodType             = vm.BloodType;
        patient.EmergencyContactName  = vm.EmergencyContactName;
        patient.EmergencyContactPhone = vm.EmergencyContactPhone;

        await _context.SaveChangesAsync();

        var user = await _userManager.GetUserAsync(User);
        await _auditService.LogAsync(
            user!.Id, user.UserName ?? user.Email!,
            "Updated", "Patient", patient.Id.ToString(),
            $"Updated patient: {patient.FullName}");

        TempData["Success"] = $"Patient '{patient.FullName}' updated successfully.";
        return RedirectToAction(nameof(Details), new { id = patient.Id });
    }

    // ── Delete (soft delete) ──────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Receptionist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (patient == null) return NotFound();

        // Soft delete — preserve historical appointment data
        patient.IsActive = false;
        await _context.SaveChangesAsync();

        var user = await _userManager.GetUserAsync(User);
        await _auditService.LogAsync(
            user!.Id, user.UserName ?? user.Email!,
            "Deleted", "Patient", patient.Id.ToString(),
            $"Soft-deleted patient: {patient.FullName}");

        TempData["Success"] = $"Patient '{patient.FullName}' has been removed.";
        return RedirectToAction(nameof(Index));
    }
}
