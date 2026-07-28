import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    content = f.read()

old_logic = """                    else if (string.IsNullOrWhiteSpace(actualContent))
                    {
                        actualContent = msg.AttachmentName ?? msg.Content;
                    }"""
                    
new_logic = """                    else 
                    {
                        type = ChatApp.Shared.Enums.PinboardItemType.File;
                        actualContent = $"{msg.AttachmentUrl}|{msg.AttachmentName}";
                    }"""

content = content.replace(old_logic, new_logic)

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(content)
