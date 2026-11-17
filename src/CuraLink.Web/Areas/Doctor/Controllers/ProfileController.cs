using CuraLink.Application.DTOs;
using CuraLink.Application.Interfaces;
using CuraLink.Domain.Common;
using CuraLink.Web.Areas.Doctor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.Web.Areas.Doctor.Controllers;

[Area("Doctor")]
[Authorize(Roles = AppRoles.Doctor)]
public class ProfileController : Controller
{
    private readonly IDoctorService _doctorService;

    public ProfileController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    public async Task<IActionResult> Index()
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var result = await _doctorService.GetProfileAsync(doctor.Id);
        if (!result.Succeeded || result.Value is null)
        {
            TempData["Error"] = result.Errors.Any() ? string.Join(" ", result.Errors) : "Profile not found.";
            return RedirectToAction("Index", "Dashboard");
        }

        return View(result.Value);
    }

    public async Task<IActionResult> Edit()
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var result = await _doctorService.GetProfileAsync(doctor.Id);
        if (!result.Succeeded || result.Value is null)
        {
            TempData["Error"] = "Profile not found.";
            return RedirectToAction("Index", "Dashboard");
        }

        var profile = result.Value;
        var model = new DoctorProfileEditViewModel
        {
            Profile = profile,
            Specialties = await _doctorService.GetActiveSpecialtiesAsync(),
            Input = new DoctorProfileInput
            {
                Biography = profile.Biography,
                ClinicName = profile.ClinicName,
                ClinicAddress = profile.ClinicAddress,
                City = profile.City,
                PublicPhone = profile.PublicPhone,
                ConsultationFee = profile.ConsultationFee,
                CurrencyCode = profile.CurrencyCode,
                SpecialtyId = profile.SpecialtyId,
                ProfileImagePath = profile.ProfileImagePath
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DoctorProfileEditViewModel model)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        model.Specialties = await _doctorService.GetActiveSpecialtiesAsync();

        if (!model.Specialties.Any(s => s.Id == model.Input.SpecialtyId && s.IsActive))
        {
            ModelState.AddModelError("Input.SpecialtyId", "Select an active specialty.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _doctorService.UpdateProfileAsync(doctor.Id, model.Input);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction("Index");
    }
}
