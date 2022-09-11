using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NovaBugTracker.Data;
using NovaBugTracker.Extensions;
using NovaBugTracker.Models;
using NovaBugTracker.Models.Enums;
using NovaBugTracker.Models.ViewModels;
using NovaBugTracker.Services;
using NovaBugTracker.Services.Interfaces;

namespace NovaBugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;

        public ProjectsController(ApplicationDbContext context, UserManager<BTUser> userManager, IBTFileService fileService, IBTProjectService projectService, IBTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _projectService = projectService;
            _rolesService = rolesService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            //Get CompanyId
            //int companyId = User.Identity!.GetCompanyId();
            int companyId = User.Identity!.GetCompanyId();
            var projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            
            return View(projects);
        }


        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity!.GetCompanyId();

            return View(await _projectService.GetArchivedProjectsAsync(companyId));
        }

        public async Task<IActionResult> MyProjects()
        {
            BTUser me = await _userManager.GetUserAsync(User);

            //TODO use service
            List<Project> myProjects = await _context.Projects!
                .Where(p => !p.Archived && p.Members.Contains(me))
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .ToListAsync();

            return View("Index", myProjects);
        }

        public async Task<IActionResult> AllProjects()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> allProjects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            return View("Index", allProjects);
        }

        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity!.GetCompanyId();
            List<Project> unassignedProjects = await _projectService.GetUnassignedProjectsAsync(companyId);
            return View("Index", unassignedProjects);
        }

        //Get:

        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> AssignProjectManager(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            AssignPMViewModel model = new();
            //Get Company ID
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            model.Project = await _projectService.GetProjectByIdAsync(id.Value);

            //get PM if it exixts.
            string? currentPMId = (await _projectService.GetProjectManagerAsync(model.Project.Id)!)?.Id;
            //service calll to roleservice to get all pm for the company.
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName", currentPMId);


            return View(model);
        }


        //Post:
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectManager(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PMId))
            {
                await _projectService.AddProjectManagerAsync(model.PMId, model.Project!.Id);                

                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("PMId", "No Project Manager choosen! Please Select a PM!");

            //companyId
            int companyId = User.Identity!.GetCompanyId();

            model.Project = await _projectService.GetProjectByIdAsync(model.Project!.Id);

            //get PM if it exixts.
            string? currentPMId = (await _projectService.GetProjectManagerAsync(model.Project.Id)!)?.Id;

            //service calll to roleservice to get all pm for the company.
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName", currentPMId);


            return View(model);
        }


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            Project project = await _projectService.GetProjectByIdAsync(id.Value);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        //get:
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> AddUsersToProject(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            int companyId = User.Identity!.GetCompanyId();

            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            List<BTUser> assignedSubmitters = await _projectService.GetProjectMembersByRoleAsync(id.Value, nameof(BTRoles.Submitter));
            List<BTUser> assignedDevelopers = await _projectService.GetProjectMembersByRoleAsync(id.Value, nameof(BTRoles.Developer));

            Project project = await _projectService.GetProjectByIdAsync(id.Value);

            ViewData["Developers"] = new MultiSelectList(developers, "Id", "FullName", assignedDevelopers.Select(d => d.Id));
            ViewData["Submitters"] = new MultiSelectList(submitters, "Id", "FullName", assignedSubmitters.Select(d => d.Id));

            return View(project);
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> AddUsersToProject(int id, List<string> submitters, List<string> developers)
        {
            List<string> existingMembers = (await _projectService.GetAllProjectMembersExceptPMAsync(id)).Select(m => m.Id).ToList();

            foreach (string submitter in submitters)
            {
                await _projectService.AddUserToProjectAsync(submitter, id);
            }

            foreach (string developer in developers)
            {
                await _projectService.AddUserToProjectAsync(developer, id);
            }

            foreach (string existingMember in existingMembers)
            {
                if (!submitters.Contains(existingMember) && !developers.Contains(existingMember))
                {
                    var user = _context.Users.Find(existingMember);
                    await _projectService.RemoveUserFromProjectAsync(user!, id);
                }
            }

            return RedirectToAction("Details", new { id = id });
        }

        public async Task<IActionResult> RemoveUserFromProject(int id, string user)
        {
            await _projectService.RemoveUserFromProjectAsync(_context.Users.Find(user)!, id);
            return RedirectToAction("Details", new { id = id });
        }



        // GET: Projects/Create
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public IActionResult Create()
        {
            // TODO: Abstract use of _context

            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate,ProjectPriorityId,ImageFormFile")] Project project)
        {
            

            if (ModelState.IsValid)
            {
                int companyId = User.Identity!.GetCompanyId();
                project.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                await _projectService.AddProjectAsync(project);
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            Project project = await _projectService.GetProjectByIdAsync(id.Value);

            if (project == null ||
                project.CompanyId != (await _userManager.GetUserAsync(User)).CompanyId)
            {
                return NotFound();
            }

            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileData,ImageFileType,Archived,ImageFormFile")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int companyId = User.Identity!.GetCompanyId();

                    if (project.ImageFormFile != null)
                    {
                        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }

                    await _projectService.UpdateProjectAsync(project);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Archive/5
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _projectService.GetProjectByIdAsync(id.Value);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }

            await _projectService.ArchiveProjectAsync(id);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Restore(int id)
        {
            await _projectService.RestoreProjectAsync(id);

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
