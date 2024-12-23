using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChecklistManager.Models;

namespace ChecklistManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChecklistTasksController : Controller
    {
        private readonly ChecklistTaskContext _context;

        public ChecklistTasksController(ChecklistTaskContext context)
        {
            _context = context;
        }

        // GET -> Gets all tasks (filterable)
        // POST -> Edit a task
        // PUT -> Create a task (do nothing if there is already a communal task of the same description,
        //        or a personal task of the same description and assignment)
        // DELETE -> Delete a task


        // GET: Tasks
        [HttpGet(Name = "Tasks")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tasks.ToListAsync());
        }

        // PUT: Tasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPut(Name = "Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description,Schedule,AssignedTo,AssignmentLevel")] ChecklistTask task)
        {
            if (ModelState.IsValid)
            {
                // TODO: Check if equivalent task already exists

                _context.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost(Name = "Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Description,Schedule,AssignedTo,AssignmentLevel,State")] Task task)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(task.Id))
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
            return View(task);
        }

        // DELETE: Tasks/Delete/5
        [HttpDelete(Name = "Delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
