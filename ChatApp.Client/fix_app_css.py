import re

with open("wwwroot/css/app.css", "r") as f:
    content = f.read()

# Replace variables block
old_vars = r""":root \{
    --bg-base: #0d0f14;
    --bg-surface: #13161e;
    --bg-card: rgba\(255, 255, 255, 0\.04\);
    --bg-card-hover: rgba\(255, 255, 255, 0\.07\);
    --bg-input: rgba\(255, 255, 255, 0\.06\);
    --bg-input-focus: rgba\(255, 255, 255, 0\.1\);

    --accent: #6c63ff;
    --accent-hover: #5a52e0;
    --accent-glow: rgba\(108, 99, 255, 0\.35\);
    --accent-2: #ff6b9d;

    --text-primary: #f0f2f8;
    --text-secondary: #8b90a7;
    --text-muted: #5a5f74;
    --text-danger: #ff4f6d;
    --text-success: #22d3a0;

    --border: rgba\(255, 255, 255, 0\.08\);
    --border-focus: rgba\(108, 99, 255, 0\.6\);

    --radius-sm: 8px;
    --radius-md: 14px;
    --radius-lg: 20px;
    --radius-full: 9999px;

    --shadow-card: 0 8px 32px rgba\(0, 0, 0, 0\.5\), 0 0 0 1px rgba\(255, 255, 255, 0\.05\);
    --shadow-glow: 0 0 40px var\(--accent-glow\);

    --transition: 0\.2s cubic-bezier\(0\.4, 0, 0\.2, 1\);
    --font: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
\}"""

new_vars = """:root {
    /* Background & Structure */
    --bg-primary: #f9f6f1;
    --surface: #ffffff;
    --sidebar-bg: #faf7f3;
    --border: #e8e1d9;
    --border-hover: #d4ccc3;
    --hover-bg: #f2ece4;

    /* Accent */
    --accent-primary: #d4724a;
    --accent-bg-soft: #fef4ef;
    --accent-border: #f5c9b0;

    /* Typography */
    --text-primary: #1a1411;
    --text-secondary: #6b5f57;
    --text-muted: #a89e96;

    /* Status */
    --status-success: #2a7a55;
    --status-info: #2e6da4;
    --status-warning: #8b6914;
    --status-danger: #c53030;

    /* Foundations */
    --radius-sm: 6px;
    --radius-md: 12px;
    --radius-lg: 16px;
    --radius-full: 9999px;
    --shadow-soft: 0 1px 3px rgba(26, 20, 17, 0.06);
    --focus-ring: 0 0 0 3px rgba(212, 114, 74, 0.2);

    --transition: 0.2s cubic-bezier(0.4, 0, 0.2, 1);
    --font: 'Outfit', 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}"""

content = re.sub(old_vars, new_vars, content, flags=re.MULTILINE)

# Global replacements for old tokens
content = content.replace("var(--bg-base)", "var(--bg-primary)")
content = content.replace("var(--bg-surface)", "var(--surface)")
content = content.replace("var(--bg-card)", "var(--surface)")
content = content.replace("var(--bg-input)", "var(--surface)")
content = content.replace("var(--bg-input-focus)", "var(--surface)")
content = content.replace("var(--border-focus)", "var(--accent-primary)")
content = content.replace("var(--accent)", "var(--accent-primary)")
content = content.replace("var(--accent-2)", "var(--accent-primary)")
content = content.replace("var(--accent-hover)", "var(--accent-primary)")
content = content.replace("var(--text-danger)", "var(--status-danger)")
content = content.replace("var(--text-success)", "var(--status-success)")

with open("wwwroot/css/app.css", "w") as f:
    f.write(content)

