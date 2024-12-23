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
        private readonly ILogger<ChecklistTasksController> _logger;

        public ChecklistTasksController(ChecklistTaskContext context, ILogger<ChecklistTasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET -> Gets all tasks (filterable)
        // POST -> Edit a task
        // PUT -> Create a task (do nothing if there is already a communal task of the same description,
        //        or a personal task of the same description and assignment)
        // DELETE -> Delete a task


        // GET: Tasks
        [HttpGet(Name = "Tasks")]
        public async Task<IActionResult> Index(bool filterByDay = false)
        {
            if (!filterByDay)
                return new JsonResult(await _context.Tasks.ToListAsync());
            return new JsonResult((await _context.Tasks.ToListAsync()).Where(t => t.Schedule == null || t.Schedule.IsSatisfiedBy(DateTime.UtcNow)));
        }

        // PUT: Tasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPut(Name = "Create")]
        public async Task<IActionResult> Create(string description, string? schedule, string? assignedTo, int assignmentLevel)
        {
            if (_context.Tasks.Any(t => t.Description == description &&
                    (t.AssignmentLevel == (TaskAssignmentLevel)assignmentLevel &&
                    (t.AssignmentLevel == TaskAssignmentLevel.Communal || t.AssignedTo == assignedTo))))
            {
                return BadRequest("Task already exists.");
            }

            try
            {
                // Protects against ID collisions after resetting the server, and doubly serves to re-use IDs of deleted tasks
                /*while (_context.Tasks.Any(t => t.Id == ChecklistTask.nextId))
                {
                    ChecklistTask.nextId++;
                }*/

                var task = new ChecklistTask(/*ChecklistTask.nextId++, */description, schedule, assignedTo, (TaskAssignmentLevel)assignmentLevel);
                _logger.Log(LogLevel.Information, $"Creating {task.AssignmentLevel} task [ID:{task.Id}]: {task.Description}");

                _context.Add(task);
                await _context.SaveChangesAsync();
                return new JsonResult(task);
            }
            catch (FormatException)
            {
                //ChecklistTask.nextId--;
                return BadRequest("Cron expression is invalid.");
            }
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost(Name = "Edit")]
        public async Task<IActionResult> Edit(long id, string? description, string? schedule, string? assignedTo, int? assignmentLevel, int? state)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null)
                {
                    return NotFound();
                }

                _logger.Log(LogLevel.Information, "Editing task: " + task.Description);

                task.Description = description ?? task.Description;
                task.ScheduleString = schedule ?? task.ScheduleString;
                task.UpdateCron();
                task.AssignedTo = assignedTo ?? task.AssignedTo;
                task.AssignmentLevel = (TaskAssignmentLevel?)assignmentLevel ?? task.AssignmentLevel;
                task.State = (TaskState?)state ?? task.State;

                _context.Update(task);
                await _context.SaveChangesAsync();

                return new JsonResult(task);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (FormatException)
            {
                return BadRequest("Cron expression is invalid");
            }
        }

        // DELETE: Tasks/Delete/5
        [HttpDelete(Name = "Delete"), ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                _logger.Log(LogLevel.Information, "Deleted task with ID " + id);

                return StatusCode(200);
            }

            return StatusCode(404);
        }

        private bool TaskExists(long id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
