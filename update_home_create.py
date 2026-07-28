import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Add OnCreateItemRequested to CanvasBoard usage
old_canvas = """                        <CanvasBoard Items="pinboardItems" 
                                     OnItemDeleted="DeletePinboardItem"
                                     OnItemUpdated="UpdatePinboardItem"
                                     OnItemCreatedFromDrop="HandleItemCreatedFromDrop" />"""
new_canvas = """                        <CanvasBoard Items="pinboardItems" 
                                     OnItemDeleted="DeletePinboardItem"
                                     OnItemUpdated="UpdatePinboardItem"
                                     OnItemCreatedFromDrop="HandleItemCreatedFromDrop"
                                     OnCreateItemRequested="HandleCreateItemRequested" />"""
content = content.replace(old_canvas, new_canvas)

# Add HandleCreateItemRequested
old_end = """        }
    }
}
"""
new_end = """        }
    }

    private async Task HandleCreateItemRequested(ChatApp.Shared.Enums.PinboardItemType type)
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
    }
}
"""
content = content.replace(old_end, new_end)

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(content)
print("Home.razor updated with HandleCreateItemRequested")
