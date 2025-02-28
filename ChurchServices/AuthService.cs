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
    public async Task<(bool IsSuccess, string Token, string Message)> AuthenticateUserAsync(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username); // Find user
        if (user == null)
            return (false, null, "User not found."); // Debug: Check username

        if (!await _userManager.CheckPasswordAsync(user, password))
            return (false, null, "Invalid password."); // Debug: Wrong password?

        if (user.Status != UserStatus.Active)
            return (false, null, "Your account is not approved."); // Debug: User inactive?

        return (true, GenerateJwtToken(user), "Login successful."); // Success: Token generated
    }

    // RegisterUserAsync: Creates user and assigns roles
    public async Task<User?> RegisterUserAsync(RegisterDto model)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(); // Start transaction
        try
        {
            // Create user
            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                EmailConfirmed = true,
                ParishId = model.ParishId,
                FamilyId = model.FamilyId
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"User creation failed: {errors}"); // Debug: Check error details
            }

            // Assign roles
            foreach (var roleId in model.RoleIds)
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role == null)
                    throw new Exception($"Role with ID {roleId} not found."); // Debug: Invalid role ID?

                var isInRole = await _userManager.IsInRoleAsync(user, role.Name);
                if (!isInRole)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, role.Name);
                    if (!roleResult.Succeeded)
                    {
                        var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to assign role {role.Name}: {roleErrors}"); // Debug: Role assignment failed?
                    }

                    // Update UserRole entity
                    var userRole = await _context.UserRoles
                        .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == roleId);
                    if (userRole != null)
                    {
                        userRole.Status = RoleStatus.Pending;
                        userRole.ApprovedBy = null;
                        userRole.ApprovedAt = null;
                    }
                    else
                        throw new Exception("UserRole entity not found after role assignment."); // Debug: Missing UserRole?

                    await _context.SaveChangesAsync(); // Save changes
                }
            }

            await transaction.CommitAsync(); // Commit if all succeeds
            return user;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(); // Rollback on error
            _logger.LogError(ex, "Error during user registration for {Username}", model.Username); // Debug: Check log for exception
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