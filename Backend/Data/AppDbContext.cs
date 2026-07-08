using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Vitals> Vitals => Set<Vitals>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Required — sets up Identity tables

        // --- Patient ---
        builder.Entity<Patient>(e =>
        {
            e.HasIndex(p => p.Phone);
            e.HasIndex(p => p.FullName);

            // Prevent cascade delete from ApplicationUser → Patient
            e.HasOne(p => p.CreatedBy)
             .WithMany()
             .HasForeignKey(p => p.CreatedById)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // --- Doctor ---
        builder.Entity<Doctor>(e =>
        {
            // One ApplicationUser can be linked to one Doctor
            e.HasOne(d => d.ApplicationUser)
             .WithMany()
             .HasForeignKey(d => d.ApplicationUserId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // --- Appointment ---
        builder.Entity<Appointment>(e =>
        {
            e.HasIndex(a => a.AppointmentDate);
            e.HasIndex(a => a.Status);

            // Patient deleted → don't cascade to appointments (soft delete pattern)
            e.HasOne(a => a.Patient)
             .WithMany(p => p.Appointments)
             .HasForeignKey(a => a.PatientId)
             .OnDelete(DeleteBehavior.Restrict);

            // Doctor deleted → don't cascade
            e.HasOne(a => a.Doctor)
             .WithMany(d => d.Appointments)
             .HasForeignKey(a => a.DoctorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.CreatedBy)
             .WithMany()
             .HasForeignKey(a => a.CreatedById)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // --- Vitals ---
        builder.Entity<Vitals>(e =>
        {
            // One-to-one with Appointment
            e.HasOne(v => v.Appointment)
             .WithOne(a => a.Vitals)
             .HasForeignKey<Vitals>(v => v.AppointmentId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(v => v.RecordedBy)
             .WithMany()
             .HasForeignKey(v => v.RecordedById)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // --- AuditLog ---
        builder.Entity<AuditLog>(e =>
        {
            e.HasIndex(a => a.Timestamp);
            e.HasIndex(a => a.EntityName);

            e.HasOne(a => a.User)
             .WithMany(u => u.AuditLogs)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
