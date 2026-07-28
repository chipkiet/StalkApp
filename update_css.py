import re

css_file = r'ChatApp.Client\wwwroot\css\pinboard.css'
with open(css_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Add styles at the end
new_css = """

/* File Card */
.file-card {
    min-width: 200px;
    background-color: var(--card-bg);
    border: 1px solid var(--border-color);
    border-radius: var(--radius-lg);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    overflow: hidden;
    position: absolute;
    cursor: grab;
}

.file-toolbar {
    position: absolute;
    top: 0.5rem;
    right: 0.5rem;
    display: flex;
    gap: 0.25rem;
    opacity: 0;
    transition: opacity 0.2s;
    background: rgba(255,255,255,0.8);
    border-radius: var(--radius-sm);
    padding: 2px;
}

.file-card:hover .file-toolbar {
    opacity: 1;
}

.file-content {
    padding: 1.5rem;
    display: flex;
    align-items: center;
    justify-content: center;
}

.file-link {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.5rem;
    text-decoration: none;
    color: var(--text-primary);
}

.file-link:hover {
    color: var(--accent-color);
}

.file-icon {
    width: 48px;
    height: 48px;
    color: var(--accent-color);
}

.file-name {
    font-size: 0.9rem;
    font-weight: 500;
    text-align: center;
    word-break: break-all;
    max-width: 150px;
}

/* Floating Toolbar */
.canvas-floating-toolbar {
    position: absolute;
    bottom: 2rem;
    left: 50%;
    transform: translateX(-50%);
    display: flex;
    gap: 1rem;
    background-color: var(--surface-color);
    padding: 0.5rem 1rem;
    border-radius: var(--radius-full);
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
    z-index: 1000;
}

.canvas-floating-toolbar .btn {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    background-color: transparent;
    border: none;
    color: var(--text-secondary);
    font-weight: 500;
    cursor: pointer;
    padding: 0.5rem 1rem;
    border-radius: var(--radius-full);
    transition: all 0.2s;
}

.canvas-floating-toolbar .btn:hover {
    background-color: var(--background-color);
    color: var(--accent-color);
}

.canvas-floating-toolbar .btn svg {
    width: 1.25rem;
    height: 1.25rem;
}

.canvas-floating-toolbar .btn-create-note:hover {
    color: #eab308; /* Yellow */
}
"""
content += new_css

with open(css_file, 'w', encoding='utf-8') as f:
    f.write(content)
print("pinboard.css updated")
