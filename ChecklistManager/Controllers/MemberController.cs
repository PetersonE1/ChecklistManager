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
    public class MemberController : Controller
    {
        private readonly MemberContext _context;
        private readonly ILogger<MemberController> _logger;

        public MemberController(MemberContext context, ILogger<MemberController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet(Name = "Members")]
        public async Task<IActionResult> Index(string? name)
        {
            if (name == null)
            {
                return new JsonResult(await _context.Members.ToListAsync());
            }
            else
            {
                return new JsonResult(await _context.Members.FindAsync(name));
            }
        }

        // PUT: Tasks/MassCreate
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPut(Name = "MassCreate")]
        public async Task<IActionResult> MassCreate(Member[] members)
        {
            foreach (Member member in members)
            {
                if (!_context.Members.Any(m => member.Name == m.Name))
                {
                    _context.Add(member);
                }
            }
            await _context.SaveChangesAsync();
            return new JsonResult(await _context.Members.ToListAsync());
        }

        // POST: Member/Edit/John
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost(Name = "Edit")]
        public async Task<IActionResult> Edit(string name, int score)
        {
            try
            {
                var member = await _context.Members.FindAsync(name);
                if (member == null)
                {
                    return NotFound();
                }

                _logger.Log(LogLevel.Information, "Editing member: " + member.Name);

                member.Score = score;

                _context.Update(member);
                await _context.SaveChangesAsync();

                return new JsonResult(member);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _context.Members.FindAsync(name) == null)
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

        [HttpDelete(Name = "DeleteOrResetScores"), ActionName("DeleteOrResetScores")]
        public async Task<IActionResult> DeleteOrResetScores(string? name)
        {
            if (name != null)
            {
                var member = await _context.Members.FindAsync(name);
                if (member == null)
                {
                    return StatusCode(404);
                }
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
                _logger.Log(LogLevel.Information, "Deleted member {name}", name);
                return StatusCode(200);
            }

            foreach (var member in _context.Members)
            {
                member.Score = 0;
            }
            await _context.SaveChangesAsync();

            _logger.Log(LogLevel.Information, "Reset scores");

            return StatusCode(200);
        }
    }
}
