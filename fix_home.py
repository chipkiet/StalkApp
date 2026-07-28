import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    home_content = f.read()

old_cmd_create = """                var cmd = new ChatApp.Application.Features.Pinboard.Commands.CreatePinboardConnection.CreatePinboardConnectionCommand(
                    selectedId, 
                    data.sourceId, 
                    data.targetId,
                    "Related"
                );"""
new_cmd_create = """                var cmd = new {
                    ConversationId = selectedId, 
                    SourceItemId = data.sourceId, 
                    TargetItemId = data.targetId,
                    Label = "Related"
                };"""

old_cmd_delete = """var cmd = new ChatApp.Application.Features.Pinboard.Commands.DeletePinboardConnection.DeletePinboardConnectionCommand(connectionId);"""
new_cmd_delete = """var cmd = new { Id = connectionId };"""

home_content = home_content.replace(old_cmd_create, new_cmd_create)
home_content = home_content.replace(old_cmd_delete, new_cmd_delete)

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(home_content)

print("Home.razor fixed.")
