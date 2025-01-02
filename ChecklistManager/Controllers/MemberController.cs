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
