using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Services;
using ClinicManagementSystem.ViewModels.Account;

namespace ClinicManagementSystem.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser>  _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAuditService _auditService;

    public AccountController(
        UserManager<ApplicationUser>  userManager,
        SignInManager<ApplicationUser> signInManager,
        IAuditService auditService)
    {
        _userManager   = userManager;
        _signInManager = signInManager;
        _auditService  = auditService;
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Already logged in → go to dashboard
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToDashboard();

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            await _auditService.LogAsync(
                user.Id, user.UserName ?? user.Email!,
                "Login", "Auth", user.Id, $"{user.FullName} logged in");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToDashboard();
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty,
                "Account locked due to multiple failed attempts. Try again in 15 minutes.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    // ── Register (Receptionist-only access) ───────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "Receptionist")]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Receptionist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            FullName       = model.FullName,
            UserName       = model.Email,
            Email          = model.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role);

            var currentUser = await _userManager.GetUserAsync(User);
            await _auditService.LogAsync(
                currentUser!.Id, currentUser.UserName ?? currentUser.Email!,
                "Created", "User", user.Id,
                $"Created user {user.FullName} with role {model.Role}");

            TempData["Success"] = $"User '{model.FullName}' created successfully with role '{model.Role}'.";
            return RedirectToAction(nameof(Register));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            await _auditService.LogAsync(
                user.Id, user.UserName ?? user.Email!,
                "Logout", "Auth", user.Id, $"{user.FullName} logged out");
        }

        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    // ── Access Denied ─────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // ── Private helper — redirect based on role ───────────────────────────────

    private IActionResult RedirectToDashboard()
    {
        return RedirectToAction("Index", "Dashboard");
    }
}
