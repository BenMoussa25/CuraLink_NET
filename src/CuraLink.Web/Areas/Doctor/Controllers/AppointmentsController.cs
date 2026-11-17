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
public class AppointmentsController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IAppointmentService _appointmentService;
    private readonly ICurrentUserService _currentUserService;

    public AppointmentsController(
        IDoctorService doctorService,
        IAppointmentService appointmentService,
        ICurrentUserService currentUserService)
    {
        _doctorService = doctorService;
        _appointmentService = appointmentService;
        _currentUserService = currentUserService;
    }

    public async Task<IActionResult> Index(AppointmentFilter? filter)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        filter ??= new AppointmentFilter();
        if (filter.Page < 1)
        {
            filter.Page = 1;
        }

        if (filter.PageSize < 1 || filter.PageSize > 100)
        {
            filter.PageSize = 20;
        }

        var result = await _appointmentService.GetDoctorAppointmentsAsync(doctor.Id, filter);
        return View(new AppointmentIndexViewModel
        {
            Filter = filter,
            Result = result
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var result = await _appointmentService.GetForDoctorAsync(doctor.Id, id);
        if (!result.Succeeded || result.Value is null)
        {
            TempData["Error"] = result.Errors.Any() ? string.Join(" ", result.Errors) : "Appointment not found.";
            return RedirectToAction("Index");
        }

        return View(new AppointmentDetailsViewModel
        {
            Appointment = result.Value,
            NewStatus = result.Value.Status,
            StartUtc = result.Value.StartAtUtc,
            EndUtc = result.Value.EndAtUtc
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, AppointmentStatus newStatus, string? reason)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        if (!Enum.IsDefined(typeof(AppointmentStatus), newStatus))
        {
            TempData["Error"] = "Invalid appointment status.";
            return RedirectToAction("Details", new { id });
        }

        var result = await _appointmentService.ChangeStatusAsync(
            doctor.Id,
            id,
            newStatus,
            reason,
            _currentUserService.UserId,
            DateTime.UtcNow);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "Appointment status updated."
            : string.Join(" ", result.Errors);

        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(int id, DateTime startUtc, DateTime endUtc)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        if (startUtc == default || endUtc == default || endUtc <= startUtc)
        {
            TempData["Error"] = "Provide a valid future time range.";
            return RedirectToAction("Details", new { id });
        }

        var result = await _appointmentService.RescheduleAsync(
            doctor.Id,
            id,
            DateTime.SpecifyKind(startUtc, DateTimeKind.Utc),
            DateTime.SpecifyKind(endUtc, DateTimeKind.Utc),
            _currentUserService.UserId,
            DateTime.UtcNow);

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "Appointment rescheduled."
            : string.Join(" ", result.Errors);

        return RedirectToAction("Details", new { id });
    }
}
