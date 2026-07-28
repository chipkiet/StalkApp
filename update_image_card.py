import re

image_card_file = r'ChatApp.Client\Components\Pinboard\ImageCard.razor'
with open(image_card_file, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("OnConnectRequested", "OnConnectClicked")
content = content.replace("OnDeleteRequested", "OnTaskDeleted")

with open(image_card_file, 'w', encoding='utf-8') as f:
    f.write(content)

print("ImageCard.razor updated")
