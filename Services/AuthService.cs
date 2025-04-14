using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AvstickareApi.Models;
using Microsoft.IdentityModel.Tokens;


namespace AvstickareApi.Services
{
    public class AuthService
    {
        //läser in konfigurationer från program.cs
       private readonly IConfiguration _configuration; 

       public AuthService(IConfiguration configuration)
       {
        _configuration = configuration;
       }

        //skapa och returnera token
       public string CreateToken(AppUser user)
       {
            //hämta konfigvärden
            var jwtKey = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT-key saknas");
            var jwtIssuer = _configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("Issuer saknas");
            var jwtAudience = _configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("Audience saknas");

            //konvertera nyckel
            var keyBytes =Encoding.UTF8.GetBytes(jwtKey);
            //skapa säkerhetsnyckel som signerar token
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);


            //lista med claims som läggs i token
            var claims = new List<Claim>
            {
                //identifiera användare
                new Claim(ClaimTypes.NameIdentifier, user.AppUserId),
                //lägg till användarnamn och e-post
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)  
            };

            //definiera tokens innehåll (claims, utgångstid, utfärdat av, signering)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                //giltigt 1 timme
                Expires = DateTime.Now.AddHours(1),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = signingCredentials
            };

            //inbyggd tokenhanterare
            var tokenHandler = new JwtSecurityTokenHandler();
            //generera token baserat på tokenDescriptor
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            
            //returnera token
            return tokenHandler.WriteToken(securityToken); 
       }
    }
}