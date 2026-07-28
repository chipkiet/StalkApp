import re

js_file = r'ChatApp.Client\wwwroot\js\canvas-physics.js'
with open(js_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Change mousedown to pointerdown
content = content.replace("addEventListener('mousedown'", "addEventListener('pointerdown'")
content = content.replace("addEventListener('mousemove'", "addEventListener('pointermove'")
content = content.replace("addEventListener('mouseup'", "addEventListener('pointerup'")

with open(js_file, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated to pointer events in JS")
