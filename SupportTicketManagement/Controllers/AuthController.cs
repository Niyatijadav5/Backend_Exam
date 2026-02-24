using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SupportTicketManagement.DTO;
using SupportTicketManagement.Models;

namespace SupportTicketManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SupportTicketManagementContext context;
        private readonly IConfiguration configuration;

        public AuthController(SupportTicketManagementContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }
        //Token Generation
        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };
            var key = new SymmetricSecurityKey
            (
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"])
            );
            
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var token = new JwtSecurityToken
            (
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(configuration["Jwt:TokenExpiryMinutes"])),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        //Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = await context.Users
                    .Include(x => x.Role)
                    .FirstOrDefaultAsync(x => x.Email == dto.Email);
                if (user == null)
                {
                    return Unauthorized("Invalid email or password.");
                }
                
                bool PasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);
                
                if (!PasswordValid)
                {
                    return Unauthorized("Invalid email or password.");
                }
                var token = GenerateToken(user);
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                return Ok(new
                {
                    token = token,
                    role = user.Role.Name,
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
