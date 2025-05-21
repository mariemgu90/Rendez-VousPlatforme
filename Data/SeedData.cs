using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rendez_Vousdotnet.Models;

namespace Rendez_Vousdotnet.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>()))
            {
                if (context.Users.Any())
                {
                    return; // La base de données a déjà été initialisée
                }

                var passwordHasher = new PasswordHasher<User>();

                // Créer un admin
                var admin = new User
                {
                    FirstName = "Admin",
                    LastName = "Admin",
                    Email = "admin@example.com",
                    Password = "Admin123!",
                    Role = "Admin"
                };
                admin.Password = passwordHasher.HashPassword(admin, admin.Password);
                context.Users.Add(admin);

                // Créer des professionnels
                var professionals = new List<User>
            {
                new User { FirstName = "Jean", LastName = "Dupont", Email = "jean.dupont@example.com", Password = "Professional123!", Role = "Professional", Speciality = "Avocat" },
                new User { FirstName = "Marie", LastName = "Martin", Email = "marie.martin@example.com", Password = "Professional123!", Role = "Professional", Speciality = "Médecin" }
            };

                foreach (var professional in professionals)
                {
                    professional.Password = passwordHasher.HashPassword(professional, professional.Password);
                    context.Users.Add(professional);
                }

                // Créer un client
                var client = new User
                {
                    FirstName = "Client",
                    LastName = "Test",
                    Email = "client@example.com",
                    Password = "Client123!",
                    Role = "Client"
                };
                client.Password = passwordHasher.HashPassword(client, client.Password);
                context.Users.Add(client);

                context.SaveChanges();
            }
        }
    }
}
