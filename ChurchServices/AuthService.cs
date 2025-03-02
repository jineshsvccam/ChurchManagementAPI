using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<AuthService> _logger;
    private readonly ApplicationDbContext _context;

    // Constructor: Sets up dependencies
    public AuthService(UserManager<User> userManager, SignInManager<User> signInManager,
        RoleManager<Role> roleManager, IConfiguration configuration,
        ILogger<AuthService> logger, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    // AuthenticateUserAsync: Checks login and returns token
    public async Task<AuthResultDto> AuthenticateUserAsync(string username, string password)
    {
        // var user = await _userManager.FindByNameAsync(username);
        var user = await _context.Users
             .Include(u => u.Family)
             .Include(u => u.Parish)
             .FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null)
            return new AuthResultDto
            {
                IsSuccess = false,
                Message = "User not found."
            };

        if (!await _userManager.CheckPasswordAsync(user, password))
            return new AuthResultDto
            {
                IsSuccess = false,
                Message = "Invalid password."
            };

        if (user.Status != UserStatus.Active)
            return new AuthResultDto
            {
                IsSuccess = false,
                Message = "Your account is not approved."
            };

        // Get roles for the user
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResultDto
        {
            IsSuccess = true,
            Token = GenerateJwtToken(user),
            Message = "Login successful.",
            FullName = user.FullName,
            ParishId = user.ParishId,
            ParishName = user.Parish?.ParishName,
            FamilyId = user.FamilyId,
            FamilyName = string.Concat(user.Family?.HeadName, " ", user.Family?.FamilyName),
            Roles = roles.ToList()
        };
    }

    // RegisterUserAsync: Creates user and assigns roles
    public async Task<User?> RegisterUserAsync(RegisterDto model)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                EmailConfirmed = true,
                ParishId = model.ParishId,
                FamilyId = model.FamilyId,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Status = UserStatus.Pending
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"User creation failed: {errors}");
            }

            foreach (var roleId in model.RoleIds)
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role == null)
                    throw new Exception($"Role with ID {roleId} not found.");

                var isInRole = await _userManager.IsInRoleAsync(user, role.Name);
                if (!isInRole)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId,
                        Status = RoleStatus.Pending,
                        ApprovedBy = null,
                        ApprovedAt = null
                    };
                    _context.UserRoles.Add(userRole);
                }
            }

            await _context.SaveChangesAsync(); // Save all UserRoles at once
            await transaction.CommitAsync();
            return user;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during user registration for {Username}", model.Username);
            throw;
        }
    }
    // GenerateJwtToken: Creates JWT for user
    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var userRoles = _userManager.GetRolesAsync(user).Result; // Get user roles
        foreach (var role in userRoles)
            claims.Add(new Claim(ClaimTypes.Role, role)); // Add roles to claims

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])); // JWT key
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1), // Expires in 1 hour
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token); // Return token string
    }
}