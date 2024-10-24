using AutoMapper;
using black_follow.Data;
using black_follow.Data.Dtos.UserDto;
using black_follow.Entity;
using black_follow.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace black_follow.Repository;

public class UserRepository : IUserRepository
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly DataContext _context;

    public UserRepository(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        IConfiguration configuration, DataContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }

    public async Task<(UserLoginDto? user, State state)> LoginAsync(UserLoginForm form)
    {
        var user = await _userManager.FindByEmailAsync(form.Email);
        if (user == null)
            return (null, State.NOT_FOUND);

        var result = await _signInManager.CheckPasswordSignInAsync(user, form.Password, false);

        if (!result.Succeeded)
            return (null, State.BAD_REQUEST);

        // Generate JWT token
        var token = GenerateJwtToken(user);

        // Create UserLoginDto with JWT token
        var userLoginDto = new UserLoginDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Token = token
        };

        // Optionally, store session information
        var userSession = new UserSession
        {
            UserId = user.Id,
            Token = token,
            LoginTime = DateTime.UtcNow,
            Device = "Browser/Device", // You can capture the actual device info
            IPAddress = "", // Optionally capture the user's IP address
            IsActive = true
        };

        // Save session (if required)
        _context.UserSessions.Add(userSession);
        await _context.SaveChangesAsync();

        return (userLoginDto, State.Ok);
    }

    // Helper method to generate JWT
    private string GenerateJwtToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Issuer"],
            claims,
            expires: DateTime.Now.AddMinutes(30), // Token expiration time
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
