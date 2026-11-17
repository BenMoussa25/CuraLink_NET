using CuraLink.Application.DTOs;

namespace CuraLink.Web.Areas.Doctor.ViewModels;

public class DoctorProfileEditViewModel
{
    public DoctorProfileInput Input { get; set; } = new();

    public DoctorProfileDto? Profile { get; set; }

    public IReadOnlyList<SpecialtyDto> Specialties { get; set; } = new List<SpecialtyDto>();
}
