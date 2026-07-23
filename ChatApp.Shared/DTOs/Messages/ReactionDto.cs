using System;

namespace ChatApp.Application.DTOs.Messages;

public record ReactionDto(Guid UserId, string Emotion);
