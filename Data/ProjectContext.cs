using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class ProjectContext : DbContext
{
    private readonly string _connectionString;

    public ProjectContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' is missing in configuration.");

        Projects = Set<ProjectEntity>();
        Customers = Set<CustomerEntity>();
    }

    public DbSet<ProjectEntity> Projects { get; set; }

    public DbSet<CustomerEntity> Customers { get; set; }

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

        modelBuilder.Entity<CustomerEntity>()
            .HasKey(c => c.CustomerId);

        modelBuilder.Entity<CustomerEntity>()
            .Property(c => c.CustomerId)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<ProjectEntity>()
            .HasOne(p => p.Customer)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.CustomerId);

        modelBuilder.Entity<ProjectEntity>().HasData(
            // Sample Project 1
            new ProjectEntity("P001")
            {
                Name = "Sample Project",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(3),
                ProjectManager = "John Doe",
                CustomerId = 1,
                Service = "Development",
                TotalPrice = 300000m,
                Status = ProjectStatus.NotStarted
            },

            // Sample Project 2
            new ProjectEntity("P002")
            {
                Name = "Website Redesign",
                StartDate = DateTime.Now.AddDays(-10),
                EndDate = DateTime.Now.AddMonths(2),
                ProjectManager = "Jane Smith",
                CustomerId = 2,
                Service = "Web Design",
                TotalPrice = 150000m,
                Status = ProjectStatus.Ongoing
            },

            // Sample Project 3
            new ProjectEntity("P003")
            {
                Name = "Database Migration",
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddDays(15),
                ProjectManager = "Mike Johnson",
                CustomerId = 3,
                Service = "Database Services",
                TotalPrice = 200000m,
                Status = ProjectStatus.Ongoing
            }
        );

        modelBuilder.Entity<CustomerEntity>().HasData(
            new CustomerEntity { CustomerId = 1, Name = "Sample Customer" },
            new CustomerEntity { CustomerId = 2, Name = "Tech Corp" },
            new CustomerEntity { CustomerId = 3, Name = "Data Inc" }
        );
    }
}