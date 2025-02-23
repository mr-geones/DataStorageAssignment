using Data.Entities;
using Data.Interfaces;
using Data.Repositories;

namespace Data.Services;

public class ProjectInputModel
{
    public string Name { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string ProjectManager { get; set; } = null!;
    public string Customer { get; set; } = null!;
    public string Service { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public ProjectStatus Status { get; set; }

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) ||
            string.IsNullOrWhiteSpace(ProjectManager) ||
            string.IsNullOrWhiteSpace(Customer) ||
            string.IsNullOrWhiteSpace(Service))
            return false;
        if (StartDate > EndDate) throw new ArgumentException("Start date must be before end date.");
        if (TotalPrice < 0) throw new ArgumentException("Total price cannot be negative.");

        return true;
    }
}

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;

    public ProjectService(IProjectRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<ProjectEntity> GetAllProjects()
    {
        return _repository.GetAllProjects();
    }

    public ProjectEntity GetProject(string projectNumber)
    {
        var project = _repository.GetProject(projectNumber);
        if (project == null)
        {
            throw new InvalidOperationException($"Project with number {projectNumber} not found.");
        }
        return project;
    }

    public ProjectEntity CreateProject(ProjectInputModel model)
    {
        return _repository.CreateProject(model);
    }
}
