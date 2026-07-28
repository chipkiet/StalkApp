import re

home_file = r'ChatApp.Client\Pages\Home.razor'
with open(home_file, 'r', encoding='utf-8') as f:
    content = f.read()

old_code = """    private async Task HandleItemDeleted(Guid id)
    {
        var item = MyCanvasItems.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            MyCanvasItems.Remove(item);
            StateHasChanged();
            
            var cmd = new { Id = id };
            if (hubConnection != null)
            {
                await hubConnection.SendAsync("DeletePinboardItem", cmd, selectedId.ToString());
            }
        }
    }"""
    
new_code = """    private async Task HandleItemDeleted(Guid id)
    {
        var item = MyCanvasItems.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            MyCanvasItems.Remove(item);
            StateHasChanged();
            
            var cmd = new { Id = id };
            if (hubConnection != null)
            {
                // Sử dụng InvokeAsync để chờ server xử lý xong, tránh lỗi EF Core concurrent
                await hubConnection.InvokeAsync("DeletePinboardItem", cmd, selectedId.ToString());
            }
        }
    }"""

content = content.replace(old_code, new_code)

with open(home_file, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated HandleItemDeleted to use InvokeAsync")
