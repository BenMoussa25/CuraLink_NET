using CuraLink.Application.DTOs;

namespace CuraLink.Web.Areas.Doctor.ViewModels;

public class PrescriptionCreateViewModel
{
    public PrescriptionInput Input { get; set; } = new()
    {
        Items = new List<PrescriptionItemInput> { new() }
    };

    public IReadOnlyList<ConsultationDto> Consultations { get; set; } = new List<ConsultationDto>();
}
