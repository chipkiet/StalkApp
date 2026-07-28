import re

canvas_file = r'ChatApp.Client\Components\Pinboard\CanvasBoard.razor'
with open(canvas_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Add to the end of the file
new_methods = """
    [Parameter]
    public EventCallback<PinboardItemType> OnCreateItemRequested { get; set; }

    private async Task CreateNewStickyNote()
    {
        await OnCreateItemRequested.InvokeAsync(PinboardItemType.StickyNote);
    }

    private async Task CreateNewTask()
    {
        await OnCreateItemRequested.InvokeAsync(PinboardItemType.Task);
    }
}
"""
content = content.rsplit('}', 1)[0] + new_methods

with open(canvas_file, 'w', encoding='utf-8') as f:
    f.write(content)
print("Added methods to CanvasBoard.razor")
