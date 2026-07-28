using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Features.Messages.Commands.DeleteMessage;
using ChatApp.Application.Features.Messages.Commands.EditMessage;
using ChatApp.Application.Features.Messages.Queries.GetMessages;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ChatApp.WebApi.Tests;

// ──────────────────────────────────────────────────────────────────────────────
// Fake authentication handler: inject userId via X-Test-UserId header.
// Send X-Test-NoAuth header to simulate unauthenticated requests.
// ──────────────────────────────────────────────────────────────────────────────
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Context.Request.Headers.ContainsKey("X-Test-NoAuth"))
            return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));

        var userId = Context.Request.Headers["X-Test-UserId"].ToString();
        if (string.IsNullOrEmpty(userId))
            userId = Guid.NewGuid().ToString();

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// Authorization tests for MessagesController and ConversationsController
// ──────────────────────────────────────────────────────────────────────────────
public class MessagesAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MessagesAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // Helper: create a test HTTP client with mocked MediatR and optional participant repo.
    private HttpClient CreateClient(
        Mock<IMediator> mediatorMock,
        Mock<IGenericRepository<Participant>>? participantRepoMock = null,
        Mock<IGenericRepository<User>>? userRepoMock = null)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                    options.DefaultScheme = "TestScheme";
                });

                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "TestScheme", options => { });

                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("TestScheme")
                        .RequireAuthenticatedUser()
                        .Build();
                });

                services.AddSingleton(mediatorMock.Object);

                if (participantRepoMock != null)
                    services.AddSingleton(participantRepoMock.Object);

                if (userRepoMock != null)
                    services.AddSingleton(userRepoMock.Object);
            });
        }).CreateClient();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // GET /api/messages/{conversationId}
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMessages_WithoutLogin_Returns401()
    {
        var mediatorMock = new Mock<IMediator>();
        var client = CreateClient(mediatorMock);
        client.DefaultRequestHeaders.Add("X-Test-NoAuth", "true");

        var response = await client.GetAsync($"/api/messages/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMessages_NotParticipant_Returns403()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetMessagesQuery>(), default))
            .ThrowsAsync(new UnauthorizedAccessException("Bạn không có quyền truy cập vào cuộc trò chuyện này."));

        var client = CreateClient(mediatorMock);

        var response = await client.GetAsync($"/api/messages/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // GET /api/conversations/{id}/participants – IDOR check
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetParticipants_NotParticipant_Returns403()
    {
        var mediatorMock = new Mock<IMediator>();
        var callerUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        // The caller is NOT a participant of this conversation → empty result
        var participantRepoMock = new Mock<IGenericRepository<Participant>>();
        participantRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Participant, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<Participant>());

        var userRepoMock = new Mock<IGenericRepository<User>>();

        var client = CreateClient(mediatorMock, participantRepoMock, userRepoMock);
        client.DefaultRequestHeaders.Add("X-Test-UserId", callerUserId.ToString());

        var response = await client.GetAsync($"/api/conversations/{conversationId}/participants");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // PUT /api/messages/{messageId} – EditMessage: ONLY author, Admin blocked
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EditMessage_AsAdminButNotAuthor_Returns403()
    {
        var mediatorMock = new Mock<IMediator>();

        // The handler throws UnauthorizedAccessException because caller ≠ author,
        // even if they happen to be an Admin elsewhere.
        mediatorMock
            .Setup(m => m.Send(It.IsAny<EditMessageCommand>(), default))
            .ThrowsAsync(new UnauthorizedAccessException("Only the sender can edit this message."));

        var client = CreateClient(mediatorMock);

        var content = new System.Net.Http.StringContent(
            System.Text.Json.JsonSerializer.Serialize(new { NewContent = "hacked edit" }),
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync($"/api/messages/{Guid.NewGuid()}", content);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // DELETE /api/messages/{messageId} – DeleteMessage
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteMessage_AsNonParticipantAndNonAuthor_Returns403()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteMessageCommand>(), default))
            .ThrowsAsync(new UnauthorizedAccessException("Only the message author or a conversation admin can delete this message."));

        var client = CreateClient(mediatorMock);

        var response = await client.DeleteAsync($"/api/messages/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMessage_AsAdminNotAuthor_Returns200()
    {
        var mediatorMock = new Mock<IMediator>();
        var messageId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();

        // Admin is allowed → handler returns successfully
        mediatorMock
            .Setup(m => m.Send(It.Is<DeleteMessageCommand>(c => c.MessageId == messageId && c.UserId == adminUserId), default))
            .ReturnsAsync(new ChatApp.Application.DTOs.Messages.MessageDto(
                messageId, Guid.NewGuid(), Guid.NewGuid(), // SenderId is a different user
                ChatApp.Domain.Enums.MessageType.Text, null,
                DateTime.UtcNow, null, null, false, true, DateTime.UtcNow));

        var client = CreateClient(mediatorMock);
        client.DefaultRequestHeaders.Add("X-Test-UserId", adminUserId.ToString());

        var response = await client.DeleteAsync($"/api/messages/{messageId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMessage_AsAuthor_Returns200()
    {
        var mediatorMock = new Mock<IMediator>();
        var messageId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        mediatorMock
            .Setup(m => m.Send(It.Is<DeleteMessageCommand>(c => c.MessageId == messageId && c.UserId == userId), default))
            .ReturnsAsync(new ChatApp.Application.DTOs.Messages.MessageDto(
                messageId, Guid.NewGuid(), userId, ChatApp.Domain.Enums.MessageType.Text, null,
                DateTime.UtcNow, null, null, false, true, DateTime.UtcNow));

        var client = CreateClient(mediatorMock);
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());

        var response = await client.DeleteAsync($"/api/messages/{messageId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
