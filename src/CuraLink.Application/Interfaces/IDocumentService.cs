using CuraLink.Application.Common;
using CuraLink.Application.DTOs;
using CuraLink.Domain.Enums;

namespace CuraLink.Application.Interfaces;

public interface IDocumentService
{
    Task<IReadOnlyList<MedicalDocumentDto>> GetForDoctorAsync(int doctorId);
    Task<Result<int>> UploadForDoctorAsync(int doctorId, int patientId, string fileName, string contentType, Stream content, DocumentType documentType, string? actorUserId, DateTime nowUtc);
    Task<(Stream Stream, string ContentType, string FileName)?> DownloadForDoctorAsync(int doctorId, int documentId);
}
