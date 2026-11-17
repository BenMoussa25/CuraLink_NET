# CuraLink --- Doctor & Patient Management System

> **Project type:** University web application\
> **Primary stack:** C#, ASP.NET Core MVC, Entity Framework Core, SQL
> Server, ASP.NET Core Identity, Bootstrap 5\
> **Project status:** Initial development specification\
> **Working title/tagline:** CuraLink --- Connecting Doctors.
> Simplifying Healthcare.

------------------------------------------------------------------------

## 1. Instructions for the AI Development Agent

You are the lead software engineer responsible for implementing
**CuraLink**, a secure, maintainable, beginner-friendly healthcare
management web application.

Treat this README as the source of truth for the initial implementation.

### Required working approach

1.  Inspect the existing repository before changing anything. If it is
    empty, create the solution and project structure described below.
2.  Implement the application in small, verifiable phases. Do not
    generate the entire system in one unreviewable step.
3.  Before each phase, briefly state the files and functionality you
    intend to add.
4.  After each phase, build the solution and run relevant tests. Fix
    errors before proceeding.
5.  Do not leave core features as fake buttons, hard-coded demo data, or
    TODO-only implementations.
6.  Use dependency injection, asynchronous database operations,
    validation, clear naming, and separation of concerns.
7.  Keep secrets out of source control. Provide an `.env.example` only
    if a selected configuration library actually uses it; otherwise use
    .NET user secrets and environment variables.
8.  Do not introduce microservices, Kubernetes, CQRS frameworks, or
    unnecessary abstractions. This is a university-scale modular
    monolith.
9.  If a requirement is ambiguous, choose the simplest safe
    implementation, document the assumption, and continue. Ask only when
    a decision blocks progress.
10. Update this README or add `docs/` documentation when implementation
    decisions materially change.

### Definition of done

A feature is complete only when: - Its UI is connected to real backend
logic. - Server-side validation is present. - Authorization is enforced
on the server, not only by hiding UI elements. - Database changes are
represented by EF Core migrations. - Relevant automated tests pass. -
Empty, loading, success, and error states are handled where
appropriate. - No sensitive information is written to logs or exposed to
unauthorized users.

------------------------------------------------------------------------

## 2. Product Overview

CuraLink is a web platform that connects patients with doctors and helps
medical practices manage appointments and patient records.

Patients can discover doctors by specialty and location, view
professional profiles, request appointments, and access information that
has been shared with them.

Doctors can manage their professional profiles, schedules, appointments,
patients, consultation records, prescriptions, and medical documents.

Administrators manage platform accounts, doctor verification, and
medical specialties. Administrators must not automatically receive
access to private clinical records.

### 2.1 Product goals

-   Make it easier for patients to find doctors.
-   Simplify appointment scheduling.
-   Give doctors an organized workspace for their patients.
-   Keep consultation records and prescriptions structured.
-   Give patients a clear view of their appointments and shared medical
    documents.
-   Protect sensitive health information through strict access control
    and auditability.

### 2.2 Out of scope for the first release

Do not implement these unless explicitly requested after the MVP is
complete:

-   Video consultations or telemedicine.
-   Online payments or insurance claims.
-   Pharmacy integrations or electronic transmission of prescriptions.
-   AI diagnosis, treatment recommendations, or symptom triage.
-   Hospital information-system integrations.
-   Public patient reviews and doctor ratings.
-   Native mobile applications.
-   Multi-clinic enterprise management.

CuraLink is an administrative and record-management tool. It must not
claim to diagnose patients or replace professional medical judgment.

------------------------------------------------------------------------

## 3. User Roles and Permissions

Use ASP.NET Core Identity with role-based authorization and
resource-level ownership checks.

### 3.1 Roles

#### Administrator

Can: - View platform-level operational statistics. - Create, edit,
activate, and deactivate user accounts where appropriate. - Review
doctor registration and verification requests. - Manage medical
specialties. - View audit events needed for platform administration. -
Manage non-clinical platform settings.

Cannot by default: - Read consultation notes, diagnoses, prescriptions,
patient notes, or other clinical content. - Edit clinical records. -
Impersonate doctors or patients.

#### Doctor

Can: - Manage their own professional profile. - Submit professional
details and verification documents. - Manage their own availability and
working schedule. - View and manage appointments assigned to them. -
Access patient records only when an appropriate care relationship or
appointment authorization exists. - Create consultation records
associated with their appointments. - Create prescriptions associated
with consultations. - Upload and manage documents they are authorized to
handle. - View their own operational dashboard.

Cannot: - Access another doctor's private patients or appointments
without an explicit permitted relationship. - Change patient account
credentials. - Read another doctor's private consultation notes.

#### Patient

Can: - Manage their own basic profile. - Search and view public doctor
profiles. - Request appointments with doctors. - View and manage their
own appointments, subject to business rules. - View consultations,
prescriptions, and documents explicitly shared with them. - Create
private personal notes and reminders. - Manage notification preferences.

Cannot: - Read another patient's data. - Edit a doctor's clinical
consultation or prescription. - Access internal doctor-only notes.

### 3.2 Authorization rule

Never rely only on navigation visibility or frontend checks. Every
protected controller action and data query must verify the authenticated
user's role and ownership.

Use resource-based authorization or equivalent service-layer checks for
sensitive records. Avoid fetching a record by ID and returning it before
checking whether the current user is allowed to access it.

------------------------------------------------------------------------

## 4. Technology Stack

Use supported, mutually compatible stable releases available in the
development environment. Do not arbitrarily mix preview packages with
stable packages.

  Layer             Technology
  ----------------- ------------------------------------------------
  Language          C#
  Framework         ASP.NET Core MVC
  Runtime           Current supported .NET LTS release
  ORM               Entity Framework Core
  Database          SQL Server
  Authentication    ASP.NET Core Identity
  Frontend          Razor Views, HTML5, CSS3, JavaScript
  UI framework      Bootstrap 5
  Icons             Bootstrap Icons or another consistent icon set
  Validation        Data Annotations and server-side validation
  Testing           xUnit, ASP.NET Core integration testing
  Version control   Git
  IDE               Visual Studio or Visual Studio Code

### 4.1 Architectural style

Build a **modular monolith** using a layered structure:

-   Presentation: MVC controllers, Razor views, view models, filters.
-   Application: business services, DTOs, validation, authorization
    policies.
-   Domain: core entities and business concepts.
-   Infrastructure: EF Core, Identity, database configuration, file
    storage, email implementation.

Do not put business rules directly in Razor views or large controller
actions.

### 4.2 Suggested solution structure

``` text
CuraLink/
├── CuraLink.sln
├── src/
│   ├── CuraLink.Web/
│   │   ├── Controllers/
│   │   ├── Areas/
│   │   │   ├── Admin/
│   │   │   ├── Doctor/
│   │   │   └── Patient/
│   │   ├── Views/
│   │   ├── ViewModels/
│   │   ├── wwwroot/
│   │   │   ├── css/
│   │   │   ├── js/
│   │   │   ├── images/
│   │   │   └── uploads/       # Do not expose private medical files here
│   │   ├── Middleware/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── CuraLink.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   └── Common/
│   ├── CuraLink.Application/
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   ├── DTOs/
│   │   ├── Validators/
│   │   └── Authorization/
│   └── CuraLink.Infrastructure/
│       ├── Persistence/
│       │   ├── ApplicationDbContext.cs
│       │   ├── Configurations/
│       │   └── Migrations/
│       ├── Identity/
│       ├── FileStorage/
│       └── Email/
├── tests/
│   ├── CuraLink.UnitTests/
│   └── CuraLink.IntegrationTests/
├── docs/
│   ├── architecture.md
│   ├── database.md
│   └── security-and-privacy.md
├── .gitignore
└── README.md
```

The agent may adjust the structure if there is a clear technical reason,
but must preserve separation of responsibilities.

------------------------------------------------------------------------

## 5. Functional Requirements

## 5.1 Public Website

Create a responsive public-facing website with:

-   Home page explaining CuraLink.
-   Navigation to doctor search, login, and registration.
-   Doctor search page.
-   Public doctor profile page.
-   About page.
-   Contact/help page (informational only in MVP).
-   Privacy and terms placeholder pages that clearly require review
    before production.

Public users must not see private patient information.

## 5.2 Authentication and Account Management

Implement: - Patient registration. - Doctor registration with
professional details. - Login and logout. - Password reset flow using a
development-safe email abstraction. - Email confirmation if email
delivery is configured. - Profile editing. - Role-aware redirection
after login. - Account activation/deactivation. - Secure password
hashing through ASP.NET Core Identity.

Doctor accounts must have a verification status. A newly registered
doctor should not appear in public search until approved by an
administrator.

Suggested doctor verification statuses: - Pending - Approved -
Rejected - Suspended

Do not allow public self-registration as Administrator. Seed the initial
administrator through a secure development setup process or documented
command, never through a public registration form.

## 5.3 Doctor Directory and Search

Patients and public visitors can search approved, active doctors.

Search/filter fields: - Doctor name. - Specialty. - City or location. -
Availability (if practical in MVP).

Doctor public profile: - Display name. - Profile image, if uploaded. -
Specialty. - Biography. - Clinic address. - Public contact details
selected by the doctor. - Consultation fee (optional). - Working hours
or availability summary. - Verification indicator.

Use pagination and safe query construction. Do not expose private email,
phone, identity documents, or internal verification notes.

## 5.4 Doctor Profile Management

Doctors can manage: - Professional biography. - Specialty (subject to
platform rules). - Clinic name and address. - Public phone/contact
information. - Consultation fee and currency. - Profile image. - Working
schedule.

Changes to specialty or professional credentials may require renewed
administrator approval.

## 5.5 Schedule and Availability

Doctors can define recurring working hours by day of week and
date-specific exceptions.

Suggested concepts: - DoctorSchedule: recurring day, start time, end
time, slot duration. - ScheduleException: date, unavailable or modified
time range. - Appointment: selected start and end time.

Rules: - End time must be after start time. - Appointment duration must
be positive. - Do not allow appointments outside the doctor's
availability. - Do not allow overlapping active appointments for the
same doctor. - Do not allow booking in the past. - Handle concurrent
booking attempts safely at the database/service level. - Store
appointment timestamps consistently (prefer UTC in persistence; display
in the configured local timezone). - Make the application timezone
configurable. For the initial Tunisia-oriented demo, document the chosen
timezone rather than scattering timezone assumptions through code.

## 5.6 Appointment Management

Patients can: - Select an approved doctor. - View available slots. -
Submit an appointment request. - Add a short reason for the visit. -
View appointment status. - Cancel an appointment according to
cancellation rules.

Doctors can: - View daily, weekly, and upcoming appointments. - Confirm
or reject pending requests. - Reschedule appointments through a
controlled workflow. - Cancel appointments with a reason. - Mark
appointments as completed. - Mark a patient as not attended, if this
status is included.

Suggested statuses: - Pending - Confirmed - Rejected - Cancelled -
Completed - NoShow

Business rules: - A patient cannot book the same slot twice. - A doctor
cannot have overlapping active appointments. - Only the assigned doctor
can confirm, reject, or complete the appointment. - Only the patient who
created an appointment or the assigned doctor can request cancellation,
subject to policy. - Keep status history or audit events for important
appointment changes. - A consultation can only be created for an
appropriate appointment, unless a separately documented authorized
workflow is implemented.

## 5.7 Patient Management

Doctors need a patient list based on an established relationship, such
as a confirmed/completed appointment or explicit authorized care
relationship.

Patient summary available to an authorized doctor: - Patient name. -
Date of birth (only when needed). - Contact information required for
care. - Relevant recorded allergies. - Relevant medical history. -
Previous consultations visible to that doctor under the access policy. -
Prescriptions and documents available to that doctor.

Do not make all patients globally searchable by doctors. Do not expose a
patient's complete record merely because a doctor knows the patient ID.

## 5.8 Medical Records

Create a structured patient medical record.

Suggested information: - Patient reference. - Relevant medical
history. - Known allergies. - Long-term conditions, if recorded. -
Current medication information, if recorded. - Created/updated
timestamps. - Record visibility/sharing metadata where required.

Separate patient-entered information from doctor-authored clinical
information. Clearly label the source and author of each item.

A doctor must not silently overwrite historical clinical records. Use
amendment or addendum behavior for signed/finalized records, and
preserve audit history.

## 5.9 Consultation Management

A consultation is associated with an appointment and an authorized
doctor-patient relationship.

Fields: - Appointment reference. - Patient reference. - Doctor
reference. - Consultation date. - Reason for consultation. - Symptoms as
reported. - Clinical observations. - Diagnosis entered by the doctor. -
Treatment plan. - Recommendations. - Follow-up date (optional). - Status
(Draft/Finalized, if implemented). - Created and updated timestamps.

Rules: - Only the assigned/authorized doctor can create or edit a
consultation. - Patients can view only consultation information
explicitly published/shared with them. - A finalized consultation should
not be silently overwritten. - Record who created and modified the
consultation. - Do not generate AI diagnoses or treatment advice.

## 5.10 Prescriptions

Doctors can create a prescription linked to a consultation.

Prescription fields: - Patient. - Doctor. - Consultation. - Issue
date. - General instructions. - Status (Draft/Issued/Cancelled, if
implemented). - One or more prescription items.

Prescription item fields: - Medicine name (free text for MVP). -
Strength, if known. - Dosage instructions. - Frequency. - Duration. -
Route/instructions, if relevant. - Additional notes.

Rules: - Only an authorized doctor can issue a prescription. - A patient
can view only prescriptions issued to them and shared/published. -
Issued prescriptions should be immutable or amended through a traceable
correction process. - Do not recommend medicines, calculate dosages, or
make clinical decisions automatically. - Clearly label generated
documents as records created by the application, not as a guarantee of
legal validity.

## 5.11 Medical Documents

Allow authorized users to upload and access documents such as: -
Examination reports. - Medical certificates. - Referral documents. -
Consultation summaries. - Other supporting files.

Security requirements: - Store private files outside `wwwroot` or in a
private object/file store. - Serve files through an authorized
controller/service after checking access. - Validate file size and
allowed extensions and inspect content type. - Generate server-side
filenames; never trust a user-supplied path. - Prevent path traversal. -
Do not execute uploaded files. - Do not log file contents or sensitive
document names unnecessarily. - Use fictional files during development
and demos.

## 5.12 Patient Personal Notes and Reminders

Patients can create private personal notes: - Title. - Content. -
Created date. - Optional reminder date. - Optional completion state.

These notes are private to the patient by default and are not part of
the official clinical record. A patient may choose to share a note with
a doctor through an explicit, recorded action if this feature is
implemented.

## 5.13 Notifications

MVP: - In-app notifications for appointment requests, confirmations,
rejections, cancellations, and newly shared documents/prescriptions.

Optional later: - Email notifications through an `IEmailSender`
abstraction. - Reminder jobs.

Do not send real emails unless configuration and user consent are in
place. Do not put diagnosis or other sensitive clinical details in
notification messages. Prefer generic text such as "A new document is
available in your CuraLink account."

## 5.14 Dashboards

Administrator dashboard: - Total active doctors. - Pending doctor
verifications. - Total registered patients. - Appointment counts by
status. - Recent non-clinical platform activity.

Doctor dashboard: - Today's appointments. - Pending appointment
requests. - Upcoming appointments. - Patient count within the doctor's
authorized practice. - Recent operational activity.

Patient dashboard: - Next appointment. - Appointment history. - Recently
shared prescriptions/documents. - Personal reminders.

Dashboard statistics must respect role-based access and should not
reveal clinical details to administrators.

------------------------------------------------------------------------

## 6. Data Model and Database Design

Use EF Core Code First and migrations.

The following is a conceptual model. The agent should refine field
types, constraints, indexes, and navigation properties before
implementation.

### 6.1 Identity and profiles

#### ApplicationUser

Extend `IdentityUser` with only shared account fields: - Id -
FirstName - LastName - CreatedAtUtc - IsActive

Avoid duplicating password or authentication fields in Doctor and
Patient tables.

#### Doctor

-   Id
-   UserId (unique FK to ApplicationUser)
-   SpecialtyId
-   Biography
-   ClinicName
-   ClinicAddress
-   City
-   PublicPhone
-   ConsultationFee (nullable decimal)
-   CurrencyCode
-   VerificationStatus
-   VerifiedAtUtc (nullable)
-   CreatedAtUtc

#### Patient

-   Id
-   UserId (unique FK to ApplicationUser)
-   DateOfBirth (nullable)
-   Address (nullable)
-   EmergencyContactName (optional)
-   EmergencyContactPhone (optional)
-   CreatedAtUtc

#### Specialty

-   Id
-   Name (unique)
-   Description
-   IsActive

### 6.2 Scheduling

#### DoctorSchedule

-   Id
-   DoctorId
-   DayOfWeek
-   StartTime
-   EndTime
-   SlotDurationMinutes
-   IsActive

#### ScheduleException

-   Id
-   DoctorId
-   Date
-   StartTime (nullable)
-   EndTime (nullable)
-   IsUnavailable
-   Reason (optional)

### 6.3 Appointments

#### Appointment

-   Id
-   DoctorId
-   PatientId
-   StartAtUtc
-   EndAtUtc
-   Status
-   PatientReason (limited length)
-   CancellationReason (nullable)
-   CreatedAtUtc
-   UpdatedAtUtc

Add indexes for doctor/date, patient/date, and status. Enforce
non-overlap in the application service and use appropriate
transaction/concurrency protection. A simple unique index on start time
alone is not sufficient to prevent all interval overlaps.

#### AppointmentStatusHistory (recommended)

-   Id
-   AppointmentId
-   OldStatus
-   NewStatus
-   ChangedByUserId
-   ChangedAtUtc
-   Reason (nullable)

### 6.4 Medical information

#### MedicalRecord

-   Id
-   PatientId (unique if using one main record per patient)
-   CreatedAtUtc
-   UpdatedAtUtc

#### Allergy

-   Id
-   MedicalRecordId
-   Name
-   Reaction (optional)
-   Notes (optional)

#### MedicalCondition

-   Id
-   MedicalRecordId
-   Name
-   Notes
-   RecordedAtUtc
-   RecordedByDoctorId (nullable)

#### Consultation

-   Id
-   AppointmentId (unique where one consultation per appointment is
    intended)
-   PatientId
-   DoctorId
-   Reason
-   ReportedSymptoms
-   ClinicalObservations
-   Diagnosis
-   TreatmentPlan
-   Recommendations
-   FollowUpDate (nullable)
-   Status
-   CreatedAtUtc
-   UpdatedAtUtc
-   FinalizedAtUtc (nullable)

#### Prescription

-   Id
-   ConsultationId
-   PatientId
-   DoctorId
-   IssuedAtUtc
-   GeneralInstructions
-   Status

#### PrescriptionItem

-   Id
-   PrescriptionId
-   MedicineName
-   Strength (nullable)
-   DosageInstructions
-   Frequency
-   Duration
-   Route (nullable)
-   AdditionalInstructions (nullable)

### 6.5 Documents, notes, notifications, audit

#### MedicalDocument

-   Id
-   PatientId
-   UploadedByUserId
-   DoctorId (nullable)
-   DocumentType
-   OriginalFileName (sanitized display value)
-   StoredFileName
-   ContentType
-   FileSizeBytes
-   StorageKey
-   VisibilityStatus
-   UploadedAtUtc

#### PatientNote

-   Id
-   PatientId
-   Title
-   Content
-   ReminderAtUtc (nullable)
-   IsCompleted
-   IsSharedWithDoctor
-   CreatedAtUtc
-   UpdatedAtUtc

#### Notification

-   Id
-   UserId
-   Message
-   Type
-   RelatedEntityType (nullable)
-   RelatedEntityId (nullable)
-   IsRead
-   CreatedAtUtc

#### AuditLog

-   Id
-   ActorUserId (nullable for system events)
-   Action
-   EntityType
-   EntityId
-   OccurredAtUtc
-   CorrelationId (optional)
-   MetadataJson (only non-sensitive metadata)

Do not store full medical notes, passwords, tokens, or document contents
in audit metadata.

### 6.6 Entity relationship summary

-   One ApplicationUser has one Doctor profile or one Patient profile,
    depending on role.
-   One Specialty has many Doctors.
-   One Doctor has many schedules and schedule exceptions.
-   One Doctor has many Appointments.
-   One Patient has many Appointments.
-   An Appointment may have one Consultation.
-   A Consultation may have one or more Prescriptions if the business
    model allows it.
-   A Prescription has one or more PrescriptionItems.
-   A Patient has one MedicalRecord.
-   A MedicalRecord has many Allergies and MedicalConditions.
-   A Patient has many PatientNotes and MedicalDocuments.
-   A User has many Notifications.
-   Sensitive operations create AuditLog entries.

Use explicit delete behavior. Prefer restricted deletes for clinical
records. Do not cascade-delete medical history or finalized
prescriptions when an account is deactivated.

------------------------------------------------------------------------

## 7. UI/UX Requirements

Create a clean, professional, accessible healthcare interface.

### 7.1 Visual direction

-   Primary color: deep healthcare blue/navy.
-   Secondary color: teal or muted green.
-   Background: light neutral gray or white.
-   Use consistent spacing, typography, cards, badges, and form styles.
-   Avoid overly decorative gradients and cluttered dashboards.
-   Use responsive layouts for desktop, tablet, and mobile.
-   Use clear labels and helpful validation messages.
-   Meet reasonable accessibility practices: semantic HTML, keyboard
    navigation, visible focus, sufficient contrast, and labels for form
    fields.

### 7.2 Layouts

Create separate layouts or navigation shells for: - Public pages. -
Administrator area. - Doctor area. - Patient area.

Each dashboard should include: - Sidebar or responsive navigation. - Top
bar with account menu and notifications. - Page title and breadcrumb
where useful. - Main content area. - Consistent empty states and error
messages.

### 7.3 Required pages

Public: - `/` - `/Doctors` - `/Doctors/Details/{id}` -
`/Account/Login` - `/Account/Register` - `/About` - `/Privacy`

Admin: - `/Admin/Dashboard` - `/Admin/Doctors` -
`/Admin/Doctors/Verification` - `/Admin/Patients` -
`/Admin/Specialties` - `/Admin/AuditLogs`

Doctor: - `/Doctor/Dashboard` - `/Doctor/Profile` - `/Doctor/Schedule` -
`/Doctor/Appointments` - `/Doctor/Patients` -
`/Doctor/Patients/Details/{id}` - `/Doctor/Consultations` -
`/Doctor/Prescriptions` - `/Doctor/Documents`

Patient: - `/Patient/Dashboard` - `/Patient/Profile` -
`/Patient/Doctors` - `/Patient/Appointments` -
`/Patient/MedicalRecords` - `/Patient/Prescriptions` -
`/Patient/Documents` - `/Patient/Notes` - `/Patient/Notifications`

Route names can be adjusted to match MVC Areas and conventional routing.

------------------------------------------------------------------------

## 8. Security, Privacy, and Responsible Handling of Health Data

Health information is highly sensitive. Implement privacy and security
as core requirements, not as a final polish step.

### 8.1 Minimum safeguards

-   Use HTTPS outside local development.
-   Use ASP.NET Core Identity password hashing.
-   Use secure, HttpOnly cookies and appropriate SameSite settings.
-   Enable antiforgery protection for state-changing MVC forms.
-   Validate all inputs on the server.
-   Use EF Core parameterized queries; never concatenate user input into
    SQL.
-   Apply least-privilege role and resource authorization.
-   Avoid exposing sequential database IDs where opaque public
    identifiers are more appropriate.
-   Do not include sensitive information in URLs, logs, exception
    messages, or notifications.
-   Configure secure headers and production error handling.
-   Rate-limit sensitive endpoints such as login and password reset
    where practical.
-   Keep secrets in user secrets or environment variables.
-   Maintain audit events for sensitive access and modifications.
-   Back up the database securely and document restore procedures before
    production use.

### 8.2 Data minimization

Collect only information needed for the project's stated purpose. Make
optional fields genuinely optional. Do not collect national ID numbers
or other highly sensitive identifiers unless a documented requirement
exists.

### 8.3 Development and demo data

-   Use fictional names and synthetic medical data only.
-   Never use real patient records in screenshots, seed data, tests, or
    demonstrations.
-   Do not commit uploaded medical files or database backups.
-   Provide a development seed process with clearly fictional users.
-   Make demo credentials configurable and document that they must never
    be used in production.

### 8.4 Legal review

This project specification is not legal advice or a compliance
certification. Before any real-world deployment, review applicable
Tunisian data-protection, health-data, medical-practice,
record-retention, consent, and electronic-prescription requirements with
qualified professionals and relevant authorities.

------------------------------------------------------------------------

## 9. Non-Functional Requirements

-   Maintainable, readable code with clear naming.
-   Responsive interface.
-   Server-side validation for every form.
-   Asynchronous database access for I/O operations.
-   Pagination for potentially large lists.
-   Search and filtering should be database-backed.
-   Avoid N+1 query patterns; use appropriate projections and includes.
-   Use UTC timestamps in persistence.
-   Use decimal types for monetary amounts, never floating-point.
-   Add database indexes for common search and relationship fields.
-   Provide useful error handling without exposing stack traces to end
    users.
-   Log operational events without logging private clinical content.
-   Unit-test business rules and integration-test critical authorization
    and workflows.

------------------------------------------------------------------------

## 10. Testing Strategy

Create unit and integration tests.

### 10.1 Unit tests

At minimum, test: - Appointment time validation. - Appointment overlap
detection. - Appointment status transitions. - Doctor verification
eligibility for public listing. - Prescription item validation. -
Patient note ownership rules. - Role and resource authorization
policies.

### 10.2 Integration tests

At minimum, test: - Patient registration and login. - Doctor
registration and pending verification. - Unapproved doctors are not
shown in public search. - Administrator can approve a doctor but cannot
read clinical records by default. - Patient can only view their own
records. - Doctor cannot access an unrelated patient's record. - Doctor
can access an authorized patient record. - Patient can request an
available appointment. - Conflicting appointment booking is rejected. -
Only the assigned doctor can complete an appointment or create its
consultation. - Patient can view a prescription after it is
issued/shared. - Unauthorized users cannot download private documents.

Use a test database strategy that is reliable for relational behavior.
Do not assume EF Core InMemory behaves like SQL Server for constraints
and transactions.

------------------------------------------------------------------------

## 11. Development Phases and Deliverables

Implement in the following order. Do not start later phases until the
current phase builds and its acceptance criteria pass.

### Phase 0 --- Repository and foundation

Tasks: - Inspect repository. - Create solution and projects. - Configure
dependency injection. - Configure EF Core and SQL Server. - Add
environment-based configuration. - Add logging and error handling. - Add
initial README and architecture notes.

Deliverables: - Solution builds. - Application starts. - Database
connection configuration is documented. - Health/startup instructions
are available.

### Phase 1 --- Identity and role-based access

Tasks: - Configure ASP.NET Core Identity. - Add ApplicationUser and
profile entities. - Implement patient and doctor registration. -
Implement login/logout. - Add role seeding for Admin, Doctor, Patient. -
Add secure admin bootstrap instructions. - Create role-specific
dashboard shells. - Add authorization tests.

Acceptance criteria: - A patient can register and log in. - A doctor can
register and remains pending verification. - Public users cannot access
dashboards. - Patient cannot access doctor/admin routes. - Doctor cannot
access admin routes. - No public administrator registration exists.

### Phase 2 --- Specialties and doctor directory

Tasks: - Specialty CRUD for admin. - Doctor verification workflow. -
Doctor profile management. - Public doctor listing and detail pages. -
Search, filters, and pagination.

Acceptance criteria: - Only active, approved doctors appear publicly. -
Doctor profile does not expose private account data. - Admin can
approve/reject doctor registration. - Doctors cannot approve themselves.

### Phase 3 --- Scheduling and appointments

Tasks: - Doctor recurring schedules. - Schedule exceptions. -
Availability service. - Patient booking workflow. - Doctor
confirmation/rejection. - Cancellation and rescheduling rules. -
Appointment status history. - Concurrency protection and tests.

Acceptance criteria: - Patient can book a valid available slot. - Past
or unavailable slots are rejected. - Overlapping active appointments are
prevented. - Only authorized parties can change appointment state. -
Appointment history is retained.

### Phase 4 --- Patient records and consultations

Tasks: - Medical record creation. - Allergies and medical conditions. -
Doctor patient list based on authorized relationship. - Consultation
creation and editing. - Draft/finalized workflow. - Patient read-only
view of shared consultation information.

Acceptance criteria: - An unrelated doctor cannot read the patient's
record. - Patient cannot modify doctor-authored consultation details. -
Finalized consultations cannot be silently overwritten. - Changes are
attributable to an authenticated user.

### Phase 5 --- Prescriptions and documents

Tasks: - Prescription and prescription item management. - Issue/cancel
workflow. - Patient prescription view. - Private document storage and
authorized downloads. - File validation and upload limits.

Acceptance criteria: - Only an authorized doctor can issue a
prescription. - Patient can only access their own issued/shared
prescriptions. - Private documents are not publicly addressable. -
Invalid file types and oversized uploads are rejected.

### Phase 6 --- Patient notes, notifications, dashboards

Tasks: - Patient personal notes and reminders. - In-app notifications. -
Role-specific dashboard statistics. - Empty states and usability
refinements.

Acceptance criteria: - Patient notes are private by default. -
Notifications do not reveal clinical details. - Dashboard data is scoped
to the current user's permissions.

### Phase 7 --- Quality, documentation, and handover

Tasks: - Complete automated tests. - Review authorization paths. -
Review logs for sensitive data leakage. - Add synthetic demo data. -
Document migrations and setup. - Add screenshots only using fictional
demo records. - Prepare a project demonstration guide.

Deliverables: - Buildable source code. - Database migrations. -
Automated tests. - Setup documentation. - Architecture and database
documentation. - Demo workflow.

------------------------------------------------------------------------

## 12. Local Development Setup

The agent should provide exact commands based on the installed .NET SDK
and selected package versions. The following is the expected workflow.

### Prerequisites

-   Supported .NET SDK.
-   SQL Server Developer/Express or LocalDB.
-   Visual Studio or VS Code.
-   EF Core CLI tools.

### Configuration

Use a connection string such as the following in local user secrets or
environment configuration:

``` json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CuraLinkDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

This is an example for local development only. Use an appropriate SQL
Server connection string for the actual environment. Do not commit
credentials.

### Expected EF Core workflow

``` bash
dotnet tool install --global dotnet-ef
dotnet restore
dotnet build
dotnet ef migrations add InitialCreate --project src/CuraLink.Infrastructure --startup-project src/CuraLink.Web
dotnet ef database update --project src/CuraLink.Infrastructure --startup-project src/CuraLink.Web
dotnet run --project src/CuraLink.Web
```

Adjust project references and EF command options to match the final
solution structure.

### Tests

``` bash
dotnet test
```

The README must be updated with the exact commands that work in the
generated repository.

------------------------------------------------------------------------

## 13. Seed Data for Demonstration

Create a development-only seeding mechanism with fictional data: - A
small set of medical specialties. - Two or three approved fictional
doctors. - One pending fictional doctor. - Several fictional patients. -
Example schedules. - A few appointments in different statuses. -
Synthetic consultation and prescription records only if needed for
demonstrating the workflow.

Do not seed real personal or health information.

If demo accounts are created, make their credentials configurable
through local development secrets or print one-time setup instructions.
Do not commit production-like passwords.

------------------------------------------------------------------------

## 14. UI Quality Checklist

Before considering the interface complete, verify:

-   [ ] Consistent CuraLink branding and navigation.
-   [ ] Responsive public pages.
-   [ ] Separate Admin, Doctor, and Patient navigation.
-   [ ] Dashboard cards display real database values.
-   [ ] Tables support empty states and pagination.
-   [ ] Forms show validation errors next to relevant fields.
-   [ ] Destructive actions require confirmation.
-   [ ] Success and error feedback is visible.
-   [ ] Loading states are provided for asynchronous interactions.
-   [ ] Dates and times are displayed in the configured local timezone.
-   [ ] No private medical information appears on public pages.
-   [ ] Keyboard navigation and accessible form labels work.

------------------------------------------------------------------------

## 15. Important Implementation Decisions

Unless a later requirement overrides them, follow these defaults:

1.  Use ASP.NET Core MVC with Razor Views, not a separate SPA.
2.  Use ASP.NET Core Identity for authentication.
3.  Use three roles: Admin, Doctor, Patient.
4.  A doctor must be approved before appearing in public search or
    accepting normal patient bookings.
5.  Use SQL Server and EF Core Code First migrations.
6.  Use a modular monolith with clear project boundaries.
7.  Keep clinical records private and apply resource-level
    authorization.
8.  Keep patient personal notes separate from official medical records.
9.  Keep private documents outside the public web root.
10. Use synthetic data only.
11. Build the MVP before adding optional features.
12. Do not implement AI medical diagnosis or automated treatment
    recommendations.

------------------------------------------------------------------------

## 16. First Task for the AI Agent

**Start with Phase 0 only.**

Your first response and implementation should:

1.  Inspect the repository and report what already exists.
2.  Check the installed .NET SDK.
3.  Propose the exact solution/project references.
4.  Create the solution and the four projects under `src/`.
5.  Add the test projects under `tests/`.
6.  Configure the initial dependency injection and EF Core SQL Server
    setup.
7.  Add a minimal home page and a health/startup check if appropriate.
8.  Add configuration examples without secrets.
9.  Ensure `dotnet restore`, `dotnet build`, and `dotnet test` succeed.
10. Document the exact local run instructions.

Do not implement every business feature in Phase 0. After Phase 0
passes, summarize the files created, commands executed, test results,
and any assumptions, then proceed to Phase 1.

------------------------------------------------------------------------

## 17. Project Success Criteria

CuraLink's first complete university release is successful when:

-   Three distinct roles can authenticate and use their own dashboards.
-   Patients can discover approved doctors and request appointments.
-   Doctors can manage schedules and appointments.
-   Doctors can create authorized consultation records and
    prescriptions.
-   Patients can access their own shared records and documents.
-   Administrators can manage platform operations without unrestricted
    clinical-record access.
-   Core workflows are protected by server-side authorization.
-   The database can be created from migrations.
-   Critical workflows and access restrictions have automated tests.
-   A new developer can run the project using the documented setup
    steps.

**Build CuraLink incrementally, prioritize privacy and correctness, and
keep the code understandable for a university-level C# developer.**

