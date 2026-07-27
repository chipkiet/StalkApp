using System;
using ChatApp.Shared.DTOs.Users;
using MediatR;

namespace ChatApp.Application.Features.Users.Queries.GetUserProfile;

public record GetUserProfileQuery(Guid TargetUserId, Guid CurrentUserId) : IRequest<UserProfileDto?>;
