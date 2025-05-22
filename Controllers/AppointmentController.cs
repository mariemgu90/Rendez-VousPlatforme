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

        // GET: Formulaire de création de rendez-vous (Client uniquement)
        [Authorize(Roles = "Client")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Specialities = new List<string> { "Avocat", "Médecin" };
            return View();
        }

        // POST: Création du rendez-vous (Client uniquement)
        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> Create(Appointment appointment, int professionalId)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim, out var clientId))
                {
                    return Forbid();
                }

                appointment.ClientId = clientId;
                appointment.ProfessionalId = professionalId;
                appointment.Status = "En cours";

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Rendez-vous créé avec succès";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Specialities = new List<string> { "Avocat", "Médecin" };
            return View(appointment);
        }

        // GET: Récupérer les professionnels d'une spécialité (utilisé avec AJAX)
        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> GetProfessionals(string speciality)
        {
            var professionals = await _context.Users
                .Where(u => u.Role == "Professional" && u.Speciality == speciality)
                .Select(u => new {
                    id = u.Id,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    speciality = u.Speciality
                })
                .ToListAsync();

            return Json(professionals);
        }

        // POST: Mise à jour du statut du rendez-vous (Professional uniquement)
        [Authorize(Roles = "Professional")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var professionalId))
                return Forbid();

            if (appointment.ProfessionalId != professionalId)
                return Forbid();

            appointment.Status = status;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        // DELETE: Suppression du rendez-vous (Client uniquement)
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var clientId))
                return Forbid();

            if (appointment.ClientId != clientId)
                return Forbid();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
