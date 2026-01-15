// Controllers/HomeController.cs
using BlogApplication.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get all published posts, newest first
            var posts = await _context.BlogPosts
                .Include(p => p.Author) // Load author info (username)
                .Where(p => p.Status == "published")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }
    }
}