import re

css_file = r'ChatApp.Client\wwwroot\css\pinboard.css'
with open(css_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Add styles at the end
new_css = """

/* Marquee Selection (Vùng chọn nhiều thẻ) */
.marquee-selection {
    position: absolute;
    border: 1px dashed var(--accent-color);
    background-color: rgba(var(--accent-color-rgb, 10, 132, 255), 0.1);
    pointer-events: none; /* Không bắt sự kiện chuột */
    z-index: 9999;
}

/* Hiệu ứng viền phát sáng khi thẻ được chọn */
.selected-card {
    outline: 2px solid var(--accent-color);
    outline-offset: 2px;
    box-shadow: 0 0 15px rgba(var(--accent-color-rgb, 10, 132, 255), 0.3) !important;
}
"""

if "marquee-selection" not in content:
    with open(css_file, 'a', encoding='utf-8') as f:
        f.write(new_css)
    print("Added CSS styles")
else:
    print("Styles already exist")
