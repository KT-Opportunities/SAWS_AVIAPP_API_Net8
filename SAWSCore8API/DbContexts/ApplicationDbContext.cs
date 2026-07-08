using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SAWSCore8API.Models;

namespace SAWSCore8API.DbContexts;
public class SAWSDbContext : IdentityDbContext<ApplicationUser>
{
    public SAWSDbContext(DbContextOptions<SAWSDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Subscription>()
            .Property(s => s.package_price)
            .HasPrecision(18, 2);

        builder.Entity<Package>()
            .Property(p => p.price)
            .HasPrecision(18, 2);

        builder.Entity<Package>()
            .HasData(
                new Package
                {
                    packageId = 1,
                    name = "Free",
                    price = 0.00m,
                    created_at = new DateTime(2024, 2, 23, 12, 0, 0),
                    updated_at = new DateTime(2024, 2, 23, 12, 0, 0),
                    isdeleted = false,
                    deleted_at = null
                },
                new Package
                {
                    packageId = 2,
                    name = "monthly Premium",
                    price = 180.00m,
                    created_at = new DateTime(2024, 2, 23, 12, 0, 0),
                    updated_at = new DateTime(2024, 2, 23, 12, 0, 0),
                    isdeleted = false,
                    deleted_at = null
                },
                new Package
                {
                    packageId = 3,
                    name = "monthly Regulated",
                    price = 380.00m,
                    created_at = new DateTime(2024, 6, 13, 12, 49, 49, 577),
                    updated_at = new DateTime(2024, 6, 13, 12, 49, 49, 577),
                    isdeleted = false,
                    deleted_at = null
                },
                new Package
                {
                    packageId = 4,
                    name = "annually Premium",
                    price = 2160.00m,
                    created_at = new DateTime(2024, 6, 13, 12, 56, 53, 177),
                    updated_at = new DateTime(2024, 6, 13, 12, 56, 53, 177),
                    isdeleted = false,
                    deleted_at = null
                },
                new Package
                {
                    packageId = 5,
                    name = "annually Regulated",
                    price = 4560.00m,
                    created_at = new DateTime(2024, 6, 13, 12, 56, 53, 177),
                    updated_at = new DateTime(2024, 6, 13, 12, 56, 53, 177),
                    isdeleted = false,
                    deleted_at = null
                },
                new Package
                  {
                      packageId = 6,
                      name = "Admin",
                      price = 0.00m,
                      created_at = new DateTime(2024, 6, 13, 12, 56, 53, 177),
                      updated_at = new DateTime(2024, 6, 13, 12, 56, 53, 177),
                      isdeleted = false,
                      deleted_at = null
                },
                new Package
                {
                    packageId = 7,
                    name = "Admin monthly Regulated",
                    price = 0.00m,
                    created_at = new DateTime(2024, 6, 13, 12, 56, 53, 177),
                    updated_at = new DateTime(2024, 6, 13, 12, 56, 53, 177),
                    isdeleted = false,
                    deleted_at = null
                },
                new Package
                {
                    packageId = 8,
                    name = "Admin annually Regulated",
                    price = 0.00m,
                    created_at = new DateTime(2024, 6, 13, 12, 56, 53, 177),
                    updated_at = new DateTime(2024, 6, 13, 12, 56, 53, 177),
                    isdeleted = false,
                    deleted_at = null
                },
                new Package
                {
                    packageId = 9,
                    name = "Admin monthly Premium",
                    price = 0.00m,
                    created_at = new DateTime(2024, 6, 13, 12, 56, 53, 177),
                    updated_at = new DateTime(2024, 6, 13, 12, 56, 53, 177),
                    isdeleted = false,
                    deleted_at = null
                },
                new Package
                {
                    packageId = 10,
                    name = "Admin annually Premium",
                    price = 0.00m,
                    created_at = new DateTime(2024, 6, 13, 12, 56, 53, 177),
                    updated_at = new DateTime(2024, 6, 13, 12, 56, 53, 177),
                    isdeleted = false,
                    deleted_at = null
                }

            );
    }

    public DbSet<ApplicationUser> User { get; set; }
    public DbSet<UserProfile> userProfiles { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<FeedbackMessage> FeedbackMessages { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Advert> Adverts { get; set; }
    public DbSet<DocAdvert> DocAdverts { get; set; }
    public DbSet<DocFeedback> DocFeedbacks { get; set; }
    public DbSet<Package> Package { get; set; }
    public DbSet<AdvertClick> AdvertClicks { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<FlightTemplate> FlightTemplates { get; set; }
    public DbSet<OperationalSettings> OperationalSettings { get; set; }
}
