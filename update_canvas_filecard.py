import re

canvas_file = r'ChatApp.Client\Components\Pinboard\CanvasBoard.razor'
with open(canvas_file, 'r', encoding='utf-8') as f:
    content = f.read()

old_logic = """            else if (item.Type == PinboardItemType.Image)
            {
                <ImageCard @key="item.Id" Item="item" IsConnecting="isConnecting" OnConnectClicked="HandleConnectClicked" OnTaskDeleted="HandleItemDeleted" />
            }"""

new_logic = """            else if (item.Type == PinboardItemType.Image)
            {
                <ImageCard @key="item.Id" Item="item" IsConnecting="isConnecting" OnConnectClicked="HandleConnectClicked" OnTaskDeleted="HandleItemDeleted" />
            }
            else if (item.Type == PinboardItemType.File)
            {
                <FileCard @key="item.Id" Item="item" IsConnecting="isConnecting" OnConnectClicked="HandleConnectClicked" OnTaskDeleted="HandleItemDeleted" />
            }"""

content = content.replace(old_logic, new_logic)

with open(canvas_file, 'w', encoding='utf-8') as f:
    f.write(content)
print("Added FileCard to CanvasBoard")
