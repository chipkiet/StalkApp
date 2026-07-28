import re

canvas_file = r'ChatApp.Client\Components\Pinboard\CanvasBoard.razor'
with open(canvas_file, 'r', encoding='utf-8') as f:
    content = f.read()

old_drop = """    [JSInvokable]
    public async Task OnFileDropped(string url, double x, double y)
    {
        await OnItemCreatedFromDrop.InvokeAsync((Guid.Empty, url, x, y));
    }"""

new_drop = """    [JSInvokable]
    public async Task OnFileDropped(string url, string fileName, bool isImage, double x, double y)
    {
        var payload = isImage ? url : $"{url}|{fileName}";
        var msgId = isImage ? Guid.Empty : Guid.Parse("00000000-0000-0000-0000-000000000001");
        // We use a dummy Guid to distinguish between File and Image drop from desktop
        await OnItemCreatedFromDrop.InvokeAsync((msgId, payload, x, y));
    }"""

content = content.replace(old_drop, new_drop)

with open(canvas_file, 'w', encoding='utf-8') as f:
    f.write(content)
