// Controllers/HomeController.cs
using BlogApplication.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 5;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, int? categoryId = null, string? tag = null)
        {
            // Base query for published posts
            var query = _context.BlogPosts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .Where(p => p.Status == "published");

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
                ViewBag.Search = search;
            }

            // Apply category filter
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
                ViewBag.CategoryId = categoryId;
                var category = await _context.Categories.FindAsync(categoryId);
                ViewBag.CategoryName = category?.Name;
            }

            // Apply tag filter
            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(p => p.Tags.Any(t => t.Name == tag));
                ViewBag.Tag = tag;
            }

            // Get total count for pagination
            var totalPosts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalPosts / (double)PageSize);

            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            // Get posts for current page
            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Pass pagination info to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalPosts = totalPosts;

            // Load all categories and tags for sidebar
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.AllTags = await _context.Tags.ToListAsync();

            return View(posts);
        }
    }
}