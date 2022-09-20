using NovaBugTracker.Models;

namespace NovaBugTracker.Services.Interfaces
{
    public interface IBTProjectService
    {
        public Task AddProjectAsync(Project project);
        public Task<bool> AddProjectManagerAsync(string userId, int projectId);
        public Task<bool> AddUserToProjectAsync(BTUser user, int projectId);
        public Task<bool> AddUserToProjectAsync(string userId, int projectId);
        public Task ArchiveProjectAsync(int projectId);
        public Task<List<Project>> GetArchivedProjectsAsync(int companyId);
        public Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string roleName);
        public Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId ,string priorityName);
        public Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId);
        public Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId);
        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId);
        public Task<Project> GetProjectByIdAsync(int projectId);
        public Task<BTUser>? GetProjectManagerAsync(int projectId);
        public Task<List<Project>> GetUnassignedProjectsAsync(int companyId);
        public Task<List<Project>> GetUserProjectsAsync(string userId);
        public Task<bool> IsUserOnProjectAsync(string userId, int projectId);
        public Task RemoveProjectManagerAsync(int projectId);
        public Task<bool> RemoveUserFromProjectAsync(BTUser user, int projectId);
        public Task RestoreProjectAsync(int projectId);
        public Task UpdateProjectAsync(Project project);
    }
}
