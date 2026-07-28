import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    content = f.read()

old_canvas = """            OnTaskCompleted="HandleTaskCompleted" 
            OnItemDeleted="HandleItemDeleted"
            OnItemContentUpdated="HandleItemContentUpdated"
            OnConnectionCreated="HandleConnectionCreated"
            OnConnectionDeleted="HandleConnectionDeleted" />"""
            
new_canvas = """            OnTaskCompleted="HandleTaskCompleted" 
            OnItemDeleted="HandleItemDeleted"
            OnItemContentUpdated="HandleItemContentUpdated"
            OnConnectionCreated="HandleConnectionCreated"
            OnConnectionDeleted="HandleConnectionDeleted"
            OnCreateItemRequested="HandleCreateItemRequested" />"""

content = content.replace(old_canvas, new_canvas)

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Home.razor")
