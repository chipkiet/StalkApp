import re

css_file = r'ChatApp.Client\wwwroot\css\pinboard.css'
with open(css_file, 'r', encoding='utf-8') as f:
    css_content = f.read()

old_connection = """.canvas-connection {
    filter: drop-shadow(0 2px 4px rgba(0,0,0,0.2));
    pointer-events: stroke; /* Allow clicking on the stroke */
    transition: stroke 0.3s ease, filter 0.3s ease;
}

.canvas-connection:hover {
    stroke: #ff4444 !important; /* Red on hover to indicate deletion */
    filter: drop-shadow(0 4px 8px rgba(255, 68, 68, 0.4));
}"""

new_connection = """.canvas-connection {
    filter: drop-shadow(0 2px 4px rgba(0,0,0,0.2));
    pointer-events: stroke; /* Allow clicking on the stroke */
    transition: stroke 0.3s ease, filter 0.3s ease;
    stroke-dasharray: 12 12;
    animation: marching-ants 1s linear infinite;
}

.canvas-connection:hover {
    stroke: #ff4444 !important; /* Red on hover to indicate deletion */
    filter: drop-shadow(0 4px 8px rgba(255, 68, 68, 0.4));
}

@keyframes marching-ants {
    to { stroke-dashoffset: -24; }
}"""

css_content = css_content.replace(old_connection, new_connection)

with open(css_file, 'w', encoding='utf-8') as f:
    f.write(css_content)

print("CSS marching-ants restored.")
