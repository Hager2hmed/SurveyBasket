using DentalNUB.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using static DentalNUB.Api.Entities.Clinic;


namespace DentalNUB.Api.Data;

public class DentalNUBDbContext : DbContext
{
    public DentalNUBDbContext(DbContextOptions<DentalNUBDbContext> options)
            : base(options)
    {
    }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Consultant> Consultants { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<PatientCase> PatientCases { get; set; }
    public DbSet<Diagnose> Diagnoses { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatNUB> ChatNUBs { get; set; }
    public DbSet<ToolPost> ToolPosts { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Clinic> Clinics { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<User>  Users { get; set; }
    public DbSet<DoctorSectionRanking> DoctorSectionRankings { get; set; }
    public DbSet<ClinicSection> clinicSections { get; set; }
    public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);

        // Relationship: Patient <-> Appointment (one-to-many)
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientID)
           ;

        // Relationship: Consultant <-> Appointment (one-to-many)
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Consultant)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.ConsID)
            .OnDelete(DeleteBehavior.SetNull);

        // Relationship: Doctor <-> PatientCase (one-to-many)
        modelBuilder.Entity<PatientCase>()
            .HasOne(pc => pc.Doctor)
            .WithMany(d => d.Patientcases)
            .HasForeignKey(pc => pc.DoctorID);


        // Relationship: Consultant <-> PatientCase (one-to-many)
        modelBuilder.Entity<PatientCase>()
            .HasOne(pc => pc.Consultant)
            .WithMany(c => c.PatientCases)
            .HasForeignKey(pc => pc.ConsID);
            

        modelBuilder.Entity<PatientCase>()
            .HasOne(c => c.Diagnose)
            .WithMany(d => d.Cases)           
            .HasForeignKey(c => c.DiagnoseID)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Question <-> Patient (one-to-many)
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Patient)
            .WithMany(p => p.Questions)
            .HasForeignKey(q => q.PatientID)
            ; 

        // Relationship: ChatMessage <-> Doctor (one-to-many)
        modelBuilder.Entity<ChatMessage>()
            .HasOne(cm => cm.Doctor)
            .WithMany(d => d.ChatMessages)
            .HasForeignKey(cm => cm.DoctorID);


        // Relationship: ChatNUB <-> Patient (one-to-many)
        modelBuilder.Entity<ChatNUB>()
            .HasOne(c => c.Patient)
            .WithMany(p => p.ChatNUB)
            .HasForeignKey(c => c.PatientID);


        // Relationship: ToolPost <-> Doctor (one-to-many)
        modelBuilder.Entity<ToolPost>()
            .HasOne(tp => tp.Doctor)
            .WithMany(d => d.ToolPost)
            .HasForeignKey(tp => tp.DoctorID);
         
        // Relationship: Notification <-> Patient (one-to-many)
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Patient)
            .WithMany(p => p.Notification)
            .HasForeignKey(n => n.PatientID); 

        // Relationship: PatientCase <-> Clinic (one-to-many)
        modelBuilder.Entity<PatientCase>()
            .HasOne(pc => pc.Clinic)
            .WithMany(c => c.PatientCases)
            .HasForeignKey(pc => pc.ClinicID); 

        // Relationship: Question <-> Answer (one-to-one)
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Answer)
            .WithOne(a => a.Question)
            .HasForeignKey<Answer>(a => a.QuestID)
           ;


        modelBuilder.Entity<User>()
           .HasOne(u => u.Doctor)
           .WithOne(d => d.User)
           .HasForeignKey<Doctor>(d => d.UserId);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Patient)
            .WithOne(p => p.User)
            .HasForeignKey<Patient>(p => p.UserId);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Consultant)
            .WithOne(c => c.User)
            .HasForeignKey<Consultant>(c => c.UserId);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Admin)
            .WithOne(a => a.User)
            .HasForeignKey<Admin>(a => a.UserId);
       

    }
}
