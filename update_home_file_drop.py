import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    content = f.read()

old_logic = """        if (data.msgId == Guid.Empty) 
        {
            type = ChatApp.Shared.Enums.PinboardItemType.Image;
        }"""
        
new_logic = """        if (data.msgId == Guid.Empty) 
        {
            type = ChatApp.Shared.Enums.PinboardItemType.Image;
        }
        else if (data.msgId == Guid.Parse("00000000-0000-0000-0000-000000000001"))
        {
            type = ChatApp.Shared.Enums.PinboardItemType.File;
        }"""

content = content.replace(old_logic, new_logic)

# Wait, we need to fix LinkedMessageId too
old_linked = """            LinkedMessageId = data.msgId == Guid.Empty ? (Guid?)null : data.msgId,"""
new_linked = """            LinkedMessageId = (data.msgId == Guid.Empty || data.msgId == Guid.Parse("00000000-0000-0000-0000-000000000001")) ? (Guid?)null : data.msgId,"""
content = content.replace(old_linked, new_linked)

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(content)
