using CuraLink.Application.DTOs;
using CuraLink.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CuraLink.Web.Areas.Doctor.ViewModels;

public class DocumentUploadViewModel
{
    [Required]
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }

    [Required]
    public DocumentType DocumentType { get; set; }

    [Required]
    public IFormFile? File { get; set; }

    public IReadOnlyList<PatientSummaryDto> Patients { get; set; } = new List<PatientSummaryDto>();
}
