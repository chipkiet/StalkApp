import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Add scroll to bottom in OnAfterRenderAsync
old_code_render = """        if (messages != null && !isLoadingMessages && !messagesRendered)
        {
            messagesRendered = true;
            try
            {
                await JS.InvokeVoidAsync("canvasPhysics.initDraggableMessages");
            }
            catch { /* Ignore if script not loaded yet */ }
        }"""

new_code_render = """        if (messages != null && !isLoadingMessages && !messagesRendered)
        {
            messagesRendered = true;
            try
            {
                await JS.InvokeVoidAsync("canvasPhysics.initDraggableMessages");
                await JS.InvokeVoidAsync("scrollToBottom", "chat-messages-area");
            }
            catch { /* Ignore if script not loaded yet */ }
        }"""

content = content.replace(old_code_render, new_code_render)

# Now, also add scroll in SendMessage
old_code_send = """        }

        newMessage = "";
        await InvokeAsync(StateHasChanged);
    }"""

new_code_send = """        }

        newMessage = "";
        await InvokeAsync(StateHasChanged);
        try { await JS.InvokeVoidAsync("scrollToBottom", "chat-messages-area"); } catch {}
    }"""
    
content = content.replace(old_code_send, new_code_send)

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated OnAfterRenderAsync and SendMessage to scroll")
