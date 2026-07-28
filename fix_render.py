import re

razor_file = r'ChatApp.Client\Components\Pinboard\CanvasBoard.razor'
with open(razor_file, 'r', encoding='utf-8') as f:
    razor_content = f.read()

old_render = """    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _objRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("canvasPhysics.initCanvas", _objRef, "pinboard-canvas");
            await JS.InvokeVoidAsync("canvasPhysics.initDraggableCards", _objRef);
        }
    }"""

new_render = """    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _objRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("canvasPhysics.initCanvas", _objRef, "pinboard-canvas");
            await JS.InvokeVoidAsync("canvasPhysics.initDraggableCards", _objRef);
        }
        try {
            await JS.InvokeVoidAsync("canvasPhysics.updateAllConnections");
        } catch { }
    }"""

razor_content = razor_content.replace(old_render, new_render)

with open(razor_file, 'w', encoding='utf-8') as f:
    f.write(razor_content)

print("CanvasBoard updated")
