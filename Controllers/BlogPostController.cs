// Controllers/BlogPostController.cs
using BlogApplication.Data;
using BlogApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApplication.Controllers
{
    [Authorize] // 🔒 Only logged-in users can access ANY action in this controller
    public class BlogPostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogPostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /BlogPost/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string title, string content)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "Title and content are required.");
                return View();
            }

            // Get current user ID from claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int authorId))
            {
                return Unauthorized(); // Should not happen if [Authorize] works
            }

            var post = new BlogPost
            {
                Title = title,
                Content = content,
                AuthorId = authorId,
                Status = "published",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your post has been published!";
            return RedirectToAction("Index", "Home");
        }

        // GET: /BlogPost/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.BlogPosts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            // Get current user ID
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Only allow author or admin to edit
            if (post.AuthorId.ToString() != currentUserId && !User.IsInRole("admin"))
            {
                return Forbid(); // Shows 403 Forbidden
            }

            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string title, string content)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            
            if (post == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "Title and content are required.");
                return View(post);
            }

            // Re-check ownership (security!)
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (post.AuthorId.ToString() != currentUserId && !User.IsInRole("admin"))
            {
                return Forbid();
            }

            // Update post
            post.Title = title;
            post.Content = content;
            post.UpdatedAt = DateTime.UtcNow;

            _context.Update(post);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Post updated successfully!";
            return RedirectToAction("Index", "Home");
        }


    }
}