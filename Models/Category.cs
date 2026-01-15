using System.ComponentModel.DataAnnotations;

namespace BlogApplication.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Navigation property
        public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }
}
