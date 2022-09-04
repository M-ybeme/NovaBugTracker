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
            var projects = await _context.Projects!
                 .Where(p => !p.Archived)
                 .Include(p => p.Company)
                 .Include(p => p.ProjectPriority)
                 .ToListAsync();

            return View(projects);
        }

        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

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
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            List<Project> allProjects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            return View("Index", allProjects);
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
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            model.Project = await _projectService.GetProjectByIdAsync(id.Value);
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName");


            return View(model);
        }


        //Post:
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectManager(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PMId))
            {
                //AddMP to project and TODO: Enhance this process.
                Project project = await _projectService.GetProjectByIdAsync(model.Project!.Id);
                BTUser? projectManager = await _context.Users.FindAsync(model.PMId);

                project.Members.Add(projectManager!);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(AssignProjectManager), new { id = model.Project!.Id});
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
            //ModelState.Remove("Created");

            if (ModelState.IsValid)
            {
                project.CompanyId = (await _userManager.GetUserAsync(User)).CompanyId;
                project.Created = DateTime.Now;

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
                    project.CompanyId = (await _userManager.GetUserAsync(User)).CompanyId;

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
