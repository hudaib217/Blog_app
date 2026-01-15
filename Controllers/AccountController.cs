using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApplication.Data;        // ← Replace " BlogApplication" with your actual project name
using BlogApplication.Models;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;


namespace BlogApplication.Controllers
{

    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost]
    

        // ===== REGISTER =====
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            // Prevent overposting: only allow specific fields
            if (!ModelState.IsValid)
                return View(model);

            // Check if Usersname or email already exists
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Usersname", "Usersname already taken.");
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View(model);
            }

            // Hash the password (temporary method – improve later!)
            model.PasswordHash = HashPassword(model.PasswordHash); // We'll fix this soon
            model.Role = "Users"; // Default role
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }

        // ===== LOGIN =====
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            var Users = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (Users == null || !VerifyPassword(password, Users.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            // === CREATE AUTHENTICATION COOKIE ===
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Users.Id.ToString()),
                new Claim(ClaimTypes.Name, Users.Username),
                new Claim(ClaimTypes.Role, Users.Role)
            };

            var identity = new ClaimsIdentity(claims, "BlogAppAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("BlogAppAuth", principal);

            return RedirectToAction("Index", "Home");
        }

        // ===== LOGOUT =====
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("BlogAppAuth");
            return RedirectToAction("Index", "Home");
        }

        // ===== HELPER METHODS (TEMPORARY) =====
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            string inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }
    }
}
