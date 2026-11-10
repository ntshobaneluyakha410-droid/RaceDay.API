using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Organiser> Organisers => Set<Organiser>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User <-> Organiser
        modelBuilder.Entity<Organiser>()
            .HasOne(o => o.User)
            .WithOne(u => u.Organiser)
            .HasForeignKey<Organiser>(o => o.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        // User <-> Participant
        modelBuilder.Entity<Participant>()
            .HasOne(p => p.User)
            .WithOne(u => u.Participant)
            .HasForeignKey<Participant>(p => p.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        // Organiser -> Events
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganiserID)
            .OnDelete(DeleteBehavior.Restrict);

        // Event -> Categories
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventID)
            .OnDelete(DeleteBehavior.Cascade);

        // Participant -> Enrolments
        modelBuilder.Entity<Enrolment>()
            .HasOne(en => en.Participant)
            .WithMany(p => p.Enrolments)
            .HasForeignKey(en => en.ParticipantID)
            .OnDelete(DeleteBehavior.Restrict);

        // Category -> Enrolments
        modelBuilder.Entity<Enrolment>()
            .HasOne(en => en.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(en => en.CategoryID)
            .OnDelete(DeleteBehavior.Restrict);

        // Enrolment <-> Result
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment)
            .WithOne(en => en.Result)
            .HasForeignKey<Result>(r => r.EnrolmentID)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique Constraints
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Enrolment>().HasIndex(en => new { en.ParticipantID, en.CategoryID }).IsUnique();

        // Decimal Precision
        modelBuilder.Entity<Category>().Property(c => c.EntryFee).HasPrecision(18, 2);
    }
}