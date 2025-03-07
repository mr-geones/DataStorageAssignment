using Data.Entities;
using Data.Interfaces;

namespace Data.Services;

public class ProjectInputModel
{
    public string Name { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string ProjectManager { get; set; } = null!;

    public int CustomerId { get; set; }

    public string Service { get; set; } = null!;

    public decimal TotalPrice { get; set; }

    public ProjectStatus Status { get; set; }

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Project name cannot be empty.");

        if (string.IsNullOrWhiteSpace(ProjectManager))
            throw new ArgumentException("Project manager cannot be empty.");

        if (CustomerId <= 0)
            throw new ArgumentException("Customer ID must be valid.");

        if (string.IsNullOrWhiteSpace(Service))
            throw new ArgumentException("Service cannot be empty.");

        if (StartDate > EndDate)
            throw new ArgumentException("Start date must be before end date.");

        if (TotalPrice < 0)
            throw new ArgumentException("Total price cannot be negative.");

        return true;
    }
}

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;

    public ProjectService(IProjectRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public IEnumerable<ProjectEntity> GetAllProjects()
    {
        try
        {
            return _repository.GetAllProjects();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving all projects: {ex.Message}");
            throw new Exception("Failed to retrieve projects. Please try again later.", ex);
        }
    }

    public ProjectEntity GetProject(string projectNumber)
    {
        if (string.IsNullOrWhiteSpace(projectNumber))
        {
            throw new ArgumentException("Project number cannot be empty.");
        }

        try
        {
            var project = _repository.GetProject(projectNumber);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with number {projectNumber} not found.");
            }

            return project;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving project {projectNumber}: {ex.Message}");
            throw new Exception($"Failed to retrieve project {projectNumber}. Please try again later.", ex);
        }
    }

    public ProjectEntity CreateProject(ProjectInputModel model)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model), "Project data cannot be null.");
        }

        try
        {
            model.Validate();
            return _repository.CreateProject(model);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating project: {ex.Message}");
            throw new Exception("Failed to create project. Please try again later.", ex);
        }
    }
}