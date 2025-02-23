using Data.Entities;
using Data.Services;

namespace Data.Interfaces;

public interface IProjectService
{
    IEnumerable<ProjectEntity> GetAllProjects();
    ProjectEntity GetProject(string projectNumber);
    ProjectEntity CreateProject(ProjectInputModel model);
}
