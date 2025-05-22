using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rendez_Vousdotnet.Data;
using Rendez_Vousdotnet.Models;
using System.Security.Claims;

namespace Rendez_Vousdotnet.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // Affichage du formulaire d'inscription
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Specialities = new List<string> { "Avocat", "Médecin" };
            return View();
        }

        // Traitement de l'inscription
        [HttpPost]
        public async Task<IActionResult> Register(User user, string confirmPassword)
        {
            ViewBag.Specialities = new List<string> { "Avocat", "Médecin" };

            if (!ModelState.IsValid)
                return View(user);

            if (user.Password != confirmPassword)
            {
                ModelState.AddModelError("", "Les mots de passe ne correspondent pas");
                return View(user);
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Cet email est déjà utilisé");
                return View(user);
            }

            user.Password = _passwordHasher.HashPassword(user, user.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await SignInUser(user);

            return RedirectToAction("RedirectToDashboard");
        }

        // Affichage du formulaire de connexion
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Traitement de la connexion
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email ou mot de passe incorrect");
                return View();
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Email ou mot de passe incorrect");
                return View();
            }

            await SignInUser(user);

            return RedirectToAction("RedirectToDashboard");
        }

        // Redirection automatique selon rôle après login/register
        [Authorize]
        public IActionResult RedirectToDashboard()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Client" => RedirectToAction("Dashboard", "Client"),
                "Professionnel" => RedirectToAction("Dashboard", "Professionnel"),
                _ => RedirectToAction("Index", "Home"),
            };
        }

        // Déconnexion pour tous les types d'utilisateurs
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Login", "Account");
        }

        // Méthode privée pour connecter un utilisateur
        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);
        }
    }
}
