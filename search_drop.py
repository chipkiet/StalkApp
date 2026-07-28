import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    content = f.read()

lines = content.split('\n')
for i, line in enumerate(lines):
    if 'HandleItemCreatedFromDrop' in line:
        for j in range(i, i+30):
            if j < len(lines):
                print(f"{j+1}: {lines[j]}")
        break
