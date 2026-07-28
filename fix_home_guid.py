import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    home_content = f.read()

home_content = home_content.replace('UserId = Guid.Parse(currentUserId)', 'UserId = currentUserId')

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(home_content)

print("Home.razor fixed for currentUserId Guid.")
