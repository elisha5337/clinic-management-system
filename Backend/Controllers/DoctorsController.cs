using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Services;
using ClinicManagementSystem.ViewModels.Doctors;

namespace ClinicManagementSystem.Controllers;

[Authorize]
public class DoctorsController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;

    public DoctorsController(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        IAuditService auditService)
    {
        _context      = context;
        _userManager  = userManager;
        _auditService = auditService;
    }

    // ── Index ─────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var doctors = await _context.Doctors
            .Include(d => d.ApplicationUser)
            .OrderBy(d => d.FullName)
            .ToListAsync();

        return View(doctors);
    }

    // ── Create ────────────────────────────────────────────────────
    [Authorize(Roles = "Receptionist")]
    public IActionResult Create()
    {
        return View(new DoctorFormViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Receptionist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DoctorFormViewModel vm)
    {
        // AccountEmail + AccountPassword are optional but must both be provided together
        if (!string.IsNullOrEmpty(vm.AccountEmail) && string.IsNullOrEmpty(vm.AccountPassword))
            ModelState.AddModelError(nameof(vm.AccountPassword),
                "Password is required when creating a login account.");

        if (!ModelState.IsValid) return View(vm);

        string? userId = null;

        // Create linked login account if credentials were provided
        if (!string.IsNullOrEmpty(vm.AccountEmail) && !string.IsNullOrEmpty(vm.AccountPassword))
        {
            var appUser = new ApplicationUser
            {
                FullName       = vm.FullName,
                UserName       = vm.AccountEmail,
                Email          = vm.AccountEmail,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(appUser, vm.AccountPassword);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(vm);
            }

            await _userManager.AddToRoleAsync(appUser, "Doctor");
            userId = appUser.Id;
        }

        var doctor = new Doctor
        {
            FullName          = vm.FullName,
            Specialization    = vm.Specialization,
            Phone             = vm.Phone,
            Email             = vm.Email,
            IsAvailable       = vm.IsAvailable,
            ApplicationUserId = userId
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        var currentUser = await _userManager.GetUserAsync(User);
        await _auditService.LogAsync(
            currentUser!.Id, currentUser.UserName ?? currentUser.Email!,
            "Created", "Doctor", doctor.Id.ToString(),
            $"Added doctor: {doctor.FullName} ({doctor.Specialization})");

        TempData["Success"] = $"Dr. {doctor.FullName} added successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── Edit ──────────────────────────────────────────────────────
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> Edit(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null) return NotFound();

        var vm = new DoctorFormViewModel
        {
            Id             = doctor.Id,
            FullName       = doctor.FullName,
            Specialization = doctor.Specialization,
            Phone          = doctor.Phone,
            Email          = doctor.Email,
            IsAvailable    = doctor.IsAvailable
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Receptionist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DoctorFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        // On edit, account fields are not required
        ModelState.Remove(nameof(vm.AccountEmail));
        ModelState.Remove(nameof(vm.AccountPassword));

        if (!ModelState.IsValid) return View(vm);

        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null) return NotFound();

        doctor.FullName       = vm.FullName;
        doctor.Specialization = vm.Specialization;
        doctor.Phone          = vm.Phone;
        doctor.Email          = vm.Email;
        doctor.IsAvailable    = vm.IsAvailable;

        await _context.SaveChangesAsync();

        var user = await _userManager.GetUserAsync(User);
        await _auditService.LogAsync(
            user!.Id, user.UserName ?? user.Email!,
            "Updated", "Doctor", doctor.Id.ToString(),
            $"Updated doctor: {doctor.FullName}");

        TempData["Success"] = $"Dr. {doctor.FullName} updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── Toggle availability (AJAX-friendly POST) ──────────────────
    [HttpPost]
    [Authorize(Roles = "Receptionist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null) return NotFound();

        doctor.IsAvailable = !doctor.IsAvailable;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Dr. {doctor.FullName} is now marked as " +
                              $"{(doctor.IsAvailable ? "Available" : "Unavailable")}.";
        return RedirectToAction(nameof(Index));
    }
}
