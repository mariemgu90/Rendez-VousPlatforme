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

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Specialities = new List<string> { "Avocat", "Médecin" };
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
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
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Specialities = new List<string> { "Avocat", "Médecin" };
            return View(user);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

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
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }

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
