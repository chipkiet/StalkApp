using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.Common.Exceptions;
using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Application.Interfaces.Security;
using ChatApp.Domain.Entities;
using ChatApp.Shared.DTOs.Auth;
using ChatApp.Shared.Enums;
using MediatR;

namespace ChatApp.Application.Features.Auth.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterResponse>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(
        IGenericRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Auto generate Username from DisplayName
        var normalizedDisplayName = NormalizeString(request.DisplayName);
        var randomSuffix = new Random().Next(1000, 9999);
        var username = $"{normalizedDisplayName}{randomSuffix}";

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = passwordHash,
            DisplayName = request.DisplayName,
            Username = username,
            Status = UserStatus.PendingVerification,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            // Catch PostgreSQL unique constraint violation (error code 23505) without referencing EF Core
            if (ex.InnerException != null && ex.InnerException.Message.Contains("23505"))
            {
                throw new ConflictException("Email or Username is already taken.");
            }
            throw; // Re-throw if it's a different DB error
        }

        return new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        };
    }

    private string NormalizeString(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "user";

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        var cleanString = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        cleanString = Regex.Replace(cleanString, @"[^a-zA-Z0-9]", ""); // Remove non-alphanumeric
        return cleanString.ToLowerInvariant();
    }
}
