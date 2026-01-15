using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogApplication.Models
{
    public class Session
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        public string SessionToken { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }



    }
}
