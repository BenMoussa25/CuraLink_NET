using CuraLink.Application.Interfaces;
using CuraLink.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.Web.Areas.Doctor.Controllers;

[Area("Doctor")]
[Authorize(Roles = AppRoles.Doctor)]
public class PatientsController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;

    public PatientsController(IDoctorService doctorService, IPatientService patientService)
    {
        _doctorService = doctorService;
        _patientService = patientService;
    }

    public async Task<IActionResult> Index()
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var model = await _patientService.GetDoctorPatientsAsync(doctor.Id);
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var result = await _patientService.GetPatientForDoctorAsync(doctor.Id, id);
        if (!result.Succeeded || result.Value is null)
        {
            TempData["Error"] = result.Errors.Any() ? string.Join(" ", result.Errors) : "Patient not found.";
            return RedirectToAction("Index");
        }

        return View(result.Value);
    }
}
