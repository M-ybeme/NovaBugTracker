﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
    [Authorize(Roles = $"{nameof(BTRoles.Admin)},{nameof(BTRoles.ProjectManager)}")]
    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IDataProtector _protector;
        private readonly IBTCompanyService _companyService;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTInviteService _inviteService;

        public InvitesController(ApplicationDbContext context,
                                 IBTProjectService projectService,
                                 IDataProtectionProvider dataProtectorProvider,
                                 IBTCompanyService companyService,
                                 IEmailSender emailSender,
                                 UserManager<BTUser> userManager,
                                 IBTInviteService inviteService)
        {
            _context = context;
            _projectService = projectService;
            _protector = dataProtectorProvider.CreateProtector("CF.Nova.BugTr@cker.2020");
            _companyService = companyService;
            _emailSender = emailSender;
            _userManager = userManager;
            _inviteService = inviteService;
        }

        // GET: Invites
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invites!.Include(i => i.Company).Include(i => i.Invitee).Include(i => i.Invitor).Include(i => i.Project);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // GET: Invites/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();

            ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyIdAsync(companyId), "Id", "Description");
            return View();
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,InviteeEmail,InviteeFirstName,InviteeLastName,Message")] Invite invite)
        {
            ModelState.Remove("InvitorId");
            int companyId = User.Identity!.GetCompanyId();

            if (ModelState.IsValid)
            {
                try
                {

                    //encrypt code for invite
                    Guid guid = Guid.NewGuid();

                    string token = _protector.Protect(guid.ToString());
                    string email = _protector.Protect(invite.InviteeEmail!);
                    string company = _protector.Protect(companyId.ToString());

                    string? callbackUrl = Url.Action("ProcessInvite", "Invites", new {token, email, company }, protocol:Request.Scheme );

                    string body = $@"{invite.Message} <br />
                              Please join my Company. <br />
                              Click the following link to join our team. <br />
                              <a href=""{callbackUrl}"">COLLABORATE</a>";

                    string? destination = invite.InviteeEmail;

                    Company btCompany = await _companyService.GetCompanyInfoAsync(companyId);

                    string? subject = $" Nova Bug Tracker: {btCompany.CompanyName} Invite";

                    await _emailSender.SendEmailAsync(destination, subject, body);

                    invite.CompanyToken = guid;
                    invite.CompanyId = companyId;
                    invite.InviteDate = DataUtility.GetPostgresDate(DateTime.Now);
                    invite.InvitorId = _userManager.GetUserId(User);
                    invite.IsValid = true;


                    //Add Invite Service method for "AddNewInviteASync"
                    await _inviteService.AddNewInviteAsync(invite);

                    return RedirectToAction("Index", "Home");

                    //TODO: Possible use of SWAL message.

                }
                catch (Exception)
                {
                    throw;
                }
            }
            
            ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyIdAsync(companyId) , "Id", "Description", invite.ProjectId);
            return View(invite);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessInvite(string token, string email, string company)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email) || (string.IsNullOrEmpty(company)))
            {
                return NotFound();
            }

            Guid companyToken = Guid.Parse(_protector.Unprotect(token));
            string inviteeEmail = _protector.Unprotect(email);
            int companyId = int.Parse(_protector.Unprotect(company));

            try
            {
                Invite? invite = await _inviteService.GetInviteAsync(companyToken, inviteeEmail, companyId);

                if(invite != null)
                {
                    return View(invite);
                }
                return NotFound();
            }
            catch (Exception)
            {

                throw;
            }


        }

    }
}
