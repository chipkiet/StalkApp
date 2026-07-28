import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    home_content = f.read()

# Replace HandleConnectionCreated to use SignalR
old_handle_conn = """    private async Task HandleConnectionCreated((Guid sourceId, Guid targetId) data)
    {
        try
        {
            var command = new 
            {
                SourceItemId = data.sourceId,
                TargetItemId = data.targetId
            };

            var response = await Http.PostAsJsonAsync($"api/pinboard/{selectedConversation?.ConversationId}/connections", command);
            if (response.IsSuccessStatusCode)
            {
                var connection = await response.Content.ReadFromJsonAsync<PinboardConnectionDto>();
                if (connection != null && MyCanvasConnections != null)
                {
                    MyCanvasConnections.Add(connection);
                    StateHasChanged();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating connection: {ex.Message}");
        }
    }"""

new_handle_conn = """    private async Task HandleConnectionCreated((Guid sourceId, Guid targetId) data)
    {
        try
        {
            if (hubConnection != null)
            {
                var cmd = new ChatApp.Application.Features.Pinboard.Commands.CreatePinboardConnection.CreatePinboardConnectionCommand(
                    selectedId, 
                    data.sourceId, 
                    data.targetId,
                    "Related"
                );
                await hubConnection.SendAsync("CreatePinboardConnection", cmd);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating connection: {ex.Message}");
        }
    }
    
    private async Task HandleConnectionDeleted(Guid connectionId)
    {
        try
        {
            if (hubConnection != null)
            {
                var cmd = new ChatApp.Application.Features.Pinboard.Commands.DeletePinboardConnection.DeletePinboardConnectionCommand(connectionId);
                await hubConnection.SendAsync("DeletePinboardConnection", cmd, selectedId.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting connection: {ex.Message}");
        }
    }
"""

home_content = home_content.replace(old_handle_conn, new_handle_conn)
home_content = home_content.replace('OnConnectionCreated="HandleConnectionCreated"', 'OnConnectionCreated="HandleConnectionCreated"\n                          OnConnectionDeleted="HandleConnectionDeleted"')

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(home_content)

# Update CanvasBoard.razor
canvas_file = r'ChatApp.Client\Components\Pinboard\CanvasBoard.razor'
with open(canvas_file, 'r', encoding='utf-8') as f:
    canvas_content = f.read()

# Add OnConnectionDeleted event callback
if "public EventCallback<Guid> OnConnectionDeleted { get; set; }" not in canvas_content:
    canvas_content = canvas_content.replace("public EventCallback<(Guid sourceId, Guid targetId)> OnConnectionCreated { get; set; }", "public EventCallback<(Guid sourceId, Guid targetId)> OnConnectionCreated { get; set; }\n\n    [Parameter]\n    public EventCallback<Guid> OnConnectionDeleted { get; set; }")

# Add HandleConnectionDeleted method
if "private async Task HandleConnectionDeleted(Guid connectionId)" not in canvas_content:
    method = """    private async Task HandleConnectionDeleted(Guid connectionId)
    {
        if (OnConnectionDeleted.HasDelegate)
        {
            await OnConnectionDeleted.InvokeAsync(connectionId);
        }
    }
"""
    canvas_content = canvas_content.replace("private async Task HandleContentUpdated", method + "    private async Task HandleContentUpdated")

# Add UI interaction to canvas connection path
old_path = """<path @key="conn.Id" 
                          d="@pathData" 
                          data-source-id="@conn.SourceItemId"
                          data-target-id="@conn.TargetItemId"
                          fill="none" 
                          stroke="var(--accent-primary)" 
                          stroke-width="3" 
                          class="canvas-connection" />"""

new_path = """<path @key="conn.Id" 
                          d="@pathData" 
                          data-source-id="@conn.SourceItemId"
                          data-target-id="@conn.TargetItemId"
                          fill="none" 
                          stroke="var(--accent-primary)" 
                          stroke-width="6" 
                          class="canvas-connection" 
                          style="cursor: pointer;"
                          @onclick="() => HandleConnectionDeleted(conn.Id)"
                          title="Nhấn để xóa đường nối" />"""

canvas_content = canvas_content.replace(old_path, new_path)

with open(canvas_file, 'w', encoding='utf-8') as f:
    f.write(canvas_content)

print("Connections updated.")
