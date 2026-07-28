import re

js_file = r'ChatApp.Client\wwwroot\js\canvas-physics.js'
with open(js_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Add keydown logic for Delete in initCanvas
old_code = """        // Store canvas reference globally for drop checking
        this.canvasElem = canvasElem;"""

new_code = """        // Store canvas reference globally for drop checking
        this.canvasElem = canvasElem;

        // Xử lý xoá các thẻ đang chọn bằng phím Delete / Backspace
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Delete' || e.key === 'Backspace') {
                // Nếu đang gõ text trong thẻ thì không xoá
                if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA' || e.target.isContentEditable) {
                    return;
                }

                let selectedCards = document.querySelectorAll('.selected-card');
                if (selectedCards.length > 0) {
                    // Prevent default backspace behavior (going back in history)
                    if (e.key === 'Backspace') {
                        e.preventDefault();
                    }
                    
                    let idsToDelete = [];
                    selectedCards.forEach(card => {
                        let cardId = card.getAttribute('data-id');
                        if (cardId) {
                            idsToDelete.push(cardId);
                            // Xoá tạm giao diện ngay lập tức để phản hồi nhanh
                            card.remove(); 
                        }
                    });

                    if (idsToDelete.length > 0 && this.dotnetHelper) {
                        this.dotnetHelper.invokeMethodAsync('OnCardsDeletedJS', idsToDelete).catch(err => console.error(err));
                    }
                }
            }
        });"""

content = content.replace(old_code, new_code)

with open(js_file, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated canvas-physics.js with Delete shortcut")
