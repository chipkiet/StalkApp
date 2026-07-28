using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ChatApp.Application.Features.Conversations.Commands.AddMembers;
using ChatApp.Application.Features.Conversations.Commands.CreateConversation;
using ChatApp.Application.Features.Conversations.Commands.DeleteConversation;
using ChatApp.Application.Features.Conversations.Commands.DisbandGroup;
using ChatApp.Application.Features.Conversations.Commands.RemoveMember;
using ChatApp.Application.Features.Conversations.Commands.ToggleMute;
using ChatApp.Application.Features.Conversations.Commands.TogglePinConversation;
using ChatApp.Application.Features.Conversations.Commands.UpdateGroup;
using ChatApp.Application.Features.Conversations.Queries.GetInbox;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using ChatApp.Shared.DTOs.Conversations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ChatApp.WebApi.Hubs;

namespace ChatApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<ChatHub> _hubContext;

    public ConversationsController(IMediator mediator, IHubContext<ChatHub> hubContext)
    {
        _mediator = mediator;
        _hubContext = hubContext;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("nameid")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [HttpGet("inbox")]
    public async Task<IActionResult> GetInbox()
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new GetInboxQuery(userId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationCommand command)
    {
        var userId = GetCurrentUserId();
        var cmdWithCreator = command with { CreatorId = userId };
        var result = await _mediator.Send(cmdWithCreator);
        return Ok(result);
    }

    [HttpDelete("{conversationId}")]
    public async Task<IActionResult> DeleteConversation(Guid conversationId)
    {
        var userId = GetCurrentUserId();
        var command = new DeleteConversationCommand(conversationId, userId);
        try
        {
            await _mediator.Send(command);
            return Ok();
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
    }

    [HttpGet("{id}/participants")]
    public async Task<IActionResult> GetParticipants(
        Guid id,
        [FromServices] IGenericRepository<Participant> participantRepo,
        [FromServices] IGenericRepository<User> userRepo)
    {
        var userId = GetCurrentUserId();
        var myParticipant = (await participantRepo.FindAsync(p => p.ConversationId == id && p.UserId == userId))
            .FirstOrDefault();

        if (myParticipant == null)
            return StatusCode(403, new { message = "Bạn không có quyền truy cập thông tin nhóm này." });

        var participants = await participantRepo.FindAsync(p => p.ConversationId == id);
        var result = new System.Collections.Generic.List<object>();
        foreach (var p in participants)
        {
            var user = await userRepo.GetByIdAsync(p.UserId);
            if (user != null)
            {
                result.Add(new
                {
                    userId = user.Id,
                    displayName = user.DisplayName,
                    phoneNumber = user.PhoneNumber,
                    avatarUrl = user.AvatarUrl,
                    bio = user.Bio,
                    role = (int)p.Role   // 0 = Admin, 1 = Member
                });
            }
        }
        return Ok(result);
    }

    // ─── PUT /api/conversations/{id}/group – Sửa tên/ảnh nhóm (mọi thành viên) ──
    [HttpPut("{id}/group")]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateGroupRequest body)
    {
        var userId = GetCurrentUserId();
        try
        {
            var command = new UpdateGroupCommand(id, userId, body.Title, body.AvatarUrl);
            await _mediator.Send(command);

            // Broadcast cho các thành viên trong nhóm biết thông tin nhóm đã cập nhật
            await _hubContext.Clients.Group(id.ToString())
                .SendAsync("GroupUpdated", id, body.Title, body.AvatarUrl);

            return Ok();
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // ─── POST /api/conversations/{id}/members – Thêm thành viên (mọi thành viên) ──
    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMembers(Guid id, [FromBody] AddMembersRequest body)
    {
        var userId = GetCurrentUserId();
        try
        {
            var command = new AddMembersCommand(id, userId, body.UserIds);
            await _mediator.Send(command);

            // Broadcast danh sách user vừa được thêm để tất cả cập nhật
            await _hubContext.Clients.Group(id.ToString())
                .SendAsync("MembersAdded", id, body.UserIds);

            return Ok();
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // ─── DELETE /api/conversations/{id}/members/{targetUserId} – Xóa thành viên (Admin only) ──
    [HttpDelete("{id}/members/{targetUserId}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid targetUserId)
    {
        var userId = GetCurrentUserId();
        try
        {
            var command = new RemoveMemberCommand(id, userId, targetUserId);
            await _mediator.Send(command);

            // Thông báo cho nhóm: member bị xóa
            await _hubContext.Clients.Group(id.ToString())
                .SendAsync("MemberRemoved", id, targetUserId);

            return Ok();
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // ─── DELETE /api/conversations/{id}/disband – Giải tán nhóm (Admin only) ──
    [HttpDelete("{id}/disband")]
    public async Task<IActionResult> DisbandGroup(Guid id)
    {
        var userId = GetCurrentUserId();
        try
        {
            var command = new DisbandGroupCommand(id, userId);
            await _mediator.Send(command);

            // Broadcast cho tất cả thành viên biết nhóm đã bị giải tán
            await _hubContext.Clients.Group(id.ToString())
                .SendAsync("GroupDisbanded", id);

            return Ok();
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{conversationId}/pin")]
    public async Task<IActionResult> TogglePinConversation(Guid conversationId, [FromBody] TogglePinConversationRequest body)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new TogglePinConversationCommand(conversationId, userId, body.IsPinned);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id}/mute")]
    public async Task<IActionResult> ToggleMute(Guid id, [FromQuery] bool isMuted)
    {
        var userId = GetCurrentUserId();
        try
        {
            await _mediator.Send(new ToggleMuteCommand(id, userId, isMuted));
            return Ok();
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}

public record TogglePinConversationRequest(bool IsPinned);
