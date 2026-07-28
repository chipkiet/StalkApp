import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    home_content = f.read()

home_content = home_content.replace('var cmd = new { TaskId = id };', 'var cmd = new { TaskId = id, UserId = Guid.Parse(currentUserId) };')

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(home_content)

print("Home.razor fixed for TaskCompleted.")
