using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Data.Data;

public class ProjectContext : DbContext
{
    private readonly string _connectionString;

    public ProjectContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("Connection string is missing.");
    }

    public DbSet<ProjectEntity> Projects { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectEntity>()
            .HasKey(p => p.ProjectNumber);

        modelBuilder.Entity<ProjectEntity>()
            .Property(p => p.Status)
            .HasConversion<string>();

        modelBuilder.Entity<ProjectEntity>().HasData(
            new ProjectEntity("P001")
            {
                Name = "Sample Project",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(3),
                ProjectManager = "John Doe",
                Customer = "Sample Customer",
                Service = "Development",
                TotalPrice = 300000m,
                Status = ProjectStatus.NotStarted
            },
            new ProjectEntity("P002")
            {
                Name = "Website Redesign",
                StartDate = DateTime.Now.AddDays(-10),
                EndDate = DateTime.Now.AddMonths(2),
                ProjectManager = "Jane Smith",
                Customer = "Tech Corp",
                Service = "Web Design",
                TotalPrice = 150000m,
                Status = ProjectStatus.Ongoing
            },
            new ProjectEntity("P003")
            {
                Name = "Database Migration",
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddDays(15),
                ProjectManager = "Mike Johnson",
                Customer = "Data Inc",
                Service = "Database Services",
                TotalPrice = 200000m,
                Status = ProjectStatus.Ongoing
            }
        );
    }
}