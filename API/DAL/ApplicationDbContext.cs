using Microsoft.EntityFrameworkCore;
using SecurityAssessmentAPI.Models.Entities;

namespace SecurityAssessmentAPI.DAL
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssessmentRun> AssessmentRuns { get; set; }
        public DbSet<CheckType> CheckTypes { get; set; }
        public DbSet<CheckResult> CheckResults { get; set; }
        public DbSet<Finding> Findings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Customer
            modelBuilder.Entity<Customer>()
                .HasKey(c => c.CustomerId);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Assets)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId);

            // Configure Asset
            modelBuilder.Entity<Asset>()
                .HasKey(a => a.AssetId);

            modelBuilder.Entity<Asset>()
                .HasMany(a => a.AssessmentRuns)
                .WithOne(ar => ar.Asset)
                .HasForeignKey(ar => ar.AssetId);

            // Configure AssessmentRun
            modelBuilder.Entity<AssessmentRun>()
                .HasKey(ar => ar.RunId);

            modelBuilder.Entity<AssessmentRun>()
                .HasMany(ar => ar.CheckResults)
                .WithOne(cr => cr.AssessmentRun)
                .HasForeignKey(cr => cr.RunId);

            // Configure CheckType
            modelBuilder.Entity<CheckType>()
                .HasKey(ct => ct.CheckTypeId);

            modelBuilder.Entity<CheckType>()
                .HasMany(ct => ct.CheckResults)
                .WithOne(cr => cr.CheckType)
                .HasForeignKey(cr => cr.CheckTypeId);

            // Configure CheckResult
            modelBuilder.Entity<CheckResult>()
                .HasKey(cr => cr.CheckResultId);

            modelBuilder.Entity<CheckResult>()
                .HasIndex(cr => new { cr.RunId, cr.CheckTypeId })
                .IsUnique();

            modelBuilder.Entity<CheckResult>()
                .HasMany(cr => cr.Findings)
                .WithOne(f => f.CheckResult)
                .HasForeignKey(f => f.CheckResultId);

            // Configure Finding
            modelBuilder.Entity<Finding>()
                .HasKey(f => f.ReasonId);

            // Convert enums to strings in the database
            modelBuilder.Entity<Asset>()
                .Property(a => a.AssetType)
                .HasConversion<string>();

            modelBuilder.Entity<AssessmentRun>()
                .Property(ar => ar.Status)
                .HasConversion<string>();

            modelBuilder.Entity<AssessmentRun>()
                .Property(ar => ar.Grade)
                .HasConversion<string>();

            modelBuilder.Entity<CheckResult>()
                .Property(cr => cr.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Finding>()
                .Property(f => f.Severity)
                .HasConversion<string>();
        }
    }
}
