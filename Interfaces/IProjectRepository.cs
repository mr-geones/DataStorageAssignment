using Data.Entities;
using Data.Services;

namespace Data.Interfaces;

public interface IProjectRepository
{
    IEnumerable<ProjectEntity> GetAllProjects();
    ProjectEntity? GetProject(string projectNumber); // Updated to match the implementation
    ProjectEntity CreateProject(ProjectInputModel model);
}

