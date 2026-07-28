import re

canvas_file = r'ChatApp.Client\Components\Pinboard\CanvasBoard.razor'
with open(canvas_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Add Create tools to UI
old_end = """    </div>
</div>"""
new_end = """    </div>

    <!-- Nút Tạo Mới Trên Canvas -->
    <div class="canvas-floating-toolbar">
        <button class="btn btn-create-note" @onclick="CreateNewStickyNote" title="Tạo Note">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
            </svg>
            Note
        </button>
        <button class="btn btn-create-task" @onclick="CreateNewTask" title="Tạo Task">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
            </svg>
            Task
        </button>
    </div>
</div>"""
content = content.replace(old_end, new_end)

# Add parameters and handlers
old_code_end = """    [JSInvokable]
    public async Task OnFileDropped(string url, string fileName, bool isImage, double x, double y)
    {
        var payload = isImage ? url : $"{url}|{fileName}";
        var msgId = isImage ? Guid.Empty : Guid.Parse("00000000-0000-0000-0000-000000000001");
        // We use a dummy Guid to distinguish between File and Image drop from desktop
        await OnItemCreatedFromDrop.InvokeAsync((msgId, payload, x, y));
    }
}"""
new_code_end = """    [JSInvokable]
    public async Task OnFileDropped(string url, string fileName, bool isImage, double x, double y)
    {
        var payload = isImage ? url : $"{url}|{fileName}";
        var msgId = isImage ? Guid.Empty : Guid.Parse("00000000-0000-0000-0000-000000000001");
        // We use a dummy Guid to distinguish between File and Image drop from desktop
        await OnItemCreatedFromDrop.InvokeAsync((msgId, payload, x, y));
    }

    [Parameter]
    public EventCallback<PinboardItemType> OnCreateItemRequested { get; set; }

    private async Task CreateNewStickyNote()
    {
        await OnCreateItemRequested.InvokeAsync(PinboardItemType.StickyNote);
    }

    private async Task CreateNewTask()
    {
        await OnCreateItemRequested.InvokeAsync(PinboardItemType.Task);
    }
}"""
content = content.replace(old_code_end, new_code_end)

with open(canvas_file, 'w', encoding='utf-8') as f:
    f.write(content)

print("CanvasBoard updated with Floating Toolbar")
