using System.ComponentModel.DataAnnotations;

namespace Rendez_Vousdotnet.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; } // "Admin", "Professional", "Client"

        public string? Speciality { get; set; } // Only for professionals
    }
}
