import re

js_file = r'ChatApp.Client\wwwroot\js\canvas-physics.js'
with open(js_file, 'r', encoding='utf-8') as f:
    js_content = f.read()

old_init = """    initCanvas: function (dotnetHelper, containerId) {
        this.dotnetHelper = dotnetHelper;
        
        var canvasElem = document.getElementById(containerId);
        
        // Initialize Panzoom
        this.panzoom = Panzoom(canvasElem, {
            maxScale: 3,
            minScale: 0.1,
            cursor: 'grab',
            excludeClass: 'pinboard-card', // Đừng pan khi kéo thẻ
            startX: 0,
            startY: 0
        });
        
        // Force transform-origin to match Panzoom's internal math
        canvasElem.style.transformOrigin = '50% 50%';
        
        canvasElem.parentElement.addEventListener('wheel', (e) => {
            try { e.preventDefault(); } catch(err) {}
            this.panzoom.zoomWithWheel(e);
        }, { passive: false });
        
        // Track scale for dragging math
        canvasElem.addEventListener('panzoomzoom', (e) => {
            this.currentScale = e.detail.scale;
            // Also notify Blazor if needed, but not required yet
        });
        
        // Store canvas reference globally for drop checking
        this.canvasElem = canvasElem;

        // Broadcast cursor movement for Live Cursors
        let lastCursorSent = 0;
        canvasElem.parentElement.addEventListener('mousemove', (e) => {
            let now = Date.now();
            if (now - lastCursorSent > 50) { // Throttle to 20Hz
                lastCursorSent = now;
                if (this.panzoom && this.dotnetHelper) {
                    let p = this.panzoom.getPan();
                    let s = this.panzoom.getScale();
                    let rect = canvasElem.parentElement.getBoundingClientRect();
                    
                    let x = (e.clientX - rect.left - p.x) / s;
                    let y = (e.clientY - rect.top - p.y) / s;
                    
                    this.dotnetHelper.invokeMethodAsync('OnCursorMoved', x, y).catch(() => {});
                }
            }
        });
    },"""

new_init = """    initCanvas: function (dotnetHelper, containerId) {
        this.dotnetHelper = dotnetHelper;
        
        var canvasElem = document.getElementById(containerId);
        
        // Initialize Panzoom
        this.panzoom = Panzoom(canvasElem, {
            maxScale: 3,
            minScale: 0.1,
            cursor: 'grab',
            excludeClass: 'pinboard-card', // Đừng pan khi kéo thẻ
            startX: 0,
            startY: 0
        });
        
        // Force transform-origin to match Panzoom's internal math
        canvasElem.style.transformOrigin = '50% 50%';
        
        canvasElem.parentElement.addEventListener('wheel', (e) => {
            try { e.preventDefault(); } catch(err) {}
            this.panzoom.zoomWithWheel(e);
        }, { passive: false });
        
        // Track scale for dragging math
        canvasElem.addEventListener('panzoomzoom', (e) => {
            this.currentScale = e.detail.scale;
            // Also notify Blazor if needed, but not required yet
        });
        
        // Store canvas reference globally for drop checking
        this.canvasElem = canvasElem;

        // Listen for native file drops
        canvasElem.parentElement.addEventListener('dragover', (e) => {
            e.preventDefault();
        });
        canvasElem.parentElement.addEventListener('drop', (e) => {
            e.preventDefault();
            if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
                let file = e.dataTransfer.files[0];
                if (file.type.startsWith('image/')) {
                    // Caculate drop coordinates
                    let p = this.panzoom.getPan();
                    let s = this.panzoom.getScale();
                    let rect = canvasElem.parentElement.getBoundingClientRect();
                    let x = (e.clientX - rect.left - p.x) / s;
                    let y = (e.clientY - rect.top - p.y) / s;
                    
                    // Upload file
                    let formData = new FormData();
                    formData.append('file', file);
                    
                    // Thể hiện loading UI nếu cần
                    fetch('/api/attachments/upload', {
                        method: 'POST',
                        body: formData
                    })
                    .then(response => response.json())
                    .then(data => {
                        if (data.url) {
                            this.dotnetHelper.invokeMethodAsync('OnFileDropped', data.url, x, y);
                        }
                    })
                    .catch(err => console.error('Upload failed', err));
                }
            }
        });

        // Broadcast cursor movement for Live Cursors
        let lastCursorSent = 0;
        canvasElem.parentElement.addEventListener('mousemove', (e) => {
            let now = Date.now();
            if (now - lastCursorSent > 50) { // Throttle to 20Hz
                lastCursorSent = now;
                if (this.panzoom && this.dotnetHelper) {
                    let p = this.panzoom.getPan();
                    let s = this.panzoom.getScale();
                    let rect = canvasElem.parentElement.getBoundingClientRect();
                    
                    let x = (e.clientX - rect.left - p.x) / s;
                    let y = (e.clientY - rect.top - p.y) / s;
                    
                    this.dotnetHelper.invokeMethodAsync('OnCursorMoved', x, y).catch(() => {});
                }
            }
        });
    },"""

js_content = js_content.replace(old_init, new_init)

with open(js_file, 'w', encoding='utf-8') as f:
    f.write(js_content)

print("JS updated with native drop")
