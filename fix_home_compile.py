import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    content = f.read()

old_logic = """    private async Task HandleCreateItemRequested(ChatApp.Shared.Enums.PinboardItemType type)
    {
        if (pinboardHubConnection != null && activeConversationId.HasValue)
        {
            var newItem = new ChatApp.Shared.DTOs.Pinboard.CreatePinboardItemDto
            {
                ConversationId = activeConversationId.Value,
                Type = type,
                Content = type == ChatApp.Shared.Enums.PinboardItemType.Task ? "New Task" : "New Note",
                PositionX = 100, // We could dynamically get center if needed, for now just offset
                PositionY = 100,
                ZIndex = pinboardItems.Count > 0 ? pinboardItems.Max(i => i.ZIndex) + 1 : 1,
                Color = type == ChatApp.Shared.Enums.PinboardItemType.StickyNote ? "#ffeb3b" : "#ffffff",
                IsCompleted = false
            };
            await pinboardHubConnection.SendAsync("CreatePinboardItem", newItem);
        }
    }"""
    
new_logic = """    private async Task HandleCreateItemRequested(ChatApp.Shared.Enums.PinboardItemType type)
    {
        if (hubConnection != null && selectedId != Guid.Empty)
        {
            var cmd = new {
                ConversationId = selectedId,
                Type = type,
                Content = type == ChatApp.Shared.Enums.PinboardItemType.Task ? "New Task" : "New Note",
                PositionX = 100.0,
                PositionY = 100.0,
                LinkedMessageId = (Guid?)null,
                ZIndex = MyCanvasItems.Count + 1,
                Color = type == ChatApp.Shared.Enums.PinboardItemType.StickyNote ? "#ffeb3b" : "#ffffff"
            };
            await hubConnection.SendAsync("CreatePinboardItem", cmd);
        }
    }"""

content = content.replace(old_logic, new_logic)

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(content)
