using CuraLink.Application.DTOs;
using CuraLink.Application.Interfaces;
using CuraLink.Domain.Common;
using CuraLink.Web.Areas.Doctor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.Web.Areas.Doctor.Controllers;

[Area("Doctor")]
[Authorize(Roles = AppRoles.Doctor)]
public class ConsultationsController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IConsultationService _consultationService;
    private readonly IAppointmentService _appointmentService;
    private readonly ICurrentUserService _currentUserService;

    public ConsultationsController(
        IDoctorService doctorService,
        IConsultationService consultationService,
        IAppointmentService appointmentService,
        ICurrentUserService currentUserService)
    {
        _doctorService = doctorService;
        _consultationService = consultationService;
        _appointmentService = appointmentService;
        _currentUserService = currentUserService;
    }

    public async Task<IActionResult> Index()
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var model = await _consultationService.GetForDoctorAsync(doctor.Id);
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var result = await _consultationService.GetForDoctorAsync(doctor.Id, id);
        if (!result.Succeeded || result.Value is null)
        {
            TempData["Error"] = result.Errors.Any() ? string.Join(" ", result.Errors) : "Consultation not found.";
            return RedirectToAction("Index");
        }

        return View(result.Value);
    }

    public async Task<IActionResult> Create(int? appointmentId)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        return View(new ConsultationFormViewModel
        {
            Appointments = await LoadAppointmentsAsync(doctor.Id),
            Input = new ConsultationInput { AppointmentId = appointmentId ?? 0 }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConsultationFormViewModel model)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        model.Appointments = await LoadAppointmentsAsync(doctor.Id);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _consultationService.CreateAsync(
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

        TempData["Success"] = "Consultation created.";
        return RedirectToAction("Details", new { id = result.Value });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var result = await _consultationService.GetForDoctorAsync(doctor.Id, id);
        if (!result.Succeeded || result.Value is null)
        {
            TempData["Error"] = "Consultation not found.";
            return RedirectToAction("Index");
        }

        var consultation = result.Value;
        return View(new ConsultationFormViewModel
        {
            ConsultationId = consultation.Id,
            Appointments = await LoadAppointmentsAsync(doctor.Id),
            Input = new ConsultationInput
            {
                AppointmentId = consultation.AppointmentId,
                Reason = consultation.Reason,
                ReportedSymptoms = consultation.ReportedSymptoms,
                ClinicalObservations = consultation.ClinicalObservations,
                Diagnosis = consultation.Diagnosis,
                TreatmentPlan = consultation.TreatmentPlan,
                Recommendations = consultation.Recommendations,
                FollowUpDate = consultation.FollowUpDate
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ConsultationFormViewModel model)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        model.ConsultationId = id;
        model.Appointments = await LoadAppointmentsAsync(doctor.Id);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _consultationService.UpdateAsync(
            doctor.Id,
            id,
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

        TempData["Success"] = "Consultation updated.";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalize(int id)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var result = await _consultationService.FinalizeAsync(
            doctor.Id,
            id,
            _currentUserService.UserId,
            DateTime.UtcNow);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "Consultation finalized."
            : string.Join(" ", result.Errors);

        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Share(int id)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var result = await _consultationService.ShareWithPatientAsync(
            doctor.Id,
            id,
            _currentUserService.UserId,
            DateTime.UtcNow);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "Consultation shared with the patient."
            : string.Join(" ", result.Errors);

        return RedirectToAction("Details", new { id });
    }

    private async Task<IReadOnlyList<AppointmentDto>> LoadAppointmentsAsync(int doctorId)
    {
        var result = await _appointmentService.GetDoctorAppointmentsAsync(
            doctorId,
            new AppointmentFilter { Page = 1, PageSize = 200 });

        return result.Items;
    }
}
