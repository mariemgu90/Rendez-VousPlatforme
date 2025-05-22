using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rendez_Vousdotnet.Models;
using Rendez_Vousdotnet.Data;

namespace Rendez_Vousdotnet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == "Admin")
            {
                ViewBag.Appointments = await _context.Appointments
                    .Include(a => a.Client)
                    .Include(a => a.Professional)
                    .ToListAsync();
            }
            else if (userRole == "Professional")
            {
                ViewBag.Appointments = await _context.Appointments
                    .Where(a => a.ProfessionalId == userId)
                    .Include(a => a.Client)
                    .ToListAsync();
            }
            else if (userRole == "Client")
            {
                ViewBag.Appointments = await _context.Appointments
                    .Where(a => a.ClientId == userId)
                    .Include(a => a.Professional)
                    .ToListAsync();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
