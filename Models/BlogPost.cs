using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogApplication.Models
{
    public class BlogPost
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [ForeignKey("Author")]
        public int AuthorId { get; set; }
        public User Author { get; set; } = null!;

        // Category (optional)
        [ForeignKey("Category")]
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        // Tags (many-to-many)
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();

        // Comments
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public string Status { get; set; } = "published";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
