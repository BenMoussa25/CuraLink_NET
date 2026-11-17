using CuraLink.Domain.Common;
using CuraLink.Domain.Entities;
using CuraLink.Domain.Enums;
using CuraLink.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CuraLink.Infrastructure.Persistence;

public static class DbInitializer
{
    private const string AdminEmail = "amina.mansouri@curalink.example";
    private const string FallbackPassword = "Demo@12345";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                await InitializeOnceAsync(services);
                return;
            }
            catch (Exception ex) when (attempt < 29 && IsTransientSqlFailure(ex))
            {
                logger.LogWarning(ex, "Database initialization attempt {Attempt} failed; retrying.", attempt + 1);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }

    private static bool IsTransientSqlFailure(Exception ex)
    {
        var sqlException = ex as SqlException ?? (ex as DbUpdateException)?.InnerException as SqlException;
        return sqlException is not null && sqlException.Number is -1 or 1205 or 4060 or 10054 or 18456;
    }

    private static async Task InitializeOnceAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var context = scopedServices.GetRequiredService<ApplicationDbContext>();
        var configuration = scopedServices.GetRequiredService<IConfiguration>();
        var environment = scopedServices.GetRequiredService<IWebHostEnvironment>();
        var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        await IdentitySeeder.EnsureRolesAsync(roleManager);

        var now = DateTime.UtcNow;
        var password = configuration["Curalink:DemoPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "Curalink:DemoPassword must be configured when database seeding runs outside the Development environment.");
            }

            password = FallbackPassword;
        }

        async Task<ApplicationUser> GetOrCreateUserAsync(string email, string firstName, string lastName, string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                return existing;
            }

            return await IdentitySeeder.CreateUserAsync(userManager, email, firstName, lastName, password, role, now);
        }

        var specialtySpecs = new[]
        {
            new { Name = "Cardiology", Description = "Fictional demo specialty for heart care." },
            new { Name = "Dermatology", Description = "Fictional demo specialty for skin care." },
            new { Name = "Pediatrics", Description = "Fictional demo specialty for child care." },
            new { Name = "General Medicine", Description = "Fictional demo specialty for general care." }
        };

        var specialties = new Dictionary<string, Specialty>(StringComparer.OrdinalIgnoreCase);
        foreach (var spec in specialtySpecs)
        {
            var specialty = await context.Specialties.SingleOrDefaultAsync(x => x.Name == spec.Name);
            if (specialty is null)
            {
                specialty = new Specialty
                {
                    Name = spec.Name,
                    Description = spec.Description,
                    IsActive = true
                };
                context.Specialties.Add(specialty);
            }

            specialties[spec.Name] = specialty;
        }

        await context.SaveChangesAsync();

        var adminUser = await GetOrCreateUserAsync(AdminEmail, "Amina", "Mansouri", AppRoles.Admin);

        var doctorSpecs = new[]
        {
            new
            {
                Email = "sara.benali@curalink.example",
                FirstName = "Sara",
                LastName = "Benali",
                Specialty = "Cardiology",
                Status = DoctorVerificationStatus.Approved,
                Biography = "Fictional cardiologist with fictional training and fictional clinic hours.",
                Fee = (decimal?)120m
            },
            new
            {
                Email = "karim.slimani@curalink.example",
                FirstName = "Karim",
                LastName = "Slimani",
                Specialty = "Dermatology",
                Status = DoctorVerificationStatus.Approved,
                Biography = "Fictional dermatologist with fictional training and fictional clinic hours.",
                Fee = (decimal?)100m
            },
            new
            {
                Email = "nadia.fekih@curalink.example",
                FirstName = "Nadia",
                LastName = "Fekih",
                Specialty = "Pediatrics",
                Status = DoctorVerificationStatus.Pending,
                Biography = "Fictional pediatrician awaiting fictional verification.",
                Fee = (decimal?)null
            }
        };

        var doctors = new List<Doctor>();
        var doctorUsers = new List<ApplicationUser>();
        foreach (var spec in doctorSpecs)
        {
            var user = await GetOrCreateUserAsync(spec.Email, spec.FirstName, spec.LastName, AppRoles.Doctor);
            doctorUsers.Add(user);

            var doctor = await context.Doctors.SingleOrDefaultAsync(x => x.UserId == user.Id);
            if (doctor is null)
            {
                doctor = new Doctor
                {
                    UserId = user.Id,
                    SpecialtyId = specialties[spec.Specialty].Id,
                    Biography = spec.Biography,
                    ClinicName = "Fictional CuraLink Clinic",
                    ClinicAddress = "Fictional Street 12, Tunis",
                    City = "Tunis",
                    PublicPhone = "+216 00 000 000",
                    ConsultationFee = spec.Fee,
                    CurrencyCode = "TND",
                    VerificationStatus = spec.Status,
                    VerifiedAtUtc = spec.Status == DoctorVerificationStatus.Approved ? now : null,
                    CreatedAtUtc = now
                };
                context.Doctors.Add(doctor);
            }

            doctors.Add(doctor);
        }

        await context.SaveChangesAsync();

        var patientSpecs = new[]
        {
            new { Email = "amine.trabelsi@curalink.example", FirstName = "Amine", LastName = "Trabelsi", DateOfBirth = (DateTime?)new DateTime(1990, 3, 14) },
            new { Email = "ines.bouzid@curalink.example", FirstName = "Ines", LastName = "Bouzid", DateOfBirth = (DateTime?)new DateTime(1988, 7, 2) },
            new { Email = "yassine.gharbi@curalink.example", FirstName = "Yassine", LastName = "Gharbi", DateOfBirth = (DateTime?)new DateTime(1995, 11, 23) }
        };

        var patients = new List<Patient>();
        var patientUsers = new List<ApplicationUser>();
        foreach (var spec in patientSpecs)
        {
            var user = await GetOrCreateUserAsync(spec.Email, spec.FirstName, spec.LastName, AppRoles.Patient);
            patientUsers.Add(user);

            var patient = await context.Patients.SingleOrDefaultAsync(x => x.UserId == user.Id);
            if (patient is null)
            {
                patient = new Patient
                {
                    UserId = user.Id,
                    DateOfBirth = spec.DateOfBirth,
                    Address = "Fictional Address 7, Tunis",
                    EmergencyContactName = "Fictional Emergency Contact",
                    EmergencyContactPhone = "+216 00 000 001",
                    CreatedAtUtc = now
                };
                context.Patients.Add(patient);
            }

            patients.Add(patient);
        }

        await context.SaveChangesAsync();

        foreach (var doctor in doctors.Where(x => x.VerificationStatus == DoctorVerificationStatus.Approved))
        {
            foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday })
            {
                if (await context.DoctorSchedules.AnyAsync(x => x.DoctorId == doctor.Id && x.DayOfWeek == day))
                {
                    continue;
                }

                context.DoctorSchedules.Add(new DoctorSchedule
                {
                    DoctorId = doctor.Id,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    SlotDurationMinutes = 30,
                    IsActive = true
                });
            }
        }

        await context.SaveChangesAsync();

        var conferenceReason = "Fictional conference.";
        if (!await context.ScheduleExceptions.AnyAsync(x => x.DoctorId == doctors[0].Id && x.Reason == conferenceReason))
        {
            context.ScheduleExceptions.Add(new ScheduleException
            {
                DoctorId = doctors[0].Id,
                Date = now.Date.AddDays(7),
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(12, 0, 0),
                IsUnavailable = true,
                Reason = conferenceReason
            });
        }

        await context.SaveChangesAsync();

        async Task<Appointment> GetOrCreateAppointmentAsync(int doctorId, int patientId, DateTime startUtc, AppointmentStatus status)
        {
            var existing = await context.Appointments.SingleOrDefaultAsync(x =>
                x.DoctorId == doctorId && x.PatientId == patientId && x.Status == status);
            if (existing is not null)
            {
                return existing;
            }

            var appointment = new Appointment
            {
                DoctorId = doctorId,
                PatientId = patientId,
                StartAtUtc = startUtc,
                EndAtUtc = startUtc.AddMinutes(30),
                Status = status,
                PatientReason = "Fictional reason for a demo appointment.",
                CreatedAtUtc = now.AddDays(-14),
                UpdatedAtUtc = status == AppointmentStatus.Pending ? null : now.AddDays(-13)
            };
            context.Appointments.Add(appointment);
            return appointment;
        }

        var pendingAppointment = await GetOrCreateAppointmentAsync(doctors[0].Id, patients[0].Id, now.Date.AddDays(2).AddHours(9), AppointmentStatus.Pending);
        var confirmedAppointment = await GetOrCreateAppointmentAsync(doctors[0].Id, patients[1].Id, now.Date.AddDays(3).AddHours(10), AppointmentStatus.Confirmed);
        var rejectedAppointment = await GetOrCreateAppointmentAsync(doctors[1].Id, patients[2].Id, now.Date.AddDays(-4).AddHours(11), AppointmentStatus.Rejected);
        var cancelledAppointment = await GetOrCreateAppointmentAsync(doctors[1].Id, patients[0].Id, now.Date.AddDays(-3).AddHours(14), AppointmentStatus.Cancelled);
        var completedAppointment = await GetOrCreateAppointmentAsync(doctors[0].Id, patients[2].Id, now.Date.AddDays(-7).AddHours(9), AppointmentStatus.Completed);
        var noShowAppointment = await GetOrCreateAppointmentAsync(doctors[1].Id, patients[1].Id, now.Date.AddDays(-5).AddHours(15), AppointmentStatus.NoShow);

        await context.SaveChangesAsync();

        var appointments = new[]
        {
            pendingAppointment,
            confirmedAppointment,
            rejectedAppointment,
            cancelledAppointment,
            completedAppointment,
            noShowAppointment
        };

        foreach (var appointment in appointments.Where(x => x.Status != AppointmentStatus.Pending))
        {
            if (await context.AppointmentStatusHistories.AnyAsync(x => x.AppointmentId == appointment.Id && x.NewStatus == appointment.Status))
            {
                continue;
            }

            context.AppointmentStatusHistories.Add(new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                OldStatus = AppointmentStatus.Pending,
                NewStatus = appointment.Status,
                ChangedByUserId = doctorUsers[0].Id,
                ChangedAtUtc = appointment.CreatedAtUtc.AddDays(1),
                Reason = "Fictional demo status change."
            });
        }

        await context.SaveChangesAsync();

        var consultation = await context.Consultations.SingleOrDefaultAsync(x => x.AppointmentId == completedAppointment.Id);
        if (consultation is null)
        {
            consultation = new Consultation
            {
                AppointmentId = completedAppointment.Id,
                PatientId = completedAppointment.PatientId,
                DoctorId = completedAppointment.DoctorId,
                Reason = "Fictional follow-up consultation.",
                ReportedSymptoms = "Fictional mild symptoms for demonstration only.",
                ClinicalObservations = "Fictional routine observations for demonstration only.",
                Diagnosis = "Fictional demo diagnosis.",
                TreatmentPlan = "Fictional demo treatment plan.",
                Recommendations = "Fictional rest and hydration advice.",
                FollowUpDate = now.AddDays(30),
                Status = ConsultationStatus.Finalized,
                IsSharedWithPatient = true,
                CreatedAtUtc = completedAppointment.StartAtUtc,
                UpdatedAtUtc = completedAppointment.StartAtUtc.AddMinutes(20),
                FinalizedAtUtc = completedAppointment.StartAtUtc.AddMinutes(25)
            };
            context.Consultations.Add(consultation);
            await context.SaveChangesAsync();
        }

        var prescription = await context.Prescriptions.SingleOrDefaultAsync(x => x.ConsultationId == consultation.Id);
        if (prescription is null)
        {
            prescription = new Prescription
            {
                ConsultationId = consultation.Id,
                PatientId = consultation.PatientId,
                DoctorId = consultation.DoctorId,
                IssuedAtUtc = consultation.FinalizedAtUtc ?? now,
                GeneralInstructions = "Fictional demo instructions for demonstration only.",
                Status = PrescriptionStatus.Issued,
                IsSharedWithPatient = true
            };
            context.Prescriptions.Add(prescription);
            await context.SaveChangesAsync();
        }

        var prescriptionItemSpecs = new[]
        {
            new { MedicineName = "Fictional Medicine Alpha", Strength = "10 mg", DosageInstructions = "Fictional dosage instructions for demonstration only.", Frequency = "Once daily", Duration = "5 days", Route = "Oral" },
            new { MedicineName = "Fictional Medicine Beta", Strength = "5 mg", DosageInstructions = "Fictional dosage instructions for demonstration only.", Frequency = "Twice daily", Duration = "3 days", Route = "Oral" }
        };

        foreach (var spec in prescriptionItemSpecs)
        {
            if (await context.PrescriptionItems.AnyAsync(x => x.PrescriptionId == prescription.Id && x.MedicineName == spec.MedicineName))
            {
                continue;
            }

            context.PrescriptionItems.Add(new PrescriptionItem
            {
                PrescriptionId = prescription.Id,
                MedicineName = spec.MedicineName,
                Strength = spec.Strength,
                DosageInstructions = spec.DosageInstructions,
                Frequency = spec.Frequency,
                Duration = spec.Duration,
                Route = spec.Route
            });
        }

        await context.SaveChangesAsync();

        var medicalRecord = await context.MedicalRecords.SingleOrDefaultAsync(x => x.PatientId == patients[0].Id);
        if (medicalRecord is null)
        {
            medicalRecord = new MedicalRecord
            {
                PatientId = patients[0].Id,
                CreatedAtUtc = now.AddDays(-30),
                UpdatedAtUtc = now.AddDays(-2)
            };
            context.MedicalRecords.Add(medicalRecord);
            await context.SaveChangesAsync();
        }

        if (!await context.Allergies.AnyAsync(x => x.MedicalRecordId == medicalRecord.Id && x.Name == "Fictional pollen allergy"))
        {
            context.Allergies.Add(new Allergy
            {
                MedicalRecordId = medicalRecord.Id,
                Name = "Fictional pollen allergy",
                Reaction = "Fictional mild reaction",
                Notes = "Fictional demo note."
            });
        }

        if (!await context.MedicalConditions.AnyAsync(x => x.MedicalRecordId == medicalRecord.Id && x.Name == "Fictional seasonal rhinitis"))
        {
            context.MedicalConditions.Add(new MedicalCondition
            {
                MedicalRecordId = medicalRecord.Id,
                Name = "Fictional seasonal rhinitis",
                Notes = "Fictional demo note.",
                RecordedAtUtc = now.AddDays(-30),
                RecordedByDoctorId = doctors[0].Id
            });
        }

        if (!await context.PatientNotes.AnyAsync(x => x.PatientId == patients[1].Id && x.Title == "Fictional follow-up reminder"))
        {
            context.PatientNotes.Add(new PatientNote
            {
                PatientId = patients[1].Id,
                Title = "Fictional follow-up reminder",
                Content = "Fictional demo note content for demonstration only.",
                CreatedAtUtc = now.AddDays(-1)
            });
        }

        await context.SaveChangesAsync();

        var notificationSpecs = new[]
        {
            new { UserId = patientUsers[0].Id, Message = "Your fictional appointment request is awaiting review.", Type = NotificationType.AppointmentRequest, RelatedEntityType = "Appointment", RelatedEntityId = pendingAppointment.Id.ToString(), CreatedAtUtc = now.AddDays(-1) },
            new { UserId = patientUsers[1].Id, Message = "Your fictional appointment request was confirmed.", Type = NotificationType.AppointmentConfirmed, RelatedEntityType = "Appointment", RelatedEntityId = confirmedAppointment.Id.ToString(), CreatedAtUtc = now },
            new { UserId = doctorUsers[0].Id, Message = "A new fictional appointment request is awaiting review.", Type = NotificationType.AppointmentRequest, RelatedEntityType = "Appointment", RelatedEntityId = pendingAppointment.Id.ToString(), CreatedAtUtc = now.AddDays(-1) },
            new { UserId = patientUsers[2].Id, Message = "A fictional prescription was issued for you.", Type = NotificationType.PrescriptionIssued, RelatedEntityType = "Prescription", RelatedEntityId = prescription.Id.ToString(), CreatedAtUtc = now },
            new { UserId = doctorUsers[1].Id, Message = "A fictional document was shared with you.", Type = NotificationType.DocumentShared, RelatedEntityType = "Consultation", RelatedEntityId = consultation.Id.ToString(), CreatedAtUtc = now }
        };

        foreach (var spec in notificationSpecs)
        {
            if (await context.Notifications.AnyAsync(x => x.UserId == spec.UserId && x.Message == spec.Message))
            {
                continue;
            }

            context.Notifications.Add(new Notification
            {
                UserId = spec.UserId,
                Message = spec.Message,
                Type = spec.Type,
                RelatedEntityType = spec.RelatedEntityType,
                RelatedEntityId = spec.RelatedEntityId,
                CreatedAtUtc = spec.CreatedAtUtc
            });
        }

        await context.SaveChangesAsync();

        if (!await context.AuditLogs.AnyAsync(x => x.ActorUserId == adminUser.Id && x.Action == "DemoSeed" && x.EntityType == "System"))
        {
            context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = adminUser.Id,
                Action = "DemoSeed",
                EntityType = "System",
                OccurredAtUtc = now
            });
            await context.SaveChangesAsync();
        }
    }
}
