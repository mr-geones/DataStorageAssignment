namespace Data.Entities;

public class ProjectEntity
{
    public string ProjectNumber { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string ProjectManager { get; set; } = null!;

    public int CustomerId { get; set; }

    public CustomerEntity Customer { get; set; } = null!;

    public string Service { get; set; } = null!;

    public decimal TotalPrice { get; set; }

    public ProjectStatus Status { get; set; }

    public ProjectEntity() { }

    public ProjectEntity(string projectNumber)
    {
        ProjectNumber = projectNumber ?? throw new ArgumentNullException(nameof(projectNumber));
    }

    public int GetRemainingDays()
    {
        int daysLeft = (EndDate - DateTime.Today).Days;
        return daysLeft > 0 ? daysLeft : 0;
    }

    public bool IsBehindSchedule()
    {
        return Status == ProjectStatus.Ongoing && DateTime.Today > EndDate;
    }
}

public enum ProjectStatus
{
    NotStarted,
    Ongoing,
    Completed
}