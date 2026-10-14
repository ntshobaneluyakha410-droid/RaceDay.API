using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Organiser> Organisers => Set<Organiser>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ===== Relationships =====

        // User <-> Organiser (one-to-one)
        modelBuilder.Entity<Organiser>()
            .HasOne(o => o.User)
            .WithOne(u => u.Organiser)
            .HasForeignKey<Organiser>(o => o.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        // User <-> Participant (one-to-one)
        modelBuilder.Entity<Participant>()
            .HasOne(p => p.User)
            .WithOne(u => u.Participant)
            .HasForeignKey<Participant>(p => p.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        // Organiser -> Events (one-to-many)
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganiserID)
            .OnDelete(DeleteBehavior.Restrict);

        // Event -> Categories (one-to-many)
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventID)
            .OnDelete(DeleteBehavior.Cascade);

        // Participant -> Enrolments (one-to-many)
        modelBuilder.Entity<Enrolment>()
            .HasOne(en => en.Participant)
            .WithMany(p => p.Enrolments)
            .HasForeignKey(en => en.ParticipantID)
            .OnDelete(DeleteBehavior.Restrict);

        // Category -> Enrolments (one-to-many)
        modelBuilder.Entity<Enrolment>()
            .HasOne(en => en.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(en => en.CategoryID)
            .OnDelete(DeleteBehavior.Restrict);

        // Enrolment <-> Result (one-to-one)
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment)
            .WithOne(en => en.Result)
            .HasForeignKey<Result>(r => r.EnrolmentID)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== Constraints / Indexes =====

        // Unique email per user
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Prevent duplicate enrolment (same participant + same category)
        modelBuilder.Entity<Enrolment>()
            .HasIndex(en => new { en.ParticipantID, en.CategoryID })
            .IsUnique();

        // ===== Column Type / Precision Fixes =====

        // EntryFee decimal precision — prevents silent truncation
        modelBuilder.Entity<Category>()
            .Property(c => c.EntryFee)
            .HasPrecision(18, 2);

        // DistanceKm decimal precision (if decimal in your model)
        modelBuilder.Entity<Category>()
            .Property(c => c.DistanceKm)
            .HasPrecision(18, 2);
    }
}