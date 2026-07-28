import re

js_file = r'ChatApp.Client\wwwroot\js\canvas-physics.js'
with open(js_file, 'r', encoding='utf-8') as f:
    js_content = f.read()

old_js = """    updateCardConnections: function(cardId, x, y) {
        var c1X = x + 140;
        var c1Y = y + 80;

        // update lines where this is source
        var sourceLines = document.querySelectorAll(`path.canvas-connection[data-source-id="${cardId}"]`);
        sourceLines.forEach(line => {
            var tgtId = line.getAttribute('data-target-id');
            var tgtCard = document.querySelector(`.pinboard-card[data-id="${tgtId}"]`);
            if (tgtCard) {
                var tx = parseFloat(tgtCard.getAttribute('data-x'));
                var ty = parseFloat(tgtCard.getAttribute('data-y'));
                var c2X = tx + 140;
                var c2Y = ty + 80;

                var p1 = window.canvasPhysics.getBoundaryPoint(c1X, c1Y, c2X, c2Y, 280, 160);
                var p2 = window.canvasPhysics.getBoundaryPoint(c2X, c2Y, c1X, c1Y, 280, 160);
                var midX = p1.x + (p2.x - p1.x) / 2;
                line.setAttribute('d', `M ${p1.x} ${p1.y} C ${midX} ${p1.y}, ${midX} ${p2.y}, ${p2.x} ${p2.y}`);
            }
        });

        // update lines where this is target
        var targetLines = document.querySelectorAll(`path.canvas-connection[data-target-id="${cardId}"]`);
        targetLines.forEach(line => {
            var srcId = line.getAttribute('data-source-id');
            var srcCard = document.querySelector(`.pinboard-card[data-id="${srcId}"]`);
            if (srcCard) {
                var sx = parseFloat(srcCard.getAttribute('data-x'));
                var sy = parseFloat(srcCard.getAttribute('data-y'));
                var c2X = sx + 140; // c2 is now the source center
                var c2Y = sy + 80;

                var p1 = window.canvasPhysics.getBoundaryPoint(c2X, c2Y, c1X, c1Y, 280, 160);
                var p2 = window.canvasPhysics.getBoundaryPoint(c1X, c1Y, c2X, c2Y, 280, 160);
                var midX = p1.x + (p2.x - p1.x) / 2;
                line.setAttribute('d', `M ${p1.x} ${p1.y} C ${midX} ${p1.y}, ${midX} ${p2.y}, ${p2.x} ${p2.y}`);
            }
        });
    },"""

new_js = """    updateAllConnections: function() {
        var lines = document.querySelectorAll('path.canvas-connection');
        lines.forEach(line => {
            var srcId = line.getAttribute('data-source-id');
            var tgtId = line.getAttribute('data-target-id');
            var srcCard = document.querySelector(`.pinboard-card[data-id="${srcId}"]`);
            var tgtCard = document.querySelector(`.pinboard-card[data-id="${tgtId}"]`);
            if (srcCard && tgtCard) {
                var sx = parseFloat(srcCard.getAttribute('data-x')) || 0;
                var sy = parseFloat(srcCard.getAttribute('data-y')) || 0;
                var tx = parseFloat(tgtCard.getAttribute('data-x')) || 0;
                var ty = parseFloat(tgtCard.getAttribute('data-y')) || 0;
                
                var w1 = srcCard.offsetWidth || 280;
                var h1 = srcCard.offsetHeight || 160;
                var w2 = tgtCard.offsetWidth || 280;
                var h2 = tgtCard.offsetHeight || 160;

                var c1X = sx + w1 / 2;
                var c1Y = sy + h1 / 2;
                var c2X = tx + w2 / 2;
                var c2Y = ty + h2 / 2;

                var p1 = window.canvasPhysics.getBoundaryPoint(c1X, c1Y, c2X, c2Y, w1, h1);
                var p2 = window.canvasPhysics.getBoundaryPoint(c2X, c2Y, c1X, c1Y, w2, h2);
                var midX = p1.x + (p2.x - p1.x) / 2;
                line.setAttribute('d', `M ${p1.x} ${p1.y} C ${midX} ${p1.y}, ${midX} ${p2.y}, ${p2.x} ${p2.y}`);
            }
        });
    },

    updateCardConnections: function(cardId, x, y) {
        var srcCard = document.querySelector(`.pinboard-card[data-id="${cardId}"]`);
        if (!srcCard) return;
        var w1 = srcCard.offsetWidth || 280;
        var h1 = srcCard.offsetHeight || 160;
        var c1X = x + w1 / 2;
        var c1Y = y + h1 / 2;

        var sourceLines = document.querySelectorAll(`path.canvas-connection[data-source-id="${cardId}"]`);
        sourceLines.forEach(line => {
            var tgtId = line.getAttribute('data-target-id');
            var tgtCard = document.querySelector(`.pinboard-card[data-id="${tgtId}"]`);
            if (tgtCard) {
                var tx = parseFloat(tgtCard.getAttribute('data-x')) || 0;
                var ty = parseFloat(tgtCard.getAttribute('data-y')) || 0;
                var w2 = tgtCard.offsetWidth || 280;
                var h2 = tgtCard.offsetHeight || 160;
                var c2X = tx + w2 / 2;
                var c2Y = ty + h2 / 2;

                var p1 = window.canvasPhysics.getBoundaryPoint(c1X, c1Y, c2X, c2Y, w1, h1);
                var p2 = window.canvasPhysics.getBoundaryPoint(c2X, c2Y, c1X, c1Y, w2, h2);
                var midX = p1.x + (p2.x - p1.x) / 2;
                line.setAttribute('d', `M ${p1.x} ${p1.y} C ${midX} ${p1.y}, ${midX} ${p2.y}, ${p2.x} ${p2.y}`);
            }
        });

        var targetLines = document.querySelectorAll(`path.canvas-connection[data-target-id="${cardId}"]`);
        targetLines.forEach(line => {
            var sId = line.getAttribute('data-source-id');
            var otherSrcCard = document.querySelector(`.pinboard-card[data-id="${sId}"]`);
            if (otherSrcCard) {
                var sx = parseFloat(otherSrcCard.getAttribute('data-x')) || 0;
                var sy = parseFloat(otherSrcCard.getAttribute('data-y')) || 0;
                var w2 = otherSrcCard.offsetWidth || 280;
                var h2 = otherSrcCard.offsetHeight || 160;
                var c2X = sx + w2 / 2;
                var c2Y = sy + h2 / 2;

                var p1 = window.canvasPhysics.getBoundaryPoint(c2X, c2Y, c1X, c1Y, w2, h2);
                var p2 = window.canvasPhysics.getBoundaryPoint(c1X, c1Y, c2X, c2Y, w1, h1);
                var midX = p1.x + (p2.x - p1.x) / 2;
                line.setAttribute('d', `M ${p1.x} ${p1.y} C ${midX} ${p1.y}, ${midX} ${p2.y}, ${p2.x} ${p2.y}`);
            }
        });
    },"""

js_content = js_content.replace(old_js, new_js)

with open(js_file, 'w', encoding='utf-8') as f:
    f.write(js_content)

print("JS boundary fixed")
