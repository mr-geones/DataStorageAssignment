namespace Data.Entities;

public class ProjectEntity
{
    public string ProjectNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string ProjectManager { get; set; } = null!;
    public string Customer { get; set; } = null!;
    public string Service { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public ProjectStatus Status { get; set; }

    // Default constructor for EF Core
    public ProjectEntity() { }

    public ProjectEntity(string projectNumber)
    {
        ProjectNumber = projectNumber ?? throw new ArgumentNullException(nameof(projectNumber));
    }
}

public enum ProjectStatus
{
    NotStarted,
    Ongoing,
    Completed
}
