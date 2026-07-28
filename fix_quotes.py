import re

def fix_quotes(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Remove the backslashes inside value="@(...)"
    content = content.replace('\\"#ffeb3b\\"', '"#ffeb3b"')
    content = content.replace('\\"#ffffff\\"', '"#ffffff"')

    # Also style background-color
    content = content.replace('style="background-color: @(Item.Color ?? \\"#ffeb3b\\");', 'style="background-color: @(Item.Color ?? \\"#ffeb3b\\");'.replace('\\"', '"'))
    content = content.replace('style="background-color: @(Item.Color ?? \\"#ffffff\\");', 'style="background-color: @(Item.Color ?? \\"#ffffff\\");'.replace('\\"', '"'))

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

fix_quotes(r'ChatApp.Client\Components\Pinboard\StickyNote.razor')
fix_quotes(r'ChatApp.Client\Components\Pinboard\TaskCard.razor')

print("Fixed quotes.")
