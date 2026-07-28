import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    content = f.read()

old_cond = """                         msg.AttachmentUrl.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)))"""
new_cond = """                         msg.AttachmentUrl.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ||
                         msg.AttachmentUrl.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)))"""

content = content.replace(old_cond, new_cond)

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(content)
