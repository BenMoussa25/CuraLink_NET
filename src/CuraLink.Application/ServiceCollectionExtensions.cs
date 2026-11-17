using CuraLink.Application.Interfaces;
using CuraLink.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CuraLink.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IConsultationService, ConsultationService>();
        services.AddScoped<IPrescriptionService, PrescriptionService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDoctorDashboardService, DoctorDashboardService>();
        return services;
    }
}
