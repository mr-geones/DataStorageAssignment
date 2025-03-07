using Data.Entities;
using Data.Services;

namespace Data.Interfaces;

public interface IProjectRepository
{
    IEnumerable<ProjectEntity> GetAllProjects();

    ProjectEntity? GetProject(string projectNumber);

    ProjectEntity CreateProject(ProjectInputModel model);
}