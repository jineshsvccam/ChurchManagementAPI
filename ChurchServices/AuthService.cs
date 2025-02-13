using ChurchData;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
  //  private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly RoleManager<Role> _roleManager;

    public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<Role> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<(bool IsSuccess, string Token, string Message)> AuthenticateUserAsync(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user == null)
            return (false, null, "User not found.");

        if (!await _userManager.CheckPasswordAsync(user, password))
            return (false, null, "Invalid password.");

        if (user.Status != UserStatus.Active)  // Assuming 'Active' is your enum value
            return (false, null, "Your account is not approved.");

        return (true, GenerateJwtToken(user), "Login successful.");
    }


    public async Task<User?> RegisterUserAsync(string username, string email, string password,int parishId,int familyId, List<int> roleIds)
    {
        var user = new User
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true,
            ParishId = parishId,
            FamilyId = familyId
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            // Log errors
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"User creation failed: {errors}");
        }

        // Assign roles
        foreach (var roleId in roleIds)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role != null)
            {
                await _userManager.AddToRoleAsync(user, role.Name);
            }
        }

        return user;
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var userRoles = _userManager.GetRolesAsync(user).Result;
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
