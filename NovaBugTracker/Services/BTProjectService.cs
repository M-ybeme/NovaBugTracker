using NovaBugTracker.Data;
using NovaBugTracker.Models;
using NovaBugTracker.Models.Enums;
using NovaBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace NovaBugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;

        public BTProjectService(ApplicationDbContext context, UserManager<BTUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task AddProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        public async Task AddProjectManagerAsync(string userId, int projectId)
        {
            await RemoveProjectManagerAsync(projectId);
            await AddUserToProjectAsync(userId, projectId);
        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser user = _context.Users.Find(userId)!;
                Project project = await GetProjectByIdAsync(projectId);

                if (project.CompanyId != user.CompanyId) return false;

                if (!project.Members.Contains(user)) project.Members.Add(user);

                await _context.SaveChangesAsync();
                return true;
            }
            catch { throw; }
        }

        public async Task ArchiveProjectAsync(int projectId)
        {
            try
            {
                Project project = await GetProjectByIdAsync(projectId);

                project.Archived = true;

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = (ticket.Archived ? false : true);
                    ticket.Archived = true;
                }

                await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        public async Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            BTUser? pm = await GetProjectManagerAsync(projectId);
            Project project = await GetProjectByIdAsync(projectId);

            return pm == null ? project.Members.Where(m => m != pm).ToList()
                : project.Members.ToList();
        }

        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                return await _context.Projects!
                    .Where(p => p.CompanyId == companyId && !p.Archived)
                    .Include(p => p.Tickets).ThenInclude(t => t.TicketPriority)
                    .Include(p => p.Tickets).ThenInclude(t => t.TicketType)
                    .Include(p => p.Tickets).ThenInclude(t => t.TicketStatus)
                    .Include(p => p.Tickets).ThenInclude(t => t.DeveloperUser)
                    .Include(p => p.Tickets).ThenInclude(t => t.SubmitterUser)
                    .Include(p => p.Tickets).ThenInclude(t => t.Attatchment)
                    .Include(p => p.Tickets).ThenInclude(t => t.Comments)
                    .Include(p => p.Tickets).ThenInclude(t => t.History)
                    .Include(p => p.Company)
                    .Include(p => p.ProjectPriority)
                    .Include(p => p.Members)
                    .ToListAsync();
            }
            catch { throw; }
        }

        public async Task<List<Project>> GetArchivedProjectsAsync(int companyId)
        {
            try
            {
                return await _context.Projects!
                    .Where(p => p.CompanyId == companyId && p.Archived)
                    .Include(p => p.Tickets).ThenInclude(t => t.TicketPriority)
                    .Include(p => p.Tickets).ThenInclude(t => t.TicketType)
                    .Include(p => p.Tickets).ThenInclude(t => t.TicketStatus)
                    .Include(p => p.Tickets).ThenInclude(t => t.DeveloperUser)
                    .Include(p => p.Tickets).ThenInclude(t => t.SubmitterUser)
                    .Include(p => p.Tickets).ThenInclude(t => t.Attatchment)
                    .Include(p => p.Tickets).ThenInclude(t => t.Comments)
                    .Include(p => p.Tickets).ThenInclude(t => t.History)
                    .Include(p => p.Company)
                    .Include(p => p.ProjectPriority)
                    .Include(p => p.Members)
                    .ToListAsync();
            }
            catch { throw; }
        }

        public async Task<Project> GetProjectByIdAsync(int projectId)
        {
            try
            {
                Project? project = await _context.Projects!
                    .Where(p => p.Id == projectId)
                    .Include(p => p.Tickets).ThenInclude(t => t.TicketPriority)
                    .Include(p => p.Tickets).ThenInclude(t => t.TicketType)
                    .Include(p => p.Tickets).ThenInclude(t => t.TicketStatus)
                    .Include(p => p.Tickets).ThenInclude(t => t.DeveloperUser)
                    .Include(p => p.Tickets).ThenInclude(t => t.SubmitterUser)
                    .Include(p => p.Tickets).ThenInclude(t => t.Attatchment)
                    .Include(p => p.Tickets).ThenInclude(t => t.Comments)
                    .Include(p => p.Tickets).ThenInclude(t => t.History)
                    .Include(p => p.Company)
                    .Include(p => p.ProjectPriority)
                    .Include(p => p.Members)
                    .FirstOrDefaultAsync();

                return project!;
            }
            catch { throw; }
        }

        public async Task<BTUser?> GetProjectManagerAsync(int projectId)
        {
            try
            {
                Project project = await GetProjectByIdAsync(projectId);

                foreach (BTUser member in project.Members)
                {
                    if (await _userManager.IsInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        return member;
                    }
                }

                return null;
            }
            catch { throw; }
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string roleName)
        {
            List<BTUser> membersInRole = new List<BTUser>();
            Project project = await GetProjectByIdAsync(projectId);

            foreach (BTUser member in project.Members)
            {
                if (await _userManager.IsInRoleAsync(member, roleName)) membersInRole.Add(member);
            }

            return membersInRole;
        }

        public async Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
        {
            List<Project> companyProjects = await GetAllProjectsByCompanyIdAsync(companyId);
            List<Project> unassignedProjects = new();

            foreach (Project project in companyProjects)
            {
                if (await GetProjectManagerAsync(project.Id) == null) unassignedProjects.Add(project);
            }

            return unassignedProjects;
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            BTUser user = await _userManager.FindByIdAsync(userId);
            return await _context.Projects!.Where(p => p.Members.Contains(user)).ToListAsync();
        }

        public async Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            Project project = await GetProjectByIdAsync(projectId);
            return await _context.Users.Where(u => u.CompanyId == companyId && !project.Members.Contains(u)).ToListAsync();
        }

        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            Project project = await GetProjectByIdAsync(projectId);

            return project.Members.Any(m => m.Id == userId);
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            Project project = await GetProjectByIdAsync(projectId);
            BTUser? pm = await GetProjectManagerAsync(projectId);

            if (pm != null) project.Members.Remove(pm);

            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            Project project = await GetProjectByIdAsync(projectId);
            BTUser user = await _userManager.FindByIdAsync(userId);

            project.Members.Remove(user);

            await _context.SaveChangesAsync();
        }

        public async Task RestoreProjectAsync(int projectId)
        {
            try
            {
                Project project = await GetProjectByIdAsync(projectId);

                project.Archived = false;

                foreach (Ticket ticket in project.Tickets)
                {
                    if (ticket.ArchivedByProject) ticket.Archived = false;
                    ticket.ArchivedByProject = false;
                }

                await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch { throw; }
        }
    }
}
