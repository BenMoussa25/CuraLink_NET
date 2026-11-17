using CuraLink.Application.Common;
using CuraLink.Application.DTOs;
using CuraLink.Application.Interfaces;
using CuraLink.Domain.Entities;
using CuraLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Services;

public sealed class DocumentService : IDocumentService
{
    private const long MaxFileSizeBytes = 10L * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx"
    };

    private readonly IApplicationDbContext _context;
    private readonly IFileStorage _fileStorage;

    public DocumentService(IApplicationDbContext context, IFileStorage fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<IReadOnlyList<MedicalDocumentDto>> GetForDoctorAsync(int doctorId)
    {
        var documents = await _context.MedicalDocuments
            .AsNoTracking()
            .Include(x => x.Patient).ThenInclude(x => x.User)
            .Where(x => x.DoctorId == doctorId ||
                        (x.VisibilityStatus == VisibilityStatus.SharedWithDoctor &&
                         _context.Appointments.Any(a => a.DoctorId == doctorId && a.PatientId == x.PatientId)))
            .OrderByDescending(x => x.UploadedAtUtc)
            .ToListAsync();

        return documents.Select(Map).ToList();
    }

    public async Task<Result<int>> UploadForDoctorAsync(
        int doctorId,
        int patientId,
        string fileName,
        string contentType,
        Stream content,
        DocumentType documentType,
        string? actorUserId,
        DateTime nowUtc)
    {
        var hasRelationship = await _context.Appointments
            .AsNoTracking()
            .AnyAsync(x => x.DoctorId == doctorId && x.PatientId == patientId);
        if (!hasRelationship)
        {
            return Result<int>.Failure("Patient is not associated with this doctor.");
        }

        var extension = Path.GetExtension(fileName ?? string.Empty);
        if (!AllowedExtensions.Contains(extension))
        {
            return Result<int>.Failure("Unsupported file extension.");
        }

        var fileSizeBytes = content.CanSeek ? content.Length : 0;
        if (fileSizeBytes > MaxFileSizeBytes)
        {
            return Result<int>.Failure("File exceeds the 10 MB limit.");
        }

        var safeFileName = SanitizeFileName(fileName ?? string.Empty, extension);
        var storageKey = await _fileStorage.SaveAsync(content, safeFileName);

        var document = new MedicalDocument
        {
            PatientId = patientId,
            UploadedByUserId = actorUserId ?? string.Empty,
            DoctorId = doctorId,
            DocumentType = documentType,
            OriginalFileName = safeFileName,
            StoredFileName = storageKey,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            StorageKey = storageKey,
            VisibilityStatus = VisibilityStatus.SharedWithPatient,
            UploadedAtUtc = nowUtc
        };

        _context.MedicalDocuments.Add(document);
        await _context.SaveChangesAsync();
        return Result<int>.Ok(document.Id);
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> DownloadForDoctorAsync(int doctorId, int documentId)
    {
        var document = await _context.MedicalDocuments
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == documentId &&
                                       (x.DoctorId == doctorId ||
                                        (x.VisibilityStatus == VisibilityStatus.SharedWithDoctor &&
                                         _context.Appointments.Any(a => a.DoctorId == doctorId && a.PatientId == x.PatientId))));
        if (document is null)
        {
            return null;
        }

        var stream = await _fileStorage.OpenReadAsync(document.StorageKey);
        if (stream is null)
        {
            return null;
        }

        return (stream, document.ContentType, document.OriginalFileName);
    }

    private static string SanitizeFileName(string fileName, string extension)
    {
        var name = Path.GetFileName(fileName ?? string.Empty);
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(invalidChar, '_');
        }

        name = name.Replace("..", "_").Replace("/", "_").Replace("\\", "_");
        if (string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(name)))
        {
            name = "document" + extension;
        }

        return name;
    }

    private static MedicalDocumentDto Map(MedicalDocument document)
    {
        return new MedicalDocumentDto
        {
            Id = document.Id,
            PatientId = document.PatientId,
            PatientName = $"{document.Patient.User.FirstName} {document.Patient.User.LastName}".Trim(),
            DocumentType = document.DocumentType,
            OriginalFileName = document.OriginalFileName,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            VisibilityStatus = document.VisibilityStatus,
            UploadedAtUtc = document.UploadedAtUtc
        };
    }
}
