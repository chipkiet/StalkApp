# Danh sách Use Case — STALK Chat

**Tiến độ:** 10/26 UC đã đạt (backend + demo UI)

## Phân hệ Auth & User

- [ ] UC-01	Login with Phone + OTP	User	User logs into the system using phone number and OTP verification. **[Quan trọng]**
- [ ] UC-02	Register Account	Guest	Guest creates a new account using phone number and OTP. **[Quan trọng]**
- [ ] UC-03	Update Profile	User	User updates personal information (name, bio, avatar, username). **[Quan trọng]**
- [ ] UC-21	Login with QR Code	User	User logs into the system by scanning a QR Code with the mobile application. **[Quan trọng]**
- [ ] UC-22	View User Status	User	User views another user's online status, offline status, or last seen information.

## Phân hệ Core Chat

- [x] UC-04	Send Message	User	User sends text messages (realtime via SignalR). Advanced extras (reply/reaction/forward…) tách UC riêng. **[Quan trọng]**
- [x] UC-07	Send Media and Files	User	User sends images, videos, voice messages, and document files. **[Quan trọng]**
- [x] UC-08	Create Group Chat	User	User creates a new group chat and adds members. **[Quan trọng]**
- [ ] UC-09	Manage Group Chat	User	User manages group settings (add/remove members, change name, avatar). **[Quan trọng]**
- [ ] UC-10	Reply to Message	User	User replies to a specific message in a chat.
- [x] UC-11	Add Reaction	User	User adds emoji reaction to a message.
- [ ] UC-12	Forward Message	User	User forwards a message to another chat.
- [x] UC-13	Edit Message	User	User edits a sent message (within time limit).
- [x] UC-14	Delete Message (For Everyone)	User	User deletes a message for all participants. **[Quan trọng]**
- [x] UC-15	Pin Message	User	User pins an important message in the chat.
- [ ] UC-16	Mention User	User	User mentions (@) another user in group chat.
- [ ] UC-17	Search in Chat	User	User searches for messages, media, or files within chats.
- [ ] UC-18	Save to Cloud (Saved Messages)	User	User saves messages or files to personal cloud storage.
- [ ] UC-19	View Shared Media	User	User views all media and files shared in a conversation.

## Phân hệ Voice / Video Call

- [x] UC-05	Start Voice Call	User	User initiates a voice call with another user. **[Quan trọng]**
- [x] UC-06	Start Video Call	User	User initiates a video call with camera and WebRTC. **[Quan trọng]**
- [ ] UC-20	View Call History	User	User views history of all voice and video calls.
- [x] UC-23	Accept or Reject Call	User	User accepts or rejects an incoming voice or video call request. **[Quan trọng]**
- [ ] UC-24	Manage Voice Call	User	User manages an active voice call by muting/unmuting the microphone and switching speaker mode.
- [ ] UC-25	Manage Video Call	User	User manages an active video call by toggling the camera, flipping the camera, and using Picture-in-Picture mode.
- [ ] UC-26	Receive Missed Call Notification	User	User receives a missed call notification when an incoming call is not answered. **[Quan trọng]**

---

## Ghi chú kiểm tra codebase (23/07/2026)

| UC | Trạng thái | Bằng chứng |
|---|---|---|
| UC-04 | Đã đạt | `ChatHub.SendMessage`, `SendMessageCommandHandler` |
| UC-05 / UC-06 | Đã đạt | `ChatHub.InitiateCall` + WebRTC relay (`SendWebRTCSignal`) |
| UC-07 | Đã đạt | `AttachmentsController.UploadFile` + gửi kèm attachment |
| UC-08 | Đã đạt | `CreateConversationCommand` (Direct/Group + Admin/Member) |
| UC-11 | Đã đạt | `AddReaction` / `RemoveReaction` + hub `MessageReactionUpdated` |
| UC-13 | Đã đạt | `EditMessage` (giới hạn 15 phút) |
| UC-14 | Đã đạt | `DeleteMessage` soft-delete for everyone |
| UC-15 | Đã đạt | `PinMessage` |
| UC-23 | Đã đạt | `ChatHub.AcceptCall` / `RejectCall` |
| UC-10, 12, 18, 22 | Schema only | Có field/entity trong DB, chưa có API/handler |
| UC-01→03, 09, 16→17, 19→21, 24→26 | Chưa làm | Chưa có controller/handler tương ứng |
