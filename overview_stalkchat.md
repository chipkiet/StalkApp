# Dự án ZALO + TELEGRAM

## 1. Phân Hệ Xác Thực & Quản Lý Người Dùng (Auth & User)

Đây là cổng vào của ứng dụng, cần sự nhanh chóng và bảo mật.

- **Đăng nhập đa phương thức:**
    - Đăng nhập bằng Số điện thoại + OTP (Đặc trưng của cả Zalo và Telegram).
    - Đăng nhập nhanh bằng Mã QR (Quét từ app điện thoại - giống Zalo Web).
- **Quản lý hồ sơ (Profile):** Cho phép đổi ảnh đại diện (Avatar), tên hiển thị, tiểu sử (Bio), và tạo **Username** (để người khác tìm kiếm không cần số điện thoại giống Telegram).
- **Trạng thái hoạt động:** Hiển thị "Online", "Offline", hoặc "Hoạt động lần cuối lúc..." (Last seen).

## 2. Phân Hệ Nhắn Tin Thời Gian Thực (Core Chat)

Trọng tâm của trang web, bắt buộc phải sử dụng công nghệ Real-time

**Chat đơn (1-1) và Chat nhóm (Group Chat):**

- Tạo nhóm, đặt tên nhóm, ảnh đại diện nhóm.
- Phân quyền trong nhóm: Quản trị viên (Admin) và Thành viên (Member).

**Trạng thái tin nhắn:** Đang gửi (Sending), Đã gửi (Sent), Đã nhận (Delivered), và Đã xem (Read).

**Tính năng tin nhắn nâng cao:**

- **Thu hồi (Delete for everyone):** Giống cả 2 app.
- **Chỉnh sửa tin nhắn (Edit message):** Tính năng rất hay của Telegram.
- **Ghim tin nhắn (Pin):** Ghim các thông báo quan trọng lên đầu đoạn chat.
- **Trả lời (Reply) và Chuyển tiếp (Forward):** Trích dẫn tin nhắn cũ hoặc gửi sang chat khác.
- **Thả cảm xúc (Reaction):** Thả tim, hỷ, nộ, ái, ố vào tin nhắn.

## 3. Phân Hệ Truyền Tải File & Đa Phương Tiện (Media & Files)

- **Gửi định dạng đa dạng:** Hình ảnh, Video, File ghi âm (Voice message), và các file tài liệu (PDF, Word, Excel, ZIP...).
- **Kho lưu trữ Media (Cloud/Saved Messages):**
    - **Zalo gọi là "Cloud của tôi", Telegram gọi là "Saved Messages".** Đây là nơi người dùng tự gửi file/tin nhắn để lưu trữ cá nhân.
    - **Bộ sưu tập chung (Shared Media):** Khi vào chi tiết một cuộc trò chuyện, người dùng có thể xem lại toàn bộ Ảnh, Link, File đã từng gửi cho nhau (phân loại rõ ràng).4.

## 4. Phân Hệ Gọi Điện Qua Video Và Voice

### Đối với Gọi Thoại (Voice Call)

- **Màn hình cuộc gọi:** Hiển thị trạng thái cuộc gọi (Đang đổ chuông, Đang kết nối, Đã kết nối, Kết thúc).
- **Bật/Tắt Micro (Mute/Unmute):** Cho phép người dùng tạm thời tắt tiếng của mình.
- **Bật/Tắt Loa ngoài (Speaker):** Chuyển đổi giữa tai nghe/loa mặc định và loa ngoài (nếu thiết bị hỗ trợ).

### Đối với Gọi Video (Face Call)

- Bao gồm toàn bộ các tính năng của Voice Call, cộng thêm:
- **Bật/Tắt Camera (Video On/Off):** Người dùng có thể tắt camera để chuyển về cuộc gọi thoại bất cứ lúc nào.
- **Lật Camera (Flip Camera):** Chuyển đổi giữa camera trước và sau (rất quan trọng khi người dùng truy cập web bằng điện thoại).
- **Chế độ hình trong hình (Picture-in-Picture):** Cho phép người dùng thu nhỏ màn hình video của đối phương thành một ô nhỏ để họ vừa gọi điện vừa có thể nhắn tin hoặc lướt xem các đoạn chat khác (Telegram làm phần này cực mượt).

### Quản lý Trạng thái & Thông báo cuộc gọi

- **Tín hiệu cuộc gọi (Signaling):** Khi User A bấm gọi, Backend (qua WebSocket) phải lập tức gửi một thông báo thời gian thực tới User B để kích hoạt màn hình chờ cuộc gọi (đổ chuông + rung).
- **Từ chối / Chấp nhận (Reject/Accept):** Xử lý các sự kiện khi người nhận bấm nghe hoặc cúp máy.
- **Thông báo cuộc gọi nhỡ (Missed Call):** Tự động tạo một tin nhắn hệ thống trong hộp thoại dạng: *"Cuộc gọi thoại nhỡ lúc 14:30"*.

## 4. Phân Hệ Nâng Cao :

Nếu muốn trang web thực sự "pro", bạn có thể hướng tới:

1. **Gọi nhóm (Group Call):** Gọi video hoặc voice cho nhiều người cùng lúc trong nhóm.
2. **Chia sẻ màn hình (Screen Sharing):** Tính năng cực kỳ ăn tiền của Telegram Web/PC. Người dùng có thể share màn hình máy tính của họ để làm việc nhóm, thuyết trình.
3. **Thay đổi Background / Bộ lọc (Filters):** Làm mờ nền hoặc đổi hình nền phía sau khi gọi video (giống Zoom, Zalo).