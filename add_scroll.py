import re

js_file = r'ChatApp.Client\wwwroot\js\canvas-physics.js'
with open(js_file, 'a', encoding='utf-8') as f:
    f.write("\nwindow.scrollToBottom = (elementId) => { var element = document.getElementById(elementId); if (element) { element.scrollTop = element.scrollHeight; } };\n")
print("Added scrollToBottom to JS")
