using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PlusAppointment.Models.Interfaces;

namespace WebApplication1.Utils.Jwt;

public class JwtUtility
{
    public static string GenerateJwtToken(IUserIdentity user, IConfiguration configuration)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var key = configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Jwt:Key is not configured properly.", nameof(configuration));
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var keyBytes = Encoding.ASCII.GetBytes(key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? string.Empty), // Ensure Username is not null
                new Claim(ClaimTypes.Role, user.Role) // Ensure Role is not null
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}