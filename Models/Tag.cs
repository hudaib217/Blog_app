using System.ComponentModel.DataAnnotations;

namespace BlogApplication.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Name { get; set; } = string.Empty;

        // Navigation property for many-to-many
        public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }
}
