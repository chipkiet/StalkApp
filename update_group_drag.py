import re

js_file = r'ChatApp.Client\wwwroot\js\canvas-physics.js'
with open(js_file, 'r', encoding='utf-8') as f:
    content = f.read()

old_move = """                move (event) {
                    var target = event.target;
                    // Scale drag by current zoom level
                    var x = (parseFloat(target.getAttribute('data-x')) || 0) + (event.dx / window.canvasPhysics.currentScale);
                    var y = (parseFloat(target.getAttribute('data-y')) || 0) + (event.dy / window.canvasPhysics.currentScale);

                    target.style.transform = 'translate(' + x + 'px, ' + y + 'px)';
                    target.setAttribute('data-x', x);
                    target.setAttribute('data-y', y);

                    // Dynamically update connection lines without waiting for Blazor
                    var cardId = target.getAttribute('data-id');
                    if (cardId) {
                        window.canvasPhysics.updateCardConnections(cardId, x, y);
                        
                        let now = Date.now();
                        if (now - lastMoveSent > 50) {
                            lastMoveSent = now;
                            dotnetHelper.invokeMethodAsync('OnCardMovedLive', cardId, x, y).catch(()=>{});
                        }
                    }
                },
                end (event) {
                    var target = event.target;
                    target.classList.remove('dragging');
                    var x = parseFloat(target.getAttribute('data-x')) || 0;
                    var y = parseFloat(target.getAttribute('data-y')) || 0;
                    var itemId = target.getAttribute('data-id');
                    
                    if (itemId) {
                        dotnetHelper.invokeMethodAsync('OnCardMoved', itemId, x, y);
                    }
                }"""

new_move = """                move (event) {
                    var target = event.target;
                    // Nếu thẻ này đang được chọn, kéo tất cả thẻ được chọn
                    var targets = target.classList.contains('selected-card') 
                        ? document.querySelectorAll('.selected-card') 
                        : [target];
                    
                    var scaledDx = event.dx / window.canvasPhysics.currentScale;
                    var scaledDy = event.dy / window.canvasPhysics.currentScale;

                    let now = Date.now();
                    let shouldSendLive = now - lastMoveSent > 50;
                    if (shouldSendLive) lastMoveSent = now;

                    targets.forEach(t => {
                        var x = (parseFloat(t.getAttribute('data-x')) || 0) + scaledDx;
                        var y = (parseFloat(t.getAttribute('data-y')) || 0) + scaledDy;

                        t.style.transform = 'translate(' + x + 'px, ' + y + 'px)';
                        t.setAttribute('data-x', x);
                        t.setAttribute('data-y', y);

                        var cardId = t.getAttribute('data-id');
                        if (cardId) {
                            window.canvasPhysics.updateCardConnections(cardId, x, y);
                            
                            if (shouldSendLive) {
                                dotnetHelper.invokeMethodAsync('OnCardMovedLive', cardId, x, y).catch(()=>{});
                            }
                        }
                    });
                },
                end (event) {
                    var target = event.target;
                    target.classList.remove('dragging');
                    
                    var targets = target.classList.contains('selected-card') 
                        ? document.querySelectorAll('.selected-card') 
                        : [target];
                        
                    targets.forEach(t => {
                        var x = parseFloat(t.getAttribute('data-x')) || 0;
                        var y = parseFloat(t.getAttribute('data-y')) || 0;
                        var itemId = t.getAttribute('data-id');
                        
                        if (itemId) {
                            dotnetHelper.invokeMethodAsync('OnCardMoved', itemId, x, y);
                        }
                    });
                }"""

content = content.replace(old_move, new_move)

with open(js_file, 'w', encoding='utf-8') as f:
    f.write(content)
