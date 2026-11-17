using CuraLink.Application.DTOs;
using CuraLink.Application.Interfaces;
using CuraLink.Domain.Common;
using CuraLink.Domain.Entities;
using CuraLink.Domain.Enums;
using CuraLink.Infrastructure.Persistence;
using CuraLink.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDoctorService _doctorService;
    private readonly ApplicationDbContext _context;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IDoctorService doctorService,
        ApplicationDbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _doctorService = doctorService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return await RedirectToRoleHomeAsync(await _userManager.GetUserAsync(User));
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            TempData["Error"] = "Invalid email or password.";
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        return await RedirectToRoleHomeAsync(user, returnUrl);
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return await RedirectToRoleHomeAsync(await _userManager.GetUserAsync(User));
        }

        await LoadSpecialtiesAsync();
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var specialties = await LoadSpecialtiesAsync();

        var role = model.Role?.Trim();
        if (role != AppRoles.Doctor && role != AppRoles.Patient)
        {
            ModelState.AddModelError(nameof(model.Role), "Choose Doctor or Patient.");
        }

        if (role == AppRoles.Doctor)
        {
            if (!model.SpecialtyId.HasValue)
            {
                ModelState.AddModelError(nameof(model.SpecialtyId), "Select a specialty.");
            }
            else if (!specialties.Any(s => s.Id == model.SpecialtyId.Value && s.IsActive))
            {
                ModelState.AddModelError(nameof(model.SpecialtyId), "Select an active specialty.");
            }

            if (string.IsNullOrWhiteSpace(model.Biography))
            {
                ModelState.AddModelError(nameof(model.Biography), "Biography is required for doctors.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existing = await _userManager.FindByEmailAsync(model.Email);
        if (existing is not null)
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        var created = await _userManager.CreateAsync(user, model.Password);
        if (!created.Succeeded)
        {
            foreach (var error in created.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role!);
        if (!roleResult.Succeeded)
        {
            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        if (role == AppRoles.Doctor)
        {
            _context.Doctors.Add(new Doctor
            {
                UserId = user.Id,
                SpecialtyId = model.SpecialtyId!.Value,
                Biography = model.Biography!.Trim(),
                VerificationStatus = DoctorVerificationStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            _context.Patients.Add(new Patient
            {
                UserId = user.Id,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        await _signInManager.SignInAsync(user, isPersistent: false);

        TempData["Success"] = role == AppRoles.Doctor
            ? "Your account was created. The doctor profile is pending verification."
            : "Welcome to CuraLink.";

        return await RedirectToRoleHomeAsync(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["Success"] = "You have been signed out.";
        return Redirect("/");
    }

    private async Task<IReadOnlyList<SpecialtyDto>> LoadSpecialtiesAsync()
    {
        var specialties = await _doctorService.GetActiveSpecialtiesAsync();
        ViewBag.Specialties = specialties;
        return specialties;
    }

    private async Task<IActionResult> RedirectToRoleHomeAsync(ApplicationUser? user, string? returnUrl = null)
    {
        if (user is not null && await _userManager.IsInRoleAsync(user, AppRoles.Doctor))
        {
            return Redirect("/Doctor/Dashboard");
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return Redirect("/");
    }
}
