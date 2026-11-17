using CuraLink.Application.DTOs;
using CuraLink.Application.Interfaces;
using CuraLink.Domain.Common;
using CuraLink.Domain.Enums;
using CuraLink.Web.Areas.Doctor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.Web.Areas.Doctor.Controllers;

[Area("Doctor")]
[Authorize(Roles = AppRoles.Doctor)]
public class PrescriptionsController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IPrescriptionService _prescriptionService;
    private readonly IConsultationService _consultationService;
    private readonly ICurrentUserService _currentUserService;

    public PrescriptionsController(
        IDoctorService doctorService,
        IPrescriptionService prescriptionService,
        IConsultationService consultationService,
        ICurrentUserService currentUserService)
    {
        _doctorService = doctorService;
        _prescriptionService = prescriptionService;
        _consultationService = consultationService;
        _currentUserService = currentUserService;
    }

    public async Task<IActionResult> Index()
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var model = await _prescriptionService.GetForDoctorAsync(doctor.Id);
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var prescriptions = await _prescriptionService.GetForDoctorAsync(doctor.Id);
        var prescription = prescriptions.FirstOrDefault(p => p.Id == id);
        if (prescription is null)
        {
            TempData["Error"] = "Prescription not found.";
            return RedirectToAction("Index");
        }

        return View(prescription);
    }

    public async Task<IActionResult> Create(int? consultationId)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        return View(new PrescriptionCreateViewModel
        {
            Consultations = (await _consultationService.GetForDoctorAsync(doctor.Id))
                .Where(x => x.DoctorId == doctor.Id && x.Status == ConsultationStatus.Finalized)
                .ToList(),
            Input = new PrescriptionInput
            {
                ConsultationId = consultationId ?? 0,
                Items = new List<PrescriptionItemInput> { new() }
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PrescriptionCreateViewModel model, bool addItem = false)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        model.Consultations = (await _consultationService.GetForDoctorAsync(doctor.Id))
            .Where(x => x.DoctorId == doctor.Id && x.Status == ConsultationStatus.Finalized)
            .ToList();

        if (addItem)
        {
            model.Input.Items ??= new List<PrescriptionItemInput>();
            model.Input.Items.Add(new PrescriptionItemInput());
            ModelState.Clear();
            return View(model);
        }

        if (model.Input.Items is null || model.Input.Items.Count == 0)
        {
            ModelState.AddModelError("Input.Items", "Add at least one medicine.");
        }

        model.Input.Items ??= new List<PrescriptionItemInput> { new() };

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _prescriptionService.IssueAsync(
            doctor.Id,
            model.Input,
            _currentUserService.UserId,
            DateTime.UtcNow);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        TempData["Success"] = "Prescription issued.";
        return RedirectToAction("Details", new { id = result.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var result = await _prescriptionService.CancelAsync(
            doctor.Id,
            id,
            _currentUserService.UserId,
            DateTime.UtcNow);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "Prescription cancelled."
            : string.Join(" ", result.Errors);

        return RedirectToAction("Details", new { id });
    }
}
