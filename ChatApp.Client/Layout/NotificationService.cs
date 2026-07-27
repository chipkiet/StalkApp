using System;

namespace ChatApp.Client.Layout
{
    public class NotificationItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ConversationId { get; set; }
        public string ConversationTitle { get; set; } = "";
        public string MessagePreview { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    public class NotificationService
    {
        public event Action<NotificationItem>? OnNotification;
        public event Action<Guid>? OnNotificationClicked;
        
        public void Push(NotificationItem item) => OnNotification?.Invoke(item);
        public void NotifyClicked(Guid conversationId) => OnNotificationClicked?.Invoke(conversationId);
    }
}
