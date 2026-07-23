using ChatApp.Application.Interfaces.Repositories;
using ChatApp.Domain.Entities;
using MediatR;

namespace ChatApp.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResult>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(
        IGenericRepository<User> userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateProfileResult> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        // 1. Tìm user
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
            return new UpdateProfileResult(false, "Người dùng không tồn tại.");

        // 2. Kiểm tra username mới có bị trùng không (nếu có đổi)
        if (!string.IsNullOrWhiteSpace(request.Username) &&
            request.Username != user.Username)
        {
            var usersWithSameName = await _userRepository.FindAsync(
                u => u.Username == request.Username && u.Id != request.UserId);

            if (usersWithSameName.Any())
                return new UpdateProfileResult(false, $"Username '{request.Username}' đã được sử dụng. Vui lòng chọn username khác.");

            user.Username = request.Username;
        }

        // 3. Cập nhật các trường
        if (!string.IsNullOrWhiteSpace(request.DisplayName))
            user.DisplayName = request.DisplayName;

        if (request.Bio is not null)
            user.Bio = request.Bio;

        if (request.AvatarUrl is not null)
            user.AvatarUrl = request.AvatarUrl;

        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateProfileResult(
            Success: true,
            Message: "Cập nhật thông tin thành công.",
            Username: user.Username,
            DisplayName: user.DisplayName,
            AvatarUrl: user.AvatarUrl,
            Bio: user.Bio
        );
    }
}
