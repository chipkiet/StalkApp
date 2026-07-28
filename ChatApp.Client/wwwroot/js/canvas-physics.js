// window.canvasPhysics handles interop between Blazor and JS for the Pinboard
window.canvasPhysics = {
    currentScale: 1,

    initCanvas: function (dotnetHelper, containerId) {
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
    },

    initDraggableMessages: function() {
        let activeClone = null;
        interact('.chat-bubble.draggable').draggable({
            inertia: false,
            autoScroll: true,
            listeners: {
                start (event) {
                    var target = event.target;
                    activeClone = target.cloneNode(true);
                    activeClone.id = 'drag-clone';
                    activeClone.style.position = 'fixed';
                    activeClone.style.zIndex = '99999';
                    activeClone.style.pointerEvents = 'none'; 
                    activeClone.style.opacity = '0.9';
                    activeClone.style.boxShadow = '0 10px 30px rgba(0,0,0,0.5)';
                    activeClone.style.transition = 'none';
                    activeClone.style.transform = 'translate(0px, 0px)';
                    
                    var rect = target.getBoundingClientRect();
                    activeClone.style.top = rect.top + 'px';
                    activeClone.style.left = rect.left + 'px';
                    activeClone.style.width = rect.width + 'px';
                    
                    document.body.appendChild(activeClone);
                    
                    activeClone.setAttribute('data-x', 0);
                    activeClone.setAttribute('data-y', 0);
                },
                move (event) {
                    if (!activeClone) return;
                    var x = (parseFloat(activeClone.getAttribute('data-x')) || 0) + event.dx;
                    var y = (parseFloat(activeClone.getAttribute('data-y')) || 0) + event.dy;

                    activeClone.style.transform = 'translate(' + x + 'px, ' + y + 'px)';
                    activeClone.setAttribute('data-x', x);
                    activeClone.setAttribute('data-y', y);
                },
                end (event) {
                    if (activeClone) {
                        // KHI THẢ RA: Kiểm tra xem chuột có nằm trong vùng Canvas không
                        var canvasElem = window.canvasPhysics.canvasElem;
                        var dotnetHelper = window.canvasPhysics.dotnetHelper;
                        
                        if (canvasElem && dotnetHelper) {
                            var rect = canvasElem.parentElement.getBoundingClientRect();
                            var mouseX = event.client.x;
                            var mouseY = event.client.y;
                            
                            if (mouseX >= rect.left && mouseX <= rect.right && mouseY >= rect.top && mouseY <= rect.bottom) {
                                // Thả vào Canvas thành công!
                                var target = event.target;
                                var msgId = target.getAttribute('data-msg-id');
                                var content = target.innerText;
                                
                                // Tính toạ độ thả tương đối theo scale
                                var innerBoard = document.getElementById('canvas-board-inner');
                                var canvasRect = innerBoard.getBoundingClientRect();
                                var dropX = (mouseX - canvasRect.left) / window.canvasPhysics.currentScale;
                                var dropY = (mouseY - canvasRect.top) / window.canvasPhysics.currentScale;
                                
                                dotnetHelper.invokeMethodAsync('OnMessageDropped', msgId, content, dropX, dropY);
                            }
                        }
                        
                        activeClone.remove();
                        activeClone = null;
                    }
                }
            }
        });
    },

    initDraggableCards: function(dotnetHelper) {
        interact('.pinboard-card').draggable({
            inertia: true,
            listeners: {
                start (event) {
                    var target = event.target;
                    target.classList.add('dragging');
                },
                move (event) {
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
                        var centerX = x + 150;
                        var centerY = y + 100;

                        // Update lines where this card is the source
                        var sourceLines = document.querySelectorAll(`path.canvas-connection[data-source-id="${cardId}"]`);
                        sourceLines.forEach(line => {
                            var d = line.getAttribute('d');
                            if (d) {
                                // M x1 y1 C midX y1, midX y2, x2 y2
                                var parts = d.split(',');
                                if (parts.length === 3) {
                                    // Parse x2, y2 from the last part: " x2 y2"
                                    var lastTokens = parts[2].trim().split(' ');
                                    if(lastTokens.length >= 2) {
                                        var x2 = parseFloat(lastTokens[lastTokens.length - 2]);
                                        var y2 = parseFloat(lastTokens[lastTokens.length - 1]);
                                        var midX = centerX + (x2 - centerX) / 2;
                                        line.setAttribute('d', `M ${centerX} ${centerY} C ${midX} ${centerY}, ${midX} ${y2}, ${x2} ${y2}`);
                                    }
                                }
                            }
                        });

                        // Update lines where this card is the target
                        var targetLines = document.querySelectorAll(`path.canvas-connection[data-target-id="${cardId}"]`);
                        targetLines.forEach(line => {
                            var d = line.getAttribute('d');
                            if (d) {
                                var mMatch = d.match(/M\s+([-\d.]+)\s+([-\d.]+)/);
                                if (mMatch) {
                                    var x1 = parseFloat(mMatch[1]);
                                    var y1 = parseFloat(mMatch[2]);
                                    var midX = x1 + (centerX - x1) / 2;
                                    line.setAttribute('d', `M ${x1} ${y1} C ${midX} ${y1}, ${midX} ${centerY}, ${centerX} ${centerY}`);
                                }
                            }
                        });
                    }
                },
                end (event) {
                    var target = event.target;
                    target.classList.remove('dragging');
                    var x = parseFloat(target.getAttribute('data-x')) || 0;
                    var y = parseFloat(target.getAttribute('data-y')) || 0;
                    var itemId = target.getAttribute('data-id');
                    
                    // Call C# to save new position
                    dotnetHelper.invokeMethodAsync('OnCardMoved', itemId, x, y);
                }
            }
        });
    },

    triggerConfetti: function(itemId) {
        var xRel = 0.5;
        var yRel = 0.5;
        
        var card = document.querySelector(`.pinboard-card[data-id="${itemId}"]`);
        if (card) {
            var rect = card.getBoundingClientRect();
            xRel = (rect.left + rect.width / 2) / window.innerWidth;
            yRel = (rect.top + rect.height / 2) / window.innerHeight;
        }
        
        confetti({
            particleCount: 100,
            spread: 70,
            origin: { x: xRel, y: yRel },
            colors: ['#d4724a', '#8b6914', '#2a7a55', '#e8e1d9'],
            zIndex: 99999
        });
        
        // Play satisfying sound
        var audio = new Audio('https://assets.mixkit.co/active_storage/sfx/2013/2013-preview.mp3');
        audio.volume = 0.5;
        audio.play().catch(e => console.log('Audio play failed', e));
    }
};
