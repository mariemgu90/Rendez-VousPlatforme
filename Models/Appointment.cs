using System.ComponentModel.DataAnnotations;

namespace Rendez_Vousdotnet.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Status { get; set; } = "En cours"; // "En cours", "Accepté", "Refusé"

        [Required]
        public string Description { get; set; }

        [Required]
        public int ClientId { get; set; }
        public User Client { get; set; }
        [Required]
        public int ProfessionalId { get; set; }
        public User Professional { get; set; }
    }
}
