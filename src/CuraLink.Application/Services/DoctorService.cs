using CuraLink.Application.Common;
using CuraLink.Application.DTOs;
using CuraLink.Application.Interfaces;
using CuraLink.Domain.Entities;
using CuraLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Services;

public sealed class DoctorService : IDoctorService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DoctorService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Doctor?> GetByUserIdAsync(string userId)
    {
        return await _context.Doctors
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<Doctor?> GetCurrentDoctorAsync()
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return await _context.Doctors
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<Result<DoctorProfileDto>> GetProfileAsync(int doctorId)
    {
        var profile = await _context.Doctors
            .AsNoTracking()
            .Where(x => x.Id == doctorId)
            .Select(x => new DoctorProfileDto
            {
                Id = x.Id,
                UserId = x.UserId,
                FirstName = x.User.FirstName,
                LastName = x.User.LastName,
                Email = x.User.Email ?? string.Empty,
                Biography = x.Biography,
                ClinicName = x.ClinicName,
                ClinicAddress = x.ClinicAddress,
                City = x.City,
                PublicPhone = x.PublicPhone,
                ConsultationFee = x.ConsultationFee,
                CurrencyCode = x.CurrencyCode,
                SpecialtyId = x.SpecialtyId,
                SpecialtyName = x.Specialty.Name,
                VerificationStatus = x.VerificationStatus,
                ProfileImagePath = x.ProfileImagePath
            })
            .SingleOrDefaultAsync();

        return profile is null
            ? Result<DoctorProfileDto>.Failure("Doctor not found.")
            : Result<DoctorProfileDto>.Ok(profile);
    }

    public async Task<Result> UpdateProfileAsync(int doctorId, DoctorProfileInput input)
    {
        var doctor = await _context.Doctors
            .SingleOrDefaultAsync(x => x.Id == doctorId);
        if (doctor is null)
        {
            return Result.Failure("Doctor not found.");
        }

        if (doctor.SpecialtyId != input.SpecialtyId)
        {
            var specialtyIsActive = await _context.Specialties
                .AsNoTracking()
                .AnyAsync(x => x.Id == input.SpecialtyId && x.IsActive);
            if (!specialtyIsActive)
            {
                return Result.Failure("Specialty not found or inactive.");
            }

            doctor.SpecialtyId = input.SpecialtyId;
            doctor.VerificationStatus = DoctorVerificationStatus.Pending;
            doctor.VerifiedAtUtc = null;
        }

        doctor.Biography = input.Biography;
        doctor.ClinicName = input.ClinicName;
        doctor.ClinicAddress = input.ClinicAddress;
        doctor.City = input.City;
        doctor.PublicPhone = input.PublicPhone;
        doctor.ConsultationFee = input.ConsultationFee;
        doctor.CurrencyCode = input.CurrencyCode;
        doctor.ProfileImagePath = input.ProfileImagePath;

        await _context.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<IReadOnlyList<SpecialtyDto>> GetActiveSpecialtiesAsync()
    {
        return await _context.Specialties
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new SpecialtyDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }
}
