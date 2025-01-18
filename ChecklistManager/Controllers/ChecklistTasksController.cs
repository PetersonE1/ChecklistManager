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
        private readonly MemberContext _memberContext;
        private readonly ILogger<ChecklistTasksController> _logger;

        public ChecklistTasksController(ChecklistTaskContext context, MemberContext memberContext, ILogger<ChecklistTasksController> logger)
        {
            _context = context;
            _memberContext = memberContext;
            _logger = logger;
        }

        // GET -> Gets all tasks (filterable)
        // POST -> Edit a task
        // PUT -> Create a task (do nothing if there is already a communal task of the same description,
        //        or a personal task of the same description and assignment)
        // DELETE -> Delete a task


        // GET: Tasks
        [HttpGet(Name = "Tasks")]
        public async Task<IActionResult> Index(bool filterByDay = false, int assignmentLevel = -1, string? assignedTo = null)
        {
            List<ChecklistTask> tasks = await _context.Tasks.ToListAsync();

            if (filterByDay)
                tasks = tasks.Where(t => t.Schedule == null || t.Schedule.IsSatisfiedBy(DateTime.Now)).ToList();

            if (assignmentLevel != -1)
                tasks = tasks.Where(t => t.AssignmentLevel == (TaskAssignmentLevel)assignmentLevel).ToList();

            if (assignedTo != null)
                tasks = tasks.Where(t => t.AssignedTo == assignedTo).ToList();

            return new JsonResult(tasks);
        }

        // PUT: Tasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPut(Name = "Create")]
        public async Task<IActionResult> Create(string description, string? schedule, string? assignedTo, int assignmentLevel, bool highPriority)
        {
            if (_context.Tasks.Any(t => t.Description == description &&
                    (t.AssignmentLevel == (TaskAssignmentLevel)assignmentLevel &&
                    (t.AssignmentLevel == TaskAssignmentLevel.Communal || t.AssignedTo == assignedTo))))
            {
                return BadRequest("Task already exists.");
            }

            try
            {
                var task = new ChecklistTask(description, schedule, assignedTo, (TaskAssignmentLevel)assignmentLevel, highPriority);
                _logger.Log(LogLevel.Information, $"Creating {task.AssignmentLevel} task [ID:{task.Id}]: {task.Description}");

                _context.Add(task);
                await _context.SaveChangesAsync();

                if (assignedTo != null && _memberContext.Members.Find(assignedTo) == null)
                {
                    _memberContext.Add(new Member(assignedTo));
                    await _memberContext.SaveChangesAsync();
                }

                return new JsonResult(task);
            }
            catch (FormatException)
            {
                return BadRequest("Cron expression is invalid.");
            }
        }

        // PUT: Tasks/MassCreate
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPut("~/ChecklistTasks/masscreate", Name = "MassCreate")]
        public async Task<IActionResult> MassCreate(ChecklistTask[] tasks)
        {
            foreach (ChecklistTask task in tasks)
            {
                if (!_context.Tasks.Any(t => task.Id == t.Id))
                {
                    _context.Add(task);
                }
            }
            await _context.SaveChangesAsync();
            return new JsonResult(await _context.Tasks.ToListAsync());
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost(Name = "Edit")]
        public async Task<IActionResult> Edit(long id, string? description, string? schedule, string? assignedTo, string? doneBy, int? assignmentLevel, int? state, bool? highPriority)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null)
                {
                    return NotFound();
                }

                _logger.Log(LogLevel.Information, "Editing task: " + task.Description);

                TaskState? taskState = (TaskState?)state;
                if (taskState != null && taskState != task.State)
                {
                    if (doneBy != null)
                        task.DoneBy = doneBy;

                    var member = await _memberContext.Members.FindAsync(task.DoneBy);
                    if (member != null && !(task.State == TaskState.Incomplete && taskState == TaskState.InProgress))
                    {
                        member.Score += taskState == TaskState.Complete ? 1 : -1;
                        await _memberContext.SaveChangesAsync();
                    }
                }

                task.Description = description ?? task.Description;
                task.ScheduleString = schedule ?? task.ScheduleString;
                task.UpdateCron();
                task.AssignedTo = assignedTo ?? task.AssignedTo;
                task.DoneBy = doneBy ?? task.DoneBy;
                task.AssignmentLevel = (TaskAssignmentLevel?)assignmentLevel ?? task.AssignmentLevel;
                task.State = taskState ?? task.State;
                task.HighPriority = highPriority ?? task.HighPriority;

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
