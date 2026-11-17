using CuraLink.Application.Interfaces;
using CuraLink.Domain.Common;
using CuraLink.Domain.Enums;
using CuraLink.Web.Areas.Doctor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuraLink.Web.Areas.Doctor.Controllers;

[Area("Doctor")]
[Authorize(Roles = AppRoles.Doctor)]
public class DocumentsController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IDocumentService _documentService;
    private readonly IPatientService _patientService;
    private readonly ICurrentUserService _currentUserService;

    public DocumentsController(
        IDoctorService doctorService,
        IDocumentService documentService,
        IPatientService patientService,
        ICurrentUserService currentUserService)
    {
        _doctorService = doctorService;
        _documentService = documentService;
        _patientService = patientService;
        _currentUserService = currentUserService;
    }

    public async Task<IActionResult> Index()
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var model = await _documentService.GetForDoctorAsync(doctor.Id);
        return View(model);
    }

    public async Task<IActionResult> Upload()
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        return View(new DocumentUploadViewModel
        {
            Patients = await _patientService.GetDoctorPatientsAsync(doctor.Id)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(DocumentUploadViewModel model)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        if (model.File is null || model.File.Length == 0)
        {
            ModelState.AddModelError(nameof(model.File), "Choose a file to upload.");
        }

        if (!ModelState.IsValid)
        {
            model.Patients = await _patientService.GetDoctorPatientsAsync(doctor.Id);
            return View(model);
        }

        var fileName = Path.GetFileName(model.File!.FileName);
        var contentType = string.IsNullOrWhiteSpace(model.File.ContentType)
            ? "application/octet-stream"
            : model.File.ContentType;

        using var stream = model.File.OpenReadStream();
        var result = await _documentService.UploadForDoctorAsync(
            doctor.Id,
            model.PatientId,
            fileName,
            contentType,
            stream,
            model.DocumentType,
            _currentUserService.UserId,
            DateTime.UtcNow);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            model.Patients = await _patientService.GetDoctorPatientsAsync(doctor.Id);
            return View(model);
        }

        TempData["Success"] = "Document uploaded.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id)
    {
        var doctor = await _doctorService.GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Home", new { area = "" });
        }

        var download = await _documentService.DownloadForDoctorAsync(doctor.Id, id);
        if (download is null)
        {
            TempData["Error"] = "Document not found.";
            return RedirectToAction("Index");
        }

        return File(download.Value.Stream, download.Value.ContentType, download.Value.FileName);
    }
}
