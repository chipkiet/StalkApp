import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    content = f.read()

lines = content.split('\n')
for i in range(1950, 1970):
    print(f"{i+1}: {lines[i]}")
