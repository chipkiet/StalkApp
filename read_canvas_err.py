import re

canvas_file = r'ChatApp.Client\Components\Pinboard\CanvasBoard.razor'
with open(canvas_file, 'r', encoding='utf-8') as f:
    content = f.read()

lines = content.split('\n')
for i in range(85, 120):
    if i < len(lines):
        print(f"{i+1}: {lines[i]}")
