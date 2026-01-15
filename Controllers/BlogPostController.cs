// Controllers/BlogPostController.cs
using BlogApplication.Data;
using BlogApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BlogApplication.Controllers
{
    public class BlogPostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogPostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /BlogPost/Details/5 (Public - no auth required)
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.BlogPosts
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            return View(post);
        }

        // POST: /BlogPost/AddComment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Details", new { id = postId });
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int authorId))
            {
                return Unauthorized();
            }

            var comment = new Comment
            {
                Content = content,
                BlogPostId = postId,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Comment added successfully!";
            return RedirectToAction("Details", new { id = postId });
        }

        // POST: /BlogPost/DeleteComment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int commentId, int postId)
        {
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
                return NotFound();

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (comment.AuthorId.ToString() != currentUserId && !User.IsInRole("admin"))
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Comment deleted!";
            return RedirectToAction("Details", new { id = postId });
        }

        // GET: /BlogPost/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(string title, string content, int? categoryId, string? tags)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "Title and content are required.");
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
                return View();
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int authorId))
            {
                return Unauthorized();
            }

            var post = new BlogPost
            {
                Title = title,
                Content = content,
                AuthorId = authorId,
                CategoryId = categoryId,
                Status = "published",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Handle tags
            if (!string.IsNullOrWhiteSpace(tags))
            {
                var tagNames = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().ToLower())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Distinct();

                foreach (var tagName in tagNames)
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        _context.Tags.Add(tag);
                    }
                    post.Tags.Add(tag);
                }
            }

            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your post has been published!";
            return RedirectToAction("Index", "Home");
        }

        // GET: /BlogPost/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.BlogPosts
                .Include(p => p.Author)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (post.AuthorId.ToString() != currentUserId && !User.IsInRole("admin"))
            {
                return Forbid();
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            ViewBag.CurrentTags = string.Join(", ", post.Tags.Select(t => t.Name));
            return View(post);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, string title, string content, int? categoryId, string? tags)
        {
            var post = await _context.BlogPosts
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "Title and content are required.");
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
                ViewBag.CurrentTags = tags;
                return View(post);
            }

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (post.AuthorId.ToString() != currentUserId && !User.IsInRole("admin"))
            {
                return Forbid();
            }

            post.Title = title;
            post.Content = content;
            post.CategoryId = categoryId;
            post.UpdatedAt = DateTime.UtcNow;

            // Update tags
            post.Tags.Clear();
            if (!string.IsNullOrWhiteSpace(tags))
            {
                var tagNames = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().ToLower())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Distinct();

                foreach (var tagName in tagNames)
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        _context.Tags.Add(tag);
                    }
                    post.Tags.Add(tag);
                }
            }

            _context.Update(post);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Post updated successfully!";
            return RedirectToAction("Index", "Home");
        }

        // POST: /BlogPost/Delete/5
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);

            if (post == null)
                return NotFound();

            // Only allow author or admin to delete
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (post.AuthorId.ToString() != currentUserId && !User.IsInRole("admin"))
            {
                return Forbid();
            }

            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Post deleted successfully!";
            return RedirectToAction("Index", "Home");
        }
    }
}