import re

canvas_file = r'ChatApp.Client\Components\Pinboard\CanvasBoard.razor'
with open(canvas_file, 'r', encoding='utf-8') as f:
    canvas_content = f.read()

canvas_content = canvas_content.replace("await OnItemContentUpdated.InvokeAsync((id, newContent));", "await OnItemContentUpdated.InvokeAsync((id, newContent, null));")

with open(canvas_file, 'w', encoding='utf-8') as f:
    f.write(canvas_content)

print("CanvasBoard fixed.")
