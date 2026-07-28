import re

def update_file(filepath, is_sticky):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # 1. Add background color logic
    if is_sticky:
        content = content.replace('style="transform:', 'style="background-color: @(Item.Color ?? \\"#ffeb3b\\"); transform:')
    else:
        content = content.replace('style="transform:', 'style="background-color: @(Item.Color ?? \\"#ffffff\\"); transform:')

    # 2. Add color picker button
    toolbar = '<div class="task-toolbar">'
    color_btn = f'<input type="color" class="color-picker-btn" style="width:20px;height:20px;padding:0;border:none;background:transparent;cursor:pointer;margin-right:4px;" value="@(Item.Color ?? \\"#ffeb3b\\")" @onchange=\'@(e => ChangeColor(e.Value?.ToString() ?? \\"#ffeb3b\\"))\' title="Đổi màu" />\n            '
    
    # We replace only the first occurrence of <div class="task-toolbar">
    content = content.replace(toolbar, toolbar + '\n            ' + color_btn, 1)

    # 3. Update EventCallback signature
    content = content.replace("EventCallback<(Guid id, string content)> OnContentUpdated", "EventCallback<(Guid id, string content, string? color)> OnContentUpdated")

    # 4. Update InvokeAsync for SaveEdit
    content = content.replace("OnContentUpdated.InvokeAsync((Item.Id, editContent))", "OnContentUpdated.InvokeAsync((Item.Id, editContent, Item.Color))")

    # 5. Add ChangeColor method
    color_method = """
    private async Task ChangeColor(string newColor)
    {
        Item.Color = newColor;
        if (OnContentUpdated.HasDelegate)
        {
            await OnContentUpdated.InvokeAsync((Item.Id, Item.Content, newColor));
        }
    }
"""
    # Insert it right before "private async Task DeleteCard"
    content = content.replace("private async Task DeleteCard", color_method + "\n    private async Task DeleteCard")

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

update_file(r'ChatApp.Client\Components\Pinboard\StickyNote.razor', True)
update_file(r'ChatApp.Client\Components\Pinboard\TaskCard.razor', False)

print("Files updated safely.")
