using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rendez_Vousdotnet.Data;
using Rendez_Vousdotnet.Models;
using System.Security.Claims;

namespace Rendez_Vousdotnet.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Client")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Specialities = new List<string> { "Avocat", "Médecin" };
            return View();
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> Create(Appointment appointment, string speciality)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Forbid();
                }

                appointment.ClientId = userId;
                appointment.Status = "En cours";

                // Déclare le professionnel ici
                var professional = await _context.Users
                    .FirstOrDefaultAsync(u => u.Role == "Professional" && u.Speciality == speciality);

                if (professional == null)
                {
                    ModelState.AddModelError("", "Aucun professionnel disponible pour cette spécialité");
                    ViewBag.Specialities = new List<string> { "Avocat", "Médecin" };
                    return View(appointment);
                }

                appointment.ProfessionalId = professional.Id;

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Specialities = new List<string> { "Avocat", "Médecin" };
            return View(appointment);
        }

        [Authorize(Roles = "Professional")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Forbid();
            }

            if (appointment.ProfessionalId != userId)
            {
                return Forbid();
            }

            appointment.Status = status;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Forbid();
            }

            if (appointment.ClientId != userId)
            {
                return Forbid();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
