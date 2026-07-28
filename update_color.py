import re

files = [
    r'ChatApp.Client\Components\Pinboard\StickyNote.razor',
    r'ChatApp.Client\Components\Pinboard\TaskCard.razor'
]

for file in files:
    with open(file, 'r', encoding='utf-8') as f:
        content = f.read()

    # Change tuple type
    content = content.replace("EventCallback<(Guid id, string content)>", "EventCallback<(Guid id, string content, string? color)>")
    
    # Change InvokeAsync
    content = content.replace("OnContentUpdated.InvokeAsync((Item.Id, editContent))", "OnContentUpdated.InvokeAsync((Item.Id, editContent, Item.Color))")

    # Add ChangeColor method if not exists
    if "private async Task ChangeColor" not in content:
        color_method = """
    private async Task ChangeColor(string newColor)
    {
        Item.Color = newColor;
        await OnContentUpdated.InvokeAsync((Item.Id, Item.Content, newColor));
    }
"""
        content = content.replace("private async Task CompleteTask()", color_method + "    private async Task CompleteTask()")
        content = content.replace("private async Task DeleteCard()", color_method + "    private async Task DeleteCard()")

    # Add color picker button to toolbar
    toolbar_start = '<div class="task-toolbar">'
    color_btn = '<input type="color" class="color-picker-btn" style="width:20px;height:20px;padding:0;border:none;background:transparent;cursor:pointer;margin-right:4px;" value="@(Item.Color ?? \\"#ffeb3b\\")" @onchange="@(e => ChangeColor(e.Value?.ToString() ?? \\"#ffeb3b\\"))" title="Đổi màu" />\n            '
    # Use double backslashes in script to output single backslash + quote in powershell string
    color_btn_true = r'<input type="color" class="color-picker-btn" style="width:20px;height:20px;padding:0;border:none;background:transparent;cursor:pointer;margin-right:4px;" value="@(Item.Color ?? \"#ffeb3b\")" @onchange="@(e => ChangeColor(e.Value?.ToString() ?? \"#ffeb3b\"))" title="Đổi màu" />' + '\n            '

    if 'type="color"' not in content:
        content = content.replace(toolbar_start, toolbar_start + '\n            ' + color_btn_true)
    
    # Apply background color based on Item.Color
    # in StickyNote:
    if "sticky-note" in content and "background-color" not in content:
        content = content.replace('style="transform:', 'style="background-color: @(Item.Color ?? \"#ffeb3b\"); transform:')
    elif "task-card" in content and "background-color" not in content:
        content = content.replace('style="transform:', 'style="background-color: @(Item.Color ?? \"#ffffff\"); transform:')

    with open(file, 'w', encoding='utf-8') as f:
        f.write(content)

# Update CanvasBoard
canvas_file = r'ChatApp.Client\Components\Pinboard\CanvasBoard.razor'
with open(canvas_file, 'r', encoding='utf-8') as f:
    canvas_content = f.read()

canvas_content = canvas_content.replace("EventCallback<(Guid id, string content)>", "EventCallback<(Guid id, string content, string? color)>")
canvas_content = canvas_content.replace("Task HandleContentUpdated((Guid id, string content) data)", "Task HandleContentUpdated((Guid id, string content, string? color) data)")
# Also JS interop UpdatePinboardItemContent if needed - wait, this is just passing event to parent.
with open(canvas_file, 'w', encoding='utf-8') as f:
    f.write(canvas_content)

# Update Home.razor
home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    home_content = f.read()

home_content = home_content.replace("Task HandleItemContentUpdated((Guid id, string content) data)", "Task HandleItemContentUpdated((Guid id, string content, string? color) data)")
home_content = home_content.replace("var cmd = new { Id = data.id, Content = data.content };", "var cmd = new { Id = data.id, Content = data.content, Color = data.color };")
home_content = home_content.replace("item.Content = data.content;", "item.Content = data.content; item.Color = data.color;")

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(home_content)

print("Done replacing.")
