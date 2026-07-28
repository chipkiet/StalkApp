import re

razor_file = r'ChatApp.Client\Components\Pinboard\CanvasBoard.razor'
with open(razor_file, 'r', encoding='utf-8') as f:
    razor_content = f.read()

# Add OnFileDropped
old_jsinvokables = """    [JSInvokable]
    public async Task OnMessageDropped(string msgId, string content, double x, double y)
    {
        if (Guid.TryParse(msgId, out var parsedId))
        {
            await OnItemCreatedFromDrop.InvokeAsync((parsedId, content, x, y));
        }
    }"""

new_jsinvokables = """    [JSInvokable]
    public async Task OnMessageDropped(string msgId, string content, double x, double y)
    {
        if (Guid.TryParse(msgId, out var parsedId))
        {
            await OnItemCreatedFromDrop.InvokeAsync((parsedId, content, x, y));
        }
    }

    [JSInvokable]
    public async Task OnFileDropped(string url, double x, double y)
    {
        await OnItemCreatedFromDrop.InvokeAsync((Guid.Empty, url, x, y));
    }"""

razor_content = razor_content.replace(old_jsinvokables, new_jsinvokables)

# Render ImageCard in switch
old_switch = """                        case PinboardItemType.Task:
                            <TaskCard Item="item" 
                                      IsConnecting="@(connectingSourceId == item.Id)"
                                      OnConnectRequested="StartConnection"
                                      OnDeleteRequested="HandleDelete" />
                            break;
                    }"""

new_switch = """                        case PinboardItemType.Task:
                            <TaskCard Item="item" 
                                      IsConnecting="@(connectingSourceId == item.Id)"
                                      OnConnectRequested="StartConnection"
                                      OnDeleteRequested="HandleDelete" />
                            break;
                        case PinboardItemType.Image:
                            <ImageCard Item="item" 
                                       IsConnecting="@(connectingSourceId == item.Id)"
                                       OnConnectRequested="StartConnection"
                                       OnDeleteRequested="HandleDelete" />
                            break;
                    }"""

razor_content = razor_content.replace(old_switch, new_switch)

with open(razor_file, 'w', encoding='utf-8') as f:
    f.write(razor_content)

print("CanvasBoard updated for ImageCard")
