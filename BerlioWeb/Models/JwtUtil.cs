using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BerlioWeb.Models
{
    public class JwtUtil
    {
        public static string GenerateJwtToken(User user, JwtConfig jwtConfig)
        {
            Claim[] claims = [new("userLogin", user.Login)];
        //    {
        //    new Claim(ClaimTypes.NameIdentifier, user.Login.ToString()),
        //    new Claim(ClaimTypes.Name, user.Login.ToString()),
        //    //new Claim(ClaimTypes.Role, "User") // Добавьте роли, если нужно
        //};

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtConfig.Issuer,
                audience: jwtConfig.Audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
