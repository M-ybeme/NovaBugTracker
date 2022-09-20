using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NovaBugTracker.Data;
using NovaBugTracker.Extensions;
using NovaBugTracker.Models;
using NovaBugTracker.Models.ViewModels;
using NovaBugTracker.Services.Interfaces;

namespace NovaBugTracker.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyService _companyService;

        public CompaniesController(ApplicationDbContext context, IBTRolesService rolesService, IBTCompanyService companyService)
        {
            _context = context;
            _rolesService = rolesService;
            _companyService = companyService;
        }

        //// GET: Companies
        //public async Task<IActionResult> Index()
        //{
        //      return _context.Companies != null ? 
        //                  View(await _context.Companies.ToListAsync()) :
        //                  Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
        //}

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        public async Task<IActionResult> ManageUserRoles()
        {
            //1. Add instance of the ViewModel as a List (model)
            List<ManageUserRolesViewModel> model = new();

            //2. Get CompanyId
            int companyId = User.Identity!.GetCompanyId();

            //3. Get all Comapny Users
            List<BTUser> members = await _companyService.GetMembersAsync(companyId);

            //4. Loop Over users to populate the view Model
            // - instantiate single VM
            // - use _roleService
            // - Create multiSelects

            foreach (BTUser member in members)
            {
                ManageUserRolesViewModel viewModel = new();
                IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(member);

                viewModel.BTUser = member;
                viewModel.Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", currentRoles);

                model.Add(viewModel);

            }

            //5 Return the model to the View
            return View(model);


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            // 1. Get CompanyId.
            int companyId = User.Identity!.GetCompanyId();

            // 2. Instantiate BTUser.
            BTUser? bTUser = (await _companyService.GetMembersAsync(companyId)).FirstOrDefault(m => m.Id == member.BTUser!.Id);

            // 3. Get Roles for the user.
            IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(bTUser!);

            // 4. Get Selected Role(s) for the user.
            string? selectedRole = member.SelectedRoles!.FirstOrDefault();

            // 5. Remove current role(s) and Add new role.
            if (!string.IsNullOrEmpty(selectedRole))
            {
                if (await _rolesService.RemoveUserFromRolesAsync(bTUser!, currentRoles))
                {
                    await _rolesService.AddUserToRoleAsync(bTUser!, selectedRole);
                } 
            }

            //6. Navigate
            return RedirectToAction(nameof(ManageUserRoles));
        }
    }
}

        //// GET: Companies/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,CompanyName,Description,ImageFileName,ImageFileType,ImageFileData")] Company company)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(company);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(company);
        //}

        // GET: Companies/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _context.Companies == null)
        //    {
        //        return NotFound();
        //    }

        //    var company = await _context.Companies.FindAsync(id);
        //    if (company == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(company);
        //}

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyName,Description,ImageFileName,ImageFileType,ImageFileData")] Company company)
        //{
        //    if (id != company.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(company);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!CompanyExists(company.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(company);
        //}

//        // GET: Companies/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null || _context.Companies == null)
//            {
//                return NotFound();
//            }

//            var company = await _context.Companies
//                .FirstOrDefaultAsync(m => m.Id == id);
//            if (company == null)
//            {
//                return NotFound();
//            }

//            return View(company);
//        }

//        // POST: Companies/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            if (_context.Companies == null)
//            {
//                return Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
//            }
//            var company = await _context.Companies.FindAsync(id);
//            if (company != null)
//            {
//                _context.Companies.Remove(company);
//            }
            
//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        private bool CompanyExists(int id)
//        {
//          return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
//        }
//    }
//}
