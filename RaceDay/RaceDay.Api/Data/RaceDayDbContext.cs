using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
 
    public class RaceDayDbContext: DbContext
    {
        public RaceDayDbContext(
        DbContextOptions<RaceDayDbContext> options)
        : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<RaceResults> RaceResults { get; set; }
        public DbSet<Awards> Awards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One Organiser -> many Events
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganiserID);

            // One Event -> many Categories
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventID);

            // One Participant -> many Enrolments
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Participant)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(e => e.ParticipantId);

            // One Event -> many Enrolments
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Event)
                .WithMany(ev => ev.Enrolments)
                .HasForeignKey(e => e.EventID);

            // One Category -> many Enrolments
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CategoryId);

            // One Enrolment -> zero or one RaceResult
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.RaceResult)
                .WithOne(r => r.Enrolment)
                .HasForeignKey<RaceResults>(r => r.EnrolmentID);

            // Store Role enum values as strings
            modelBuilder.Entity<Account>()
                .Property(a => a.Role)
                .HasConversion<string>();
        }
    }
}
