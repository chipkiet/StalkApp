import re

def update_card(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Replace @Item.Content with @((MarkupString)ParseLinks(Item.Content)) in else block of isEditing
    old_content_render = """        else
        {
            @Item.Content
        }"""
    new_content_render = """        else
        {
            @((MarkupString)ParseLinks(Item.Content))
        }"""
    content = content.replace(old_content_render, new_content_render)

    # Add ParseLinks function
    parse_links_func = """    private string ParseLinks(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return "";
        var encoded = System.Net.WebUtility.HtmlEncode(content);
        var urlRegex = new System.Text.RegularExpressions.Regex(
            @"(http|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
        return urlRegex.Replace(encoded, "<a href=\\"$0\\" target=\\"_blank\\" class=\\"text-accent\\" style=\\"text-decoration: underline;\\" onclick=\\"event.stopPropagation()\\">$0</a>").Replace("\\n", "<br/>");
    }
}"""
    # Find the last closing brace and replace it
    content = content.rsplit('}', 1)[0] + parse_links_func

    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)

update_card(r'ChatApp.Client\Components\Pinboard\TaskCard.razor')
update_card(r'ChatApp.Client\Components\Pinboard\StickyNote.razor')
print("Cards updated with ParseLinks")
