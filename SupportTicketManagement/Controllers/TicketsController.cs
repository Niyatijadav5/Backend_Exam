using System.Security.Claims;
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
    public class TicketsController : ControllerBase
    {
        private readonly SupportTicketManagementContext context;

        public TicketsController(SupportTicketManagementContext context)
        {
            this.context=context;
        }
        
        //Create Ticket
        [HttpPost("CreateTicket")]
        [Authorize(Roles ="USER,MANAGER")]
        public async Task<IActionResult> createTicket([FromBody]CreateTicketDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (dto.Priority != "LOW" && dto.Priority != "MEDIUM" && dto.Priority != "HIGH")
                {
                    return BadRequest("Invalid priority");   
                }
                var userId=User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var ticket = new Ticket
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    Priority = dto.Priority,
                    Status = "OPEN",
                    CreatedBy = Convert.ToInt32(userId),
                    AssignedTo = null,
                    CreatedAt = DateTime.Now
                };
                await context.Tickets.AddAsync(ticket);
                await context.SaveChangesAsync();

                return StatusCode(201,"Ticket created successfully");
            }
            catch(Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }
        
        //List all tickets
        [Authorize(Roles = "USER,MANAGER,SUPPORT")]
        [HttpGet("TicketsList")]
        public async Task<IActionResult> listTickets(string? status,int page=1,int pageSize=6)
        {
            try
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (role == null || id == null)
                {
                    return Unauthorized();
                }

                int userId = Convert.ToInt32(id);

                var result = new List<object>();
                int totalRecords = 0;

                if(role == "MANAGER")
                {
                    var query = context.Tickets.AsQueryable();

                    if(!string.IsNullOrEmpty(status))
                        query = query.Where(t => t.Status == status);
                    totalRecords = await query.CountAsync();

                    result = await query
                        .OrderByDescending(t => t.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(t => new
                        {
                            t.Id,
                            t.Title,
                            t.Description,
                            t.Status,
                            t.Priority,
                            created_by = t.CreatedBy,
                            assigned_to = t.AssignedTo,
                            t.CreatedAt
                        }).ToListAsync<object>();
                }
                else if(role == "SUPPORT")
                {
                    var query = context.Tickets
                        .Where(t => t.AssignedTo == userId);

                    if(!string.IsNullOrEmpty(status))
                        query = query.Where(t => t.Status == status);
                    totalRecords = await query.CountAsync();

                    result = await query
                        .OrderByDescending(t => t.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(t => new
                        {
                            t.Id,
                            t.Title,
                            t.Description,
                            t.Status,
                            t.Priority,
                            created_by = t.CreatedBy,
                            assigned_to = t.AssignedTo,
                            t.CreatedAt
                        }).ToListAsync<object>();
                }
                else
                {
                    var query = context.Tickets
                        .Where(t => t.CreatedBy == userId);

                    if(!string.IsNullOrEmpty(status))
                        query = query.Where(t => t.Status == status);
                    totalRecords = await query.CountAsync();

                    result = await query
                        .OrderByDescending(t => t.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(t => new
                        {
                            t.Id,
                            t.Title,
                            t.Description,
                            t.Status,
                            t.Priority,
                            created_by = t.CreatedBy,
                            assigned_to = t.AssignedTo,
                            t.CreatedAt
                        }).ToListAsync<object>();
                }

                return Ok(new
                { totalRecords,page,pageSize,data = result });
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        //Assign Ticket
        [HttpPatch("{id}/Assign")]
        [Authorize(Roles ="MANAGER,SUPPORT")]
        public async Task<IActionResult> assignTicket(int id,AssignDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ticket = await context.Tickets.FirstOrDefaultAsync(x => x.Id == id);

                if (ticket == null)
                {
                    return NotFound("Ticket not found");
                }
                var user = await context.Users
                    .Include(x => x.Role)
                    .FirstOrDefaultAsync(x => x.Id == dto.UserId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                if (user.Role.Name == "USER")
                {
                    return BadRequest("Cannot assign ticket to USER role");   
                }
                ticket.AssignedTo = dto.UserId;
                
                await context.SaveChangesAsync();

                return Ok(new
                {
                    ticket.Id,
                    ticket.Title,
                    ticket.Status,
                    assigned_to = ticket.AssignedTo
                });
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        //Change Status
        [HttpPatch("{id}/Status")]
        [Authorize(Roles ="MANAGER,SUPPORT")]
        public async Task<IActionResult> changeStatus(int id,StatusDto data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ticket = await context.Tickets.FirstOrDefaultAsync(x => x.Id == id);

                if (ticket == null)
                {
                    return NotFound("Ticket not found");
                }

                var oldStatus = ticket.Status;
                var newStatus = data.Status;

                bool valid = false;

                if (oldStatus == "OPEN" && newStatus == "IN_PROGRESS")
                {
                    valid = true;
                }
                else if(oldStatus == "IN_PROGRESS" && newStatus == "RESOLVED")
                {
                    valid = true;
                }
                else if(oldStatus == "RESOLVED" && newStatus == "CLOSED")
                {
                    valid = true;
                }
                if (valid == false)
                {
                    return BadRequest("Invalid status");
                }
                ticket.Status = newStatus;
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var log = new TicketStatusLog
                {
                    TicketId = ticket.Id,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    ChangedBy = Convert.ToInt32(userId),
                    ChangedAt = DateTime.Now
                };
                await context.TicketStatusLogs.AddAsync(log);
                await context.SaveChangesAsync();

                return Ok(new
                {
                    ticket.Id,
                    old_status = oldStatus,
                    new_status = newStatus
                });
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        //Delete Ticket
        [HttpDelete("{id}/DeleteTicket")]
        [Authorize(Roles ="MANAGER")]
        public async Task<IActionResult> deleteTicket(int id)
        {
            try
            {
                var ticket = await context.Tickets.FirstOrDefaultAsync(x => x.Id == id);

                if (ticket == null)
                {
                    return NotFound("Ticket not found");
                }
                context.Tickets.Remove(ticket);
                await context.SaveChangesAsync();

                return StatusCode(201,"Ticket deleted successfully");
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
