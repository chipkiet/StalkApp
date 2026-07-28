import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    content = f.read()

old_func = """    private async Task HandleItemCreatedFromDrop((Guid msgId, string content, double x, double y) data)
    {
        var type = data.msgId == Guid.Empty 
            ? ChatApp.Shared.Enums.PinboardItemType.Image 
            : ChatApp.Shared.Enums.PinboardItemType.Task;

        var cmd = new {
            ConversationId = selectedId, 
            Type = type,
            Content = data.content,
            PositionX = data.x,
            PositionY = data.y,
            LinkedMessageId = data.msgId == Guid.Empty ? (Guid?)null : data.msgId,
            ZIndex = MyCanvasItems.Count + 1
        };
        
        if (hubConnection != null)
        {
            await hubConnection.SendAsync("CreatePinboardItem", cmd);
        }
    }"""

new_func = """    private async Task HandleItemCreatedFromDrop((Guid msgId, string content, double x, double y) data)
    {
        var type = ChatApp.Shared.Enums.PinboardItemType.Task;
        var actualContent = data.content;

        if (data.msgId == Guid.Empty) 
        {
            type = ChatApp.Shared.Enums.PinboardItemType.Image;
        }
        else 
        {
            var msg = messages?.FirstOrDefault(m => m.Id == data.msgId);
            if (msg != null)
            {
                if (msg.MessageType == ChatApp.Domain.Enums.MessageType.Image)
                {
                    type = ChatApp.Shared.Enums.PinboardItemType.Image;
                    actualContent = msg.AttachmentUrl ?? msg.Content;
                }
                else if (msg.MessageType == ChatApp.Domain.Enums.MessageType.File)
                {
                    if (!string.IsNullOrEmpty(msg.AttachmentUrl) && 
                        (msg.AttachmentUrl.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                         msg.AttachmentUrl.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || 
                         msg.AttachmentUrl.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || 
                         msg.AttachmentUrl.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) || 
                         msg.AttachmentUrl.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)))
                    {
                        type = ChatApp.Shared.Enums.PinboardItemType.Image;
                        actualContent = msg.AttachmentUrl;
                    }
                    else if (string.IsNullOrWhiteSpace(actualContent))
                    {
                        actualContent = msg.AttachmentName ?? msg.Content;
                    }
                }
            }
        }

        var cmd = new {
            ConversationId = selectedId, 
            Type = type,
            Content = actualContent,
            PositionX = data.x,
            PositionY = data.y,
            LinkedMessageId = data.msgId == Guid.Empty ? (Guid?)null : data.msgId,
            ZIndex = MyCanvasItems.Count + 1
        };
        
        if (hubConnection != null)
        {
            await hubConnection.SendAsync("CreatePinboardItem", cmd);
        }
    }"""

content = content.replace(old_func, new_func)

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(content)

print("Home.razor drop handler updated")
