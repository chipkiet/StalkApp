import re

js_file = r'ChatApp.Client\wwwroot\js\canvas-physics.js'
with open(js_file, 'r', encoding='utf-8') as f:
    js_content = f.read()

old_drop = """                if (file.type.startsWith('image/')) {
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
                }"""

new_drop = """                // Caculate drop coordinates
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
                        let isImage = file.type.startsWith('image/');
                        this.dotnetHelper.invokeMethodAsync('OnFileDropped', data.url, file.name, isImage, x, y);
                    }
                })
                .catch(err => console.error('Upload failed', err));"""

js_content = js_content.replace(old_drop, new_drop)

with open(js_file, 'w', encoding='utf-8') as f:
    f.write(js_content)
