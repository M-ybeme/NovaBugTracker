using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NovaBugTracker.Data;
using NovaBugTracker.Models;

namespace NovaBugTracker.Controllers
{
    public class TicketAttatchmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketAttatchmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TicketAttatchments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TicketAttatchments!.Include(t => t.Ticket).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TicketAttatchments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TicketAttatchments == null)
            {
                return NotFound();
            }

            var ticketAttatchment = await _context.TicketAttatchments
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketAttatchment == null)
            {
                return NotFound();
            }

            return View(ticketAttatchment);
        }

        // GET: TicketAttatchments/Create
        public IActionResult Create()
        {
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: TicketAttatchments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Created,TicketId,UserId,ImageFileName,ImageFileData,ImageFileType")] TicketAttatchment ticketAttatchment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticketAttatchment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketAttatchment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketAttatchment.UserId);
            return View(ticketAttatchment);
        }

        // GET: TicketAttatchments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TicketAttatchments == null)
            {
                return NotFound();
            }

            var ticketAttatchment = await _context.TicketAttatchments.FindAsync(id);
            if (ticketAttatchment == null)
            {
                return NotFound();
            }
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketAttatchment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketAttatchment.UserId);
            return View(ticketAttatchment);
        }

        // POST: TicketAttatchments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Created,TicketId,UserId,ImageFileName,ImageFileData,ImageFileType")] TicketAttatchment ticketAttatchment)
        {
            if (id != ticketAttatchment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketAttatchment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketAttatchmentExists(ticketAttatchment.Id))
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
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketAttatchment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketAttatchment.UserId);
            return View(ticketAttatchment);
        }

        // GET: TicketAttatchments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TicketAttatchments == null)
            {
                return NotFound();
            }

            var ticketAttatchment = await _context.TicketAttatchments
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketAttatchment == null)
            {
                return NotFound();
            }

            return View(ticketAttatchment);
        }

        // POST: TicketAttatchments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TicketAttatchments == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TicketAttatchments'  is null.");
            }
            var ticketAttatchment = await _context.TicketAttatchments.FindAsync(id);
            if (ticketAttatchment != null)
            {
                _context.TicketAttatchments.Remove(ticketAttatchment);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketAttatchmentExists(int id)
        {
          return (_context.TicketAttatchments?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
