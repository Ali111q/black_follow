using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using black_follow.Data;
using black_follow.Data.Dtos.UserDto;
using black_follow.Entity;
using black_follow.Service;
using Microsoft.EntityFrameworkCore;

namespace black_follow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly IUserService _userService;

        public UsersController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration,
            DataContext context, IUserService userService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _userService = userService;
        }

        // POST: api/Users/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new AppUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return Ok(new { message = "User registered successfully!" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

        // POST: api/Users/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginForm form)
        {
            // if (!ModelState.IsValid)
            //     return BadRequest(ModelState);
            //
            // var user = await _userManager.FindByEmailAsync(model.Email);
            // if (user == null)
            //     return Unauthorized(new { message = "Invalid email or password" });
            //
            // var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            //
            // if (!result.Succeeded)
            //     return Unauthorized(new { message = "Invalid email or password" });
            //
            // // Create a session token
            // var sessionToken = Guid.NewGuid().ToString();
            //
            // // Store session information
            // var userSession = new UserSession
            // {
            //     UserId = user.Id,
            //     Token = sessionToken,
            //     LoginTime = DateTime.UtcNow,
            //     Device = "Browser/Device", // You can capture the actual device info
            //     IPAddress = HttpContext.Connection.RemoteIpAddress.ToString(),
            //     IsActive = true
            // };
            //
            // _context.UserSessions.Add(userSession);
            // await _context.SaveChangesAsync();
            //
            // // Create JWT token including the sessionToken as a claim
            // var token = GenerateJwtToken(user, sessionToken);
            // return Ok(new { token });

            var login = await _userService.LoginAsync( form);
            if (login.state == State.NOT_FOUND)
                return NotFound(new { message = "Invalid email or password" });
            if (login.state == State.BAD_REQUEST) 
                return BadRequest(new { message = "Invalid email or password" });
            return Ok(login.user);
            
        }


        // GET: api/Users/Me (Get current logged-in user)
        [HttpGet("Me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            var user = await _context.Users.FirstOrDefaultAsync(U=>U.UserName==userId);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email
            });
        }
        [HttpPost("KickOut/{sessionId}")]
        [Authorize]  // Ensure that only authorized users can kick someone out
        public async Task<IActionResult> KickOut(int sessionId)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null || !session.IsActive)
                return NotFound("Session not found or already inactive");

            session.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "User has been signed out from the session." });
        }
        [HttpGet("ActiveSessions")]
        [Authorize]
        public async Task<IActionResult> GetActiveSessions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not found" });
            }

            var sessions = await _context.UserSessions
                .Where(s => s.User.UserName == userId && s.IsActive)  // Only get active sessions for this user
                .Select(s => new
                {
                    s.Id,
                    s.LoginTime,
                    s.Device,
                    s.IPAddress
                })
                .ToListAsync();

            return Ok(sessions);
        }



        // Helper method to generate JWT token
        private string GenerateJwtToken(AppUser user, string sessionToken)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("SessionToken", sessionToken) // Add session token to claims
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // Models for the API requests
        public class RegisterModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class LoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
