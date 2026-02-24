using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportTicketManagement.DTO;
using SupportTicketManagement.Models;

namespace SupportTicketManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SupportTicketManagementContext context;

        public UsersController(SupportTicketManagementContext context)
        {
            this.context=context;
        }
        
        //Create User
        [Authorize(Roles = "MANAGER")]
        [HttpPost("CreateUser")]
        public async Task<IActionResult> Createuser([FromBody] CreateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var emailCheck=await context.Users.AnyAsync(x=>x.Email==dto.Email);

                if (emailCheck)
                {
                    return BadRequest("Email already exists");   
                }
                var role=await context.Roles.FirstOrDefaultAsync(x=>x.Name==dto.Role);

                if (role == null)
                {
                    return BadRequest("Invalid role");   
                }
                var hash=BCrypt.Net.BCrypt.HashPassword(dto.Password);

                var user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Password = hash,
                    RoleId = role.Id,
                    CreatedAt = DateTime.Now
                };

                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                return StatusCode(201,"User created successfully");
            }
            catch(Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }
        
        //List of Users
        [Authorize(Roles = "MANAGER")]
        [HttpGet("UserList")]
        public async Task<IActionResult> UserList()
        {
            try
            {
                var list=await context.Users
                    .Include(x=>x.Role)
                    .Select(x=>new
                    {
                        x.Id,
                        x.Name,
                        x.Email,
                        Role=x.Role.Name,
                        x.CreatedAt
                    })
                    .ToListAsync();

                return Ok(list);
            }
            catch(Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }
    }
}
