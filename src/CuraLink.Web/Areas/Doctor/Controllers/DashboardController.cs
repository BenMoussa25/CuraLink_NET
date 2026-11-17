using CuraLink.Application.Interfaces;
using CuraLink.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.Web.Areas.Doctor.Controllers;

[Area("Doctor")]
[Authorize(Roles = AppRoles.Doctor)]
public class DashboardController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IDoctorDashboardService _dashboardService;

    public DashboardController(IDoctorService doctorService, IDoctorDashboardService dashboardService)
    {
        _doctorService = doctorService;
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var model = await _dashboardService.GetDashboardAsync(doctor.Id, DateTime.UtcNow);
        return View(model);
    }
}
