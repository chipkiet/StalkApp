import re

js_file = r'ChatApp.Client\wwwroot\js\canvas-physics.js'
with open(js_file, 'r', encoding='utf-8') as f:
    content = f.read()

# I want to add marquee selection logic right after Panzoom initialization
old_init = """        // Initialize Panzoom
        this.panzoom = Panzoom(canvasElem, {
            maxScale: 3,
            minScale: 0.1,
            cursor: 'grab',
            excludeClass: 'pinboard-card', // Đừng pan khi kéo thẻ
            startX: 0,
            startY: 0
        });"""
        
new_init = """        // Initialize Panzoom
        this.panzoom = Panzoom(canvasElem, {
            maxScale: 3,
            minScale: 0.1,
            cursor: 'grab',
            excludeClass: 'pinboard-card', // Đừng pan khi kéo thẻ
            startX: 0,
            startY: 0
        });

        // Hỗ trợ Marquee Selection (Ctrl + Drag)
        let isMarqueeSelecting = false;
        let startX = 0;
        let startY = 0;
        let marqueeDiv = null;

        canvasElem.parentElement.addEventListener('mousedown', (e) => {
            if (e.ctrlKey && e.button === 0) {
                // Prevent Panzoom
                e.stopPropagation();
                e.preventDefault();
                isMarqueeSelecting = true;
                
                let rect = canvasElem.parentElement.getBoundingClientRect();
                startX = e.clientX - rect.left;
                startY = e.clientY - rect.top;
                
                marqueeDiv = document.createElement('div');
                marqueeDiv.className = 'marquee-selection';
                marqueeDiv.style.left = startX + 'px';
                marqueeDiv.style.top = startY + 'px';
                marqueeDiv.style.width = '0px';
                marqueeDiv.style.height = '0px';
                canvasElem.parentElement.appendChild(marqueeDiv);
            } else if (e.button === 0 && !e.target.closest('.pinboard-card')) {
                // Click ra ngoài mà không giữ Ctrl thì xoá chọn
                document.querySelectorAll('.selected-card').forEach(c => c.classList.remove('selected-card'));
            }
        }, { capture: true }); // Dùng capture để chặn trước Panzoom

        canvasElem.parentElement.addEventListener('mousemove', (e) => {
            if (isMarqueeSelecting && marqueeDiv) {
                e.stopPropagation();
                e.preventDefault();
                
                let rect = canvasElem.parentElement.getBoundingClientRect();
                let currentX = e.clientX - rect.left;
                let currentY = e.clientY - rect.top;
                
                let minX = Math.min(startX, currentX);
                let minY = Math.min(startY, currentY);
                let width = Math.abs(currentX - startX);
                let height = Math.abs(currentY - startY);
                
                marqueeDiv.style.left = minX + 'px';
                marqueeDiv.style.top = minY + 'px';
                marqueeDiv.style.width = width + 'px';
                marqueeDiv.style.height = height + 'px';
            }
        }, { capture: true });

        canvasElem.parentElement.addEventListener('mouseup', (e) => {
            if (isMarqueeSelecting && marqueeDiv) {
                e.stopPropagation();
                e.preventDefault();
                isMarqueeSelecting = false;
                
                // Tính toán overlap
                let marqueeRect = marqueeDiv.getBoundingClientRect();
                let cards = document.querySelectorAll('.pinboard-card');
                
                cards.forEach(card => {
                    let cardRect = card.getBoundingClientRect();
                    // Check overlap
                    if (!(marqueeRect.right < cardRect.left || 
                          marqueeRect.left > cardRect.right || 
                          marqueeRect.bottom < cardRect.top || 
                          marqueeRect.top > cardRect.bottom)) {
                        card.classList.add('selected-card');
                    }
                });
                
                marqueeDiv.remove();
                marqueeDiv = null;
            }
        }, { capture: true });"""

content = content.replace(old_init, new_init)

with open(js_file, 'w', encoding='utf-8') as f:
    f.write(content)
