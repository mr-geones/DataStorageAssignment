using Data.Entities;
using Data.Interfaces;
using Data.Services;

namespace Data.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ProjectContext _context;

    public ProjectRepository(ProjectContext context)
    {
        _context = context;
    }

    public IEnumerable<ProjectEntity> GetAllProjects()
    {
        return _context.Projects.ToList();
    }

    public ProjectEntity? GetProject(string projectNumber)
    {
        return _context.Projects.Find(projectNumber);
    }

    public ProjectEntity CreateProject(ProjectInputModel model)
    {
        var project = new ProjectEntity
        {
            ProjectNumber = GetNextProjectNumber(),
            Name = model.Name,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            ProjectManager = model.ProjectManager,
            CustomerId = model.CustomerId,
            Service = model.Service,
            TotalPrice = model.TotalPrice,
            Status = model.Status
        };

        _context.Projects.Add(project);
        _context.SaveChanges();

        return project;
    }

    private string GetNextProjectNumber()
    {
        var lastProject = _context.Projects
            .OrderByDescending(p => p.ProjectNumber)
            .FirstOrDefault();

        if (lastProject == null)
        {
            return "P001";
        }

        var lastProjectNumber = lastProject.ProjectNumber;
        var numberPart = int.Parse(lastProjectNumber.Substring(1));
        var nextNumber = numberPart + 1;

        return $"P{nextNumber:D3}";
    }
}