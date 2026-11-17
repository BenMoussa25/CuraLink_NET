using CuraLink.Application.Interfaces;
using CuraLink.Domain.Common;
using CuraLink.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace CuraLink.Web.Filters;

public class ApprovedDoctorFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var area = context.RouteData.Values["area"] as string;
        var controller = context.RouteData.Values["controller"] as string;

        if (!string.Equals(area, "Doctor", StringComparison.OrdinalIgnoreCase)
            || string.Equals(controller, "Profile", StringComparison.OrdinalIgnoreCase)
            || !context.HttpContext.User.IsInRole(AppRoles.Doctor))
        {
            await next();
            return;
        }

        var doctorService = context.HttpContext.RequestServices.GetRequiredService<IDoctorService>();
        var doctor = await doctorService.GetCurrentDoctorAsync();
        if (doctor is null || doctor.VerificationStatus != DoctorVerificationStatus.Approved)
        {
            context.Result = new RedirectResult("/Doctor/Profile/Index");
            return;
        }

        await next();
    }
}
