using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var context     = services.GetRequiredService<AppDbContext>();

        // Apply any pending migrations automatically
        await context.Database.MigrateAsync();

        // ── 1. Seed Roles ─────────────────────────────────────────────────────
        string[] roles = ["Receptionist", "Nurse", "Doctor"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ── 2. Seed Default Users ─────────────────────────────────────────────
        var defaultUsers = new[]
        {
            new { FullName = "Admin Receptionist", Email = "receptionist@clinic.com",
                  Password = "Clinic@123",  Role = "Receptionist" },
            new { FullName = "Nurse Sarah",        Email = "nurse@clinic.com",
                  Password = "Clinic@123",  Role = "Nurse" },
            new { FullName = "Dr. James Wilson",   Email = "doctor1@clinic.com",
                  Password = "Clinic@123",  Role = "Doctor" },
            new { FullName = "Dr. Emily Carter",   Email = "doctor2@clinic.com",
                  Password = "Clinic@123",  Role = "Doctor" },
        };

        foreach (var u in defaultUsers)
        {
            if (await userManager.FindByEmailAsync(u.Email) == null)
            {
                var user = new ApplicationUser
                {
                    FullName = u.FullName,
                    UserName = u.Email,
                    Email    = u.Email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, u.Password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, u.Role);
            }
        }

        // ── 3. Seed Doctors (linked to their user accounts) ───────────────────
        if (!await context.Doctors.AnyAsync())
        {
            var doc1User = await userManager.FindByEmailAsync("doctor1@clinic.com");
            var doc2User = await userManager.FindByEmailAsync("doctor2@clinic.com");

            context.Doctors.AddRange(
                new Doctor
                {
                    ApplicationUserId = doc1User?.Id,
                    FullName          = "Dr. James Wilson",
                    Specialization    = "General Practice",
                    Phone             = "555-0101",
                    Email             = "doctor1@clinic.com",
                    IsAvailable       = true
                },
                new Doctor
                {
                    ApplicationUserId = doc2User?.Id,
                    FullName          = "Dr. Emily Carter",
                    Specialization    = "Pediatrics",
                    Phone             = "555-0102",
                    Email             = "doctor2@clinic.com",
                    IsAvailable       = true
                }
            );
            await context.SaveChangesAsync();
        }

        // ── 4. Seed Patients ──────────────────────────────────────────────────
        if (!await context.Patients.AnyAsync())
        {
            var receptionist = await userManager.FindByEmailAsync("receptionist@clinic.com");

            context.Patients.AddRange(
                new Patient
                {
                    FullName             = "John Martinez",
                    DateOfBirth          = new DateTime(1985, 3, 15),
                    Gender               = Gender.Male,
                    Phone                = "555-1001",
                    Email                = "john.m@email.com",
                    BloodType            = "O+",
                    EmergencyContactName = "Maria Martinez",
                    EmergencyContactPhone= "555-1002",
                    CreatedById          = receptionist?.Id
                },
                new Patient
                {
                    FullName             = "Sarah Thompson",
                    DateOfBirth          = new DateTime(1992, 7, 22),
                    Gender               = Gender.Female,
                    Phone                = "555-1003",
                    BloodType            = "A+",
                    EmergencyContactName = "Tom Thompson",
                    EmergencyContactPhone= "555-1004",
                    CreatedById          = receptionist?.Id
                },
                new Patient
                {
                    FullName             = "Robert Chen",
                    DateOfBirth          = new DateTime(1978, 11, 8),
                    Gender               = Gender.Male,
                    Phone                = "555-1005",
                    BloodType            = "B-",
                    CreatedById          = receptionist?.Id
                },
                new Patient
                {
                    FullName             = "Linda Okafor",
                    DateOfBirth          = new DateTime(2001, 5, 30),
                    Gender               = Gender.Female,
                    Phone                = "555-1006",
                    BloodType            = "AB+",
                    EmergencyContactName = "Charles Okafor",
                    EmergencyContactPhone= "555-1007",
                    CreatedById          = receptionist?.Id
                },
                new Patient
                {
                    FullName             = "Michael Brown",
                    DateOfBirth          = new DateTime(1965, 9, 12),
                    Gender               = Gender.Male,
                    Phone                = "555-1008",
                    BloodType            = "O-",
                    CreatedById          = receptionist?.Id
                }
            );
            await context.SaveChangesAsync();
        }

        // ── 5. Seed Appointments ──────────────────────────────────────────────
        if (!await context.Appointments.AnyAsync())
        {
            var receptionist = await userManager.FindByEmailAsync("receptionist@clinic.com");
            var patients     = await context.Patients.ToListAsync();
            var doctors      = await context.Doctors.ToListAsync();

            var today = DateTime.Today;

            context.Appointments.AddRange(
                new Appointment
                {
                    PatientId      = patients[0].Id,
                    DoctorId       = doctors[0].Id,
                    AppointmentDate= today.AddHours(9),
                    ReasonForVisit = "Annual check-up",
                    Status         = AppointmentStatus.Scheduled,
                    CreatedById    = receptionist?.Id
                },
                new Appointment
                {
                    PatientId      = patients[1].Id,
                    DoctorId       = doctors[1].Id,
                    AppointmentDate= today.AddHours(10),
                    ReasonForVisit = "Fever and sore throat",
                    Status         = AppointmentStatus.InProgress,
                    CreatedById    = receptionist?.Id
                },
                new Appointment
                {
                    PatientId      = patients[2].Id,
                    DoctorId       = doctors[0].Id,
                    AppointmentDate= today.AddDays(-1).AddHours(14),
                    ReasonForVisit = "Follow-up on blood pressure",
                    Status         = AppointmentStatus.Completed,
                    Diagnosis      = "Hypertension - Stage 1",
                    Prescription   = "Lisinopril 10mg once daily",
                    CreatedById    = receptionist?.Id
                },
                new Appointment
                {
                    PatientId      = patients[3].Id,
                    DoctorId       = doctors[1].Id,
                    AppointmentDate= today.AddDays(1).AddHours(11),
                    ReasonForVisit = "Routine vaccination",
                    Status         = AppointmentStatus.Scheduled,
                    CreatedById    = receptionist?.Id
                },
                new Appointment
                {
                    PatientId      = patients[4].Id,
                    DoctorId       = doctors[0].Id,
                    AppointmentDate= today.AddDays(-2).AddHours(9),
                    ReasonForVisit = "Chest pain evaluation",
                    Status         = AppointmentStatus.Cancelled,
                    CreatedById    = receptionist?.Id
                }
            );
            await context.SaveChangesAsync();
        }
    }
}
