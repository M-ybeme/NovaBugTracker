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
        private readonly IBTRolesService _roleService;

        public BTProjectService(ApplicationDbContext context, UserManager<BTUser> userManager, IBTRolesService roleService)
        {
            _context = context;
            _userManager = userManager;
            _roleService = roleService;
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

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {

            try
            {
                BTUser currentPM = await GetProjectManagerAsync(projectId)!;
                BTUser? selectedPM = await _context.Users.FindAsync(userId)!;
                
                
                //remove current PM
                if(currentPM != null)
                {
                    await RemoveProjectManagerAsync(projectId);
                }
                //Add new PM
                try
                {
                    await AddUserToProjectAsync(selectedPM!, projectId);
                    return true;
                }
                catch (Exception)
                {

                    throw;
                }

            }
            catch (Exception)
            {

                throw;
            }
            //await RemoveProjectManagerAsync(projectId);
            //await AddUserToProjectAsync(userId, projectId);
        }

        public async Task<bool> AddUserToProjectAsync(BTUser user, int projectId)
        {
            try
            {
                
                Project project = await GetProjectByIdAsync(projectId);

                bool onProject = !project.Members.Any(m => m.Id == user.Id);

                ///if on prject is true run true else return false.

                //check if BTUser is on project
                if (onProject)
                {
                    project.Members.Add(user);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;

                //if (project.CompanyId != user.CompanyId) return false;

                //if (!project.Members.Contains(user)) project.Members.Add(user);

                //await _context.SaveChangesAsync();
                //return true;
            }
            catch { throw; }
        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser btUser = _context.Users.FirstOrDefault(u => u.Id == userId)!;
                await AddUserToProjectAsync(btUser, projectId);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
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
            BTUser? pm = await GetProjectManagerAsync(projectId)!;
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

        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName)
        {
            return await _context.Projects!
                   .Where(p => p.CompanyId == companyId && !p.Archived && p.ProjectPriority!.Name == priorityName)
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

        public async Task<BTUser>? GetProjectManagerAsync(int projectId)
        {
            try
            {
                Project project = await GetProjectByIdAsync(projectId);

                foreach (BTUser member in project.Members)
                {
                    if (await _roleService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        return member;
                    }
                }

                return null!;
            }
            catch { throw; }
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string roleName)
        {

            try
            {
                Project? project = await _context.Projects!.Include(p => p.Members)
                                                         .FirstOrDefaultAsync(p => p.Id == projectId);
                List<BTUser> members = new();
                foreach (BTUser btUser in project!.Members)
                {
                    if (await _userManager.IsInRoleAsync(btUser, roleName))
                    {
                        members.Add(btUser);
                    }
                }

                return members;

            }
            catch (Exception)
            { throw; }

        }

        public async Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
        {
            List<Project> companyProjects = await GetAllProjectsByCompanyIdAsync(companyId);
            List<Project> unassignedProjects = new();

            foreach (Project project in companyProjects)
            {
                if (await GetProjectManagerAsync(project.Id)! == null) unassignedProjects.Add(project);
            }

            return unassignedProjects;
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            try
            {
                List<Project>? projects = (await _context.Users
                                                         .Include(u => u.Projects)!
                                                            .ThenInclude(p => p.Company)
                                                         .Include(u => u.Projects)!
                                                            .ThenInclude(p => p.Members)
                                                         .Include(u => u.Projects)!
                                                            .ThenInclude(p => p.Tickets)
                                                         .Include(u => u.Projects)!
                                                            .ThenInclude(t => t.Tickets)
                                                                .ThenInclude(t => t.DeveloperUser)
                                                         .Include(u => u.Projects)!
                                                             .ThenInclude(t => t.Tickets)
                                                                 .ThenInclude(t => t.SubmitterUser)
                                                         .Include(u => u.Projects)!
                                                             .ThenInclude(t => t.Tickets)
                                                                 .ThenInclude(t => t.TicketPriority)
                                                         .Include(u => u.Projects)!
                                                             .ThenInclude(t => t.Tickets)
                                                                 .ThenInclude(t => t.TicketStatus)
                                                         .Include(u => u.Projects)!
                                                             .ThenInclude(t => t.Tickets)
                                                                 .ThenInclude(t => t.TicketType)
                                                         .FirstOrDefaultAsync(u => u.Id == userId))?.Projects!.ToList();
                return projects!;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            Project project = await GetProjectByIdAsync(projectId);
            return await _context.Users.Where(u => u.CompanyId == companyId && !project.Members.Contains(u)).ToListAsync();
        }

        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {

            try
            {
                Project? project = await GetProjectByIdAsync(projectId);
                if (project == null)
                {
                    //checked to see if the user(Id) is async project member
                    return project!.Members.Any(m => m.Id == userId); 
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
            //Project project = await GetProjectByIdAsync(projectId);

            //return project.Members.Any(m => m.Id == userId);
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            try
            {
                Project project = await GetProjectByIdAsync(projectId);

                foreach (BTUser member in project.Members)
                {
                    if (await _roleService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        //Remove BTUser from Project
                        await RemoveUserFromProjectAsync(member, projectId);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            //Project project = await GetProjectByIdAsync(projectId);
            //BTUser? pm = await GetProjectManagerAsync(projectId)!;

            //if (pm != null)
            //{ project.Members.Remove(pm);

            //  await _context.SaveChangesAsync();
            //}
        }

        public async Task<bool> RemoveUserFromProjectAsync(BTUser user, int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId);

                bool onProject = project.Members.Any(m => m.Id == user.Id);

                ///if on prject is true run true else return false.

                //check if BTUser is on project
                if(onProject)
                {
                    project.Members.Remove(user);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }



            //Project project = await GetProjectByIdAsync(projectId);
            //BTUser user = await _userManager.FindByIdAsync(userId);

            //project.Members.Remove(user);

            //await _context.SaveChangesAsync();
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
