import re

enum_file = r'ChatApp.Shared\Enums\PinboardItemType.cs'
with open(enum_file, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("Image = 4", "Image = 4,\n        File = 5")

with open(enum_file, 'w', encoding='utf-8') as f:
    f.write(content)

print("PinboardItemType updated")
