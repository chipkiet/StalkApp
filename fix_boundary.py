import re

razor_file = r'ChatApp.Client\Components\Pinboard\CanvasBoard.razor'
with open(razor_file, 'r', encoding='utf-8') as f:
    razor_content = f.read()

old_draw = """                    var x1 = src.PositionX + 150; 
                    var y1 = src.PositionY + 100;
                    var x2 = tgt.PositionX + 150;
                    var y2 = tgt.PositionY + 100;
                    var midX = x1 + (x2 - x1) / 2;
                    var pathData = System.FormattableString.Invariant($"M {x1} {y1} C {midX} {y1}, {midX} {y2}, {x2} {y2}");"""

new_draw = """                    var c1X = src.PositionX + 140; 
                    var c1Y = src.PositionY + 80;
                    var c2X = tgt.PositionX + 140;
                    var c2Y = tgt.PositionY + 80;
                    var p1 = GetBoundaryPoint(c1X, c1Y, c2X, c2Y);
                    var p2 = GetBoundaryPoint(c2X, c2Y, c1X, c1Y);
                    var midX = p1.X + (p2.X - p1.X) / 2;
                    var pathData = System.FormattableString.Invariant($"M {p1.X} {p1.Y} C {midX} {p1.Y}, {midX} {p2.Y}, {p2.X} {p2.Y}");"""

razor_content = razor_content.replace(old_draw, new_draw)

# Add method to @code block
method_code = """
    private (double X, double Y) GetBoundaryPoint(double cX, double cY, double tX, double tY, double width = 280, double height = 160)
    {
        double dx = tX - cX;
        double dy = tY - cY;
        if (Math.Abs(dx) < 0.001 && Math.Abs(dy) < 0.001) return (cX, cY);

        double w = width / 2;
        double h = height / 2;
        double slope = dy / dx;

        double xEdge = dx > 0 ? w : -w;
        double yAtXEdge = slope * xEdge;

        if (Math.Abs(yAtXEdge) <= h) return (cX + xEdge, cY + yAtXEdge);

        double yEdge = dy > 0 ? h : -h;
        double xAtYEdge = yEdge / slope;
        return (cX + xAtYEdge, cY + yEdge);
    }
"""
razor_content = razor_content.replace("@code {", "@code {" + method_code)

with open(razor_file, 'w', encoding='utf-8') as f:
    f.write(razor_content)

js_file = r'ChatApp.Client\wwwroot\js\canvas-physics.js'
with open(js_file, 'r', encoding='utf-8') as f:
    js_content = f.read()

# Replace updateCardConnections and add getBoundaryPoint
old_js = """    updateCardConnections: function(cardId, x, y) {
        var centerX = x + 150;
        var centerY = y + 100;

        // Update lines where this card is the source
        var sourceLines = document.querySelectorAll(`path.canvas-connection[data-source-id="${cardId}"]`);
        sourceLines.forEach(line => {
            var d = line.getAttribute('d');
            if (d) {
                var parts = d.split(',');
                if (parts.length === 3) {
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
    },"""

new_js = """    getBoundaryPoint: function(cX, cY, tX, tY, w, h) {
        var dx = tX - cX;
        var dy = tY - cY;
        if (Math.abs(dx) < 0.001 && Math.abs(dy) < 0.001) return {x: cX, y: cY};
        
        var w2 = w / 2;
        var h2 = h / 2;
        var slope = dy / dx;
        
        var xEdge = dx > 0 ? w2 : -w2;
        var yAtXEdge = slope * xEdge;
        
        if (Math.abs(yAtXEdge) <= h2) {
            return { x: cX + xEdge, y: cY + yAtXEdge };
        }
        
        var yEdge = dy > 0 ? h2 : -h2;
        var xAtYEdge = yEdge / slope;
        return { x: cX + xAtYEdge, y: cY + yEdge };
    },

    updateCardConnections: function(cardId, x, y) {
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

js_content = js_content.replace(old_js, new_js)

with open(js_file, 'w', encoding='utf-8') as f:
    f.write(js_content)

print("Boundary math added successfully.")
