import re

def fix_razor_file(filepath, default_color):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Fix the @onchange lambda
    bad_lambda = f'@onchange="@(e => ChangeColor(e.Value?.ToString() ?? \\"{default_color}\\"))"'
    good_lambda = '@onchange="@(e => ChangeColor(e.Value?.ToString()))"'
    content = content.replace(bad_lambda, good_lambda)
    
    # Fix the ChangeColor signature
    bad_sig = "private async Task ChangeColor(string newColor)"
    good_sig = f"private async Task ChangeColor(string? newColor)\n    {{\n        newColor = newColor ?? \"{default_color}\";"
    content = content.replace(bad_sig + "\n    {", good_sig)

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

fix_razor_file(r'ChatApp.Client\Components\Pinboard\StickyNote.razor', "#ffeb3b")
fix_razor_file(r'ChatApp.Client\Components\Pinboard\TaskCard.razor', "#ffffff")

print("Fixed razor syntax.")
