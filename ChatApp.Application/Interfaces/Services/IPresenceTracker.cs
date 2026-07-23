namespace ChatApp.Application.Interfaces.Services;

public interface IPresenceTracker
{
    /// <summary>
    /// Đăng ký một kết nối mới cho user. Trả về true nếu đây là kết nối ĐẦU TIÊN (user vừa online).
    /// </summary>
    Task<bool> UserConnectedAsync(Guid userId, string connectionId);

    /// <summary>
    /// Huỷ đăng ký kết nối khi user ngắt. Trả về true nếu đây là kết nối CUỐI CÙNG (user vừa offline).
    /// </summary>
    Task<bool> UserDisconnectedAsync(Guid userId, string connectionId);

    /// <summary>
    /// Kiểm tra user hiện có đang online không.
    /// </summary>
    Task<bool> IsOnlineAsync(Guid userId);

    /// <summary>
    /// Lấy danh sách tất cả userId đang online.
    /// </summary>
    Task<IEnumerable<Guid>> GetOnlineUsersAsync();
}
