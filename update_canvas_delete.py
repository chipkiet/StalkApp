import re

canvas_file = r'ChatApp.Client\Components\Pinboard\CanvasBoard.razor'
with open(canvas_file, 'r', encoding='utf-8') as f:
    content = f.read()

old_code = """    [JSInvokable]
    public async Task OnItemContentUpdatedJs(string idStr, string newContent)
    {
        if (Guid.TryParse(idStr, out var id))
        {
            await OnItemContentUpdated.InvokeAsync((id, newContent, null));
        }
    }"""
    
new_code = """    [JSInvokable]
    public async Task OnItemContentUpdatedJs(string idStr, string newContent)
    {
        if (Guid.TryParse(idStr, out var id))
        {
            await OnItemContentUpdated.InvokeAsync((id, newContent, null));
        }
    }

    [JSInvokable]
    public async Task OnCardsDeletedJS(string[] itemIds)
    {
        foreach (var idStr in itemIds)
        {
            if (Guid.TryParse(idStr, out var parsedId))
            {
                await OnItemDeleted.InvokeAsync(parsedId);
            }
        }
    }"""

content = content.replace(old_code, new_code)

with open(canvas_file, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated CanvasBoard.razor with OnCardsDeletedJS")
