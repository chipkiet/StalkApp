import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace ReceiveNewMessage to be async and call scrollToBottom
old_code_receive = """        hubConnection.On<MessageDto>("ReceiveNewMessage", (message) =>
        {
            if (selectedId == message.ConversationId)
            {
                if (messages == null) messages = new List<MessageDto>();
                messages.Add(message);

                if (inboxItems != null)
                {
                    var conv = inboxItems.FirstOrDefault(c => c.ConversationId == message.ConversationId);
                    if (conv != null)
                    {
                        var updatedConv = conv with { LastMessage = message.Content ?? "Tệp đính kèm", LastMessageAt = message.CreatedAt };
                        var idx = inboxItems.IndexOf(conv);
                        if (idx != -1) inboxItems[idx] = updatedConv;
                        if (selectedConversation?.ConversationId == updatedConv.ConversationId)
                        {
                            selectedConversation = updatedConv;
                        }
                        inboxItems = inboxItems.OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.LastMessageAt).ToList();
                    }
                }

                InvokeAsync(StateHasChanged);
            }"""

new_code_receive = """        hubConnection.On<MessageDto>("ReceiveNewMessage", async (message) =>
        {
            if (selectedId == message.ConversationId)
            {
                if (messages == null) messages = new List<MessageDto>();
                messages.Add(message);

                if (inboxItems != null)
                {
                    var conv = inboxItems.FirstOrDefault(c => c.ConversationId == message.ConversationId);
                    if (conv != null)
                    {
                        var updatedConv = conv with { LastMessage = message.Content ?? "Tệp đính kèm", LastMessageAt = message.CreatedAt };
                        var idx = inboxItems.IndexOf(conv);
                        if (idx != -1) inboxItems[idx] = updatedConv;
                        if (selectedConversation?.ConversationId == updatedConv.ConversationId)
                        {
                            selectedConversation = updatedConv;
                        }
                        inboxItems = inboxItems.OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.LastMessageAt).ToList();
                    }
                }

                await InvokeAsync(StateHasChanged);
                try { await JS.InvokeVoidAsync("scrollToBottom", "chat-messages-area"); } catch {}
            }"""

content = content.replace(old_code_receive, new_code_receive)

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated ReceiveNewMessage")
