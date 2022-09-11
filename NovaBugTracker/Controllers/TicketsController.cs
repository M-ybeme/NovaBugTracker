using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using NovaBugTracker.Services;
using NovaBugTracker.Services.Interfaces;

namespace NovaBugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;
        private readonly IBTFileService _fileService;

        public TicketsController(ApplicationDbContext context,
                                 UserManager<BTUser> userManager,
                                 IBTTicketService ticketService,
                                 IBTRolesService rolesService,
                                 IBTProjectService projectService,
                                 IBTFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _rolesService = rolesService;
            _projectService = projectService;
            _fileService = fileService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {

            var tickets = await _context.Tickets!
               .Where(t => !t.Archived)
               .Include(t => t.DeveloperUser)
               .Include(t => t.Project)
               .Include(t => t.SubmitterUser)
               .Include(t => t.TicketPriority)
               .Include(t => t.TicketStatus)
               .Include(t => t.TicketType)
               .ToListAsync();


            return View(tickets);
        }

        public async Task<IActionResult> ArchivedTickets()
        {
            List<Ticket> tickets = await _context.Tickets!
                .Where(t => t.Archived)
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .ToListAsync();

            return View(tickets);
        }

        public async Task<IActionResult> MyTickets()
        {
            string userId = _userManager.GetUserId(User);

            List<Ticket> tickets = new();
            if (User.IsInRole(nameof(BTRoles.ProjectManager)))
            {
                BTUser user = await _userManager.GetUserAsync(User);
                tickets = await _context.Tickets!
                    .Where(t => t.Project!.Members.Contains(user))
                    .Include(t => t.DeveloperUser)
                    .Include(t => t.Project)
                    .Include(t => t.SubmitterUser)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.TicketType)
                    .ToListAsync();
            }
            else
            {
                tickets = await _context.Tickets!
                    .Where(t => (t.SubmitterUserId == userId || t.DeveloperUserId == userId) && !t.Archived)
                    .Include(t => t.DeveloperUser)
                    .Include(t => t.Project)
                    .Include(t => t.SubmitterUser)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.TicketType)
                    .ToListAsync();
            }

            return View("Index", tickets);
        }

        public async Task<IActionResult> AllTickets()
        {
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            List<Ticket> tickets = await _context.Tickets!
                .Where(t => t.Project!.CompanyId == companyId && !t.Archived)
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .ToListAsync();

            return View("Index", tickets);
        }

        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            ViewData["Title"] = "Unassigned Tickets";
            return View("Index", await _ticketService.GetUnassignedTicketsAsync(companyId));
        }

        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> AssignTicket(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);

            int companyId = User.Identity!.GetCompanyId();
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            ViewData["DeveloperIds"] = new SelectList(developers, "Id", "FullName");
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicket(int TicketId, string DeveloperId)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(TicketId);

            if (DeveloperId == "unassigned")
            {
                ticket.DeveloperUserId = null;
                await _ticketService.UpdateTicketAsync(ticket);
            }
            else
            {
                if (!ticket.Project!.Members.Any(m => m.Id == DeveloperId))
                    await _projectService.AddUserToProjectAsync(DeveloperId, ticket.ProjectId);

                await _ticketService.AssignDeveloperAsync(TicketId, DeveloperId);
            }

            return RedirectToAction("Details", new { id = TicketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id, Description, TicketId")] TicketAttatchment ticketAttatchment, IFormFile formFile)
        {
            string statusMessage;

            ModelState.Remove("UserId");

            if (ModelState.IsValid && formFile != null)
            {
                ticketAttatchment.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(formFile);
                ticketAttatchment.ImageFileName = formFile.FileName;
                ticketAttatchment.ImageFileType = formFile.ContentType;

                ticketAttatchment.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                ticketAttatchment.UserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttatchment);
                statusMessage = "Success: New atachment added to ticket.";
            }
            else
            {
                statusMessage = "Error: Invalide data.";
            }

            return RedirectToAction("Details", new {id = ticketAttatchment.TicketId, message = statusMessage});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        {
            ModelState.Remove("UserId");
            if (ModelState.IsValid)
            {
                ticketComment.UserId = _userManager.GetUserId(User);
                ticketComment.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                _context.Add(ticketComment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            }
            return View(Details);
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttatchment attachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = attachment.ImageFileName!;
            byte[] fileData = attachment.ImageFileData!;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }



        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        

        // GET: Tickets/Create
        public IActionResult Create()
        {
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");

            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ArchivedByProject,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Created = DateTime.Now;
                ticket.SubmitterUserId = _userManager.GetUserId(User);
                ticket.TicketStatus = await _context.TicketStatuses!.FirstOrDefaultAsync(ts => ts.Name == nameof(BTTicketStatuses.New));

                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)},{nameof(BTRoles.Developer)},{nameof(BTRoles.Submitter)}")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    ticket.Updated = DateTime.Now;

                    Ticket? oldRecord = await _context.Tickets!.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                    string[] propsToSkip = { "Id", "Created", "Updated", "ProjectId", "History", "Attachments", "Comments", "Archived", "ArchivedByProject" };

                    foreach (PropertyInfo property in ticket.GetType().GetProperties())
                    {
                        if (propsToSkip.Contains(property.Name)) continue;

                        bool isChanged = property.PropertyType == typeof(string) ?
                                            !string.Equals(property.GetValue(ticket), property.GetValue(oldRecord))
                                            : property.GetValue(ticket) != property.GetValue(oldRecord);

                        if (isChanged)
                        {
                            TicketHistory newChange = new()
                            {
                                TicketId = ticket.Id,
                                PropertyName = property.Name,
                                Created = DateTime.Now,
                                OldValue = property.GetValue(oldRecord)!.ToString(),
                                NewValue = property.GetValue(ticket)!.ToString(),
                                UserId = _userManager.GetUserId(User)
                            };

                            ticket.History.Add(newChange);
                        }
                    }

                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
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
            ViewData["DeveloperUserId"] = new SelectList(_context.Set<BTUser>(), "Id", "FullName", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Set<BTUser>(), "Id", "FullName", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchivedConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                ticket.Archived = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                ticket.Archived = true;
                //_context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
