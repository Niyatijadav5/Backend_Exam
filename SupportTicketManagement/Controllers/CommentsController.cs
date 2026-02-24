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
    public class CommentsController : ControllerBase
    {
        private readonly SupportTicketManagementContext context;

        public CommentsController(SupportTicketManagementContext context)
        {
            this.context = context;
        }

        // Add Comment
        [HttpPost("Tickets/{id}/Comments")]
        [Authorize]
        public async Task<IActionResult> addComment(int id,CommentDto data)
        {
            try
            {
                if(!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ticket = await context.Tickets.FirstOrDefaultAsync(x => x.Id == id);
                if(ticket == null)
                    return NotFound("Ticket not found");

                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if(role == "SUPPORT" && ticket.AssignedTo != userId)
                    return Forbid();

                if(role == "USER" && ticket.CreatedBy != userId)
                    return Forbid();

                var comment = new TicketComment();
                comment.TicketId = id;
                comment.UserId = userId;
                comment.Comment = data.Comment;
                comment.CreatedAt = DateTime.Now;

                await context.TicketComments.AddAsync(comment);
                await context.SaveChangesAsync();

                return StatusCode(201,"Comment added");
            }
            catch(Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        // Get all comments
        [HttpGet("Tickets/{id}/Comments")]
        [Authorize]
        public async Task<IActionResult> getComments(int id)
        {
            try
            {
                var ticket = await context.Tickets.FirstOrDefaultAsync(x => x.Id == id);
                if(ticket == null)
                    return NotFound("Ticket not found");

                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if(role == "SUPPORT" && ticket.AssignedTo != userId)
                    return Forbid();

                if(role == "USER" && ticket.CreatedBy != userId)
                    return Forbid();

                var list = await context.TicketComments
                    .Where(x => x.TicketId == id)
                    .Select(x => new
                    {
                        x.Id,
                        x.Comment,
                        x.UserId,
                        x.CreatedAt
                    }).ToListAsync();

                return Ok(list);
            }
            catch(Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        // Update Comment
        [HttpPatch("Comments/{id}")]
        [Authorize]
        public async Task<IActionResult> updateComment(int id,CommentDto data)
        {
            try
            {
                var comment = await context.TicketComments.FirstOrDefaultAsync(x => x.Id == id);
                if(comment == null)
                    return NotFound("Comment not found");

                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if(role != "MANAGER" && comment.UserId != userId)
                    return Forbid();

                comment.Comment = data.Comment;
                await context.SaveChangesAsync();

                return Ok("Comment updated");
            }
            catch(Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        // Delete Comment
        [HttpDelete("Comments/{id}")]
        [Authorize]
        public async Task<IActionResult> deleteComment(int id)
        {
            try
            {
                var comment = await context.TicketComments.FirstOrDefaultAsync(x => x.Id == id);
                if(comment == null)
                    return NotFound("Comment not found");

                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if(role != "MANAGER" && comment.UserId != userId)
                    return Forbid();

                context.TicketComments.Remove(comment);
                await context.SaveChangesAsync();

                return StatusCode(201,"Comment deleted successfully");
            }
            catch(Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }
    }
}
