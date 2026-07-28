import re

with open('ChatApp.WebApi/Hubs/ChatHub.cs', 'r', encoding='utf-8') as f:
    content = f.read()

bad_regex = r"public async Task DeletePinboardConnection\(DeletePinboardConnectionCommand command, string conversationId\)\s*\{\s*try\s*\{\s*var result = await _mediator\.Send\(command\);\s*if \(result\)\s*\{\s*\}\s*\}"

good_code = """public async Task DeletePinboardConnection(DeletePinboardConnectionCommand command, string conversationId)
    {
        try
        {
            var result = await _mediator.Send(command);
            if (result)
            {
                await Clients.Group(conversationId).SendAsync("PinboardConnectionDeleted", command.Id);
            }
        }
        catch (Exception ex) { await Clients.Caller.SendAsync("ErrorMessage", ex.Message); }
    }"""

content = re.sub(bad_regex, good_code, content)

with open('ChatApp.WebApi/Hubs/ChatHub.cs', 'w', encoding='utf-8') as f:
    f.write(content)

