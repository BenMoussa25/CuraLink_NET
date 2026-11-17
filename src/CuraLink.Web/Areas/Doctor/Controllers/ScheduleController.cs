using CuraLink.Application.DTOs;
using CuraLink.Application.Interfaces;
using CuraLink.Domain.Common;
using CuraLink.Web.Areas.Doctor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.Web.Areas.Doctor.Controllers;

[Area("Doctor")]
[Authorize(Roles = AppRoles.Doctor)]
public class ScheduleController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IDoctorService doctorService, IScheduleService scheduleService)
    {
        _doctorService = doctorService;
        _scheduleService = scheduleService;
    }

    public async Task<IActionResult> Index()
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var model = new ScheduleIndexViewModel
        {
            Schedules = await _scheduleService.GetSchedulesAsync(doctor.Id),
            Exceptions = await _scheduleService.GetExceptionsAsync(doctor.Id)
        };

        return View(model);
    }

    public IActionResult Add()
    {
        return View(new ScheduleInput
        {
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            SlotDurationMinutes = 30,
            IsActive = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(ScheduleInput input)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        if (input.EndTime <= input.StartTime)
        {
            ModelState.AddModelError(string.Empty, "End time must be after start time.");
        }

        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var result = await _scheduleService.AddScheduleAsync(doctor.Id, input);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(input);
        }

        TempData["Success"] = "Schedule added.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var schedule = (await _scheduleService.GetSchedulesAsync(doctor.Id)).FirstOrDefault(s => s.Id == id);
        if (schedule is null)
        {
            TempData["Error"] = "Schedule not found.";
            return RedirectToAction("Index");
        }

        return View(new ScheduleInput
        {
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            SlotDurationMinutes = schedule.SlotDurationMinutes,
            IsActive = schedule.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ScheduleInput input)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        if (input.EndTime <= input.StartTime)
        {
            ModelState.AddModelError(string.Empty, "End time must be after start time.");
        }

        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var result = await _scheduleService.UpdateScheduleAsync(doctor.Id, id, input);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(input);
        }

        TempData["Success"] = "Schedule updated.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var result = await _scheduleService.DeleteScheduleAsync(doctor.Id, id);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "Schedule deleted."
            : string.Join(" ", result.Errors);

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Exceptions()
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        return View(new ScheduleExceptionsViewModel
        {
            Exceptions = await _scheduleService.GetExceptionsAsync(doctor.Id),
            Input = new ScheduleExceptionInput { Date = DateTime.Today, IsUnavailable = true }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddException(ScheduleExceptionsViewModel model)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        if (model.Input.Date == default)
        {
            ModelState.AddModelError("Input.Date", "Date is required.");
        }

        if (model.Input.StartTime.HasValue && model.Input.EndTime.HasValue && model.Input.EndTime <= model.Input.StartTime)
        {
            ModelState.AddModelError("Input.EndTime", "End time must be after start time.");
        }

        model.Exceptions = await _scheduleService.GetExceptionsAsync(doctor.Id);

        if (!ModelState.IsValid)
        {
            return View("Exceptions", model);
        }

        var result = await _scheduleService.AddExceptionAsync(doctor.Id, model.Input);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View("Exceptions", model);
        }

        TempData["Success"] = "Exception added.";
        return RedirectToAction("Exceptions");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteException(int id)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var result = await _scheduleService.DeleteExceptionAsync(doctor.Id, id);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "Exception deleted."
            : string.Join(" ", result.Errors);

        return RedirectToAction("Exceptions");
    }
}
