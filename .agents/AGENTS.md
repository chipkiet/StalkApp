# STALKchat - AI Agent Guidelines

Chào mừng bạn đến với dự án **STALKchat**. Đây là tài liệu quy định các nguyên tắc kiến trúc và phong cách lập trình dành cho các AI Agents khi tham gia phát triển dự án này. Hãy đọc kỹ và tuân thủ tuyệt đối các quy định dưới đây trước khi sinh mã hoặc thay đổi cấu trúc dự án.

## 1. Kiến Trúc Tổng Thể (Architecture)
- **Công nghệ cốt lõi**: Dự án là một ứng dụng Full-stack .NET sử dụng mô hình **Hosted Blazor WebAssembly** trên nền tảng **.NET 8**.
- **Không sử dụng Javascript SPA**: Dự án này tuyệt đối **KHÔNG** sử dụng React, Vue hay Angular. Mọi giao diện người dùng phải được viết bằng Blazor (`.razor` components) trong dự án `ChatApp.Client`.

## 2. Cấu Trúc Solution (`STALK` directory)
Solution bao gồm các thành phần theo mô hình Clean Architecture:
- `ChatApp.Client`: Ứng dụng Blazor WebAssembly (Frontend).
- `ChatApp.WebApi`: Ứng dụng ASP.NET Core Web API đóng vai trò là Backend Server, đồng thời phân phối (host) các tệp tĩnh của `ChatApp.Client`.
- `ChatApp.Application`: Chứa logic nghiệp vụ (CQRS bằng MediatR), Services và Interfaces.
- `ChatApp.Domain`: Chứa các Entities cốt lõi.
- `ChatApp.Infrastructure`: Chứa cấu hình Data Access (Entity Framework Core), SignalR Hubs và các dịch vụ bên ngoài.
- `ChatApp.Shared`: **[QUAN TRỌNG]** Đây là thư viện dùng chung cho cả Frontend và Backend. Tất cả các Data Transfer Objects (DTOs), Enums, Constants hoặc request/response models bắt buộc phải được đặt ở đây để có thể chia sẻ trực tiếp mã nguồn C# giữa Server và Client.

## 3. Cơ Sở Dữ Liệu (Database)
- **Hệ quản trị CSDL**: PostgreSQL.
- **Môi trường cục bộ (Local)**: Sử dụng Docker thông qua file `docker-compose.yml`. Lưu ý cổng được map ra máy host là `5434` (để tránh xung đột với native postgres). 
  - *Connection String*: `Host=localhost;Port=5434;Database=stalkchat_db;Username=postgres;Password=stalk_password_123`
- **ORM**: Entity Framework Core với phương pháp **Code-First**. Mọi thay đổi về database phải được thực hiện thông qua Migrations trong dự án `ChatApp.Infrastructure`.

## 4. Quy Tắc Code & Workflow
- **Đặt tên nhánh (Branch Naming)**: Mọi tính năng mới phải tạo nhánh với cú pháp `feat/<tên-chức-năng>-<tên-người-code>` (Ví dụ: `feat/register-kiet`).
- **Real-time Chat**: Các tính năng gửi/nhận tin nhắn thời gian thực bắt buộc phải thông qua **SignalR** (`ChatHub`).
- **Tránh trùng lặp**: Luôn kiểm tra xem một Model, Enum hay DTO đã tồn tại trong `ChatApp.Shared` hay chưa trước khi tạo mới.

---
*Lưu ý dành cho AI*: Mọi lệnh thay đổi code hoặc kiến trúc phải được đánh giá dựa trên mức độ ảnh hưởng tới mô hình Hosted Blazor hiện tại. Luôn ưu tiên sử dụng C# thay vì Javascript cho logic Frontend.
