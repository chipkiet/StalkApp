import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    content = f.read()

old_func = """    private async Task HandleItemCreatedFromDrop((Guid msgId, string content, double x, double y) data)
    {
        var cmd = new {
            ConversationId = selectedId, 
            Type = ChatApp.Shared.Enums.PinboardItemType.Task,
            Content = data.content,
            PositionX = data.x,
            PositionY = data.y,
            LinkedMessageId = (Guid?)null,
            ZIndex = MyCanvasItems.Count + 1
        };
        
        if (hubConnection != null)
        {
            await hubConnection.SendAsync("CreatePinboardItem", cmd);
        }
    }"""

new_func = """    private async Task HandleItemCreatedFromDrop((Guid msgId, string content, double x, double y) data)
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

content = content.replace(old_func, new_func)

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(content)

print("Home.razor drop handler updated")
